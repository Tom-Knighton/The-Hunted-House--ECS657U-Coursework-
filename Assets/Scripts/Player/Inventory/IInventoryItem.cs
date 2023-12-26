using UnityEngine;

namespace Player.Inventory
{
    public interface IInventoryItem
    {
        public string Name { get; }
        public int MaxStackableSize { get; }
        Sprite Icon { get; }
    }
}