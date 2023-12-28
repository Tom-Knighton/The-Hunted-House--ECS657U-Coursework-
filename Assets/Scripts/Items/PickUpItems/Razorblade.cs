using Game;
using Player.Inventory;
using UnityEngine;

namespace Items
{
    public class Razorblade : PickupObject, IInventoryItem
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject model;

        public string Name => "Razorblade";
        public int MaxStackableSize => 10;
        public Sprite Icon => icon;

        public GameObject ItemModel => model;

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
