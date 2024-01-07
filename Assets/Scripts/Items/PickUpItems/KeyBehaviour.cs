using Game;
using Player.Inventory;
using UnityEngine;

namespace Items
{
    public class KeyBehaviour: PickupObject, IInventoryItem
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject model;
        public string Name => "GateKey";
        public int MaxStackableSize => GameManager.Instance.GameSettings.KeysRequired;
        public Sprite Icon => icon;
        public GameObject ItemModel => model;

        /// <summary>
        /// Adds a key to the inventory
        /// </summary>
        public override void OnInteract()
        {
            Debug.Log($"Attempting to add {Name} to inventory");
            var added = GameManager.Instance.player.Inventory?.TryAddItemToInventory(this) ?? false;

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