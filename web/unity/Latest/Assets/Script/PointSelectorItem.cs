using System.Collections;
using UnityEngine;

public class PointSelectorItem : MonoBehaviour
{
    public enum ItemType { BidPoint, XRayTarget, StealTarget, BidAction, ChallengeAction, IncreaseQuantity, DecreaseQuantity }
    public ItemType itemType;
    public int value;
    public float hoverOffset=.8f, smoothSpeed=12;
    Vector3 home, target; Quaternion homeRotation;
    bool initialized, animating;
    void Awake() { RememberPose(); }
    void RememberPose() { if(initialized) return; initialized=true; home=target=transform.localPosition; homeRotation=transform.localRotation; }
    void Update() { if(!animating) transform.localPosition=Vector3.Lerp(transform.localPosition,target,Time.deltaTime*smoothSpeed); }
    // Pointer events are routed by OneBitPresentation, after integer pixel mapping.
    public void SetHovered(bool active) { RememberPose(); if(!animating) target=home+(active ? transform.parent ? transform.parent.InverseTransformVector(Vector3.up*hoverOffset) : Vector3.up*hoverOffset : Vector3.zero); }
    public void ResetPose() { RememberPose(); animating=false; transform.localPosition=target=home; transform.localRotation=homeRotation; }
    public void ResetItem() { ResetPose(); gameObject.SetActive(true); }
    public IEnumerator FlyToCameraSequence(Transform cameraTransform,System.Action onComplete)
    {
        animating=true; var start=transform.position; float elapsed=0;
        while(elapsed<.8f) { elapsed+=Time.deltaTime; transform.position=Vector3.Lerp(start,cameraTransform.position,1-Mathf.Pow(1-elapsed/.8f,3)); yield return null; }
        gameObject.SetActive(false); onComplete?.Invoke();
    }
}
