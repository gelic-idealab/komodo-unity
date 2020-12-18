using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.EventHandlers;
using UnityEngine.Profiling;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object
	/// </summary>
	[DisallowMultipleComponent]
	public class LazyActionRunner : MonoBehaviour
	{
		List<Action> _postponeActions;

		private void OnEnable()
		{
			_postponeActions = new List<Action>();
		}

        public void RunLazyActions()
        {
			if (_postponeActions == null)
				return;
			
			StartCoroutine(ProcessSlicePostponeActions(_postponeActions));
        }

		private IEnumerator ProcessSlicePostponeActions(List<Action> actions)
		{
			for (int i = 0; i < actions.Count; i++)
			{
				yield return null;
				var action = actions[i];
				action();
			}
			
			Destroy(this);
		}

        public void AddLazyAction(Action action)
        {
			if (_postponeActions == null)
			{
				action();
			}
			else
			{
	            _postponeActions.Add(action);
			}
        }
	}
}