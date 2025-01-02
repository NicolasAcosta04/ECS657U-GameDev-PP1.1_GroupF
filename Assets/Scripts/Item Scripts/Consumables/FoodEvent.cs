using UnityEngine;

public class FoodEvent : ItemEffect
{
    [Header("Hunger Restoration Settings")]
    public float hungerRegenRate = 10f; // Hunger restored per second
    public float duration = 5f; // Effect duration in seconds

    public override void ApplyEffect(GameObject player)
    {
        DupePlayerStats playerStats = player.GetComponent<DupePlayerStats>();
        if (playerStats != null)
        {
            playerStats.RestoreHunger(hungerRegenRate, duration);
            Debug.Log($"Restoring hunger at {hungerRegenRate}/s for {duration} seconds.");
        }
    }
}