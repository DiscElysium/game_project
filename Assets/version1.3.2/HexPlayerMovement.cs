using UnityEngine;
using System.Collections;

public class HexPlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;           // 移动速度
    public float moveAnimationTime = 0.3f; // 移动动画时间
    public float moveRepeatDelay = 0.2f;   // 移动重复延迟
    public float moveCooldown = 0.5f;      // 移动冷却时间

    [Header("引用")]
    public HexGridGenerator gridGenerator; // 六边形网格生成器
    public Camera followCamera;            // 跟随相机
    public float cameraFollowSpeed = 5f;   // 相机跟随速度
    public Vector3 cameraOffset = new Vector3(0, 0, -10); // 相机偏移

    private HexCell currentCell;           // 当前所在的单元格
    private bool isMoving = false;         // 是否正在移动
    private float lastMoveTime;            // 最后一次移动时间
    private Vector2Int lastMoveDirection;  // 最后一次移动方向
    private float cooldownTimer;           // 冷却计时器
    private SpriteRenderer playerSpriteRenderer; // 玩家的SpriteRenderer

    private void Start()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.sortingLayerName = "Player";
            playerSpriteRenderer.sortingOrder = 1;
        }
        else
        {
            Debug.LogWarning("玩家没有SpriteRenderer，请确保有Sprite Renderer组件");
        }

        // 如果没有手动赋值，尝试查找HexGridGenerator
        if (gridGenerator == null)
        {
            gridGenerator = FindObjectOfType<HexGridGenerator>();
            if (gridGenerator == null)
            {
                Debug.LogError("找不到HexGridGenerator，请确保场景中有HexGridGenerator对象");
                return;
            }
        }

        // 等待网格生成完成
        StartCoroutine(InitializePlayerPosition());
    }

    // 等待网格生成完成后初始化玩家位置
    private IEnumerator InitializePlayerPosition()
    {
        // 等待一帧确保网格已生成
        yield return null;

        // 获取中心位置或默认位置
        Vector2Int centerPos = gridGenerator.GetCenterPosition();
        currentCell = gridGenerator.GetHexCell(centerPos.x, centerPos.y);

        if (currentCell != null)
        {
            transform.position = currentCell.transform.position;
            Debug.Log($"玩家初始位置: ({currentCell.Row}, {currentCell.Column})");
        }
        else
        {
            Debug.LogError("无法找到初始单元格，请检查网格设置");
        }

        cooldownTimer = 0f;
    }

    private void Update()
    {
        // 冷却时间处理
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // 移动状态检查
        if (isMoving) return;

        // 获取移动方向
        Vector2Int moveDirection = GetMoveDirection();

        if (moveDirection != Vector2Int.zero)
        {
            // 防止重复按键过快
            if (moveDirection != lastMoveDirection || Time.time - lastMoveTime >= moveRepeatDelay)
            {
                TryMoveTo(currentCell.Row + moveDirection.x, currentCell.Column + moveDirection.y);
                lastMoveDirection = moveDirection;
                lastMoveTime = Time.time;
            }
        }
        else
        {
            lastMoveDirection = Vector2Int.zero;
        }
    }

    private void LateUpdate()
    {
        // 相机跟随
        if (followCamera != null)
        {
            Vector3 targetPosition = transform.position + cameraOffset;
            followCamera.transform.position = Vector3.Lerp(
                followCamera.transform.position,
                targetPosition,
                cameraFollowSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// 获取当前输入的移动方向
    /// </summary>
    Vector2Int GetMoveDirection()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (Input.GetKey(KeyCode.D)) return new Vector2Int(1, 1);   // W+D
            if (Input.GetKey(KeyCode.A)) return new Vector2Int(1, 0);   // W+A
            return new Vector2Int(1, 1);                                // W
        }
        else if (Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.D)) return new Vector2Int(-1, 0);  // S+D
            if (Input.GetKey(KeyCode.A)) return new Vector2Int(-1, -1); // S+A
            return new Vector2Int(-1, -1);                              // S
        }
        else if (Input.GetKey(KeyCode.D)) return new Vector2Int(0, 1);  // D
        else if (Input.GetKey(KeyCode.A)) return new Vector2Int(0, -1); // A

        return Vector2Int.zero;
    }

    /// <summary>
    /// 尝试移动到指定位置
    /// </summary>
    void TryMoveTo(int targetRow, int targetCol)
    {
        if (gridGenerator == null) return;

        HexCell targetCell = gridGenerator.GetHexCell(targetRow, targetCol);

        // 检查目标单元格是否有效
        if (targetCell == null)
        {
            Debug.Log($"无法移动到 ({targetRow}, {targetCol}) - 超出地图边界");
            return;
        }

        // 检查目标单元格是否可通行
        if (!targetCell.IsWalkable)
        {
            Debug.Log($"无法移动到 ({targetRow}, {targetCol}) - 单元格不可通行");
            return;
        }

        // 开始移动
        StartCoroutine(MoveToCell(targetCell));
    }

    /// <summary>
    /// 移动到目标单元格的协程
    /// </summary>
    IEnumerator MoveToCell(HexCell targetCell)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = targetCell.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveAnimationTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveAnimationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保到达目标位置
        transform.position = endPos;

        // 更新当前单元格
        currentCell = targetCell;

        isMoving = false;
        cooldownTimer = moveCooldown;

        Debug.Log($"移动到 ({currentCell.Row}, {currentCell.Column})");
    }
}