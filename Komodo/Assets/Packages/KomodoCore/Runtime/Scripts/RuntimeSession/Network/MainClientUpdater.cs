// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using Unity.Entities;
using Komodo.Utilities;
//using Komodo.AssetImport;

namespace Komodo.Runtime
{
    //Customized unityevent to allow it to accept particular funcions to send our network information to
    [System.Serializable] public class Coord_UnityEvent : UnityEvent<Position> { }

    /// <summary>
    /// This class is intended to be attached to one GameObject in a scene and allows for obtaining data from GameObjects 
    /// with an attached Net_Register_GameObject and sending that information in a serialized way into 
    /// whichever funcion is attached to the specified UnityEvent. It involves manually adding our avatars 
    /// head and hand GameObjects that will be used to send updates  accross the network for others. 
    /// It also provides funcions to add or remove scene objects with a Net_Register_GameObject attached 
    /// to the update loop to send its own GameObject data aswell.
    /// </summary>
    public class MainClientUpdater : SingletonComponent<MainClientUpdater>, IUpdatable
    {
        public static MainClientUpdater Instance
        {
            get { return ((MainClientUpdater)_Instance); }
            set { _Instance = value; }
        }

        public AvatarEntityGroup mainClientAvatarEntityGroup;

        private Transform leftHandEntityTransform;

        private Transform rightHandEntityTransform;

        private Transform headEntityTransform;

        //list to maintain objects in scene to send update accross
        [HideInInspector] public List<NetworkedGameObject> networkedEntities = new List<NetworkedGameObject>();

        private int clientID => NetworkUpdateHandler.Instance.client_id;

        //Hand References for position and animation information
        private Vector3 leftHandOriginalLocalPosition;

        private Vector3 rightHandOriginalLocalPosition;

        private Animator leftHandAnimator;

        private Animator rightHandAnimator;

        private EntityManager entityManager;

        private NetworkUpdateHandler netUpdateHandler;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }

        IEnumerator Start()
        {
            netUpdateHandler = NetworkUpdateHandler.Instance;

            if (netUpdateHandler == null)
            {
                throw new System.Exception("NetworkUpdateHandler instance not found.");
            }

            //WebGLMemoryStats.LogMoreStats("MainClientUpdater Start BEFORE");
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //if we are missing our avatarentitygroup in editor try to get it from our game object
            if (!mainClientAvatarEntityGroup) mainClientAvatarEntityGroup = GetComponent<AvatarEntityGroup>();

            if (!mainClientAvatarEntityGroup)
            {
                Debug.LogError("No mainclientAvatarEntityGroup found in  MainClientUpdater.cs to use to send updates to network", gameObject);
            }

            //cashe our avatar transform references
            (headEntityTransform, leftHandEntityTransform, rightHandEntityTransform) = (mainClientAvatarEntityGroup.avatarComponent_Head.transform, mainClientAvatarEntityGroup.avatarComponent_hand_L.transform, mainClientAvatarEntityGroup.avatarComponent_hand_R.transform);

            //get references to AnimControllers to send anim state info 
            (leftHandAnimator, rightHandAnimator) = (leftHandEntityTransform.GetComponent<Animator>(), rightHandEntityTransform.GetComponent<Animator>());

            if (!rightHandAnimator || !leftHandAnimator)
            {
                Debug.LogError("We are missing our Animator Controller from our hands in MainClientUpdater");
            }

            //WebGLMemoryStats.LogMoreStats("MainClientUpdater Start IN_MIDDLE");

            //Wait for the avatar to finish loading to allow us to continue and start sending updates
            yield return new WaitUntil(() =>
            {
                return GameStateManager.Instance.isAvatarLoadingFinished;
            });

            //Register our OnUpdate funcion to start sending updates 
            GameStateManager.Instance.RegisterUpdatableObject(this);

            //Grab current position of hands to detect if they have moved to avoid rendering them when they havent;
            leftHandOriginalLocalPosition = mainClientAvatarEntityGroup.avatarComponent_hand_L.transform.localPosition;

            rightHandOriginalLocalPosition = mainClientAvatarEntityGroup.avatarComponent_hand_R.transform.localPosition;
        }

