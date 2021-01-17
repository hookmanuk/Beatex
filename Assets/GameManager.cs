using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

public class GameManager : MonoBehaviour
{
    public float RefreshRate { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (XRDevice.refreshRate != RefreshRate)
        {
            RefreshRate = XRDevice.refreshRate;
            Time.fixedDeltaTime = (float)Math.Round(1 / XRDevice.refreshRate, 8);
        }
    }
}
