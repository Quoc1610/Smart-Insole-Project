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
using System.IO;
using Newtonsoft.Json.Linq;
using static SetPressure;
using static UnityEngine.Rendering.HableCurve;
using Unity.VisualScripting;
using UnityEngine.Android;

public class BLEManager : MonoBehaviour
{
    private List<DeviceHandle> deviceList = new List<DeviceHandle>();
    private Dictionary<string, Dictionary<string, Data>> jsonList = new Dictionary<string, Dictionary<string, Data>>(); // List to store JSON strings
    bool isScanning = false;
    bool isConnecting = false;
    float timeout = 0f;
    public bool isRecord = false;
    public bool isReplay = false;
    public GameObject recordButton;
    public GameObject replayButton;
    public List<Sprite> spriteList = new List<Sprite>();
    public SetPressure pressureHandle;
    public TrippingRiskCalculator trippingCalculator;
    public GameObject skeletonDisplay;
    public bool isSkeleton = false;
    //public UnityEngine.UI.Slider skeletonSlider;
    public TestRotation LeftObject;
    public TestRotation RightObject;

    private GameObject body;
    private float medium = 2250;
    private UserInstance userInstance;

    public TextMeshProUGUI speedText;
    public int GetDeviceCount()
    {
        return deviceList.Count;
    }

    public string GetDeviceName(int id)
    {
        return deviceList[id].DeviceName;
    }
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
                gyroValue = new float[3],
                accelValue = new float[3],
                temperatureValue = 0,
                uwbValue = 0,
                pressureMappingValue = new float[4],
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

    Dictionary<string, int> directionIdList = new Dictionary<string, int>();


    public int AddNewDevice(string _name,string _uuid, string _characteristic, int _side)
    {
        DeviceHandle item = new DeviceHandle(_name, _uuid, _characteristic);
        deviceList.Add(item);
        if (_side == 0)
        {
            directionIdList["Left"] = deviceList.Count - 1;
        }
        else if (_side == 1) directionIdList["Right"] = deviceList.Count - 1;
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

    private void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        BluetoothLEHardwareInterface.DisconnectAll();

        // Delay deinitialization slightly to allow disconnects to finish
        BluetoothLEHardwareInterface.DeInitialize(() =>
        {
            Debug.Log("Bluetooth LE DeInitialized.");
        });
#endif
    }
    void Start()
    {
        userInstance = GameObject.FindGameObjectWithTag("UserInstance").GetComponent<UserInstance>();
        checkRecordButton();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {
            Debug.Log("Initialize");
            setFieldText("Initializing...");

        }, (error) => {
            Debug.Log("Fail to Initialize");
            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    private int currentIndex = 0;
    private int frameIndex = 0;

    void increaseFrame()
    {
        frameIndex++;
        if (frameIndex >= jsonList.Count)
        {
            toggleReplay();
        }
    }

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
            if (deviceList[i]._connected == true) average += deviceList[i].dataJson.speedValue;
        }
        Debug.Log("Speed: " + average.ToString());
        return average;
    }

    float sumPressure()
    {
        float sum = 0f;
        for (int i = 0; i < deviceList.Count; i++)
        {
            if (deviceList[i]._connected == true)
            {
                for (int j = 0; i < deviceList[i].dataJson.pressureMappingValue.Length; j++)
                {
                    sum+= deviceList[i].dataJson.pressureMappingValue[j] * 0.01f;
                }
            }
        }
        return sum;
    }

    float averageRotation()
    {
        float average = 0f;
        int count = deviceList.Count;
        for (int i = 0; i < deviceList.Count; i++)
        {
            if (deviceList[i]._connected == true)
            {
                average += deviceList[i].dataJson.gyroValue[2] * -1;
            }
            else
                count--;
                
        }
        if (count == 0) return 0;
        Debug.Log("Rotation: " + (average / count).ToString());
        return average / count;
    }

    void UpdateRotation()
    {
        DeviceHandle myDeviceLeft = deviceList[directionIdList["Left"]];
        if (myDeviceLeft == null) return;
        Vector3 gyroLeft = new Vector3(-1 * myDeviceLeft.dataJson.gyroValue[0], -1 * myDeviceLeft.dataJson.gyroValue[2], -1* myDeviceLeft.dataJson.gyroValue[1]);
        Vector3 accelLeft = new Vector3(myDeviceLeft.dataJson.accelValue[0], myDeviceLeft.dataJson.accelValue[1], myDeviceLeft.dataJson.accelValue[2]);
        LeftObject.changeRotation(gyroLeft.x, gyroLeft.y, gyroLeft.z);
        LeftObject.ShowForce(accelLeft);

        DeviceHandle myDeviceRight = deviceList[directionIdList["Right"]];
        if (myDeviceRight == null) return;
        Vector3 gyroRight = new Vector3(-1 * myDeviceRight.dataJson.gyroValue[0], -1 * myDeviceRight.dataJson.gyroValue[2], -1 * myDeviceRight.dataJson.gyroValue[1]);
        Vector3 accelRight = new Vector3(myDeviceRight.dataJson.accelValue[0], myDeviceRight.dataJson.accelValue[1], myDeviceRight.dataJson.accelValue[2]);
        RightObject.changeRotation(gyroRight.x, gyroRight.y, gyroRight.z);
        RightObject.ShowForce(accelRight);


    }

