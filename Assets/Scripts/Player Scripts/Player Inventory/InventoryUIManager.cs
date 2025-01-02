using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public List<GameObject> SlotUIs;

    public void UpdateInventoryUI(List<Inventory.ItemSlot> slots, int selectedSlotIndex)
    {
        for (int i = 0; i < SlotUIs.Count; i++)
        {
            var slotUI = SlotUIs[i];
            var border = slotUI.transform.Find("Border").GetComponent<Image>();

            // Ensure the first slot (Slot) is always visible
            if (i == 0)
            {
                slotUI.SetActive(true);
            }
            else
            {
                // Make the slot visible if it has items, otherwise hide it
                slotUI.SetActive(slots[i].StackCount > 0);
            }

            // Update border color for selected slot
            if (slotUI.activeSelf)
            {
                border.color = i == selectedSlotIndex ? Color.yellow : Color.white;
            }
        }
    }
}