using System.Collections.Generic;
using UnityEngine;

public class SliderHandler : MonoBehaviour
{
    [System.Serializable]
    public class SlideElement
    {
        public Transform slideTransform;
        public Vector3 targetLocalOffset;
        public float returnSpeed = 12f;

        [HideInInspector] public Vector3 originalLocalPos;
        [HideInInspector] public Vector3 currentPosOffset;
    }

    [SerializeField] List<SlideElement> sliders;
    private bool isLocked;

    private void Awake()
    {
        foreach (var slide in sliders)
        {
            if (slide.slideTransform == null) continue;
            slide.originalLocalPos = slide.slideTransform.localPosition;
            slide.currentPosOffset = Vector3.zero;
        }
    }
    public void LateUpdate()
    {
        foreach (var slide in sliders)
        {
            if (slide.slideTransform == null) continue;
            if (isLocked) continue;

            slide.currentPosOffset = Vector3.Lerp(slide.currentPosOffset, Vector3.zero, Time.deltaTime * slide.returnSpeed);
            slide.slideTransform.localPosition = slide.originalLocalPos + slide.currentPosOffset;
        }

    }
    public void AnimateSlide()
    {
        if (isLocked) return;

        foreach (var slide in sliders)
        {
            if (slide.slideTransform == null) continue;
            slide.currentPosOffset = slide.targetLocalOffset;
        }
    }
    public bool SetLocked(bool state) => isLocked = state;
}
