using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class DupePlayerStats : MonoBehaviour
{
    [Header("Player Health")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] public float currentHealth;
    [SerializeField] private float healthHungerRegenRate; // Health regeneration rate when hunger is full
    [SerializeField] private float baseHealthDepletionRate; // Base rate of health decrease
    [SerializeField] private float finalHealthDepletionRate;



    [Header("Player Hunger")]
    [SerializeField] public float maxHunger = 100f;
    [SerializeField] public float currentHunger;
    [SerializeField] private float hungerDepletionRate; // Hunger depletion per second
    [SerializeField] private float hungerDepletionDelay; // Time before hunger starts depleting
    [SerializeField] private float hungerHealthFactor; // Multiplier when hunger is 0
    [SerializeField] private float hungerSanityFactor; // Multiplier when hunger is below threshold
    [SerializeField] private float hungerPsychThreshold; // Minimum hunger threshold for sanity impact




    private float hungerDepletionTimer = 0f;


    [Header("Player Thirst")]
    [SerializeField] public float maxThirst = 100f;
    [SerializeField] public float currentThirst;
    [SerializeField] private float thirstDepletionRate; // Thirst depletion per second
    [SerializeField] private float thirstHealthFactor; // Multiplier when thirst is 0
    [SerializeField] private float thirstSanityFactor; // Multiplier when thirst is below threshold
    [SerializeField] private float thirstPsychThreshold; // Minimum thirst threshold for sanity impact





    [Header("Player Stamina")]
    [SerializeField] public float maxStamina = 100f;
    [SerializeField] public float currentStamina;
    [SerializeField] private float staminaRegenRate; // Stamina per second
    [SerializeField] private float staminaDepletionRate; // Stamina per second while sprinting
    [SerializeField] private float staminaRegenDelay; // Delay before regeneration starts
    [SerializeField] private float staminaJumpPenalty;
    private float staminaRegenTimer;




    [Header("Player Sanity")]
    [SerializeField] public float maxSanity = 100f;
    [SerializeField] public float currentSanity;
    [SerializeField] private float baseSanityDepletionRate; // Base rate of sanity decrease
    [SerializeField] private float finalSanityDepletionRate; // Display-only field





    private bool isSprinting = false;
    private void Start()
    {
        if (StatsStorage.Instance != null)
        {
            StatsStorage.Instance.PlayerStats = this;
            Debug.Log("PlayerStats assigned to StatsStorage.");
        }
        
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
        HandleHealth();
        HandleSanity();
    }



// Health handling -- all health handling under this comment

    private void HandleHealth()
    {
        float healthDecreaseMultiplier = 0f;

        if (currentHunger <= 0)
        {
            healthDecreaseMultiplier += hungerHealthFactor; // Apply hunger penalty
        }

        if (currentThirst <= 0)
        {
            healthDecreaseMultiplier += thirstHealthFactor; // Apply thirst penalty
        }

        // Calculate the final rate for display
        finalHealthDepletionRate = baseHealthDepletionRate * healthDecreaseMultiplier;

        if (healthDecreaseMultiplier > 0)
        {
            currentHealth -= finalHealthDepletionRate * Time.deltaTime;
            currentHealth = Mathf.Max(0, currentHealth); // Ensure health doesn't go below 0
        }

        if (currentHunger >= maxHunger)
        {
            currentHealth += healthHungerRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(maxHealth, currentHealth); // Ensure health doesn't exceed maximum
        }

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log("Player has died!");
        SceneManager.LoadScene("Game Over");
    }

    
    public void TakeDamage(float damageAmount, float duration)
    {
        if (duration <= 0)
        {
            // Instant damage
            currentHealth = Mathf.Max(0, currentHealth - damageAmount);
            Debug.Log($"Took {damageAmount} instant damage. Current health: {currentHealth}");
        }
        else
        {
            // Damage over time
            StartCoroutine(ApplyDamageOverTime(damageAmount, duration));
        }
    }


    private IEnumerator ApplyDamageOverTime(float damageAmount, float duration)
    {
        float elapsedTime = 0f;
        float damageRate = damageAmount / duration;
        while (elapsedTime < duration)
        {
            currentHealth = Mathf.Max(0, currentHealth - damageRate * Time.deltaTime);
            Debug.Log($"Took {damageRate * Time.deltaTime} damage. Current health: {currentHealth}");
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

// End health Handling


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
        staminaRegenTimer = staminaRegenDelay;
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
            // Stop regeneration if thirst is 0
            if (currentThirst > 0)
            {
                staminaRegenTimer -= Time.deltaTime;
                if (staminaRegenTimer <= 0)
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(maxStamina, currentStamina);
                }
            }
        }
    }

// END stamnia handling -- end all stamina handling here 




// hunger handling -- all hunger handling under this comment
    private void HandleHunger()
    {
        if (currentHunger >= maxHunger)
        {
            // If hunger is full, decrement the depletion timer without resetting it
            if (hungerDepletionTimer > 0)
            {
                hungerDepletionTimer -= Time.deltaTime;
            }
            else
            {
                // Start decrementing hunger after the delay
                currentHunger -= hungerDepletionRate * Time.deltaTime;
                currentHunger = Mathf.Max(0, currentHunger);
            }
        }
        else
        {
            // Reset depletion timer when hunger is not at maximum
            hungerDepletionTimer = hungerDepletionDelay;
        }
        
        if (currentHunger < maxHunger)
        {
            // Start decrementing hunger after the delay
            currentHunger -= hungerDepletionRate * Time.deltaTime;
            currentHunger = Mathf.Max(0, currentHunger);
        }
    }

    public void RestoreHunger(float amount, float duration)
    {
        StartCoroutine(RestoreHungerOverTime(amount, duration));
    }

    private IEnumerator RestoreHungerOverTime(float amount, float duration)
    {
        float rate = amount / duration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            currentHunger = Mathf.Min(maxHunger, currentHunger + rate * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
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
    }

    public void RestoreThirst(float amount, float duration)
    {
        StartCoroutine(RestoreThirstOverTime(amount, duration));
    }

    private IEnumerator RestoreThirstOverTime(float amount, float duration)
    {
        float rate = amount / duration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            currentThirst = Mathf.Min(maxThirst, currentThirst + rate * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

// END thirst handling -- end all thirst handling here 

// sanity handling -- all sanity handling under this comment


    private void HandleSanity()
    {
        float sanityDecreaseMultiplier = 0f;

        // Check if hunger is below threshold
        if (currentHunger < hungerPsychThreshold)
        {
            sanityDecreaseMultiplier += hungerSanityFactor; // Apply hunger penalty
        }

        // Check if thirst is below threshold
        if (currentThirst < thirstPsychThreshold)
        {
            sanityDecreaseMultiplier += thirstSanityFactor; // Apply thirst penalty
        }

        // Calculate final rate for display and apply sanity decrease
        finalSanityDepletionRate = baseSanityDepletionRate * sanityDecreaseMultiplier;

        if (sanityDecreaseMultiplier > 0)
        {
            currentSanity -= finalSanityDepletionRate * Time.deltaTime;
            currentSanity = Mathf.Max(0, currentSanity); // Ensure sanity doesn't drop below 0
        }
    }
// END sanity handling
}
