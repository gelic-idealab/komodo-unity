using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Entity_Type
{
    users_head = 0,
    users_Lhand = 1,
    users_Rhand = 2,
    objects = 3,

    main_Player = 5,

    Line = 10,
    LineEnd = 11,
    LineDelete = 12,

    physicsObject = 4,
    physicsEnd = 8,
}

[System.Serializable]
[CreateAssetMenu(fileName = "ClientData_Asset", menuName = "ClientData_Asset", order = 0)]

public class Entity_Data : ScriptableObject
{
    [Header("Type_Of_Data")]
    public Entity_Type current_Entity_Type; 


    [Header("IDs")]
    [Space]
    public int entityID;
    public int clientID;
    public int sessionID;
    //public WebVRControllerHand clientObjectIds;


    [Header("Flags")]
    [Space]
    public bool isTeacher;
    public bool isInVR;
    public bool canMove;
    public bool isCurrentlyGrabbed;

    [Header("Trasformations")]
    [Space]
    //public Vector3 mainPlayerControlerPosition;
    public Vector3 pos;
    public Vector4 rot;

   
}
