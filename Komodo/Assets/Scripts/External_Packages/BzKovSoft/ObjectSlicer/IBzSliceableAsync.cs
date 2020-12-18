using System;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// Sliceable object
	/// </summary>
	public interface IBzSliceableAsync
	{
		/// <summary>
		/// Start slicing the object
		/// </summary>
		/// <param name="plane">Plane by which you are going to slice</param>
		/// <param name="callBack">Method that will be called when the slice will be done</param>
		void Slice(Plane plane, Action<BzSliceTryResult> callBack);
	}
}
