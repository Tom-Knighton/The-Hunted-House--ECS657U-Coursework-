using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupObject : Interactable
{
    private GameObject currObjHold;
    private Rigidbody currHoldRb;
    private GameObject objInRange;
    private Renderer objRender;
    private Color originalColour;
    private Renderer[] childObjs;

    //public float throwForce = 450f;
    //public float rangePickup = 5f;
    public Transform pickUpPos;
    

    //Highlights the object when in focus of the player
    public override void OnFocus()
    {
        childObjs = gameObject.GetComponentsInChildren<Renderer>(); //stored in an array because some prefabs consist of more than 1 component, and an empty parent
        if (childObjs.Length > 0)
        {
            objRender = childObjs[0]; //storing the first child of the parent
            originalColour = objRender.material.GetColor("_Color"); //geting the colour of the first child to keep as the original colour
            Highlight(); 
        }
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

    //Returns the colour of the object back to default when looking away from the object
    public override void OnLoseFocus()
    {
        DefaultCol();
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

    /*public void Throw()
    {
        currHoldRb.isKinematic = false;
        objToHold.transform.parent = null;
        currHoldRb.AddForce(transform.forward * throwForce);
        objToHold = null;
    }*/

    //Changes the colour of the object to a colour that represents 'highlight'
    void Highlight()
    {
        Color highlightCol = new Color(1f, 0.92f, 0.016f, 1f);

        foreach (Renderer childObjs in childObjs)
        {
            childObjs.material.SetColor("_Color", highlightCol);
        }
    }

    //Turns back the colour of the object back to default when looking away
    void DefaultCol()
    {
        
        foreach (Renderer childObjs in childObjs)
        {
            childObjs.material.SetColor("_Color", originalColour);
        }
    }
}
