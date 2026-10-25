using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceSpawner : MonoBehaviour
{
    [Header("核心设置")]
    public GameObject dicePrefab;        // 普通骰子预制体 (白)
    public GameObject specialDicePrefab; // 【新增】特殊骰子预制体 (红/金)

    public Transform spawnPoint;    // 发射起点
    public Transform targetPoint;   // 发射目标点

    [Header("参数调整")]
    public float spawnInterval = 0.2f; // 连发间隔
    public float throwForce = 15f;
    public float spreadAmount = 0.5f;

    public List<Dice> spawnedDice = new List<Dice>();

    void Update()
    {
        // 测试用
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartCoroutine(SpawnDiceSequence(5));
        //}
    }

    public bool AreAllDiceStopped
    {
        get
        {
            if (spawnedDice.Count == 0) return false;
            foreach (var d in spawnedDice)
            {
                if (!d.isStopped) return false;
            }
            return true;
        }
    }

    // 【修改】允许传入数量，默认为 5
    public void ThrowDice(int count = 5)
    {
        StartCoroutine(SpawnDiceSequence(count));
    }

    // 【修改】根据数量生成
    IEnumerator SpawnDiceSequence(int totalCount)
    {
        // 清理旧骰子
        foreach (var d in spawnedDice) { if (d != null) Destroy(d.gameObject); }
        spawnedDice.Clear();

        // 确保至少有1个骰子
        if (totalCount < 1) totalCount = 1;

        // 1. 先生成 (Total - 1) 个普通骰子
        for (int i = 0; i < totalCount - 1; i++)
        {
            SpawnSingleDice(dicePrefab); // 传入普通 Prefab
            yield return new WaitForSeconds(spawnInterval);
        }

        // 2. 【关键】最后生成 1 个特殊骰子
        // 这样它在 list 中的索引就是最后一个 (index == count-1)，符合 Manager 的结算逻辑
        if (specialDicePrefab != null)
        {
            SpawnSingleDice(specialDicePrefab);
        }
        else
        {
            // 如果忘了赋值特殊骰子，就用普通的顶替，防止报错
            SpawnSingleDice(dicePrefab);
        }
    }

    // 【修改】增加参数 prefabToSpawn
    void SpawnSingleDice(GameObject prefabToSpawn)
    {
        if (spawnPoint == null || targetPoint == null || prefabToSpawn == null)
        {
            Debug.LogError("请检查 Inspector 面板：SpawnPoint, TargetPoint 或 Prefab 未赋值！");
            return;
        }

        // 1. 生成指定的骰子 (普通 或 特殊)
        GameObject newDiceObj = Instantiate(prefabToSpawn, spawnPoint.position, Random.rotation);

        Dice diceScript = newDiceObj.GetComponent<Dice>();
        // 这里最好做个防呆，万一特殊骰子忘了挂 Dice 脚本
        if (diceScript == null) diceScript = newDiceObj.AddComponent<Dice>();

        Rigidbody rb = newDiceObj.GetComponent<Rigidbody>();
        spawnedDice.Add(diceScript);

        // 2. 计算方向
        Vector3 baseDirection = (targetPoint.position - spawnPoint.position).normalized;

        // 3. 散射
        Vector3 randomSpread = new Vector3(
            Random.Range(-spreadAmount, spreadAmount),
            Random.Range(-spreadAmount / 2, spreadAmount / 2),
            0
        );
        Vector3 finalDirection = baseDirection + spawnPoint.TransformDirection(randomSpread);

        // 4. 施加力
        rb.AddForce(finalDirection.normalized * throwForce, ForceMode.Impulse);

        // 5. 施加旋转
        rb.AddTorque(Random.insideUnitSphere * throwForce * 5f, ForceMode.Impulse);
    }

    void OnDrawGizmos()
    {
        if (spawnPoint != null && targetPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(spawnPoint.position, targetPoint.position);
            Gizmos.DrawWireSphere(targetPoint.position, 0.5f);
        }
    }
}