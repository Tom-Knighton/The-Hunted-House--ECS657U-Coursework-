using Game;
using Player.Inventory;

namespace Items
{
    public class Screwdriver: PickupObject, IInventoryItem
    {
        public string Name => "Screwdriver";
        public int MaxStackableSize => 16;
        
        public override void OnInteract()
        {
            GameManager.Instance.player.Inventory?.TryAddItemToInventory(this);
            Destroy(gameObject);
            OnLoseFocus();
        }
    }
}