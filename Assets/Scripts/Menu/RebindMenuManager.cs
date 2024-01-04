using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindMenuManager : MonoBehaviour
{
    public InputActionReference MoveRef, LookRef, JumpRef, CrouchRef, SprintRef, AttackRef, InteractRef, InventoryRef, Hotbar1Ref, Hotbar2Ref, Hotbar3Ref, Hotbar4Ref;
    void Start()
    {
        
    }

    private void OnEnable()
    {
        MoveRef.action.Disable();
        LookRef.action.Disable();
        JumpRef.action.Disable();
        CrouchRef.action.Disable();
        SprintRef.action.Disable();
        AttackRef.action.Disable();
        InteractRef.action.Disable();
        InventoryRef.action.Disable();
        Hotbar1Ref.action.Disable();
        Hotbar2Ref.action.Disable();
        Hotbar3Ref.action.Disable();
        Hotbar4Ref.action.Disable();
    }

    private void OnDisable()
    {
        MoveRef.action.Enable();
        LookRef.action.Enable();
        JumpRef.action.Enable();
        CrouchRef.action.Enable();
        SprintRef.action.Enable();
        AttackRef.action.Enable();
        InteractRef.action.Enable();
        InventoryRef.action.Enable();
        Hotbar1Ref.action.Enable();
        Hotbar2Ref.action.Enable();
        Hotbar3Ref.action.Enable();
        Hotbar4Ref.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
