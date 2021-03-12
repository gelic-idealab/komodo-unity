using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class TriggerEraseDraw : MonoBehaviour
    {
        public EntityManager entityManager;

        public UnityEvent onTriggeredOn;
        public UnityEvent onTriggeredOff;

        public void OnTriggerEnter(Collider other)
        {
            //if line? tag, line rend? 
            if (!other.CompareTag("Drawing"))
            {
                return;
            }

            //with redo/undo we cannot destroy user and/or external strokes until the user list is greater than the allocated cutoff
            if (other.TryGetComponent(out NetworkedGameObject netReg))
            {
                //get our line entitiy reference
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netReg.Entity).entityID;

                /////turn it off for ourselves and others
                netReg.gameObject.SetActive(false);

                NetworkUpdateHandler.Instance.DrawUpdate(
                    new Draw((int)NetworkUpdateHandler.Instance.client_id, entityID
                    , (int)Entity_Type.LineNotRender, 1, Vector3.zero,
                        Vector4.zero));

                ////save our reverted action for undoing the process with the undo button
               if(UndoRedoManager.IsAlive)
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {

                    netReg.gameObject.SetActive(true);

                    NetworkUpdateHandler.Instance.DrawUpdate(
                       new Draw(NetworkUpdateHandler.Instance.client_id, entityID
                       , (int)Entity_Type.LineRender, 1, Vector3.zero,
                           Vector4.zero));
                }
                );

            }
        }

        public void OnEnable()=> onTriggeredOn.Invoke();
        public void OnDisable() => onTriggeredOff.Invoke();



    }
}
