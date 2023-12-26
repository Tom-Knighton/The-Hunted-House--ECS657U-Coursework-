using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Player.Inventory;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public InventorySlot Slot { get; private set; }

    [SerializeField] private Image itemIcon; // Assign in inspector
    [SerializeField] private TextMeshProUGUI quantityText; // Assign in inspector
    [SerializeField] private InventoryUI inventoryUI; // Assign in inspector

    private Canvas parentCanvas; // The canvas this UI element is part of
    private RectTransform rectTransform;
    private GameObject draggedIcon;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(InventorySlot slot)
    {
        Slot = slot;
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (!Slot.IsEmpty)
        {
            itemIcon.sprite = Slot.Item.Icon;
            itemIcon.enabled = true;
            quantityText.text = Slot.Quantity.ToString();
        }
        else
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            quantityText.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Handle item use or other click interactions here
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Slot != null && !Slot.IsEmpty)
        {
            draggedIcon = new GameObject("Dragged Icon");
            draggedIcon.transform.SetParent(parentCanvas.transform);
            draggedIcon.transform.SetAsLastSibling(); // Ensure it renders on top
            var image = draggedIcon.AddComponent<Image>();
            image.sprite = itemIcon.sprite;
            image.raycastTarget = false;
            image.SetNativeSize();
            RectTransform iconRectTransform = draggedIcon.GetComponent<RectTransform>();
            iconRectTransform.sizeDelta = rectTransform.sizeDelta; // Match the size of the slot
            inventoryUI.OnBeginDragItem(Slot, this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            draggedIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon);
            inventoryUI.OnEndDragItem();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI droppedSlotUI = eventData.pointerDrag.GetComponent<InventorySlotUI>();
        if (droppedSlotUI != null && this.Slot != null)
        {
            if (droppedSlotUI.Slot.Item != null && this.Slot.Item != null &&
                droppedSlotUI.Slot.Item.Name == this.Slot.Item.Name)
            {
                int totalQuantity = droppedSlotUI.Slot.Quantity + this.Slot.Quantity;
                int stackQuantity = Mathf.Min(totalQuantity, this.Slot.Item.MaxStackableSize);
                int remainingQuantity = totalQuantity - stackQuantity;

                this.Slot.SetItem(droppedSlotUI.Slot.Item, stackQuantity);

                if (remainingQuantity > 0)
                {
                    droppedSlotUI.Slot.SetItem(droppedSlotUI.Slot.Item, remainingQuantity);
                }
                else
                {
                    droppedSlotUI.Slot.ClearSlot();
                }
            }
            else
            {
                inventoryUI.SwapItemSlots(droppedSlotUI.Slot, this.Slot);
            }
            inventoryUI.RefreshInventoryDisplay();
        }
    }
}
