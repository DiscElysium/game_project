using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // 必须引用这个，为了使用 Action
public class DiceManager : MonoBehaviour
{
    [Header("绑定所有发射器")]
    public DiceSpawner[] spawners; // 在这里把那4个Spawner拖进去

    [Header("状态")]
    public bool isRolling = false; // 是否正在投掷过程中

    public int[] pointCounts = new int[7];
    public int grandTotal = 0;
    void Update()
    {
        // 全局控制：按下空格键，所有发射器一起发射
        //if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        //{
        //    StartCoroutine(RollAndCalculate());
        //}
    }
    public List<int> GetPlayerDice(int playerIndex)
    {
        List<int> results = new List<int>();

        // 保护机制：防止越界
        if (playerIndex < 0 || playerIndex >= spawners.Length)
            return results;

        // 获取对应发射器的骰子
        foreach (var dice in spawners[playerIndex].spawnedDice)
        {
            results.Add(dice.currentPoints);
        }
        return results;
    }
    public IEnumerator RollAndCalculate()
    {
        isRolling = true;
        pointCounts = new int[7]; grandTotal = 0;
        Debug.Log("=== 开始新一轮投掷 ===");

        // 1. 命令所有 Spawner 开火
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                // 这里传入 5，代表生成 4白 + 1红
                // 如果以后做了“玩家输了扣骰子”的功能，可以在这里传入变量
                spawner.ThrowDice(6);
            }
        }

        // 2. 等待生成结束 (等待 5个骰子 * 间隔 + 额外缓冲)
        yield return new WaitForSeconds(1.5f);

        // 3. 动态等待：直到所有骰子都静止
        bool allStopped = false;
        float timeOut = 10f; // 10秒超时防止死循环
        float timer = 0;

        while (!allStopped && timer < timeOut)
        {
            allStopped = true;
            foreach (var spawner in spawners)
            {
                if (!spawner.AreAllDiceStopped)
                {
                    allStopped = false;
                    break;
                }
            }
            timer += Time.deltaTime;
            yield return null; // 等待下一帧再次检查
        }

        // 4. 统计结果
        CollectAndPrintResults();

        isRolling = false;
    }

    public IEnumerator RollAndCalculate(Action onRollingFinished = null)
    {
        isRolling = true;
        pointCounts = new int[7]; grandTotal = 0;
        Debug.Log("=== 开始新一轮投掷 ===");

        // 1. 命令所有 Spawner 开火
        foreach (var spawner in spawners)
        {
            if (spawner != null) spawner.ThrowDice();
        }

        // 2. 等待生成结束
        yield return new WaitForSeconds(1.5f);

        // 3. 动态等待：直到所有骰子都静止
        bool allStopped = false;
        float timeOut = 10f;
        float timer = 0;

        while (!allStopped && timer < timeOut)
        {
            allStopped = true;
            foreach (var spawner in spawners)
            {
                if (!spawner.AreAllDiceStopped)
                {
                    allStopped = false;
                    break;
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. 统计结果
        CollectAndPrintResults();

        isRolling = false;

        // 【关键】所有事情做完了，执行回调通知 GameManager
        if (onRollingFinished != null)
        {
            onRollingFinished.Invoke();
        }
    }
    // Keep hand values out of player logs; they are private game state.
    void CollectAndPrintResults()
    {
        pointCounts = new int[7];
        grandTotal = 0;
        foreach (var spawner in spawners)
        {
            if (!spawner) continue;
            foreach (var die in spawner.spawnedDice)
            {
                int point = die.currentPoints;
                if (point >= 1 && point <= 6) { pointCounts[point]++; grandTotal += point; }
            }
        }
    }
}
