using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implementation of the Interactable for testing purposes.
public class TestInteractable : Interactable
{
    // Prints to console when you start looking at an object
    // Can be used to highlight object and show "Press F to interact"
    public override void OnFocus()
    {
        print("LOOKING AT " + gameObject.name);
    }

    // Prints to console when looking at an object and you click interact
    // Usually edit this class for object purposes e.g. Pick up object, Open Door, Press button
    public override void OnInteract()
    {
        print("INTERACTED WITH " + gameObject.name);
    }

    // Prints to console when you stop looking at an object
    public override void OnLoseFocus()
    {
        print("NO LONGER LOOKING AT " + gameObject.name);
    }
}
