using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// ============================================================
// DayEventDefinition
// ------------------------------------------------------------
// Asset de datos para eventos del inicio del dia.
//
// En esta etapa solo define:
// - cuando puede dispararse
// - si es repetible
// - su tipo
// - un peso para sorteos aleatorios
// - que cartas debe spawnear si el evento lo requiere
//
// La ejecucion real del evento quedara mas adelante en el
// `DayEventExecutor`.
// ============================================================
[CreateAssetMenu(fileName = "DayEvent", menuName = "Day Cycle/Day Event Definition")]
public class DayEventDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;

    [Header("Trigger")]
    public DayEventTriggerMode triggerMode = DayEventTriggerMode.ExactDay;
    [Min(1)] public int exactDay = 1;
    [Min(1)] public int minDay = 1;
    [Min(1)] public int maxDay = 1;
    [Min(0f)] public float weight = 1f;
    public bool repeatable = false;

    [Header("Effect")]
    public DayEventType eventType = DayEventType.None;
    public DayEventSpawnEntry[] spawnEntries;

    // Helper chico para que futuros sistemas puedan preguntar
    // rapido si el evento entra en ventana de evaluacion.
    public bool IsDayInSupportedRange(int dayNumber)
    {
        int safeDay = Mathf.Max(1, dayNumber);

        switch (triggerMode)
        {
            case DayEventTriggerMode.ExactDay:
                return safeDay == Mathf.Max(1, exactDay);

            case DayEventTriggerMode.DayRange:
                return safeDay >= Mathf.Max(1, minDay) && safeDay <= Mathf.Max(Mathf.Max(1, minDay), maxDay);

            case DayEventTriggerMode.RandomFromMinDay:
                return safeDay >= Mathf.Max(1, minDay);

            default:
                return false;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        DayEventValidationUtility.ValidateAndLog(this);
    }
#endif
}
