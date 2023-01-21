using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NexpControl : MonoBehaviour
{
    public Slider slider; 
    public void SetExp(int exp)
    {
        slider.value = exp;
    }
    public void SetMaxExp(int Level , int exp )
    {
        if (Level == 1 )
        {
            slider.maxValue = 500;
        }
    }
    public void SetExp(float exp)
    {
        slider.value = exp;
    }
    public void SetMaxExp( float exp)
    {
        slider.maxValue = exp;
        
    }

}
