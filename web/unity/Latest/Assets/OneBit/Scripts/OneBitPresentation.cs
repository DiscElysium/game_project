using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public sealed class OneBitPresentation : MonoBehaviour
{
    public static OneBitPresentation Instance { get; private set; }
    public static readonly Color Ink=new Color32(44,27,23,255);
    public static readonly Color Paper=new Color32(238,220,186,255);
    public int verticalResolution=360;
    public const int NativeWidth=640, NativeHeight=360;
    public Rect ScreenViewport { get; private set; }
    public int PixelScale { get; private set; }
    public int UIScale { get; private set; }
    public int RenderWidth => pixels ? pixels.width : NativeWidth;
    public int RenderHeight => pixels ? pixels.height : NativeHeight;
    Camera gameCamera; RenderTexture pixels; GameObject screen, displayCamera; Material outputMaterial;
    RawImage sceneImage;
    RectTransform outputRect; PlayerUIController ui; PointSelectorItem hovered;
    readonly RaycastHit[] hits=new RaycastHit[64];
    void Awake() { Instance=this; Application.runInBackground=true; Application.targetFrameRate=60; }
    void Start()
    {
        gameCamera=Camera.main; if(!gameCamera) return;
        gameCamera.aspect=(float)NativeWidth/NativeHeight;
        gameCamera.allowHDR=gameCamera.allowMSAA=false; gameCamera.allowDynamicResolution=false;
        gameCamera.clearFlags=CameraClearFlags.SolidColor; gameCamera.backgroundColor=Ink;
        gameCamera.cullingMask &= ~((1<<30)|(1<<31));
        displayCamera=new GameObject("Pixel display camera",typeof(Camera));
        var output=displayCamera.GetComponent<Camera>(); output.depth=100; output.cullingMask=(1<<30)|(1<<31); output.orthographic=true;
        output.clearFlags=CameraClearFlags.SolidColor; output.backgroundColor=Ink; output.allowHDR=output.allowMSAA=false;
        screen=new GameObject("Pixel scene output",typeof(RectTransform),typeof(Canvas)); screen.layer=31;
        var canvas=screen.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceCamera; canvas.worldCamera=output; canvas.planeDistance=1; canvas.sortingOrder=-100; canvas.pixelPerfect=true;
        var imageObject=new GameObject("Scene",typeof(RectTransform),typeof(RawImage)); imageObject.layer=31; imageObject.transform.SetParent(screen.transform,false);
        outputRect=imageObject.GetComponent<RectTransform>(); outputRect.anchorMin=outputRect.anchorMax=outputRect.pivot=new Vector2(.5f,.5f);
        var image=imageObject.GetComponent<RawImage>(); sceneImage=image; image.raycastTarget=false;
        outputMaterial=new Material(Shader.Find("MaskedDice/PixelUI")); outputMaterial.SetFloat("_PaletteTexture",1); image.material=outputMaterial;
        ui=FindFirstObjectByType<PlayerUIController>();
        var hud=ui.hudRoot.GetComponent<Canvas>(); hud.renderMode=RenderMode.ScreenSpaceCamera; hud.worldCamera=output; hud.planeDistance=1; hud.pixelPerfect=true;
        foreach(var child in ui.hudRoot.GetComponentsInChildren<Transform>(true)) child.gameObject.layer=30;
        RefreshViewport();
    }
    public static Rect IntegerViewport(int width,int height,out int scale)
    {
        scale=Mathf.Max(1,Mathf.Min(width/NativeWidth,height/NativeHeight));
        return new Rect(Mathf.Floor((width-NativeWidth*scale)*.5f),Mathf.Floor((height-NativeHeight*scale)*.5f),NativeWidth*scale,NativeHeight*scale);
    }
    public void RefreshViewport()
    {
        ScreenViewport=IntegerViewport(Screen.width,Screen.height,out int scale);
        int detail=OneBitToneSettings.Instance ? OneBitToneSettings.Instance.values.detail : 0;
        // 0: fine pixels matching the display; 1: the original 640x360; 2: 2x detail.
        int multiplier=detail==0 ? scale : detail==1 ? 1 : Mathf.Min(2,scale);
        int zoom=Mathf.Max(1,scale/multiplier); scale=multiplier*zoom;
        ScreenViewport=new Rect(Mathf.Floor((Screen.width-NativeWidth*scale)*.5f),Mathf.Floor((Screen.height-NativeHeight*scale)*.5f),NativeWidth*scale,NativeHeight*scale);
        PixelScale=zoom; UIScale=scale;
        if(gameCamera && (!pixels || pixels.width!=NativeWidth*multiplier))
        {
            if(pixels) { gameCamera.targetTexture=null; pixels.Release(); Destroy(pixels); }
            pixels=new RenderTexture(NativeWidth*multiplier,NativeHeight*multiplier,24,RenderTextureFormat.ARGB32);
            pixels.filterMode=FilterMode.Point; pixels.wrapMode=TextureWrapMode.Clamp; pixels.antiAliasing=1; pixels.useMipMap=false; pixels.autoGenerateMips=false; pixels.Create();
            gameCamera.targetTexture=pixels; sceneImage.texture=pixels;
        }
        if(outputRect)
        {
            outputRect.sizeDelta=ScreenViewport.size;
            outputRect.anchoredPosition=ScreenViewport.center-new Vector2(Screen.width*.5f,Screen.height*.5f);
        }
        if(ui && ui.hudRoot)
        {
            ui.hudRoot.GetComponent<CanvasScaler>().scaleFactor=scale;
            ui.layout.anchoredPosition=(ScreenViewport.center-new Vector2(Screen.width*.5f,Screen.height*.5f))/scale;
            ui.HUDMaterial.SetFloat("_PixelGridScale",scale);
            ui.HUDMaterial.SetVector("_PixelGridOrigin",new Vector4(ScreenViewport.x,Screen.height-ScreenViewport.yMax,ScreenViewport.x,ScreenViewport.y));
            ui.UpdateArtworkGrid(scale,new Vector4(ScreenViewport.x,Screen.height-ScreenViewport.yMax,ScreenViewport.x,ScreenViewport.y));
        }
    }
    void LateUpdate()
    {
        if(!gameCamera || !pixels) return;
        RefreshViewport(); gameCamera.aspect=(float)NativeWidth/NativeHeight;
        var mouse=(Vector2)Input.mousePosition;
        var item=ui.BlocksWorldInput || (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) ? null : PickItemAt(mouse);
        if(hovered!=item) { if(hovered) hovered.SetHovered(false); hovered=item; if(hovered) hovered.SetHovered(true); }
        ui.ShowHover(item,(mouse-ScreenViewport.position)/UIScale);
        if(item && Input.GetMouseButtonDown(0)) ui.OnObjectSelected(item);
    }
    public PointSelectorItem PickItemAt(Vector2 screenPoint)
    {
        if(!gameCamera || !ScreenViewport.Contains(screenPoint)) return null;
        Vector2 native=(screenPoint-ScreenViewport.position)/PixelScale;
        // One pointer sample per rendered pixel, in exactly the shader's grid.
        native=new Vector2(Mathf.Floor(native.x)+.5f,Mathf.Floor(native.y)+.5f);
        var ray=gameCamera.ViewportPointToRay(new Vector3(native.x/RenderWidth,native.y/RenderHeight,0));
        int count=Physics.RaycastNonAlloc(ray,hits,5000,~((1<<30)|(1<<31)),QueryTriggerInteraction.Collide);
        PointSelectorItem result=null; float nearest=float.PositiveInfinity;
        for(int i=0;i<count;i++)
        {
            var item=hits[i].collider.GetComponentInParent<PointSelectorItem>();
            if(item && ui.CanInteract(item) && hits[i].distance<nearest) { result=item; nearest=hits[i].distance; }
        }
        return result;
    }
    public Vector2 WorldToScreen(Vector3 world)
    {
        var point=gameCamera.WorldToViewportPoint(world);
        return ScreenViewport.position+new Vector2(point.x*ScreenViewport.width,point.y*ScreenViewport.height);
    }
    void OnDestroy()
    {
        if(gameCamera) gameCamera.targetTexture=null;
        if(pixels) { pixels.Release(); Destroy(pixels); }
        if(screen) Destroy(screen); if(displayCamera) Destroy(displayCamera); if(outputMaterial) Destroy(outputMaterial);
        if(Instance==this) Instance=null;
    }
    public static Vector3 FaceDirection(int face)
    { return new[]{Vector3.zero,Vector3.up,Vector3.forward,Vector3.right,Vector3.left,Vector3.back,Vector3.down}[Mathf.Clamp(face,1,6)]; }
}
