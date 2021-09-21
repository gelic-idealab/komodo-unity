using UnityEngine;
using Komodo.Runtime;

namespace Komodo.Runtime
{
    public class MoveWithHover : MonoBehaviour, ICursorHover
    {
        public Transform target;

        public void OnHover (CursorHoverEventData cursorData)
        {
            target.transform.position = cursorData.currentHitLocation;
        }
    }
}
