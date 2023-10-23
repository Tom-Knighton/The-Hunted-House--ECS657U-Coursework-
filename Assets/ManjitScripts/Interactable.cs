using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for interactable objects.
public abstract class Interactable : MonoBehaviour
{
    // Change this based on interactable layer in project
    public virtual void Awake()
    {
        gameObject.layer = 9;
    }

    public abstract void OnInteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();


}
