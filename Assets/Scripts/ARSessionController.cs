using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ARSessionController : MonoBehaviour
{
    private ARSessionOrigin _arSessionOrigin;

    private Text _debugText;

    private void Awake()
    {
        _debugText = GetComponent<Text>();
        StartCoroutine(InitializeLocation());
    }

    // Start is called before the first frame update
    void Start()
    {
        _arSessionOrigin = GetComponent<ARSessionOrigin>();
        //transform.rotation = Quaternion.Euler(0f, -Input.compass.magneticHeading, 0f);
    }

    void Update()
    {
        if (Input.compass.enabled)
        {
            _debugText.text = (-Input.compass.magneticHeading).ToString();
        }
    }
    
    public IEnumerator InitializeLocation()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            _debugText.text = "location disabled by user";
        }
        // enable compass
        Input.compass.enabled = true;
        // start the location service
        _debugText.text = "start location service";
        Input.location.Start(10, 0.01f);
        // Wait until service initializes
        int maxSecondsToWaitForLocation = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxSecondsToWaitForLocation > 0)
        {
            yield return new WaitForSeconds(1);
            maxSecondsToWaitForLocation--;
        }
     
        // Service didn't initialize in 20 seconds
        if (maxSecondsToWaitForLocation < 1)
        {
            _debugText.text = "location service timeout";
            yield break;
        }
        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            _debugText.text = "unable to determine device location";
            yield break;
        }
        _debugText.text = "location service loaded";
    }
}
