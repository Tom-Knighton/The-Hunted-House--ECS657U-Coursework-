using Game;
using Player.Inventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Items
{
    public class KeyBehaviour: PickupObject, IInventoryItem
    {
        public string Name => "Key";
        public int MaxStackableSize => 3;

        public override void OnInteract()
        {
            GameManager.Instance.player.Inventory?.TryAddItemToInventory(this);
            Destroy(gameObject);
        }
    }
}