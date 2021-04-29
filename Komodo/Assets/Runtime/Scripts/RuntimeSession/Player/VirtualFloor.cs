using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualFloor : MonoBehaviour
{
    public void SetXZPosition(Vector3 position)
    {
        Vector3 finalPosition = transform.position;

        finalPosition.x = position.x;

        finalPosition.z = position.z;

        transform.position = finalPosition;
    }

    public void SetXZPosition(Transform otherTransform)
    {
        
        Vector3 finalPosition = otherTransform.position;

        finalPosition.x = otherTransform.position.x;

        finalPosition.z = otherTransform.position.z;

        transform.position = finalPosition;
    }

    public void SetYPosition(Vector3 position)
    {
        Vector3 finalPosition = transform.position;

        finalPosition.y = position.y;

        transform.position = finalPosition;
    }
}
