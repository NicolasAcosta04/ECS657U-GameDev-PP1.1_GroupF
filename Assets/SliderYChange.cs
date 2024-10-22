using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderYChange : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI sliderText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sliderText.text = slider.value.ToString();
    }
}
