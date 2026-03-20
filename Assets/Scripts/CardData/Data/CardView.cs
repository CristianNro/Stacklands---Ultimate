using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Vincula CardData/CardInstance con la representación visual.
public class CardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardInstance cardInstance;
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;

    private void Reset()
    {
        if (cardInstance == null) cardInstance = GetComponent<CardInstance>();
    }

    private void Awake()
    {
        if (cardInstance == null) cardInstance = GetComponent<CardInstance>();
    }

    private void OnValidate()
    {
        Refresh();
    }

    public void Refresh()
    {
        CardData data = cardInstance != null ? cardInstance.data : null;
        if (data == null) return;

        if (cardNameText != null)
            cardNameText.text = string.IsNullOrWhiteSpace(data.displayName)
                ? data.cardName
                : data.displayName;

        if (cardImage != null)
            cardImage.sprite = data.cardImage;
    }
}