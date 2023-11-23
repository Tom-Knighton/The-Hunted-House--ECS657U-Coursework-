using Game;
using UnityEngine;

namespace Items
{
    public class CellDoorBehaviour: PickupObject
    {
        
        bool isOpen = false;
        
        public override void OnInteract()
        {
            if (isOpen)
            {
                UIManager.Instance.ShowHint("You've already opened this door...");
                return;
            }
            
            var hasScrewdriver = GameManager.Instance.player.Inventory.HasItemWithName("Screwdriver");

            if (hasScrewdriver)
            {
                var hinge = GameObject.Find("Hinge");
                transform.RotateAround(hinge.transform.position, Vector3.up, 90);
                isOpen = true;
            }
            else
            {
                UIManager.Instance.ShowHint("You need to find something to open the door with...");
            }
        }
    }
}