using UnityEngine;

public class DupePlayerStats : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth;

    [Header("Player Hunger")]
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float currentHunger;

    [Header("Player Thirst")]
    [SerializeField] private float maxThirst = 100;
    [SerializeField] private float currentThirst;

    [Header("Player Stamina")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaRegenRate; // Stamina per second
    [SerializeField] private float staminaDepletionRate; // Stamina per second while sprinting
    [SerializeField] private float staminaRegenDelay; // Delay before regeneration starts
    [SerializeField] private float staminaJumpPenalty;
    private float staminaRegenTimer;

    [Header("Player Sanity")]
    [SerializeField] private int maxSanity = 100;
    [SerializeField] private int currentSanity;

    private bool isSprinting = false;

    private void Start()
    {
        // Initialize stats to maximum
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentStamina = maxStamina;
        currentSanity = maxSanity;
    }

    private void Update()
    {
        HandleStamina();
    }

    public void SetSprinting(bool sprinting)
    {
        isSprinting = sprinting;
        if (isSprinting)
        {
            staminaRegenTimer = staminaRegenDelay; // Reset regen delay when sprinting starts
        }
    }

    public bool CanJump()
    {
        return currentStamina >= staminaJumpPenalty;
    }

    public void ApplyJumpPenalty()
    {
        currentStamina = Mathf.Max(0, currentStamina - staminaJumpPenalty);
    }

    private void HandleStamina()
    {
        if (isSprinting && currentStamina > 0)
        {
            currentStamina -= staminaDepletionRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }
        else if (!isSprinting && currentStamina < maxStamina)
        {
            staminaRegenTimer -= Time.deltaTime;
            if (staminaRegenTimer <= 0)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
        
    }






    // Prevent sprinting if out of stamina
    public bool CanSprint()
    {
        return currentStamina > 0;
    }
}
