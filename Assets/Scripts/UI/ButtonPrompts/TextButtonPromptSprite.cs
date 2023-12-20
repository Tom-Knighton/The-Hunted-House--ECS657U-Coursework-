using System;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;
using System.Linq;

public static class TextButtonPromptSprite
{
    public static string ReadAndReplaceBinding(string textToDisplay, InputBinding binding, TMP_SpriteAsset spriteAsset)
    {
        string bindingName = GetBindingName(binding);
        if (spriteAsset != null && spriteAsset.spriteCharacterTable.Any(sc => sc.name == bindingName))
        {
            textToDisplay = textToDisplay.Replace("BUTTONPROMPT", $"<sprite=\"{spriteAsset.name}\" name=\"{bindingName}\">");
        }
        else
        {
            string humanReadableBinding = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
            textToDisplay = textToDisplay.Replace("BUTTONPROMPT", humanReadableBinding);
        }
        return textToDisplay;
    }

    private static string GetBindingName(InputBinding binding)
    {
        string path = binding.effectivePath;
        string device = path.Contains("Keyboard") ? "Keyboard_" : "Gamepad_";
        string keyOrButtonName = path.Substring(path.LastIndexOf('/') + 1).Replace("<", "").Replace(">", "");
        return device + keyOrButtonName;
    }
}