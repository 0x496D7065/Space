using UnityEngine;
using TMPro;

public class AzimuthController : MonoBehaviour
{
    [Header("References")]
    public DialDigit digitHundreds;
    public DialDigit digitTens;
    public DialDigit digitUnits;

    [SerializeField] private RadarShoot radarShoot;



    private void Start()
    {
        digitHundreds.OnValueChanged += UpdateDigitConstraints;
        digitTens.OnValueChanged += UpdateDigitConstraints;
        digitUnits.OnValueChanged += UpdateDigitConstraints;

        UpdateDigitConstraints();
    }

    private void UpdateDigitConstraints()
    {
        // Hundreds can only be 0–3
        digitHundreds.SetConstraints(0, 3);

        // If hundreds == 3, tens must be 0–5, else 0–9
        if (digitHundreds.value == 3)
        {
            digitTens.SetConstraints(0, 5);
        }
        else
        {
            digitTens.SetConstraints(0, 9);
        }

        // Units always 0–9
        digitUnits.SetConstraints(0, 9);

        // Combine digits
        int azimuth = digitHundreds.value * 100 + digitTens.value * 10 + digitUnits.value;
        radarShoot.azimuth = azimuth;
    }

    public int GetAzimuth()
    {
        return digitHundreds.value * 100 + digitTens.value * 10 + digitUnits.value;
    }
}
