using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Entity_Container : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
 
   public Entity_Data entity_data;

    public void OnPointerEnter(PointerEventData eventData)
    {
        try
        {
            NetworkUpdateHandler.Instance.InteractionUpdate(
          new Interaction
          {
              interactionType = (int)INTERACTIONS.LOOK,
              sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,
              targetEntity_id = entity_data.entityID,
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
               sourceEntity_id = ClientSpawnManager.Instance.mainPlayer_RootTransformData.entityID,
               targetEntity_id = entity_data.entityID,
           });

        }
        catch
        {
            Debug.LogWarning("Couldn't process look interaction event");
        }
    }
}
