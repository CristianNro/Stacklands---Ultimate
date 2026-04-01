using UnityEngine;
using StacklandsLike.Cards;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ContainerCard", menuName = "Cards/Container Card")]
public class ContainerCardData : CardData
{
    [Min(1)]
    // Cantidad maxima de cartas que pueden quedar guardadas dentro.
    public int capacity = 10;

    [Header("Accepted Cards")]
    // Define si la lista funciona como whitelist o blacklist.
    public ContainerListMode listMode = ContainerListMode.BlockListed;
    // Tipos de carta que el contenedor usa para permitir o bloquear ingreso.
    public List<CardType> listedCardTypes = new List<CardType>();

    [Header("Accepted Resources")]
    // Si esta activo, los recursos ademas pasan por un filtro especifico por ResourceType.
    public bool useResourceTypeFilter = false;
    // Define si la lista de recursos funciona como whitelist o blacklist.
    public ContainerListMode resourceListMode = ContainerListMode.AllowOnlyListed;
    // Tipos de recurso permitidos o bloqueados cuando el filtro especifico esta activo.
    public List<ResourceType> listedResourceTypes = new List<ResourceType>();

    [Header("Release Mode")]
    // Distancia aproximada desde el contenedor donde aparecen las cartas al soltarlas.
    public float releaseRadius = 80f;
    // Si es mayor a 0, limita cuantas cartas salen por doble click.
    // 0 o menos significa soltar todo el contenido.
    [Min(0)]
    public int maxCardsReleasedPerOpen = 0;
}
