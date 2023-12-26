using Game;
using Player.Inventory;
using UnityEngine;

namespace Items
{
    public class KeyBehaviour: PickupObject, IInventoryItem
    {
        [SerializeField]
        private Sprite icon;
        public string Name => "GateKey";
        public int MaxStackableSize => 3;
        public Sprite Icon => icon;

        public override void OnInteract()
        {
            Debug.Log($"Attempting to add {Name} to inventory");
            bool added = GameManager.Instance.player.Inventory?.TryAddItemToInventory(this) ?? false;

            if (added)
            {
                Debug.Log($"{Name} added to inventory");
                Destroy(gameObject);
                OnLoseFocus();
            }
            else
            {
                Debug.Log($"Failed to add {Name} to inventory - might be full or error");
            }
        }
    }
}