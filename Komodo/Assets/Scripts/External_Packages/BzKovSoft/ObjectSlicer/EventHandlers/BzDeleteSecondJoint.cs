using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	[DisallowMultipleComponent]
	class BzDeleteSecondJoint : MonoBehaviour, IBzObjectSlicedEvent
	{
		public void ObjectSliced(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
		    var oJoint = original.GetComponent<Joint>();

		    if (oJoint == null)
		        return;

            Mesh meshA = resultNeg.GetComponent<MeshFilter>().sharedMesh;
            Mesh meshB = resultPos.GetComponent<MeshFilter>().sharedMesh;

            if (meshA == null | meshB == null)
                return;

            float distToA = (oJoint.anchor - meshA.bounds.center).magnitude;
            float distToB = (oJoint.anchor - meshB.bounds.center).magnitude;

            if (distToA > distToB)
            {
                Destroy(resultNeg.GetComponent<Joint>());
            }
            else
            {
                Destroy(resultPos.GetComponent<Joint>());
            }
        }
    }
}
