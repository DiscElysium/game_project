using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(10000)]
public sealed class CharacterCameraGuard : MonoBehaviour
{
    readonly List<Renderer> characters=new List<Renderer>();
    Camera view;
    public int HiddenCount { get; private set; }
    void Start()
    {
        view=GetComponent<Camera>();
        foreach(string name in new[]{"WatcherMask","WeaverMask","RiskerMask"})
        { var root=GameObject.Find(name); if(root) characters.AddRange(root.GetComponentsInChildren<Renderer>()); }
        foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
        {
            if(!renderer.enabled || renderer.transform.root.name=="Tribunal world" || renderer.transform.root.name=="Tabletop redesign") continue;
            var b=renderer.bounds; float radius=new Vector2(b.center.x,b.center.z).magnitude;
            bool anatomy=renderer.name.StartsWith("char:") || renderer.name=="pCube1";
            bool headDetail=radius>62 && radius<110 && b.center.y>22 && b.center.y<60 && b.size.magnitude<40;
            if(anatomy || headDetail) characters.Add(renderer);
        }
        RenderPipelineManager.beginCameraRendering+=BeforeRender;
    }
    void LateUpdate() => Refresh();
    void BeforeRender(ScriptableRenderContext context,Camera camera) { if(camera==view) Refresh(); }
    public void Refresh()
    {
        if(!view) return; HiddenCount=0; var position=view.transform.position;
        foreach(var renderer in characters) if(renderer)
        {
            var bounds=renderer.bounds; var expanded=bounds; expanded.Expand(5);
            bool near=(bounds.ClosestPoint(position)-position).sqrMagnitude<22*22;
            // Hide the whole nearby character surface, including peripheral
            // parts of a mask which miss the center ray but still cover the view.
            bool hide=expanded.Contains(position) || near;
            renderer.forceRenderingOff=hide; if(hide) HiddenCount++;
        }
    }
    void OnDisable()
    { RenderPipelineManager.beginCameraRendering-=BeforeRender; foreach(var renderer in characters) if(renderer) renderer.forceRenderingOff=false; }
}
