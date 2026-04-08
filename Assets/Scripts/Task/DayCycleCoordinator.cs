using System.Collections.Generic;
using UnityEngine;

// ============================================================
// DayCycleCoordinator
// ------------------------------------------------------------
// Orquestador chico del ciclo diario.
//
// Responsabilidades en esta etapa:
// 1. escuchar al `GameTimeService`
// 2. definir el orden de procesamiento al cerrar y abrir dias
// 3. delegar trabajo real en procesadores enchufables
//
// Importante:
// - NO mide el tiempo
// - NO consume comida por si mismo
// - NO decide eventos del mundo por si mismo
//
// Es solo la capa que ordena el flujo diario y deja puntos de
// extension claros para las siguientes etapas.
// ============================================================
public class DayCycleCoordinator : MonoBehaviour
{
    public static DayCycleCoordinator Instance { get; private set; }

    public event System.Action<int> OnDayEndSequenceStarted;
    public event System.Action<int> OnDayEndSequenceCompleted;
    public event System.Action<int> OnDayStartSequenceStarted;
    public event System.Action<int> OnDayStartSequenceCompleted;

    [Header("Ordered Processors")]
    [SerializeField] private MonoBehaviour[] dayEndProcessors;
    [SerializeField] private MonoBehaviour[] dayStartProcessors;

    private readonly List<IDayEndProcessor> resolvedDayEndProcessors = new List<IDayEndProcessor>();
    private readonly List<IDayStartProcessor> resolvedDayStartProcessors = new List<IDayStartProcessor>();

    private GameTimeService subscribedTimeService;

    private void Awake()
    {
        Instance = this;
        EnsureProcessorArrays();
    }

    private void OnValidate()
    {
        EnsureProcessorArrays();
    }

    private void OnEnable()
    {
        GameTimeService.OnServiceAvailable += HandleTimeServiceAvailable;
        TrySubscribeToGameTimeService();
    }

    private void OnDisable()
    {
        GameTimeService.OnServiceAvailable -= HandleTimeServiceAvailable;
        UnsubscribeFromGameTimeService();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void HandleTimeServiceAvailable(GameTimeService timeService)
    {
        SubscribeToGameTimeService(timeService);
    }

    private void TrySubscribeToGameTimeService()
    {
        if (GameTimeService.Instance != null)
            SubscribeToGameTimeService(GameTimeService.Instance);
    }

    private void SubscribeToGameTimeService(GameTimeService timeService)
    {
        if (timeService == null || ReferenceEquals(subscribedTimeService, timeService))
            return;

        UnsubscribeFromGameTimeService();

        subscribedTimeService = timeService;
        subscribedTimeService.OnDayEnding += HandleDayEnding;
        subscribedTimeService.OnDayStarted += HandleDayStarted;
    }

    private void UnsubscribeFromGameTimeService()
    {
        if (subscribedTimeService == null)
            return;

        subscribedTimeService.OnDayEnding -= HandleDayEnding;
        subscribedTimeService.OnDayStarted -= HandleDayStarted;
        subscribedTimeService = null;
    }

    private void HandleDayEnding(int dayNumber)
    {
        RefreshDayEndProcessors();

        OnDayEndSequenceStarted?.Invoke(dayNumber);

        for (int i = 0; i < resolvedDayEndProcessors.Count; i++)
            resolvedDayEndProcessors[i].ProcessDayEnd(dayNumber);

        OnDayEndSequenceCompleted?.Invoke(dayNumber);
    }

    private void HandleDayStarted(int dayNumber)
    {
        RefreshDayStartProcessors();

        OnDayStartSequenceStarted?.Invoke(dayNumber);

        for (int i = 0; i < resolvedDayStartProcessors.Count; i++)
            resolvedDayStartProcessors[i].ProcessDayStart(dayNumber);

        OnDayStartSequenceCompleted?.Invoke(dayNumber);
    }

    // Re-armamos las listas a demanda para que el coordinador
    // siempre use el estado real actual del inspector.
    private void RefreshDayEndProcessors()
    {
        EnsureProcessorArrays();
        resolvedDayEndProcessors.Clear();

        for (int i = 0; i < dayEndProcessors.Length; i++)
        {
            MonoBehaviour behaviour = dayEndProcessors[i];
            if (behaviour is IDayEndProcessor processor)
                resolvedDayEndProcessors.Add(processor);
        }
    }

    private void RefreshDayStartProcessors()
    {
        EnsureProcessorArrays();
        resolvedDayStartProcessors.Clear();

        for (int i = 0; i < dayStartProcessors.Length; i++)
        {
            MonoBehaviour behaviour = dayStartProcessors[i];
            if (behaviour is IDayStartProcessor processor)
                resolvedDayStartProcessors.Add(processor);
        }
    }

    private void EnsureProcessorArrays()
    {
        if (dayEndProcessors == null)
            dayEndProcessors = System.Array.Empty<MonoBehaviour>();

        if (dayStartProcessors == null)
            dayStartProcessors = System.Array.Empty<MonoBehaviour>();
    }
}
