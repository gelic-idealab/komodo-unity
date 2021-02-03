using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedGroup : MonoBehaviour
{
    public int linkID;

    public Dictionary<int, (Transform parent, List<Collider> collectedColliders)> uniqueIdToParentofLinks;
    private Transform rootParent;
    private Transform parentOfCollection;

    public void RefreshLinkedGroup()
    {
        List<Transform> childList = new List<Transform>();
        rootParent = transform;
        parentOfCollection = transform.GetChild(0);


        Bounds newBound = default;
        if(uniqueIdToParentofLinks.Count != 0 && uniqueIdToParentofLinks[0].collectedColliders.Count != 0)
        newBound = new Bounds(uniqueIdToParentofLinks[0].collectedColliders[0].transform.position, Vector3.one * 0.02f);// new Bounds();

        for (int i = 0; i < rootParent.GetChild(0).childCount; i++)
        {
            childList.Add(parentOfCollection.GetChild(i));

            var col = parentOfCollection.GetChild(i).GetComponent<Collider>();//capturedObjects[i];//uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders[i];

            //turn it on to get bounds info 
            //  col.enabled = true;

            //set new collider bounds
            newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));

            // col.enabled = false;
            Debug.Log("delete" + col.gameObject.name + " " + col.bounds.size, col.gameObject);
            //   Debug.Log(i + " " + col.bounds.size);
        }

        rootParent.transform.DetachChildren();
        parentOfCollection.transform.DetachChildren();
        //     currentLinkBoundingBox.transform.rotation *= Quaternion.Inverse(currentLinkBoundingBox.transform.rotation);

        rootParent.position = newBound.center;//newLinkParentCollider.transform.position;
        rootParent.SetGlobalScale(newBound.size);

        var prevRot = transform.rotation;
        rootParent.rotation = Quaternion.identity;

        parentOfCollection.rotation = Quaternion.identity;
        //    currentLinkBoundingBox.transform.rotation *= collider.transform.rotation;// Quaternion.Inverse(collider.transform.rotation);

        if (rootParent.TryGetComponent(out Collider boxCollider))
            Destroy(boxCollider);

        rootParent.gameObject.AddComponent<BoxCollider>();

        foreach (var item in childList)
            item.transform.SetParent(parentOfCollection.transform, true);


        parentOfCollection.transform.SetParent(rootParent.transform, true);

    }


}
