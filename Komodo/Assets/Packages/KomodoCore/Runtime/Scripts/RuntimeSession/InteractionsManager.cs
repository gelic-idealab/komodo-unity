using System;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Runtime
{
    /// <summary>
    /// This seems like a testing manager; to be honest, I don't know what this class does.
    /// </summary>
    public class InteractionsManager : MonoBehaviour
    {
        [ContextMenu("Test Receive Show Model Interaction For All Models")]
        public void TestReceiveShowInteractionForAllModels ()
        {
            foreach(KeyValuePair<int, NetworkedGameObject> pair in NetworkedObjectsManager.Instance.networkedObjectFromEntityId)
            {
                var showInteractionFromTestClient = new Interaction(9999, pair.Key, (int)INTERACTIONS.SHOW);

                SessionStateManager.Instance.ApplyInteraction(showInteractionFromTestClient);
            }
        }

        [ContextMenu("Test Receive Hide Model Interaction For All Models")]
        public void TestReceiveHideInteractionForAllModels ()
        {
            foreach(KeyValuePair<int, NetworkedGameObject> pair in NetworkedObjectsManager.Instance.networkedObjectFromEntityId)
            {
                var hideInteractionFromTestClient = new Interaction(9999, pair.Key, (int)INTERACTIONS.SHOW);

                SessionStateManager.Instance.ApplyInteraction(hideInteractionFromTestClient);
            }
        }
    }
}