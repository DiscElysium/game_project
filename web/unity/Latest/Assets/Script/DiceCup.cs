using System.Collections;
using UnityEngine;
public class DiceCup : MonoBehaviour
{
    public float targetAlpha = .2f, duration = 3, fadeSpeed = 5;
    Renderer[] renderers; bool[] previous;
    void Awake() { renderers = GetComponentsInChildren<Renderer>(); }
    public void ActivateXRay() { RestoreCover(); StartCoroutine(Observe()); }
    IEnumerator Observe() { Reveal(); yield return new WaitForSeconds(duration); RestoreCover(); }
    public void Reveal()
    {
        if (previous != null) return;
        previous = new bool[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) { previous[i] = renderers[i].forceRenderingOff; renderers[i].forceRenderingOff = true; }
    }
    public void RestoreCover()
    {
        StopAllCoroutines(); if (previous == null) return;
        for (int i = 0; i < renderers.Length; i++) if (renderers[i]) renderers[i].forceRenderingOff = previous[i];
        previous = null;
    }
    void OnDisable() => RestoreCover();
}
