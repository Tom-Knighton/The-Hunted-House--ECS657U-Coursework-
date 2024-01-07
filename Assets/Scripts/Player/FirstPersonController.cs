using System.Collections;
using System.Collections.Generic;
using Game;
using Items;
using JetBrains.Annotations;
using Player.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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
    [SerializeField, Range(1, 10)] private float lookSpeedX = 1f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 1f;
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
    [SerializeField] private Animator razorBrushAnimator; // Assign in inspector
    [SerializeField] private float razorBrushAttackDamage = 50f;
    [SerializeField] private GameObject razorBrush;
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

    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultiplier : IsSprinting && (currentStamina > 0) ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    private int[] woodIndices;
    private int[] concreteIndices;
    private int[] grassIndices;
    private int currentWoodFootstepIndex = 0;
    private int currentconcreteFootstepIndex = 0;
    private int currentGrassFootstepIndex = 0;
    private float crouchVolumeMultiplier = 0.5f;
    private bool _wasLastInside = true;
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

    #region Inventory
    [Header("Inventory settings")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private GameObject razorbrushPrefab;
    private bool inventoryOpen = false;
    public int currentEquippedSlot = 0;
    private IInventoryItem previouslyEquippedItem;
    #endregion

    [Header("Pause Menu")]
    [SerializeField] private PauseMenu pauseMenu;

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
        LoadBindingOverrides();
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYpos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _attackable = GetComponent<Attackable>();
        Inventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        lookSpeedX = PlayerPrefs.GetFloat("XSensitivity", 1f);
        lookSpeedY = PlayerPrefs.GetFloat("YSensitivity", 1f);
        // If the _attackable component is present, subscribe to its events
        if (_attackable is not null)
        {
            _attackable.OnHealthChanged.AddListener(OnHealthChanged);
            _attackable.OnDeath.AddListener(OnDeath);
        }
        EquipItemInSlot(currentEquippedSlot);
        UpdateUIOnRespawn();

        if (useFootsteps)
        {
            woodIndices = GenerateRandomIndex(AudioManager.Instance.woodClips.Length);
            concreteIndices = GenerateRandomIndex(AudioManager.Instance.concreteClips.Length);
            grassIndices = GenerateRandomIndex(AudioManager.Instance.grassClips.Length);
        }
    }

    public void LoadBindingOverrides()
    {
        var rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
        {
            controls.LoadBindingOverridesFromJson(rebinds);
        }
        controls.Enable();
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
        HandleInput();
        // Always allow movement input handling, but apply movement only if CanMove is true
        HandleMovementInput();

        if (CanMove)
        {
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

            if (enableDynamicCrosshair)
            {
                HandleCrosshair();
            }
            HandleEquip();
            HandleCrafting();
        }

        // Apply gravity and final movements regardless of CanMove to ensure gravity is always applied
        ApplyFinalMovements();

        if (wantsToStand && !IsObstacleAbove())
        {
            StartCoroutine(CrouchStand(false)); // Stand up
            wantsToStand = false; // Reset the flag
        }
    }

    private void HandleInput()
    {
        if (controls.Gameplay.Pause.triggered)
        {
            HandlePause();
        }

        if (controls.Gameplay.Inventory.triggered)
        {
            ToggleInventory();
        }
    }

    private void HandlePause()
    {
        // Call the pause menu functionality
        pauseMenu.TogglePauseMenu();

        // Toggle player UI and controls based on the pause state
        if (!CanMove)
        {
            // Game is paused, hide player UI 
            UIManager.Instance.HidePlayerUI();
        }
        else
        {
            // Game is resumed, show player UI
            UIManager.Instance.ShowPlayerUI();
        }
    }

    public void ToggleMove()
    {
        // Toggle the current movement state and cursor visibility
        CanMove = !CanMove;
        Cursor.lockState = CanMove ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !CanMove;
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

    public void UpdateLookSensitivity(float xSensitivity, float ySensitivity)
    {
        lookSpeedX = xSensitivity;
        lookSpeedY = ySensitivity;
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
        inventoryOpen = !inventoryOpen;
        inventoryUI.gameObject.SetActive(inventoryOpen);
        hotbarPanel.SetActive(inventoryOpen); // Toggle the hotbar panel visibility.

        // Lock or unlock the cursor based on whether the inventory is open
        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryOpen;

        // If the inventory has just been opened, update the display
        if (inventoryOpen)
        {
            inventoryUI.UpdateInventoryDisplay();
        }

        // Don't disable movement controls, only camera and interaction
        if (inventoryOpen)
        {
            controls.Gameplay.MouseLook.Disable();
            controls.Gameplay.Attack.Disable();
            controls.Gameplay.Interact.Disable();
        }
        else // Re-enable controls when the inventory is closed
        {
            controls.Gameplay.Enable();
        }
    }

    private void HandleCrafting()
    {
        // Check if inventory contains both "Razorblade" and "Toothbrush"
        if (HasItemInInventoryOrHotbar("Razorblade") && HasItemInInventoryOrHotbar("Toothbrush"))
        {
            // Remove one Razorblade and one Toothbrush from the inventory
            RemoveItemFromInventoryOrHotbar("Razorblade");
            RemoveItemFromInventoryOrHotbar("Toothbrush");

            // Create and add the Razorbrush to the inventory
            GameObject razorbrushInstance = Instantiate(razorbrushPrefab);
            IInventoryItem razorbrushItem = razorbrushInstance.GetComponent<IInventoryItem>();

            if (razorbrushItem != null)
            {
                Inventory.TryAddItemToInventory(razorbrushItem);
            }

            Destroy(razorbrushInstance);
        }
    }

    private bool HasItemInInventoryOrHotbar(string itemName)
    {
        // Check inventory
        if (Inventory.HasItemWithName(itemName)) return true;

        // Check hotbar
        for (int i = 0; i < Inventory.HotbarSize; i++)
        {
            if (Inventory.GetHotbarSlot(i).Item != null && Inventory.GetHotbarSlot(i).Item.Name == itemName)
                return true;
        }

        return false;
    }

    private void RemoveItemFromInventoryOrHotbar(string itemName)
    {
        // Try to remove from inventory first
        if (Inventory.RemoveItemByName(itemName)) return;

        // Try to remove from hotbar
        for (int i = 0; i < Inventory.HotbarSize; i++)
        {
            if (Inventory.GetHotbarSlot(i).Item != null && Inventory.GetHotbarSlot(i).Item.Name == itemName)
            {
                Inventory.GetHotbarSlot(i).RemoveItem();
                break;
            }
        }
    }

    private void HandleEquip()
    {
        if (controls.Gameplay.HotbarSlot1.triggered) { EquipItemInSlot(0); }
        if (controls.Gameplay.HotbarSlot2.triggered) { EquipItemInSlot(1); }
        if (controls.Gameplay.HotbarSlot3.triggered) { EquipItemInSlot(2); }
        if (controls.Gameplay.HotbarSlot4.triggered) { EquipItemInSlot(3); }
        UpdateEquippedItem();
    }
    private void EquipItemInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Inventory.HotbarSize)
            return;
        var item = Inventory.GetHotbarSlot(slotIndex).Item;
        UnequipCurrentItem();

        if (item != null && item.Name == "RazorBrush")
        {
            EquipRazorBrush();
        }
        else if (item != null)
        {
            item.ItemModel.SetActive(true);
        }
        currentEquippedSlot = slotIndex;
    }
    private void UnequipCurrentItem()
    {
        var currentItem = Inventory.GetHotbarSlot(currentEquippedSlot).Item;
        if (currentItem != null)
        {
            currentItem.ItemModel.SetActive(false);
        }
    }
    private void EquipRazorBrush()
    {
        razorBrush.SetActive(true);
        razorBrushAnimator = razorBrush.GetComponent<Animator>();
    }

    private void UpdateEquippedItem()
    {
        var currentSlot = Inventory.GetHotbarSlot(currentEquippedSlot);
        var currentSlotItem = currentSlot?.Item;

        // If the item has changed, update the equipped item.
        if (previouslyEquippedItem != currentSlotItem)
        {
            // Deactivate the previous item if it exists.
            if (previouslyEquippedItem?.ItemModel != null)
            {
                previouslyEquippedItem.ItemModel.SetActive(false);
            }

            // Activate the new item if it exists.
            if (currentSlotItem?.ItemModel != null)
            {
                foreach (Transform child in playerCamera.transform)
                {
                    if (child.gameObject != currentSlotItem.ItemModel)
                        child.gameObject.SetActive(false);
                }
                currentSlotItem.ItemModel.SetActive(true);
            }

            // Update the previously equipped item reference.
            previouslyEquippedItem = currentSlotItem;
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
        // Check if the current hotbar slot is null before accessing its properties
        if (Inventory.GetHotbarSlot(currentEquippedSlot) != null &&
           Inventory.GetHotbarSlot(currentEquippedSlot).Item != null &&
           Inventory.GetHotbarSlot(currentEquippedSlot).Item.Name == "Razorbrush")
        {
            if (canAttack && controls.Gameplay.Attack.triggered)
            {
                MeleeAttack();
            }
        }
        else
        {
            if (canAttack && controls.Gameplay.Attack.triggered)
            {
                DefaultAttack();
            }
        }
    }

    private void DefaultAttack()
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
    private void MeleeAttack()
    {
        // Play the attack animation
        razorBrushAnimator.SetTrigger("Attack");

        // Perform a raycast to check for hit
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, attackRange, attackableLayers))
        {
            var enemy = hit.collider.GetComponent<Attackable>();
            if (enemy != null)
            {
                print("Did damage with weapon");
                enemy.Attack(razorBrushAttackDamage);
            }
        }

        // Start cooldown
        StartCoroutine(AttackCooldown());
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
            var insideNow = true;
            // Raycast to determine surface type
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down, out var hit, 3))
            {
                // Adjust volume for crouch
                footstepAudioSource.volume = isCrouching ? crouchVolumeMultiplier : 0.75f;
                footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);

                // Play sound based on surface
                switch (hit.collider.tag)
                {
                    case "Footsteps/WOOD":
                        insideNow = true;
                        footstepAudioSource.PlayOneShot(AudioManager.Instance.woodClips[woodIndices[currentWoodFootstepIndex]]);
                        ShiftIndex(ref currentWoodFootstepIndex, AudioManager.Instance.woodClips.Length, ref woodIndices);
                        break;
                    case "Footsteps/CONCRETE":
                        insideNow = true;
                        footstepAudioSource.PlayOneShot(AudioManager.Instance.concreteClips[concreteIndices[currentconcreteFootstepIndex]]);
                        ShiftIndex(ref currentconcreteFootstepIndex, AudioManager.Instance.concreteClips.Length, ref concreteIndices);
                        break;
                    case "Footsteps/GRASS":
                        insideNow = false;
                        footstepAudioSource.PlayOneShot(AudioManager.Instance.grassClips[grassIndices[currentGrassFootstepIndex]]);
                        ShiftIndex(ref currentGrassFootstepIndex, AudioManager.Instance.grassClips.Length, ref grassIndices);
                        break;
                    default:
                        break;
                }
            }
            // Reset footstep timer
            footstepTimer = GetCurrentOffset;

            if (insideNow != _wasLastInside)
            {
                GameManager.Instance.ChangedTo(insideNow ? GameSceneType.Inside : GameSceneType.Outside);
                _wasLastInside = insideNow;
            }
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

        // Only move the character if CanMove is true
        if (CanMove)
        {
            characterController.Move(moveDirection * Time.deltaTime);
        }
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
        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        // Lerp height and center during crouch/stand transition
        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
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