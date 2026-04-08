using UnityEngine;

// ============================================================
// FoodRuntime
// ------------------------------------------------------------
// Estado runtime especifico de cartas de comida.
//
// Ownership:
// - el asset define `foodValue`
// - este runtime guarda cuanto valor de comida sigue disponible
// - el upkeep diario consume parcialmente desde aqui
// ============================================================
public class FoodRuntime : MonoBehaviour
{
    [SerializeField] private FoodResourceCardData foodData;
    [SerializeField] private int remainingFoodValue;

    public FoodResourceCardData FoodData => foodData;
    public int RemainingFoodValue => Mathf.Max(0, remainingFoodValue);

    public void Initialize(FoodResourceCardData data)
    {
        foodData = data;
        remainingFoodValue = data != null ? Mathf.Max(0, data.foodValue) : 0;
    }

    public void SetRemainingFoodValue(int value)
    {
        remainingFoodValue = Mathf.Max(0, value);
    }

    public int ConsumeFoodValue(int requestedAmount)
    {
        if (requestedAmount <= 0 || remainingFoodValue <= 0)
            return 0;

        int consumedAmount = Mathf.Min(requestedAmount, remainingFoodValue);
        remainingFoodValue -= consumedAmount;
        return consumedAmount;
    }
}
