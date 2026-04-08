using System.Collections.Generic;
using StacklandsLike.Cards;

public class RecipeMatchInput
{
    public CardStack sourceStack;
    public readonly List<RecipeMatchInputCard> cards = new List<RecipeMatchInputCard>();

    public static RecipeMatchInput FromStack(CardStack stack)
    {
        RecipeMatchInput input = new RecipeMatchInput
        {
            sourceStack = stack
        };

        if (stack == null)
            return input;

        IReadOnlyList<CardView> stackCards = stack.Cards;
        for (int i = 0; i < stackCards.Count; i++)
        {
            CardView cardView = stackCards[i];
            if (cardView == null || cardView.Instance == null || cardView.Instance.data == null)
                continue;

            if (cardView.Instance.IsBusy || cardView.Instance.IsInCombat())
            {
                input.cards.Clear();
                return input;
            }

            RecipeMatchInputCard card = new RecipeMatchInputCard
            {
                data = cardView.Instance.data
            };

            if (card.data.capabilities != null)
                card.capabilities.AddRange(card.data.capabilities);

            input.cards.Add(card);
        }

        return input;
    }

    public List<CardData> GetCardDataList()
    {
        List<CardData> result = new List<CardData>();

        for (int i = 0; i < cards.Count; i++)
        {
            RecipeMatchInputCard card = cards[i];
            if (card == null || card.data == null)
                continue;

            result.Add(card.data);
        }

        return result;
    }

    public int CountCardsWithCapability(CardCapabilityType capability)
    {
        if (capability == CardCapabilityType.None)
            return 0;

        int count = 0;

        for (int i = 0; i < cards.Count; i++)
        {
            RecipeMatchInputCard card = cards[i];
            if (card != null && card.HasCapability(capability))
                count++;
        }

        return count;
    }
}
