using UnityEngine;
using Unity.Entities;

[RequireComponent(typeof(Collider))]
public class Trigger_EraseDraw : MonoBehaviour
{
    public EntityManager entityManager;

    public void OnTriggerEnter(Collider other)
    {
        //if line? tag, line rend? 
        if (!other.CompareTag("Drawing"))
            return;

        if(other.TryGetComponent(out NetworkAssociatedGameObject netReg))
        {
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netReg.Entity).entityID;

                if (ClientSpawnManager.Instance.entityID_To_NetObject_Dict.ContainsKey(entityID))
                {
                   //ClientSpawnManager.Instance.entityID_To_NetObject_Dict[entityID].gameObject);
                    ClientSpawnManager.Instance.entityID_To_NetObject_Dict.Remove(entityID);
                }

                NetworkUpdateHandler.Instance.DrawUpdate(
                       new Draw((int)NetworkUpdateHandler.Instance.client_id, entityID
                       , (int)Entity_Type.LineDelete, 1, Vector3.zero,
                           Vector4.zero));

                Destroy(other.gameObject);
        }
    }
    

}
