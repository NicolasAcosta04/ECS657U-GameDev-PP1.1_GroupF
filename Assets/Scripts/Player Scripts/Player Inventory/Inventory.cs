using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class ItemSlot
    {
        public int ItemID;
        public int StackCount;
        public string ItemName; // Display in inspector
    }

    public List<ItemSlot> Slots = new List<ItemSlot>(6);
    [SerializeField] private int maxStackSize = 5;
    [SerializeField] private float pickupRange = 2f; // Pickup range
    [SerializeField] private KeyCode pickUpKey = KeyCode.E;
    [SerializeField] private KeyCode dropKey = KeyCode.Q;
    [SerializeField] private KeyCode useItemKey = KeyCode.F; // New "use item" key
    [SerializeField] private Transform dropPoint; // Drop point
    [SerializeField] private LayerMask pickupLayer; // Layer for pickupable items
    [SerializeField] private KeyCode[] inventoryKeys =
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
    };

    private int selectedSlotIndex = -1; // No slot selected by default

    private void Start()
    {
        LoadInventoryFromStorage();
    }

    private void OnDestroy()
    {
        SaveInventoryToStorage();
    }

    private void SaveInventoryToStorage()
    {
        InventoryStorage.Instance.PersistentSlots.Clear();
        InventoryStorage.Instance.PersistentSlots.AddRange(Slots);
    }

    private void LoadInventoryFromStorage()
    {
        Slots.Clear();
        Slots.AddRange(InventoryStorage.Instance.PersistentSlots);
        UpdateUI();
    }

    private void Update()
    {
        HandleInventorySelection();

        if (Input.GetKeyDown(pickUpKey))
        {
            TryPickupItem();
        }

        if (Input.GetKeyDown(dropKey) && selectedSlotIndex != -1)
        {
            DropItem(selectedSlotIndex); // Drop item from the selected slot
        }

        if (Input.GetKeyDown(useItemKey) && selectedSlotIndex != -1)
        {
            UseItem(selectedSlotIndex); // Use item from the selected slot
        }
    }

    private void HandleInventorySelection()
    {
        for (int i = 0; i < inventoryKeys.Length; i++)
        {
            if (Input.GetKeyDown(inventoryKeys[i]))
            {
                if (Slots[i].StackCount > 0) // Only select slots with items
                {
                    SelectSlot(i);
                }
                else
                {
                    Debug.Log($"Slot {i + 1} is empty!");
                }
            }
        }
    }

    private void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        Debug.Log($"Selected Slot: {index + 1}, Item: {Slots[index].ItemName}, Stack: {Slots[index].StackCount}");
        UpdateUI(); // Update UI to reflect the current selection
    }

    private void TryPickupItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

        foreach (var collider in colliders)
        {
            var item = collider.GetComponent<PickupItem>();
            if (item != null)
            {
                AddItem(item.ItemID, item.ItemName);
                Destroy(collider.gameObject); // Remove item from the world
                return;
            }
        }

        Debug.Log("No item to pick up in range!");
    }

    public void AddItem(int itemID, string itemName)
    {
        foreach (var slot in Slots)
        {
            if (slot.ItemID == itemID && slot.StackCount < maxStackSize)
            {
                slot.StackCount++;
                UpdateUI();
                return;
            }
        }

        foreach (var slot in Slots)
        {
            if (slot.StackCount == 0)
            {
                slot.ItemID = itemID;
                slot.ItemName = itemName; // Set the Item Name
                slot.StackCount = 1;
                UpdateUI();
                return;
            }
        }

        Debug.Log("Inventory full!");
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count || Slots[slotIndex].StackCount <= 0)
        {
            Debug.LogWarning("Invalid slot or no item to use.");
            return;
        }

        string addressableKey = $"Assets/Prefabs/Tutorial Prefabs/{Slots[slotIndex].ItemName}.prefab";
        Addressables.LoadAssetAsync<GameObject>(addressableKey).Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                GameObject itemPrefab = handle.Result;
                ItemEffect effect = itemPrefab.GetComponent<ItemEffect>();
                if (effect != null)
                {
                    effect.ApplyEffect(gameObject); // Pass the player GameObject
                    Debug.Log($"Used {Slots[slotIndex].ItemName}");
                }
                else
                {
                    Debug.LogWarning($"No effect found on item {Slots[slotIndex].ItemName}");
                }

                // Decrease stack count after use
                Slots[slotIndex].StackCount--;
                if (Slots[slotIndex].StackCount == 0)
                {
                    Slots[slotIndex].ItemID = 0;
                    Slots[slotIndex].ItemName = "";
                }
                UpdateUI();
            }
            else
            {
                Debug.LogError($"Failed to load item prefab: {addressableKey}");
            }
        };
    }

    public void DropItem(int slotIndex)
    {
        if (Slots[slotIndex].StackCount > 0)
        {
            string addressableKey = $"Assets/Prefabs/Tutorial Prefabs/{Slots[slotIndex].ItemName}.prefab";
            Addressables.InstantiateAsync(addressableKey, dropPoint.position, Quaternion.identity).Completed += OnItemDropped;

            Slots[slotIndex].StackCount--;
            if (Slots[slotIndex].StackCount == 0)
            {
                Slots[slotIndex].ItemID = 0;
                Slots[slotIndex].ItemName = ""; // Clear the Item Name
            }
            UpdateUI();
        }
    }

    private void OnItemDropped(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError("Failed to drop item using Addressables.");
        }
        else
        {
            Debug.Log("Item dropped successfully.");
        }
    }

    private void UpdateUI()
    {
        var uiManager = GameObject.FindWithTag("UIManager").GetComponent<InventoryUIManager>();
        if (uiManager != null)
        {
            uiManager.UpdateInventoryUI(Slots, selectedSlotIndex);
        }
        else
        {
            Debug.LogWarning("UIManager not found for inventory update!");
        }
    }
}