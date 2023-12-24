using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Player.Inventory;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject[] inventorySlotUI; // Array of GameObjects representing inventory slots
    [SerializeField] private Inventory playerInventory; // Reference to the player's inventory

    private void Start()
    {
        if (inventorySlotUI.Length != playerInventory.InventorySize)
        {
            Debug.LogError("Inventory UI slots count does not match the inventory size.");
            return;
        }
        UpdateInventoryDisplay();
    }

    public void RefreshInventoryDisplay()
    {
        UpdateInventoryDisplay();
    }

    public void UpdateInventoryDisplay()
    {
        Debug.Log("Updating Inventory UI Display");
        for (int i = 0; i < playerInventory.InventorySize; i++)
        {
            InventorySlot slot = playerInventory.GetSlot(i);

            // Find the ItemIcon child within each inventory slot
            Transform itemIconTransform = inventorySlotUI[i].transform.Find("ItemIcon");
            if (itemIconTransform == null)
            {
                Debug.LogError("ItemIcon not found in inventory slot");
                continue;
            }

            Image itemImage = itemIconTransform.GetComponent<Image>();
            TextMeshProUGUI quantityText = inventorySlotUI[i].GetComponentInChildren<TextMeshProUGUI>();

            if (!slot.IsEmpty)
            {
                itemImage.sprite = slot.Item.Icon;
                itemImage.enabled = true; // Enable the image component if there's an item
                quantityText.text = slot.Quantity.ToString();
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false; // Disable the image component if there's no item
                quantityText.text = "";
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
