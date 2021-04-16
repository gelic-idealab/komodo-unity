using Komodo.Utilities;
using UnityEngine.Events;

public class DoubleTapState : SingletonComponent<DoubleTapState>
{
    public static DoubleTapState Instance
    {
        get { return ((DoubleTapState)_Instance); }
        set { _Instance = value; }
    }

    [ShowOnly]public bool leftHandTriggerPressed;
    [ShowOnly] public bool rightHandTriggerPressed;

    [ShowOnly] public bool leftHandGripPressed;
    [ShowOnly] public bool rightHandGripPressed;

    public UnityEvent OnDoubleGripStateOn;
    public UnityEvent OnDoubleGripStateOff;

    public UnityEvent OnDoubleTriggerStateOn;
    public UnityEvent OnDoubleTriggerStateOff;
}

