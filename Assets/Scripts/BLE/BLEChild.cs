using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using static BLEManager;

public class bleChild : MonoBehaviour
{
    public int side = 0;
    [SerializeField] private string DeviceName = "Kinis 003";

    [SerializeField] private string ServiceUUID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    [SerializeField] private string Characteristic = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

    private string[] STATUS = {"None", "Scanning", "Connecting", "Connected", "Disconnecting", "Disconnected" };
    private string[] CHARGER_STATE = { "No Charger", "Charging", "Charge Termination" };
    private string[] INSOLE_STATE = { "STOPPING", "WALKING", "RUNNING" };

    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI status;

    [SerializeField] private GameObject icon;

    [SerializeField] private TextMeshProUGUI batteryField;
    [SerializeField] private TextMeshProUGUI chargerField;
    [SerializeField] private TextMeshProUGUI IMUField;
    [SerializeField] private TextMeshProUGUI temperatureField;
    [SerializeField] private TextMeshProUGUI uwbField;
    [SerializeField] private TextMeshProUGUI pressureField;
    [SerializeField] private TextMeshProUGUI stateAndSpeedField;
    [SerializeField] private TextMeshProUGUI stepEventField;
    [SerializeField] private TextMeshProUGUI stepWidthField;
    [SerializeField] private TextMeshProUGUI stepAngleField;
    [SerializeField] private TextMeshProUGUI footClearanceField;

    [SerializeField] private BLEManager bleManager;
    //private GameObject body;

    private string id;
    private BLEManager.Data data;
    public bool isDebug = true;
    public BatteryCheck battery;

    void setStatusText(int _status)
    {
        if (status == null) return;
        if (_status == 0) return;
        status.text = STATUS[_status];
    }

    void toggleIcon(bool isIcon)
    {
        if (icon == null) return;
        icon.SetActive(isIcon);
    }

    // Use this for initialization
    void Start()
    {
        if (bleManager == null)
        {
            bleManager = gameObject.GetComponentInParent<BLEManager>();
        }
        id = bleManager.AddNewDevice(DeviceName, ServiceUUID, Characteristic, side);
        bleManager.startProccess(id);
        Name.text = DeviceName;
        toggleIcon(true);
        

    }

    private void Update()
    {
        States currentState = bleManager.getStates(id);
        setStatusText((int)currentState);
        switch (currentState)
        {
            case States.None:
                break;

            case States.Scan:
                bleManager.PerformBluetoothScan();
                break;

            case States.Connect:
                bleManager.PerformConnection(id);
                break;
            case States.Subscribe:
                bleManager.PerformSubscription(id);
                break;

            case States.Unsubscribe:
                bleManager.PerformUnsubscription(id);
                break;

            case States.Disconnect:
                bleManager.PerformDisconnection(id);
                break;
        }
        ShowData();
    }

    // Update is called once per frame
    void ShowData()
    {

        if (bleManager.isConnected(id) || bleManager.isReplay)
        {
            toggleIcon(false);

            data = bleManager.getJsonData(id);
            bleManager.OnReceivePressure(side, data.pressureMappingValue);
            battery.setBattery(data.batteryValue);
            
            if (isDebug)
            {
                batteryField.text = "Battery: " + data.batteryValue.ToString("D3") + "%";
                chargerField.text = "Charger State: " + CHARGER_STATE[data.chargerValue];
                IMUField.text = "Gyro: " + data.gyroValue[0].ToString("000.0") + ", " + data.gyroValue[1].ToString("000.0") + ", " + data.gyroValue[2].ToString("000.0")
                                                            + "\nAccel: " + data.accelValue[0].ToString("000.0") + ", " + data.accelValue[1].ToString("000.0") + ", " + data.accelValue[2].ToString("000.0");
                temperatureField.text = "Temperature: " + data.temperatureValue.ToString("F2") + " celcius";
                uwbField.text = "Ranging (uwb): " + data.uwbValue.ToString("F2") + " m";
                // Convert float array to string array
                string[] stringValues = Array.ConvertAll(data.pressureMappingValue, x => x.ToString("F2"));

                // Join the string array with commas and new lines
                string pressureMappingString = string.Join(",\n", stringValues);

                pressureField.text = "Pressure (Pa): \n" + pressureMappingString;
                stateAndSpeedField.text = "State: " + INSOLE_STATE[data.stateInsoleValue]
                                                            + "\nSpeed: " + data.speedValue.ToString("F2") + " m/s";
                stepEventField.text = "Step Length: " + data.stepLengthValue.ToString("F2") + " m"
                                                            + "\nStride Length: " + data.strideLengthValue.ToString("F2") + " m";
                stepWidthField.text = "Angle: " + data.stepWidthValue.ToString("F2") + " m";
                stepAngleField.text = "Angle: " + data.stepAngleValue.ToString("F2") + " radian";
                footClearanceField.text = "Foot Height: " + data.footClearanceValue.ToString("F2") + " radian";
            }
        }
        else
        {
            toggleIcon(true);
        }
    }

   
}
