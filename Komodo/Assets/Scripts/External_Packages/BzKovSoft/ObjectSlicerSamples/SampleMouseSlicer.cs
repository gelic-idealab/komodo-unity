using UnityEngine;
using BzKovSoft.ObjectSlicer;
using System.Diagnostics;
using System;

namespace BzKovSoft.ObjectSlicerSamples
{
	/// <summary>
	/// Test class for demonstration purpose
	/// </summary>
	public class SampleMouseSlicer : MonoBehaviour
	{
		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				// if left mouse clicked, try slice this object

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

				var sliceId = SliceIdProvider.GetNewSliceId();

				for (int i = 0; i < hits.Length; i++)
				{
					var sliceableA = hits[i].transform.GetComponentInParent<IBzSliceableNoRepeat>();

					Vector3 direction = Vector3.Cross(ray.direction, Camera.main.transform.right);
					Plane plane = new Plane(direction, ray.origin);

					if (sliceableA != null)
						sliceableA.Slice(plane, sliceId, null);
				}
			}
		}
	}
}