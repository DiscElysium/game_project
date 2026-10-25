using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LiarAIStrategy
{
    // 配置：是否强制执行“点数只增不降”规则
    private static bool STRICT_FACE_INCREASE = true;

    // 定义战术类型
    public enum StrategyType
    {
        SafeFollow,     // 跟注：顺着上家的点数加1个 (求稳)
        Dominance,      // 压制：我手里这牌很多 (>=3)，强势叫牌
        Pivot,          // 换牌：换成我手里比较多的另一个点数
        PureBluff,      // 纯诈唬：我手里一张没有 (或很少)，硬装
        Panic,          // 绝境：没牌可叫了，被迫的
        Trap            // 陷阱：(保留扩展) 故意示弱
    }

    public struct Decision
    {
        public string Action; // "BID" or "CHALLENGE"
        public int Quantity;
        public int Point;
        public StrategyType Strategy; // 本次决策采用的战术
        public string Reasoning;      // 给 LLM 的扮演提示
        public float Score;           // 内部评分 (越高越想选)
    }

    /// <summary>
    /// 核心决策入口
    /// </summary>
    // 【修改】核心决策入口：增加了 history 参数
    // 【修改】核心决策入口：增加了 history 参数
    /// <summary>
    /// 核心决策入口 (优化版：更聪明的质疑逻辑)
    /// </summary>
    public static Decision MakeDecision(
            List<int> myHand,
            int currentQty,
            int currentPoint,
            float riskTolerance,
            List<TurnLog> history,
            int specialDiceCount,
            int totalUnknownDice = 15) // 注意：外部传入的 totalUnknownDice 应该是 (场上总骰子 - 我的骰子)
    {
        Decision decision = new Decision();

        // 1. 统计手牌 (区分普通和特殊)
        Dictionary<int, int> myCounts = new Dictionary<int, int>();
        Dictionary<int, int> myNormalCounts = new Dictionary<int, int>();

        for (int i = 1; i <= 6; i++) { myCounts[i] = 0; myNormalCounts[i] = 0; }

        // 确定特殊骰子索引
        int firstSpecialIndex = myHand.Count - specialDiceCount;

        for (int i = 0; i < myHand.Count; i++)
        {
            int p = myHand[i];
            bool isSpecial = (i >= firstSpecialIndex);

            if (isSpecial) myCounts[p] += 3; // 进攻：我有特殊骰子算3个
            else { myCounts[p] += 1; myNormalCounts[p] += 1; }
        }

        // --- 构建记忆 ---
        Dictionary<int, float> inference = BuildMemoryFromHistory(history);

        // =================================================================================
        // 2. 更加聪明的质疑逻辑 (Smart Challenge Logic)
        // =================================================================================

        // 步骤 A: 收集基础数据
        // 我能贡献的数量（注意：质疑时，我的特殊骰子对上家无效，只能算普通骰子）
        int myContribution = myNormalCounts[currentPoint];
        // 上家声称需要的“场上其他骰子”数量
        int neededFromOthers = currentQty - myContribution;

        // 步骤 B: 绝杀判断 (Impossible Check)
        // 如果需要的数量 > 场上未知的总骰子数 + 上家可能拥有的特殊骰子加成(最多+2额外)
        // 解释：特殊骰子算3个，即比普通骰子多2个。如果 needed > unknown + 2，那就是纯物理不可能。
        if (currentQty > 0 && neededFromOthers > totalUnknownDice + 2)
        {
            decision.Action = "CHALLENGE";
            decision.Reasoning = $"【绝杀】这不可能！场上最多只有 {totalUnknownDice + myContribution + 2} 个 {currentPoint} (假设你有神器)，你却叫了 {currentQty}。";
            return decision;
        }

        // 步骤 C: 概率与期望值分析
        float currentProb = 1.0f;
        float expectedVal = 0f;

        if (currentQty > 0)
        {
            // C1. 计算期望值 (EV)
            // 场上未知骰子中，出现 currentPoint 的理论平均值 (假设1/6概率)
            // 如果上家有特殊骰子，期望值会整体抬高一点，但我们先算基础线
            float averageUnknownHits = totalUnknownDice * (1.0f / 6.0f);
            expectedVal = myContribution + averageUnknownHits;

            // C2. 计算“普通概率” (假设上家没有特殊骰子)
            float probNormal = (neededFromOthers <= 0) ? 1.0f : CalculateBinomialProbability(neededFromOthers, totalUnknownDice, 1.0 / 6.0);

            // C3. 计算“神器概率” (假设上家有特殊骰子，他自带3个，needed减少3个)
            // 注意：这里我们假设总未知骰子数量不变，只是命中率分布变了，为了简化，我们认为他肯定有3个命中
            int neededIfHeHasSpecial = neededFromOthers - 3;
            float probSpecial = (neededIfHeHasSpecial <= 0) ? 1.0f : CalculateBinomialProbability(neededIfHeHasSpecial, totalUnknownDice, 1.0 / 6.0);

            // C4. 综合概率合成
            // 上家拥有特殊骰子的先验概率是 1/6 (假设每人1个特殊骰子，且是随机点数)
            // 但如果上家叫得很凶，他拥有特殊骰子的后验概率会极大提升

            // 激进的 AI (Risk > 0.7) 会认为：“你肯定在诈唬，我不信你有神器” -> 偏向 probNormal
            // 保守的 AI (Risk < 0.3) 会认为：“宁可信其有” -> 偏向 probSpecial

            // 动态权重：如果叫牌数量远超期望值，AI 会开始怀疑他在诈唬 (降低对 Special 的信任)
            float deviation = currentQty - expectedVal; // 偏离度，比如期望3个，叫了8个，偏离5

            float trustSpecial = 0.25f; // 基础信任度
            if (riskTolerance > 0.6f) trustSpecial -= 0.1f; // 激进者不信邪
            if (deviation > 4) trustSpecial -= 0.1f; // 偏离太离谱，不信你有神器

            trustSpecial = Mathf.Clamp(trustSpecial, 0.05f, 0.5f);

            currentProb = (probNormal * (1.0f - trustSpecial)) + (probSpecial * trustSpecial);

            // C5. 记忆修正
            if (inference.ContainsKey(currentPoint))
            {
                // 如果场上大家都叫这个数，稍微增加一点可信度
                currentProb += inference[currentPoint] * 0.15f;
            }
        }

        // 步骤 D: 动态质疑阈值 (Dynamic Threshold)
        // 之前的逻辑是 Lerp(0.4, 0.1)，这太死了。
        // 新逻辑：
        // 1. 如果叫牌符合期望值 (Deviation <= 1)，很难质疑 (阈值极低)
        // 2. 如果叫牌严重偏离 (Deviation >= 4)，很容易质疑 (阈值极高)

        float baseThreshold = 0.2f;

        // 根据激进度调整基准
        if (riskTolerance > 0.8f) baseThreshold = 0.35f; // 疯狗：很容易质疑
        else if (riskTolerance < 0.3f) baseThreshold = 0.10f; // 乌龟：很难质疑

        // 根据偏离度调整
        float deviationCheck = currentQty - expectedVal;
        if (deviationCheck >= 3) baseThreshold += 0.15f; // 偏离3个以上，大幅提高质疑门槛(即要求更高的成功率才肯信)
        if (deviationCheck >= 5) baseThreshold += 0.30f; // 偏离5个以上，几乎必开

        // 步骤 E: 最终判定
        if (currentQty > 0 && currentProb < baseThreshold)
        {
            // 最后的犹豫：如果是超级保守派，且概率不是 0，给个小机会放过
            bool cowardSave = (riskTolerance < 0.2f && currentProb > 0.05f && UnityEngine.Random.value < 0.3f);

            if (!cowardSave)
            {
                decision.Action = "CHALLENGE";

                // 生成更聪明的理由
                string reason = "";
                if (deviationCheck > 4)
                    reason = $"太离谱了！理论平均只有 {expectedVal:F1} 个，你叫了 {currentQty}。我不信你有神器！";
                else if (currentProb < 0.05f)
                    reason = $"概率只有 {currentProb:P1}，这几乎是不可能的。";
                else
                    reason = $"我看不到 {currentPoint}，且场上概率太低 ({currentProb:P1})，值得一搏。";

                decision.Reasoning = reason;
                return decision;
            }
        }

        // =================================================================================
        // 3. 生成候选叫牌列表 (逻辑保持微调，配合新的参数)
        // =================================================================================
        List<Decision> candidates = new List<Decision>();
        int maxJump = (riskTolerance > 0.6f) ? 3 : (riskTolerance > 0.3f ? 2 : 1);

        for (int qDelta = 0; qDelta <= maxJump; qDelta++)
        {
            int checkQty = currentQty + qDelta;
            if (checkQty == 0) checkQty = 1;

            for (int p = 1; p <= 6; p++)
            {
                if (checkQty < currentQty) continue;
                if (checkQty == currentQty && p <= currentPoint) continue;
                if (STRICT_FACE_INCREASE && currentQty > 0 && p < currentPoint) continue;

                Decision candidate = new Decision();
                candidate.Action = "BID";
                candidate.Quantity = checkQty;
                candidate.Point = p;

                int myCount = myCounts[p]; // 进攻时，我的特殊骰子算3个
                int needed = checkQty - myCount;

                // 计算胜率时，仍然保守一点，用 15 个普通骰子算概率
                // 这里的 effectiveUnknownTrials 是指除了我之外的 3 个人 * 5 个骰子 (粗略估计)
                float successProb = (needed <= 0) ? 1.0f : CalculateBinomialProbability(needed, totalUnknownDice, 1.0 / 6.0);

                // 判定战术类型
                if (checkQty == currentQty + 1 && p == currentPoint) candidate.Strategy = StrategyType.SafeFollow;
                else if (myCount >= 3) candidate.Strategy = StrategyType.Dominance;
                else if (myCount <= 1 && successProb < 0.3f) candidate.Strategy = StrategyType.PureBluff;
                else if (p != currentPoint) candidate.Strategy = StrategyType.Pivot;
                else candidate.Strategy = StrategyType.SafeFollow;

                // 评分
                float baseScore = successProb * 100f;
                candidate.Score = CalculateScore(baseScore, candidate.Strategy, riskTolerance, myCount, qDelta, inference, p);

                if (myHand.Count > 0 && myHand[myHand.Count - 1] == p) candidate.Score += 10f; // 有神器加分

                candidate.Reasoning = GenerateReasoning(candidate.Strategy, myCount, checkQty, p, successProb);
                candidates.Add(candidate);
            }
        }

        // 4. 决策选择
        if (candidates.Count == 0)
        {
            decision.Action = "CHALLENGE";
            decision.Reasoning = "叫到顶了，无法加注。";
            return decision;
        }

        candidates.Sort((a, b) => b.Score.CompareTo(a.Score));

        // 轮盘赌选择 Top N
        int topN = (riskTolerance > 0.6f) ? Mathf.Min(4, candidates.Count) : Mathf.Min(2, candidates.Count);
        float totalScore = 0;
        for (int i = 0; i < topN; i++) totalScore += candidates[i].Score;
        float rnd = UnityEngine.Random.Range(0, totalScore);
        float currentSum = 0;
        Decision finalChoice = candidates[0];
        for (int i = 0; i < topN; i++)
        {
            currentSum += candidates[i].Score;
            if (rnd <= currentSum) { finalChoice = candidates[i]; break; }
        }

        return finalChoice;
    }

    // 【新增】辅助函数：从历史记录构建场上点数记忆
    // 返回的 Dictionary key 是点数(1-6)，value 是置信度(越高代表越多人叫过)
    private static Dictionary<int, float> BuildMemoryFromHistory(List<TurnLog> history)
    {
        Dictionary<int, float> inference = new Dictionary<int, float>();
        for (int i = 1; i <= 6; i++) inference[i] = 0f;

        if (history == null || history.Count == 0) return inference;

        // 权重衰减系数：越近的叫牌参考价值越大
        float weight = 1.0f;

        // 倒序遍历（从最近的一步开始往回看）
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var log = history[i];
            if (log.action == "BID" && !string.IsNullOrEmpty(log.details))
            {
                // 解析 "3 x 5" 或 "3个5" 格式
                // 假设 LiarGameManager 中存储格式为 "Quantity x Point"
                try
                {
                    // 简单的解析逻辑，兼容 'x', 'X', '个'
                    string[] parts = log.details.Split(new char[] { 'x', 'X', '个' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        int q = int.Parse(parts[0].Trim());
                        int p = int.Parse(parts[1].Trim());

                        if (p >= 1 && p <= 6)
                        {
                            // 核心逻辑：有人敢叫这个点数，说明场上有的概率增加
                            // 累加值 = (数量占比) * (时间权重)
                            inference[p] += (q / 10f) * weight;
                        }
                    }
                }
                catch { /* 忽略解析错误 */ }
            }

            // 每一条历史记录衰减 15%
            weight *= 0.85f;
            if (weight < 0.1f) break; // 太久远的忽略
        }
        return inference;
    }
    // 【修改】评分逻辑：增加了 inference 和 p 参数
    private static float CalculateScore(float baseProbScore, StrategyType type, float risk, int myCount, int jumpAmount, Dictionary<int, float> inference, int p)
    {
        float score = baseProbScore;

        // --- 【新增策略】记忆权重分 ---
        // 如果历史记录中该点数活跃 (Inference高)，则给予加分
        float memoryTrust = 0f;
        if (inference.ContainsKey(p))
        {
            memoryTrust = inference[p] * 10f;
            score += memoryTrust;
        }

        // --- 【新增策略】"跟风者" (Sheep Strategy) ---
        // 如果我是保守派 (risk < 0.4)，且这个点数是场上最热的 (Inference最大)，我极度倾向于跟风
        if (risk < 0.4f)
        {
            float maxInference = inference.Values.Max();
            // 如果当前点数 p 的热度接近最大值，且我不是在诈唬
            if (inference[p] >= maxInference * 0.9f && type != StrategyType.PureBluff)
            {
                score += 35f; // 大幅奖励，促使保守NPC随大流
            }
        }

        // 原有的策略修正逻辑
        switch (type)
        {
            case StrategyType.Dominance:
                score += (myCount * 12f);
                if (jumpAmount > 0) score += 5f;
                break;

            case StrategyType.PureBluff:
                if (risk > 0.7f) score += 40f;
                else if (risk < 0.3f) score -= 50f;

                // 诈唬修正：如果场上没人叫过这个点 (Inference低)，诈唬成功率可能高（没人注意），也可能低（容易被抓）
                // 激进派觉得这是机会
                if (risk > 0.8f && (!inference.ContainsKey(p) || inference[p] < 0.1f))
                    score += 15f; // "出其不意"加分

                score += UnityEngine.Random.Range(0, 15f);
                break;

            case StrategyType.SafeFollow:
                if (risk < 0.4f) score += 20f;
                break;

            case StrategyType.Pivot:
                // 换花色时，如果换到了一个记忆中很热的点数，额外加分
                if (inference.ContainsKey(p) && inference[p] > 0.5f)
                    score += 10f;

                score += (myCount * 5f);
                break;
        }

        if (myCount == 0 && jumpAmount >= 2 && risk > 0.8f)
        {
            score += 25f;
        }

        return Mathf.Max(0, score);
    }// --- 辅助方法：生成有剧情感的理由 ---
    private static string GenerateReasoning(StrategyType type, int myCount, int q, int p, float prob)
    {
        string probStr = $"{prob:P0}";

        switch (type)
        {
            case StrategyType.Dominance:
                return $"我有 {myCount} 个 {p}，这是绝对优势(Dominance)。概率 {probStr}。我要以此施压。";

            case StrategyType.PureBluff:
                return $"我手里【一个 {p} 都没有】，但我决定诈唬(Pure Bluff)！我要假装我很自信。";

            case StrategyType.SafeFollow:
                return $"求稳策略(Safe)。上家叫了，我手里有 {myCount} 个，跟一注比较安全。";

            case StrategyType.Pivot:
                return $"我想换个花色(Pivot)。转到 {p}，因为我手里有 {myCount} 个，这比原来的点数更安全。";

            default:
                return $"综合考量，叫 {q} 个 {p}。";
        }
    }

    // (保留之前的数学计算函数 CalculateBinomialProbability 等)
    private static float CalculateBinomialProbability(int k, int n, double p)
    {
        if (k > n) return 0f;
        if (k <= 0) return 1f;
        double totalProb = 0;
        for (int i = k; i <= n; i++)
        {
            totalProb += Combinations(n, i) * System.Math.Pow(p, i) * System.Math.Pow(1 - p, n - i);
        }
        return (float)totalProb;
    }

    private static double Combinations(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        if (k == 0 || k == n) return 1;
        if (k > n / 2) k = n - k;
        double res = 1;
        for (int i = 1; i <= k; i++) res = res * (n - i + 1) / i;
        return res;
    }
}