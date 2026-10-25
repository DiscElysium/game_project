using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerUIController
{
    public GameObject picturePanel;
    public InputField keyInput;
    public readonly List<Slider> pictureSliders=new List<Slider>();
    Text rasterLabel, connectionLabel, patternLabel;
    readonly List<Text> sliderValues=new List<Text>();
    string keyFeedback="";
    OneBitToneSettings Tone => OneBitToneSettings.Instance ? OneBitToneSettings.Instance : FindFirstObjectByType<OneBitToneSettings>();
    void BuildPictureControls()
    {
        pictureSliders.Clear(); sliderValues.Clear();
        picturePanel=Panel(layout,"Picture settings",286,18,342,290).gameObject; var root=picturePanel.transform;
        Label(root,"Title","PICTURE & CONNECTION",14,258,240,20,12);
        Label(root,"Hint","F2 / ESC",260,261,68,16,12,TextAnchor.MiddleRight);
        rasterLabel=Label(root,"Raster","",14,238,314,16,12);
        ButtonAt(root,"Auto pixels","AUTO",14,214,100,20,()=>SetDetail(0),12);
        ButtonAt(root,"Classic pixels","CLASSIC",120,214,100,20,()=>SetDetail(1),12);
        ButtonAt(root,"Fine pixels","FINE 2x",226,214,102,20,()=>SetDetail(2),12);
        AddPictureSlider(root,"Black",190,0,.45f,0);
        AddPictureSlider(root,"White",168,.5f,1,1);
        AddPictureSlider(root,"Exposure",146,.5f,2,2);
        AddPictureSlider(root,"Midtones",124,.5f,1.8f,3);
        AddPictureSlider(root,"Dither",102,1,3,4);
        patternLabel=Label(root,"Threshold","",14,80,176,16,12);
        ButtonAt(root,"Dither 4","4��4",194,80,62,18,()=>SetPattern(4),12);
        ButtonAt(root,"Dither 8","8��8",262,80,66,18,()=>SetPattern(8),12);
        connectionLabel=Label(root,"API state","",14,60,314,16,12);
        keyInput=SpeechInput(root,14,36,314,20); keyInput.characterLimit=160; keyInput.contentType=InputField.ContentType.Password;
        ((Text)keyInput.placeholder).text="AI is configured by the website server."; keyInput.interactable=false;
        ButtonAt(root,"Save connection","CHECK AI",14,10,120,20,()=>StartCoroutine(Manager.apiClient.CheckConnection()),12);
        ButtonAt(root,"Check connection","CHECK",140,10,90,20,()=> { keyFeedback=""; StartCoroutine(Manager.apiClient.CheckConnection()); },12);
        ButtonAt(root,"Reset picture","RESET",236,10,92,20,ResetPicture,12);
        SyncPicture(); picturePanel.SetActive(false);
    }
    void AddPictureSlider(Transform root,string title,int y,float min,float max,int index)
    {
        Label(root,title,title,14,y,72,18,12);
        var number=Label(root,title+" value","",278,y,50,18,12,TextAnchor.MiddleRight); sliderValues.Add(number);
        var rt=Rect(root,title+" slider",90,y,178,18); var track=Rect(rt,"Track",0,7,178,4); var trackImage=track.gameObject.AddComponent<Image>(); trackImage.color=OneBitPresentation.Ink; trackImage.raycastTarget=true;
        var slide=rt.gameObject.AddComponent<Slider>(); slide.minValue=min; slide.maxValue=max; slide.wholeNumbers=index==4; slide.transition=Selectable.Transition.None;
        var fillArea=Rect(rt,"Fill area",0,7,178,4); var fill=Rect(fillArea,"Fill",0,0,178,4); fill.anchorMax=Vector2.one; fill.offsetMin=fill.offsetMax=Vector2.zero; var fillImage=fill.gameObject.AddComponent<Image>(); fillImage.color=OneBitPresentation.Ink; fillImage.raycastTarget=false; slide.fillRect=fill;
        var handleArea=Rect(rt,"Handle area",4,0,170,18); var handle=Rect(handleArea,"Handle",0,3,8,12); handle.anchorMin=handle.anchorMax=new Vector2(.5f,0); handle.pivot=new Vector2(.5f,0);
        var knob=handle.gameObject.AddComponent<Image>(); knob.color=OneBitPresentation.Ink; knob.raycastTarget=true; slide.handleRect=handle; slide.targetGraphic=knob;
        slide.onValueChanged.AddListener(value=> { if(!Tone) return; var v=Tone.values; switch(index) { case 0:v.black=value;break;case 1:v.white=value;break;case 2:v.exposure=value;break;case 3:v.gamma=value;break;case 4:v.ditherPitch=Mathf.RoundToInt(value);break; } Tone.Apply(); SyncPicture(); });
        pictureSliders.Add(slide);
    }
    public void TogglePicture() { if(!picturePanel) return; if(picturePanel.activeSelf) ClosePicture(); else { if(aiPanel) aiPanel.SetActive(false); SyncPicture(); picturePanel.SetActive(true); } }
    public void ClosePicture() { if(!picturePanel) return; if(picturePanel.activeSelf && Tone) Tone.Save(); picturePanel.SetActive(false); if(keyInput) keyInput.text=""; }
    void SetDetail(int detail) { if(!Tone) return; Tone.values.detail=detail; Tone.Apply(); Tone.Save(); if(OneBitPresentation.Instance) OneBitPresentation.Instance.RefreshViewport(); }
    void SetPattern(int size) { if(!Tone) return; Tone.values.patternSize=size; Tone.Apply(); Tone.Save(); }
    void ResetPicture() { if(Tone) { Tone.ResetDefaults(); SyncPicture(); if(OneBitPresentation.Instance) OneBitPresentation.Instance.RefreshViewport(); } }
    void SyncPicture()
    {
        if(!Tone || pictureSliders.Count!=5) return;
        var v=Tone.values; float[] numbers={v.black,v.white,v.exposure,v.gamma,v.ditherPitch};
        for(int i=0;i<5;i++) { pictureSliders[i].SetValueWithoutNotify(numbers[i]); sliderValues[i].text=i==4 ? numbers[i].ToString("0") : numbers[i].ToString("0.00"); }
    }
    void SaveAndConnect()
    {
        try { if(!string.IsNullOrWhiteSpace(keyInput.text)) DeepSeekCredentialStore.Save(keyInput.text); keyInput.text=""; keyFeedback=""; StartCoroutine(Manager.apiClient.CheckConnection()); }
        catch { keyFeedback="Invalid key format. Try again."; }
    }
    void RefreshPicture()
    {
        if(Manager.apiClient && apiStatusText) apiStatusText.text=Manager.apiClient.DisplayStatus;
        if(apiStatusText) apiStatusText.transform.parent.gameObject.SetActive(false);
        if(!picturePanel || !picturePanel.activeSelf || !Tone) return;
        var p=OneBitPresentation.Instance;
        if(p) rasterLabel.text=$"{p.RenderWidth} �� {p.RenderHeight} / integer zoom {p.PixelScale}x";
        patternLabel.text=$"Bayer {Tone.values.patternSize}x{Tone.values.patternSize} / aligned";
        connectionLabel.text=keyFeedback.Length>0 ? keyFeedback : Manager.apiClient.ConnectionStatus;
    }
    void OnApplicationQuit() { if(Tone) Tone.Save(); }
}
