using UnityEngine;

// Este componente marca a la carta como "target" de drop (objetivo para soltar cartas).
// También define cómo se apilan cartas encima de ella (el desplazamiento visual).
public class CardDropTarget : MonoBehaviour
{
    // Offset local para apilar una carta encima de esta.
    // Vector2: (x: hacia la derecha, y: hacia arriba)
    // (-28f en Y significa "28 píxeles hacia ABAJO", así se ve la carta anterior).
    // Importante: es LOCAL a esta carta, no al Board. Es relativo a la posición de ESTA carta.
    [SerializeField] private Vector2 stackOffset = new Vector2(0f, -28f);

    // Método público: devuelve el offset local que debe usar una carta apilada encima de esta.
    // CardDrag llama a esto cuando una carta se suelta encima para saber dónde posicionarla.
    public Vector2 GetStackOffset()
    {
        return stackOffset;
    }
}