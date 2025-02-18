using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.InteropServices;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputTrackingAggregator;
using System.Threading;
using UnityEngine.UIElements;
using FIMSpace.RagdollAnimatorDemo;

public class BLEManager : MonoBehaviour
{
    private List<DeviceHandle> deviceList = new List<DeviceHandle>();
    Queue<int> deviceQueue = new Queue<int>();
    bool isScanning = false;
    bool isConnecting = false;
    float timeout = 0f;

    private GameObject body;
    public class DeviceHandle
    {
        public string DeviceName;
        public string ServiceUUID;
        public string Characteristic;

        public Data dataJson;

        public States _state = States.None;
        public bool _workingFoundDevice = true;
        public bool _connected = false;
        public bool _foundID = false;
        public string _deviceAddress = "";


        public DeviceHandle(string _name, string _uuid, string _characteristic)
        {
            DeviceName = _name;
            ServiceUUID = _uuid;
            Characteristic = _characteristic;

            dataJson = new Data
            {
                batteryValue = 0,
                chargerValue = 0,
                gyroValue = null,
                accelValue = null,
                temperatureValue = 0,
                uwbValue = 0,
                pressureMappingValue = null,
                stateInsoleValue = 0,
                speedValue = 0,
                stepLengthValue = 0,
                strideLengthValue = 0,
                footClearanceValue = 0
            };
        }

        public void Reset()
        {
            _workingFoundDevice = false;    // used to guard against trying to connect to a second device while still connecting to the first
            _connected = false;
            _state = States.None;
            _foundID = false;
        }

    }

    public class Data
    {
        public int batteryValue;
        public int chargerValue;
        public float[] gyroValue;
        public float[] accelValue;
        public float temperatureValue;
        public float uwbValue;
        public float[] pressureMappingValue;
        public int stateInsoleValue;
        public float speedValue;
        public float stepLengthValue;
        public float strideLengthValue;
        public float stepWidthValue;
        public float stepAngleValue;
        public float footClearanceValue;
    }

    public enum States
    {
        None,
        Scan,
        Connect,
        Subscribe,
        Unsubscribe,
        Disconnect
    }
    [SerializeField] private TextMeshProUGUI field;
    void setFieldText(string text)
    {
        if (field == null) return;
        field.text = text;
    }


    public int AddNewDevice(string _name,string _uuid, string _characteristic)
    {
        DeviceHandle item = new DeviceHandle(_name, _uuid, _characteristic);
        deviceList.Add(item);
        return deviceList.Count - 1;
    }

    void SetState(int id, States state)
    {
        deviceList[id]._state = state;
    }

    public void startProccess(int id)
    {
        deviceList[id].Reset();
        SetState(id, States.Scan);
    }

    public void stopProccess(int id)
    {
        SetState(id,States.Unsubscribe);
    }

    public Data getJsonData(int id)
    {
        return deviceList[id].dataJson;
    }

    public int getStates(int id)
    {
        return (int)deviceList[id]._state;
    }

    public bool isConnected(int id)
    {
        return deviceList[id]._connected;
    }


