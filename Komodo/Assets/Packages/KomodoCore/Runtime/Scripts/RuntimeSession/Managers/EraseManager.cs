using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Entities;

namespace Komodo.Runtime
{
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
        /// Funcion available to overide to include more networkobjects that can be erased and undone.
        /// </summary>
        /// <param name="netObj"></param>
        public virtual void TryAndErase(NetworkedGameObject netObj)
        {
            var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObj.Entity).entityID;

            //draw functionality 
            if (entityManager.HasComponent<DrawingTag>(netObj.Entity))
            {
                /////turn it off for ourselves and others
                netObj.gameObject.SetActive(false);

                DrawingInstanceManager.Instance.SendStrokeNetworkUpdate(entityID, Entity_Type.LineNotRender);

                ////save our reverted action for undoing the process with the undo button
                if (UndoRedoManager.IsAlive)
                    UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                    {

                        netObj.gameObject.SetActive(true);

                        DrawingInstanceManager.Instance.SendStrokeNetworkUpdate(entityID, Entity_Type.LineRender);
                    }
                    );

            }

    }
    }
}
