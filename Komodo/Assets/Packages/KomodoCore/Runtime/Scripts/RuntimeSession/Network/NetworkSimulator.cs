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

            Position result = new Position
            {
                clientId = clientID,
                entityId = MainClientUpdater.Instance.ComputeEntityID(clientID, entityType),
                entityType = MainClientUpdater.Instance.ComputeEntityType(entityType),
                scaleFactor = MainClientUpdater.Instance.ComputeScaleFactor(entityType),
                rot = rotation,
                pos = position,
            };

            //Debug.Log($"new Position({result.clientId}, {result.entityId}, {result.entityType}, {result.scaleFactor}, {result.rot}, {result.pos})");

            return result;
        }

        /** Test receiving updates in the editor. 
         *  This function acts as if the data is already unpacked from 
         *  raw data. 
         **/
        [ContextMenu("Receive Position Update For Head")]
        public void ReceiveCenterPositionUpdateForHead() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_head, Vector3.zero, Quaternion.AngleAxis(180f, Vector3.forward));

            ClientSpawnManager.Instance.Client_Refresh(position);
        }
        
        //TODO(Brandon): Suggestion: rename this to PositionUpdate
        /**
         * Based on NetworkUpdateHandler > NetworkUpdate, but instead of 
         * sending the update out, it causes this code client to "receive" 
         * a relay update (which will apply to the top incremental user client).
         */
        private void NetworkUpdate(Position pos) 
        {
            float[] arr_pos = NetworkUpdateHandler.Instance.SerializeCoordsStruct(pos);
#if UNITY_WEBGL && !UNITY_EDITOR
    //do nothing, so the compiler doesn't complain
#else
            NetworkUpdateHandler.Instance.SocketSim.RelayPositionUpdate(arr_pos);
#endif

        }
        
        /** Test receiving updates in the editor. 
         *  This function acts as if the data is already unpacked from 
         *  raw data. 
         **/
        [ContextMenu("Packed Position Update For Head")]
        public void PackedCenterPositionUpdateForHead() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_head, Vector3.zero, Quaternion.AngleAxis(180f, Vector3.forward));

            NetworkUpdate(position);
        }

        /** Test receiving updates in the editor. 
         *  This function acts as if the data is already unpacked from 
         *  raw data. 
         **/
        [ContextMenu("Receive Position Update For Left Hand")]
        public void ReceiveCenterPositionUpdateForLeftHand() {

            if (_GetTopIncrementalClientId() == 0) {
                AddIncrementalClient();
            }

            Position position = GeneratePosition(Entity_Type.users_Lhand, Vector3.one, Quaternion.AngleAxis(180f, Vector3.forward));

            ClientSpawnManager.Instance.Client_Refresh(position);
        }

        /** Test receiving updates in the editor. 
         *  This function acts as if the data is already unpacked from 
         *  raw data. 
         **/
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