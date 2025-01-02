using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;
    [SerializeField] private Slider staminaSlider;

    private DupePlayerStats playerStats;

    private void Start()
    {
        TryInitializePlayerStats();
    }

    private void Update()
    {
        if (playerStats != null)
        {
            UpdateSliders();
        }
    }

    private void TryInitializePlayerStats()
    {
        playerStats = StatsStorage.Instance?.PlayerStats;

        if (playerStats == null)
        {
            Debug.LogWarning("PlayerStats not yet available. Retrying...");
            Invoke(nameof(TryInitializePlayerStats), 0.5f); // Retry initialization
        }
        else
        {
            Debug.Log("PlayerStats successfully linked to PlayerUIController.");
            UpdateSliders();
        }
    }

    private void UpdateSliders()
    {
        healthSlider.value = playerStats.currentHealth / playerStats.maxHealth;
        hungerSlider.value = playerStats.currentHunger / playerStats.maxHunger;
        thirstSlider.value = playerStats.currentThirst / playerStats.maxThirst;
        staminaSlider.value = playerStats.currentStamina / playerStats.maxStamina;
    }
}