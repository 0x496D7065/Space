using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{    
    [Header("References")]
        
    [SerializeField] private PlayerInput         playerInput = null;
    [SerializeField] private Transform           playerBody = null;
    [SerializeField] private CharacterController controller = null;


    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float maxAirControlAmount = 90f;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;

    [Header("Grab Settings")]
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabDistance = 3f;
    [SerializeField] private LayerMask grabMask;

    private Vector3 inputMovement;
    private Vector3 velocity;
    private float currentMoveSpeed;
    private bool keepMomentum = false;
    private Vector3 airMomentum = Vector3.zero;

    private float groundDistance = 0.4f;
    private bool isGrounded;
    private Rigidbody heldObject;
    private Camera playerCam;

    public PlayerInput PlayerInput => playerInput;

    private void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
    }
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0 && !keepMomentum)
        {
            velocity.y = -2f; // Small push down to keep grounded
        }
        if (isGrounded && velocity.y <= 0 && keepMomentum)
        {
            keepMomentum = false;
            airMomentum = Vector3.zero;
        }
        float usedSpeed = keepMomentum ? currentMoveSpeed : movementSpeed;
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Calculate final movement
        Vector3 inputDir = playerBody.right * inputMovement.x + playerBody.forward * inputMovement.z;
        inputDir.y = 0f;
        inputDir.Normalize();

        Vector3 move;
        if (keepMomentum)
        {
            // While airborne
            if (inputDir.magnitude > 0f)
            {
                // Adjust air momentum gradually toward input direction
                airMomentum = Vector3.RotateTowards(
                airMomentum,
                inputDir,
                maxAirControlAmount * Mathf.Deg2Rad * Time.deltaTime,
                float.MaxValue
                );
            }

            move = airMomentum * currentMoveSpeed;
        }
        else
        {
            // Grounded movement, use input directly
            move = inputDir * movementSpeed;
        }


        // Combine horizontal movement and vertical velocity
        Vector3 finalMovement = (move * usedSpeed + new Vector3(0, velocity.y, 0)) * Time.deltaTime;

        controller.Move(finalMovement);
    }
        
    public void OnMove(InputAction.CallbackContext ctx)
    {  
        Vector2 inputValue = ctx.ReadValue<Vector2>();
            
        inputMovement = new Vector3(inputValue.x, 0f, inputValue.y);
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        Debug.Log("jump");
        if (!ctx.performed) { return; }
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            currentMoveSpeed = movementSpeed;
            keepMomentum = true;
        }
    }

    public void OnSprint(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
        {
            movementSpeed = 2.5f;
        }
        else 
            movementSpeed = 3.5f;
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
}

