using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    public enum PanelType
    {
        Inventory,
        Hotbar
    }

    [Header("Layout Properties")]
    [SerializeField] private int columns = 4;
    [SerializeField] private Vector2 baseCellSize = new Vector2(100, 100);
    [SerializeField] private Vector2 basePadding = new Vector2(10, 10);
    [SerializeField] private Vector2 spacing = new Vector2(5, 5);

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
        gridLayout.padding = new RectOffset((int)basePadding.x, (int)basePadding.x, (int)basePadding.y, (int)basePadding.y);
        gridLayout.spacing = spacing;
    }

    void Start()
    {
        AdjustCellSize();
    }

    void AdjustCellSize()
    {
        Vector2 cellSize = baseCellSize; // Default to base cell size

        // Set the cell size
        gridLayout.cellSize = cellSize;

        // Adjust spacing based on the panel width and number of columns
        float panelWidth = rectTransform.rect.width - (gridLayout.padding.left + gridLayout.padding.right);
        float dynamicSpacing = (panelWidth - (cellSize.x * columns)) / Mathf.Max(1, columns - 1);
        gridLayout.spacing = new Vector2(dynamicSpacing, gridLayout.spacing.y);
    }
}
