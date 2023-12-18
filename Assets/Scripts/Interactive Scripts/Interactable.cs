using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Base class for interactable objects.
public abstract class Interactable : MonoBehaviour
{
    private List<Outline> outlineComponents;

    public virtual void Awake()
    {
        gameObject.layer = 9;

        outlineComponents = new List<Outline>(GetComponentsInChildren<Outline>());
        if (outlineComponents.Count == 0)
        {
            Debug.LogError("No Outline components found in the children of the interactable object.");
        }
    }

    public abstract void OnInteract();

    public virtual void OnFocus()
    {
        foreach (var outline in outlineComponents)
        {
            outline.ToggleOutline(true);
        }
        Debug.Log("Looking at " + gameObject.name);
    }

    public virtual void OnLoseFocus()
    {
        foreach (var outline in outlineComponents)
        {
            outline.ToggleOutline(false);
        }
        Debug.Log("Stopped looking at " + gameObject.name);
    }

    protected string GetInteractKey()
    {
        var controls = new PlayerInputActions();
        int bindingIndex = controls.Gameplay.Interact.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
        string bindingPath = controls.Gameplay.Interact.bindings[bindingIndex].effectivePath;
        return InputControlPath.ToHumanReadableString(bindingPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }
}
