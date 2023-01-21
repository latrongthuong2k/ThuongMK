using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecondHPbar : MonoBehaviour
{
    public Slider   slider = null;
    public Gradient gradient;
    public Image    fill = null;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        fill.color      = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}