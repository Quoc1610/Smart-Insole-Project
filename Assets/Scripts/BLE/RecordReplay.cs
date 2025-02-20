using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using System.IO;
using TMPro;
using UnityEngine.UIElements;
using System;

public class RecordReplay : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private BLEManager bleManager;

    void Start()
    {
        if (bleManager == null)
        {
            bleManager = gameObject.GetComponentInParent<BLEManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (bleManager.isRecord)
        {
            bleManager.AddJSONString(JsonExtract());
        }
    }
    Dictionary<string, BLEManager.Data> JsonExtract()
    {
        // Create a dictionary to hold the merged data
        Dictionary<string, BLEManager.Data> mergedData = new Dictionary<string, BLEManager.Data>();

        // Merge the Data objects with index-based keys
        for (int i = 0; i < bleManager.GetDeviceCount(); i++)
        {
            BLEManager.Data data = bleManager.getJsonData(i);
            string[] stringValues = Array.ConvertAll(data.pressureMappingValue, x => x.ToString());

            // Join the string array with commas and new lines
            string pressureMappingString = string.Join(",", stringValues);
            Debug.Log("Collected - "+ bleManager.GetDeviceName(i) + ": "+pressureMappingString);
            mergedData.Add(bleManager.GetDeviceName(i), data);
        }
        
        return mergedData;
    }
}
