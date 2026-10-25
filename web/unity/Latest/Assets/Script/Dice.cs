using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    public int currentPoints = 0; // �洢��ǰ����
    public bool isStopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // ��һ����ʼ�������ת����֤���ʱ�������
        transform.rotation = Random.rotation;
    }

    void Update()
    {
        // ֻ�е������ٶȷǳ���ʱ���ż������
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
        // �����׼���Ӳ��֣�
        // local up (+y) = 1
        // local down (-y) = 6
        // local forward (+z) = 2
        // local back (-z) = 5
        // local right (+x) = 3
        // local left (-x) = 4

        // ��ȡ���Ӹ�����ķ�������
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

        // ѭ���Ƚ��ĸ�����ӽ���������ġ��ϡ� (Vector3.up)
        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(directions[i], Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                sideIndex = i;
            }
        }

        // ��������+1 ��Ϊ���� (0->1, 1->2, ...)
        currentPoints = sideIndex + 1;
    }
}