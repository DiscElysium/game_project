using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Browser port of the verified desktop client. Only the server owns the key.
public sealed partial class DeepSeekClient : MonoBehaviour
{
    public string model="deepseek-flash";
    public bool enableThinking=true;
    [Range(10,180)] public int timeoutSeconds=55;
    public const string BuildVersion="2026.10.25-r3-web";
    public string serverOrigin="";
    public string LastStatus { get; private set; }="Not connected";
    public string ConnectionStatus { get; private set; }="Checking server AI";
    public bool CheckingConnection { get; private set; }
    public string DisplayStatus => LastStatus=="Not connected" ? ConnectionStatus : LastStatus;
    [Serializable] public class Message { public string role,content; }
    [Serializable] public class Format { public string type="json_object"; }
    [Serializable] public class Thinking { public string type; }
    [Serializable] public class StreamOptions { public bool include_usage=true; }
    [Serializable] public class Request
    {
        public string model; public Message[] messages; public bool stream=true;
        public int max_tokens=8192; public Format response_format=new Format();
        public Thinking thinking; public string reasoning_effort;
        public StreamOptions stream_options=new StreamOptions();
    }
    [Serializable] class Health { public bool configured; public string model; }
    [Serializable] class Role { public string role; }
    [Serializable] class BrowserEvent { public int id; public string kind,data; public long status; }
    int sequence,activeId; long responseCode; string browserError; bool finished;
    AITrace activeTrace; DeepSeekStreamParser parser;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern string MD_AIOrigin();
    [DllImport("__Internal")] static extern void MD_AIStart(string target,int id,string url,string body,int timeout);
    [DllImport("__Internal")] static extern void MD_AICancel(int id);
#endif
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        serverOrigin=MD_AIOrigin();
#endif
    }
    void Start() { LoadSettings(); if(!string.IsNullOrEmpty(serverOrigin)) StartCoroutine(CheckConnection()); }
    public void ConfigureServer(string origin)
    {
        if(!Uri.TryCreate(origin,UriKind.Absolute,out var uri) || (uri.Scheme!="https" && uri.Scheme!="http")) return;
        serverOrigin=uri.GetLeftPart(UriPartial.Authority); StartCoroutine(CheckConnection());
    }
    public IEnumerator CheckConnection()
    {
        if(CheckingConnection || string.IsNullOrEmpty(serverOrigin)) yield break;
        CheckingConnection=true; ConnectionStatus="Verifying server AI";
        using(var request=UnityWebRequest.Get(serverOrigin+"/api/health"))
        {
            request.timeout=20; yield return request.SendWebRequest();
            if(request.result==UnityWebRequest.Result.Success)
            {
                Health health=null; try { health=JsonUtility.FromJson<Health>(request.downloadHandler.text); } catch { }
                ConnectionStatus=health!=null && health.configured ? "DeepSeek server configured" : "Server AI unavailable";
            }
            else ConnectionStatus="Server connection unavailable / retry";
        }
        CheckingConnection=false;
    }
    public IEnumerator Decide(string systemPrompt,string situationJson,Action<NPCAnalysisResult,string> complete)
    {
        string role="Opponent"; try { role=JsonUtility.FromJson<Role>(situationJson).role; } catch { }
        var body=new Request {
            model=model,max_tokens=Mathf.Clamp(maxTokens,1024,8192),
            messages=new[]{new Message {role="system",content=systemPrompt+(string.IsNullOrWhiteSpace(extraInstructions) ? "" : "\nAdditional director instructions:\n"+extraInstructions)},new Message {role="user",content=situationJson}},
            thinking=new Thinking {type=enableThinking ? "enabled" : "disabled"},reasoning_effort=thinkingEffort
        };
        string json=JsonUtility.ToJson(body); int retries=retryCount,seconds=timeoutSeconds; bool thinking=enableThinking;
        string error="Server connection unavailable";
        for(int attempt=0;attempt<=retries;attempt++)
        {
            var trace=AITraceStore.Begin(role,body.model,thinking,body.reasoning_effort,attempt+1,json);
            activeTrace=trace; parser=new DeepSeekStreamParser(trace); activeId=++sequence;
            int requestId=activeId; responseCode=0; browserError=""; finished=false;
            LastStatus=attempt==0 ? "DeepSeek thinking" : "Retrying connection";
#if UNITY_WEBGL && !UNITY_EDITOR
            MD_AIStart(gameObject.name,requestId,serverOrigin+"/api/chat",json,seconds*1000);
            float deadline=Time.realtimeSinceStartup+seconds+3;
            while(!finished && requestId==activeId && Time.realtimeSinceStartup<deadline) yield return null;
            if(!finished && requestId==activeId) { browserError="Request timed out"; MD_AICancel(requestId); }
#else
            using(var request=new UnityWebRequest(serverOrigin+"/api/chat","POST"))
            {
                request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)); request.downloadHandler=new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type","application/json"); request.timeout=seconds;
                yield return request.SendWebRequest(); responseCode=request.responseCode;
                byte[] bytes=request.downloadHandler.data; if(bytes!=null) parser.Feed(bytes,bytes.Length);
                if(request.result!=UnityWebRequest.Result.Success) browserError="Server request failed";
            }
