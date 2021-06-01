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
            if (other.TryGetComponent(out NetworkedGameObject netReg))
                EraseManager.Instance.TryAndErase(netReg);
        }

        public void OnEnable()=> onTriggeredOn.Invoke();
        public void OnDisable() => onTriggeredOff.Invoke();

    }
}
