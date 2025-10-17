using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform playerCamera;

    [Header("Pitch Settings")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private Vector2 lookInput = Vector2.zero;
    private float pitch = 0f;

    void Start()
    {
        LockCursor(true);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        // Apply sensitivity
        float mouseX = lookInput.x * 0.5f;
        float mouseY = lookInput.y * 0.5f;

        // Horizontal rotation
        playerBody.Rotate(Vector3.up * mouseX);

        // Vertical rotation
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
