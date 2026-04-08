using System;
using UnityEngine;

// ============================================================
// DayEventSpawnEntry
// ------------------------------------------------------------
// Entrada simple de spawn para un evento diario.
//
// Cada entrada define una carta y cuantas copias deben aparecer
// cuando el evento se ejecuta.
// ============================================================
[Serializable]
public class DayEventSpawnEntry
{
    public CardData card;
    [Min(1)] public int count = 1;
}
