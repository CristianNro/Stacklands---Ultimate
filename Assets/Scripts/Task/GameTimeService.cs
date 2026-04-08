using UnityEngine;

// ============================================================
// GameTimeService
// ------------------------------------------------------------
// Fuente central de tiempo de juego.
//
// En esta etapa cumple dos responsabilidades:
// 1. seguir siendo la fuente de delta escalado para sistemas temporales
// 2. empezar a actuar como reloj de ciclo diario compartido
//
// Importante:
// - este servicio mide el tiempo y avanza el dia
// - todavia NO consume comida ni ejecuta eventos de dia
// - otros sistemas deben escuchar sus eventos y reaccionar
// ============================================================
public class GameTimeService : MonoBehaviour
{
    public static GameTimeService Instance { get; private set; }
    public static event System.Action<GameTimeService> OnServiceAvailable;

    // Eventos del ciclo diario.
    // Se disparan en este orden cuando un dia termina:
    // 1. OnDayEnding(currentDay)
    // 2. OnDayEnded(currentDay)
    // 3. se incrementa el contador interno
    // 4. OnDayAdvanced(newDay)
    // 5. OnDayStarted(newDay)
    public event System.Action<int> OnDayEnding;
    public event System.Action<int> OnDayEnded;
    public event System.Action<int> OnDayAdvanced;
    public event System.Action<int> OnDayStarted;

    [Header("Runtime Time Controls")]
    [SerializeField] private bool pauseTimedSystems;
    [SerializeField] private float timedSystemsTimeScale = 1f;

    [Header("Day Cycle")]
    [SerializeField, Min(1f)] private float dayDurationSeconds = 60f;
    [SerializeField, Min(1)] private int currentDay = 1;
    [SerializeField, Min(0f)] private float currentDayElapsed;

    public bool PauseTimedSystems
    {
        get => pauseTimedSystems;
        set => pauseTimedSystems = value;
    }

    public float TimedSystemsTimeScale
    {
        get => timedSystemsTimeScale;
        set => timedSystemsTimeScale = Mathf.Max(0f, value);
    }

    public float DayDurationSeconds
    {
        get => Mathf.Max(1f, dayDurationSeconds);
        set => dayDurationSeconds = Mathf.Max(1f, value);
    }

    public int CurrentDay => Mathf.Max(1, currentDay);
    public float CurrentDayElapsed => Mathf.Max(0f, currentDayElapsed);

    // Progreso normalizado del dia actual.
    // 0 = acaba de empezar
    // 1 = el dia ya llego a su duracion maxima
    public float CurrentDayProgress01
    {
        get
        {
            float duration = DayDurationSeconds;
            if (duration <= 0f)
                return 0f;

            return Mathf.Clamp01(CurrentDayElapsed / duration);
        }
    }

    // Delta escalado compartido por tareas y por el reloj diario.
    public float ScaledDeltaTime
    {
        get
        {
            if (pauseTimedSystems)
                return 0f;

            return Time.deltaTime * Mathf.Max(0f, timedSystemsTimeScale);
        }
    }

    private void Awake()
    {
        Instance = this;
        dayDurationSeconds = Mathf.Max(1f, dayDurationSeconds);
        currentDay = Mathf.Max(1, currentDay);
        currentDayElapsed = Mathf.Clamp(currentDayElapsed, 0f, dayDurationSeconds);

        OnServiceAvailable?.Invoke(this);
    }

    private void Start()
    {
        // Al entrar en play notificamos el dia actual para que futuros
        // listeners puedan inicializar UI o estado dependiente del dia.
        OnDayStarted?.Invoke(CurrentDay);
    }

    private void Update()
    {
        AdvanceDayClock(ScaledDeltaTime);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --------------------------------------------------------
    // Control del reloj diario
    // --------------------------------------------------------

    public void AdvanceDayClock(float deltaTime)
    {
        if (deltaTime <= 0f)
            return;

        currentDayElapsed += deltaTime;

        float duration = DayDurationSeconds;
        while (currentDayElapsed >= duration)
        {
            currentDayElapsed -= duration;
            CompleteCurrentDayAndAdvance();
            duration = DayDurationSeconds;
        }
    }

    public void PauseTime()
    {
        PauseTimedSystems = true;
    }

    public void ResumeTime()
    {
        PauseTimedSystems = false;
    }

    public void SetTimeScale(float scale)
    {
        TimedSystemsTimeScale = scale;
    }

    public void SetSpeedX1()
    {
        TimedSystemsTimeScale = 1f;
    }

    public void SetSpeedX2()
    {
        TimedSystemsTimeScale = 2f;
    }

    public void SetSpeedX3()
    {
        TimedSystemsTimeScale = 3f;
    }

    // Recorre el ciclo de velocidades base del juego:
    // x1 -> x2 -> x3 -> x1
    public void CycleSpeedMultiplier()
    {
        float currentScale = Mathf.Max(0f, TimedSystemsTimeScale);

        if (Mathf.Approximately(currentScale, 1f))
        {
            SetSpeedX2();
            return;
        }

        if (Mathf.Approximately(currentScale, 2f))
        {
            SetSpeedX3();
            return;
        }

        SetSpeedX1();
    }

    // Permite reiniciar el ciclo desde un dia concreto.
    // Util para debugging, sandbox o starts especiales.
    public void ResetDayCycle(int startDay = 1, bool notifyStarted = true)
    {
        currentDay = Mathf.Max(1, startDay);
        currentDayElapsed = 0f;

        if (notifyStarted)
            OnDayStarted?.Invoke(CurrentDay);
    }

    private void CompleteCurrentDayAndAdvance()
    {
        int finishedDay = CurrentDay;

        OnDayEnding?.Invoke(finishedDay);
        OnDayEnded?.Invoke(finishedDay);

        currentDay = Mathf.Max(1, currentDay + 1);

        OnDayAdvanced?.Invoke(CurrentDay);
        OnDayStarted?.Invoke(CurrentDay);
    }

    public static float GetTimedSystemsDeltaTime()
    {
        if (Instance == null)
            return Time.deltaTime;

        return Instance.ScaledDeltaTime;
    }
}
