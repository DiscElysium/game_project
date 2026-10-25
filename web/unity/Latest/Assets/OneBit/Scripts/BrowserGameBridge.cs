using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public sealed class BrowserGameBridge : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install() => new GameObject("BrowserGameBridge").AddComponent<BrowserGameBridge>();
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void MD_BrowserState(string json);
    [DllImport("__Internal")] static extern void MD_Download(string name,string text);
    [DllImport("__Internal")] static extern void MD_SyncStorage();
#endif
    [Serializable] class State
    {
        public string version,aiStatus,aiSource,applied;
        public bool started,playerTurn,roundOver,observing;
        public int bidQuantity,bidFace,draftQuantity,draftFace,traceCount,reasoningCharacters,outputCharacters,packets,width,height;
        public long http;
        public Vector3 cameraPosition;
        public Vector2 plus,face6,bell,peek2;
    }
    float next;
    void Update()
    {
        if(Time.unscaledTime<next) return; next=Time.unscaledTime+.25f;
        var manager=LiarGameManager.Instance; var p=OneBitPresentation.Instance;
        if(!manager || !p || !manager.uiController.hudRoot) return;
        var ui=manager.uiController; var trace=AITraceStore.Latest;
        var look=manager.GetComponent<SeatedLookController>();
        var state=new State {
            version=DeepSeekClient.BuildVersion,started=!ui.welcomePanel.activeSelf,playerTurn=manager.IsPlayerTurn,roundOver=manager.IsRoundOver,
            observing=look && look.Observing,aiStatus=manager.apiClient.DisplayStatus,aiSource=trace?.source ?? "NONE",http=trace?.http ?? 0,
            traceCount=AITraceStore.Records.Count,reasoningCharacters=trace?.reasoning.Length ?? 0,outputCharacters=trace?.output.Length ?? 0,packets=trace?.packets ?? 0,applied=trace?.applied ?? "",
            bidQuantity=manager.PublicState.currentBidQuantity,bidFace=manager.PublicState.currentBidPoint,draftQuantity=ui.DraftQuantity,draftFace=ui.DraftFace,
            width=Screen.width,height=Screen.height,cameraPosition=Camera.main.transform.position,
            plus=Point(ui.increaseActionObjects[0]),face6=Point(ui.bidPointObjects[5]),bell=Point(ui.bidActionObjects[0]),peek2=Point(ui.xrayTargetObjects[1])
        };
#if UNITY_WEBGL && !UNITY_EDITOR
        MD_BrowserState(JsonUtility.ToJson(state));
#endif
    }
    static Vector2 Point(GameObject obj)
    { var c=obj.GetComponentInChildren<Collider>(); return OneBitPresentation.Instance.WorldToScreen(c ? c.bounds.center : obj.transform.position); }
    [Preserve] public void ConfigureAI(string url) => LiarGameManager.Instance.apiClient.ConfigureServer(url);
    [Preserve] public void Command(string command)
    {
        var manager=LiarGameManager.Instance; var ui=manager.uiController; var look=manager.GetComponent<SeatedLookController>();
        switch(command)
        {
            case "start":ui.btnStartGame.onClick.Invoke();break;
            case "observe":look.SetObservation(true);break;
            case "north":look.SetLookAngles(0,-10);break;
            case "east":look.SetLookAngles(90,-10);break;
            case "south":look.SetLookAngles(180,-10);break;
            case "west":look.SetLookAngles(270,-10);break;
            case "table":look.SetObservation(false);break;
            case "ai":ui.ToggleAILab();break;
            case "tone":ui.TogglePicture();break;
            case "reason":ui.SelectAITab(0);break;
            case "output":ui.SelectAITab(1);break;
            case "raw":ui.SelectAITab(4);break;
            case "export":AITraceStore.Export();break;
        }
    }
    public static void Download(string name,string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MD_Download(name,text);
#endif
    }
    public static void SyncStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        MD_SyncStorage();
#endif
    }
}
