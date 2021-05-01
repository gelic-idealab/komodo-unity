using System;
using Unity.Entities;

[Serializable]
public struct ButtonIDSharedComponentData : ISharedComponentData
{
    public int buttonID;
}
