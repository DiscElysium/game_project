using UnityEngine;

/// <summary>
/// 六边形单元格组件，用于管理单个格子的属性
/// </summary>
public class HexCell : MonoBehaviour
{
    [Header("基本属性")]
    public int Row;          // 行坐标
    public int Column;       // 列坐标
    public bool IsWalkable = true;  // 是否可通行
    public int MovementCost = 1;    // 移动消耗

    [Header("视觉元素")]
    public SpriteRenderer spriteRenderer; // 可选：用于改变外观

    /// <summary>
    /// 初始化单元格
    /// </summary>
    public void Initialize(int row, int col)
    {
        Row = row;
        Column = col;
        name = $"Hex_{row}_{col}";

        // 这里可以添加更多初始化逻辑
        // 例如根据坐标设置不同属性
    }

    /// <summary>
    /// 设置格子是否可通行
    /// </summary>
    public void SetWalkable(bool walkable)
    {
        IsWalkable = walkable;
        UpdateAppearance();
    }

    /// <summary>
    /// 更新格子外观（根据属性）
    /// </summary>
    private void UpdateAppearance()
    {
        if (spriteRenderer != null)
        {
            // 示例：根据通行性改变颜色
            spriteRenderer.color = IsWalkable ? Color.white : Color.gray;
        }
    }
    public void SetVisible(bool visible)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = visible;
        }

        // 如果有子对象的渲染器也需要隐藏
        foreach (Transform child in transform)
        {
            SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
            if (childRenderer != null)
            {
                childRenderer.enabled = visible;
            }
        }
    }
    // 添加其他自定义方法和属性
}