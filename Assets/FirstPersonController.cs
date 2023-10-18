using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Player movement and action state checks
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !IsSliding;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && characterController.isGrounded;


    // Options to enable or disable functionalities
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;

    // Key bindings for controls
    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;

    // Movement speed settings
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 6f;

    // Mouse look sensitivity and constraints
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    // Jumping settings
    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    // Crouching settings and state flags
    [Header("Crouch Parameters")]
    [SerializeField] private bool enableCrouchToggle = true;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;
    private Coroutine crouchRoutine;

    // Headbob settings
    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.10f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYpos = 0;
    private float timer;

    // Zoom settings
    [Header("Zoom Parameters")]
    [SerializeField] private bool enableZoomToggle = false;
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;


    // SLIDING PARAMETERS

    private Vector3 hitPointNormal;

    // Check if player is sliding on a slope
    private bool IsSliding
    {
        get
        {   
            // If grounded and on a slope
            if (characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                // True if slope angle exceeds the limit
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    // References to essential components
    private Camera playerCamera;
    private CharacterController characterController;

    // Movement direction and input
    private Vector3 moveDirection;
    private Vector2 currentInput;

    // Current rotation in the X-axis (for looking up and down)
    private float rotationX = 0;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYpos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
            {
                HandleJump();
            }

            if (canCrouch)
            {
                HandleCrouch();
            }

            if (canUseHeadbob)
            {
                HandleHeadbob();
            }

            if (canZoom)
            {
                HandleZoom();
            }

            ApplyFinalMovements();
        }
    }

    // Calculate player movement based on input
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        // Calculates the move directions and makes sure vertical movement isn't affected
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);

        // Makes sure the player doesn't move faster diagonally
        float currentSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, currentSpeed);

        moveDirection.y = moveDirectionY;

    }

    // Handle mouse look based on input
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    // Handles jumping
    private void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    // Handles crouching
    private void HandleCrouch()
    {
        // If crouch toggle is enabled
        if (enableCrouchToggle)
        {
            // On pressing the crouch key
            if (Input.GetKeyDown(crouchKey))
            {
                // If currently standing, crouch. Otherwise, stand up
                if (!isCrouching)
                {
                    StartCrouch(true);
                }
                else
                {
                    StartCrouch(false);
                }
            }
        }
        else // If hold-to-crouch is enabled
        {
            // Crouch on key press
            if (Input.GetKeyDown(crouchKey))
            {
                StartCrouch(true);
            }
            // Stand up on key release
            if (Input.GetKeyUp(crouchKey))
            {
                StartCrouch(false);
            }
        }
    }

    // Starts the crouch transition, stopping any ongoing crouch coroutine
    private void StartCrouch(bool isCrouching)
    {
        // Stop any ongoing crouch routines
        if (crouchRoutine != null)
        {
            StopCoroutine(crouchRoutine);
            crouchRoutine = null;
        }
        // Start the crouch coroutine
        crouchRoutine = StartCoroutine(ToggleCrouch(isCrouching));
    }

    // Handles headbobbing
    private void HandleHeadbob()
    {
        // Skip headbobbing if player is airborne
        if (!characterController.isGrounded) return;

        // Apply headbob only when player is moving
        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            // Adjust headbob speed based on movement state(crouching, sprinting, walking)
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            // Set camera position for headbob effect
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYpos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    // Handles zooming in and out
    private void HandleZoom()
    {
        // If zoom toggle is enabled
        if (enableZoomToggle)
        {
            // On pressing the zoom key
            if (Input.GetKeyDown(zoomKey))
            {   
                // If currently at default FOV, zoom in. Otherwise, zoom out
                if (playerCamera.fieldOfView == defaultFOV)
                {
                    StartZoom(true);
                }
                else
                {
                    StartZoom(false);
                }
            }
        }
        else // If hold-to-zoom is enabled
        {
            // Zoom in on key press
            if (Input.GetKeyDown(zoomKey))
            {
                StartZoom(true);
            }
            // Zoom out on key release
            if (Input.GetKeyUp(zoomKey))
            {
                StartZoom(false);
            }
        }
    }

    // Initiates the zoom coroutine, ensuring any previous zoom routine is stopped
    private void StartZoom(bool isEnter)
    {
        // If a zoom routine is already running, stop it
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }
        // Start the zoom coroutine
        zoomRoutine = StartCoroutine(ToggleZoom(isEnter));
    }

    // Apply movement and gravity
    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        if(WillSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    // Coroutine for transitioning between crouching and standing
    private IEnumerator ToggleCrouch(bool isCrouching)
    {
        // Set target values based on crouching/standing
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        Vector3 targetCenter = isCrouching ? crouchingCenter : standingCenter;

        // Calculate change rates
        float heightChangeRate = Mathf.Abs(standingHeight - crouchHeight) / timeToCrouch;
        float centerChangeRate = (standingCenter - crouchingCenter).magnitude / timeToCrouch;

        // Adjust height and center to target values
        while (!Mathf.Approximately(characterController.height, targetHeight) ||
               !Vector3Approximately(characterController.center, targetCenter, 0.01f))
        {
            characterController.height = Mathf.MoveTowards(characterController.height, targetHeight, heightChangeRate * Time.deltaTime);
            characterController.center = Vector3.MoveTowards(characterController.center, targetCenter, centerChangeRate * Time.deltaTime);
            yield return null;
        }

        // Update crouching state and reset coroutine reference
        this.isCrouching = isCrouching;
        crouchRoutine = null;
    }

    // Checks if two Vector3s are close based on a tolerance
    private bool Vector3Approximately(Vector3 a, Vector3 b, float tolerance)
    {
        return Mathf.Abs(a.x - b.x) < tolerance &&
               Mathf.Abs(a.y - b.y) < tolerance &&
               Mathf.Abs(a.z - b.z) < tolerance;
    }

    // Coroutine for zooming the camera's field of view
    private IEnumerator ToggleZoom(bool isEnter)
    {
        // Set target FOV based on zoom direction
        float targetFOV = isEnter ? zoomFOV : defaultFOV;

        // Adjust FOV until target is reached
        while (!Mathf.Approximately(playerCamera.fieldOfView, targetFOV))
        {
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, targetFOV, (Mathf.Abs(defaultFOV - zoomFOV) / timeToZoom) * Time.deltaTime);
            yield return null;
        }

        // Reset coroutine after zoom
        zoomRoutine = null;
    }
}