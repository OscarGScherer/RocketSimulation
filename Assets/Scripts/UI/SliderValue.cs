using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    public enum NumberFormat
    {
        FUEL,
        ALTITUDE

    }
    public NumberFormat format = NumberFormat.FUEL;

    [SerializeField] private Slider slider;
    private TextMeshProUGUI tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        UpdateText(slider.value);
        slider.onValueChanged.AddListener((value)=>{UpdateText(value);});
    }

    void UpdateText(float value)
    {
        if(format == NumberFormat.FUEL)
        {
            int percentage = Mathf.FloorToInt(value*100f);
            if(percentage > 0)
            {
                tmp.text = percentage + "%";
                tmp.color = Color.white;
            }
            else
            {
                tmp.text = "empty";
                tmp.color = Color.red;
            }
        }
        else
        {
            int altitude = Mathf.FloorToInt(value);
            tmp.text = altitude + "m";
            tmp.color = Color.white;
        }
    }

}
