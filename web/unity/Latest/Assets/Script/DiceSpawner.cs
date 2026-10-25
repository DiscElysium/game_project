using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceSpawner : MonoBehaviour
{
    [Header("��������")]
    public GameObject dicePrefab;        // ��ͨ����Ԥ���� (��)
    public GameObject specialDicePrefab; // ����������������Ԥ���� (��/��)

    public Transform spawnPoint;    // �������
    public Transform targetPoint;   // ����Ŀ���

    [Header("��������")]
    public float spawnInterval = 0.2f; // �������
    public float throwForce = 15f;
    public float spreadAmount = 0.5f;

    public List<Dice> spawnedDice = new List<Dice>();

    void Update()
    {
        // ������
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

    // ���޸ġ���������������Ĭ��Ϊ 5
    public void ThrowDice(int count = 5)
    {
        StartCoroutine(SpawnDiceSequence(count));
    }

    // ���޸ġ�������������
    IEnumerator SpawnDiceSequence(int totalCount)
    {
        // ����������
        foreach (var d in spawnedDice) { if (d != null) Destroy(d.gameObject); }
        spawnedDice.Clear();

        // ȷ��������1������
        if (totalCount < 1) totalCount = 1;

        // 1. ������ (Total - 1) ����ͨ����
        for (int i = 0; i < totalCount - 1; i++)
        {
            SpawnSingleDice(dicePrefab); // ������ͨ Prefab
            yield return new WaitForSeconds(spawnInterval);
        }

        // 2. ���ؼ���������� 1 ����������
        // �������� list �е������������һ�� (index == count-1)������ Manager �Ľ����߼�
        if (specialDicePrefab != null)
        {
            SpawnSingleDice(specialDicePrefab);
        }
        else
        {
            // ������˸�ֵ�������ӣ�������ͨ�Ķ��棬��ֹ����
            SpawnSingleDice(dicePrefab);
        }
    }

    // ���޸ġ����Ӳ��� prefabToSpawn
    void SpawnSingleDice(GameObject prefabToSpawn)
    {
        if (spawnPoint == null || targetPoint == null || prefabToSpawn == null)
        {
            Debug.LogError("���� Inspector ��壺SpawnPoint, TargetPoint �� Prefab δ��ֵ��");
            return;
        }

        // 1. ����ָ�������� (��ͨ �� ����)
        GameObject newDiceObj = Instantiate(prefabToSpawn, spawnPoint.position, Random.rotation);

        Dice diceScript = newDiceObj.GetComponent<Dice>();
        // �������������������һ�����������˹� Dice �ű�
        if (diceScript == null) diceScript = newDiceObj.AddComponent<Dice>();

        Rigidbody rb = newDiceObj.GetComponent<Rigidbody>();
        spawnedDice.Add(diceScript);

        // 2. ���㷽��
        Vector3 baseDirection = (targetPoint.position - spawnPoint.position).normalized;

        // 3. ɢ��
        Vector3 randomSpread = new Vector3(
            Random.Range(-spreadAmount, spreadAmount),
            Random.Range(-spreadAmount / 2, spreadAmount / 2),
            0
        );
        Vector3 finalDirection = baseDirection + spawnPoint.TransformDirection(randomSpread);

        // 4. ʩ����
        rb.AddForce(finalDirection.normalized * throwForce, ForceMode.Impulse);

        // 5. ʩ����ת
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