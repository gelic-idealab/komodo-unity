using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	[DisallowMultipleComponent]
	class BzReaplyForce : MonoBehaviour, IBzObjectSlicedEvent
	{
		public void ObjectSliced(GameObject original, GameObject resultNeg, GameObject resultPos)
        {
			// we need to wait one fram to allow destroyed component to be destroyed.
			StartCoroutine(NextFrame(original, resultNeg, resultPos));
        }

		private IEnumerator NextFrame(GameObject original, GameObject resultNeg, GameObject resultPos)
		{
			yield return null;

			var oRigid = original.GetComponent<Rigidbody>();
			var aRigid = resultNeg.GetComponent<Rigidbody>();
			var bRigid = resultPos.GetComponent<Rigidbody>();

			if (oRigid == null)
				yield break;

			aRigid.angularVelocity = oRigid.angularVelocity;
			bRigid.angularVelocity = oRigid.angularVelocity;
			aRigid.velocity = oRigid.velocity;
			bRigid.velocity = oRigid.velocity;
		}
	}
}