#endif
            if(requestId!=activeId) yield break;
            NPCAnalysisResult result=null;
            if(responseCode==200 && parser.Done && trace.finishReason=="stop")
            {
                try {
                    result=JsonUtility.FromJson<NPCAnalysisResult>(trace.output);
                    if(result==null || (result.action!="BID" && result.action!="CHALLENGE") || string.IsNullOrWhiteSpace(result.dialogue) || string.IsNullOrWhiteSpace(result.micro_expression)) throw new FormatException();
                    result.risk_factor=Mathf.Clamp01(result.risk_factor); result.dialogue=OneBitRules.SafeText(result.dialogue,100); result.micro_expression=OneBitRules.SafeText(result.micro_expression,140);
                } catch { result=null; }
            }
            activeTrace=null; parser=null;
            if(result!=null)
            { AITraceStore.End(trace,200,"API reply accepted","REAL_API"); LastStatus="DeepSeek connected / "+(thinking ? "thinking" : "direct"); complete(result,null); yield break; }
            error=responseCode==429 ? "AI request limit reached / try later" : responseCode==503 ? "Server AI unavailable" : responseCode==200 ? "Incomplete AI reply" : string.IsNullOrEmpty(browserError) ? "AI service unavailable" : browserError;
            AITraceStore.End(trace,responseCode,error,responseCode==200 ? "INVALID_REPLY" : "API_ERROR"); AITraceStore.Persist(trace);
            if(responseCode!=0 && responseCode!=429 && responseCode<500) break;
            yield return new WaitForSecondsRealtime(2);
        }
        LastStatus=error+" / offline fallback"; complete(null,LastStatus);
    }
    [UnityEngine.Scripting.Preserve] public void ReceiveBrowserAI(string json)
    {
        BrowserEvent message; try { message=JsonUtility.FromJson<BrowserEvent>(json); } catch { return; }
        if(message.id!=activeId || activeTrace==null) return;
        if(message.status>0) responseCode=message.status;
        if(message.kind=="chunk") { byte[] bytes=Encoding.UTF8.GetBytes(message.data); parser.Feed(bytes,bytes.Length); }
        if(message.kind=="error") { browserError="AI connection interrupted"; finished=true; }
        if(message.kind=="done") finished=true;
    }
    public void CancelPending()
    {
        if(activeTrace!=null) { AITraceStore.End(activeTrace,responseCode,"Cancelled","CANCELLED"); AITraceStore.Persist(activeTrace); activeTrace=null; }
#if UNITY_WEBGL && !UNITY_EDITOR
        MD_AICancel(activeId);
#endif
        activeId=++sequence; parser=null; finished=true;
    }
    void OnDisable() => CancelPending();
    void OnDestroy() => CancelPending();
}
public static class DeepSeekCredentialStore
{
    public static string PathOnDisk => Path.Combine(Application.persistentDataPath,"server-managed-no-local-key");
    public static string ReadStatus => "Server-managed AI";
    public static string Read() => null;
    public static void Save(string key) => throw new InvalidOperationException("The website server manages the AI key.");
}
