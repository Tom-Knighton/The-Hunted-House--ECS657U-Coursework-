using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    public int columns = 6;
    public int rows = 3;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        AdjustCellSize();
    }

    void AdjustCellSize()
    {
        // Calculate the size of the panel (account for padding)
        float panelWidth = rectTransform.rect.width - (gridLayout.padding.left + gridLayout.padding.right);
        float panelHeight = rectTransform.rect.height - (gridLayout.padding.top + gridLayout.padding.bottom);

        // Calculate the size available for each cell (account for spacing)
        float widthAvailablePerCell = (panelWidth - gridLayout.spacing.x * (columns - 1)) / columns;
        float heightAvailablePerCell = (panelHeight - gridLayout.spacing.y * (rows - 1)) / rows;

        // Calculate the size for the cells based on the most restrictive dimension
        float cellSize = Mathf.Min(widthAvailablePerCell, heightAvailablePerCell);

        // If the calculated cell size based on width is smaller than the height, it means we have excess height
        if (cellSize * rows + gridLayout.spacing.y * (rows - 1) < panelHeight)
        {
            // Adjust the cell size based on the height instead
            cellSize = (panelHeight - gridLayout.spacing.y * (rows - 1)) / rows;
        }

        // Apply the cell size
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

         float dynamicSpacing = (panelWidth - (cellSize * columns)) / (columns - 1);
         gridLayout.spacing = new Vector2(dynamicSpacing, gridLayout.spacing.y);
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveGridLayout : MonoBehaviour
{
    public int columns = 6;
    public int rows = 3;

    private GridLayoutGroup gridLayout;
    private RectTransform rectTransform;

    void Awake()
    {
        gridLayout = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        AdjustCellSize();
    }

    void AdjustCellSize()
    {
        // Calculate the size of the panel (account for padding)
        float panelWidth = rectTransform.rect.width - (gridLayout.padding.left + gridLayout.padding.right);
        float panelHeight = rectTransform.rect.height - (gridLayout.padding.top + gridLayout.padding.bottom);

        // Calculate the size available for each cell (account for spacing)
        float widthAvailablePerCell = (panelWidth - gridLayout.spacing.x * (columns - 1)) / columns;
        float heightAvailablePerCell = (panelHeight - gridLayout.spacing.y * (rows - 1)) / rows;

        // Calculate the size for the cells based on the most restrictive dimension
        float cellSize = Mathf.Min(widthAvailablePerCell, heightAvailablePerCell);

        // If the calculated cell size based on width is smaller than the height, it means we have excess height
        if (cellSize * rows + gridLayout.spacing.y * (rows - 1) < panelHeight)
        {
            // Adjust the cell size based on the height instead
            cellSize = (panelHeight - gridLayout.spacing.y * (rows - 1)) / rows;
        }

        // Apply the cell size
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

         float dynamicSpacing = (panelWidth - (cellSize * columns)) / (columns - 1);
         gridLayout.spacing = new Vector2(dynamicSpacing, gridLayout.spacing.y);
    }
}