    // Use this for initialization
    void Start()
    {
        BluetoothLEHardwareInterface.Initialize(true, false, () => {
            Debug.Log("Initialize");
            setFieldText("Initializing...");

        }, (error) => {
            Debug.Log("Fail to Initialize");
            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    private int currentIndex = 0;

    void increaseIndex()
    {
        currentIndex++;
        if (currentIndex >= deviceList.Count) currentIndex = 0;
    }

    float averageSpeed()
    {
        float average = 0f;
        for (int i = 0;i< deviceList.Count; i++)
        {
            average += deviceList[i].dataJson.speedValue;
        }
        return average;
    }

    float averageRotation()
    {
        float average = 0f;
        for (int i = 0; i < deviceList.Count; i++)
        {
            average += deviceList[i].dataJson.gyroValue[2] * -1;
        }
        return average / deviceList.Count;
    }

    bool isReady()
    {
        bool result = true;
        for (int i = 0;i < deviceList.Count;i++)
        {
            if (!deviceList[i]._connected) result = false;
        }
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (body == null)
        {
            body = GameObject.FindGameObjectWithTag("Model");
        }
        if (timeout > 0f)
        {
            timeout -= Time.deltaTime;
            return;
        }
        timeout = 1f;
        switch (deviceList[currentIndex]._state)
        {
            case States.None:
                break;

            case States.Scan:
                setFieldText("Scanning for " + deviceList[currentIndex].DeviceName);
                if (!isScanning) PerformBluetoothScan(currentIndex);
                break;
                
            case States.Connect:
                setFieldText("Connecting to "+ deviceList[currentIndex].DeviceName);
                // set these flags
                deviceList[currentIndex]._foundID = false;
                if (!isConnecting) PerformConnection(currentIndex);
                break;
            case States.Subscribe:
                setFieldText("Subscribing to " + deviceList[currentIndex].DeviceName);
                PerformSubscription(currentIndex);
                // set to the none state and the user can start sending and receiving data
                SetState(currentIndex, States.None);
                // setFieldText("Waiting...");
                break;

            case States.Unsubscribe:
                BluetoothLEHardwareInterface.UnSubscribeCharacteristic(deviceList[currentIndex]._deviceAddress, deviceList[currentIndex].ServiceUUID, deviceList[currentIndex].Characteristic, null);
                SetState(currentIndex,States.Disconnect);
                break;

            case States.Disconnect:
                if (deviceList[currentIndex]._connected)
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral(deviceList[currentIndex]._deviceAddress, (address) => {
                    }); 
                }
                deviceList[currentIndex]._connected = false;
                // SetState(currentIndex, States.None);
                startProccess(currentIndex);
                break;
        }
        if (body)
        {
            FBasic_RigidbodyMover fb = body.GetComponent<FBasic_RigidbodyMover>();
            if (fb != null && isReady())
            {
                fb.setSpeed(averageSpeed());
                fb.changeRotation(averageRotation());
            }
        }
        increaseIndex();
    }

    public void FullScan()
    {
        for (int i = 0; i < deviceList.Count;i++)
        {
            if (!deviceList[i]._connected) startProccess(i);
        }
    }    


    public void PerformBluetoothScan(int index)
    {
        isScanning = true;
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {
            // Check if the scanned device matches the device in the list
            // Inside the scan callback function
            if (name.Contains(deviceList[index].DeviceName))
            {
                deviceList[index]._workingFoundDevice = true;

                // Update device address and set state to connect
                deviceList[index]._deviceAddress = address;
                SetState(index,States.Connect);
                Debug.Log("Found " + deviceList[index].DeviceName);
                setFieldText("Found " + deviceList[index].DeviceName);

                isScanning = false;
                BluetoothLEHardwareInterface.StopScan();
            }

        }, null, false, false);
    }
    void PerformConnection(int index)
    {
        isConnecting = true;
        Debug.Log("Connected test 1: " + index.ToString());
        // note that the first parameter is the address, not the name. I have not fixed this because
        // of backwards compatiblity.
        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
        BluetoothLEHardwareInterface.ConnectToPeripheral(deviceList[index]._deviceAddress, null, null, (address, serviceUUID, characteristicUUID) => {
            Debug.Log("Connected test 2: " + index.ToString());
            if (IsEqual(serviceUUID, deviceList[index].ServiceUUID))
            {
                // if we have found the characteristic that we are waiting for
                // set the state. make sure there is enough timeout that if the
                // device is still enumerating other characteristics it finishes
                // before we try to subscribe
                if (IsEqual(characteristicUUID, deviceList[index].Characteristic))
                {
                    deviceList[index]._connected = true;
                    deviceList[index]._foundID = true;
                    setFieldText("Connected to " + deviceList[index].DeviceName);
                    Debug.Log("Connected to: " + deviceList[index].DeviceName);
                    isConnecting = false;
                    SetState(index,States.Subscribe);
                }
            }
        }, (disconnectedAddress) => {
            for (int i = 0;i<deviceList.Count;i++)
            {
                if (disconnectedAddress == deviceList[i]._deviceAddress)
                {
                    deviceList[index]._connected = false;
                    setFieldText("Disconnected: " + deviceList[i].DeviceName);
                    SetState(index, States.Disconnect);
                    break;
                }
            }
            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
        });
    }

    void PerformSubscription(int index)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceList[index]._deviceAddress, deviceList[index].ServiceUUID, deviceList[index].Characteristic, null, (address, characteristicUUID, bytes) => {
            string hexString = BitConverter.ToString(bytes).Replace("-", ""); // Convert bytes to hex string

            // Extracting individual fields from the byte array
            byte startByte = bytes[0];
            byte protocolID = bytes[1];
            byte dataLen = bytes[2];
            byte[] checkSum = new byte[2];
            Array.Copy(bytes, bytes.Length - 3, checkSum, 0, 2);
            byte stopByte = bytes[bytes.Length - 1];

            switch (protocolID)
            {
                // Battery
                case 0x30:
                    byte[] battery = new byte[2];
                    Array.Copy(bytes, 3, battery, 0, 2);
                    Array.Reverse(battery);
                    int batteryValue = BitConverter.ToUInt16(battery, 0);
                    deviceList[index].dataJson.batteryValue = batteryValue;
                    break;
                // Charger
                case 0x31:
                    byte charger = bytes[3];
                    int chargerValue = charger;
                    deviceList[index].dataJson.chargerValue = chargerValue;
                    break;
                // IMU
                case 0x32:
                    byte[][] gyro = new byte[3][];
                    float[] gyroValue = new float[3];
                    for (int i = 0; i < 3; i++)
                    {
                        gyro[i] = new byte[4];
                        Array.Copy(bytes, 3 + (i * 4), gyro[i], 0, 4);
                        Array.Reverse(gyro[i]);
                        gyroValue[i] = (float)BitConverter.ToSingle(gyro[i], 0);
                    }
                    byte[][] accel = new byte[3][];
                    float[] accelValue = new float[3];
                    for (int i = 0; i < 3; i++)
                    {
                        accel[i] = new byte[4];
                        Array.Copy(bytes, 15 + (i * 4), accel[i], 0, 4);
                        Array.Reverse(accel[i]);
                        accelValue[i] = (float)BitConverter.ToSingle(accel[i], 0);
                    }
                    byte[] temperatureIMU = new byte[4];
                    Array.Copy(bytes, 27, temperatureIMU, 0, 4);
                    Array.Reverse(temperatureIMU);
                    float temperatureIMUValue = (float)BitConverter.ToSingle(temperatureIMU, 0);
                    deviceList[index].dataJson.gyroValue = gyroValue;
                    deviceList[index].dataJson.accelValue = accelValue;
                    break;
                // Temperature
                case 0x34:
                    byte[] temperature = new byte[2];
                    Array.Copy(bytes, 3, temperature, 0, 2);
                    Array.Reverse(temperature);
                    float temperatureValue = (float)(BitConverter.ToUInt16(temperature, 0) * 0.01);
                    deviceList[index].dataJson.temperatureValue = temperatureValue;
                    break;
                // UWB
                case 0x35:
                    byte[] uwb = new byte[4];
                    Array.Copy(bytes, 3, uwb, 0, 4);
                    Array.Reverse(uwb);
                    float uwbValue = (float)(BitConverter.ToSingle(uwb, 0));
                    deviceList[index].dataJson.uwbValue = uwbValue;
                    break;
                // Pressure Mapping
                case 0x36:
                    byte[][] pressureMapping = new byte[4][];
                    float[] pressureMappingValue = new float[4];
                    for (int i = 0; i < 4; i++)
                    {
                        pressureMapping[i] = new byte[4];
                        Array.Copy(bytes, 3 + (i * 4), pressureMapping[i], 0, 4);
                        Array.Reverse(pressureMapping[i]);
                        pressureMappingValue[i] = (float)(BitConverter.ToUInt32(pressureMapping[i], 0) * 0.01 * 0.001);
                    }

                    deviceList[index].dataJson.pressureMappingValue = pressureMappingValue;
                    break;
                // State and Speed
                case 0x50:
                    byte stateInsole = bytes[3];
                    int stateInsoleValue = stateInsole;
                    byte[] speed = new byte[4];
                    Array.Copy(bytes, 4, speed, 0, 4);
                    Array.Reverse(speed);
                    float speedValue = (float)BitConverter.ToSingle(speed, 0);
                    deviceList[index].dataJson.stateInsoleValue = stateInsoleValue;
                    deviceList[index].dataJson.speedValue = speedValue;
                    break;
                // Step Event
                case 0x51:
                    byte[] stepLength = new byte[4];
                    Array.Copy(bytes, 3, stepLength, 0, 4);
                    Array.Reverse(stepLength);
                    float stepLengthValue = (float)(BitConverter.ToSingle(stepLength, 0));
                    byte[] strideLength = new byte[4];
                    Array.Copy(bytes, 7, strideLength, 0, 4);
                    Array.Reverse(strideLength);
                    float strideLengthValue = (float)(BitConverter.ToSingle(strideLength, 0));
                    deviceList[index].dataJson.stepLengthValue = stepLengthValue;
                    deviceList[index].dataJson.strideLengthValue = strideLengthValue;
                    break;
                // Step Width
                case 0x52:
                    byte[] stepWidth = new byte[4];
                    Array.Copy(bytes, 3, stepWidth, 0, 4);
                    Array.Reverse(stepWidth);
                    float stepWidthValue = (float)(BitConverter.ToSingle(stepWidth, 0));
                    deviceList[index].dataJson.stepWidthValue = stepWidthValue;
                    break;
                // Step Angle
                case 0x53:
                    byte[] stepAngle = new byte[4];
                    Array.Copy(bytes, 3, stepAngle, 0, 4);
                    Array.Reverse(stepAngle);
                    float stepAngleValue = (float)(BitConverter.ToSingle(stepAngle, 0));
                    deviceList[index].dataJson.stepAngleValue = stepAngleValue;
                    break;
                // Foot Clearance
                case 0x54:
                    byte[] footClearance = new byte[4];
                    Array.Copy(bytes, 3, footClearance, 0, 4);
                    Array.Reverse(footClearance);
                    float footClearanceValue = (float)(BitConverter.ToSingle(footClearance, 0));
                    deviceList[index].dataJson.footClearanceValue = footClearanceValue;
                    break;
                default:
                    break;
            }
            // Creating a JSON object with the extracted fields
            // string jsonData = JsonConvert.SerializeObject(deviceList[index].dataJson);
        });
    }

    string FullUUID(string uuid)
    {
        return uuid + "-b5a3-f393-e0a9-e50e24dcca9e";
    }

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
    }
}
