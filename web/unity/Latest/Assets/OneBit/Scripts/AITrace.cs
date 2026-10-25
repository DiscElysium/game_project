using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

[Serializable] public sealed class AITrace
{
    public int id, attempt, inputTokens, outputTokens, packets;
    public string started, role, model, effort, source="PENDING", status="Sending", finishReason="", request="", reasoning="", output="", raw="", applied="";
    public bool thinking;
    public long http;
    public float elapsed, startedAt;
}
public static class AITraceStore
{
    public static readonly List<AITrace> Records=new List<AITrace>();
    public static int Revision { get; private set; }
    static string sessionPath;
    public static AITrace Latest => Records.Count==0 ? null : Records[Records.Count-1];
    public static string Redact(string value) => Regex.Replace(value ?? "",@"sk-[A-Za-z0-9_-]{16,}","[REDACTED KEY]");
    public static AITrace Begin(string role,string model,bool thinking,string effort,int attempt,string request)
    {
        var record=new AITrace { id=Records.Count+1,started=DateTime.UtcNow.ToString("u"),role=role,model=model,thinking=thinking,effort=effort,attempt=attempt,request=Redact(request),startedAt=Time.realtimeSinceStartup };
        Records.Add(record); Changed(); return record;
    }
    public static void Changed() => Revision++;
    public static void End(AITrace record,long code,string status,string source)
    { record.http=code; record.status=status; record.source=source; record.elapsed=Time.realtimeSinceStartup-record.startedAt; record.reasoning=Redact(record.reasoning); record.output=Redact(record.output); record.raw=Redact(record.raw); Changed(); }
    public static void Applied(NPCAnalysisResult result,bool fallback,string correction)
    {
        var record=Latest; if(record==null) return;
        if(fallback) record.source="OFFLINE_FALLBACK";
        record.applied=Redact((fallback ? "Offline fallback" : "API decision")+"\n"+correction+"\n"+JsonUtility.ToJson(result,true)); Changed(); Persist(record);
    }
    public static void Persist(AITrace record)
    {
        try { Directory.CreateDirectory(Application.persistentDataPath); if(sessionPath==null) sessionPath=Path.Combine(Application.persistentDataPath,"AI-session-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".jsonl"); File.AppendAllText(sessionPath,Redact(JsonUtility.ToJson(record))+"\n"); } catch { }
    }
    public static string Export()
    {
        Directory.CreateDirectory(Application.persistentDataPath); var path=Path.Combine(Application.persistentDataPath,"AI-export.jsonl"); var sb=new StringBuilder(); foreach(var record in Records) sb.AppendLine(Redact(JsonUtility.ToJson(record))); File.WriteAllText(path,sb.ToString()); BrowserGameBridge.Download("AI-export.jsonl",sb.ToString()); return path;
    }
}

public sealed class DeepSeekStreamParser
{
    [Serializable] public class Delta { public string content, reasoning_content; }
    [Serializable] public class Choice { public Delta delta; public string finish_reason; }
    [Serializable] public class Usage { public int prompt_tokens, completion_tokens; }
    [Serializable] public class Packet { public Choice[] choices; public Usage usage; }
    readonly AITrace record;
    readonly Decoder decoder=Encoding.UTF8.GetDecoder();
    readonly StringBuilder lines=new StringBuilder();
    public bool Done { get; private set; }
    public DeepSeekStreamParser(AITrace record) { this.record=record; }
    public bool Feed(byte[] bytes,int length)
    {
        if(bytes==null || length<=0) return true;
        var chars=new char[Encoding.UTF8.GetMaxCharCount(length)]; int count=decoder.GetChars(bytes,0,length,chars,0,false);
        var text=AITraceStore.Redact(new string(chars,0,count)); record.raw+=text; lines.Append(text);
        int newline; while((newline=lines.ToString().IndexOf('\n'))>=0)
        {
            string line=lines.ToString(0,newline).TrimEnd('\r'); lines.Remove(0,newline+1);
            if(!line.StartsWith("data:")) continue;
            string payload=line.Substring(5).Trim(); if(payload=="[DONE]") { Done=true; continue; }
            try
            {
                var packet=JsonUtility.FromJson<Packet>(payload);
                record.packets++;
                if(packet.usage!=null) { record.inputTokens=packet.usage.prompt_tokens; record.outputTokens=packet.usage.completion_tokens; }
                if(packet.choices!=null) foreach(var choice in packet.choices)
                {
                    if(choice.delta!=null) { record.reasoning+=choice.delta.reasoning_content ?? ""; record.output+=choice.delta.content ?? ""; }
                    if(!string.IsNullOrEmpty(choice.finish_reason)) record.finishReason=choice.finish_reason;
                }
                record.status=record.output.Length>0 ? "Receiving output" : "Receiving provider reasoning";
            }
            catch { record.status="Malformed stream packet"; }
        }
        AITraceStore.Changed(); return true;
    }
}
public sealed class DeepSeekStream : DownloadHandlerScript
{
    readonly DeepSeekStreamParser parser;
    public bool Done => parser.Done;
    public DeepSeekStream(AITrace record):base(new byte[8192]) { parser=new DeepSeekStreamParser(record); }
    protected override bool ReceiveData(byte[] bytes,int length) => parser.Feed(bytes,length);
}
