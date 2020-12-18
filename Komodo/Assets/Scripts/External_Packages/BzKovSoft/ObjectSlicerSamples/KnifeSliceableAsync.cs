using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BzKovSoft.ObjectSlicer;
using System.Diagnostics;

namespace BzKovSoft.ObjectSlicerSamples
{
	/// <summary>
	/// This script will invoke slice method of IBzSliceableNoRepeat interface if knife slices this GameObject.
	/// The script must be attached to a GameObject that have rigidbody on it and
	/// IBzSliceable implementation in one of its parent.
	/// </summary>
	[DisallowMultipleComponent]
	public class KnifeSliceableAsync : MonoBehaviour
	{
		IBzSliceableNoRepeat _sliceableAsync;

        Entity_Data entityData;
        BzKnife templateKnife;
		void Start()
		{
            if(_sliceableAsync == null)
			_sliceableAsync = GetComponentInParent<IBzSliceableNoRepeat>();

           // templateKnife = new BzKnife();

            try
            {
                if(entityData == null)
                entityData = GetComponent<Net_Register_GameObject>().entity_data;
            }
            catch
            {
                print("Could not Locate EntityData From Net_Register");
            }
		}



		void OnTriggerEnter(Collider other)
		{
			var knife = other.gameObject.GetComponent<BzKnife>();
			if (knife == null)
				return;

            StartCoroutine(Slice(knife));
		}
        
        public BzKnife PropagatedSlice(Vector3 BladeDir, Vector3 MoveDir, Vector3 KnifeOrigin)
        {
          //  print("I AM PRESENT");

            templateKnife = new BzKnife();
            templateKnife.isCustomValues = true;
            templateKnife.customBladeDir = BladeDir;
            templateKnife.customMoveDir = MoveDir;
            templateKnife.customOrigin = KnifeOrigin;

            //allows to differentiate each knife
            templateKnife.BeginNewSlice();

            //refering to the same object? updates through network?
            //templateKnife.customObjectPosition = customObjectPosition;

            SliceNew(templateKnife);

            
            //if (gameObject.activeInHierarchy)
            //    StartCoroutine(Slice(templateKnife));
            //else
            //    bzKifesList.Add(templateKnife);

            return templateKnife;

        }
     //   List<BzKnife> bzKifesList = new List<BzKnife>();
        //TODO IENUMERATORS CANT BE CALLED ON A INACTIVATED OBJECT - WE MESS UP THE ORDER OF NET_OBJECTLIST NOT DOING IT AS SOON AS IT IS DONE THIS WILL NOT WORK
        //public void OnEnable()
        //{
        //    foreach (var kn in bzKifesList)
        //    {
        //        StartCoroutine(Slice(kn));
        //    }
        //}
        private IEnumerator Slice(BzKnife knife)
		{
			// The call from OnTriggerEnter, so some object positions are wrong.
			// We have to wait for next frame to work with correct values
			yield return null;

			Vector3 point = GetCollisionPoint(knife);
			Vector3 normal = Vector3.Cross(knife.MoveDirection, knife.BladeDirection);
			Plane plane = new Plane(normal, point);


            try
            {
                if (entityData == null)
                    entityData = GetComponent<Net_Register_GameObject>().entity_data;

                UnityEngine.Debug.Log("Entity ID : " + entityData.entityID + " : " + "BladeDir :" + knife.BladeDirection.ToString("F2") + " MoveDIR: " + knife.MoveDirection.ToString("F2") + "Origin" + knife.Origin.ToString("F2"));

            }
            catch { }
            //To simulate this for everyone else, we need: knife origin, knife movedirection and knife blade direction



            if (_sliceableAsync != null)
			{
               
				_sliceableAsync.Slice(plane, knife.SliceID, null);
            }
          
		}
        //wrong?
        private void SliceNew(BzKnife knife)
        {
            // The call from OnTriggerEnter, so some object positions are wrong.
            // We have to wait for next frame to work with correct values
            //  yield return null;
            //  print("I AM PRESENT");

            //CHANGE GETCOLLISION SECOND ARGUMENT YOU ARE REFERING TO THE MAIN OBJECT ALREADY
            Vector3 point = GetCollisionPoint(knife);//GetCollisionPointNew(knife, knife.CustomCollisionPoint);
            Vector3 normal = Vector3.Cross(knife.MoveDirection, knife.BladeDirection);
            Plane plane = new Plane(normal, point);

            //  print("I AM PRESENT");
            entityData = GetComponent<Net_Register_GameObject>().entity_data;
            // UnityEngine.Debug.Log("Entity ID : " + entityData.entityID + " : " + "BladeDir :" + knife.BladeDirection + " MoveDIR: " + knife.MoveDirection + "Origin" + knife.Origin);

            UnityEngine.Debug.Log("Entity ID : " + entityData.entityID + " : " + "BladeDir :" + knife.BladeDirection.ToString("F2") + " MoveDIR: " + knife.MoveDirection.ToString("F2") + "Origin" + knife.Origin.ToString("F2"));



            if (_sliceableAsync != null)
            {
            //    print("I AM PRESENT");
                _sliceableAsync.Slice(plane, knife.SliceID, null);
            }
            else
            {
                _sliceableAsync = GetComponent<IBzSliceableNoRepeat>();
                entityData = GetComponent<Net_Register_GameObject>().entity_data;

                _sliceableAsync.Slice(plane, knife.SliceID, null);
            }
        }

        private Vector3 GetCollisionPoint(BzKnife knife)
		{
			Vector3 distToObject = transform.position - knife.Origin;
			Vector3 proj = Vector3.Project(distToObject, knife.BladeDirection);

			Vector3 collisionPoint = knife.Origin + proj;
			return collisionPoint;
		}

        //private Vector3
        private Vector3 GetCollisionPointNew(BzKnife knife, Vector3 customPosition)
        {
            //this is the dependency //DONT NEED CUSTOM SINCE YOU ALREADY ARE REFEARING TO ITS REAL POSITION? TAKE OFF LAST ARGUMENT

            Vector3 distToObject = customPosition - knife.Origin;
            Vector3 proj = Vector3.Project(distToObject, knife.BladeDirection);

            Vector3 collisionPoint = knife.Origin + proj;
            return collisionPoint;
        }
    }
}