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

//Customized unityevent to allow it to accept particular funcions to send our network information to
[System.Serializable]public class Coord_UnityEvent : UnityEvent<Position> { }

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

    [Header("Attach Funcions to send our serialized AvatarEntityGroup positions to")]
    public Coord_UnityEvent coordExport;

    [Tooltip("Attach our MainClient AvatarEntityGroup to send updates accross Network ")]
    public AvatarEntityGroup mainClientAvatarEntityGroup;
   // public Transform[] transformsNetworkOutput;
    private Transform leftHandEntityTransform;
    private Transform rightHandEntityTransform;
    private Transform headEntityTransform;


    //list to maintain objects in scene to send update accross
    [HideInInspector]public List<Net_Register_GameObject> entityContainers_InNetwork_OutputList = new List<Net_Register_GameObject>();
    [HideInInspector]public List<Net_Register_GameObject> physics_entityContainers_InNetwork_OutputList = new List<Net_Register_GameObject>();


    //To stop update loop from sending information accross the network when our client setup is not complete
    private bool isSendingUpdates;

    private int clientID => NetworkUpdateHandler.Instance.client_id;

    //Hand References for position and animation information
    private Vector3 leftHandOriginalLocalPosition;
    private Vector3 rightHandOriginalLocalPosition;
    private Animator leftHandAnimator;
    private Animator rightHandAnimator;

  
    IEnumerator Start()
    {
        //if we are missing our avatarentitygroup in editor try to get it from our game object
        if (!mainClientAvatarEntityGroup) mainClientAvatarEntityGroup = GetComponent<AvatarEntityGroup>();
        if (!mainClientAvatarEntityGroup) Debug.LogError("No mainclientAvatarEntityGroup found in  MainClientUpdater.cs to use to send updates to network", gameObject);

        //cashe our avatar transform references
        (headEntityTransform, leftHandEntityTransform, rightHandEntityTransform) = (mainClientAvatarEntityGroup._EntityContainer_Head.transform, mainClientAvatarEntityGroup._EntityContainer_hand_L.transform, mainClientAvatarEntityGroup._EntityContainer_hand_R.transform);

        //get references to AnimControllers to send anim state info 
        (leftHandAnimator, rightHandAnimator) = (leftHandEntityTransform.GetComponent<Animator>(), rightHandEntityTransform.GetComponent<Animator>());
     //   (leftHandAnimator, rightHandAnimator) = (transformsNetworkOutput[1].GetComponent<Animator>(),  transformsNetworkOutput[2].GetComponent<Animator>());

        if (!rightHandAnimator || !leftHandAnimator)
            Debug.LogError("We are missing our Animator Controller from our hands in MainClientUpdater");

        //Wait for the avatart to finish loading to allow us to continue and start sending updates
        yield return new WaitUntil(() => GameStateManager.Instance.isClientAvatarLoading_Finished);

        //Register our OnUpdate funcion to start sending updates 
        GameStateManager.Instance.RegisterUpdatableObject(this);

        //Grab current position of hands to detect if they have moved to avoid rendering them when they havent;
        leftHandOriginalLocalPosition = mainClientAvatarEntityGroup._EntityContainer_hand_L.transform.localPosition;
        rightHandOriginalLocalPosition = mainClientAvatarEntityGroup._EntityContainer_hand_R.transform.localPosition;

    }

    public void OnUpdate(float realTime)
    {
        //SENDS AVATAR HEAD REFERENCE TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
        SendUpdatesToNetwork(Entity_Type.users_head, headEntityTransform.position, headEntityTransform.rotation);

        //CHECKS IF OUR L_HAND HAS MOVED TO SEND LHAND REFERENCE DATA TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
        if (leftHandOriginalLocalPosition != leftHandEntityTransform.localPosition)
        {
            SendUpdatesToNetwork(Entity_Type.users_Lhand, leftHandEntityTransform.position, leftHandEntityTransform.rotation);
            leftHandOriginalLocalPosition = leftHandEntityTransform.localPosition;
        }

        //CHECKS IF OUR R_HAND HAS MOVED TO SENDS LHAND REFERENCE DATA TO FUNCION TO FIT STRUCTURE NEEDED FOR UNITYEVENT
        if (rightHandOriginalLocalPosition != rightHandEntityTransform.localPosition)
        {
            SendUpdatesToNetwork(Entity_Type.users_Rhand, rightHandEntityTransform.position, rightHandEntityTransform.rotation);
            rightHandOriginalLocalPosition = rightHandEntityTransform.localPosition;

        }

        //ADJUST DATA AND SEND UPDATES FROM THOSE GAMEOBJECTS REGISTERED TO OUR LIST 
        foreach (var entityContainers in entityContainers_InNetwork_OutputList)
            Send_GameObject_UpdatesToNetwork(entityContainers);

        foreach (var entityContainers in physics_entityContainers_InNetwork_OutputList)
            Send_PHYSICS_GameObject_UpdatesToNetwork(entityContainers);

        //remove physics objects that should not send calls anymore if RigidBody is changed to isKinematic or IsSleeping()
        foreach (var item in physicsnRGOToRemove)
            physics_entityContainers_InNetwork_OutputList.Remove(item);

        //clear the list of physics objects to remove from sending updates
        physicsnRGOToRemove.Clear();

    }

    //When the GameObject with this script attached is destroyed we take it out of the update loop
    public void OnDestroy()
    {
        if (GameStateManager.IsAlive)
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }

    #region Add/Remove From Update Loop Funcions
    /// <summary>
    /// Place GameObject in update loop to send network information across
    /// </summary>
    /// <param name="nRO attached to an objects to keep network relevant data"></param>
    public void PlaceInNetworkUpdateList(Net_Register_GameObject nRO)
    {
        if (!entityContainers_InNetwork_OutputList.Contains(nRO))
            entityContainers_InNetwork_OutputList.Add(nRO);
    }
    /// <summary>
    /// Remove GameObject from update loop to stop sending network information
    /// </summary>
    /// <param name="nRO attached to an objects to keep network relevant data"></param>
    public void RemoveFromInNetworkUpdateList(Net_Register_GameObject nRO)
    {
        if (entityContainers_InNetwork_OutputList.Contains(nRO))
            entityContainers_InNetwork_OutputList.Remove(nRO);
    }
    #endregion

    #region Serializing Methods and UnityEvent Invoke Calls
    /// <summary>
    /// Meant to convert our Avatar data to follow our POSITION struct to be sent each update
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void SendUpdatesToNetwork(Entity_Type entityType, Vector3 position, Quaternion rotation)
    {
        
        Position coords = new Position
        {
            clientId = this.clientID,
            entityId = (this.clientID * 10) + (int)entityType,
            entityType = (int)entityType,
            rot = rotation,
            pos = position,
        };

        //setup animation parameter to update
        switch (entityType)
        {

            case Entity_Type.users_Lhand:
                coords.scaleFactor = leftHandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                break;

            case Entity_Type.users_Rhand:
                coords.scaleFactor = rightHandAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                break;

        }

        //send data over to those funcions attached to our UnityEvent
        coordExport.Invoke(coords);

    }

    /// <summary>
    /// Meant to convert our GameObject data to follow our POSITION struct to be sent each update
    /// </summary>
    /// <param name="Net_Register_GameObject container of data"></param>
    public void Send_GameObject_UpdatesToNetwork(Net_Register_GameObject eContainer)
    {
        Position coords = new Position
        {
            clientId = this.clientID,
            entityId = (int)eContainer.entity_data.entityID,
            entityType = (int)eContainer.entity_data.current_Entity_Type,
            rot = eContainer.transform.rotation,
            pos = eContainer.transform.position,

            //since using parenting for objects, we need to translate local to global scalling when having it in your hand, when releasing we need to return such objects scalling from global to local scale
            scaleFactor = eContainer.transform.lossyScale.x,
        };

        coordExport.Invoke(coords);
    }

    List<Net_Register_GameObject> physicsnRGOToRemove = new List<Net_Register_GameObject>();
  
    /// <summary>
    /// Meant to convert our Physics GameObject data send  data to follow our POSITION struct to be sent each update
    /// </summary>
    /// <param name="Net_Register_GameObject container of data"></param>
    public void Send_PHYSICS_GameObject_UpdatesToNetwork(Net_Register_GameObject eContainer)
    {
        //make sure that we setup the reference to our rigidBody of our physics object that we are using to send data from
        if (!ClientSpawnManager.Instance.entityID_To_RigidBody.ContainsKey(eContainer.entity_data.entityID))
            ClientSpawnManager.Instance.entityID_To_RigidBody.Add(eContainer.entity_data.entityID , eContainer.GetComponent<Rigidbody>());

        var rb = ClientSpawnManager.Instance.entityID_To_RigidBody[eContainer.entity_data.entityID]; 

        if (!rb)
        {
            Debug.LogError("There is no rigidbody in netobject entity id DICTIONARY: " + eContainer.entity_data.entityID);
            return;
        }

        //remove if not currently using physics and if it is sleeping  or if is currently grabbed 
        if (!rb.isKinematic && rb.IsSleeping() || eContainer.entity_data.isCurrentlyGrabbed)
        {
            physicsnRGOToRemove.Add(eContainer);

            //Send a last update for our network objects to be remove their physics funcionality to sync with others. 
            StopPhysicsUpdates(eContainer);
        }
            
        Position coords = new Position
        {
            clientId = this.clientID,
            entityId = (int)eContainer.entity_data.entityID,
            entityType = (int)eContainer.entity_data.current_Entity_Type,
            rot = eContainer.transform.rotation,
            pos = eContainer.transform.position,
            scaleFactor = eContainer.transform.lossyScale.x,
        };

        coordExport.Invoke(coords);
    }

    /// <summary>
    /// A call to remove Physics funcionality from specified netObject 
    /// </summary>
    /// <param name="eContainer"></param>
    public void StopPhysicsUpdates(Net_Register_GameObject eContainer)
    {
        Position coords = new Position
        {
            clientId = this.clientID,
            entityId = (int)eContainer.entity_data.entityID,
            entityType = (int)Entity_Type.physicsEnd,
            
        };
        coordExport.Invoke(coords);
    }
    #endregion
}