using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    public int currentPoints = 0; // 存储当前点数
    public bool isStopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 给一个初始的随机旋转，保证落地时的随机性
        transform.rotation = Random.rotation;
    }

    void Update()
    {
        // 只有当骰子速度非常慢时，才计算点数
        if (rb.velocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
        {
            isStopped = true;
            CalculateUpFace();
        }
        else
        {
            isStopped = false;
        }
    }

    void CalculateUpFace()
    {
        // 假设标准骰子布局：
        // local up (+y) = 1
        // local down (-y) = 6
        // local forward (+z) = 2
        // local back (-z) = 5
        // local right (+x) = 3
        // local left (-x) = 4

        // 获取骰子各个面的方向向量
        Vector3[] directions = new Vector3[] {
            transform.up,           // 1
            transform.forward,      // 2
            transform.right,        // 3
            -transform.right,       // 4
            -transform.forward,     // 5
            -transform.up           // 6
        };

        float maxDot = -1.0f;
        int sideIndex = 0;

        // 循环比较哪个面最接近世界坐标的“上” (Vector3.up)
        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(directions[i], Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                sideIndex = i;
            }
        }

        // 数组索引+1 即为点数 (0->1, 1->2, ...)
        currentPoints = sideIndex + 1;
    }
}