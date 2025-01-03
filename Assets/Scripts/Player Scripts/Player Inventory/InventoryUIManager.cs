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

            // Ensure the first slot is always visible
            if (i == 0)
            {
                slotUI.SetActive(true);
            }
            else
            {
                // Make slot visible if it has items, otherwise hide it
                slotUI.SetActive(slots[i].StackCount > 0);
            }

            if (slotUI.activeSelf)
            {
                // Update the `ItemImage` sprite
                itemImage.sprite = slots[i].ItemSprite;
                itemImage.enabled = slots[i].ItemSprite != null;

                // Highlight the selected slot
                border.color = i == selectedSlotIndex ? Color.yellow : Color.white;
            }
        }
    }

}
