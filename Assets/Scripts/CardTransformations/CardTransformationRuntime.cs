using UnityEngine;

// ============================================================
// CardTransformationRuntime
// ------------------------------------------------------------
// Estado runtime de una transformacion temporal de una sola
// carta.
//
// Ownership:
// - `CardTransformationRule` define la configuracion
// - este componente guarda el progreso mutable real
// - el sistema scheduler futuro decidira cuando avanzar o pausar
//
// Importante:
// - este componente no busca contexto
// - no ejecuta resultados
// - no decide si la carta deberia o no estar corriendo
// ============================================================
public class CardTransformationRuntime : MonoBehaviour
{
    [SerializeField] private CardTransformationRule activeRule;
    [SerializeField] private float elapsedTime;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isPaused;
    [SerializeField] private float currentSpeedMultiplier = 1f;

    public CardTransformationRule ActiveRule => activeRule;
    public float ElapsedTime => Mathf.Max(0f, elapsedTime);
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public float CurrentSpeedMultiplier => Mathf.Max(0f, currentSpeedMultiplier);

    public float Progress01
    {
        get
        {
            if (activeRule == null || activeRule.baseDuration <= 0f)
                return 0f;

            return Mathf.Clamp01(ElapsedTime / activeRule.baseDuration);
        }
    }

    public bool IsComplete
    {
        get
        {
            if (activeRule == null || activeRule.baseDuration <= 0f)
                return false;

            return ElapsedTime >= activeRule.baseDuration;
        }
    }

    public void Initialize(CardTransformationRule rule)
    {
        activeRule = rule;
        elapsedTime = 0f;
        isRunning = rule != null;
        isPaused = false;
        currentSpeedMultiplier = 1f;
    }

    public void SetProgress(float elapsedSeconds)
    {
        elapsedTime = Mathf.Max(0f, elapsedSeconds);
    }

    public void SetRunning(bool value)
    {
        isRunning = value;
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetSpeedMultiplier(float value)
    {
        currentSpeedMultiplier = Mathf.Max(0f, value);
    }

    public void ResetProgress()
    {
        elapsedTime = 0f;
        isPaused = false;
        currentSpeedMultiplier = 1f;
    }

    public void ClearRule()
    {
        activeRule = null;
        elapsedTime = 0f;
        isRunning = false;
        isPaused = false;
        currentSpeedMultiplier = 1f;
    }

    public float Advance(float deltaTime)
    {
        if (activeRule == null || !isRunning || isPaused || deltaTime <= 0f)
            return 0f;

        float appliedDelta = deltaTime * CurrentSpeedMultiplier;
        if (appliedDelta <= 0f)
            return 0f;

        elapsedTime += appliedDelta;
        return appliedDelta;
    }
}
