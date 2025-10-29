using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunItem : ItemComponent
{
    [Header("子弹设置")]
    public GameObject bulletPrefab;    // 子弹预制体
    public float bulletSpeed = 10f;    // 子弹速度
    public float bulletDuration = 2f;  // 子弹持续时间

    [Header("散射设置")]
    public int bulletCount = 1;        // 每次发射子弹数量
    public float scatterAngle = 0f;    // 散射角度（度）
    public bool symmetricalScatter = true; // 是否对称散射

    private GameObject bullet;
    private GunItem Fatheritem;

    // 玩家位置参考
    public Transform playerTransform;

    // 当前物品所在的格子
    private BackpackSlot currentSlot;

    private void Start()
    {
        // 初始化
        // 获取当前物品所在的格子
        currentSlot = GetComponentInParent<BackpackSlot>();
    }

    // 修改TriggerEffect方法，接受位置参数
    public override void TriggerEffect(Vector3 position)
    {
        if (Fatheritem != null)
        {
            // 如果存在父物品，可能需要进行特殊处理，这里暂时忽略
            // playerTransform.position = Fatheritem.getBulletTransform();
        }
        Debug.Log($"物品效果触发 - GameObject: {gameObject.name} 在位置 {position}");

        // 如果传递的位置是零向量，则使用玩家位置作为后备
        if (position == Vector3.zero)
        {
            if (playerTransform == null)
            {
                Debug.LogError("玩家位置为空，无法发射子弹");
                return;
            }
            position = playerTransform.position;
        }

        // 获取鼠标位置
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // 计算基础方向
        Vector2 baseDirection = (mousePosition - position).normalized; // 使用传递的位置

        // 根据子弹数量选择发射模式
        if (bulletCount == 1)
        {
            // 单发模式
            FireBullet(baseDirection, position);
        }
        else
        {
            // 散射模式
            FireScatterBullets(baseDirection, position);
        }
    }

    public void setFatherItem(GunItem faitem)
    {
        Fatheritem = faitem;
    }

    // 修改FireBullet方法，接受发射位置
    private void FireBullet(Vector2 direction, Vector3 firePosition)
    {
        bullet = Instantiate(bulletPrefab, firePosition, Quaternion.identity);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.direction = direction;
            bulletComponent.speed = bulletSpeed;
            bulletComponent.lifetime = bulletDuration;
            currentSlot = GetComponentInParent<BackpackSlot>();
            bulletComponent.originSlot = currentSlot; // 设置子弹的发射格子
            currentSlot.Subscribe(bulletComponent);
        }
    }

    // 修改FireScatterBullets方法，接受发射位置
    private void FireScatterBullets(Vector2 baseDirection, Vector3 firePosition)
    {
        float angleStep = 0f;
        float startAngle = 0f;

        if (symmetricalScatter)
        {
            // 对称散射：所有子弹均匀分布在中心线两侧
            angleStep = scatterAngle / (bulletCount - 1);
            startAngle = -scatterAngle / 2f;
        }
        else
        {
            // 随机散射：每颗子弹随机偏移
        }

        for (int i = 0; i < bulletCount; i++)
        {
            Vector2 bulletDirection;

            if (symmetricalScatter)
            {
                // 对称散射的角度计算
                float currentAngle = startAngle + (angleStep * i);
                bulletDirection = RotateVector(baseDirection, currentAngle);
            }
            else
            {
                // 随机散射的角度计算
                float randomAngle = Random.Range(-scatterAngle / 2f, scatterAngle / 2f);
                bulletDirection = RotateVector(baseDirection, randomAngle);
            }

            FireBullet(bulletDirection, firePosition);
        }
    }

    // 向量旋转
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    // 获取子弹位置（用于父物品调用）
    public Vector3 getBulletTransform()
    {
        if (bullet != null)
        {
            return bullet.transform.position;
        }
        return playerTransform.position;
    }
}