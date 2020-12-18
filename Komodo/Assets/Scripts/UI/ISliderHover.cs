using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ISliderHover : IEventSystemHandler
{
    void OnSliderHover(SliderEventData cursorData);
}

public class SliderEventData : BaseEventData
{
    // This doesn't have to be in this scope, but I didn't want to create
    // yet another script file for it, so this seemed the most logical place
    public static readonly ExecuteEvents.EventFunction<ISliderHover> cursorFollowDelegate
    = delegate (ISliderHover handler, BaseEventData data)
    {
        var casted = ExecuteEvents.ValidateEventData<SliderEventData>(data);
        handler.OnSliderHover(casted);
    };

    public Vector3 currentHitLocation;
    public bool isInputActive;
    // public float damageAmount;

    public SliderEventData(EventSystem eventSystem,
                           Vector3 currentHitLocation,
                           bool isInputActive
                           ) : base(eventSystem)
    {
        this.currentHitLocation = currentHitLocation;
        this.isInputActive = isInputActive;
    }
}