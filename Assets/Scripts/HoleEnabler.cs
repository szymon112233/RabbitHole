using System;
using System.Collections.Generic;
using UnityEngine;


public class HoleEnabler : MonoBehaviour
{
    public List<GameObject> ToEnable;
    public List<GameObject> ToDisable;

    private void Awake()
    {
        gameObject.GetComponent<VehicleContoller>().OnVehicleEntered += MakeHole;
    }


    public void MakeHole()
    {
        foreach (var go in ToEnable)
        {
            go.SetActive(true);
        }
        foreach (var go in ToDisable)
        {
            go.SetActive(false);
        }
        
        gameObject.GetComponent<VehicleContoller>().OnVehicleEntered -= MakeHole;
        
        Destroy(this);
    }
}