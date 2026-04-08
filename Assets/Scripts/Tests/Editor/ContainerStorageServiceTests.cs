using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StacklandsLike.Cards;
using UnityEngine;

public class ContainerStorageServiceTests
{
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < createdObjects.Count; i++)
        {
            if (createdObjects[i] != null)
                Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void StoredSnapshot_RoundTrip_PreservesUsesAndRuntimeValueOverride()
    {
        CardData cardData = CreateCard("Apple", value: 2, maxUses: 5);
        CardInstance sourceInstance = CreateCardInstance(cardData, new Vector2(12f, -8f));
        sourceInstance.usesRemaining = 3;
        sourceInstance.SetRuntimeValueOverride(9);

        ContainerStorageService.StoredCardSnapshot snapshot = sourceInstance.CreateStoredSnapshot();

        Assert.That(snapshot, Is.Not.Null);
        Assert.That(snapshot.definition, Is.EqualTo(cardData));
        Assert.That(snapshot.runtime.usesRemaining, Is.EqualTo(3));
        Assert.That(snapshot.runtime.hasRuntimeValueOverride, Is.True);
        Assert.That(snapshot.runtime.runtimeValueOverride, Is.EqualTo(9));
        Assert.That(snapshot.runtime.anchoredPosition, Is.EqualTo(new Vector2(12f, -8f)));

        CardInstance restoredInstance = CreateCardInstance(cardData, Vector2.zero);
        restoredInstance.usesRemaining = 0;
        restoredInstance.ClearRuntimeValueOverride();

        restoredInstance.ApplyStoredSnapshot(snapshot);

        Assert.That(restoredInstance.usesRemaining, Is.EqualTo(3));
        Assert.That(restoredInstance.HasRuntimeValueOverride, Is.True);
        Assert.That(restoredInstance.RuntimeValueOverride, Is.EqualTo(9));
        Assert.That(restoredInstance.GetEffectiveValue(), Is.EqualTo(9));
    }

    [Test]
    public void GetStoredTotalValue_UsesRuntimeOverrideWhenPresent()
    {
        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        createdObjects.Add(storage.gameObject);

        string containerId = System.Guid.NewGuid().ToString();
        ContainerCardData containerData = CreateContainer("Chest", capacity: 5);

        CardData baseCoin = CreateCard("Coin", value: 2);
        CardInstance normalInstance = CreateCardInstance(baseCoin, Vector2.zero);

        CardData specialCoin = CreateCard("SpecialCoin", value: 4);
        CardInstance overrideInstance = CreateCardInstance(specialCoin, Vector2.zero);
        overrideInstance.SetRuntimeValueOverride(11);

        Assert.That(storage.StoreCard(containerId, containerData, normalInstance), Is.True);
        Assert.That(storage.StoreCard(containerId, containerData, overrideInstance), Is.True);

        int totalValue = storage.GetStoredTotalValue(containerId);

        Assert.That(totalValue, Is.EqualTo(13));
    }

    [Test]
    public void CanStoreCard_ReturnsFalse_WhenContainerIsAtCapacity()
    {
        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        createdObjects.Add(storage.gameObject);

        string containerId = System.Guid.NewGuid().ToString();
        ContainerCardData containerData = CreateContainer("SmallChest", capacity: 1);

        CardData firstCard = CreateCard("FirstCoin", value: 1);
        CardData secondCard = CreateCard("SecondCoin", value: 1);

        Assert.That(storage.StoreCard(containerId, containerData, CreateCardInstance(firstCard, Vector2.zero)), Is.True);
        Assert.That(storage.CanStoreCard(containerId, containerData), Is.False);
        Assert.That(storage.StoreCard(containerId, containerData, CreateCardInstance(secondCard, Vector2.zero)), Is.False);
    }

    [Test]
    public void AddStoredSnapshots_AndRemoveStoredRecords_UpdateStoredContents()
    {
        ContainerStorageService storage = ContainerStorageService.GetOrCreate();
        createdObjects.Add(storage.gameObject);

        string containerId = System.Guid.NewGuid().ToString();
        CardData apple = CreateCard("Apple", value: 2);
        CardData berry = CreateCard("Berry", value: 3);

        ContainerStorageService.StoredCardSnapshot appleSnapshot =
            ContainerStorageService.CreateSnapshotFromCardData(apple, usesRemaining: 1, anchoredPosition: new Vector2(5f, 6f));
        ContainerStorageService.StoredCardSnapshot berrySnapshot =
            ContainerStorageService.CreateSnapshotFromCardData(berry, usesRemaining: 2, anchoredPosition: new Vector2(-3f, 4f));

        storage.AddStoredSnapshots(containerId, new List<ContainerStorageService.StoredCardSnapshot> { appleSnapshot, berrySnapshot });

        List<ContainerStorageService.StoredCardSnapshot> storedAfterAdd = storage.GetStoredContentsSnapshot(containerId);
        Assert.That(storedAfterAdd.Count, Is.EqualTo(2));
        Assert.That(storedAfterAdd.Any(snapshot => ReferenceEquals(snapshot, appleSnapshot)), Is.True);
        Assert.That(storedAfterAdd.Any(snapshot => ReferenceEquals(snapshot, berrySnapshot)), Is.True);

        storage.RemoveStoredRecords(containerId, new List<ContainerStorageService.StoredCardSnapshot> { appleSnapshot });

        List<ContainerStorageService.StoredCardSnapshot> storedAfterRemove = storage.GetStoredContentsSnapshot(containerId);
        Assert.That(storedAfterRemove.Count, Is.EqualTo(1));
        Assert.That(storedAfterRemove.Any(snapshot => ReferenceEquals(snapshot, appleSnapshot)), Is.False);
        Assert.That(storedAfterRemove.Any(snapshot => ReferenceEquals(snapshot, berrySnapshot)), Is.True);
    }

    private CardData CreateCard(string id, int value = 0, int maxUses = 0)
    {
        ResourceCardData cardData = ScriptableObject.CreateInstance<ResourceCardData>();
        cardData.name = id;
        cardData.id = id;
        cardData.cardName = id;
        cardData.cardType = CardType.Resource;
        cardData.value = value;
        cardData.maxUses = maxUses;
        createdObjects.Add(cardData);
        return cardData;
    }

    private ContainerCardData CreateContainer(string id, int capacity)
    {
        ContainerCardData containerData = ScriptableObject.CreateInstance<ContainerCardData>();
        containerData.name = id;
        containerData.id = id;
        containerData.cardName = id;
        containerData.cardType = CardType.Container;
        containerData.capacity = capacity;
        createdObjects.Add(containerData);
        return containerData;
    }

    private CardInstance CreateCardInstance(CardData data, Vector2 anchoredPosition)
    {
        GameObject go = new GameObject(data != null ? data.name : "Card", typeof(RectTransform), typeof(CardInstance));
        createdObjects.Add(go);

        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;

        CardInstance instance = go.GetComponent<CardInstance>();
        instance.Initialize(data);

        return instance;
    }
}
