using UnityEngine;
using Unity.Entities;
using UnityEngine.Events;

namespace Komodo.Runtime
{   
    /// <summary>
    /// Erase functionality and displaying/disabling erase tool model.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerEraseDraw : MonoBehaviour
    {
        public EntityManager entityManager;

        /// <summary>
        /// Unity runtime method.
        /// </summary>
        public UnityEvent onTriggeredOn;

        /// <summary>
        /// Unity runtime method.
        /// </summary>
        public UnityEvent onTriggeredOff;

        /// <summary>
        /// This function is called when a collider enters the trigger. In this case, if the collider enters this trigger,
        /// it will get the NetworkedGameObject compoenent in the collider and erase it with the EraseManager to achieve the erasing functionality. 
        /// </summary>
        /// <param name="other">Collider</param>
        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out NetworkedGameObject netReg))
                EraseManager.Instance.TryAndErase(netReg);
        }

        /// <summary>
        /// This invokes the inspector assigned method for the gameobject that this script is attached to.
        /// It displays the erase tool in the VR mode.
        /// </summary>
        public void OnEnable()=> onTriggeredOn.Invoke();

        /// <summary>
        /// Disable the erase tool in the VR mode.
        /// </summary>
        public void OnDisable() => onTriggeredOff.Invoke();

    }
}
