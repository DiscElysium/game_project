using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

[DefaultExecutionOrder(-40)]
public sealed class SeatedLookController : MonoBehaviour
{
    public PlayerUIController ui;
    public CinemachineVirtualCamera freeView;
    public float sensitivity=2.2f;
    public bool Observing { get; private set; }
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }=-10;
    bool dragging;
    void Start() { if(!ui) ui=FindFirstObjectByType<PlayerUIController>(); ApplyPose(); }
    void Update()
    {
        if(!ui || !freeView) return;
        if(ui.IsViewing || ui.LookMenusOpen) { ReleaseMouse(); return; }
        if(Input.GetKeyDown(KeyCode.Tab)) SetObservation(!Observing);
        bool overUI=EventSystem.current && EventSystem.current.IsPointerOverGameObject();
        if(Input.GetMouseButtonDown(1) && !overUI) { SetObservation(true); dragging=true; Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; }
        if(Input.GetMouseButtonUp(1)) ReleaseMouse();
        if(!Observing) return;
        if(dragging) Rotate(new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"))*sensitivity);
        float x=(Input.GetKey(KeyCode.RightArrow) ? 1 : 0)-(Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
        float y=(Input.GetKey(KeyCode.UpArrow) ? 1 : 0)-(Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
        if(x!=0 || y!=0) Rotate(new Vector2(x,y)*75*Time.unscaledDeltaTime);
        if(Input.GetKeyDown(KeyCode.R)) SetLookAngles(0,-10);
    }
    public void Toggle() => SetObservation(!Observing);
    public void SetObservation(bool value,bool returnToTable=true)
    {
        if(value && (!ui || ui.IsViewing || ui.LookMenusOpen)) return;
        Observing=value;
        if(value) { ApplyPose(); ui.SwitchToCameraWithSpeed(freeView,false); }
        else { ReleaseMouse(); if(freeView) freeView.Priority=1; if(returnToTable && ui) ui.SwitchToCamera(LiarGameManager.Instance && LiarGameManager.Instance.IsPlayerTurn); }
    }
    public void Rotate(Vector2 delta) => SetLookAngles(Yaw+delta.x,Pitch-delta.y);
    public void SetLookAngles(float yaw,float pitch)
    { Yaw=Mathf.Repeat(yaw,360); Pitch=Mathf.Clamp(pitch,-76,65); ApplyPose(); }
    void ApplyPose()
    { if(freeView) freeView.transform.SetPositionAndRotation(new Vector3(0,46.08f,-77),Quaternion.Euler(Pitch,Yaw,0)); }
    void ReleaseMouse() { dragging=false; Cursor.lockState=CursorLockMode.None; Cursor.visible=true; }
    void OnApplicationFocus(bool focus) { if(!focus) ReleaseMouse(); }
    void OnDisable() { ReleaseMouse(); Observing=false; if(freeView) freeView.Priority=1; }
}
