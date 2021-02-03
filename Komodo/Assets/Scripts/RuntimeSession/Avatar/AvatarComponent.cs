using UnityEngine;
using UnityEngine.EventSystems;

public class AvatarComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
   public  Entity_Type thisEntityType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        try
        {
            NetworkUpdateHandler.Instance.InteractionUpdate(
          new Interaction
          {
              interactionType = (int)INTERACTIONS.LOOK,
              sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
              targetEntity_id = (int)thisEntityType,
          });

        }
        catch
        {
            Debug.LogWarning("Couldn't process look interaction event");
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        try
        {
            NetworkUpdateHandler.Instance.InteractionUpdate(
           new Interaction
           {
               interactionType = (int)INTERACTIONS.LOOK_END,
               sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
               targetEntity_id = (int)thisEntityType,
           });

        }
        catch
        {
            Debug.LogWarning("Couldn't process look interaction event");
        }
    }
}
