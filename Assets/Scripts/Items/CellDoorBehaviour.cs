using Game;
using UnityEngine;

namespace Items
{
    public class CellDoorBehaviour : PickupObject
    {
        bool isOpen = false;

        public override void OnInteract()
        {
            if (isOpen)
            {
                UIManager.Instance.ShowHint("You've already opened this door...");
                return;
            }

            // Access the FirstPersonController instance to check the current equipped item
            var playerController = GameManager.Instance.player.GetComponent<FirstPersonController>();
            var currentEquippedItem = playerController?.Inventory.GetHotbarSlot(playerController.currentEquippedSlot)?.Item;

            // Check if the currently equipped item is a screwdriver
            if (currentEquippedItem is { Name: "Screwdriver" })
            {
                var hinge = GameObject.Find("Hinge");
                transform.RotateAround(hinge.transform.position, Vector3.up, 90);
                isOpen = true;
                UIManager.Instance.ShowHint("You used the Screwdriver to open the door.");
            }
            else
            {
                var hasScrewdriver = playerController.Inventory.GetItemCount("Screwdriver") > 0;

                if (hasScrewdriver)
                {
                    UIManager.Instance.ShowHint("You need to equip the screwdriver to your hotbar. Open your inventory to do this.");
                }
                else
                {
                    UIManager.Instance.ShowHint("You need to find something to open the door with...");
                }
            }
        }
    }
}