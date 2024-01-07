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
            var added = GameManager.Instance.player.Inventory?.TryAddItemToInventory(this) ?? false;

            if (added)
            {
                Destroy(gameObject);
                OnLoseFocus();
            }
        }
    }
}