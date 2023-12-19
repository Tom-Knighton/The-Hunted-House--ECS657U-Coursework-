using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock; // Importing the DualShock namespace

public class TextBoxTextSetter : MonoBehaviour
{
    [TextArea(2, 3)]
    [SerializeField] private string message = "Press BUTTONPROMPT to interact";

    [SerializeField] private SpriteAssetsList spriteAssetsList;

    private PlayerInputActions _playerInput;
    private TMP_Text _textBox;

    private void Awake()
    {
        _playerInput = new PlayerInputActions();
        _textBox = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        SetText();
    }

    public void SetInteractText()
    {
        SetText();
    }

    private void SetText()
    {
        var currentDevice = InputSystem.GetDevice<InputDevice>();
        Debug.Log($"Current Input Device: {currentDevice}");

        // Check if the current device is either Keyboard or Mouse
        bool isKeyboardOrMouse = currentDevice is Keyboard || currentDevice is Mouse;

        Debug.Log($"Device Type: {currentDevice.GetType()}, Is Keyboard or Mouse: {isKeyboardOrMouse}");

        TMP_SpriteAsset spriteAsset = DetermineSpriteAsset(currentDevice);
        if (spriteAsset == null)
        {
            Debug.LogError("No Sprite Asset found for current device.");
            return;
        }

        // Use the correct binding index based on whether we have a keyboard/mouse or a gamepad
        var bindingIndex = isKeyboardOrMouse ? 0 : 1;
        Debug.Log($"Selected Binding Index: {bindingIndex}");

        _textBox.text = TextButtonPromptSprite.ReadAndReplaceBinding(
            message,
            _playerInput.Gameplay.Interact.bindings[bindingIndex],
            spriteAsset
        );

        Debug.Log($"Final text set to TMP_Text: {_textBox.text}");
    }

    private TMP_SpriteAsset DetermineSpriteAsset(InputDevice currentDevice)
    {
        if (currentDevice is Keyboard || currentDevice is Mouse)
        {
            // Treat mouse input like keyboard input for UI prompts
            return spriteAssetsList.SpriteAssets[0]; // Keyboard sprites
        }
        else if (currentDevice is Gamepad gamepad)
        {
            if (gamepad is DualShockGamepad || gamepad is DualSenseGamepadHID)
            {
                return spriteAssetsList.SpriteAssets[1]; // PlayStation sprites
            }
            else
            {
                return spriteAssetsList.SpriteAssets[2]; // Xbox sprites
            }
        }
        else
        {
            Debug.LogError($"Unrecognized input device: {currentDevice}");
            return null; // or a default sprite asset
        }
    }
}