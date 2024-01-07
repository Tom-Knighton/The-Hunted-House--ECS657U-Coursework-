using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using System.Collections.Generic;
using System;
using System.Linq;

public class TextBoxTextSetter : MonoBehaviour
{
    [TextArea(2, 3)]
    [SerializeField] private string message = "Press BUTTONPROMPT to interact";

    [SerializeField] private SpriteAssetsList spriteAssetsList;

    [Serializable]
    public class InputBindingContainer
    {
        public List<InputBinding> bindings;
    }

    private PlayerInputActions _playerInput;
    private PlayerInput _playerInputComponent;
    private TMP_Text _textBox;


    private void Awake()
    {
        _playerInput = new PlayerInputActions();
        _textBox = GetComponent<TMP_Text>();
        _playerInputComponent = GetComponent<PlayerInput>();

        LoadBindingOverrides();
        _playerInput.Gameplay.Enable();
    }

    private void LoadBindingOverrides()
    {
        string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
        {
            _playerInput.LoadBindingOverridesFromJson(rebinds);
            Debug.Log("Loaded rebinds: " + rebinds); // Added debug statement
        }
        else
        {
            Debug.Log("No rebinds found in PlayerPrefs."); // Added debug statement for empty or missing rebinds
        }
    }


    private void Start()
    {
        SetText();
    }

    public void SetInteractText()
    {
        UpdateBinding();
        SetText();
    }

    private void UpdateBinding()
    {
        LoadBindingOverrides();
        _playerInputComponent = GetComponent<PlayerInput>();
        // Get the current control scheme from the PlayerInput component
        var controlScheme = _playerInputComponent.currentControlScheme;

        // Find the binding index for the current control scheme
        var bindingIndex = _playerInput.Gameplay.Interact.bindings.ToList().FindIndex(
            binding => binding.groups != null && binding.groups.Contains(controlScheme)
        );

        // If a valid binding index is found, update the message
        if (bindingIndex != -1)
        {
            var binding = _playerInput.Gameplay.Interact.bindings[bindingIndex];
            var interactKey = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            message = $"Press {interactKey} to interact";
        }
        else
        {
            Debug.LogWarning("No valid binding found for the current control scheme.", this);
        }
    }

    private bool AreKeybindsChanged()
    {
        // Create a new instance to get the default keybinds
        var defaultActions = new PlayerInputActions();
        var defaultKeybinds = defaultActions.Gameplay.Interact.bindings.Select(b => b.effectivePath).ToList();

        // Compare with current keybinds
        var currentKeybinds = _playerInput.Gameplay.Interact.bindings.Select(b => b.effectivePath).ToList();

        // Check if the keybinds are different
        bool keybindsAreDifferent = !defaultKeybinds.SequenceEqual(currentKeybinds);
        return keybindsAreDifferent;
    }


    private void SetText()
    {
        var currentDevice = InputSystem.GetDevice<InputDevice>();
        TMP_SpriteAsset spriteAsset = DetermineSpriteAsset(currentDevice);
        var bindingIndex = currentDevice is Keyboard || currentDevice is Mouse ? 0 : 1;
        bool keybindsChanged = AreKeybindsChanged();

        InputBinding binding;
        if (keybindsChanged)
        {
            binding = _playerInput.Gameplay.Interact.bindings[bindingIndex];
        }
        else
        {
            binding = new PlayerInputActions().Gameplay.Interact.bindings[bindingIndex];
        }

        _textBox.text = TextButtonPromptSprite.ReadAndReplaceBinding(
            message,
            binding,
            spriteAsset
        );
    }


    private TMP_SpriteAsset DetermineSpriteAsset(InputDevice currentDevice)
    {
        TMP_SpriteAsset selectedSpriteAsset;
        if (currentDevice is Keyboard || currentDevice is Mouse)
        {
            selectedSpriteAsset = spriteAssetsList.SpriteAssets[0];
        }
        else if (currentDevice is Gamepad gamepad)
        {
            if (gamepad is DualShockGamepad || gamepad is DualSenseGamepadHID)
            {
                selectedSpriteAsset = spriteAssetsList.SpriteAssets[1];
            }
            else
            {
                selectedSpriteAsset = spriteAssetsList.SpriteAssets[2];
            }
        }
        else
        {
            Debug.LogWarning($"Unrecognized input device: {currentDevice}. Defaulting to Keyboard sprites.");
            selectedSpriteAsset = spriteAssetsList.SpriteAssets[0];
        }
        return selectedSpriteAsset;
    }

}