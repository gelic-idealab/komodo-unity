using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;

public class HeightCalibrationMenu : MonoBehaviour
{
    public HeightCalibration heightCalibration;
    public void Start ()
    {
        if (!heightCalibration) 
        {
            throw new System.Exception("You must set a heightCalibration object in HeightCalibrationMenu");
        }
    }

    public void StartCalibration () 
    {
        heightCalibration.StartCalibration();
    }

    public void EndCalibration () 
    {
        heightCalibration.EndCalibration();
    }

    public void BumpHeightUpSmall () 
    {
        heightCalibration.BumpHeightUpSmall();
    }

    public void BumpHeightDownSmall () 
    {
        heightCalibration.BumpHeightDownSmall();
    }

    public void BumpHeightUpLarge () 
    {
        heightCalibration.BumpHeightUpLarge();
    }

    public void BumpHeightDownLarge () 
    {
        heightCalibration.BumpHeightDownLarge();
    }
}
