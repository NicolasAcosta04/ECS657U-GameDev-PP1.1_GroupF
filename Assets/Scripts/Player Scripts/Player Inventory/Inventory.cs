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
        public Sprite ItemSprite; // New field for the item's sprite
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

    public Transform displayItem; // Parent object for the held item (attach to camera in inspector)
    private GameObject HeldItem; // Currently displayed 3D model

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
                // If the key corresponds to the already selected slot, deselect it
                if (selectedSlotIndex == i)
                {
                    DeselectSlot(); // Deselect the current slot
                    return;
                }

                // Only select the slot if it has items
                if (Slots[i].StackCount > 0)
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

    private void DisplayHeldItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count || Slots[slotIndex].StackCount <= 0)
        {
            ClearHeldItem(); // No valid item in slot, clear the held item
            return;
        }

        // Get the item name to load its 3D model
        string addressableKey = $"Assets/Prefabs/Tutorial Prefabs/{Slots[slotIndex].ItemName}.prefab";

        Addressables.LoadAssetAsync<GameObject>(addressableKey).Completed += handle =>
        {
            if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                ClearHeldItem(); // Remove the previous model
                GameObject itemModel = handle.Result;

                // Instantiate the model and parent it to the displayItem
                HeldItem = Instantiate(itemModel, displayItem);
                HeldItem.transform.localPosition = Vector3.zero; // Adjust for desired position
                HeldItem.transform.localRotation = Quaternion.identity; // Adjust for desired rotation
                HeldItem.transform.localScale = Vector3.one * 0.2f; // Adjust scale to fit
                HeldItem.tag = "HeldItem";
            }
            else
            {
                Debug.LogError($"Failed to load item model: {addressableKey}");
            }
        };
    }

    private void ClearHeldItem()
    {
        if (HeldItem != null)
        {
            Destroy(HeldItem);
            HeldItem = null;
        }
    }

    private void SelectSlot(int index)
    {
        selectedSlotIndex = index;
        Debug.Log($"Selected Slot: {index + 1}, Item: {Slots[index].ItemName}, Stack: {Slots[index].StackCount}");
        UpdateUI(); // Update UI to reflect the current selection
        DisplayHeldItem(selectedSlotIndex); // Show the held item model
    }

    private void DeselectSlot()
    {
        Debug.Log($"Deselected Slot: {selectedSlotIndex + 1}");
        selectedSlotIndex = -1; // No slot is selected
        UpdateUI(); // Update UI to reflect the deselection
        ClearHeldItem(); // Remove the held item model
    }

    private void ClearSlot(int slotIndex)
    {
        Slots[slotIndex].ItemID = 0;
        Slots[slotIndex].ItemName = "";
        Slots[slotIndex].ItemSprite = null;
        if (selectedSlotIndex == slotIndex)
        {
            ClearHeldItem(); // Clear the held item if it corresponds to the emptied slot
        }
    }

    private void TryPickupItem()
    {
        // Get all colliders in pickup range
        Collider[] colliders = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);

        foreach (var collider in colliders)
        {
            var item = collider.GetComponent<PickupItem>();
            
            // Skip if this is the currently held item
            if (item != null && item.gameObject.CompareTag("HeldItem"))
            {
                continue;
            }

            if (item != null)
            {
                AddItem(item.ItemID, item.ItemName, item.ItemSprite);
                Destroy(collider.gameObject); // Remove item from the world
                return; // Exit after picking up one item
            }
        }

        Debug.Log("No item to pick up in range!");
    }


    public void EquipItem(GameObject itemPrefab)
    {
        if (HeldItem != null)
        {
            Destroy(HeldItem); // Remove the previously held item
        }

        HeldItem = Instantiate(itemPrefab, displayItem); // Instantiate the new held item
        HeldItem.transform.localPosition = Vector3.zero;
        HeldItem.transform.localRotation = Quaternion.identity;

        // Mark the item as being held
        HeldItem.tag = "HeldItem";
    }



    public void AddItem(int itemID, string itemName, Sprite itemSprite)
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
                slot.ItemName = itemName;
                slot.ItemSprite = itemSprite;
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
                    ClearSlot(slotIndex); // Clear the slot if empty

                    // Deselect the slot if it was the selected slot
                    if (selectedSlotIndex == slotIndex)
                    {
                        DeselectSlot();
                    }
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
                ClearSlot(slotIndex); // Clear the slot if empty

                // Deselect the slot if it was the selected slot
                if (selectedSlotIndex == slotIndex)
                {
                    DeselectSlot();
                }
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