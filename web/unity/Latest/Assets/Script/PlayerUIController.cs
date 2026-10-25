using System.Collections;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class PlayerUIController : MonoBehaviour
{
    public CinemachineBrain cinemachineBrain;
    public CinemachineVirtualCamera playerVcam, overviewVcam;
    public CinemachineVirtualCamera[] npcViewVcams;
    public GameObject[] bidPointObjects, xrayTargetObjects, stealTargetObjects, increaseActionObjects, decreaseActionObjects, bidActionObjects, challengeActionObjects, rewardDiceArray;
    public Transform[] rewardDiceTargetPositions;
    public float defaultBlendTime=.8f, fastBlendTime=.3f, xrayViewDuration=3, animationDuration=1;
    public Text timerText, decisionResultText, winLoseText, resultDetailsText;
    public GameObject gameOverPanel, specialActionPanel, targetSelectPanel, guessInputPanel;
    public Button btnRestart, btnStartGame;
    public TMP_InputField inputQuantity, inputChat;
    public TextMesh quantityText;
    public Font pixelFont;
    public Material pixelUIMaterial;
    public GameObject hudRoot;
    public GameObject quantitySelector;
    public Sprite panelArtwork;
    Material artworkMaterial;
    public Text currentBidText, historyText, noticeText, apiStatusText, observationName, observationDialogue, observationText, draftQuantityText, phaseText;
    public InputField speechInput;
    public GameObject welcomePanel, rulesPanel, feedbackPanel, historyPanel, chatPanel, hoverPanel;
    public Text hoverText, quantityMark;
    public RectTransform layout;
    enum Phase { Waiting, Bidding, Special, Guessing, Viewing }
    Phase phase;
    int draft=1, face=1, selectedTarget=1;
    float noticeUntil, observationUntil;
    Material hudPixelMaterial;
    Text lookModeText;
    SeatedLookController Look => Manager ? Manager.GetComponent<SeatedLookController>() : null;
    public Material HUDMaterial => hudPixelMaterial ? hudPixelMaterial : pixelUIMaterial;
    LiarGameManager Manager => LiarGameManager.Instance ? LiarGameManager.Instance : FindFirstObjectByType<LiarGameManager>();
    public int DraftQuantity => draft;
    public int DraftFace => face;
    public bool LookMenusOpen => rulesPanel.activeSelf || historyPanel.activeSelf || gameOverPanel.activeSelf || chatPanel.activeSelf || (picturePanel && picturePanel.activeSelf) || (aiPanel && aiPanel.activeSelf);
    public bool IsViewing => phase==Phase.Viewing;
    public bool BlocksWorldInput => LookMenusOpen || welcomePanel.activeSelf || (Look && Look.Observing);

    void Awake() { BuildUI(); }
    void Start() { SwitchToCamera(false); }
    void Update()
    {
        if(!Manager) return;
        bool typing=chatPanel.activeSelf || (keyInput && keyInput.isFocused) || (aiPanel && aiPanel.activeSelf);
        if(Input.GetKeyDown(KeyCode.H) && !typing) historyPanel.SetActive(!historyPanel.activeSelf);
        if(Input.GetKeyDown(KeyCode.F1) && !typing) rulesPanel.SetActive(!rulesPanel.activeSelf);
        if(Input.GetKeyDown(KeyCode.F2)) TogglePicture();
        if(Input.GetKeyDown(KeyCode.F3)) ToggleAILab();
        if(Input.GetKeyDown(KeyCode.Escape)) { rulesPanel.SetActive(false); historyPanel.SetActive(false); chatPanel.SetActive(false); ClosePicture(); if(aiPanel) aiPanel.SetActive(false); }
        RefreshPicture();
        RefreshAILab();
        if(lookModeText) lookModeText.text=Look && Look.Observing ? "OBSERVE / RMB LOOK / TAB TABLE" : "TABLE / TAB OBSERVE";
        if(Input.GetKeyDown(KeyCode.T) && Manager.IsPlayerTurn && phase==Phase.Bidding && !chatPanel.activeSelf && !typing)
        { chatPanel.SetActive(true); speechInput.ActivateInputField(); }
        if(chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Return)) { chatPanel.SetActive(false); ShowNotice("Your words will accompany the next bid."); }
        feedbackPanel.SetActive(Time.unscaledTime<noticeUntil);
        if(Manager.microExpressionBg) Manager.microExpressionBg.SetActive((Time.unscaledTime<observationUntil && !Manager.IsPlayerTurn) || (Input.GetKey(KeyCode.Space) && !string.IsNullOrEmpty(observationDialogue.text)));
        timerText.text=Manager.IsRoundOver ? "" : Manager.IsPlayerTurn ? "YOUR TURN" : "THINKING...";
        timerText.transform.parent.gameObject.SetActive(!Manager.IsRoundOver);
    }
    public void BuildUI()
    {
        if(hudPixelMaterial) Destroy(hudPixelMaterial);
        if(artworkMaterial) { if(Application.isPlaying) Destroy(artworkMaterial); else DestroyImmediate(artworkMaterial); }
        if(Application.isPlaying && pixelUIMaterial) hudPixelMaterial=new Material(pixelUIMaterial);
        if(pixelUIMaterial) { artworkMaterial=new Material(pixelUIMaterial); artworkMaterial.SetFloat("_PaletteTexture",2); }
        if(hudRoot) { hudRoot.SetActive(false); if(Application.isPlaying) Destroy(hudRoot); else DestroyImmediate(hudRoot); }
        foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            if(canvas.name!="Pixel scene output") canvas.enabled=false;
        foreach(var oldText in FindObjectsByType<TextMesh>(FindObjectsInactive.Include,FindObjectsSortMode.None)) oldText.gameObject.SetActive(false);
        var oldMark=transform.Find("Table quantity");
        if(oldMark) { oldMark.gameObject.SetActive(false); if(Application.isPlaying) Destroy(oldMark.gameObject); else DestroyImmediate(oldMark.gameObject); }
        hudRoot=new GameObject("OneBitHUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); hudRoot.layer=5;
        var canvasNew=hudRoot.GetComponent<Canvas>(); canvasNew.renderMode=RenderMode.ScreenSpaceCamera; canvasNew.worldCamera=Camera.main; canvasNew.planeDistance=1; canvasNew.pixelPerfect=true; canvasNew.sortingOrder=100;
        hudRoot.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
        layout=Rect(hudRoot.transform,"Layout",0,0,640,360); layout.anchorMin=layout.anchorMax=new Vector2(.5f,.5f); layout.pivot=new Vector2(.5f,.5f);
        if(!FindFirstObjectByType<EventSystem>()) new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        if(pixelFont && pixelFont.material.mainTexture) pixelFont.material.mainTexture.filterMode=FilterMode.Point;
        var brand=Panel(layout,"Signature",12,312,128,36); Label(brand,"Title","MASKED DICE",10,17,108,16,12);
        Label(brand,"Edition","MASKED TABLE",10,5,108,14,12);
        var bid=Panel(layout,"Current bid",324,312,224,36); Label(bid,"Caption","CURRENT BID",10,19,204,14,12); currentBidText=Label(bid,"Bid","No bid yet",10,5,204,16,12,TextAnchor.MiddleRight);
        ButtonAt(layout,"Help","F1 HELP",560,312,68,16,()=>rulesPanel.SetActive(!rulesPanel.activeSelf),12);
        ButtonAt(layout,"Picture","F2 TONE",560,332,68,16,TogglePicture,12);
        ButtonAt(layout,"AI settings","F3 AI",560,292,68,16,ToggleAILab,12);
        ButtonAt(layout,"Look mode","LOOK/TAB",560,272,68,16,()=> { if(Look) Look.Toggle(); },12);
        var lookHint=Panel(layout,"Look mode plaque",12,6,234,16);
        lookModeText=Label(lookHint,"Look hint","TABLE / TAB OBSERVE",7,0,220,16,12);
        var turn=Panel(layout,"Turn",12,292,128,16); timerText=Label(turn,"Text","",8,1,112,14,12);
        Manager.statusText=Label(layout,"Status","",12,290,390,14,8); Manager.statusText.gameObject.SetActive(false);
        var seal=Panel(layout,"Connection seal",352,292,276,16); apiStatusText=Label(seal,"Connection","Checking connection",7,1,262,14,12,TextAnchor.MiddleRight);
        var observation=Panel(layout,"Whisper",102,14,436,96);
        observationName=Label(observation,"Speaker","",16,68,404,16,12);
        observationDialogue=Label(observation,"Dialogue","",16,40,404,28,12);
        observationText=Label(observation,"Expression","",16,10,404,28,12);
        Manager.microExpressionText=observationText; Manager.microExpressionBg=observation.gameObject; observation.gameObject.SetActive(false);
        decisionResultText=Label(layout,"Decision","",12,256,350,14,8); decisionResultText.gameObject.SetActive(false);
        var notice=Panel(layout,"Notice",110,66,420,18); noticeText=Label(notice,"Text","",8,1,404,16,12,TextAnchor.MiddleCenter);
        notice.gameObject.SetActive(false); feedbackPanel=notice.gameObject;
        hoverPanel=Panel(layout,"Hover",12,90,300,18).gameObject; hoverText=Label(hoverPanel.transform,"Text","",7,1,286,16,12); hoverPanel.SetActive(false);
        historyPanel=Panel(layout,"Ledger",348,140,280,182).gameObject;
        Label(historyPanel.transform,"Title","TABLE LEDGER / H",10,154,260,18,12);
        historyText=Label(historyPanel.transform,"History","No entries yet.",10,12,260,138,12,TextAnchor.UpperLeft); historyPanel.SetActive(false);
        chatPanel=Panel(layout,"Say something",134,12,372,56).gameObject;
        Label(chatPanel.transform,"Hint","SPEAK / ENTER to confirm / ESC to close",10,34,352,16,12);
        speechInput=SpeechInput(chatPanel.transform,10,10,352,22); chatPanel.SetActive(false);
        welcomePanel=Rect(layout,"Welcome",0,0,640,360).gameObject;
        var welcome=Panel(welcomePanel.transform,"Intro",176,20,288,76);
        Label(welcome,"Copy","EVERY CUP HOLDS A SECRET.",16,43,256,16,12,TextAnchor.MiddleCenter);
        btnStartGame=ButtonAt(welcome,"Start","TAKE A SEAT",72,14,144,20,StartGame,12);
        rulesPanel=Panel(layout,"Rulebook",156,37,328,286).gameObject;
        Label(rulesPanel.transform,"Title","THE RULES",14,254,300,24,16);
        Label(rulesPanel.transform,"Rules","Choose a face; use the register for count.\nRing the bell to bid; call bluff to reveal.\nRaise count, or raise face at equal count.\n\nOrdinary matching dice count 1.\nThe bidder's matching special dice count 3.\nOther special dice count 0.\nA count meeting the bid wins for the bidder.\nAfter bidding: peek, steal, or ring to pass.\nTo steal, choose a face and guess.\n\nTAB observe / table; hold RMB to look.\nArrow keys look / R face north.\nT speak / H ledger / SPACE recall\nF2 picture / F3 AI lab",14,40,300,210,12,TextAnchor.UpperLeft);
        ButtonAt(rulesPanel.transform,"Close","RETURN TO TABLE",14,12,300,20,()=>rulesPanel.SetActive(false),12); rulesPanel.SetActive(false);
        gameOverPanel=Panel(layout,"Settlement",168,68,304,224).gameObject;
        winLoseText=Label(gameOverPanel.transform,"Title","ROUND OVER",12,188,280,24,16);
        resultDetailsText=Label(gameOverPanel.transform,"Result","",12,48,280,138,12,TextAnchor.UpperLeft);
        btnRestart=ButtonAt(gameOverPanel.transform,"Restart","PLAY AGAIN",12,14,280,24,StartGame,12); gameOverPanel.SetActive(false);
        Manager.playerPanel=Rect(layout,"Turn flag",0,0,0,0).gameObject;
        PrepareItems(); BuildTableQuantity(); BuildPeekLabels(); BuildPictureControls(); BuildAILab(); phase=Phase.Waiting; SetPhysicalPhase(); RefreshBid(); RefreshHistory();
        if(HUDMaterial) foreach(var graphic in hudRoot.GetComponentsInChildren<Graphic>(true)) graphic.material=graphic is Image art && art.sprite==panelArtwork && panelArtwork ? artworkMaterial : HUDMaterial;
    }
    void PrepareItems()
    {
        Bind(bidPointObjects,PointSelectorItem.ItemType.BidPoint,true); Bind(xrayTargetObjects,PointSelectorItem.ItemType.XRayTarget,true);
        Bind(stealTargetObjects,PointSelectorItem.ItemType.StealTarget,true); Bind(increaseActionObjects,PointSelectorItem.ItemType.IncreaseQuantity,false);
        Bind(decreaseActionObjects,PointSelectorItem.ItemType.DecreaseQuantity,false); Bind(bidActionObjects,PointSelectorItem.ItemType.BidAction,false);
        Bind(challengeActionObjects,PointSelectorItem.ItemType.ChallengeAction,false); SetGroup(rewardDiceArray,false);
    }
    void Bind(GameObject[] objects,PointSelectorItem.ItemType type,bool numbered)
    {
        if(objects==null) return;
        for(int i=0;i<objects.Length;i++) if(objects[i])
        {
            var item=objects[i].GetComponent<PointSelectorItem>(); if(!item) item=objects[i].AddComponent<PointSelectorItem>();
            item.itemType=type; if(numbered) item.value=i+1; item.hoverOffset=.8f;
            if(objects[i].GetComponentsInChildren<Collider>(true).Length==0)
            {
                var renderers=objects[i].GetComponentsInChildren<Renderer>(true);
                if(renderers.Length>0)
                {
                    var bounds=renderers[0].bounds; foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                    var collider=objects[i].AddComponent<BoxCollider>(); var local=new Bounds(objects[i].transform.InverseTransformPoint(bounds.min),Vector3.zero);
                    for(int a=0;a<8;a++) local.Encapsulate(objects[i].transform.InverseTransformPoint(new Vector3((a&1)==0 ? bounds.min.x : bounds.max.x,(a&2)==0 ? bounds.min.y : bounds.max.y,(a&4)==0 ? bounds.min.z : bounds.max.z)));
                    collider.center=local.center; collider.size=local.size;
                }
            }
        }
    }
    void BuildTableQuantity()
    {
        if(!quantitySelector) return;
        foreach(var canvas in quantitySelector.GetComponentsInChildren<Canvas>(true)) { if(Application.isPlaying) Destroy(canvas.gameObject); else DestroyImmediate(canvas.gameObject); }
        quantityMark=TableLabel("COUNT_Seat","Quantity","01",44,44,24,.09f);
        TableLabel("COUNT_CaptionSeat","Caption","COUNT",40,16,12,.07f);
        TableLabel("BUTTON_MINUS_LabelSeat","Minus","MINUS",38,16,12,.07f);
        TableLabel("BUTTON_PLUS_LabelSeat","Plus","PLUS",38,16,12,.07f);
    }
    Text TableLabel(string seat,string name,string text,float w,float h,int size,float scale)
    {
        Transform anchor=null; foreach(var child in quantitySelector.GetComponentsInChildren<Transform>(true)) if(child.name==seat) { anchor=child; break; }
        if(!anchor) return null; return WorldLabel(anchor,name,text,w,h,size,scale);
    }
    Text WorldLabel(Transform anchor,string name,string text,float w,float h,int size,float scale)
    {
        var mark=new GameObject(name+" engraving",typeof(RectTransform),typeof(Canvas)); mark.layer=5; mark.transform.SetParent(anchor,false);
        mark.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace; mark.transform.rotation=Quaternion.Euler(90,0,0); mark.transform.localScale=Vector3.one*scale;
        mark.GetComponent<RectTransform>().sizeDelta=new Vector2(w,h);
        var label=Label(mark.transform,name,text,0,0,w,h,size,TextAnchor.MiddleCenter); label.material=pixelUIMaterial; if(name=="Caption") label.color=OneBitPresentation.Paper; return label;
    }
    void BuildPeekLabels()
    {
        for(int i=0;i<xrayTargetObjects.Length;i++)
        {
            foreach(var canvas in xrayTargetObjects[i].GetComponentsInChildren<Canvas>(true)) { if(Application.isPlaying) Destroy(canvas.gameObject); else DestroyImmediate(canvas.gameObject); }
            foreach(var child in xrayTargetObjects[i].GetComponentsInChildren<Transform>(true)) if(child.name=="PEEK_LabelSeat") { WorldLabel(child,"PEEK label","PEEK "+(i+1),54,18,12,.08f); break; }
        }
    }
    void StartGame() { if(Manager.diceManager.isRolling) return; StopAllCoroutines(); Manager.StartNewRound(); }
    public void ResetRoundUI()
    {
        if(Look) Look.SetObservation(false,false);
        welcomePanel.SetActive(false); gameOverPanel.SetActive(false); rulesPanel.SetActive(false); historyPanel.SetActive(false); chatPanel.SetActive(false);
        phase=Phase.Waiting; SetPhysicalPhase(); speechInput.text=""; observationDialogue.text=observationText.text=""; observationUntil=noticeUntil=0; feedbackPanel.SetActive(false); RefreshBid(); RefreshHistory();
    }
    public void BeginPlayerTurn()
    {
        phase=Phase.Bidding; draft=Mathf.Max(1,Manager.GetCurrentMatchBidQuantity()); face=Manager.GetCurrentMatchBidPoint()==6 ? 1 : Mathf.Max(1,Manager.GetCurrentMatchBidPoint()+1);
        if(Manager.GetCurrentMatchBidPoint()==6) draft++;
        quantityMark.text=draft.ToString("D2"); SetPhysicalPhase(); ShowNotice("Use the table / T speak / H ledger");
    }
    static void SetGroup(GameObject[] objects,bool active)
    { if(objects==null) return; foreach(var obj in objects) if(obj) { var item=obj.GetComponent<PointSelectorItem>(); if(item) item.ResetPose(); obj.SetActive(active); } }
    void SetPhysicalPhase()
    {
        SetGroup(bidPointObjects,phase==Phase.Bidding || phase==Phase.Guessing); SetGroup(increaseActionObjects,phase==Phase.Bidding); SetGroup(decreaseActionObjects,phase==Phase.Bidding);
        SetGroup(bidActionObjects,phase==Phase.Bidding || phase==Phase.Special || phase==Phase.Guessing); SetGroup(challengeActionObjects,phase==Phase.Bidding && Manager.HasCurrentBid);
        SetGroup(xrayTargetObjects,true); SetGroup(stealTargetObjects,phase==Phase.Special);
        if(quantitySelector) quantitySelector.SetActive(phase==Phase.Bidding); hoverPanel.SetActive(false);
    }
    public bool CanInteract(PointSelectorItem item)
    {
        if(!Manager.IsPlayerTurn || BlocksWorldInput) return false;
        if(phase==Phase.Bidding) return item.itemType==PointSelectorItem.ItemType.BidPoint || item.itemType==PointSelectorItem.ItemType.IncreaseQuantity || item.itemType==PointSelectorItem.ItemType.DecreaseQuantity || item.itemType==PointSelectorItem.ItemType.BidAction || (item.itemType==PointSelectorItem.ItemType.ChallengeAction && Manager.HasCurrentBid);
        if(phase==Phase.Special) return item.itemType==PointSelectorItem.ItemType.XRayTarget || item.itemType==PointSelectorItem.ItemType.StealTarget || item.itemType==PointSelectorItem.ItemType.BidAction;
        return phase==Phase.Guessing && (item.itemType==PointSelectorItem.ItemType.BidPoint || item.itemType==PointSelectorItem.ItemType.BidAction);
    }
    public void OnObjectSelected(PointSelectorItem item)
    {
        if(!item || !CanInteract(item)) return;
        switch(item.itemType)
        {
            case PointSelectorItem.ItemType.BidPoint:
                if(phase==Phase.Guessing) { bool success=Manager.PlayerGuessSpecialDie(selectedTarget,item.value); ShowNotice(success ? "Correct. The special die is yours." : "Wrong. The secret stays with him."); FinishSpecial(); }
                else { face=item.value; ShowNotice("Face "+face+" selected"); } break;
            case PointSelectorItem.ItemType.IncreaseQuantity: draft=Mathf.Min(draft+1,Manager.MaximumQuantity(0)); quantityMark.text=draft.ToString("D2"); break;
            case PointSelectorItem.ItemType.DecreaseQuantity: draft=Mathf.Max(1,draft-1); quantityMark.text=draft.ToString("D2"); break;
            case PointSelectorItem.ItemType.BidAction:
                if(phase!=Phase.Bidding) { FinishSpecial(); break; }
                if(Manager.TryPlayerBid(draft,face,GetHumanChat())) { speechInput.text=""; phase=Phase.Special; SetPhysicalPhase(); ShowNotice("Choose a tool, or ring the bell to pass."); } break;
            case PointSelectorItem.ItemType.ChallengeAction: Manager.PlayerChallenge(); break;
            case PointSelectorItem.ItemType.XRayTarget:
                phase=Phase.Viewing; SetPhysicalPhase(); Manager.PlayerUseXRay(item.value); ShowNotice("Peek inside the cup / 3 seconds"); StartCoroutine(EndAfterView()); break;
            case PointSelectorItem.ItemType.StealTarget:
                selectedTarget=item.value; phase=Phase.Guessing; SetPhysicalPhase(); ShowNotice("Guess the face / choose a die on the table"); break;
        }
    }
    IEnumerator EndAfterView() { yield return new WaitForSeconds(xrayViewDuration); FinishSpecial(); }
    void FinishSpecial() { phase=Phase.Waiting; SetPhysicalPhase(); Manager.FinishPlayerTurn(); SwitchToCamera(false); }
    public void ShowHover(PointSelectorItem item,Vector2 nativePoint)
    {
        if(!item || !CanInteract(item)) { hoverPanel.SetActive(false); return; }
        string text;
        switch(item.itemType)
        {
            case PointSelectorItem.ItemType.BidPoint: text=phase==Phase.Guessing ? "Guess "+item.value : "Face "+item.value+" / draft "+draft+" x "+face; break;
            case PointSelectorItem.ItemType.IncreaseQuantity: text="Count +1 / draft "+draft+" x "+face; break;
            case PointSelectorItem.ItemType.DecreaseQuantity: text="Count -1 / draft "+draft+" x "+face; break;
            case PointSelectorItem.ItemType.BidAction: text=phase==Phase.Bidding ? "Bid "+draft+" x "+face : "Keep the secret / pass"; break;
            case PointSelectorItem.ItemType.ChallengeAction: text="Call bluff / open the cups"; break;
            case PointSelectorItem.ItemType.XRayTarget: text="Peek / "+Manager.GetPlayerName(item.value); break;
            default: text="Steal / "+Manager.GetPlayerName(item.value); break;
        }
        hoverText.text=text; hoverPanel.SetActive(true);
        var rect=hoverPanel.GetComponent<RectTransform>(); rect.sizeDelta=new Vector2(Mathf.Clamp(text.Length*6+14,96,300),18);
        rect.anchoredPosition=new Vector2(Mathf.Round(Mathf.Clamp(nativePoint.x+10,12,628-rect.sizeDelta.x)),Mathf.Round(Mathf.Clamp(nativePoint.y+14,76,302)));
    }
    public void ShowNotice(string text)
    {
        noticeText.text=OneBitRules.SafeText(text,100); noticeUntil=Time.unscaledTime+4; feedbackPanel.SetActive(true);
        float width=Mathf.Clamp(noticeText.text.Length*6+16,128,500);
        var rect=feedbackPanel.GetComponent<RectTransform>(); rect.sizeDelta=new Vector2(width,18); rect.anchoredPosition=new Vector2(Mathf.Floor((640-width)*.5f),66);
        noticeText.rectTransform.sizeDelta=new Vector2(width-16,16);
    }
    public void ShowObservation(int player,string dialogue,string expression)
    {
        observationName.text=Manager.GetPlayerName(player).ToUpperInvariant(); observationDialogue.text=OneBitRules.SafeText(dialogue,100); observationText.text=OneBitRules.SafeText(expression,140); observationUntil=Time.unscaledTime+10;
        float expressionHeight=Mathf.Ceil(Mathf.Max(14,observationText.preferredHeight));
        float dialogueHeight=Mathf.Ceil(Mathf.Max(14,observationDialogue.preferredHeight));
        observationText.rectTransform.sizeDelta=new Vector2(404,expressionHeight);
        observationDialogue.rectTransform.sizeDelta=new Vector2(404,dialogueHeight);
        observationDialogue.rectTransform.anchoredPosition=new Vector2(16,expressionHeight+14);
        observationName.rectTransform.anchoredPosition=new Vector2(16,expressionHeight+dialogueHeight+20);
        Manager.microExpressionBg.GetComponent<RectTransform>().sizeDelta=new Vector2(436,expressionHeight+dialogueHeight+48);
    }
    public void SetActivePlayer(int player) { if(player!=0) { phase=Phase.Waiting; SetPhysicalPhase(); } }
    public void RefreshBid() { var s=Manager.PublicState; currentBidText.text=s.currentBidQuantity==0 ? "No bid yet" : $"{s.currentBidPlayer} / {s.currentBidQuantity} x {s.currentBidPoint}"; }
    public void RefreshHistory()
    { var logs=Manager.PublicState.detailedLog; string text=""; for(int i=Mathf.Max(0,logs.Count-4);i<logs.Count;i++) text+=$"{logs[i].playerName}  {logs[i].details}\n\n"; historyText.text=text.Length==0 ? "No entries yet." : text; }
    public void RefreshHand() { }
    public void ShowGameOver(string title,string details) { phase=Phase.Waiting; SetPhysicalPhase(); Manager.playerPanel.SetActive(false); gameOverPanel.SetActive(true); winLoseText.text=title; resultDetailsText.text=details; }
    public string GetHumanChat() => OneBitRules.SafeText(speechInput.text,80);
    public void SwitchToCamera(bool playerTurn) { if(Look && Look.Observing) return; SwitchToCameraWithSpeed(playerTurn ? playerVcam : overviewVcam,false); }
    public void SwitchToNPCCamera(int index) { if(Look) Look.SetObservation(false,false); if(index>=1 && index<=npcViewVcams.Length) SwitchToCameraWithSpeed(npcViewVcams[index-1],true); }
    public void SwitchToCameraWithSpeed(CinemachineVirtualCamera target,bool fast)
    { if(!target) return; if(cinemachineBrain) cinemachineBrain.m_DefaultBlend.m_Time=fast ? fastBlendTime : defaultBlendTime; playerVcam.Priority=overviewVcam.Priority=10; foreach(var view in npcViewVcams) if(view) view.Priority=10; if(Look && Look.freeView) Look.freeView.Priority=1; target.Priority=20; }
    public void OnPointSelectedFrom3D(int point) { if(phase==Phase.Bidding) face=Mathf.Clamp(point,1,6); }
    public void ResetDraftQuantity() { draft=1; if(quantityMark) quantityMark.text="01"; }
    void OnDestroy() { if(hudPixelMaterial) Destroy(hudPixelMaterial); if(artworkMaterial) Destroy(artworkMaterial); }
    public void UpdateArtworkGrid(int scale,Vector4 origin)
    { if(artworkMaterial) { artworkMaterial.SetFloat("_PixelGridScale",scale); artworkMaterial.SetVector("_PixelGridOrigin",origin); } }
    RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
    { var obj=new GameObject(name,typeof(RectTransform)); obj.layer=parent.gameObject.layer; obj.transform.SetParent(parent,false); var rt=obj.GetComponent<RectTransform>(); rt.anchorMin=rt.anchorMax=Vector2.zero; rt.pivot=Vector2.zero; rt.anchoredPosition=new Vector2(x,y); rt.sizeDelta=new Vector2(w,h); return rt; }
    RectTransform Panel(Transform parent,string name,float x,float y,float w,float h)
    {
        var rt=Rect(parent,name,x,y,w,h); var image=rt.gameObject.AddComponent<Image>(); image.color=OneBitPresentation.Paper; image.raycastTarget=false;
        if(panelArtwork && h>=50 && w>100)
        { image.sprite=panelArtwork; image.type=Image.Type.Sliced; image.pixelsPerUnitMultiplier=8; image.color=Color.white; image.material=artworkMaterial; return rt; }
        FrameRule(rt,"Bottom",new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(-1,2));
        FrameRule(rt,"Top",new Vector2(0,1),new Vector2(1,1),new Vector2(1,-2),new Vector2(-1,-1));
        FrameRule(rt,"Left",new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(2,-1));
        FrameRule(rt,"Right",new Vector2(1,0),new Vector2(1,1),new Vector2(-2,1),new Vector2(-1,-1));
        if(h>=36 && w>100)
        {
            FrameRule(rt,"Inner bottom",new Vector2(0,0),new Vector2(1,0),new Vector2(4,4),new Vector2(-4,5));
            FrameRule(rt,"Inner top",new Vector2(0,1),new Vector2(1,1),new Vector2(4,-5),new Vector2(-4,-4));
        }
        return rt;
    }
    void FrameRule(Transform parent,string name,Vector2 min,Vector2 max,Vector2 lower,Vector2 upper)
    { var rt=Rect(parent,name,0,0,1,1); rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=lower; rt.offsetMax=upper; var image=rt.gameObject.AddComponent<Image>(); image.color=OneBitPresentation.Ink; image.raycastTarget=false; }
    void Rule(Transform parent,string name,float x,float y,float w,float h)
    { var rt=Rect(parent,name,x,y,w,h); var image=rt.gameObject.AddComponent<Image>(); image.color=OneBitPresentation.Ink; image.raycastTarget=false; }
    Text Label(Transform parent,string name,string value,float x,float y,float w,float h,int size,TextAnchor anchor=TextAnchor.MiddleLeft)
    { var rt=Rect(parent,name,x,y,w,h); var text=rt.gameObject.AddComponent<Text>(); text.font=pixelFont; text.fontSize=size; text.text=value; text.color=OneBitPresentation.Ink; text.alignment=anchor; text.supportRichText=false; text.raycastTarget=false; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate; return text; }
    Button ButtonAt(Transform parent,string name,string value,float x,float y,float w,float h,UnityAction action,int size)
    { var rt=Panel(parent,name,x,y,w,h); rt.GetComponent<Image>().raycastTarget=true; rt.GetComponent<Image>().color=OneBitPresentation.Ink; Label(rt,"Text",value,2,0,w-4,h,size,TextAnchor.MiddleCenter).color=OneBitPresentation.Paper; var button=rt.gameObject.AddComponent<Button>(); button.transition=Selectable.Transition.None; button.targetGraphic=rt.GetComponent<Image>(); button.onClick.AddListener(action); return button; }
    InputField SpeechInput(Transform parent,float x,float y,float w,float h)
    { var rt=Panel(parent,"Human dialogue",x,y,w,h); rt.GetComponent<Image>().raycastTarget=true; var input=rt.gameObject.AddComponent<InputField>(); input.textComponent=Label(rt,"Text","",4,2,w-8,h-4,12); input.placeholder=Label(rt,"Placeholder","Say something to the table...",4,2,w-8,h-4,12); input.characterLimit=80; input.customCaretColor=true; input.caretColor=OneBitPresentation.Ink; input.selectionColor=OneBitPresentation.Ink; input.lineType=InputField.LineType.SingleLine; return input; }
}
