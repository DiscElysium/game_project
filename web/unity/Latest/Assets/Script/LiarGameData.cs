using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public int currentTurnIndex;
    public string currentBidPlayer;
    public int currentBidQuantity;
    public int currentBidPoint;

    // �޸���ʷ��¼�ṹ������������Ϣ
    public List<TurnLog> detailedLog = new List<TurnLog>();
}

[Serializable]
public class TurnLog
{
    public string playerName;
    public string action; // "BID" or "CHALLENGE"
    public string details; // "3��5"
    public string chatContent; // ���˵�Ļ� (�����NPC��Ϊ��)
}
// �� LiarGameData.cs �� Strategy ������
public class TableMemory
{
    // ��¼ÿ����������ҡ����족�����ۻ�ǿ��
    // ���ǵ��� (1-6)��ֵ��Ȩ��
    public Dictionary<int, float> pointConfidence = new Dictionary<int, float>();

    public TableMemory()
    {
        for (int i = 1; i <= 6; i++) pointConfidence[i] = 0f;
    }

    // ÿһ�����˽���ʱ�����¼���
    public void UpdateMemory(int point, int quantity, int myCount)
    {
        // �߼���������˽���ĳ�������������������ӣ�˵�����ϸõ�����ĸ��������
        // Ȩ�ع�ʽ��(�������� / ��������) �ļ�Ȩ
        pointConfidence[point] += (quantity / 20f);
    }
}
[Serializable]
public class NPCAnalysisResult
{
    public string dialogue;
    // ԭ���ֶ� (���ģʽʹ��)
    public float risk_factor;    // 0.0 (���ȱ���) ~ 1.0 (���ȼ���/����ͽ)
    public string analysis_summary; // ��̵ķ�����AI����OS�����ڵ��ԣ�
    public string micro_expression; // ����

    // �����ֶ� (��LLMģʽʹ��)
    public string action;        // "BID" or "CHALLENGE"
    public int bid_quantity;     // ��������
    public int bid_point;        // ���Ƶ���
}
