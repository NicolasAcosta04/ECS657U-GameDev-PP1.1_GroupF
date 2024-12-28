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
        playerStats = GeneralSettings.Instance.PlayerStats;
    }

    private void Update()
    {
        if (playerStats != null)
        {
            healthSlider.value = playerStats.currentHealth / playerStats.maxHealth;
            hungerSlider.value = playerStats.currentHunger / playerStats.maxHunger;
            thirstSlider.value = playerStats.currentThirst / playerStats.maxThirst;
            staminaSlider.value = playerStats.currentStamina / playerStats.maxStamina;
        }
    }
}