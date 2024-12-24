using UnityEngine;

public class DupePlayerStats : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthHungerDepletionRate; // Health depletion rate when hunger is empty
    [SerializeField] private float healthHungerRegenRate; // Health regeneration rate when hunger is full


    [Header("Player Hunger")]
    [SerializeField] private float maxHunger = 100;
    [SerializeField] private float currentHunger;
    [SerializeField] private float hungerDepletionRate; // Hunger depletion per second
    [SerializeField] private float hungerDepletionDelay; // Time before hunger starts depleting
    private float hungerDepletionTimer = 0f;



    [Header("Player Thirst")]
    [SerializeField] private float maxThirst = 100;
    [SerializeField] private float currentThirst;
    [SerializeField] private float thirstDepletionRate; // Thirst depletion per second


    [Header("Player Stamina")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaRegenRate; // Stamina per second
    [SerializeField] private float staminaDepletionRate; // Stamina per second while sprinting
    [SerializeField] private float staminaRegenDelay; // Delay before regeneration starts
    [SerializeField] private float staminaJumpPenalty;
    private float staminaRegenTimer;

    [Header("Player Sanity")]
    [SerializeField] private float maxSanity = 100;
    [SerializeField] private float currentSanity;
    [SerializeField] private float hungerSanityDepletion; // Sanity loss modifier
    [SerializeField] private float hungerMinPsycheThreshold; // Threshold for faster sanity loss



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
        HandleHunger();
        HandleThirst();
    }




// stamina handling -- all stamina handling under this comment

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

    public bool CanSprint()
    {
        return currentStamina > 0;
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

// END stamnia handling -- end all stamina handling here 




// hunger handling -- all hunger handling under this comment

    private void HandleHunger()
    {
        // If hunger is full, regenerate health
        if (currentHunger == maxHunger)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + healthHungerRegenRate * Time.deltaTime);
        }

        // Decrement the depletion timer if it's above zero
        if (hungerDepletionTimer > 0)
        {
            hungerDepletionTimer -= Time.deltaTime;
        }
        else if (currentHunger > 0)
        {
            // Deplete hunger when the delay timer reaches zero
            currentHunger -= hungerDepletionRate * Time.deltaTime;
            currentHunger = Mathf.Max(0, currentHunger); // Clamp to 0
        }

        // Apply penalties when hunger is empty
        if (currentHunger == 0)
        {
            currentHealth -= healthHungerDepletionRate * Time.deltaTime;
            currentHealth = Mathf.Max(0, currentHealth); // Clamp to 0
        }

        // Apply sanity depletion modifier if hunger is below the threshold
        if (currentHunger < hungerMinPsycheThreshold)
        {
            currentSanity -= hungerSanityDepletion * Time.deltaTime;
            currentSanity = Mathf.Max(0, currentSanity); // Clamp to 0
        }
    }

    private void BeginHungerDepletion()
    {

    }

    private void ApplyHungerPenalty()
    {
        // Example: Reduce health due to starvation
        currentHealth = Mathf.Max(0, currentHealth - hungerDepletionRate * Time.deltaTime);
    }

// END hunger handling -- end all hunger handling here 





// thirst handling -- all thirst handling under this comment

    private void HandleThirst()
    {
        if (currentThirst > 0)
        {
            currentThirst -= thirstDepletionRate * Time.deltaTime;
            currentThirst = Mathf.Max(0, currentThirst); // Clamp to prevent negative values
        }
        else
        {
            // Thirst effects (e.g., stamina reduction)
            ApplyThirstPenalty();
        }
    }

    private void ApplyThirstPenalty()
    {
        // Example: Reduce stamina due to dehydration
        currentStamina = Mathf.Max(0, currentStamina - thirstDepletionRate * Time.deltaTime);
    }

// END thirst handling -- end all thirst handling here 
}
