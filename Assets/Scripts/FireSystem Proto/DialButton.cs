using UnityEngine;

public enum DialDigitType
{
    Hundreds,
    Tens,
    Units,
}

public enum DialDirection
{
    Up,
    Down
}

public class DialButton : Interactable
{
    [Header("Dial Button Settings")]
    [SerializeField] private DialDigitType dialDigitType;
    [SerializeField] private DialDirection dialDirection;

    [Header("Reference to Azimuth Controller")]
    [SerializeField] private AzimuthController azimuthController;

    public override void Interact()
    {
        switch (dialDigitType)
        {
            case DialDigitType.Hundreds:
                UpdateDigit(azimuthController.digitHundreds);
                break;
            case DialDigitType.Tens:
                UpdateDigit(azimuthController.digitTens);
                break;
            case DialDigitType.Units:
                UpdateDigit(azimuthController.digitUnits);
                break;
        }
    }

    private void UpdateDigit(DialDigit dialDigit)
    {
        if (dialDirection == DialDirection.Up)
            dialDigit.Increment();
        else
            dialDigit.Decrement();
    }
}
