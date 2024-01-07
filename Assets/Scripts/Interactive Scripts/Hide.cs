using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Hide : Interactable
{

    public Transform teleportTarget;
    public GameObject thePlayer;  
    private bool isInside = false;
    public FirstPersonController personControl;

    public override void OnInteract()
    {  
        isInside = !isInside;
        if (isInside){
            personControl.SavePosition();
            thePlayer.GetComponent<CharacterController>().enabled = false;
            personControl.canMovement();
            thePlayer.transform.position = teleportTarget.transform.position;
            

        }else{
            thePlayer.transform.position = personControl.exitPosition;
            personControl.canMovement();
            thePlayer.GetComponent<CharacterController>().enabled = true;
            
        }
        
    }


}
