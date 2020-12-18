using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public class ObjectSlicerInitializer : MonoBehaviour
	{
		static bool _initialized;
		void Start()
		{
			if (_initialized)
				return;

			_initialized = true;
			Init();
		}
		
		static void Init()
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Cube);;
			var slicer = go.AddComponent<ObjectSlicerInitializerObj>();
			slicer.asynchronously = true;
			slicer.defaultSliceMaterial = new Material(Shader.Find("Standard"));
			Action<BzSliceTryResult> action = (x) =>
			{
				if (!x.sliced)
					throw new InvalidOperationException();
				
				Destroy(x.outObjectNeg);
				Destroy(x.outObjectPos);
			};
			slicer.Slice(new Plane(Vector3.up, Vector3.zero), action);
		}

		class ObjectSlicerInitializerObj : BzSliceableObjectBase
		{
			protected override BzSliceTryData PrepareData(Plane plane)
			{
				var colliders = gameObject.GetComponentsInChildren<Collider>();

				return new BzSliceTryData()
				{
					componentManager = new StaticComponentManager(gameObject, plane, colliders),
					plane = plane,
				};
			}

			protected override void OnSliceFinished(BzSliceTryResult result)
			{

			}
		}
	}
}