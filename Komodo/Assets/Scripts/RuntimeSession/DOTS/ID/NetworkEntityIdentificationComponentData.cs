using Unity.Entities;

[GenerateAuthoringComponent]
public struct NetworkEntityIdentificationComponentData : IComponentData
{
    public int clientID;
    public int entityID;
    public int sessionID;

    //users_head = 0,
    //users_Lhand = 1,
    //users_Rhand = 2,
    //objects = 3,

    //main_Player = 5,

    //Line = 10,
    //LineEnd = 11,
    //LineDelete = 12,

    //physicsObject = 4,
    //physicsEnd = 8,
    public Entity_Type current_Entity_Type;
}
