using BzKovSoft.ObjectSlicer;
using BzKovSoft.ObjectSlicer.Polygon;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	public class BzSliceTryResult
	{
		public BzSliceTryResult(bool sliced, object addData)
		{
			this.sliced = sliced;
			this.addData = addData;
		}

		public BzMeshSliceResult[] meshItems;
		public readonly bool sliced;
		public readonly object addData;
		public GameObject outObjectNeg;
		public GameObject outObjectPos;
	}

	public class BzMeshSliceResult
	{
		public BzSliceEdgeResult[] sliceEdgesNeg;
		public Renderer rendererNeg;

		public BzSliceEdgeResult[] sliceEdgesPos;
		public Renderer rendererPos;
	}

	public class BzSliceEdgeResult
	{
		public PolyMeshData capsData;
	}
}