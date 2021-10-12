using System;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    public class InteractionsManager : MonoBehaviour
    {
        [ContextMenu("Test Receive Show Model Interaction For All Models")]
        public void TestReceiveShowInteractionForAllModels ()
        {
            foreach(KeyValuePair<int, NetworkedGameObject> pair in NetworkedObjectsManager.Instance.networkedObjectFromEntityId)
            {
                SessionStateManager.Instance.ApplyInteraction(new Interaction(9999, pair.Key, (int)INTERACTIONS.RENDERING));
            }
        }

        [ContextMenu("Test Receive Hide Model Interaction For All Models")]
        public void TestReceiveHideInteractionForAllModels ()
        {
            foreach(KeyValuePair<int, NetworkedGameObject> pair in NetworkedObjectsManager.Instance.networkedObjectFromEntityId)
            {
                SessionStateManager.Instance.ApplyInteraction(new Interaction(9999, pair.Key, (int)INTERACTIONS.NOT_RENDERING));
            }
        }
    }
}