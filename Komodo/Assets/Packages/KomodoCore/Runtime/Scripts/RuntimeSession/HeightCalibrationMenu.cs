using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

/// <summary>
/// This class uses most of the methods from <c>HeightCalibration.cs</c>, and these methods are used for implementing the height calibration menu in the Komodo UI menu.
/// </summary>
public class HeightCalibrationMenu : MonoBehaviour
{
    /// <summary>
    /// A <c>HeightCalibration</c> type object.
    /// </summary>
    public HeightCalibration heightCalibration;

    /// <summary>
    /// Check if the public variable <c>heightCalibration</c> is null or not.
    /// </summary>
    /// <exception cref="System.Exception">throw an exception if heightCalibration is null.</exception>
    public void Start ()
    {
        if (!heightCalibration) 
        {
            throw new System.Exception("You must set a heightCalibration object in HeightCalibrationMenu");
        }
    }

    /// <summary>
    /// Start calibration.
    /// </summary>
    public void StartCalibration () 
    {
        heightCalibration.StartCalibration();
    }

    /// <summary>
    /// End calibration.
    /// </summary>
    public void EndCalibration () 
    {
        heightCalibration.EndCalibration();
    }

    /// <summary>
    /// Bump height up with a small default value.
    /// </summary>
    public void BumpHeightUpSmall () 
    {
        heightCalibration.BumpHeightUpSmall();
    }

    /// <summary>
    /// Bump height down with a small default value.
    /// </summary>
    public void BumpHeightDownSmall () 
    {
        heightCalibration.BumpHeightDownSmall();
    }

    /// <summary>
    /// Bump height up with a larger default value.
    /// </summary>
    public void BumpHeightUpLarge () 
    {
        heightCalibration.BumpHeightUpLarge();
    }

    /// <summary>
    /// Bump height down with a larger default value.
    /// </summary>
    public void BumpHeightDownLarge () 
    {
        heightCalibration.BumpHeightDownLarge();
    }
}
