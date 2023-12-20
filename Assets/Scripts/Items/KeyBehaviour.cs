using Game;
using Player.Inventory;

namespace Items
{
    public class KeyBehaviour: PickupObject, IInventoryItem
    {
        public string Name => "GateKey";
        public int MaxStackableSize => 3;

        public override void OnInteract()
        {
            GameManager.Instance.player.Inventory?.TryAddItemToInventory(this);
            Destroy(gameObject);
            OnLoseFocus();
        }
    }
}