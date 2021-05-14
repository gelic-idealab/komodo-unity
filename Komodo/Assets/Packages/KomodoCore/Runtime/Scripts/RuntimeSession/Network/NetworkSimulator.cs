using Komodo.Utilities;
using UnityEngine;

namespace Komodo.Runtime
{
    public class NetworkSimulator : MonoBehaviour {

        [ShowOnly]
        public string hint = "Use the context menu on this component.";
        private int _numIncrementalClients = 0;
        private int _incrementalClientsStartIndex = 1;

        private int _GetTopIncrementalClientId() {
            return _numIncrementalClients + _incrementalClientsStartIndex - 1;
        }

        /** Test client spawning in the editor. **/
        [ContextMenu("Add Incremental Client")]
        public void AddIncrementalClient() {
            ClientSpawnManager.Instance.AddNewClient(_GetTopIncrementalClientId() + 1);
            _numIncrementalClients += 1;
        }

        /** Test client spawning in the editor. **/
        [ContextMenu("Remove Incremental Client")]
        public void RemoveIncrementalClient() {
            ClientSpawnManager.Instance.RemoveClient(_GetTopIncrementalClientId());
            _numIncrementalClients -= 1;
        }

        
        private Position GeneratePosition(Entity_Type entityType, Vector3 position, Quaternion rotation) 
        {
            int clientID = _GetTopIncrementalClientId();

            return new Position
            {
                clientId = clientID,
                entityId = MainClientUpdater.Instance.ComputeEntityID(clientID, entityType),
                entityType = MainClientUpdater.Instance.ComputeEntityType(entityType),
                scaleFactor = MainClientUpdater.Instance.ComputeScaleFactor(entityType),
                rot = rotation,
                pos = position,
            };
        }

        /** Test receiving updates in the editor **/
        [ContextMenu("Receive Position Update For Head")]
        public void ReceiveCenterPositionUpdateForHead() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_head, Vector3.zero, Quaternion.AngleAxis(180f, Vector3.forward));

            ClientSpawnManager.Instance.Client_Refresh(position);
        }

        [ContextMenu("Receive Position Update For Left Hand")]
        public void ReceiveCenterPositionUpdateForLeftHand() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_Lhand, Vector3.one, Quaternion.AngleAxis(180f, Vector3.forward));

            ClientSpawnManager.Instance.Client_Refresh(position);
        }

        [ContextMenu("Receive Position Update For Right Hand")]
        public void ReceiveCenterPositionUpdateForRightHand() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_Rhand, Vector3.one + Vector3.one, Quaternion.AngleAxis(180f, Vector3.forward));

            ClientSpawnManager.Instance.Client_Refresh(position);
        }

        /** Test sending updates in the editor **/


    }
}