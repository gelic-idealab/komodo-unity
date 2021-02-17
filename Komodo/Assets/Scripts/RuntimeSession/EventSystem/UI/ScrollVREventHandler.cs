using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class ScrollVREventHandler : MonoBehaviour, ISliderHover
{
    private Slider slider;
    private RectTransform rectTrans;
    private Rect rect;

    public void Start()
    {
        slider = GetComponent<Slider>();
        rectTrans = GetComponent<RectTransform>();
    }

    public void OnSliderHover(SliderEventData cursorData)
    {
        //dont adjust slider when input source is not on
        if (!cursorData.isInputActive)
            return;
        
        //get relative location of the hit from our rectTransform
        var locPos = rectTrans.InverseTransformPoint(cursorData.currentHitLocation);

        //move our reference point above the half point removing negative numbers then divide that with the total width to get ther normalized position
        var posShift = (locPos.x + (rectTrans.rect.width/2)) / rectTrans.rect.width ;

        slider.normalizedValue = posShift;
    }

}
