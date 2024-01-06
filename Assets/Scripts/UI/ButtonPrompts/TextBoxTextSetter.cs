using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

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
        string rebinds = PlayerPrefs.GetString("rebinds", string.Empty);
        if (!string.IsNullOrEmpty(rebinds))
            _playerInput.LoadBindingOverridesFromJson(rebinds);
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

        TMP_SpriteAsset spriteAsset = DetermineSpriteAsset(currentDevice);
        var bindingIndex = currentDevice is Keyboard || currentDevice is Mouse ? 0 : 1;

        _textBox.text = TextButtonPromptSprite.ReadAndReplaceBinding(
            message,
            _playerInput.Gameplay.Interact.bindings[bindingIndex],
            spriteAsset
        );
    }

    private TMP_SpriteAsset DetermineSpriteAsset(InputDevice currentDevice)
    {
        if (currentDevice is Keyboard || currentDevice is Mouse)
        {
            return spriteAssetsList.SpriteAssets[0];
        }
        else if (currentDevice is Gamepad gamepad)
        {
            if (gamepad is DualShockGamepad || gamepad is DualSenseGamepadHID)
            {
                return spriteAssetsList.SpriteAssets[1];
            }
            else
            {
                return spriteAssetsList.SpriteAssets[2];
            }
        }
        else
        {
            Debug.LogWarning($"Unrecognized input device: {currentDevice}. Defaulting to Keyboard sprites.");
            return spriteAssetsList.SpriteAssets[0];
        }
    }
}