using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Entities;

namespace Komodo.Runtime
{
    /// <summary>
    /// This class controls the erase fucntionality in Komodo.
    /// </summary>
    public class EraseManager : SingletonComponent<EraseManager>
    {
        public static EraseManager Instance
        {
            get { return ((EraseManager)_Instance); }
            set { _Instance = value; }
        }

        TriggerEraseDraw leftHandErase;

        TriggerEraseDraw rightHandErase;

        public EntityManager entityManager;

        public virtual void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        /// <summary>
        /// This overidable function can erase drawings and adding the erased drawings into an Undo-stack, The Undo-stack is managed by the UndoRedoManager.cs. 
        /// This Funcion is also available to be overided to include more networkobjects that can be erased and undone.
        /// </summary>
        /// <param name="netObj">network objects; objects that can be seen by other users through the network</param>
        public virtual void TryAndErase(NetworkedGameObject netObj)
        {
            var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObj.Entity).entityID;

            //draw functionality 
            if (entityManager.HasComponent<DrawingTag>(netObj.Entity))
            {
                //turn it off for ourselves and others
                netObj.gameObject.SetActive(false);

                // when actions of erasing are being captured, the curStrokepos and curColor will both be set to 0. 
                DrawingInstanceManager.Instance.SendDrawUpdate(entityID, Entity_Type.LineNotRender);

                //save our reverted action for undoing the process with the undo button
                if (UndoRedoManager.IsAlive)
                {
                    UndoRedoManager.Instance.savedStrokeActions.Push
                    (
                        (System.Action)
                        (
                            () =>
                            {
                                netObj.gameObject.SetActive(true);

                                DrawingInstanceManager.Instance.SendDrawUpdate(entityID, Entity_Type.LineRender);
                            }
                        )
                    );
                }
            }

    }
    }
}
