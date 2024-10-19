
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusBarScript : MonoBehaviour
{
    public UnityEngine.UI.Slider thirstBar;
    public UnityEngine.UI.Slider hungerBar;
    public UnityEngine.UI.Slider healthBar;
    
    bool firstInstanceHealth = true;
    private float timer = 0;
    private int incrementAmount = 1;

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

        if (thirstBar.value == thirstBar.maxValue && hungerBar.value == thirstBar.maxValue)
        {
            if (firstInstanceHealth == true)
            {
                IncreaseHealthValue();
                firstInstanceHealth = false;
            }
            print("Full");
            timer += Time.deltaTime;
            print(timer);
            if (timer >= 10)
            {
                IncreaseHealthValue();
                timer = 0;
            }
        }

    }
    public void IncreaseThirstValue()
    {
        thirstBar.value = thirstBar.value + incrementAmount;
    }
    public void IncreaseHungerValue()
    {
        hungerBar.value = hungerBar.value + incrementAmount;
    }
    public void IncreaseHealthValue()
    {
        healthBar.value = healthBar.value + incrementAmount;
    }
}