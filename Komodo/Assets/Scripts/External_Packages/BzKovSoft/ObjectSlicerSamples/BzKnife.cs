using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicerSamples
{
	/// <summary>
	/// The script must be attached to a GameObject that have collider marked as a "IsTrigger".
	/// </summary>
	public class BzKnife : MonoBehaviour
	{
		public int SliceID { get; private set; }
		Vector3 _prevPos;
		Vector3 _pos;

		[SerializeField]
		private Vector3 _origin = Vector3.down;

		//[SerializeField]
		public Vector3 _direction = Vector3.up;

        public Transform thisTransform;
        [Header("Custom_Edits for Propagation")]
        public bool isCustomValues = false;
 
        public Vector3 customMoveDir = Vector3.zero;
        public Vector3 customBladeDir = Vector3.zero;

       // public Vector3 customObjectPosition;

        public Vector3 customOrigin = Vector3.zero;
        private void Awake()
        {
            thisTransform = transform;

        }
        private void Update()
        {
          //  _prevPos = _pos;
            _pos = thisTransform.position;
            //new erase and change from bottom if issues
           // currentRotation = thisTransform.rotation;
        }

        private void LateUpdate()
        {
          //  _pos = thisTransform.position;
            if (Mathf.Approximately(Vector3.Distance(_prevPos, _pos), 0))
            {
               
            }
            else
            {
                _direction = thisTransform.TransformDirection((_prevPos - _pos));
                //  _direction = thisTransform.TransformDirection((_pos - _prevPos));//Vector3.ProjectOnPlane((_pos - _prevPos).normalized, transform.right );
                _prevPos = thisTransform.position;
            }


            
        }
    
        public Vector3 Origin
		{
            
			get
			{
                if (isCustomValues == false)
                {
                    Vector3 localShifted = thisTransform.InverseTransformPoint(thisTransform.position) + _origin;
                    return thisTransform.TransformPoint(localShifted);
                }
                else
                {

                    return customOrigin;
                }
			}
		}
    

		public Vector3 BladeDirection { get { if (isCustomValues == false) return thisTransform.rotation * _direction.normalized;
                                              else  return customBladeDir;  } }

		public Vector3 MoveDirection { get { if (isCustomValues == false) return (_pos - _prevPos).normalized;
                                             else  return customMoveDir;  } }

        //public Vector3 CustomCollisionPoint
        //{
        //    get
        //    {
        //        return customObjectPosition;
        //    }
        //}

        public void BeginNewSlice()
		{
			SliceID = SliceIdProvider.GetNewSliceId();
		}
	}
}
