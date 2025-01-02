using UnityEngine;

public class WaterEvent : ItemEffect
{
    [Header("Thirst Restoration Settings")]
    public float thirstRegenRate = 15f; // Thirst restored per second
    public float duration = 3f; // Effect duration in seconds

    public override void ApplyEffect(GameObject player)
    {
        DupePlayerStats playerStats = player.GetComponent<DupePlayerStats>();
        if (playerStats != null)
        {
            playerStats.RestoreThirst(thirstRegenRate, duration);
            Debug.Log($"Restoring thirst at {thirstRegenRate}/s for {duration} seconds.");
        }
    }
}