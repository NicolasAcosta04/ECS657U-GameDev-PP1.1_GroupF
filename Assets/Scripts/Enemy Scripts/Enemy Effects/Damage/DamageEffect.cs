using UnityEngine;
public class DamageEffect : EnemyEffect
{
    [Header("Damage Settings")]
    public float damageAmount = 10f; // Amount of damage
    public float damageDuration = 0f; // Duration over which damage is applied (0 for instant damage)
    // ApplyEffect implementation
    public override void ApplyEffect(GameObject target)
    {
        var playerStats = target.GetComponent<DupePlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(damageAmount, damageDuration);
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