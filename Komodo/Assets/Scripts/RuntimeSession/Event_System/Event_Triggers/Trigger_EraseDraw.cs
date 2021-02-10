using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(Collider))]
public class TriggerEraseDraw : MonoBehaviour
{
    public EntityManager entityManager;

    public void OnTriggerEnter(Collider other)
    {
        //if line? tag, line rend? 
        if (!other.CompareTag("Drawing")) 
        {
            return;
        }

        var netReg = other.GetComponent<NetworkedGameObject>();

        if (netReg)
        {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netReg.Entity).entityID;

                if (ClientSpawnManager.Instance.networkedObjectFromEntityID.ContainsKey(entityID))
                {
                    Destroy(ClientSpawnManager.Instance.networkedObjectFromEntityID[entityID].gameObject);
                    ClientSpawnManager.Instance.networkedObjectFromEntityID.Remove(entityID);
                }

                NetworkUpdateHandler.Instance.DrawUpdate(
                       new Draw((int)NetworkUpdateHandler.Instance.client_id, entityID
                       , (int)Entity_Type.LineDelete, 1, Vector3.zero,
                           Vector4.zero));
        }
    }
    

}
