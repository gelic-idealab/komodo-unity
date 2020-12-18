using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using BzKovSoft.ObjectSlicer;

namespace BzKovSoft.CharacterSlicer
{
	/// <summary>
	/// Base class for sliceable character
	/// </summary>
	public abstract class BzSliceableCharacterBase : BzSliceableBase
	{
		private void Awake()
		{
			var animator = GetComponent<Animator>();
			if (animator != null && animator.updateMode != AnimatorUpdateMode.AnimatePhysics)
				UnityEngine.Debug.LogWarning("Recomended to use Animator.UpdateMode = AnimatePhysics for your sliceable character");
		}

		protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
		{
			var skinnedRenderer = renderer as SkinnedMeshRenderer;
			if (skinnedRenderer != null)
			{
				var result = new AdapterAndMesh();
				result.mesh = skinnedRenderer.sharedMesh;
				result.adapter = new BzSliceSkinnedMeshAdapter(skinnedRenderer);
				return result;
			}

			var meshRenderer = renderer as MeshRenderer;
			if (meshRenderer != null)
			{
				var result = new AdapterAndMesh();
				result.mesh = meshRenderer.gameObject.GetComponent<MeshFilter>().sharedMesh;
				result.adapter = new BzSliceMeshFilterAdapter(result.mesh.vertices, meshRenderer.gameObject);
				return result;
			}

			return null;
		}
	}
}