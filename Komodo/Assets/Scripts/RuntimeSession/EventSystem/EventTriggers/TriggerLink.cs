using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;

namespace Komodo.Runtime
{

    public class TriggerLink : MonoBehaviour
    {
        // List<Collider> capturedObjects = new List<Collider>();

        //public static Dictionary<int, List<Collider>> SharedlinkListParents = new Dictionary<int, List<Collider>>();
        public static int uniqueID;

        public int currentIDworkingWith;

        public (int uniqueId, Collider lastCollider) lastCollectedCollider;

        //public Dictionary<int, (Transform parent, List<Collider> collectedColliders)> uniqueIdToParentofLinks = new Dictionary<int, (Transform parent, List<Collider> collectedColliders)>();

        LinkedGroup linkedGroup;

        public LineRenderer linkIndicator;
        public GameObject linkBoundingPrefab;
        public GameObject currentLinkBoundingBox;
        public Transform linkCollectionParent;
        BoxCollider currentRootCollider;

        public KomodoControllerInteraction LcontrollerInteraction;
        public KomodoControllerInteraction RcontrollerInteraction;
        public void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Interactable"))
            {

                if (currentLinkBoundingBox)
                    if (currentLinkBoundingBox.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                        return;

                currentIDworkingWith = 0;//uniqueID;

                //two parents to avoid weird scaling visual isue with just parent children scalling, so we do parent scaling, on another parent that has children
                if (linkCollectionParent == null)
                    linkCollectionParent = new GameObject("Linker Parent").transform;

                if (currentLinkBoundingBox == null)
                {
                    currentLinkBoundingBox = GameObject.Instantiate(linkBoundingPrefab);
                    linkedGroup = currentLinkBoundingBox.AddComponent<LinkedGroup>();

                    linkedGroup.uniqueIdToParentofLinks = new Dictionary<int, (Transform parent, List<Collider> collectedColliders)>();


                    currentLinkBoundingBox.tag = "Interactable";
                }

                if (!linkedGroup.uniqueIdToParentofLinks.ContainsKey(currentIDworkingWith))
                    linkedGroup.uniqueIdToParentofLinks.Add(currentIDworkingWith, (linkCollectionParent.transform, new List<Collider>() { collider }));
                else
                {
                    linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders.Add(collider);

                    linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith] = (linkCollectionParent.transform, linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders);
                }
            }



            currentLinkBoundingBox.transform.DetachChildren();
            linkCollectionParent.transform.DetachChildren();

            //if it is not our parent collider we shut of its own collider
            //if( linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders.Count !=0)
            var newBound = new Bounds(linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders[0].transform.position, Vector3.one * 0.02f);
            for (int i = 0; i < linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders.Count; i++)
            {
                var col = linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders[i];

                //turn it on to get bounds info 
                col.enabled = true;

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));

                col.enabled = false;
                //   Debug.Log(col.gameObject.name + " " + newBound.size, col.gameObject);
            }


            currentLinkBoundingBox.transform.position = newBound.center;//newLinkParentCollider.transform.position;
            currentLinkBoundingBox.transform.SetGlobalScale(newBound.size);

            currentLinkBoundingBox.transform.rotation = Quaternion.identity;

            linkCollectionParent.transform.rotation = Quaternion.identity;

            if (currentLinkBoundingBox.TryGetComponent(out BoxCollider boxCollider))
                Destroy(boxCollider);

            currentRootCollider = currentLinkBoundingBox.AddComponent<BoxCollider>();

            foreach (var item in linkedGroup.uniqueIdToParentofLinks[currentIDworkingWith].collectedColliders)
                item.transform.SetParent(linkCollectionParent.transform, true);


            linkCollectionParent.transform.SetParent(currentLinkBoundingBox.transform, true);


            //only set our grab object to the group object when it is a different object than its own to avoid the object reparanting itself
            if (LcontrollerInteraction.currentTransform != null)
            {
                var lcontrollerOBJID = LcontrollerInteraction.currentTransform.GetInstanceID();
                if (lcontrollerOBJID == collider.transform.GetInstanceID()) //&& lcontrollerOBJID != currentLinkBoundingBox.transform.GetInstanceID())
                {
                    LcontrollerInteraction.Drop();
                    //LcontrollerInteraction.PickUp(currentLinkBoundingBox.transform);
                    //RcontrollerInteraction.Drop();
                    //collider.transform.SetParent(linkCollectionParent, true);
                }

                if (lcontrollerOBJID == currentLinkBoundingBox.transform.GetInstanceID())
                {



                }

            }
            Debug.Log("THIS : " + collider.transform.name);
            // if(!pickedUp)
            //IF WE ENVELOP THE OBJECT THAT WE ARE GRABBING DROP THAT OBJECT
            if (RcontrollerInteraction.currentTransform != null)
                if (RcontrollerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID()) //&& RcontrollerInteraction.currentTransform.GetInstanceID() != currentLinkBoundingBox.transform.GetInstanceID())
                {
                    RcontrollerInteraction.Drop();
                    //RcontrollerInteraction.PickUp(currentLinkBoundingBox.transform);
                    //RcontrollerInteraction.Drop();
                    //collider.transform.SetParent(linkCollectionParent, true);

                }

        }
        //show our link bounding box when having our link button on
        public void SetEnable()
        {
            //disable it to be able to get colliders inside 
            //if (linkCollectionParent != null)
            //    linkCollectionParent.enabled = false;

            if (currentLinkBoundingBox)
            {
                currentLinkBoundingBox.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        public void SetOnDisable()
        {
            //if (linkCollectionParent != null)
            //    linkCollectionParent.enabled = true;

            if (currentLinkBoundingBox)
            {
                currentLinkBoundingBox.GetComponent<MeshRenderer>().enabled = false;
                // currentLinkBoundingBox.SetActive(false);
            }
        }

    }
}
