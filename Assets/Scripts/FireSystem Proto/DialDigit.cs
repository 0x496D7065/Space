using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialDigit : MonoBehaviour
{
    [SerializeField] private TMP_Text digitText;

    [HideInInspector] public int value = 0;

    //private int maxValue = 9;
    //private int minValue = 0;

    //public Action OnValueChanged;

    private void Start()
    {
        SetValue(value);
    }

    public void SetValue(int newValue)
    {
        value = newValue;
        digitText.text = value.ToString();
    }

    //public void SetConstraints(int min, int max)
    //{
    //    minValue = min;
    //    maxValue = max;

    //    // Clamp current value in case constraints changed
    //    value = Mathf.Clamp(value, minValue, maxValue);
    //    UpdateDisplay();
    //}

    //public void Increment()
    //{
    //    value++;
    //    if (value > maxValue)
    //        value = minValue;

    //    UpdateDisplay();
    //    OnValueChanged?.Invoke();
    //}

    //public void Decrement()
    //{
    //    value--;
    //    if (value < minValue)
    //        value = maxValue;

    //    UpdateDisplay();
    //    OnValueChanged?.Invoke();
    //}
    //private void UpdateDisplay()
    //{
    //    digitText.text = value.ToString();
    //}

    //public void SetValue(int newValue)
    //{
    //    value = Mathf.Clamp(newValue, minValue, maxValue);
    //    UpdateDisplay();
    //}
}
