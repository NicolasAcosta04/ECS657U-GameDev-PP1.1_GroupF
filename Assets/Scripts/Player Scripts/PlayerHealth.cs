
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Survival Bars")]
    public UnityEngine.UI.Slider staminaBar;
    public UnityEngine.UI.Slider thirstBar;
    public UnityEngine.UI.Slider hungerBar;
    public UnityEngine.UI.Slider healthBar;
    private int incrementAmount = 1;

    [Header("Health States")]
    public HealthState state;
    public enum HealthState
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

    // Start is called before the first frame update
    void Start()
    {
        thirstBar.value = 8;
        hungerBar.value = 8;
        healthBar.value = 1;

    }

    // Update is called once per frame
    void Update()
    {
        HealthRegeneration();
        ActionHandler();
    }

    private void ActionHandler()
    {
        // Survival Timers
        hungerTimer += Time.deltaTime;
        thirstTimer += Time.deltaTime;

        // Mode - Eating
        if (Input.GetKey(eatKey) && state != HealthState.Drinking)
        {
            state = HealthState.Eating;
            HungerRestore();
        }

        // Mode - Drinking
        else if (Input.GetKey(drinkKey) && state != HealthState.Eating)
        {
            state = HealthState.Drinking;
            ThirstRestore();
        }

        // Mode - Resting
        else
        {
            state = HealthState.Resting;
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
        if (thirstBar.value == thirstBar.maxValue && hungerBar.value == thirstBar.maxValue)
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
    }

    private void IncreaseHungerValue()
    {
        hungerBar.value += incrementAmount;
    }
    private void IncreaseThirstValue()
    {
        thirstBar.value += incrementAmount;
    }
    private void IncreaseHealthValue()
    {
        healthBar.value += incrementAmount;
    }
}