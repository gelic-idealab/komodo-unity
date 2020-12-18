using UnityEngine;

public class MoveColorTargetWithSelection : MonoBehaviour, ICursorHover
{
    public Transform target;
    public Vector3 lastLocalPos;

    public void OnHover(CursorHoverEventData cursorData)
    {
        /// only move when we turn on our lazer that is emiting the event query
        if (cursorData.inputSourceActiveState)
        {
            target.transform.position = cursorData.currentHitLocation; 
            lastLocalPos = target.transform.localPosition;
        }
        else
        {
            target.transform.localPosition = lastLocalPos;
        }


    }
}
