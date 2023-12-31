using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Player.Inventory
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private int inventorySize = 8; 
        [SerializeField] private int hotbarSize = 4;   
        private InventorySlot[] _inventorySlots;
        private InventorySlot[] _hotbarSlots;

        public event Action InventoryChanged;

        private void Awake()
        {
            // Initialize inventory and hotbar slots
            _inventorySlots = new InventorySlot[inventorySize];
            _hotbarSlots = new InventorySlot[hotbarSize];

            for (int i = 0; i < inventorySize; i++)
            {
                _inventorySlots[i] = new InventorySlot();
            }
            for (int i = 0; i < hotbarSize; i++)
            {
                _hotbarSlots[i] = new InventorySlot();
            }
        }
        public InventorySlot FindStackableSlot(IInventoryItem item)
        {
            foreach (var slot in _inventorySlots.Concat(_hotbarSlots))
            {
                if (slot.CanStackItem(item))
                {
                    return slot;
                }
            }
            return null;
        }

        public bool TryAddItemToInventory(IInventoryItem item)
        {
            var stackableSlot = FindStackableSlot(item);
            if (stackableSlot != null)
            {
                stackableSlot.AddItem(item);
                NotifyInventoryChanged();
                return true;
            }
            // First, try to stack the item in an existing slot
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

        public bool RemoveItemByName(string itemName)
        {
            // Iterate through inventory slots
            for (int i = 0; i < _inventorySlots.Length; i++)
            {
                if (_inventorySlots[i].Item != null && _inventorySlots[i].Item.Name == itemName)
                {
                    _inventorySlots[i].RemoveItem();
                    return true; // Item was successfully removed
                }
            }

            // Iterate through hotbar slots
            for (int i = 0; i < _hotbarSlots.Length; i++)
            {
                if (_hotbarSlots[i].Item != null && _hotbarSlots[i].Item.Name == itemName)
                {
                    _hotbarSlots[i].RemoveItem();
                    return true; // Item was successfully removed
                }
            }

            return false; // Item was not found
        }

        public int GetItemCount(string itemName)
        {
            return _inventorySlots
                .Where(slot => slot.Item != null && slot.Item.Name == itemName)
                .Sum(slot => slot.Quantity);
        }

        public bool MoveToHotbar(int inventoryIndex, int hotbarIndex)
        {
            // Validate indices and slot availability
            if (inventoryIndex < 0 || inventoryIndex >= inventorySize ||
                hotbarIndex < 0 || hotbarIndex >= hotbarSize ||
                _inventorySlots[inventoryIndex].IsEmpty ||
                !_hotbarSlots[hotbarIndex].IsEmpty)
            {
                return false;
            }

            // Move the item
            _hotbarSlots[hotbarIndex] = _inventorySlots[inventoryIndex];
            _inventorySlots[inventoryIndex] = new InventorySlot();

            NotifyInventoryChanged();
            return true;
        }

        public bool MoveToInventory(int hotbarIndex, int inventoryIndex)
        {
            // Validate indices and slot availability
            if (hotbarIndex < 0 || hotbarIndex >= hotbarSize ||
                inventoryIndex < 0 || inventoryIndex >= inventorySize ||
                _hotbarSlots[hotbarIndex].IsEmpty ||
                !_inventorySlots[inventoryIndex].IsEmpty)
            {
                return false;
            }

            // Move the item
            _inventorySlots[inventoryIndex] = _hotbarSlots[hotbarIndex];
            _hotbarSlots[hotbarIndex] = new InventorySlot();

            NotifyInventoryChanged();
            return true;
        }

        public InventorySlot GetInventorySlot(int index)
        {
            // Added method for accessing inventory slots
            if (index < 0 || index >= inventorySize)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Inventory index is out of range.");
            }
            return _inventorySlots[index];
        }

        public InventorySlot GetHotbarSlot(int index)
        {
            // Added method for accessing hotbar slots
            if (index < 0 || index >= hotbarSize)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Hotbar index is out of range.");
            }
            return _hotbarSlots[index];
        }
        public void SwapSlots(InventorySlot slot1, InventorySlot slot2)
        {
            // Your logic for swapping the items in the slots
            IInventoryItem tempItem = slot1.Item;
            int tempQuantity = slot1.Quantity;

            slot1.SetItem(slot2.Item, slot2.Quantity);
            slot2.SetItem(tempItem, tempQuantity);

            NotifyInventoryChanged();
        }

        public int InventorySize => inventorySize;
        public int HotbarSize => hotbarSize;

        private void NotifyInventoryChanged()
        {
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
    }

    [Serializable]
    public class InventorySlot
    {
        public IInventoryItem Item { get; private set; }
        public int Quantity { get; private set; }

        public bool IsEmpty => Item == null;

        public void AddItem(IInventoryItem newItem)
        {
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
        public void SetItem(IInventoryItem newItem, int newQuantity)
        {
            Item = newItem;
            Quantity = newQuantity;
        }

        public bool CanStackItem(IInventoryItem itemToCheck)
        {
            return (Item != null) && (Item.Name == itemToCheck.Name) && (Quantity < Item.MaxStackableSize);
        }

        public void ClearSlot()
        {
            SetItem(null, 0);
        }

    }
}