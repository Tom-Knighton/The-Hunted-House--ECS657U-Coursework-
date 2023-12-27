using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Door : Interactable
{
    private bool _isOpen = false;
    private bool _canInteract = true;
    private Animator _anim;
    private static readonly int Dot = Animator.StringToHash("dot");
    private static readonly int IsOpen = Animator.StringToHash("isOpen");
    
    private void Start()
    {
        _anim = GetComponent<Animator>();
    }
    
    public override void OnInteract()
    {
        if (!_canInteract) return;
        
        _isOpen = !_isOpen;

        var doorTransformDirection = transform.TransformDirection(Vector3.forward);
        var playerTransformDirection = FirstPersonController.instance.transform.position - transform.position;
        var dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

        _anim.SetFloat(Dot, dot);
        _anim.SetBool(IsOpen, _isOpen);

        StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        while(_isOpen)
        {
            yield return new WaitForSeconds(5);
            
            _isOpen = false;
            _anim.SetFloat(Dot, 0);
            _anim.SetBool(IsOpen, _isOpen);
        }
    }

    private void Animator_LockInteraction()
    {
        _canInteract = false;
    }

    private void Animator_UnlockInteraction()
    {
        _canInteract = true;
    }
}
