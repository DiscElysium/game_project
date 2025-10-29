using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    [Header("网格设置")]
    public int gridWidth = 5;      // 网格宽度（列数）
    public int gridHeight = 5;     // 网格高度（行数）
    public float hexRadius = 1f;   // 六边形外接圆半径
    public GameObject hexPrefab;   // 六边形预制体

    [Header("调试")]
    public bool showGridInfo = true; // 是否在控制台显示网格信息

    // 存储所有HexCell引用的二维数组
    public HexCell[,] hexGrid { get; private set; }

    private void Start()
    {
        GenerateHexGrid();
    }

    /// <summary>
    /// 在XY平面上生成六边形网格
    /// </summary>
    void GenerateHexGrid()
    {
        // 初始化二维数组
        hexGrid = new HexCell[gridHeight, gridWidth];

        // 计算六边形尺寸（XY平面）
        float hexWidth = Mathf.Sqrt(3) * hexRadius;    // 六边形宽度
        float hexHeight = 1.5f * hexRadius;            // 六边形高度（行间距）
        float horizontalOffset = hexWidth;             // 水平间距
        float verticalOffset = hexHeight;              // 垂直间距

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                // 计算XY平面上的位置
                float xPos = col * horizontalOffset;
                float yPos = row * verticalOffset;

                // 每行都向右偏移半个六边形宽度
                xPos -= hexWidth * 0.5f * row;

                // 在XY平面上创建位置
                Vector3 position = new Vector3(xPos, yPos, 0f);

                // 实例化六边形
                GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity, transform);

                // 获取或添加HexCell组件
                HexCell hexCell = hex.GetComponent<HexCell>();
                if (hexCell == null)
                {
                    hexCell = hex.AddComponent<HexCell>();
                }

                SpriteRenderer sr = hex.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = hex.AddComponent<SpriteRenderer>(); // 如果没有，添加一个，但通常hexPrefab应该有
                }

                // 设置Sorting Layer和Order in Layer
                sr.sortingLayerName = "Map"; // 使用你创建的Sorting Layer名称
                sr.sortingOrder = 0; // 地图单元格的Order in Layer设为0

                // 初始化单元格
                hexCell.Initialize(row, col);

                // 存储到二维数组
                hexGrid[row, col] = hexCell;
            }
        }

        // 调试：打印网格信息
        if (showGridInfo)
        {
            Debug.Log($"已生成六边形网格: {gridHeight}行 × {gridWidth}列");
            Debug.Log($"总单元格数: {gridHeight * gridWidth}");
        }
    }

    /// <summary>
    /// 根据行列坐标获取HexCell
    /// </summary>
    public HexCell GetHexCell(int row, int col)
    {
        if (row >= 0 && row < gridHeight && col >= 0 && col < gridWidth)
        {
            return hexGrid[row, col];
        }
        return null;
    }

    /// <summary>
    /// 获取相邻的HexCell（6个方向）
    /// </summary>
    public HexCell[] GetNeighbors(int row, int col)
    {
        HexCell[] neighbors = new HexCell[6];

        // 尖顶六边形的相邻坐标偏移（偶数行和奇数行偏移不同）
        int[,] offsets =
        {
            // 偶数行偏移
            { 0, -1 }, { 1, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { -1, -1 },
            // 奇数行偏移
            { 0, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }
        };

        int offsetSet = row % 2; // 0=偶数行，1=奇数行

        for (int i = 0; i < 6; i++)
        {
            int neighborRow = row + offsets[offsetSet * 6 + i, 0];
            int neighborCol = col + offsets[offsetSet * 6 + i, 1];
            neighbors[i] = GetHexCell(neighborRow, neighborCol);
        }

        return neighbors;
    }
    public Vector2Int GetCenterPosition()
    {
        // 计算中心行和列（基于网格尺寸）
        int centerRow = gridHeight / 2;
        int centerCol = gridWidth / 2;
        return new Vector2Int(centerRow, centerCol);
    }
}