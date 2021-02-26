using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Komodo.Runtime
{
    public class EntityDataAuthoring : MonoBehaviour//, IConvertGameObjectToEntity
    {
        [Header("Identification")]
        public bool hasOurPlayerData;

        public Entity_Type current_Entity_Type;

        public bool hasDisplayState;

        [Header("Constraints")]
        public bool hasTransformLockState;

        [Header("Trasformations")]
        public bool hasPosition;
        public bool hasRotation;

        public void Awake()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity EntityForThisGO = entityManager.CreateEntity();

#if UNITY_EDITOR
            entityManager.SetName(EntityForThisGO, gameObject + "--Data");
#endif

            if (hasOurPlayerData) entityManager.AddComponentData(EntityForThisGO, new OurPlayerTag());

            entityManager.AddComponentData(EntityForThisGO, new NetworkEntityIdentificationComponentData()
            {
                current_Entity_Type = this.current_Entity_Type,
                entityID = (int)this.current_Entity_Type,

            });

            if (hasDisplayState) entityManager.AddComponentData(EntityForThisGO, new DesktopStateTag());

            if (hasTransformLockState) entityManager.AddComponentData(EntityForThisGO, new TransformLockTag());

            if (hasPosition) entityManager.AddComponentData(EntityForThisGO, new Translation());

            if (hasRotation) entityManager.AddComponentData(EntityForThisGO, new Rotation());

            Destroy(this);
        }



    }
}