    bool isReady()
    {
        bool result = true;
        for (int i = 0;i < deviceList.Count;i++)
        {
            if (!deviceList[i]._connected) result = false;
        }
        //return true;
        return result;
    }

    int convertPressure(float p)
    {
        float weight = userInstance.entity.weight;
        float result = (p / (float)((weight * 2.5 * 10) / 0.015)) * 100;
        return (int)Mathf.Round(result);
    }

    public void OnReceivePressure(int side, float[] pressureMaps)
    {
        UIManager._instance.uiPressure.ResetHeightGrid(side);
        if (side == 0)
        {
            UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(convertPressure(pressureMaps[3]), 0);
            UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(convertPressure(pressureMaps[0]), 0);
            UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(convertPressure(pressureMaps[2]), 0);
            UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(convertPressure(pressureMaps[1]), 0);
        }
        else
        {
            UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(convertPressure(pressureMaps[0]), 1);
            UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(convertPressure(pressureMaps[1]), 1);
            UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(convertPressure(pressureMaps[3]), 1);
            UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(convertPressure(pressureMaps[2]), 1);
        }
    }

    public void toggleModel()
    {
        isSkeleton = !isSkeleton;
        Debug.Log("Skeleton " +isSkeleton.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (body == null)
        {
            body = GameObject.FindGameObjectWithTag("Model");
        }
        if (body != null && !isSkeleton)
        {
            body.SetActive(true);
            skeletonDisplay.transform.parent = gameObject.transform;
            skeletonDisplay.transform.position = Vector3.zero;
            FBasic_RigidbodyMover fb = body.GetComponent<FBasic_RigidbodyMover>();
            if (fb != null && isReady())
            {
                fb.setSpeed(averageSpeed());
                fb.changeRotation(averageRotation());
            }
        }
        else if (isSkeleton)
        {
            body.SetActive(false);
            skeletonDisplay.transform.parent = body.transform.parent;
            skeletonDisplay.transform.localPosition = body.transform.localPosition;
            skeletonDisplay.transform.localScale = body.transform.localScale;
            skeletonDisplay.transform.localRotation = body.transform.localRotation;
        }
        if (isRecord)
        {
            AddJSONString(JsonExtract());
        }
        if (isReplay)
        {
            Dictionary<string, Data> jsonStr = jsonList[frameIndex.ToString()];
            for (int i = 0;i<deviceList.Count;i++)
            {
                deviceList[i].dataJson = jsonStr[deviceList[i].DeviceName];
            }
            increaseFrame();
        }
        UpdatePressure();
        //UpdateRotation();
        speedText.text = averageSpeed().ToString("F2") + " m/s";
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
        
        increaseIndex();
    }
    float[] offsetGyro(float[] data)
    {
        float[] result = new float[3];
        result[0] = data[0] * 16.4f;
        result[1] = data[2] * 16.4f;
        result[2] = data[1] * 16.4f;
        return result;
    }
    void UpdatePressure()
    {
        string[] dirList = new string[] { "Left", "Right" };
        Dictionary<string, int[]> pressureOrder = new Dictionary<string, int[]>();
        pressureOrder["Left"] = new int[] { 1, 0, 2, 3};
        pressureOrder["Right"] = new int[] { 2, 3, 1, 0 };

        SensorInfo sensorData = new SensorInfo();
        sensorData.pressureMapping = new Dictionary<string, float[]>();
        sensorData.gyro = new Dictionary<string, float[]>();
        sensorData.accel = new Dictionary<string, float[]>();
        sensorData.whs = new Dictionary<string, float>();
        sensorData.groundDetect = new Dictionary<string, bool>();
        foreach (string segmentName in dirList)
        {
            DeviceHandle myDevice = deviceList[directionIdList[segmentName]];
            float[] mapping = new float[myDevice.dataJson.pressureMappingValue.Length]; 

            for (int i = 0;i <mapping.Length;i++)
            {
                mapping[i] = myDevice.dataJson.pressureMappingValue[pressureOrder[segmentName][i]];
                mapping[i] *= 0.01f; // Convert pa to mbar: 1 mbar = 100 pa
            }
            sensorData.pressureMapping[segmentName] = mapping;

            //sensorData.gyro[segmentName] = offsetGyro(myDevice.dataJson.gyroValue);
            sensorData.gyro[segmentName] = myDevice.dataJson.gyroValue;
            sensorData.accel[segmentName] = myDevice.dataJson.accelValue;
            sensorData.groundDetect[segmentName] = true;
        }
        sensorData.whs["Weight"] = userInstance.entity.weight;
        sensorData.whs["Height"] = userInstance.entity.height;
        sensorData.whs["Sex"] = userInstance.entity.sex; 

        pressureHandle.setSensor(sensorData);
    }

    public void PredictTrippingRisk(bool isBalanced)
    {
        trippingCalculator.isBalanced = isBalanced;
        trippingCalculator.weight = userInstance.entity.weight;
        trippingCalculator.speed = averageSpeed();
        trippingCalculator.footPressure = sumPressure();
        Debug.Log("Run Predict Tripping Risk");
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
                    deviceList[i]._connected = false;
                    setFieldText("Disconnected: " + deviceList[i].DeviceName);
                    SetState(i, States.Disconnect);
                    break;
                }
            }
            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
        });
    }

    void PerformSubscription(int index)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceList[index]._deviceAddress, deviceList[index].ServiceUUID, deviceList[index].Characteristic, null, (address, characteristicUUID, bytes) => {
            if (isReplay) return;
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
                        pressureMappingValue[i] = ((float)(BitConverter.ToSingle(pressureMapping[i], 0) * 0.01) * medium) / 0.0006f;
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

    Dictionary<string, Data> JsonExtract()
    {
        // Create a dictionary to hold the merged data
        Dictionary<string, Data> mergedData = new Dictionary<string, Data>();

        // Merge the Data objects with index-based keys
        for (int i = 0; i < deviceList.Count; i++)
        {
            // Serialize the original dataJson object
            string jsonCopy = JsonConvert.SerializeObject(deviceList[i].dataJson);

            // Deserialize the serialized JSON to create a deep copy
            Data newDataJson = JsonConvert.DeserializeObject<Data>(jsonCopy);

            // Add the deep copy to the mergedData dictionary
            mergedData.Add(deviceList[i].DeviceName, newDataJson);
            //mergedData.Add(deviceList[i].DeviceName, deviceList[i].dataJson);
        }
        return mergedData;
    }

    // Add a JSON string to the list
    public void AddJSONString(Dictionary<string, Data> jsonString)
    {
        jsonList.Add(jsonList.Count.ToString(), jsonString);
        if (jsonList.Count > 100000)
        {
            Debug.Log("Over Limit: Stop Record");
            toggleRecord();
        }
    }

    // Save the list of JSON strings to a JSON file
    void SaveJSONListToFile()
    {
        // Serialize the list of JSON strings to a single JSON array string
        
        string jsonArrayString = JsonConvert.SerializeObject(jsonList, Formatting.Indented);

        // Get the path to the PersistentDataPath and create a file name
        string filePath = Path.Combine(Application.persistentDataPath, "jsondata.json");

        // Write the JSON array string to the file
        File.WriteAllText(filePath, jsonArrayString);

        Debug.Log("JSON data saved to file: " + filePath);
    }

    void ReadJsonFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "jsondata.json");
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            // Deserialize the JSON content into the desired data structure
            jsonList = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Data>>>(jsonContent);
            //Debug.Log("Read Count: "+ jsonList.Count.ToString());
            //Debug.Log("Read Value: " + string.Join(", ", jsonList["0"].Keys));
            // Now you can access the jsonData object which represents the nested dictionary structure
        }
        else
        {
            Debug.Log("File not found: " + filePath);
        }
    }


    public void toggleRecord()
    {
        isRecord = !isRecord;
        checkRecordButton();
        if (!isRecord)
        {
            AndroidPopupMessage.ShowPopupMessage("Stop Recording Data");
            SaveJSONListToFile();
            jsonList.Clear();
            replayButton.SetActive(true);
        }
        else
        {
            AndroidPopupMessage.ShowPopupMessage("Begin Recording Data");
            replayButton.SetActive(false);
        }
    }

    public void toggleReplay()
    {
        isReplay= !isReplay;
        checkReplayButton();
        if (!isReplay)
        {
            AndroidPopupMessage.ShowPopupMessage("Stop Replay");
            jsonList.Clear();
            frameIndex = 0;
            recordButton.SetActive(true);
        }
        else
        {
            AndroidPopupMessage.ShowPopupMessage("Begin Replay");
            ReadJsonFile();
            recordButton.SetActive(false);
        }
    }

    void checkRecordButton()
    {
        if (recordButton)
        {
            if (isRecord)
            {
                recordButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[1];
                recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
            }
            else
            {
                recordButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[0];
                recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Record";
            }
        }
    }

    void checkReplayButton()
    {
        if (replayButton)
        {
            if (isReplay)
            {
                replayButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[1];
                replayButton.GetComponentInChildren<TextMeshProUGUI>().text = "||";
            }
            else
            {
                replayButton.GetComponent<UnityEngine.UI.Image>().sprite = spriteList[2];
                replayButton.GetComponentInChildren<TextMeshProUGUI>().text = ">>";
            }
        }
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