        public void OnUpdate(float realTime)
        {
            //SENDS AVATAR HEAD REFERENCE TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
            SendPositionUpdates(Entity_Type.users_head, headEntityTransform.position, headEntityTransform.rotation);

            //CHECKS IF OUR L_HAND HAS MOVED TO SEND LHAND REFERENCE DATA TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
            if (leftHandOriginalLocalPosition != leftHandEntityTransform.localPosition)
            {
                SendPositionUpdates(Entity_Type.users_Lhand, leftHandEntityTransform.position, leftHandEntityTransform.rotation);

                leftHandOriginalLocalPosition = leftHandEntityTransform.localPosition;
            }

            //CHECKS IF OUR R_HAND HAS MOVED TO SENDS LHAND REFERENCE DATA TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
            if (rightHandOriginalLocalPosition != rightHandEntityTransform.localPosition)
            {
                SendPositionUpdates(Entity_Type.users_Rhand, rightHandEntityTransform.position, rightHandEntityTransform.rotation);

                rightHandOriginalLocalPosition = rightHandEntityTransform.localPosition;
            }

            //ADJUST DATA AND SEND UPDATES FROM THOSE GAMEOBJECTS REGISTERED TO OUR LIST 
            foreach (var entityContainers in networkedEntities)
            {
                SendNetObjectUpdates(entityContainers);
            }

            NetworkedPhysicsManager.Instance.OnUpdate();
        }

        //When the GameObject with this script attached is destroyed we take it out of the update loop
        public void OnDestroy()
        {
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }

        #region Add/Remove From Update Loop Funcions
        /// <summary>
        /// Place GameObject in update loop to send network information across
        /// </summary>
        /// <param name="netObject attached to an objects to keep network relevant data"></param>
        public void AddUpdatable(NetworkedGameObject netObject)
        {
            if (!networkedEntities.Contains(netObject))
            {
                networkedEntities.Add(netObject);
            }
        }

        /// <summary>
        /// Remove GameObject from update loop to stop sending network information
        /// </summary>
        /// <param name="netObject attached to an objects to keep network relevant data"></param>
        public void RemoveUpdatable(NetworkedGameObject netObject)
        {
            if (networkedEntities.Contains(netObject))
            {
                networkedEntities.Remove(netObject);
            }
        }
        #endregion

        public int ComputeClientID(MainClientUpdater who) {
            return this.clientID;
        }

        public int ComputeEntityID(int clientID, Entity_Type entityType) {
            return (clientID * 10) + (int) entityType;
        }

        public int ComputeEntityType(Entity_Type entityType) {
            return (int) entityType;
        }

        public float ComputeScaleFactor(Entity_Type entityType) {
            float scaleFactor = 0.123456789f;

            //setup animation parameter to update
            switch (entityType)
            {
                case Entity_Type.users_head:
                    scaleFactor = headEntityTransform.parent.lossyScale.x;
                    break;

                case Entity_Type.users_Lhand:
                    scaleFactor = leftHandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    break;

                case Entity_Type.users_Rhand:
                    scaleFactor = rightHandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    break;

                default: 
                    throw new System.Exception("Invalid entity type encountered.");
            }

            return scaleFactor;
        }

        public Position GeneratePosition(MainClientUpdater who, Entity_Type entityType, Vector3 position, Quaternion rotation) {
            return new Position
            {
                clientId = ComputeClientID(who),
                entityId = ComputeEntityID(clientID, entityType),
                entityType = ComputeEntityType(entityType),
                scaleFactor = ComputeScaleFactor(entityType),
                rot = rotation,
                pos = position,
            };
        }

        #region Serializing Methods and UnityEvent Invoke Calls
        /// <summary>
        /// Meant to convert our Avatar data to follow our POSITION struct to be sent each update
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void SendPositionUpdates(Entity_Type entityType, Vector3 position, Quaternion rotation)
        {
            Position coords = GeneratePosition(this, entityType, position, rotation);

            //send data over to those funcions attached to our UnityEvent
            netUpdateHandler.NetworkUpdate(coords);
        }

        /// <summary>
        /// Meant to convert our GameObject data to follow our POSITION struct to be sent each update
        /// </summary>
        /// <param name="eContainer"></param>
        public void SendNetObjectUpdates(NetworkedGameObject eContainer)
        {
            var entityData = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(eContainer.Entity);
            Position coords = new Position
            {
                clientId = entityData.clientID,
                entityId = entityData.entityID,
                entityType = (int)entityData.current_Entity_Type,
                rot = eContainer.transform.rotation,
                pos = eContainer.transform.position,

                //since using parenting for objects, we need to translate local to global scalling when having it in your hand, when releasing we need to return such objects scalling from global to local scale
                scaleFactor = eContainer.transform.lossyScale.x,
            };

            netUpdateHandler.NetworkUpdate(coords);
        }
        #endregion
    }
}