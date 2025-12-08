using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;

public class PlayerController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController charController;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioStepClips;
    [SerializeField] private AudioClip audioGrabClips;
    [SerializeField] private AudioClip audioDropClips;
    [SerializeField] private AudioClip audioTorchClips;

    [Header("Camera Reference")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform playerCamera;

    [Header("Pitch Settings")]
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

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
    private PickableObject heldObject;
    private Camera playerCam;
    private AudioListener listener;

    private PlayerInput input;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    private Vector2 lookInput = Vector2.zero;
    private float pitch = 0f;

    private Vector3 lastPosition;
    private float tempTime;

    private const float STEP_COOLDOWN = 0.5f;
    private const float STEP_DISTANCE = 1.5f;

    private void Awake()
    {
        charController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        playerCam = GetComponentInChildren<Camera>();
        input = GetComponent<PlayerInput>();
        listener = playerCam.GetComponent<AudioListener>();
        audioSource = GetComponent<AudioSource>();
        lastPosition = this.transform.position;

        moveAction = input.actions["Move"];
        sprintAction = input.actions["Sprint"];
        jumpAction = input.actions["Jump"];
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log($"[PlayerController] Local player started: {netId}, isLocalPlayer={isLocalPlayer}");

        EnableLocalPlayer();
        StartCoroutine(AssignUIManagerWhenReady());
    }

    private IEnumerator AssignUIManagerWhenReady()
    {
        UIManager uiManager = null;
        while (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            yield return null;
        }

        uiManager.playerInput = GetComponent<PlayerInput>();
    }

    private void EnableLocalPlayer()
    {
        LockCursor(true);
        if (playerCam != null)
            playerCam.enabled = true;
        if (input != null)
            input.enabled = true;
        if (listener != null)
            listener.enabled = true;
    }
    public void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void Update()
    {
        if (Vector3.Distance(this.transform.position, lastPosition) > STEP_DISTANCE)
        {
            lastPosition = this.transform.position;

            PlayStepSound();
        }

        if (!isLocalPlayer) 
        {
            return; 
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();


        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        float speed = isSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

        /*if (moveDirection != Vector3.zero)
            PlayStepSound();*/

        charController.Move(speed * Time.deltaTime * moveDirection);
        //rb.MovePosition(rb.position + speed * Time.deltaTime * moveDirection);

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
        //rb.linearVelocity = velocity;

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

    private void PlayStepSound()
    {
        //if (tempTime > Time.time)
        //return;

        //tempTime = Time.time + STEP_COOLDOWN;
        PlaySoundToPlayer(audioStepClips[UnityEngine.Random.Range(0, audioStepClips.Count)]);
    }

    public void PlaySoundToPlayer(AudioClip clip)
    {
        //audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private bool IsGrounded()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, groundDistance, groundMask))
            return true;
        else
            return false;
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) { return; }
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnGrab(InputAction.CallbackContext ctx)
    {
        if (!isLocalPlayer) { return; }
        if (!ctx.performed) return;
        if (heldObject != null) return;

        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit Hit, grabDistance, grabMask))
        {
            if (Hit.collider.TryGetComponent(out PickableObject pickable))
            {
                heldObject = pickable;
                StartCoroutine(WaitForAuthorityAndAttach(pickable));
            }
        }
    }

    public void OnDrop(InputAction.CallbackContext ctx)
    {
        if (!isLocalPlayer) { return; }
        if (!ctx.performed) return;

        StartCoroutine(WaitForAuthorityAndDrop(heldObject));
    }

    public void InteractClick(InputAction.CallbackContext ctx)
    {
        if (!isLocalPlayer) { return; }
        //if (!ctx.performed) { return; }
        //Debug.Log("Tried interact");
        if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit Hit, interactDistance, interactMask))
        {
            if (Hit.collider.TryGetComponent(out Interactable component) && ctx.performed)
                component.Interact();
            Debug.Log("Instant Interact");
        }
        else if (heldObject != null)
        {
            Debug.Log("HeldObject not null");
            if (heldObject.TryGetComponent(out Interactable component))
            {
                //Debug.Log("Hold Interact");
                if (ctx.started || ctx.canceled)
                    component.Interact();
            }
        }
    }
    [Command]
    void CmdAssignAuthority(NetworkIdentity obj)
    {
        if (obj.connectionToClient != null)
            obj.RemoveClientAuthority();

        obj.AssignClientAuthority(connectionToClient);
    }

    [Command]
    void CmdRemoveAuthority(NetworkIdentity obj)
    {
        obj.RemoveClientAuthority();
    }

    private IEnumerator WaitForAuthorityAndAttach(PickableObject pickable)
    {
        CmdAssignAuthority(pickable.netIdentity);

        float timeout = 2f; // max wait 2 seconds
        float elapsed = 0f;

        while (!pickable.isOwned && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!pickable.isOwned)
        {
            Debug.LogWarning("Failed to gain authority over pickable.");
            heldObject = null;
            yield break;
        }

        pickable.transform.SetParent(grabPoint);
        pickable.transform.localPosition = Vector3.zero;
        pickable.transform.localRotation = Quaternion.identity;
    }
    private IEnumerator WaitForAuthorityAndDrop(PickableObject pickable)
    {
        pickable.transform.SetParent(null);
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        heldObject = null;

        //CmdRemoveAuthority(pickable.netIdentity);
        yield return null;
    }
}

