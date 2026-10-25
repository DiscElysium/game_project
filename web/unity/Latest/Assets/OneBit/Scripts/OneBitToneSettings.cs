using UnityEngine;

public sealed class OneBitToneSettings : MonoBehaviour
{
    public static OneBitToneSettings Instance { get; private set; }
    [System.Serializable] public class Values
    {
        public float black=.14f, white=.86f, exposure=1, gamma=1;
        public int ditherPitch=1, patternSize=8, detail=0;
    }
    public Values values=new Values();
    Texture2D threshold;
    const string Preference="MaskedDice.Picture.v2";
    void Awake()
    {
        Instance=this;
        if(PlayerPrefs.HasKey(Preference))
            try { values=JsonUtility.FromJson<Values>(PlayerPrefs.GetString(Preference)) ?? new Values(); } catch { values=new Values(); }
        Apply();
    }
    public void Apply()
    {
        values.black=Mathf.Clamp(values.black,0,.45f); values.white=Mathf.Clamp(values.white,.5f,1);
        values.exposure=Mathf.Clamp(values.exposure,.5f,2); values.gamma=Mathf.Clamp(values.gamma,.5f,1.8f);
        values.ditherPitch=Mathf.Clamp(values.ditherPitch,1,3); values.patternSize=values.patternSize==4 ? 4 : 8; values.detail=Mathf.Clamp(values.detail,0,2);
        if(!threshold || threshold.width!=values.patternSize) RebuildThreshold();
        Shader.SetGlobalVector("_OBLevels",new Vector4(values.black,values.white,values.exposure,values.gamma));
        Shader.SetGlobalFloat("_OBDitherPitch",values.ditherPitch);
        Shader.SetGlobalFloat("_OBThresholdSize",values.patternSize);
        Shader.SetGlobalTexture("_OBThresholdMap",threshold);
    }
    void RebuildThreshold()
    {
        if(threshold) Destroy(threshold);
        int size=values.patternSize;
        threshold=new Texture2D(size,size,TextureFormat.RGBA32,false,true) { name="Pixel-aligned Bayer threshold", filterMode=FilterMode.Point, wrapMode=TextureWrapMode.Repeat };
        var pixels=new Color[size*size];
        int[] seed={0,2,3,1};
        for(int y=0;y<size;y++) for(int x=0;x<size;x++)
        {
            int rank=0;
            for(int bit=0;(1<<bit)<size;bit++) rank=rank*4+seed[((y>>bit)&1)*2+((x>>bit)&1)];
            float value=(rank+.5f)/(size*size); pixels[y*size+x]=new Color(value,value,value,1);
        }
        threshold.SetPixels(pixels); threshold.Apply(false,true);
    }
    public void Save() { PlayerPrefs.SetString(Preference,JsonUtility.ToJson(values)); PlayerPrefs.Save(); }
    public void ResetDefaults() { values=new Values(); Apply(); Save(); }
    void OnDestroy() { if(threshold) Destroy(threshold); if(Instance==this) { Instance=null; Shader.SetGlobalFloat("_OBThresholdSize",0); } }
}
