using System.Collections.Generic;
using UnityEngine;

// ============================================================
// BaseMarketPackData
// ------------------------------------------------------------
// Contrato comun para cualquier tipo de pack del Market.
// Todas las variantes comparten identidad, precio, carta fisica
// y el metodo que devuelve que cartas salen al abrirlo.
// ============================================================
public abstract class BaseMarketPackData : ScriptableObject
{
    [Header("Identity")]
    // Nombre visible del sobre dentro de la tienda.
    public string displayName;

    [Header("Cost")]
    // Costo total en currency normal para comprar este sobre.
    [Min(1)]
    public int price = 1;

    [Header("Pack Card")]
    // Carta fisica que aparece en el tablero cuando el jugador compra el sobre.
    public CardData packCard;

    public abstract List<CardData> GetOpenedCards();
}
