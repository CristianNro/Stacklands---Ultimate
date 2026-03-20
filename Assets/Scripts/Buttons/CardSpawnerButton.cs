using UnityEngine;
using UnityEngine.EventSystems;

// Este componente vincula un botón de UI con la creación de cartas.
public class CardSpawnerButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CardData cardToSpawn;
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CardSpawner.Instance == null)
        {
            Debug.LogError("No existe CardSpawner.Instance en la escena.");
            return;
        }

        CardSpawner.Instance.Spawn(cardToSpawn, spawnPosition);
    }
}