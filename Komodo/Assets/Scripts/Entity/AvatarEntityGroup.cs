using UnityEngine;
/// <summary>
/// Used to store Entity components that maek up avatars to help ease access to the GameObjects that should receive/send network updates. 
/// </summary>
public class AvatarEntityGroup : MonoBehaviour
{
    public int clientID;
    public Entity_Container _EntityContainer_Head;
    public Entity_Container _EntityContainer_hand_L;
    public Entity_Container _EntityContainer_hand_R;
}
