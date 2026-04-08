using System.Collections.Generic;
using StacklandsLike.Cards;
using UnityEngine;

// ============================================================
// CardTransformationSystem
// ------------------------------------------------------------
// Scheduler central para transformaciones temporales de una sola
// carta.
//
// Responsabilidades:
// - registrar cartas activas con transformation runtime
// - reunir capabilities del contexto actual
// - pausar o acelerar cada runtime
// - avanzar progreso con el tiempo compartido del juego
// - delegar la finalizacion al executor
//
// En esta primera etapa:
// - corre para cartas activas en board
// - soporta contexto desde stack
// - aun no resuelve progreso de cartas guardadas dentro de
//   contenedores porque esas cartas hoy salen fisicamente del board
// ============================================================
public class CardTransformationSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CardTransformationExecutor transformationExecutor;

    private readonly HashSet<CardInstance> trackedCards = new HashSet<CardInstance>();
    private readonly List<CardInstance> iterationBuffer = new List<CardInstance>();
    private readonly HashSet<CardCapabilityType> contextCapabilitiesBuffer = new HashSet<CardCapabilityType>();

    private BoardRoot subscribedBoardRoot;

    private void Awake()
    {
        if (transformationExecutor == null)
            transformationExecutor = FindFirstObjectByType<CardTransformationExecutor>();
    }

    private void OnEnable()
    {
        BoardRoot.OnBoardRootAvailable += HandleBoardRootAvailable;
        TrySubscribeToBoardRoot();
    }

    private void OnDisable()
    {
        BoardRoot.OnBoardRootAvailable -= HandleBoardRootAvailable;
        UnsubscribeFromBoardRoot();
        trackedCards.Clear();
        iterationBuffer.Clear();
        contextCapabilitiesBuffer.Clear();
    }

    private void Update()
    {
        float deltaTime = GameTimeService.GetTimedSystemsDeltaTime();
        RefreshTrackedCards();
        AdvanceTrackedTransformations(deltaTime);
    }

    private void HandleBoardRootAvailable(BoardRoot boardRoot)
    {
        SubscribeToBoardRoot(boardRoot);
    }

    private void TrySubscribeToBoardRoot()
    {
        if (BoardRoot.Instance != null)
            SubscribeToBoardRoot(BoardRoot.Instance);
    }

    private void SubscribeToBoardRoot(BoardRoot boardRoot)
    {
        if (boardRoot == null || ReferenceEquals(subscribedBoardRoot, boardRoot))
            return;

        UnsubscribeFromBoardRoot();

        subscribedBoardRoot = boardRoot;
        subscribedBoardRoot.OnCardRegistered += HandleCardRegistered;
        subscribedBoardRoot.OnCardUnregistered += HandleCardUnregistered;

        BootstrapTrackedCards();
    }

    private void UnsubscribeFromBoardRoot()
    {
        if (subscribedBoardRoot == null)
            return;

        subscribedBoardRoot.OnCardRegistered -= HandleCardRegistered;
        subscribedBoardRoot.OnCardUnregistered -= HandleCardUnregistered;
        subscribedBoardRoot = null;
    }

    private void BootstrapTrackedCards()
    {
        trackedCards.Clear();

        if (subscribedBoardRoot == null || subscribedBoardRoot.ActiveCards == null)
            return;

        for (int i = 0; i < subscribedBoardRoot.ActiveCards.Count; i++)
            TryTrackCard(subscribedBoardRoot.ActiveCards[i]);
    }

    private void HandleCardRegistered(CardInstance instance)
    {
        TryTrackCard(instance);
    }

    private void HandleCardUnregistered(CardInstance instance)
    {
        if (instance != null)
            trackedCards.Remove(instance);
    }

    private void TryTrackCard(CardInstance instance)
    {
        if (!CanTrackCard(instance))
            return;

        trackedCards.Add(instance);
    }

    private void RefreshTrackedCards()
    {
        // Las cartas pueden registrarse en BoardRoot antes de terminar
        // su Initialize(...). Por eso, en cada refresh re-sincronizamos
        // tambien contra ActiveCards para descubrir transformaciones que
        // quedaron listas despues del spawn.
        SyncTrackedCardsFromBoard();

        if (trackedCards.Count == 0)
            return;

        iterationBuffer.Clear();
        iterationBuffer.AddRange(trackedCards);

        for (int i = 0; i < iterationBuffer.Count; i++)
        {
            CardInstance instance = iterationBuffer[i];
            if (CanTrackCard(instance))
                continue;

            trackedCards.Remove(instance);
        }
    }

    private void SyncTrackedCardsFromBoard()
    {
        if (subscribedBoardRoot == null || subscribedBoardRoot.ActiveCards == null)
            return;

        for (int i = 0; i < subscribedBoardRoot.ActiveCards.Count; i++)
            TryTrackCard(subscribedBoardRoot.ActiveCards[i]);
    }

    private void AdvanceTrackedTransformations(float deltaTime)
    {
        if (trackedCards.Count == 0)
            return;

        iterationBuffer.Clear();
        iterationBuffer.AddRange(trackedCards);

        for (int i = 0; i < iterationBuffer.Count; i++)
        {
            CardInstance instance = iterationBuffer[i];
            if (!CanTrackCard(instance))
                continue;

            AdvanceCardTransformation(instance, deltaTime);
        }
    }

    private void AdvanceCardTransformation(CardInstance instance, float deltaTime)
    {
        CardTransformationRuntime runtime = instance.TransformationRuntime;
        CardTransformationRule rule = runtime != null ? runtime.ActiveRule : null;
        if (runtime == null || rule == null)
            return;

        if (rule.runOnlyOnBoard && !IsCardOnBoard(instance))
        {
            runtime.SetRunning(false);
            runtime.SetPaused(true);
            runtime.SetSpeedMultiplier(0f);
            return;
        }

        runtime.SetRunning(true);

        GatherContextCapabilities(instance, contextCapabilitiesBuffer);

        bool missingRequiredCapabilities = !HasRequiredCapabilities(rule, contextCapabilitiesBuffer);
        bool shouldPause = missingRequiredCapabilities || ShouldPause(rule, contextCapabilitiesBuffer);
        runtime.SetPaused(shouldPause);

        float speedMultiplier = shouldPause ? 0f : CalculateEffectiveSpeed(rule, contextCapabilitiesBuffer);
        runtime.SetSpeedMultiplier(speedMultiplier);

        if (!shouldPause && deltaTime > 0f)
            runtime.Advance(deltaTime);

        if (!runtime.IsComplete)
            return;

        if (transformationExecutor == null)
            transformationExecutor = FindFirstObjectByType<CardTransformationExecutor>();

        if (transformationExecutor == null)
            return;

        runtime.SetRunning(false);
        trackedCards.Remove(instance);
        transformationExecutor.ExecuteTransformation(instance, rule);
    }

    private bool CanTrackCard(CardInstance instance)
    {
        if (instance == null || instance.data == null)
            return false;

        if (instance.TransformationRuntime == null || !instance.TransformationRuntime.isActiveAndEnabled)
            return false;

        return instance.TransformationRuntime.ActiveRule != null;
    }

    private bool IsCardOnBoard(CardInstance instance)
    {
        if (instance == null || subscribedBoardRoot == null || subscribedBoardRoot.ActiveCards == null)
            return false;

        if (trackedCards.Contains(instance))
            return true;

        for (int i = 0; i < subscribedBoardRoot.ActiveCards.Count; i++)
        {
            if (subscribedBoardRoot.ActiveCards[i] == instance)
                return true;
        }

        return false;
    }

    private void GatherContextCapabilities(CardInstance sourceInstance, HashSet<CardCapabilityType> buffer)
    {
        buffer.Clear();

        if (sourceInstance == null)
            return;

        CardStack currentStack = sourceInstance.CurrentStack;
        if (currentStack == null || currentStack.Cards == null)
            return;

        for (int i = 0; i < currentStack.Cards.Count; i++)
        {
            CardView card = currentStack.Cards[i];
            CardInstance instance = card != null ? card.Instance : null;
            if (instance == null || instance == sourceInstance || instance.data == null || instance.data.capabilities == null)
                continue;

            for (int capabilityIndex = 0; capabilityIndex < instance.data.capabilities.Count; capabilityIndex++)
            {
                CardCapabilityType capability = instance.data.capabilities[capabilityIndex];
                if (capability == CardCapabilityType.None)
                    continue;

                buffer.Add(capability);
            }
        }
    }

    private bool ShouldPause(CardTransformationRule rule, HashSet<CardCapabilityType> contextCapabilities)
    {
        if (rule == null || rule.pauseCapabilities == null || rule.pauseCapabilities.Count == 0)
            return false;

        for (int i = 0; i < rule.pauseCapabilities.Count; i++)
        {
            CardCapabilityType capability = rule.pauseCapabilities[i];
            if (capability == CardCapabilityType.None)
                continue;

            if (contextCapabilities.Contains(capability))
                return true;
        }

        return false;
    }

    private bool HasRequiredCapabilities(CardTransformationRule rule, HashSet<CardCapabilityType> contextCapabilities)
    {
        if (rule == null || rule.requiredCapabilities == null || rule.requiredCapabilities.Count == 0)
            return true;

        for (int i = 0; i < rule.requiredCapabilities.Count; i++)
        {
            CardCapabilityType capability = rule.requiredCapabilities[i];
            if (capability == CardCapabilityType.None)
                continue;

            if (!contextCapabilities.Contains(capability))
                return false;
        }

        return true;
    }

    private float CalculateEffectiveSpeed(CardTransformationRule rule, HashSet<CardCapabilityType> contextCapabilities)
    {
        if (rule == null)
            return 1f;

        float multiplier = 1f;
        if (rule.speedModifiers == null || rule.speedModifiers.Count == 0)
            return multiplier;

        for (int i = 0; i < rule.speedModifiers.Count; i++)
        {
            CardTransformationSpeedModifier modifier = rule.speedModifiers[i];
            if (modifier == null || modifier.capability == CardCapabilityType.None)
                continue;

            if (!contextCapabilities.Contains(modifier.capability))
                continue;

            multiplier *= Mathf.Max(0f, modifier.speedMultiplier);
        }

        return Mathf.Max(0f, multiplier);
    }
}
