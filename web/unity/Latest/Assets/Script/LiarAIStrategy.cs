using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LiarAIStrategy
{
    // ���ã��Ƿ�ǿ��ִ�С�����ֻ������������
    private static bool STRICT_FACE_INCREASE = true;

    // ����ս������
    public enum StrategyType
    {
        SafeFollow,     // ��ע��˳���ϼҵĵ�����1�� (����)
        Dominance,      // ѹ�ƣ����������ƺܶ� (>=3)��ǿ�ƽ���
        Pivot,          // ���ƣ�����������Ƚ϶����һ������
        PureBluff,      // ��թ����������һ��û�� (�����)��Ӳװ
        Panic,          // ������û�ƿɽ��ˣ����ȵ�
        Trap            // ���壺(������չ) ����ʾ��
    }

    public struct Decision
    {
        public string Action; // "BID" or "CHALLENGE"
        public int Quantity;
        public int Point;
        public StrategyType Strategy; // ���ξ��߲��õ�ս��
        public string Reasoning;      // �� LLM �İ�����ʾ
        public float Score;           // �ڲ����� (Խ��Խ��ѡ)
    }

    /// <summary>
    /// ���ľ������
    /// </summary>
    // ���޸ġ����ľ�����ڣ������� history ����
    // ���޸ġ����ľ�����ڣ������� history ����
    /// <summary>
    /// ���ľ������ (�Ż��棺�������������߼�)
    /// </summary>
    public static Decision MakeDecision(
            List<int> myHand,
            int currentQty,
            int currentPoint,
            float riskTolerance,
            List<TurnLog> history,
            int specialDiceCount,
            int totalUnknownDice = 15) // ע�⣺�ⲿ����� totalUnknownDice Ӧ���� (���������� - �ҵ�����)
    {
        Decision decision = new Decision();

        // 1. ͳ������ (������ͨ������)
        Dictionary<int, int> myCounts = new Dictionary<int, int>();
        Dictionary<int, int> myNormalCounts = new Dictionary<int, int>();

        for (int i = 1; i <= 6; i++) { myCounts[i] = 0; myNormalCounts[i] = 0; }

        // ȷ��������������
        int firstSpecialIndex = myHand.Count - specialDiceCount;

        for (int i = 0; i < myHand.Count; i++)
        {
            int p = myHand[i];
            bool isSpecial = (i >= firstSpecialIndex);

            if (isSpecial) myCounts[p] += 3; // ��������������������3��
            else { myCounts[p] += 1; myNormalCounts[p] += 1; }
        }

        // --- �������� ---
        Dictionary<int, float> inference = BuildMemoryFromHistory(history);

        // =================================================================================
        // 2. ���Ӵ����������߼� (Smart Challenge Logic)
        // =================================================================================

        // ���� A: �ռ���������
        // ���ܹ��׵�������ע�⣺����ʱ���ҵ��������Ӷ��ϼ���Ч��ֻ������ͨ���ӣ�
        int myContribution = myNormalCounts[currentPoint];
        // �ϼ�������Ҫ�ġ������������ӡ�����
        int neededFromOthers = currentQty - myContribution;

        // ���� B: ��ɱ�ж� (Impossible Check)
        // �����Ҫ������ > ����δ֪���������� + �ϼҿ���ӵ�е��������Ӽӳ�(���+2����)
        // ���ͣ�����������3����������ͨ���Ӷ�2������� needed > unknown + 2���Ǿ��Ǵ����������ܡ�
        if (currentQty > 0 && neededFromOthers > totalUnknownDice + 2)
        {
            decision.Action = "CHALLENGE";
            decision.Reasoning = $"����ɱ���ⲻ���ܣ��������ֻ�� {totalUnknownDice + myContribution + 2} �� {currentPoint} (������������)����ȴ���� {currentQty}��";
            return decision;
        }

        // ���� C: ����������ֵ����
        float currentProb = 1.0f;
        float expectedVal = 0f;

        if (currentQty > 0)
        {
            // C1. ��������ֵ (EV)
            // ����δ֪�����У����� currentPoint ������ƽ��ֵ (����1/6����)
            // ����ϼ����������ӣ�����ֵ������̧��һ�㣬���������������
            float averageUnknownHits = totalUnknownDice * (1.0f / 6.0f);
            expectedVal = myContribution + averageUnknownHits;

            // C2. ���㡰��ͨ���ʡ� (�����ϼ�û����������)
            float probNormal = (neededFromOthers <= 0) ? 1.0f : CalculateBinomialProbability(neededFromOthers, totalUnknownDice, 1.0 / 6.0);

            // C3. ���㡰�������ʡ� (�����ϼ����������ӣ����Դ�3����needed����3��)
            // ע�⣺�������Ǽ�����δ֪�����������䣬ֻ�������ʷֲ����ˣ�Ϊ�˼򻯣�������Ϊ���϶���3������
            int neededIfHeHasSpecial = neededFromOthers - 3;
            float probSpecial = (neededIfHeHasSpecial <= 0) ? 1.0f : CalculateBinomialProbability(neededIfHeHasSpecial, totalUnknownDice, 1.0 / 6.0);

            // C4. �ۺϸ��ʺϳ�
            // �ϼ�ӵ���������ӵ���������� 1/6 (����ÿ��1���������ӣ������������)
            // ������ϼҽеú��ף���ӵ���������ӵĺ�����ʻἫ������

            // ������ AI (Risk > 0.7) ����Ϊ������϶���թ�����Ҳ������������� -> ƫ�� probNormal
            // ���ص� AI (Risk < 0.3) ����Ϊ�������������С� -> ƫ�� probSpecial

            // ��̬Ȩ�أ������������Զ������ֵ��AI �Ὺʼ��������թ�� (���Ͷ� Special ������)
            float deviation = currentQty - expectedVal; // ƫ��ȣ���������3��������8����ƫ��5

            float trustSpecial = 0.25f; // �������ζ�
            if (riskTolerance > 0.6f) trustSpecial -= 0.1f; // �����߲���а
            if (deviation > 4) trustSpecial -= 0.1f; // ƫ��̫���ף�������������

            trustSpecial = Mathf.Clamp(trustSpecial, 0.05f, 0.5f);

            currentProb = (probNormal * (1.0f - trustSpecial)) + (probSpecial * trustSpecial);

            // C5. ��������
            if (inference.ContainsKey(currentPoint))
            {
                // ������ϴ�Ҷ������������΢����һ����Ŷ�
                currentProb += inference[currentPoint] * 0.15f;
            }
        }

        // ���� D: ��̬������ֵ (Dynamic Threshold)
        // ֮ǰ���߼��� Lerp(0.4, 0.1)����̫���ˡ�
        // ���߼���
        // 1. ������Ʒ�������ֵ (Deviation <= 1)���������� (��ֵ����)
        // 2. �����������ƫ�� (Deviation >= 4)������������ (��ֵ����)

        float baseThreshold = 0.2f;

        // ���ݼ����ȵ�����׼
        if (riskTolerance > 0.8f) baseThreshold = 0.35f; // �蹷������������
        else if (riskTolerance < 0.3f) baseThreshold = 0.10f; // �ڹ꣺��������

        // ����ƫ��ȵ���
        float deviationCheck = currentQty - expectedVal;
        if (deviationCheck >= 3) baseThreshold += 0.15f; // ƫ��3�����ϣ������������ż�(��Ҫ����ߵĳɹ��ʲſ���)
        if (deviationCheck >= 5) baseThreshold += 0.30f; // ƫ��5�����ϣ������ؿ�

        // ���� E: �����ж�
        if (currentQty > 0 && currentProb < baseThreshold)
        {
            // ������ԥ������ǳ��������ɣ��Ҹ��ʲ��� 0������С����Ź�
            bool cowardSave = (riskTolerance < 0.2f && currentProb > 0.05f && UnityEngine.Random.value < 0.3f);

            if (!cowardSave)
            {
                decision.Action = "CHALLENGE";

                // ���ɸ�����������
                string reason = "";
                if (deviationCheck > 4)
                    reason = $"̫�����ˣ�����ƽ��ֻ�� {expectedVal:F1} ��������� {currentQty}���Ҳ�������������";
                else if (currentProb < 0.05f)
                    reason = $"����ֻ�� {currentProb:P1}���⼸���ǲ����ܵġ�";
                else
                    reason = $"�ҿ����� {currentPoint}���ҳ��ϸ���̫�� ({currentProb:P1})��ֵ��һ����";

                decision.Reasoning = reason;
                return decision;
            }
        }

        // =================================================================================
        // 3. ���ɺ�ѡ�����б� (�߼�����΢��������µĲ���)
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

                int myCount = myCounts[p]; // ����ʱ���ҵ�����������3��
                int needed = checkQty - myCount;

                // ����ʤ��ʱ����Ȼ����һ�㣬�� 15 ����ͨ���������
                // ����� effectiveUnknownTrials ��ָ������֮��� 3 ���� * 5 ������ (���Թ���)
                float successProb = (needed <= 0) ? 1.0f : CalculateBinomialProbability(needed, totalUnknownDice, 1.0 / 6.0);

                // �ж�ս������
                if (checkQty == currentQty + 1 && p == currentPoint) candidate.Strategy = StrategyType.SafeFollow;
                else if (myCount >= 3) candidate.Strategy = StrategyType.Dominance;
                else if (myCount <= 1 && successProb < 0.3f) candidate.Strategy = StrategyType.PureBluff;
                else if (p != currentPoint) candidate.Strategy = StrategyType.Pivot;
                else candidate.Strategy = StrategyType.SafeFollow;

                // ����
                float baseScore = successProb * 100f;
                candidate.Score = CalculateScore(baseScore, candidate.Strategy, riskTolerance, myCount, qDelta, inference, p);

                if (myHand.Count > 0 && myHand[myHand.Count - 1] == p) candidate.Score += 10f; // �������ӷ�

                candidate.Reasoning = GenerateReasoning(candidate.Strategy, myCount, checkQty, p, successProb);
                candidates.Add(candidate);
            }
        }

        // 4. ����ѡ��
        if (candidates.Count == 0)
        {
            decision.Action = "CHALLENGE";
            decision.Reasoning = "�е����ˣ��޷���ע��";
            return decision;
        }

        candidates.Sort((a, b) => b.Score.CompareTo(a.Score));

        // ���̶�ѡ�� Top N
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

    // ����������������������ʷ��¼�������ϵ�������
    // ���ص� Dictionary key �ǵ���(1-6)��value �����Ŷ�(Խ�ߴ���Խ���˽й�)
    private static Dictionary<int, float> BuildMemoryFromHistory(List<TurnLog> history)
    {
        Dictionary<int, float> inference = new Dictionary<int, float>();
        for (int i = 1; i <= 6; i++) inference[i] = 0f;

        if (history == null || history.Count == 0) return inference;

        // Ȩ��˥��ϵ����Խ���Ľ��Ʋο���ֵԽ��
        float weight = 1.0f;

        // ����������������һ����ʼ���ؿ���
        for (int i = history.Count - 1; i >= 0; i--)
        {
            var log = history[i];
            if (log.action == "BID" && !string.IsNullOrEmpty(log.details))
            {
                // ���� "3 x 5" �� "3��5" ��ʽ
                // ���� LiarGameManager �д洢��ʽΪ "Quantity x Point"
                try
                {
                    // �򵥵Ľ����߼������� 'x', 'X', '��'
                    string[] parts = log.details.Split(new char[] { 'x', 'X', '��' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        int q = int.Parse(parts[0].Trim());
                        int p = int.Parse(parts[1].Trim());

                        if (p >= 1 && p <= 6)
                        {
                            // �����߼������˸ҽ����������˵�������еĸ�������
                            // �ۼ�ֵ = (����ռ��) * (ʱ��Ȩ��)
                            inference[p] += (q / 10f) * weight;
                        }
                    }
                }
                catch { /* ���Խ������� */ }
            }

            // ÿһ����ʷ��¼˥�� 15%
            weight *= 0.85f;
            if (weight < 0.1f) break; // ̫��Զ�ĺ���
        }
        return inference;
    }
    // ���޸ġ������߼��������� inference �� p ����
    private static float CalculateScore(float baseProbScore, StrategyType type, float risk, int myCount, int jumpAmount, Dictionary<int, float> inference, int p)
    {
        float score = baseProbScore;

        // --- ���������ԡ�����Ȩ�ط� ---
        // �����ʷ��¼�иõ�����Ծ (Inference��)�������ӷ�
        float memoryTrust = 0f;
        if (inference.ContainsKey(p))
        {
            memoryTrust = inference[p] * 10f;
            score += memoryTrust;
        }

        // --- ���������ԡ�"������" (Sheep Strategy) ---
        // ������Ǳ����� (risk < 0.4)������������ǳ������ȵ� (Inference���)���Ҽ��������ڸ���
        if (risk < 0.4f)
        {
            float maxInference = inference.Values.Max();
            // �����ǰ���� p ���ȶȽӽ����ֵ�����Ҳ�����թ��
            if (inference[p] >= maxInference * 0.9f && type != StrategyType.PureBluff)
            {
                score += 35f; // �����������ʹ����NPC�����
            }
        }

        // ԭ�еĲ��������߼�
        switch (type)
        {
            case StrategyType.Dominance:
                score += (myCount * 12f);
                if (jumpAmount > 0) score += 5f;
                break;

            case StrategyType.PureBluff:
                if (risk > 0.7f) score += 40f;
                else if (risk < 0.3f) score -= 50f;

                // թ���������������û�˽й������ (Inference��)��թ���ɹ��ʿ��ܸߣ�û��ע�⣩��Ҳ���ܵͣ����ױ�ץ��
                // �����ɾ������ǻ���
                if (risk > 0.8f && (!inference.ContainsKey(p) || inference[p] < 0.1f))
                    score += 15f; // "���䲻��"�ӷ�

                score += UnityEngine.Random.Range(0, 15f);
                break;

            case StrategyType.SafeFollow:
                if (risk < 0.4f) score += 20f;
                break;

            case StrategyType.Pivot:
                // ����ɫʱ�����������һ�������к��ȵĵ���������ӷ�
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
    }// --- ���������������о���е����� ---
    private static string GenerateReasoning(StrategyType type, int myCount, int q, int p, float prob)
    {
        string probStr = $"{prob:P0}";

        switch (type)
        {
            case StrategyType.Dominance:
                return $"���� {myCount} �� {p}�����Ǿ�������(Dominance)������ {probStr}����Ҫ�Դ�ʩѹ��";

            case StrategyType.PureBluff:
                return $"�����һ�� {p} ��û�С������Ҿ���թ��(Pure Bluff)����Ҫ��װ�Һ����š�";

            case StrategyType.SafeFollow:
                return $"���Ȳ���(Safe)���ϼҽ��ˣ��������� {myCount} ������һע�Ƚϰ�ȫ��";

            case StrategyType.Pivot:
                return $"���뻻����ɫ(Pivot)��ת�� {p}����Ϊ�������� {myCount} �������ԭ���ĵ�������ȫ��";

            default:
                return $"�ۺϿ������� {q} �� {p}��";
        }
    }

    // (����֮ǰ����ѧ���㺯�� CalculateBinomialProbability ��)
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