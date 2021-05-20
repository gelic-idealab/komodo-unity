//This is done for optimizing prevent crossing the barrier between native and managed with Update calls, it centralizes calls
//see Unity Game Optimization - Third Edition - By Dr. Davide Aversa , Chris Dickinson & https://forum.unity.com/threads/net-native-and-il2cpp.413972/ "Although IL2CPP is generating C++ code, there is still a cost of marshal types from managed to native code for p/invoke calls."
// we only register classes that run through the lifetime of the app since runing them this way does not let update calls to stop when gameobject are inactive..

using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Komodo.Utilities;

namespace Komodo.Runtime
{

    #region Update Registration Call Interfaces
    public interface IUpdatable
    {
        void OnUpdate(float realTime);
    }

    public interface ILateUpdatable
    {
        void OnLateUpdate(float realTime);
    }
    #endregion

    [SerializeField]
    public class GameStateManager : SingletonComponent<GameStateManager>
    {
        public static GameStateManager Instance
        {
            get { return ((GameStateManager)_Instance); }
            set { _Instance = value; }
        }

        [ShowOnly] public bool isAvatarLoadingFinished;

        [ShowOnly] public bool isAssetImportFinished;
        private EntityManager entityManager;

        public void Awake()
        {
            // Forces the singleton to generate itself.
            var gsManager = Instance;
        }

        //Initiation process --> ClientAvatars --> URL Downloads --> UI Setup --> SyncState
        public IEnumerator Start()
        {
            if (UIManager.IsAlive)
            {
                UIManager.Instance.ToggleMenuVisibility(false);

                UIManager.Instance.initialLoadingCanvasProgressText.text = "Loading Avatars";
                yield return new WaitUntil(() => 
                {   
                    return isAvatarLoadingFinished;
                });

                //check if we are using imported objects
                if (ModelImportInitializer.IsAlive)
                {
                    UIManager.Instance.initialLoadingCanvasProgressText.text = "Loading Assets";
                    yield return new WaitUntil(() => isAssetImportFinished);
                }

                UIManager.Instance.initialLoadingCanvasProgressText.text = "Setting Up Menu";
                yield return new WaitUntil(() =>  
                {   
                    return UIManager.Instance.IsReady();
                });

                UIManager.Instance.initialLoadingCanvas.gameObject.SetActive(false);

                UIManager.Instance.ToggleMenuVisibility(true);
            }
        }

        #region Update Registration Calls
        public List<IUpdatable> updateObjects = new List<IUpdatable>();

        public List<ILateUpdatable> lateUpdateObjects = new List<ILateUpdatable>();

        public void RegisterUpdatableObject(IUpdatable obj)
        {
            if (!updateObjects.Contains(obj))
            {
                updateObjects.Add(obj);
            }
        }

        public void DeRegisterUpdatableObject(IUpdatable obj)
        {
            if (updateObjects.Contains(obj)) 
            {
                updateObjects.Remove(obj);
            }
        }


        public void RegisterLateUpdatableObject(ILateUpdatable obj)
        {
            if (!lateUpdateObjects.Contains(obj))
            {
                lateUpdateObjects.Add(obj);
            }
        }

        public void DeRegisterLateUpdatableObject(ILateUpdatable obj)
        {
            if (lateUpdateObjects.Contains(obj))
            {
                lateUpdateObjects.Remove(obj);
            }
        }

        void Update()
        {
            float rT = Time.realtimeSinceStartup;

            for (int i = 0; i < updateObjects.Count; i++)
            {
                updateObjects[i].OnUpdate(rT);
            }
        }
        void LateUpdate()
        {
            float rT = Time.realtimeSinceStartup;

            for (int i = 0; i < lateUpdateObjects.Count; i++)
            {
                lateUpdateObjects[i].OnLateUpdate(rT);
            }
        }

        #endregion
    }
}