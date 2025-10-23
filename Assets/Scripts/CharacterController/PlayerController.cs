using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{    
    [Header("References")]
    [SerializeField] private CharacterController charController;


    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;

    [Header("Grab Settings")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private LayerMask grabMask;

    [Header("Interact Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactMask;

    private readonly float groundDistance = 0.4f;
    private Vector3 velocity;
    private Rigidbody heldObject;
    private Camera playerCam;
    
    private PlayerInput input;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        playerCam = GetComponentInChildren<Camera>();
        input = GetComponent<PlayerInput>();
        moveAction = input.actions["Move"];
        sprintAction = input.actions["Sprint"];
        jumpAction = input.actions["Jump"];
    }

    private void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();


        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        charController.Move(speed * Time.deltaTime * moveDirection);

        if (IsGrounded())
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            if (jumpAction.triggered)
                velocity.y = jumpForce;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        charController.Move(velocity * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask))
            return true;
        else
            return false;
    }

    public void OnGrab(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (heldObject != null) return;

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit Hit, grabDistance, grabMask))
        {
            if (Hit.rigidbody != null)
            {
                heldObject = Hit.rigidbody;
                heldObject.isKinematic = true;
                heldObject.transform.SetParent(grabPoint);
                heldObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
    }

    public void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;
        heldObject = null;
    }

    public void InteractClick(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (heldObject != null) return;

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit Hit, interactDistance, interactMask))
        {
            Interactable interactable = Hit.collider.GetComponent<Interactable>();
            if (interactable != null)
                interactable.Interact();
        }
    }
}

