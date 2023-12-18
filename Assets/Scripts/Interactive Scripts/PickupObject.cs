using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : Interactable
{
    private GameObject currObjHold;
    private Rigidbody currHoldRb;
    private Renderer objRender;
    private Color originalColour;
    private Renderer[] childObjs;
    public Transform pickUpPos;
    

    //Highlights the object when in focus of the player
    public override void OnFocus()
    {
        base.OnFocus();
    }


    /* When the player hits the interactive key (for example, E) in front of a pickupable
    object, the function either allows an object to be picked up or dropped. */
    public override void OnInteract()
    {
        
        if (currObjHold == null)
        {
            PickUp(gameObject);
        }
        else
        {
            LetGo();
        }
        
    }
    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
    }

    /**
     * Picking up the object 
     * @param the game object that will be picked up
     */
    private void PickUp(GameObject focusObj)
    {
        //checking if the object to be picked up has a rigid body
        if (focusObj.GetComponent<Rigidbody>())
        {
            currObjHold = focusObj;
            currHoldRb = focusObj.GetComponent<Rigidbody>();
            currHoldRb.isKinematic = true;
            currObjHold.transform.parent = pickUpPos.transform;
        }
    }

    //Dropping the object
    private void LetGo()
    {
        currHoldRb.isKinematic = false;
        currObjHold.transform.parent = null;
        currObjHold = null;
    }
}
