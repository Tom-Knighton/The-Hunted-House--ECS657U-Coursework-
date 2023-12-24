using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Player.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    // Player movement and action state checks
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && controls.Gameplay.Sprint.IsPressed();
    private bool ShouldJump => controls.Gameplay.Jump.triggered && characterController.isGrounded && !IsSliding;
    private bool ShouldCrouch => controls.Gameplay.Crouch.triggered && !duringCrouchAnimation && characterController.isGrounded;

    // Components
    private Attackable _attackable;
    public Inventory Inventory;

    #region Functional Options
    // Options to enable or disable functionalities
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool enableAttack = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool enableJumpingSound = true;
    [SerializeField] private bool enableLandingSound = false;
    [SerializeField] private bool enableDynamicCrosshair = true;
    #endregion

    #region Controls
    // Key bindings for controls
    private PlayerInputActions controls;
    #endregion
    
    #region Movement
    // Movement speed settings
    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;
    #endregion

    #region Looking
    // Mouse look sensitivity and constraints
    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 1.8f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 1.5f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;
    #endregion
    
    #region Stamina
    // Stamina settings
    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 20;
    [SerializeField] private float timeBeforeStaminaRegen = 2.5f;
    [SerializeField] private float staminaValueIncrement = 0.5f;
    [SerializeField] private float staminaTimeIncrement = 0.02f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    #endregion

    #region Jumping
    // Jumping settings
    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;
    #endregion
    
    #region Crouching
    // Crouching settings and state flags
    [Header("Crouch Parameters")]
    [SerializeField] private bool enableCrouchToggle = false;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;
    private bool wantsToStand = false;
    #endregion
    
    #region Attacking
    // Attack settings
    [Header("Attack Parameters")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private LayerMask attackableLayers;
    private bool canAttack = true;
    #endregion
    
    #region Headbobbing
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
    #endregion

    #region Zooming
    // Zoom settings
    [Header("Zoom Parameters")]
    [SerializeField] private bool enableZoomToggle = false;
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    [SerializeField] private float sprintFOV = 75f;
    private float defaultFOV;
    private Coroutine zoomRoutine;
    #endregion
    
    #region Footsteps
    // Footstep Settings
    [Header("FootstepParameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] concreteClips = default;
    [SerializeField] private AudioClip[] grassClips = default;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : IsSprinting && (currentStamina > 0) ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    private int[] woodIndices;
    private int[] concreteIndices;
    private int[] grassIndices;
    private int currentWoodFootstepIndex = 0;
    private int currentconcreteFootstepIndex = 0;
    private int currentGrassFootstepIndex = 0;
    private float crouchVolumeMultiplier = 0.5f;
    #endregion

    #region Jump/Land sounds
    // Jump and Landing Settings
    [Header("Jump and Landing Sound Parameters")]
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landingClip;
    private bool wasInAir = false;
    #endregion
   
    #region Sliding
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
    #endregion
    
    #region Interaction
    // Interaction settings
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    [SerializeField] private LayerMask obstructionLayerMask;
    private Interactable currentInteractable;
    #endregion
    
    #region Crosshair
    // Crosshair settings
    [Header("Crosshair settings")]
    [SerializeField] private float crosshairRestingSize = 100f;
    [SerializeField] private float crosshairMaxSize = 175f;
    [SerializeField] private float crosshairSpeed = 5f;
    private float currentSize;
    #endregion

    [SerializeField] private InventoryUI inventoryUI;

    // References to essential components
    private Camera playerCamera;
    private CharacterController characterController;

    // Movement direction and input
    private Vector3 moveDirection;
    private Vector2 currentInput;

    // Current rotation in the X-axis (for looking up and down)
    private float rotationX = 0;

    public static FirstPersonController instance;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        instance = this;
        controls = new PlayerInputActions();
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYpos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (useFootsteps)
        {
            woodIndices = GenerateRandomIndex(woodClips.Length);
            concreteIndices = GenerateRandomIndex(concreteClips.Length);
            grassIndices = GenerateRandomIndex(grassClips.Length);
        }

        _attackable = GetComponent<Attackable>();
        Inventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        // If the _attackable component is present, subscribe to its events
        if (_attackable is not null)
        {
            _attackable.OnHealthChanged.AddListener(OnHealthChanged);
            _attackable.OnDeath.AddListener(OnDeath);
        }

        UpdateUIOnRespawn();
    }

    private void OnEnable()
    {
        controls.Enable();
    }
   private void OnDisable()
    {
        controls.Disable();
    }

    // Updates the UI elements related to player status
    public void UpdateUIOnRespawn()
    {
        UIManager.Instance.UpdatePlayerHealth(_attackable.health, _attackable.maxHealth);
        UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina);
        UIManager.Instance.UpdateAttackCooldownPercentage(0);
        UIManager.Instance.UpdateCrosshairSize(35);
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

            if (useFootsteps)
            {
                HandleFootsteps();
            }

            if (canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }
            if (controls.Gameplay.Inventory.triggered)
            {
                ToggleInventory();
            }
            if (enableLandingSound)
            {
                HandleLandingSound();
            }

            if (useStamina)
            {
                HandleStamina();
            }

            if (enableAttack)
            {
                HandleAttack();
            }

            if (enableDynamicCrosshair)
            {
                HandleCrosshair();
            }

            ApplyFinalMovements();

            if (wantsToStand && !IsObstacleAbove())
            {
                StartCoroutine(CrouchStand(false)); // Stand up
                wantsToStand = false; // Reset the flag
            }
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

    // Method to check if the player is moving
    public bool IsMoving()
    {
        return (controls.Gameplay.Movement.ReadValue<Vector2>().x != 0 || controls.Gameplay.Movement.ReadValue<Vector2>().y != 0);
    }

    // Handle mouse look based on input
    private void HandleMouseLook()
    {
        rotationX -= controls.Gameplay.MouseLook.ReadValue<Vector2>().y * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, controls.Gameplay.MouseLook.ReadValue<Vector2>().x * lookSpeedX, 0);
    }

    // Handles health change notifications
    private void OnHealthChanged(float newHealth, float dmg)
    {
        // Notify UI of health change.
        UIManager.Instance.UpdatePlayerHealth(newHealth, _attackable.maxHealth);
    }

    // Handle player's death notification
    private void OnDeath()
    {
        // Notify UI of health change
        UIManager.Instance.UpdatePlayerHealth(0, _attackable.maxHealth);

        // Deactivate Player UI
        UIManager.Instance.HidePlayerUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Display Death Screen
        UIManager.Instance.ShowDefeatScreen("You were killed.");

        // Disable player interactions
        this.enabled = false;
    }

    // Handles Stamina
    private void HandleStamina()
    {
        bool isPlayerMoving = IsMoving(); // Check if the player is moving

        // Sprinting consumes stamina
        if (IsSprinting && !isCrouching && isPlayerMoving)
        {
            // If already regenerating stamina, stop it as we're using stamina
            if (regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }
            // Decrease current stamina based on use multiplier and time
            currentStamina -= staminaUseMultiplier * Time.deltaTime;
            // Ensure stamina doesn't go below zero
            if (currentStamina < 0)
            {
                currentStamina = 0;
            }
            UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina);
            if (currentStamina <= 0)
            {
                canSprint = false;
            }
            // Smoothly transition the camera's field of view to sprintFOV for sprinting effect
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, Time.deltaTime * 5f);
        }
        else
        {
            // If not sprinting, smoothly transition the camera's field of view back to default
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * 5f);
        }

        // Change the condition to start regenerating stamina
        if ((!IsSprinting || !isPlayerMoving) && currentStamina < maxStamina && regeneratingStamina == null)
        {
            // Start the stamina regeneration coroutine
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void ToggleInventory()
    {
        if (inventoryUI != null)
        {
            bool isInventoryActive = inventoryUI.gameObject.activeSelf;
            inventoryUI.gameObject.SetActive(!isInventoryActive);

            // Refresh the inventory display if we are opening the inventory
            if (!isInventoryActive)
            {
                inventoryUI.RefreshInventoryDisplay();
            }
        }
        else
        {
            Debug.LogError("InventoryUI reference not set in FirstPersonController.");
        }
    }

    // Handles jumping
    private void HandleJump()
    {
        if (ShouldJump)
        {
            if (enableJumpingSound)
            {
                jumpAudioSource.PlayOneShot(jumpClip);
            }
            moveDirection.y = jumpForce;
        }
    }

    // Handles crouching
    private void HandleCrouch()
    {
        if (enableCrouchToggle)
        {
            if (ShouldCrouch)
            {
                StartCoroutine(ToggleCrouchStand());
            }
        }
        else
        {
            // Start crouching when crouch key is pressed
            if (controls.Gameplay.Crouch.triggered && !duringCrouchAnimation && characterController.isGrounded && !isCrouching)
            {
                wantsToStand = false; // Reset this flag when crouch key is pressed
                StartCoroutine(CrouchStand(true)); // true indicates we're crouching
            }
            // Try to stand up when crouch key is released
            else if (!controls.Gameplay.Crouch.triggered && !duringCrouchAnimation && characterController.isGrounded && isCrouching)
            {
                if (!IsObstacleAbove()) // Check for obstacle
                {
                    StartCoroutine(CrouchStand(false)); // false indicates we're standing up
                }
                else
                {
                    wantsToStand = true; // Player wants to stand but is blocked
                }
            }
        }
    }

    // Checks for an obstacle above the player
    private bool IsObstacleAbove()
    {
        return Physics.Raycast(playerCamera.transform.position, Vector3.up, standingHeight - crouchHeight);
    }

    // Handles Attack
    private void HandleAttack()
    {
        if (canAttack && controls.Gameplay.Attack.triggered)
        {
            canAttack = false;

            // Perform a raycast from the camera to detect if we hit an enemy
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, attackRange, attackableLayers))
            {
                // Check if the hit object has an "Enemy" component
                var enemy = hit.collider.GetComponent<Attackable>();
                if (enemy != null)
                {
                    print("Did damage");
                    enemy.Attack(attackDamage);
                }
            }

            // Start cooldown
            StartCoroutine(AttackCooldown());
        }

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
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting && (currentStamina > 0) ? sprintBobSpeed : walkBobSpeed);
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
            if (controls.Gameplay.Zoom.triggered)
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
            if (controls.Gameplay.Zoom.triggered)
            {
                StartZoom(true);
            }
            // Zoom out on key release
            if (controls.Gameplay.Zoom.triggered)
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

    // Check for and focus/unfocus on interactable objects
    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null && interactable.IsInViewAndNotObstructed(playerCamera.transform, obstructionLayerMask))
            {
                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLoseFocus();
                    }

                    currentInteractable = interactable;
                    currentInteractable.OnFocus();
                }
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnLoseFocus();
                currentInteractable = null;
            }
        }
    }

    // Trigger interaction if valid target and key pressed
    private void HandleInteractionInput()
    {
        // If interaction key is pressed and an interactable is in focus, interact with it
        if (controls.Gameplay.Interact.triggered && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    // Handles Footsteps
    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero)
        {
            footstepTimer = GetCurrentOffset;  // Reset timer if player is stationary
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            // Raycast to determine surface type
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                // Adjust volume for crouch
                footstepAudioSource.volume = isCrouching ? crouchVolumeMultiplier : 0.75f;
                footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

                // Play sound based on surface
                switch (hit.collider.tag)
                {
                    case "Footsteps/WOOD":
                        footstepAudioSource.PlayOneShot(woodClips[woodIndices[currentWoodFootstepIndex]]);
                        ShiftIndex(ref currentWoodFootstepIndex, woodClips.Length, ref woodIndices);
                        break;
                    case "Footsteps/CONCRETE":
                        footstepAudioSource.PlayOneShot(concreteClips[concreteIndices[currentconcreteFootstepIndex]]);
                        ShiftIndex(ref currentconcreteFootstepIndex, concreteClips.Length, ref concreteIndices);
                        break;
                    case "Footsteps/GRASS":
                        footstepAudioSource.PlayOneShot(grassClips[grassIndices[currentGrassFootstepIndex]]);
                        ShiftIndex(ref currentGrassFootstepIndex, grassClips.Length, ref grassIndices);
                        break;
                    default:
                        break;
                }
            }
            // Reset footstep timer
            footstepTimer = GetCurrentOffset;
        }

    }

    // Generate randomized indices for audio clips
    private int[] GenerateRandomIndex(int clipCount)
    {
        List<int> availableIndices = new List<int>();
        // Populate list with clip indices
        for (int i = 0; i < clipCount; i++)
        {
            availableIndices.Add(i);
        }

        int[] randomizedIndices = new int[clipCount];
        // Shuffle indices
        for (int i = 0; i < clipCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            randomizedIndices[i] = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);
        }

        return randomizedIndices;
    }

    // Increment and reset index if needed
    private void ShiftIndex(ref int currentIndex, int clipLength, ref int[] indicesArray)
    {
        currentIndex++;

        // Reset if exceeds length
        if (currentIndex >= clipLength)
        {
            currentIndex = 0;
            indicesArray = GenerateRandomIndex(clipLength);
        }
    }

    // Handles Landing Sound
    private void HandleLandingSound()
    {
        if (wasInAir && characterController.isGrounded)
        {
            // Check if player is not sliding
            if (!IsSliding)
            {
                jumpAudioSource.PlayOneShot(landingClip);
            }
            wasInAir = false; // Set the flag to false immediately after landing
        }
        else if (!characterController.isGrounded)
        {
            wasInAir = true; // Set the flag to true if the player is in the air
        }
    }

    // Adjusts the crosshair size based on player movement
    private void HandleCrosshair()
    {
        // Expand crosshair when moving
        if (IsMoving())
        {
            currentSize = Mathf.Lerp(currentSize, crosshairMaxSize, Time.deltaTime * crosshairSpeed);
        }
        // Shrink crosshair when still
        else
        {
            currentSize = Mathf.Lerp(currentSize, crosshairRestingSize, 5 * Time.deltaTime * crosshairSpeed);
        }
        // Update the UI with the new crosshair size
        PlayerUI.Instance.UpdateCrosshair(currentSize);
    }

    // Apply movement and gravity
    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }
        if (WillSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    // Coroutine for stamina regeneration
    private IEnumerator RegenerateStamina()
    {
        // Delay before starting regeneration
        yield return new WaitForSeconds(timeBeforeStaminaRegen);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        // Regenerate until max stamina is reached
        while (currentStamina < maxStamina)
        {
            // Allow sprinting if there's stamina
            if (currentStamina > 0)
            {
                canSprint = true;
            }
            // Increment stamina, ensuring it doesn't exceed max
            currentStamina += staminaValueIncrement;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
            // Notify of any stamina changes
            UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina);

            // Pause before next increment
            yield return timeToWait;
        }
        // End of regeneration
        regeneratingStamina = null;
    }

    // Coroutine for crouching
    private IEnumerator CrouchStand(bool isCrouchingNow)
    {
        // Check for obstacle above when uncrouching
        if (isCrouchingNow && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        // Initialize crouch/stand parameters
        float timeElapesd = 0;
        float targetHeight = isCrouchingNow ? crouchHeight : standingHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouchingNow ? crouchingCenter : standingCenter;
        Vector3 currentCenter = characterController.center;

        // Lerp height and center during crouch/stand transition
        while (timeElapesd < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapesd / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapesd / timeToCrouch);
            timeElapesd += Time.deltaTime;
            yield return null;
        }

        // Set final height and center after animation
        characterController.height = targetHeight;
        characterController.center = targetCenter;

        // Set crouching state
        isCrouching = isCrouchingNow;

        duringCrouchAnimation = false;
    }

    // Coroutine for crouching if toggle crouch is enabled
    private IEnumerator ToggleCrouchStand()
    {
        // Check for obstacle above when uncrouching
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        // Initialize crouch/stand parameters
        float timeElapesd = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        // Lerp height and center during crouch/stand transition
        while (timeElapesd < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapesd / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapesd / timeToCrouch);
            timeElapesd += Time.deltaTime;
            yield return null;
        }

        // Set final height and center after animation
        characterController.height = targetHeight;
        characterController.center = targetCenter;

        // Toggle crouching state
        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    // Coroutine for handling the attack cooldown duration
    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        float currentCooldown = attackCooldown;

        // Update cooldown until it's 0
        while (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            // Notify listeners of cooldown percentage
            float cooldownPercentage = (currentCooldown / attackCooldown) * 100;
            UIManager.Instance.UpdateAttackCooldownPercentage(cooldownPercentage);
            yield return null; // Wait for next frame
        }

        UIManager.Instance.UpdateAttackCooldownPercentage(0);
        canAttack = true; // Re-enable attacking
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