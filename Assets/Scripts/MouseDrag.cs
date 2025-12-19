using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseSlide : Interactable
{
    [Header("Slide Settings")]
    [SerializeField] private Transform trayAnchor;
    [SerializeField] private Vector3 localSlideAxis = Vector3.forward;
    [SerializeField] private float slideSpeed = 1f;
    [SerializeField] private float slideLerpSpeed = 10f;
    [SerializeField] private float minSlide = 0f;
    [SerializeField] private float maxSlide = 0.5f;

    private bool isSliding = false;
    private float currentSlideAmount = 0f;
    private float targetSlide = 0f;
    private Vector3 initialPosition;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //initialPosition = transform.localPosition;
        initialPosition = transform.position - trayAnchor.position;
    }

    public override void Interact(InputAction.CallbackContext ctx)
    {
        //base.Interact(ctx);

        if (ctx.started)
        {
            isSliding = true;
            Debug.Log($"isSliding:{isSliding}");
            PlayerController.localPlayer.CmdAssignAuthority(netIdentity);
        }
        else if (ctx.canceled)
        {
            isSliding = false;
            Debug.Log($"isSliding:{isSliding}");
        }
    }

    private void FixedUpdate()
    {
        //Debug.Log($"isDragging:{isSliding}, authority:{authority}, isClient:{isClient}");
        if (!authority || !isClient) return;

        currentSlideAmount = Mathf.Lerp(currentSlideAmount, targetSlide, slideLerpSpeed * Time.fixedDeltaTime);//this serves to know where is the object between it's minSlide
                                                                                                               //and maxSlide and lerp it toward the targetSlide
        //Vector3 localOffset = localSlideAxis.normalized * currentSlideAmount;
        //transform.localPosition = initialPosition + localOffset;
        //Vector3 worldPosition = transform.parent.TransformPoint(initialPosition + localOffset);
        Vector3 worldOffset = trayAnchor.TransformDirection(localSlideAxis.normalized) * currentSlideAmount;   //The tray was initially a child of the launcher but this was impossible
        Vector3 worldPosition = trayAnchor.position + initialPosition + worldOffset;                           //because both launcher and tray needs a networkidentity. So the tray was moved
                                                                                                               //out of the hierarchy and I added an anchor point that hold a reference of the
        rb.MovePosition(worldPosition);                                                                        //transform located on the launcher to place the tray

        if (!isSliding) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();                                                  //Find out how the mouse has moved between frames in pixels

        Vector3 slideAxisWorld = transform.TransformDirection(localSlideAxis.normalized);                      //Convert the localSlideAxis in world space to ensure
                                                                                                               //this works regardless of scene rotation

        Vector3 axisScreenDir = Camera.main.WorldToScreenPoint(transform.position + slideAxisWorld) -
                                Camera.main.WorldToScreenPoint(transform.position);                            //Project the slideAxis in 2D to find out the direction
                                                                                                               //in screen space

        axisScreenDir.z = 0f;                                                                                  // flattens to 2D
        Vector2 axisScreen2D = (Vector2)axisScreenDir.normalized;

        float projectedDelta = Vector2.Dot(mouseDelta, axisScreen2D);                                          //Project the player mouse movement onto that 2D axis
        targetSlide = Mathf.Clamp(targetSlide + projectedDelta * slideSpeed, minSlide, maxSlide);              //Update the targetSlide clamped to min/max
    }
}
