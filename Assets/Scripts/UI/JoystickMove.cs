using UnityEngine;
using System;

public class JoystickMove : MonoBehaviour
{
    public float stepLength = 0;
    public float speedAnim = 1f;
    private float timer = 0f;

    [Tooltip("Animator property which will not allowing character movement is set to true")]
    public string IsBusyProperty = "";
    public bool DisableRootMotion = false;

    [Space(10)]
    [Tooltip("Setting 'Grounded','Moving' and 'Speed' parameters for mecanim")]
    public Animator Mecanim;

    [Space(6)]
    public bool UpdateInput = true;
    [Space(1)]
    public float JumpPower = 3f;
    public float HoldShiftForSpeed = 0f;
    public float HoldCtrlForSpeed = 0f;

    public Action OnJump = null;

    bool wasInitialized = false;

    [NonSerialized] public float jumpRequest = 0f;

    Quaternion targetRotation;
    Quaternion targetInstantRotation;

    float rotationAngle = 0f;
    float sd_rotationAngle = 0f;
    float toJump = 0f;

    public bool isGrounded { get; private set; } = true;

    public float doubleTapTimeThreshold = 0.3f;

    public Joystick movementJoystick;
    public float playerSpeed;
    private Rigidbody rb;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;

        isGrounded = true;

        if (Mecanim) Mecanim.SetBool("Grounded", true);

        wasInitialized = true;
    }

    private float lastTapTime;
    private bool doubleTapDetected;

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(movementJoystick.Direction.x, 0, movementJoystick.Direction.y);
        bool moving = false;
        Transform playerTransform = transform;
        direction = mainCamera.transform.TransformDirection(direction);
        direction.y = 0;

        bool updateMovement = true;
        if (Mecanim) if (string.IsNullOrWhiteSpace(IsBusyProperty) == false) updateMovement = !Mecanim.GetBool(IsBusyProperty);
        if (UpdateInput && updateMovement)
        {
            if (direction.magnitude >= 0.1f)
            {
                playerTransform.localPosition += direction.normalized * (float)0.05 * playerSpeed * Time.deltaTime;
                playerTransform.rotation = Quaternion.LookRotation(direction.normalized);
                moving = true;
            }
            else
            {
                moving = false;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    if (Time.time - lastTapTime < doubleTapTimeThreshold)
                    {
                        // Double tap detected
                        doubleTapDetected = true;
                    }
                    lastTapTime = Time.time;
                }
            }

            if (doubleTapDetected)
            {
                if (toJump <= 0f)
                {
                    jumpRequest = JumpPower;
                    toJump = 0f;
                }
                doubleTapDetected = false; // Reset the double tap flag
            }

            if (Mecanim) Mecanim.SetBool("Moving", moving);

            if (Mecanim)
            {
                if (moving) Mecanim.SetFloat("Speed", direction.magnitude * playerSpeed);
            }
        }
    }
}