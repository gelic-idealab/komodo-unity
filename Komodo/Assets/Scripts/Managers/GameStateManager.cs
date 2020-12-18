//This is done for optimizing prevent crossing the barrier between native and managed with Update calls, it centralizes calls
//see Unity Game Optimization - Third Edition - By Dr. Davide Aversa , Chris Dickinson & https://forum.unity.com/threads/net-native-and-il2cpp.413972/ "Although IL2CPP is generating C++ code, there is still a cost of marshal types from managed to native code for p/invoke calls."
// we only register classes that run through the lifetime of the app since runing them this way does not let update calls to stop when gameobject are inactive..

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IUpdatable
{
    void OnUpdate(float realTime);
}

public interface ILateUpdatable
{
    void OnLateUpdate(float realTime);
}

[System.Serializable]
public class StateTrigger : UnityEvent<Coroutine> { }

[SerializeField]
public class GameStateManager : SingletonComponent<GameStateManager>
{
    public static GameStateManager Instance
    {
        get { return ((GameStateManager)_Instance); }
        set { _Instance = value; }
    }

    public CanvasGroup loadStateCanvas;
    public CanvasGroup mainUIDashboard;
    public Text loadProgressDisplay;

    [ShowOnly] public bool isClientAvatarLoading_Finished;
    [ShowOnly] public bool isAssetLoading_Finished;
    [ShowOnly] public bool isUISetup_Finished;

    //Initiation process --> ClientAvatars --> URL Downloads --> UI Setup --> SyncState
    public IEnumerator Start()
    {
        ToogleMainUIRendering(false);

        loadProgressDisplay.text = "Loading Avatars To Display";
        yield return new WaitUntil(() => isClientAvatarLoading_Finished);

        loadProgressDisplay.text = "Downloading and Loading Assets";
        yield return new WaitUntil(() => isAssetLoading_Finished);

        loadProgressDisplay.text = "UI Button Setup is finnished";
        yield return new WaitUntil(() => isUISetup_Finished);

        loadStateCanvas.gameObject.SetActive(false);
        ToogleMainUIRendering(true);
    }

    public void ToogleMainUIRendering(bool activeState)
    {
        if (activeState)
        {
            mainUIDashboard.alpha = 1;
            mainUIDashboard.blocksRaycasts = true;
        }
        else
        {
            mainUIDashboard.alpha = 0;  //SetActive(false);
            mainUIDashboard.blocksRaycasts = false;
        }
    }


    #region Update Registration Calls
    public List<IUpdatable> updateObjects = new List<IUpdatable>();
    public List<ILateUpdatable> lateUpdateObjects = new List<ILateUpdatable>();

    public void RegisterUpdatableObject(IUpdatable obj)
    {
        if (!updateObjects.Contains(obj))
            updateObjects.Add(obj);
    }

    public void DeRegisterUpdatableObject(IUpdatable obj)
    {
        if (updateObjects.Contains(obj))
            updateObjects.Remove(obj);
    }


    public void RegisterLateUpdatableObject(ILateUpdatable obj)
    {
        if (!lateUpdateObjects.Contains(obj))
            lateUpdateObjects.Add(obj);
    }

    public void DeRegisterLateUpdatableObject(ILateUpdatable obj)
    {
        if (lateUpdateObjects.Contains(obj))
            lateUpdateObjects.Remove(obj);
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
