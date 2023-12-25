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
        Debug.Log("OnDrop called");

        if (eventData.pointerDrag == null)
        {
            Debug.Log("Event data does not contain a pointerDrag object");
            return;
        }

        InventorySlotUI droppedSlotUI = eventData.pointerDrag.GetComponent<InventorySlotUI>();

        if (droppedSlotUI == null)
        {
            Debug.Log("Dropped object is not an InventorySlotUI");
            return;
        }

        if (this.Slot == null)
        {
            Debug.Log("Current slot is null");
            return;
        }

        Debug.Log($"Swapping slots: {droppedSlotUI.Slot} with {this.Slot}");
        inventoryUI.OnSlotItemDropped(droppedSlotUI, this);
    }
}
