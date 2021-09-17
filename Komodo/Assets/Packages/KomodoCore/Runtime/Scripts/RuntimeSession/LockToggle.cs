using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Toggle))]
    public class LockToggle : MonoBehaviour
    {
        private EntityManager entityManager;

        private int index;

        private Toggle toggle;

        public GameObject lockedIcon;

        public GameObject unlockedIcon;

        void Start ()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            toggle = GetComponent<Toggle>();

            if (!toggle)
            {
                throw new MissingComponentException("Toggle on object with LockButton script on it");
            }

            if (lockedIcon == null || unlockedIcon == null)
            {
                throw new UnassignedReferenceException("lockedIcon or unlockedIcon on LockToggle component");
            }

            toggle.onValueChanged.AddListener((bool doLock) =>
            {
                Toggle(doLock);
            });

            Toggle(false);
        }

        public void Toggle (bool doLock)
        {
            UpdateComponentData(doLock);

            UpdateUI(doLock);

            SendNetworkUpdate(doLock);
        }

        public void UpdateUI (bool doLock)
        {
            if (doLock)
            {
                lockedIcon.SetActive(true);

                unlockedIcon.SetActive(false);

                return;
            }

            lockedIcon.SetActive(false);

            unlockedIcon.SetActive(true);
        }

        public void Initialize (int index)
        {
            this.index = index;
        }

        public void SendNetworkUpdate (bool doLock)
        {
            int lockState = 0;

            //SETUP and send network lockstate
            if (doLock)
            {
                lockState = (int)INTERACTIONS.LOCK;
            }
            else
            {
                lockState = (int)INTERACTIONS.UNLOCK;
            }

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(ClientSpawnManager.Instance.GetNetworkedSubObjectList(this.index)[0].Entity).entityID;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = entityID,
                interactionType = lockState,
            });
        }

        public void UpdateComponentData (bool doLock)
        {
            foreach (NetworkedGameObject item in ClientSpawnManager.Instance.GetNetworkedSubObjectList(this.index))
            {
                if (doLock)
                {
                    if (!entityManager.HasComponent<TransformLockTag>(item.Entity))
                    {
                        entityManager.AddComponentData(item.Entity, new TransformLockTag());
                    }
                }
                else
                {
                    if (entityManager.HasComponent<TransformLockTag>(item.Entity))
                    {
                        entityManager.RemoveComponent<TransformLockTag>(item.Entity);
                    }
                }
            }
        }

        public void ProcessNetworkToggle (bool doLock)
        {
            UpdateComponentData(doLock);

            UpdateUI(doLock);
        }
    }
}