using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketIOEditorSimulator : SingletonComponent<SocketIOEditorSimulator>
{
    public static SocketIOEditorSimulator Instance
    {
        get { return ((SocketIOEditorSimulator)_Instance); }
        set { _Instance = value; }
    }

    public bool isVerbose = false;
    public bool doLogClientEvents = true;
    public bool doLogCustomInteractions = true;
    public bool doLogPositionEvents = false;

    public int clientId;
    public int sessionId;
    public int isTeacher;
    public string InstantiationManagerName = "Instantiation Manager";
    public string NetworkManagerName = "NetworkManager";
    private ClientSpawnManager _ClientSpawnManager;
    private NetworkUpdateHandler _NetworkUpdateHandler;

    public void Start () {
        var instMgr = GameObject.Find(InstantiationManagerName);
        if (!instMgr) {
            throw new System.Exception("You must have a GameObject named 'Instantiation Manager' in your scene.");
        }
        _ClientSpawnManager = instMgr.GetComponent<ClientSpawnManager>();

        if (!_ClientSpawnManager) {
            throw new System.Exception("Instantiation Manager must have a ClientSpawnManager component.");
        }

        var netMgr = GameObject.Find(NetworkManagerName);
        if (!netMgr) {
            throw new System.Exception("You must have a GameObject named 'Network Manager' in your scene.");
        }
        _NetworkUpdateHandler = netMgr.GetComponent<NetworkUpdateHandler>();

        if (!_NetworkUpdateHandler) {
            throw new System.Exception("Network Manager must have a NetworkUpdateHandler component.");
        }
    }

    public void GameInstanceSendMessage(string who, string what, string data) {
        if (isVerbose) Debug.Log($"GameInstanceSendMessage({who}, {what}, {data})");
    }

    public void Emit (string name, string data) {
        if (isVerbose) Debug.Log($"Emit({name}, {data})");
    }

    public void OnState (string jsonStringifiedData) {
        if (isVerbose) Debug.Log($"received state sync event: {jsonStringifiedData}");
        _ClientSpawnManager.SyncSessionState(jsonStringifiedData);
    }

    public void InitSessionStateHandler () {
        if (isVerbose) Debug.Log("InitSessionStateHandler");
        //todo(Brandon) -- set relay simulator to call OnState and send it data
    }

    public void InitSessionState () {
        if (isVerbose) Debug.Log("InitSessionState");
        Emit("state", "{ session_id: session_id, client_id: client_id }");
    }

    public void OnJoined (int clientId) {
        if (doLogClientEvents) Debug.Log($"OnJoined({clientId})");
        _NetworkUpdateHandler.RegisterNewClientId(clientId);
    }

    public void InitSocketIOClientCounter () {
        if (doLogClientEvents) Debug.Log("InitSocketIOClientCounter");
        //todo(Brandon): call OnJoined with clientId
    }

    public void OnDisconnected (int clientId) {
        if (doLogClientEvents) Debug.Log($"OnDisconnected({clientId})");
        _NetworkUpdateHandler.UnregisterClientId(clientId);
    }

    public void InitClientDisconnectHandler  () {
        if (doLogClientEvents) Debug.Log("InitClientDisconnectHandler");
        //todo(Brandon): call OnDisconnected with clientId
    }

    public void OnMicText (string jsonStringifiedData) {
        Debug.Log("OnMicText");
        _ClientSpawnManager.Text_Refresh(jsonStringifiedData);
    }

    public void InitMicTextHandler  () {
        Debug.Log("InitMicTextHandler");
        //todo(Brandon): call OnMicText with data
    }

    public void OnDraw (float[] data) {
        Debug.Log($"OnDraw({data.ToString()})");
    }

    public void InitReceiveDraw (float[] arrayPointer, int size) {
        Debug.Log("InitReceiveDraw");
        int drawCursor = 0;
        //todo(Brandon): call OnDraw with data and pass in drawCursor also
    }

    public void SendDraw (float[] arrayPointer, int size) {
        Debug.Log("SendDraw");
        Emit("draw", arrayPointer.ToString());
    }

    public int GetClientIdFromBrowser () {
        if (doLogClientEvents) Debug.Log("GetClientIdFromBrowser -- returning user-set value");
        return clientId;
    }

    public int GetSessionIdFromBrowser () {
        Debug.Log("GetSessionIdFromBrowser -- returning user-set value");
        return sessionId;
    }

    public int GetIsTeacherFlagFromBrowser () {
        Debug.Log("GetIsTeacherFlagFromBrowser -- returning user-set value");
        return isTeacher;
    }

    public void SocketIOSendPosition (float[] array, int size) {
        if (doLogPositionEvents) Debug.Log("SocketIOSendPosition");
        Emit("update", array.ToString());
    }

    public void SocketIOSendInteraction (int[] array, int size) {
        if (doLogCustomInteractions) Debug.Log($"SocketIOSendInteraction({array.ToString()}, {size})");
        Emit("interact", array.ToString());
    }

    public void OnRelayUpdate (float[] data) {
        Debug.Log($"OnRelayUpdate({data.ToString()})");
    }

    public void InitSocketIOReceivePosition(float[] arrayPointer, int size) {
        if (doLogPositionEvents) Debug.Log("InitSocketIOReceivePosition");
        var posCursor = 0;
        //todo(Brandon): call OnRelayUpdate, passing in data, and updating posCursor
    }

    public void OnInteractionUpdate (float[] data) {
        if (doLogCustomInteractions) Debug.Log($"OnInteractionUpdate({data.ToString()})");
    }
    
    public void InitSocketIOReceiveInteraction (int[] arrayPointer, int size) {
        if (isVerbose) Debug.Log("InitSocketIOReceiveInteraction");
        var intCursor = 0;
        //todo(Brandon): call OnInteractionUpdate, passing in data, and updating intCursor
    }

    public void Record_Change (int operation, int session_id) {
        if (operation == 0) {
            Emit("start_recording", session_id.ToString());
        } 
        else {
            Emit("end_recording", session_id.ToString());
        }
    }

    public string GrabAssets () {
        Debug.Log("GrabAssets -- returning \"{list:[]}\"");
        return "{list:[]}";
    }

}
