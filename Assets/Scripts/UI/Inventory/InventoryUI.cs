using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Player.Inventory;

public class InventoryUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GameObject[] inventorySlotUI; // Array of GameObjects for inventory slots
    [SerializeField] private GameObject[] hotbarSlotUI;    // Array of GameObjects for hotbar slots
    [SerializeField] private Inventory playerInventory;    // Reference to the player's inventory
    private GameObject draggedItem;
    private InventorySlot originalSlot;

    private void Start()
    {
        // Ensure UI slots count matches the inventory and hotbar sizes
        if (inventorySlotUI.Length != playerInventory.InventorySize || hotbarSlotUI.Length != playerInventory.HotbarSize)
        {
            return;
        }
        UpdateInventoryDisplay();
    }

    public void SwapItemSlots(InventorySlot slot1, InventorySlot slot2)
    {
        playerInventory.SwapSlots(slot1, slot2);
        RefreshInventoryDisplay();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Get the slot that we started dragging
        originalSlot = eventData.pointerPressRaycast.gameObject.GetComponent<InventorySlotUI>().Slot;
        if (originalSlot != null && !originalSlot.IsEmpty)
        {
            // Create a temporary icon to follow the cursor
            draggedItem = new GameObject("DraggedItem");
            var rt = draggedItem.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50, 50); // Set the size of the dragged icon
            var img = draggedItem.AddComponent<Image>();
            img.sprite = originalSlot.Item.Icon;
            img.raycastTarget = false;
            draggedItem.transform.SetParent(transform); // Set as child of the panel
            draggedItem.transform.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            draggedItem.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedItem != null)
        {
            InventorySlotUI resultSlotUI = eventData.pointerCurrentRaycast.gameObject.GetComponent<InventorySlotUI>();
            if (resultSlotUI == null)
            {
                // If the item was not dropped onto another slot, return it to the original slot
                originalSlot.AddItem(draggedItem.GetComponent<InventorySlotUI>().Slot.Item);
            }
            Destroy(draggedItem); // Clean up the dragged item icon
            RefreshInventoryDisplay(); // Refresh display to show changes
        }
    }

    public void OnBeginDragItem(InventorySlot slot, InventorySlotUI slotUI)
    {
        // Logic when an item begins being dragged
    }

    public void OnEndDragItem()
    {
        // Logic when an item ends being dragged
    }

    public void OnSlotItemDropped(InventorySlotUI droppedSlotUI, InventorySlotUI targetSlotUI)
    {
        // Call SwapSlots method from Inventory
        playerInventory.SwapSlots(droppedSlotUI.Slot, targetSlotUI.Slot);
    }

    public void RefreshInventoryDisplay()
    {
        UpdateInventoryDisplay();
    }

    public void UpdateInventoryDisplay()
    {
        // Update inventory slots
        for (int i = 0; i < playerInventory.InventorySize; i++)
        {
            var slotUI = inventorySlotUI[i].GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Initialize(playerInventory.GetInventorySlot(i));
            }
            else
            {
                Debug.LogError("InventorySlotUI component missing on inventory slot UI element");
            }
        }

        // Update hotbar slots
        for (int i = 0; i < playerInventory.HotbarSize; i++)
        {
            var hotbarSlotUIComponent = hotbarSlotUI[i].GetComponent<InventorySlotUI>();
            if (hotbarSlotUIComponent != null)
            {
                hotbarSlotUIComponent.Initialize(playerInventory.GetHotbarSlot(i));
            }
            else
            {
                Debug.LogError("InventorySlotUI component missing on hotbar slot UI element");
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to the inventory changed event
        playerInventory.InventoryChanged += UpdateInventoryDisplay;
    }

    private void OnDisable()
    {
        // Unsubscribe from the inventory changed event
        playerInventory.InventoryChanged -= UpdateInventoryDisplay;
    }
}