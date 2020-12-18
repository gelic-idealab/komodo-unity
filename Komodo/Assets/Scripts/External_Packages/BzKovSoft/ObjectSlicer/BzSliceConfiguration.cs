using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BzKovSoft.ObjectSlicer
{
	/// <summary>
	/// This component can be added to object with mesh renderer to configure its behaviour
	/// </summary>
	[DisallowMultipleComponent]
	public class BzSliceConfiguration : MonoBehaviour
	{
#pragma warning disable 0649
		public SliceType SliceType;
		public Material SliceMaterial;
#pragma warning restore 0649
	}

	public enum SliceType
	{
		Slice,
		Duplicate,
		KeepOne,
	}
}
