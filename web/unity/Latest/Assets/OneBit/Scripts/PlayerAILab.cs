using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerUIController
{
    public GameObject aiPanel;
    public InputField aiModelInput,aiTokenInput,aiTimeoutInput,aiRetryInput,aiPromptInput;
    public readonly List<Slider> aiRiskSliders=new List<Slider>();
    readonly List<Text> aiRiskNumbers=new List<Text>();
    readonly List<Text> aiTraceChunks=new List<Text>();
    Text aiHeader,aiTraceText,aiPager,aiThinkingText,aiEffortText,aiFeedback;
    ScrollRect aiScroll; GameObject aiPromptArea;
    int aiTraceIndex=-1,aiTab,aiRevision=-1;
    float aiRefreshAt;
    bool aiFollow=true,aiThinkingDraft;
    string aiEffortDraft="low";
    DeepSeekClient AIClient => Manager.apiClient;
    void BuildAILab()
    {
        aiRiskSliders.Clear(); aiRiskNumbers.Clear(); aiTraceChunks.Clear();
        aiPanel=Panel(layout,"AI lab",12,12,616,336).gameObject; var root=aiPanel.transform;
        Label(root,"Title","AI LAB / F3",14,308,180,18,16);
        ButtonAt(root,"Close AI","CLOSE",530,308,72,18,()=>aiPanel.SetActive(false),12);
        aiHeader=Label(root,"API evidence","No requests yet. Play a turn to inspect the actual API.",218,300,304,28,12);
        Label(root,"Model label","MODEL",14,282,180,16,12); aiModelInput=AIField(root,14,258,188,20);
        var thinking=ButtonAt(root,"Thinking","THINKING ON",14,232,188,20,()=> { aiThinkingDraft=!aiThinkingDraft; SyncAIDraftLabels(); },12); aiThinkingText=thinking.GetComponentInChildren<Text>();
        for(int i=0;i<3;i++) { string effort=new[]{"low","high","max"}[i]; ButtonAt(root,"Effort "+effort,effort.ToUpperInvariant(),14+i*64,206,60,20,()=> { aiEffortDraft=effort; SyncAIDraftLabels(); },12); }
        aiEffortText=Label(root,"Effort choice","",14,188,188,14,12);
        Label(root,"Tokens label","TOKENS",14,164,80,18,12); aiTokenInput=AIField(root,100,164,102,20); aiTokenInput.contentType=InputField.ContentType.IntegerNumber;
        Label(root,"Timeout label","TIMEOUT",14,138,80,18,12); aiTimeoutInput=AIField(root,100,138,102,20); aiTimeoutInput.contentType=InputField.ContentType.IntegerNumber;
        Label(root,"Retry label","RETRIES",14,112,80,18,12); aiRetryInput=AIField(root,100,112,102,20); aiRetryInput.contentType=InputField.ContentType.IntegerNumber;
        for(int i=0;i<3;i++) AIRisk(root,i,88-i*22);
        ButtonAt(root,"Apply AI","APPLY",14,16,88,20,ApplyAISettings,12);
        ButtonAt(root,"Reset AI","RESET",108,16,94,20,()=> { AIClient.ResetSettings(); SyncAIFields(); aiFeedback.text="Default parameters restored."; },12);
        string[] tabs={"REASON","OUTPUT","REQUEST","APPLIED","RAW"};
        for(int i=0;i<tabs.Length;i++) { int index=i; ButtonAt(root,"AI tab "+i,tabs[i],218+i*78,280,74,18,()=>SelectAITab(index),12); }
        var viewport=Rect(root,"Trace viewport",218,74,384,198); var bg=viewport.gameObject.AddComponent<Image>(); bg.color=OneBitPresentation.Paper; bg.raycastTarget=true; viewport.gameObject.AddComponent<RectMask2D>();
        aiScroll=viewport.gameObject.AddComponent<ScrollRect>(); aiScroll.viewport=viewport; aiScroll.horizontal=false; aiScroll.movementType=ScrollRect.MovementType.Clamped; aiScroll.scrollSensitivity=24;
        var content=Rect(viewport,"Trace content",0,0,372,198); content.anchorMin=content.anchorMax=new Vector2(0,1); content.pivot=new Vector2(0,1); content.anchoredPosition=Vector2.zero;
        var textRect=Rect(content,"Trace text",4,0,364,190); textRect.anchorMin=textRect.anchorMax=new Vector2(0,1); textRect.pivot=new Vector2(0,1); textRect.anchoredPosition=new Vector2(4,-4);
        aiTraceText=textRect.gameObject.AddComponent<Text>(); aiTraceText.font=pixelFont; aiTraceText.fontSize=12; aiTraceText.color=OneBitPresentation.Ink; aiTraceText.supportRichText=false; aiTraceText.raycastTarget=false; aiTraceText.alignment=TextAnchor.UpperLeft; aiTraceText.verticalOverflow=VerticalWrapMode.Overflow; aiScroll.content=content;
        aiTraceChunks.Add(aiTraceText);
        var bar=Rect(root,"Trace scrollbar",594,74,8,198); var barImage=bar.gameObject.AddComponent<Image>(); barImage.color=OneBitPresentation.Ink;
        var sliding=Rect(bar,"Sliding area",1,1,6,196); var handle=Rect(sliding,"Handle",0,0,6,26); handle.anchorMin=Vector2.zero; handle.anchorMax=Vector2.one; handle.offsetMin=handle.offsetMax=Vector2.zero; var handleImage=handle.gameObject.AddComponent<Image>(); handleImage.color=OneBitPresentation.Paper;
        var scrollbar=bar.gameObject.AddComponent<Scrollbar>(); scrollbar.direction=Scrollbar.Direction.BottomToTop; scrollbar.handleRect=handle; scrollbar.targetGraphic=handleImage; scrollbar.transition=Selectable.Transition.None; aiScroll.verticalScrollbar=scrollbar;
        aiPromptArea=Rect(root,"Extra instructions",218,74,384,198).gameObject;
        aiPromptInput=AIField(aiPromptArea.transform,0,0,384,198); aiPromptInput.lineType=InputField.LineType.MultiLineNewline; aiPromptInput.characterLimit=8192; aiPromptInput.textComponent.alignment=TextAnchor.UpperLeft; aiPromptInput.textComponent.verticalOverflow=VerticalWrapMode.Overflow;
        ((Text)aiPromptInput.placeholder).text="Additional instructions for all opponents.\nRules and private-hand limits still apply.\nUse APPLY to save. Affects the next request.";
        aiPromptArea.SetActive(false);
        ButtonAt(root,"Previous AI","<",218,54,30,18,()=>MoveAITrace(-1),12);
        ButtonAt(root,"Next AI",">",252,54,30,18,()=>MoveAITrace(1),12);
        aiPager=Label(root,"Trace counter","",288,54,116,18,12);
        ButtonAt(root,"Follow AI","LATEST",410,54,76,18,()=> { aiFollow=true; aiRevision=-1; },12);
        ButtonAt(root,"Prompt editor","EXTRA PROMPT",492,54,110,18,()=> { aiPromptArea.SetActive(!aiPromptArea.activeSelf); aiScroll.gameObject.SetActive(!aiPromptArea.activeSelf); },12);
        ButtonAt(root,"Export AI","EXPORT LOG",218,16,112,20,()=> { try { AITraceStore.Export(); aiFeedback.text="Downloaded AI-export.jsonl."; } catch { aiFeedback.text="Export could not be saved."; } },12);
        ButtonAt(root,"Verify AI","CHECK API",336,16,100,20,()=>StartCoroutine(AIClient.CheckConnection()),12);
        aiFeedback=Label(root,"AI feedback","Applies next request. Mouse wheel scrolls the full trace.",218,36,384,16,12);
        aiPanel.SetActive(false);
    }
    InputField AIField(Transform root,float x,float y,float w,float h)
    { var field=SpeechInput(root,x,y,w,h); field.characterLimit=200; ((Text)field.placeholder).text=""; return field; }
    void AIRisk(Transform root,int index,int y)
    {
        Label(root,"Risk name "+index,new[]{"Watcher","Weaver","Risker"}[index],14,y,62,18,12);
        var value=Label(root,"Risk value "+index,"",172,y,30,18,12,TextAnchor.MiddleRight); aiRiskNumbers.Add(value);
        var rt=Rect(root,"Risk slider "+index,80,y,86,18); var track=Rect(rt,"Track",0,7,86,4); var image=track.gameObject.AddComponent<Image>(); image.color=OneBitPresentation.Ink;
        var slider=rt.gameObject.AddComponent<Slider>(); slider.minValue=0; slider.maxValue=1; slider.transition=Selectable.Transition.None;
        var area=Rect(rt,"Handle area",4,0,78,18); var knob=Rect(area,"Handle",0,0,8,12); knob.pivot=new Vector2(.5f,.5f); knob.anchorMin=new Vector2(0,0); knob.anchorMax=new Vector2(0,1); knob.sizeDelta=new Vector2(8,-6); knob.anchoredPosition=Vector2.zero; var knobImage=knob.gameObject.AddComponent<Image>(); knobImage.color=OneBitPresentation.Ink; slider.handleRect=knob; slider.targetGraphic=knobImage;
        slider.onValueChanged.AddListener(v=>value.text=v.ToString("0.00")); aiRiskSliders.Add(slider);
    }
    void SyncAIDraftLabels() { aiThinkingText.text=aiThinkingDraft ? "THINKING ON" : "THINKING OFF"; aiEffortText.text="Effort: "+aiEffortDraft.ToUpperInvariant(); }
    void SyncAIFields()
    {
        aiModelInput.text=AIClient.model; aiTokenInput.text=AIClient.maxTokens.ToString(); aiTimeoutInput.text=AIClient.timeoutSeconds.ToString(); aiRetryInput.text=AIClient.retryCount.ToString(); aiPromptInput.text=AIClient.extraInstructions;
        aiThinkingDraft=AIClient.enableThinking; aiEffortDraft=AIClient.thinkingEffort; SyncAIDraftLabels();
        for(int i=0;i<3;i++) { aiRiskSliders[i].SetValueWithoutNotify(Manager.npcRiskTolerance[i]); aiRiskNumbers[i].text=Manager.npcRiskTolerance[i].ToString("0.00"); }
    }
    public void ApplyAISettings()
    {
        if(string.IsNullOrWhiteSpace(aiModelInput.text) || !int.TryParse(aiTokenInput.text,out int tokens) || !int.TryParse(aiTimeoutInput.text,out int seconds) || !int.TryParse(aiRetryInput.text,out int retries)) { aiFeedback.text="Enter a model and whole numbers."; return; }
        AIClient.model=aiModelInput.text.Trim(); AIClient.maxTokens=tokens; AIClient.timeoutSeconds=seconds; AIClient.retryCount=retries; AIClient.enableThinking=aiThinkingDraft; AIClient.thinkingEffort=aiEffortDraft; AIClient.extraInstructions=aiPromptInput.text;
        for(int i=0;i<3;i++) Manager.npcRiskTolerance[i]=aiRiskSliders[i].value;
        try { AIClient.SaveSettings(); SyncAIFields(); aiFeedback.text="Saved. Applied to the next API request."; } catch { aiFeedback.text="Settings could not be saved."; }
    }
    public void ToggleAILab()
    { if(!aiPanel) return; if(aiPanel.activeSelf) aiPanel.SetActive(false); else { ClosePicture(); rulesPanel.SetActive(false); historyPanel.SetActive(false); SyncAIFields(); aiPanel.SetActive(true); aiRevision=-1; RefreshAILab(); } }
    public void SelectAITab(int index)
    { aiTab=index; aiPromptArea.SetActive(false); aiScroll.gameObject.SetActive(true); aiRevision=-1; aiScroll.verticalNormalizedPosition=1; RefreshAILab(); }
    void MoveAITrace(int difference)
    { aiFollow=false; aiTraceIndex=Mathf.Clamp(aiTraceIndex+difference,0,AITraceStore.Records.Count-1); aiRevision=-1; aiScroll.verticalNormalizedPosition=1; }
    public void RefreshAILab()
    {
        if(!aiPanel || !aiPanel.activeSelf || Time.unscaledTime<aiRefreshAt) return; aiRefreshAt=Time.unscaledTime+.15f;
        if(aiFollow) aiTraceIndex=AITraceStore.Records.Count-1;
        var trace=aiTraceIndex>=0 && aiTraceIndex<AITraceStore.Records.Count ? AITraceStore.Records[aiTraceIndex] : null;
        aiPager.text=trace==null ? "No requests" : "#"+trace.id+" / "+AITraceStore.Records.Count;
        aiHeader.text=trace==null ? AIClient.ConnectionStatus : trace.role+" / HTTP "+(trace.source=="PENDING" ? "..." : trace.http.ToString())+" / "+trace.source+"\n"+(trace.source=="PENDING" ? Time.realtimeSinceStartup-trace.startedAt : trace.elapsed).ToString("0.0")+"s / tokens "+trace.inputTokens+" + "+trace.outputTokens;
        if(aiRevision==AITraceStore.Revision) return; aiRevision=AITraceStore.Revision;
        string text;
        if(trace==null) text="Play a turn to create an API request.\n\nThis developer view includes private dice, provider reasoning and full output.\n\nNo credentials are shown.";
        else switch(aiTab)
        {
            case 0:text="PROVIDER REASONING\n"+trace.status+"\n\n"+(trace.reasoning.Length>0 ? trace.reasoning : trace.thinking ? "No reasoning has been returned yet." : "Thinking disabled for this request."); break;
            case 1:text="MODEL OUTPUT / "+trace.finishReason+"\n\n"+(trace.output.Length>0 ? trace.output : "No output has been returned yet."); break;
            case 2:text="EXACT REQUEST BODY / NO AUTH HEADER\n\n"+PrettyJSON(trace.request); break;
            case 3:text="APPLIED IN GAME / "+trace.source+"\n\n"+(trace.applied.Length>0 ? trace.applied : "Waiting for a validated action."); break;
            default:text="RAW PROVIDER STREAM\n\n"+trace.raw; break;
        }
        RenderAITrace(AITraceStore.Redact(text));
    }
    void RenderAITrace(string text)
    {
        // Each UGUI Text mesh has a 65k vertex limit. Preserve the whole trace
        // in bounded chunks rather than handing a long stream to one mesh.
        int start=0,index=0; float offset=4;
        do
        {
            int count=Mathf.Min(5000,text.Length-start);
            if(start+count<text.Length)
            {
                int boundary=text.LastIndexOf('\n',start+count-1,count);
                if(boundary<start+count/2) boundary=text.LastIndexOf(' ',start+count-1,count);
                if(boundary>=start+count/2) count=boundary-start+1;
            }
            if(index==aiTraceChunks.Count)
            {
                var rt=Rect(aiScroll.content,"Trace text "+index,0,0,364,14); rt.anchorMin=rt.anchorMax=new Vector2(0,1); rt.pivot=new Vector2(0,1);
                var label=rt.gameObject.AddComponent<Text>(); label.font=pixelFont; label.fontSize=12; label.color=OneBitPresentation.Ink; label.supportRichText=false; label.raycastTarget=false; label.alignment=TextAnchor.UpperLeft; label.verticalOverflow=VerticalWrapMode.Overflow; label.material=HUDMaterial;
                aiTraceChunks.Add(label);
            }
            var chunk=aiTraceChunks[index++]; chunk.gameObject.SetActive(true); chunk.text=text.Substring(start,count);
            float height=Mathf.Ceil(Mathf.Max(14,chunk.preferredHeight)); chunk.rectTransform.anchoredPosition=new Vector2(4,-offset); chunk.rectTransform.sizeDelta=new Vector2(364,height); offset+=height; start+=count;
        } while(start<text.Length);
        for(int i=index;i<aiTraceChunks.Count;i++) aiTraceChunks[i].gameObject.SetActive(false);
        aiScroll.content.sizeDelta=new Vector2(372,Mathf.Max(198,offset+4)); Canvas.ForceUpdateCanvases();
    }
    static string PrettyJSON(string json)
    { try { return JsonUtility.ToJson(JsonUtility.FromJson<DeepSeekClient.Request>(json),true); } catch { return json; } }
}
