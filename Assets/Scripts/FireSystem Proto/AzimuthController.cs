using UnityEngine;
using Mirror;
using TMPro;

public class AzimuthController : NetworkBehaviour
{
    [Header("References")]
    public DialDigit digitHundreds;
    public DialDigit digitTens;
    public DialDigit digitUnits;

    [SerializeField] private RadarShoot radarShoot;

    [SyncVar(hook = nameof(OnHundredsChanged))] public int hundreds;
    [SyncVar(hook = nameof(OnTensChanged))] public int tens;
    [SyncVar(hook = nameof(OnUnitsChanged))] public int units;

    //private void Start()
    //{
    //    digitHundreds.OnValueChanged += UpdateDigitConstraints;
    //    digitTens.OnValueChanged += UpdateDigitConstraints;
    //    digitUnits.OnValueChanged += UpdateDigitConstraints;

    //    UpdateDigitConstraints();
    //}

    #region SyncVar Hooks
    void OnHundredsChanged(int oldVal, int newVal)
    {
        digitHundreds.SetValue(newVal);
        UpdateAzimuth();
    }

    void OnTensChanged(int oldVal, int newVal)
    {
        digitTens.SetValue(newVal);
        UpdateAzimuth();
    }

    void OnUnitsChanged(int oldVal, int newVal)
    {
        digitUnits.SetValue(newVal);
        UpdateAzimuth();
    }
    #endregion

    //private void UpdateDigitConstraints()
    //{
    //    // Hundreds can only be 0–3
    //    digitHundreds.SetConstraints(0, 3);

    //    // If hundreds == 3, tens must be 0–5, else 0–9
    //    if (digitHundreds.value == 3)
    //    {
    //        digitTens.SetConstraints(0, 5);
    //    }
    //    else
    //    {
    //        digitTens.SetConstraints(0, 9);
    //    }
        
    //    // Units always 0–9
    //    digitUnits.SetConstraints(0, 9);

    //    // Combine digits
    //    int azimuth = digitHundreds.value * 100 + digitTens.value * 10 + digitUnits.value;
    //    if (isServer)
    //        radarShoot.azimuth = azimuth;
    //}

    [Command(requiresAuthority = false)]
    public void CmdChangeDigit(DialDigitType type, int delta)
    {
        switch (type)
        {
            case DialDigitType.Hundreds:
                hundreds = Mathf.Clamp(hundreds + delta, 0, 3);
                break;

            case DialDigitType.Tens:
                int maxTens = (hundreds == 3) ? 5 : 9;
                tens = Mathf.Clamp(tens + delta, 0, maxTens);
                break;

            case DialDigitType.Units:
                units = Mathf.Clamp(units + delta, 0, 9);
                break;
        }
        if (hundreds == 3 && tens > 5)
            tens = 5;
    }

    public int GetAzimuth()
    {
        return hundreds * 100 + tens * 10 + units;
    }
    private void UpdateAzimuth()
    {
        radarShoot.azimuth = GetAzimuth();
    }
}
