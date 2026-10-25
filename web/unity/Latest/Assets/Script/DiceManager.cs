using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // �������������Ϊ��ʹ�� Action
public class DiceManager : MonoBehaviour
{
    [Header("�����з�����")]
    public DiceSpawner[] spawners; // ���������4��Spawner�Ͻ�ȥ

    [Header("״̬")]
    public bool isRolling = false; // �Ƿ�����Ͷ��������

    public int[] pointCounts = new int[7];
    public int grandTotal = 0;
    void Update()
    {
        // ȫ�ֿ��ƣ����¿ո�������з�����һ����
        //if (Input.GetKeyDown(KeyCode.Space) && !isRolling)
        //{
        //    StartCoroutine(RollAndCalculate());
        //}
    }
    public List<int> GetPlayerDice(int playerIndex)
    {
        List<int> results = new List<int>();

        // �������ƣ���ֹԽ��
        if (playerIndex < 0 || playerIndex >= spawners.Length)
            return results;

        // ��ȡ��Ӧ������������
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
        Debug.Log("=== ��ʼ��һ��Ͷ�� ===");

        // 1. �������� Spawner ����
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                // ���ﴫ�� 5���������� 4�� + 1��
                // ����Ժ����ˡ�������˿����ӡ��Ĺ��ܣ����������ﴫ�����
                spawner.ThrowDice(6);
            }
        }

        // 2. �ȴ����ɽ��� (�ȴ� 5������ * ��� + ���⻺��)
        yield return new WaitForSeconds(1.5f);

        // 3. ��̬�ȴ���ֱ���������Ӷ���ֹ
        bool allStopped = false;
        float timeOut = 10f; // 10�볬ʱ��ֹ��ѭ��
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
            yield return null; // �ȴ���һ֡�ٴμ��
        }

        // 4. ͳ�ƽ��
        CollectAndPrintResults();

        isRolling = false;
    }

    public IEnumerator RollAndCalculate(Action onRollingFinished = null)
    {
        isRolling = true;
        pointCounts = new int[7]; grandTotal = 0;
        Debug.Log("=== ��ʼ��һ��Ͷ�� ===");

        // 1. �������� Spawner ����
        foreach (var spawner in spawners)
        {
            if (spawner != null) spawner.ThrowDice();
        }

        // 2. �ȴ����ɽ���
        yield return new WaitForSeconds(1.5f);

        // 3. ��̬�ȴ���ֱ���������Ӷ���ֹ
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

        // 4. ͳ�ƽ��
        CollectAndPrintResults();

        isRolling = false;

        // ���ؼ����������������ˣ�ִ�лص�֪ͨ GameManager
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
