using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public List<GameObject> SlotUIs;

    public void UpdateInventoryUI(List<Inventory.ItemSlot> slots, int selectedSlotIndex)
    {
        if (SlotUIs == null || SlotUIs.Count == 0)
        {
            Debug.LogError("SlotUIs list is empty or not initialized.");
            return;
        }

        for (int i = 0; i < SlotUIs.Count; i++)
        {
            var slotUI = SlotUIs[i];
            if (slotUI == null)
            {
                Debug.LogError($"SlotUI at index {i} is null.");
                continue;
            }

            // Find components in the prefab structure
            var border = slotUI.transform.Find("Border")?.GetComponent<Image>();
            var itemImage = slotUI.transform.Find("Border/ItemImage")?.GetComponent<Image>();

            if (border == null || itemImage == null)
            {
                Debug.LogError($"SlotUI {slotUI.name} is missing required child components (Border/ItemImage).");
                continue;
            }

            // Ensure all slots are always visible
            slotUI.SetActive(true);

            // Update the `ItemImage` sprite and visibility
            if (i < slots.Count && slots[i].ItemSprite != null)
            {
                itemImage.sprite = slots[i].ItemSprite;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false; // Disable the image if no item
            }

            // Highlight the selected slot
            border.color = i == selectedSlotIndex ? Color.yellow : Color.white;
        }
    }
}
