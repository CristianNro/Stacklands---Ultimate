using UnityEngine;

[CreateAssetMenu(fileName = "PackCard", menuName = "Cards/Pack Card")]
public class PackCardData : ItemCardData
{
    [Header("Pack")]
    // Definicion del contenido de este sobre.
    // Permite que la carta-pack conozca su propio contenido aunque haya
    // aparecido fuera del Market.
    public BaseMarketPackData embeddedPackData;
}
