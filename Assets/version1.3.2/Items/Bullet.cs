using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, ISlotSubscriber
{
    [Header("子弹属性")]
    public Vector2 direction;  // 移动方向
    public float speed = 10f;  // 移动速度
    public float lifetime = 2f; // 存在时间

    [Header("渲染设置")]
    public string sortingLayerName = "Bullet"; // Sorting Layer名称

    [Header("格子关联")]
    public BackpackSlot originSlot; // 发射该子弹的格子

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // 获取或添加SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // 设置Sorting Layer和Order
        if (!string.IsNullOrEmpty(sortingLayerName))
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
        }

        // 设置子弹在指定时间后自动销毁
        Destroy(gameObject, lifetime);

        // 设置子弹朝向
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void Update()
    {
        // 移动子弹
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // 可选：添加碰撞检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 这里可以添加碰撞逻辑，例如伤害敌人等
        // Debug.Log($"子弹击中: {other.gameObject.name}");

        // 击中后销毁子弹
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (originSlot != null)
        {
            originSlot.Unsubscribe(this);
        }
    }


    public void TriggerChildSlotEffects(Vector3 position)
    {
        // 启动协程来处理带间隔的触发
        StartCoroutine(TriggerChildSlotEffectsWithDelay(position));
    }

    private IEnumerator TriggerChildSlotEffectsWithDelay(Vector3 position)
    {
        if (originSlot != null && originSlot.connectedSlots.Count > 0)
        {
            foreach (BackpackSlot childSlot in originSlot.connectedSlots)
            {
                if (childSlot != null && childSlot.item != null)
                {
                    childSlot.item.TriggerEffect(transform.position);
                    Debug.Log($"子弹触发子物品效果: {childSlot.item.gameObject.name}");

                    // 添加0.25秒的延迟
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}