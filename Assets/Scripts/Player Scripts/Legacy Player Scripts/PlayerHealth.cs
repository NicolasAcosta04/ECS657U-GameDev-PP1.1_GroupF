using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    public KeyCode eatKey = KeyCode.Alpha1;
    public KeyCode drinkKey = KeyCode.Alpha2;

    [Header("Hunger Restore")]
    public TextMeshProUGUI eatingCD;
    public TextMeshProUGUI foodItem;
    public int foodItemValue = 3;
    public int foodQuantity = 3;
    public float hungerDelay = 0.5f;
    private bool firstInstanceHunger = true;
    private float hungerTimer = 0;

    [Header("Thirst Restore")]
    public TextMeshProUGUI drinkingCD;
    public TextMeshProUGUI waterItem;
    public int waterItemValue = 5;
    public int waterQuantity = 3;
    public float thirstDelay = 0.5f;
    private bool firstInstanceThirst = true;
    private float thirstTimer = 0;

    [Header("Health Regeneration")]
    public int healthDelay = 3;
    private bool firstInstanceHealth = true;
    private float healthTimer = 0;

    [Header("Starving")]
    public int starvingConsumptionTime = 18;
    private float starvingTimer = 0;

    [Header("Thirsting")]
    public int thirstingConsumptionTime = 15;
    private float thirstingTimer = 0;

    [Header("The Hungry State")]
    public int hungryConsumptionTime = 4;
    private float hungryTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        thirstBar.value = 8;
        hungerBar.value = 6;
        healthBar.value = 10;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        StateHandler();
        SurvivalMechanics();
        UI();
    }

    private void StateHandler()
    {
        // Survival Timers
        hungerTimer += Time.deltaTime;
        thirstTimer += Time.deltaTime;

        // Mode - Eating
        if (Input.GetKey(eatKey) && state != HealthState.Drinking && foodQuantity > 0)
        {
            state = HealthState.Eating;
            HungerRestore();
        }

        // Mode - Drinking
        else if (Input.GetKey(drinkKey) && state != HealthState.Eating && waterQuantity > 0)
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

    private void SurvivalMechanics()
    {
        // Hungry - Loses health over time when hunger bar is at 0
        if (hungerBar.value == 0)
        {
            hungryTimer += Time.deltaTime;
            if (hungryTimer >= hungryConsumptionTime)
            {
                DecreaseHealthValue();
                hungryTimer = 0;
            }
        }

        // Starving - Loses hunger over time during the game
        starvingTimer += Time.deltaTime;
        if (starvingTimer >= starvingConsumptionTime)
        {
            DecreaseHungerValue();
            starvingTimer = 0;
        }

        // Thirsting - Loses thirst over time during the game
        thirstingTimer += Time.deltaTime;
        if (thirstingTimer >= thirstingConsumptionTime)
        {
            DecreaseThirstValue();
            thirstingTimer = 0;
        }

        // Health Regeneration - Gains health over time when the hunger bar & the thirst bar is at thier max
        if (hungerBar.value == hungerBar.maxValue && thirstBar.value == thirstBar.maxValue)
        {
            HealthRegeneration();
        }

        // Dead - Game over when the health bar is at 0
        if (healthBar.value == 0)
        {
            SceneManager.LoadScene("Game Over Screen");
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
            DecreaseFoodValue();
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
            DecreaseWaterValue();
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

    void OnCollisionEnter(Collision colissionInfo)
    {
        if (colissionInfo.collider.name == "FoodObj(Clone)")
        {
            Destroy(colissionInfo.collider);
            for (int i = 0; i < foodItemValue; i++)
            {
                IncreaseFoodValue();
            }
        }

        if (colissionInfo.collider.name == "WaterObj(Clone)")
        {
            Destroy(colissionInfo.collider);
            for (int i = 0; i < waterItemValue; i++)
            {
                IncreaseWaterValue();
            }
        }

        if (colissionInfo.collider.name == "EnemyObj(Clone)")
        {
            DecreaseHealthValue();
            DecreaseStaminaValue();
            DecreaseStaminaValue();
        }

        Debug.Log(colissionInfo.collider.name);

        if (colissionInfo.collider.name == "Room(Clone)")
        {
            DecreaseHealthValue();
        }
    }

    private void UI()
    {
        // Inventory
        foodItem.text = "Food: " + foodQuantity;
        waterItem.text = "Water: " + waterQuantity;

        // Eating Cooldown
        var hungerCD = hungerDelay - hungerTimer;

        if (hungerCD <= 0)
        {
            eatingCD.text = "Eat: " + "Ready";
        }
        else
        {
            eatingCD.text = "Eat: " + Math.Round(hungerCD, 2);
        }

        // Drinking Cooldown
        var thirstCD = thirstDelay - thirstTimer;

        if (thirstCD <= 0)
        {
            drinkingCD.text = "Drink: " + "Ready";
        }
        else
        {
            drinkingCD.text = "Drink: " + Math.Round(thirstCD, 2);
        }
    }

    private void IncreaseFoodValue()
    {
        foodQuantity += incrementAmount;
    }
    private void IncreaseWaterValue()
    {
        waterQuantity += incrementAmount;
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
    private void DecreaseThirstValue()
    {
        thirstBar.value -= incrementAmount;
    }
    private void IncreaseHealthValue()
    {
        healthBar.value += incrementAmount;
    }
    private void DecreaseHealthValue()
    {
        healthBar.value -= incrementAmount;
    }
    private void DecreaseStaminaValue()
    {
        staminaBar.value -= incrementAmount;
    }
    private void DecreaseFoodValue()
    {
        foodQuantity -= incrementAmount;
    }
    private void DecreaseWaterValue()
    {
        waterQuantity -= incrementAmount;
    }
}