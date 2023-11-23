using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.Inventory
{
    public class Inventory: MonoBehaviour
    {
        [SerializeField] private int inventorySize = 10;

        private readonly List<IInventoryItem> _inventoryItems = new();
        
        public bool TryAddItemToInventory(IInventoryItem item)
        {
            if (_inventoryItems.Count >= inventorySize) return false;
            
            _inventoryItems.Add(item);
            
            UIManager.Instance.ShowHint($"You picked up a {item.Name}!");
            return true;
        }

        public bool HasItemWithName(string name)
        {
            return _inventoryItems.Any(i => i.Name == name);
        }
    }
}