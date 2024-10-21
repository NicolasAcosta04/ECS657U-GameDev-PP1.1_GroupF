
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Survival Bars")]
    public UnityEngine.UI.Slider staminaBar;
    public UnityEngine.UI.Slider thirstBar;
    public UnityEngine.UI.Slider hungerBar;
    public UnityEngine.UI.Slider healthBar;
    private int incrementAmount = 1;

    [Header("Action States")]
    public ActionState stateA;
    public enum ActionState
    {
        Resting,
        Eating,
        Drinking
    }
    public KeyCode eatKey = KeyCode.Alpha2;
    public KeyCode drinkKey = KeyCode.Alpha3;

    [Header("Hunger Restore")]
    public int hungerDelay = 3;
    private bool firstInstanceHunger = true;
    private float hungerTimer = 0;

    [Header("Thirst Restore")]
    public int thirstDelay = 2;
    private bool firstInstanceThirst = true;
    private float thirstTimer = 0;

    [Header("Health Regeneration")]
    public int healthDelay = 6;
    private bool firstInstanceHealth = true;
    private float healthTimer = 0;

    [Header("Health States")]
    public HealthState stateB;
    public enum HealthState
    {
        Healthy,
        Thirsty,
        Hungry,
        Dead
    }

    [Header("Starving")]
    public int starvingConsumptionTime = 10;
    private float starvingTimer = 0;

    [Header("Hungry State")]
    public int hungryConsumptionTime = 4;
    private float hungryTimer = 0;

    // [Header("Thirsty State")]
    // public int thirstyConsumptionTime = 4;
    // private float thirstyTimer = 0;
    // public bool stopStaminaRegeneration = false;

    // Start is called before the first frame update
    void Start()
    {
        thirstBar.value = 8;
        hungerBar.value = 0;
        healthBar.value = 4;

    }

    // Update is called once per frame
    void Update()
    {
        ActionStateHandler();
        HealthStateHandler();
    }

    private void ActionStateHandler()
    {
        // Survival Timers
        hungerTimer += Time.deltaTime;
        thirstTimer += Time.deltaTime;

        // Mode - Eating
        if (Input.GetKey(eatKey) && stateA != ActionState.Drinking)
        {
            stateA = ActionState.Eating;
            HungerRestore();
        }

        // Mode - Drinking
        else if (Input.GetKey(drinkKey) && stateA != ActionState.Eating)
        {
            stateA = ActionState.Drinking;
            ThirstRestore();
        }

        // Mode - Resting
        else
        {
            stateA = ActionState.Resting;
        }

        if (thirstBar.value == thirstBar.maxValue && hungerBar.value == thirstBar.maxValue)
        {
            HealthRegeneration();
        }
    }

    private void HealthStateHandler()
    {
        // Mode - Hungry
        if (hungerBar.value == 0)
        {
            stateB = HealthState.Hungry;
            hungryTimer += Time.deltaTime;
            if (hungryTimer >= hungryConsumptionTime)
            {
                DecreaseHealthValue();
                hungryTimer = 0;
            }
        }

        // Mode - Thirsty
        // else if (thirstBar.value == 0)
        // {
        //     stateB = HealthState.Thirsty;
        //     thirstyTimer += Time.deltaTime;
        //     if (thirstyTimer >= thirstyConsumptionTime)
        //     {
        //         stopStaminaRegeneration = true;
        //         thirstyTimer = 0;
        //     }
        // }

        // Mode - Hungry
        if (healthBar.value == 0)
        {
            stateB = HealthState.Dead;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("Game Over");
        }

        // // Mode - Healthy
        // else
        // {
        //     state = HealthState.Healthy;
        // }

        // Health Regeneration
        if (thirstBar.value == thirstBar.maxValue && hungerBar.value == thirstBar.maxValue)
        {
            HealthRegeneration();
        }

        starvingTimer += Time.deltaTime;
        // Starving 
        if (starvingTimer >= starvingConsumptionTime)
        {
            DecreaseHungerValue();
            starvingTimer = 0;
        }
    }

    private void HungerRestore()
    {
        // Eaten in the last 4 seconds?

        // NO
        if (firstInstanceHunger == true)
        {
            firstInstanceHunger = false;
            IncreaseHungerValue();
            hungerTimer = 0;
        }

        // YES
        if (hungerTimer >= hungerDelay)
        {
            firstInstanceHunger = true;
        }
    }

    private void ThirstRestore()
    {
        // Had water in the last 2 seconds?

        // NO
        if (firstInstanceThirst == true)
        {
            firstInstanceThirst = false;
            IncreaseThirstValue();
            thirstTimer = 0;
        }

        // YES
        if (thirstTimer >= thirstDelay)
        {
            firstInstanceThirst = true;
        }
    }

    private void HealthRegeneration()
    {
        // Initial Health Restore
        if (firstInstanceHealth == true)
        {
            IncreaseHealthValue();
            firstInstanceHealth = false;
        }

        healthTimer += Time.deltaTime;

        // Health Regeneration Delay
        if (healthTimer >= healthDelay)
        {
            IncreaseHealthValue();
            healthTimer = 0;
        }
    }

    private void IncreaseHungerValue()
    {
        hungerBar.value += incrementAmount;
    }
    private void DecreaseHungerValue()
    {
        hungerBar.value -= incrementAmount;
    }
    private void IncreaseThirstValue()
    {
        thirstBar.value += incrementAmount;
    }
    private void IncreaseHealthValue()
    {
        healthBar.value += incrementAmount;
    }
    private void DecreaseHealthValue()
    {
        healthBar.value -= incrementAmount;
    }
}