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
    }

    public abstract void OnInteract();

    public virtual void OnFocus()
    {
        foreach (var outline in outlineComponents)
        {
            outline.ToggleOutline(true);
        }
        PlayerUI.Instance.ShowInteractPrompt();
    }

    public virtual void OnLoseFocus()
    {
        foreach (var outline in outlineComponents)
        {
            outline.ToggleOutline(false);
        }
        PlayerUI.Instance.HideInteractPrompt();
    }

    protected string GetInteractKey()
    {
        var controls = new PlayerInputActions();
        controls.Gameplay.Enable();
        int bindingIndex = controls.Gameplay.Interact.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
        if (bindingIndex >= 0 && bindingIndex < controls.Gameplay.Interact.bindings.Count)
        {
            string bindingPath = controls.Gameplay.Interact.bindings[bindingIndex].effectivePath;
            Debug.Log($"Valid binding index found: {bindingIndex}, Binding path: {bindingPath}");
            return InputControlPath.ToHumanReadableString(bindingPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        Debug.Log($"No valid binding index found. Defaulting to 'E'. Binding Index: {bindingIndex}");
        return "E";
    }
    public bool IsInViewAndNotObstructed(Transform playerTransform, LayerMask obstructionLayers)
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        directionToPlayer.Normalize();

        // Check if there's an obstruction between the interactable and the player
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstructionLayers))
        {
            // If the raycast hits something that is not the player, return false
            if (hit.transform != playerTransform)
            {
                return false;
            }
        }
        // No obstruction detected
        return true;
    }
}