using System.Collections; // Ensure this namespace is included
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class InventoryUIManager : MonoBehaviour
{
    public List<GameObject> SlotUIs;
    public TMP_Text currentItemText; // Reference to "Current Item" TMP_Text
    public float fadeDuration = 1.5f; // Duration of the fade effect

    private Coroutine fadeCoroutine; // To keep track of the active fade-out coroutine

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
            var stackNumberText = slotUI.transform.Find("Stack Number")?.GetComponent<TMP_Text>();

            if (border == null || itemImage == null || stackNumberText == null)
            {
                Debug.LogError($"SlotUI {slotUI.name} is missing required child components (Border/ItemImage/Stack Number).");
                continue;
            }

            // Ensure all slots are always visible
            slotUI.SetActive(true);

            // Update the `ItemImage` sprite and visibility
            if (i < slots.Count && slots[i].ItemSprite != null)
            {
                itemImage.sprite = slots[i].ItemSprite;
                itemImage.enabled = true;

                // Update the stack number
                if (slots[i].StackCount > 1)
                {
                    stackNumberText.text = slots[i].StackCount.ToString();
                    stackNumberText.enabled = true;
                }
                else
                {
                    stackNumberText.text = "";
                    stackNumberText.enabled = false; // Hide if stack is 0 or 1
                }

                // Show the item name in "Current Item" text when selected
                if (i == selectedSlotIndex)
                {
                    ShowCurrentItemText(slots[i].ItemName);
                }
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false; // Disable the image if no item
                stackNumberText.text = "";
                stackNumberText.enabled = false; // Hide stack number if no item
            }

            // Highlight the selected slot
            border.color = i == selectedSlotIndex ? Color.yellow : Color.white;
        }
    }

    private void ShowCurrentItemText(string itemName)
    {
        if (currentItemText != null)
        {
            // Stop the current fade-out coroutine, if any
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // Set the text and reset its alpha to fully visible
            currentItemText.text = itemName;
            currentItemText.color = new Color(currentItemText.color.r, currentItemText.color.g, currentItemText.color.b, 1);

            // Start the fade-out coroutine
            fadeCoroutine = StartCoroutine(FadeOutText());
        }
    }

    private IEnumerator FadeOutText()
    {
        float elapsedTime = 0f;
        Color originalColor = currentItemText.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            currentItemText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        currentItemText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0); // Ensure fully transparent at the end
        fadeCoroutine = null; // Clear the reference once finished
    }
}
