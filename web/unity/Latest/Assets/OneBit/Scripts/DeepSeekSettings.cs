using System;
using System.IO;
using UnityEngine;

public sealed partial class DeepSeekClient
{
    public string thinkingEffort="low",extraInstructions="";
    public int maxTokens=8192,retryCount=1;
    [Serializable] class Settings
    { public string model,effort,instructions; public bool thinking; public int tokens,timeout,retries; public float[] risk; }
    string SettingsPath => Path.Combine(Application.persistentDataPath,"ai-settings.json");
    public void SaveSettings()
    {
        maxTokens=Mathf.Clamp(maxTokens,1024,8192); timeoutSeconds=Mathf.Clamp(timeoutSeconds,10,180); retryCount=Mathf.Clamp(retryCount,0,2);
        if(thinkingEffort!="low" && thinkingEffort!="high" && thinkingEffort!="max") thinkingEffort="low";
        model=model.Trim(); extraInstructions=AITraceStore.Redact(extraInstructions);
        var manager=LiarGameManager.Instance;
        Directory.CreateDirectory(Application.persistentDataPath); File.WriteAllText(SettingsPath,JsonUtility.ToJson(new Settings { model=model,effort=thinkingEffort,instructions=extraInstructions,thinking=enableThinking,tokens=maxTokens,timeout=timeoutSeconds,retries=retryCount,risk=manager ? manager.npcRiskTolerance : new[]{.2f,.5f,.8f} },true)); BrowserGameBridge.SyncStorage();
    }
    public void LoadSettings()
    {
        try
        {
            if(!File.Exists(SettingsPath)) return; var s=JsonUtility.FromJson<Settings>(File.ReadAllText(SettingsPath));
            if(string.IsNullOrWhiteSpace(s.model)) return; model=s.model; thinkingEffort=s.effort; enableThinking=s.thinking; extraInstructions=s.instructions ?? ""; maxTokens=Mathf.Clamp(s.tokens,1024,32768); timeoutSeconds=Mathf.Clamp(s.timeout,10,180); retryCount=Mathf.Clamp(s.retries,0,2);
            if(LiarGameManager.Instance && s.risk?.Length==3) for(int i=0;i<3;i++) LiarGameManager.Instance.npcRiskTolerance[i]=Mathf.Clamp01(s.risk[i]);
        }
        catch { }
    }
    public void ResetSettings()
    { model="deepseek-flash"; enableThinking=true; thinkingEffort="low"; extraInstructions=""; maxTokens=8192; timeoutSeconds=55; retryCount=1; if(LiarGameManager.Instance) LiarGameManager.Instance.npcRiskTolerance=new[]{.2f,.5f,.8f}; SaveSettings(); }
}
