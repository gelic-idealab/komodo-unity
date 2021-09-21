using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Utilities
{
    public class SimpleLookAt : MonoBehaviour//,IUpdatable
    {

        //not in main updatable because I want it to be shut off when not in view and not on;


        public Transform lookAtTarget;
        public Transform thisTransform;

        public string playspaceTag = "XRCamera";
        void Start()
        {
            thisTransform = transform;

            if (lookAtTarget == null)
                lookAtTarget = GameObject.FindWithTag(playspaceTag).transform;
        }
        public void Update()
        {
            thisTransform.LookAt(lookAtTarget, Vector3.up);
        }
    }
}
