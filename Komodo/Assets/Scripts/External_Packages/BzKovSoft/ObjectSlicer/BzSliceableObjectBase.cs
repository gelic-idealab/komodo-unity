using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading;
using BzKovSoft.ObjectSlicer;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Base class for sliceable object
	/// </summary>
	public abstract class BzSliceableObjectBase : BzSliceableBase
	{
		protected override AdapterAndMesh GetAdapterAndMesh(Renderer renderer)
		{
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