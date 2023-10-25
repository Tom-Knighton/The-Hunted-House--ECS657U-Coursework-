using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    // Player movement and action state checks
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded && !IsSliding;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    // Components
    private Attackable _attackable;


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
    [SerializeField] private bool enableLandingSound = true;

    // Key bindings for controls
    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode attackKey = KeyCode.Mouse0;

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

    // Stamina settings
    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegen = 3;
    [SerializeField] private float staminaValueIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;

    // Jumping settings
    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

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

    // Attack settings
    [Header("Attack Parameters")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackDamage = 10.0f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private LayerMask attackableLayers;
    private bool canAttack = true;

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

    // Jump and Landing Settings
    [Header("Jump and Landing Sound Parameters")]
    [SerializeField] private AudioSource jumpAudioSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landingClip;
    private bool wasInAir = false;

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

    // Interaction settings
    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    // References to essential components
    private Camera playerCamera;
    private CharacterController characterController;
    public GameObject deathScreenCanvas;
    public GameObject playerUI;

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
    }

    private void Start()
    {
        if (_attackable is not null)
        {
            _attackable.OnHealthChanged.AddListener(OnHealthChanged);
            _attackable.OnDeath.AddListener(OnDeath);
        }

        UIManager.Instance.ShowPlayerUI();
        UpdateUIOnRespawn();
    }

    private void UpdateUIOnRespawn()
    {
        UIManager.Instance.UpdatePlayerHealth(_attackable.health, _attackable.maxHealth);
        UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina);
        UIManager.Instance.UpdateAttackCooldownPercentage(0);
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

    // Handle mouse look based on input
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
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
        PlayerUI.Instance.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Display Death Screen
        deathScreenCanvas.SetActive(true);

        // Disable player interactions
        this.enabled = false;
    }

    // Handles Stamina
    private void HandleStamina()
    {
        // If player is sprinting, not crouching, and moving
        if (IsSprinting && !isCrouching && currentInput != Vector2.zero)
        {
            // Stop stamina regeneration if active
            if (regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }
            // Decrease stamina based on sprinting
            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            // Ensure stamina doesn't go negative
            if (currentStamina < 0)
            {
                currentStamina = 0;
            }

            // Notify of stamina change
            UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina);

            // Disable sprinting if stamina is depleted
            if (currentStamina <= 0)
            {
                canSprint = false;
            }
        }
        // Start stamina regeneration if not sprinting and stamina isn't full
        if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
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
            if (Input.GetKey(crouchKey) && !duringCrouchAnimation && characterController.isGrounded && !isCrouching)
            {
                wantsToStand = false; // Reset this flag when crouch key is pressed
                StartCoroutine(CrouchStand(true)); // true indicates we're crouching
            }
            // Try to stand up when crouch key is released
            else if (!Input.GetKey(crouchKey) && !duringCrouchAnimation && characterController.isGrounded && isCrouching)
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
        if (canAttack && Input.GetKeyDown(attackKey))
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

    // Check for and focus/unfocus on interactable objects
    private void HandleInteractionCheck()
    {
        // Raycast to detect interactable objects
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            // If a new interactable is detected, focus on it
            if (hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable)
                {
                    currentInteractable.OnFocus();
                }
            }
        }
        // If no interactable is detected, lose focus on the current one
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    // Trigger interaction if valid target and key pressed
    private void HandleInteractionInput()
    {
        // If interaction key is pressed and an interactable is in focus, interact with it
        if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
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
                footstepAudioSource.volume = isCrouching ? crouchVolumeMultiplier : 1f;
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