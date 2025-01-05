using UnityEngine;

public class DamageEffect : EnemyEffect
{
    [Header("Damage Settings")]
    public float damageAmount = 10f; // Health damage amount
    public float damageDuration = 0f; // Time for health damage (0 for instant damage)
    public float sanityDamageAmount = 5f; // Sanity damage amount
    public float sanityDamageDuration = 0f; // Time for sanity damage (0 for instant damage)

    // ApplyEffect implementation
    public override void ApplyEffect(GameObject target)
    {
        var playerStats = target.GetComponent<DupePlayerStats>();
        if (playerStats != null)
        {
            // Apply health damage
            playerStats.TakeDamage(damageAmount, damageDuration);

            // Apply sanity damage
            playerStats.TakeSanityDamage(sanityDamageAmount, sanityDamageDuration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyEffect(collision.gameObject);
        }
    }
}
