using System;
using System.Collections.Generic;

[Serializable]
public class GameState
{
    public int currentTurnIndex;
    public string currentBidPlayer;
    public int currentBidQuantity;
    public int currentBidPoint;

    // 修改历史记录结构，包含更多信息
    public List<TurnLog> detailedLog = new List<TurnLog>();
}

[Serializable]
public class TurnLog
{
    public string playerName;
    public string action; // "BID" or "CHALLENGE"
    public string details; // "3个5"
    public string chatContent; // 玩家说的话 (如果是NPC则为空)
}
// 在 LiarGameData.cs 或 Strategy 中添加
public class TableMemory
{
    // 记录每个点数被玩家“认领”过的累积强度
    // 键是点数 (1-6)，值是权重
    public Dictionary<int, float> pointConfidence = new Dictionary<int, float>();

    public TableMemory()
    {
        for (int i = 1; i <= 6; i++) pointConfidence[i] = 0f;
    }

    // 每一轮有人叫牌时，更新记忆
    public void UpdateMemory(int point, int quantity, int myCount)
    {
        // 逻辑：如果有人叫了某个点数，且数量在增加，说明场上该点数多的概率在提高
        // 权重公式：(叫牌数量 / 总骰子数) 的加权
        pointConfidence[point] += (quantity / 20f);
    }
}
[Serializable]
public class NPCAnalysisResult
{
    public string dialogue;
    // 原有字段 (混合模式使用)
    public float risk_factor;    // 0.0 (极度保守) ~ 1.0 (极度激进/亡命徒)
    public string analysis_summary; // 简短的分析（AI内心OS，用于调试）
    public string micro_expression; // 表情

    // 新增字段 (纯LLM模式使用)
    public string action;        // "BID" or "CHALLENGE"
    public int bid_quantity;     // 叫牌数量
    public int bid_point;        // 叫牌点数
}
