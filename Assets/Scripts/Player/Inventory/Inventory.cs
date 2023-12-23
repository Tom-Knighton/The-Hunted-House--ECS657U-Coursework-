using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 18;
        private InventorySlot[] _inventorySlots;

        private void Awake()
        {
            _inventorySlots = new InventorySlot[inventorySize];
            for (int i = 0; i < inventorySize; i++)
            {
                _inventorySlots[i] = new InventorySlot();
            }
        }
        public bool TryAddItemToInventory(IInventoryItem item)
        {
            // First, try to stack the item in an existing slot
            Debug.Log($"Trying to add {item.Name} to inventory");
            foreach (var slot in _inventorySlots)
            {
                if (slot.CanStackItem(item))
                {
                    slot.AddItem(item);
                    NotifyInventoryChanged();
                    return true;
                }
            }
            // If item can't be stacked, find an empty slot
            foreach (var slot in _inventorySlots)
            {
                if (slot.IsEmpty)
                {
                    slot.AddItem(item);
                    NotifyInventoryChanged();
                    return true;
                }
            }
            // Inventory is full
            return false;
        }

        public bool HasItemWithName(string itemName)
        {
            return _inventorySlots.Any(slot => slot.Item != null && slot.Item.Name == itemName);
        }

        public int GetItemCount(string itemName)
        {
            return _inventorySlots
                .Where(slot => slot.Item != null && slot.Item.Name == itemName)
                .Sum(slot => slot.Quantity);
        }

        public event Action InventoryChanged;

        private void NotifyInventoryChanged()
        {
            Debug.Log("Notifying inventory change");
            InventoryChanged?.Invoke();
        }

        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= _inventorySlots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return _inventorySlots[index];
        }

        public int InventorySize => inventorySize;
    }

    [Serializable]
    public class InventorySlot
    {
        public IInventoryItem Item { get; private set; }
        public int Quantity { get; private set; }

        public bool IsEmpty => Item == null;

        public void AddItem(IInventoryItem newItem)
        {
            Debug.Log($"Adding {newItem.Name} to slot. Current Item: {Item?.Name}, Current Quantity: {Quantity}");
            if (Item == null)
            {
                Item = newItem;
                Quantity = 1;
            }
            else if (Item.Name == newItem.Name && Quantity < Item.MaxStackableSize)
            {
                Quantity++;
            }
        }

        public void RemoveItem()
        {
            if (Quantity > 0)
            {
                Quantity--;
                if (Quantity == 0)
                {
                    Item = null;
                }
            }
        }

        public bool CanStackItem(IInventoryItem itemToCheck)
        {
            return (Item != null) && (Item.Name == itemToCheck.Name) && (Quantity < Item.MaxStackableSize);
        }
    }
}