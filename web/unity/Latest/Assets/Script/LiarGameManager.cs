using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiarGameManager : MonoBehaviour
{
    public static LiarGameManager Instance;
    public DiceManager diceManager;
    public PlayerUIController uiController;
    public DeepSeekClient apiClient;
    public float turnDuration = 15, resultDisplayDuration = 2;
    public float[] npcRiskTolerance = { .2f, .5f, .8f };
    public Text statusText, microExpressionText;
    public GameObject microExpressionBg, decisionResultBg, playerPanel;
    public DiceCup[] npcCups;
    public bool usePureLLMDecision = true;
    GameState state = new GameState();
    readonly Dictionary<int, List<int>> allHands = new Dictionary<int, List<int>>();
    readonly Dictionary<int, int> specialCounts = new Dictionary<int, int>();
    bool playerActive, playerActed, specialAvailable, ended = true;
    int bidderIndex = -1, roundVersion;
    public bool IsPlayerTurn => playerActive && !ended;
    public bool IsSpecialAction => IsPlayerTurn && specialAvailable;
    public bool HasCurrentBid => state.currentBidQuantity > 0;
    public bool IsRoundOver => ended;
    public GameState PublicState => state;
    public string APIStatus => apiClient ? apiClient.LastStatus : "Key not configured";
    public int MaximumQuantity(int player) => 16 + 3 * GetSpecialCount(player);
    public List<int> GetHumanHand() => allHands.ContainsKey(0) ? new List<int>(allHands[0]) : new List<int>();
    public int GetSpecialCount(int index) => specialCounts.ContainsKey(index) ? specialCounts[index] : 1;
    public string GetPlayerName(int index) => index == 0 ? "You" : new[] { "", "Watcher", "Weaver", "Risker" }[index];
    void Awake() { Instance = this; if (!apiClient) apiClient = GetComponent<DeepSeekClient>() ?? gameObject.AddComponent<DeepSeekClient>(); }
    void Start() { Status("Take a seat. Every cup holds a secret."); if (playerPanel) playerPanel.SetActive(false); }
    void Status(string text) { if (statusText) statusText.text = text; }
    void Decision(string text) { if (uiController && uiController.decisionResultText) uiController.decisionResultText.text = text; }
    public void StartNewRound()
    {
        if (diceManager.isRolling) return;
        roundVersion++; apiClient.CancelPending(); StopAllCoroutines();
        playerActive = playerActed = specialAvailable = false; ended = false; bidderIndex = -1;
        state = new GameState(); allHands.Clear(); specialCounts.Clear();
        for (int i = 0; i < 4; i++) specialCounts[i] = 1;
        if (npcCups != null) foreach (var cup in npcCups) if (cup) cup.RestoreCover();
        uiController.ResetRoundUI(); Status("Let the dice settle.");
        StartCoroutine(diceManager.RollAndCalculate(OnDiceRolled));
    }
    void OnDiceRolled()
    {
        for (int i = 0; i < 4; i++)
        {
            allHands[i] = diceManager.GetPlayerDice(i);
            if (allHands[i].Count != 5) { ended = true; uiController.ShowGameOver("Dice not ready", "Check the dice spawners and start again."); return; }
            foreach (int point in allHands[i]) if (point < 1 || point > 6)
            { ended = true; uiController.ShowGameOver("Dice still moving", "The roll could not be read. Start again."); return; }
        }
        StartCoroutine(TurnRoutine(0));
    }
    IEnumerator TurnRoutine(int player)
    {
        state.currentTurnIndex = player; playerActed = false;
        uiController.SetActivePlayer(player); Decision("");
        if (player == 0)
        {
            uiController.SwitchToCamera(true);
            yield return new WaitForSeconds(uiController.defaultBlendTime);
            playerActive = true; specialAvailable = false;
            if (playerPanel) playerPanel.SetActive(true);
            Status("Your turn. Raise the bid or call a bluff."); uiController.BeginPlayerTurn();
            while (!playerActed && !ended) yield return null;
            playerActive = false;
        }
        else
        {
            playerActive = false; if (playerPanel) playerPanel.SetActive(false);
            uiController.SwitchToCamera(false); Status(GetPlayerName(player) + " is weighing the bid...");
            int version = roundVersion; NPCAnalysisResult response = null; string error = null;
            yield return apiClient.Decide(OneBitRules.SystemPrompt, BuildSituation(player), (r, e) => { response = r; error = e; });
            if (version != roundVersion || ended) yield break;
            bool fallback=response==null;
            if (fallback) response = Fallback(player);
            string proposed=response.action+" "+response.bid_quantity+" x "+response.bid_point;
            ValidateDecision(player, response);
            string applied=response.action+" "+response.bid_quantity+" x "+response.bid_point;
            AITraceStore.Applied(response,fallback,proposed==applied ? "Legal action: accepted unchanged." : "Rule correction: "+proposed+" -> "+applied);
            uiController.ShowObservation(player, response.dialogue, response.micro_expression);
            if (!string.IsNullOrEmpty(error)) uiController.ShowNotice(error);
            if (response.action == "CHALLENGE")
            { Record(player, "CHALLENGE", "Call bluff", response.dialogue); EndRoundChallenge(player); yield break; }
            CommitBid(player, response.bid_quantity, response.bid_point, response.dialogue);
            Decision($"{GetPlayerName(player)} bids {response.bid_quantity} x {response.bid_point}");
        }
        if (ended) yield break;
        yield return new WaitForSeconds(resultDisplayDuration);
        if (!ended) StartCoroutine(TurnRoutine((player + 1) % 4));
    }
    [Serializable] class Situation
    {
        public string role, personality, currentBidder;
        public int[] ownNormalDice, ownSpecialDice, publicSpecialCounts;
        public int unknownNormalDice, currentQuantity, currentFace, maximumLegalQuantity;
        public float riskTolerance;
        public TurnLog[] publicHistory;
    }
    string BuildSituation(int player)
    {
        var hand = allHands[player]; int special = specialCounts[player];
        var personalities = new[] { "", "Watcher: cautious and analytical. Trust probability and evidence; test gently and question inflated bids.", "Weaver: a patient trickster. Remember history, disguise confident risks as hesitation and bluff with restraint.", "Risker: bold and provocative. Raise pressure and watch reactions, but respect probability." };
        int start = Math.Max(0, state.detailedLog.Count - 16);
        return JsonUtility.ToJson(new Situation
        {
            role = GetPlayerName(player), personality = personalities[player],
            riskTolerance = npcRiskTolerance[player-1],
            ownNormalDice = hand.GetRange(0, hand.Count - special).ToArray(), ownSpecialDice = hand.GetRange(hand.Count - special, special).ToArray(),
            publicSpecialCounts = new[] { specialCounts[0], specialCounts[1], specialCounts[2], specialCounts[3] },
            unknownNormalDice = 16 - (hand.Count - special), currentQuantity = state.currentBidQuantity, currentFace = state.currentBidPoint,
            currentBidder = bidderIndex < 0 ? "No bid yet" : GetPlayerName(bidderIndex), maximumLegalQuantity = MaximumQuantity(player),
            publicHistory = state.detailedLog.GetRange(start, state.detailedLog.Count - start).ToArray()
        });
    }
    NPCAnalysisResult Fallback(int player)
    {
        var d = LiarAIStrategy.MakeDecision(allHands[player], state.currentBidQuantity, state.currentBidPoint, npcRiskTolerance[player - 1], state.detailedLog, specialCounts[player], 20 - allHands[player].Count);
        return new NPCAnalysisResult { action = d.Action, bid_quantity = d.Quantity, bid_point = d.Point, risk_factor = npcRiskTolerance[player - 1], dialogue = "I will weigh that bid carefully.", micro_expression = "His knuckles rest against the cup. His eyes sweep the table." };
    }
    void ValidateDecision(int player, NPCAnalysisResult result)
    {
        if (result.action == "CHALLENGE" && HasCurrentBid) return;
        if (result.action == "BID" && OneBitRules.LegalBid(result.bid_quantity, result.bid_point, state.currentBidQuantity, state.currentBidPoint, MaximumQuantity(player))) return;
        if (HasCurrentBid) { result.action = "CHALLENGE"; result.bid_quantity = result.bid_point = 0; }
        else { result.action = "BID"; result.bid_quantity = 1; result.bid_point = allHands[player][0]; }
    }
    void CommitBid(int player, int quantity, int face, string chat)
    {
        state.currentBidQuantity = quantity; state.currentBidPoint = face; bidderIndex = player;
        state.currentBidPlayer = GetPlayerName(player); Record(player, "BID", $"{quantity} x {face}", chat); uiController.RefreshBid();
    }
    void Record(int player, string action, string details, string chat)
    {
        state.detailedLog.Add(new TurnLog { playerName = GetPlayerName(player), action = action, details = details, chatContent = OneBitRules.SafeText(chat, 80) });
        uiController.RefreshHistory();
    }
    public bool TryPlayerBid(int quantity, int face, string chat)
    {
        if (!IsPlayerTurn || specialAvailable || playerActed) return false;
        if (!OneBitRules.LegalBid(quantity, face, state.currentBidQuantity, state.currentBidPoint, MaximumQuantity(0)))
        { uiController.ShowNotice("Raise the quantity, or raise the face at the same quantity."); return false; }
        CommitBid(0, quantity, face, chat); specialAvailable = true;
        Decision($"You bid {quantity} x {face}. Choose one special action."); return true;
    }
    public void PlayerBid(int q, int p, string chatText) => TryPlayerBid(q, p, chatText);
    public void FinishPlayerTurn()
    {
        if (!IsPlayerTurn) return;
        specialAvailable = false; playerActed = true; if (playerPanel) playerPanel.SetActive(false);
    }
    public void PlayerChallenge()
    {
        if (!IsPlayerTurn || specialAvailable || !HasCurrentBid || playerActed) return;
        Record(0, "CHALLENGE", "Call bluff", uiController.GetHumanChat()); playerActed = true; EndRoundChallenge(0);
    }
    public void PlayerUseXRay(int target)
    {
        if (!IsSpecialAction || target < 1 || target > 3) return;
        specialAvailable = false;
        if (npcCups != null && target - 1 < npcCups.Length && npcCups[target - 1]) npcCups[target - 1].ActivateXRay();
        Record(0, "XRAY", "Peeked at " + GetPlayerName(target), ""); uiController.SwitchToNPCCamera(target);
    }
    public bool PlayerGuessSpecialDie(int target, int point)
    {
        if (!IsSpecialAction || target < 1 || target > 3 || point < 1 || point > 6) return false;
        specialAvailable = false; var hand = allHands[target]; int match = -1;
        for (int i = hand.Count - specialCounts[target]; i < hand.Count; i++) if (hand[i] == point) { match = i; break; }
        bool success = match >= 0;
        if (success)
        {
            hand.RemoveAt(match); specialCounts[target]--; allHands[0].Add(point); specialCounts[0]++;
            var source = diceManager.spawners[target]; var human = diceManager.spawners[0];
            if (match < source.spawnedDice.Count)
            {
                var die = source.spawnedDice[match]; source.spawnedDice.RemoveAt(match); human.spawnedDice.Add(die);
                var rb = die.GetComponent<Rigidbody>(); if (rb) { rb.velocity = rb.angularVelocity = Vector3.zero; rb.isKinematic = true; }
                die.transform.position = human.targetPoint.position + Vector3.up * .6f;
                die.transform.rotation = Quaternion.FromToRotation(OneBitPresentation.FaceDirection(point), Vector3.up);
            }
        }
        Record(0, "STEAL", success ? "Stole from " + GetPlayerName(target) : "Steal failed", ""); return success;
    }
    public void EndRoundChallenge(int challenger)
    {
        if (ended || bidderIndex < 0) return;
        ended = true; playerActive = specialAvailable = false; apiClient.CancelPending(); StopAllCoroutines();
        int actual = 0; string detail = "";
        for (int i = 0; i < 4; i++)
        {
            int count = OneBitRules.CountHand(allHands[i], specialCounts[i], state.currentBidPoint, i == bidderIndex);
            actual += count; detail += $"{GetPlayerName(i)}  /  {count} counted\n";
        }
        string title = actual >= state.currentBidQuantity ? GetPlayerName(bidderIndex) + " holds the bid" : GetPlayerName(challenger) + " calls the bluff";
        string result = $"Bid  {state.currentBidQuantity} x {state.currentBidPoint}\nActual count  {actual}\n\n" + detail + "\nBidder special dice count 3; others count 0.";
        if (npcCups != null) foreach (var cup in npcCups) if (cup) cup.Reveal();
        uiController.SwitchToCamera(false); Status("Cups open. The round is over."); Decision(title); uiController.ShowGameOver(title, result);
    }
    public int GetCurrentMatchBidQuantity() => state.currentBidQuantity;
    public int GetCurrentMatchBidPoint() => state.currentBidPoint;
    void OnDisable() { roundVersion++; if (apiClient) apiClient.CancelPending(); }
}
