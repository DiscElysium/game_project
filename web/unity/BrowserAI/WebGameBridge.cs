using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class WebGameBridge : MonoBehaviour
{
    public static WebGameBridge Instance { get; private set; }
    private string endpoint;
    private bool afterBid;
    private bool waitingXRay;
    private int xrayTarget;
    private float nextState;
    [Serializable] private class BidRequest { public int quantity; public int point; public string chat; }
    [Serializable] private class ViewState { public bool started; public bool canAct; public bool afterBid; public bool gameOver; public int quantity; public int point; public string status; public string micro; public string decision; public string hand; public int specialCount; public string peek; }

    public void BeginGame()
    {
        var manager = LiarGameManager.Instance;
        var ui = FindObjectOfType<PlayerUIController>();
        if (!manager || !ui) return;
        afterBid = false;
        if (ui.btnStartGame && ui.btnStartGame.gameObject.activeInHierarchy) ui.btnStartGame.onClick.Invoke();
        else if (ui.gameOverPanel && ui.gameOverPanel.activeInHierarchy) { ui.gameOverPanel.SetActive(false); manager.StartNewRound(); }
    }
    public void PlaceBid(string json)
    {
        var manager = LiarGameManager.Instance;
        if (!manager || !manager.WebCanAct || afterBid || waitingXRay) return;
        var bid = JsonUtility.FromJson<BidRequest>(json);
        if (bid.quantity < 1 || bid.quantity > 60 || bid.point < 1 || bid.point > 6) return;
        if (bid.quantity < manager.GetCurrentMatchBidQuantity() || (bid.quantity == manager.GetCurrentMatchBidQuantity() && bid.point <= manager.GetCurrentMatchBidPoint())) return;
        manager.PlayerBid(bid.quantity, bid.point, bid.chat ?? "");
        afterBid = true;
    }
    public void Challenge()
    {
        var manager = LiarGameManager.Instance;
        if (manager && manager.WebCanAct && !afterBid && !waitingXRay && manager.GetCurrentMatchBidQuantity() > 0) manager.PlayerChallenge();
    }
    public void SpecialAction(string json)
    {
        var manager = LiarGameManager.Instance;
        if (!manager || !manager.WebCanAct || !afterBid || waitingXRay) return;
        var action = JsonUtility.FromJson<BidRequest>(json);
        afterBid = false;
        if (action.quantity >= 1 && action.quantity <= 3)
        {
            if (action.point == 0) { waitingXRay = true; xrayTarget = action.quantity; manager.PlayerUseXRay(action.quantity); StartCoroutine(FinishXRay(action.quantity)); return; }
            if (action.point >= 1 && action.point <= 6) manager.PlayerGuessSpecialDie(action.quantity, action.point);
        }
        manager.FinishPlayerTurn();
    }
    private IEnumerator FinishXRay(int target)
    {
        var ui = FindObjectOfType<PlayerUIController>();
        if (ui) ui.SwitchToNPCCamera(target);
        yield return new WaitForSeconds(3);
        LiarGameManager.Instance.FinishPlayerTurn();
        waitingXRay = false;
    }
    private void Update()
    {
        if (Time.unscaledTime < nextState) return;
        nextState = Time.unscaledTime + 0.2f;
        var manager = LiarGameManager.Instance;
        var ui = FindObjectOfType<PlayerUIController>();
        if (!manager || !ui) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        MaskedDiceViewState(JsonUtility.ToJson(new ViewState {
            started = ui.btnStartGame && !ui.btnStartGame.gameObject.activeInHierarchy,
            canAct = manager.WebCanAct && !waitingXRay, afterBid = afterBid,
            gameOver = ui.gameOverPanel && ui.gameOverPanel.activeInHierarchy,
            quantity = manager.GetCurrentMatchBidQuantity(), point = manager.GetCurrentMatchBidPoint(),
            status = manager.statusText ? manager.statusText.text : "",
            micro = manager.microExpressionText ? manager.microExpressionText.text : "",
            decision = ui.decisionResultText ? ui.decisionResultText.text : "",
            hand = manager.diceManager && !manager.diceManager.isRolling ? manager.GetWebHand(0) : "",
            specialCount = manager.GetWebSpecialCount(0),
            peek = waitingXRay ? "NPC " + xrayTarget + ": " + manager.GetWebHand(xrayTarget) : ""
        }));
#endif
    }

    [Serializable] private class AnalysisRequest { public string persona; public string prompt; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (Instance != null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
#endif
        var bridge = new GameObject("WebGameBridge");
        DontDestroyOnLoad(bridge);
        Instance = bridge.AddComponent<WebGameBridge>();
    }

    public void Configure(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)))
            return;
        endpoint = url;
        Report("ready");
    }

    public Task<string> Analyze(string persona, string prompt)
    {
        var result = new TaskCompletionSource<string>();
        StartCoroutine(Request(persona, prompt, result));
        return result.Task;
    }

    private IEnumerator Request(string persona, string prompt, TaskCompletionSource<string> result)
    {
        const string unavailable = "{\"risk_factor\":0.5,\"micro_expression\":\"AI 连接暂时不可用。\",\"analysis_summary\":\"Remote AI unavailable\"}";
        if (string.IsNullOrEmpty(endpoint))
        {
            Report("unconfigured");
            result.TrySetResult(unavailable);
            yield break;
        }
        string body = JsonUtility.ToJson(new AnalysisRequest { persona = persona, prompt = prompt });
        using (var request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 45;
            Report("thinking");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success || string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                Report("unavailable");
                Debug.LogWarning("Remote AI unavailable; game continues using the original strategy algorithm.");
                result.TrySetResult(unavailable);
            }
            else
            {
                Report("ready");
                result.TrySetResult(request.downloadHandler.text);
            }
        }
    }

    private static void Report(string state)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MaskedDiceAIStatus(state);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void MaskedDiceAIStatus(string state);
    [DllImport("__Internal")] private static extern void MaskedDiceViewState(string state);
#endif
}
