using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using Newtonsoft.Json;
using FIMSpace.RagdollAnimatorDemo;
using System.IO;
using static SetPressure;

public class BLEManager : MonoBehaviour
{
    private Dictionary<string, DeviceHandle> deviceList = new Dictionary<string, DeviceHandle>();
    private Dictionary<string, Dictionary<string, Data>> jsonList = new Dictionary<string, Dictionary<string, Data>>(); // List to store JSON strings
    bool isScanning = false;
    bool isConnecting = false;
    string connectingSide;

    string[] DIRECTIONS = { "Left", "Right" };

    public bool isRecord = false;
    public bool isReplay = false;

    public GameObject recordButton;
    public GameObject replayButton;

    public List<Sprite> spriteList = new List<Sprite>();

    public SetPressure pressureHandle;
    public TrippingRiskCalculator trippingCalculator;
    public GameObject skeletonDisplay;
    public bool isSkeleton = false;

    private GameObject body;
    private float medium = 2250;
    private UserInstance userInstance;

    public TextMeshProUGUI speedText;
    public int GetDeviceCount()
    {
        return deviceList.Count;
    }

    public string GetDeviceName(string side)
    {
        return deviceList[side].DeviceName;
    }
    public class DeviceHandle
    {
        public string DeviceName;
        public string ServiceUUID;
        public string Characteristic;

        public Data dataJson;

        public States _state = States.None;
        public bool _connected = false;
        public bool _isFound = false;
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
            _connected = false;
            _isFound = false;
            _state = States.None;
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


    public string AddNewDevice(string _name, string _uuid, string _characteristic, int _side)
    {
        DeviceHandle item = new DeviceHandle(_name, _uuid, _characteristic);
        if (_side == 0)
        {
            deviceList["Left"] = item;
            return "Left";
        }
        else
        {
            deviceList["Right"] = item;
            return "Right";
        }
    }

    public void startProccess(string side)
    {
        deviceList[side].Reset();
        deviceList[side]._state = States.Scan;
    }

    public Data getJsonData(string side)
    {
        return deviceList[side].dataJson;
    }

    public States getStates(string side)
    {
        return deviceList[side]._state;
    }

    public bool isConnected(string side)
    {
        return deviceList[side]._connected;
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

    float averageSpeed()
    {
        float average = 0f;
        foreach (string side in DIRECTIONS)
        {
            if (deviceList[side]._connected == true) average += deviceList[side].dataJson.speedValue;
        }
        return average;
    }

    float sumPressure()
    {
        float sum = 0f;
        foreach (string side in DIRECTIONS)
        {
            if (deviceList[side]._connected == true)
            {
                for (int i = 0; i < deviceList[side].dataJson.pressureMappingValue.Length; i++)
                {
                    sum += deviceList[side].dataJson.pressureMappingValue[i] * 0.01f;
                }
            }
        }
        return sum;
    }

    float averageRotation()
    {
        float average = 0f;
        int count = deviceList.Count;
        foreach (string side in DIRECTIONS)
        {
            if (deviceList[side]._connected == true)
            {
                average += deviceList[side].dataJson.gyroValue[2] * -1;
            }
            else
                count--;

        }
        if (count == 0) return 0;
        return average / count;
    }


    bool isReady()
    {
        foreach (string side in DIRECTIONS)
        {
            if (!deviceList[side]._connected) return false;
        }
        return true;
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
        Debug.Log("Skeleton " + isSkeleton.ToString());
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
            foreach (string side in DIRECTIONS)
            {
                deviceList[side].dataJson = jsonStr[deviceList[side].DeviceName];
            }
            increaseFrame();
        }
        UpdatePressure();
        speedText.text = averageSpeed().ToString("F2") + " m/s";
        CheckScanning();
    }
    float[] offsetGyro(float[] data)
    {
        float[] result = new float[3];
        result[0] = data[0] * 16.4f;
        result[1] = data[2] * 16.4f;
        result[2] = data[1] * 16.4f;
        return result;
    }

    Dictionary<string, int[]> pressureOrder = new Dictionary<string, int[]>
    {
        { "Left",  new int[] { 1, 0, 2, 3 } },
        { "Right", new int[] { 2, 3, 1, 0 } }
    };

    void UpdatePressure()
    {
        SensorInfo sensorData = new SensorInfo();
        sensorData.pressureMapping = new Dictionary<string, float[]>();
        sensorData.gyro = new Dictionary<string, float[]>();
        sensorData.accel = new Dictionary<string, float[]>();
        sensorData.whs = new Dictionary<string, float>();
        sensorData.groundDetect = new Dictionary<string, bool>();
        foreach (string segmentName in DIRECTIONS)
        {
            DeviceHandle myDevice = deviceList[segmentName];
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

    void CheckScanning()
    {
        foreach (string side in DIRECTIONS)
        {
            if (!deviceList[side]._isFound) return;
        }
        isScanning = false;
        BluetoothLEHardwareInterface.StopScan();
    }


    public void PerformBluetoothScan()
    {
        if (isScanning) return;
        isScanning = true;
        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {
            // Check if the scanned device matches the device in the list
            // Inside the scan callback function
            foreach (string side in DIRECTIONS)
            {
                if (!deviceList[side]._isFound)
                {
                    if (name.Contains(deviceList[side].DeviceName))
                    {
                        deviceList[side]._isFound = true;

                        // Update device address and set state to connect
                        deviceList[side]._deviceAddress = address;
                        deviceList[side]._state = States.Connect;
                        Debug.Log("Found " + deviceList[side].DeviceName);
                        setFieldText("Found " + deviceList[side].DeviceName);
                    }
                }
            }
            
        }, null, false, false);
    }

    string SearchForAddress(string address)
    {
        foreach (string side in DIRECTIONS)
        {
            if (address == deviceList[side]._deviceAddress)
            {
                return side;
            }
        }
        return null;
    }
    public void PerformConnection(string side)
    {
        if (isConnecting) return;
        isConnecting = true;
        connectingSide = side;
        BluetoothLEHardwareInterface.ConnectToPeripheral(deviceList[side]._deviceAddress, null, null, (address, serviceUUID, characteristicUUID) => {
            if (IsEqual(serviceUUID, deviceList[side].ServiceUUID))
            {
                if (IsEqual(characteristicUUID, deviceList[side].Characteristic))
                {
                    deviceList[connectingSide]._connected = true;
                    setFieldText("Connected to " + deviceList[connectingSide].DeviceName);
                    Debug.Log("Connected to: " + deviceList[connectingSide].DeviceName);
                    
                    deviceList[connectingSide]._state = States.Subscribe;
                    isConnecting = false;
                }
            }
        }, (disconnectedAddress) => {
            string Side = SearchForAddress(disconnectedAddress);
            if (Side == null) return;
            deviceList[Side]._connected = false;
            deviceList[Side]._state = States.Disconnect;
            setFieldText("Disconnected: " + deviceList[Side].DeviceName);
            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
        });
    }

    public void PerformSubscription(string side)
    {
        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(deviceList[side]._deviceAddress, deviceList[side].ServiceUUID, deviceList[side].Characteristic, null, (address, characteristicUUID, bytes) => {
            if (isReplay) return;
            string index = SearchForAddress(address);
            if (index == null) return;

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
        });
        deviceList[side]._state = States.None;
    }

    public void PerformUnsubscription(string side)
    {
        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(deviceList[side]._deviceAddress, deviceList[side].ServiceUUID, deviceList[side].Characteristic, null);
        deviceList[side]._state = States.Disconnect;
    }

    public void PerformDisconnection(string side)
    {
        if (deviceList[side]._connected)
        {
            BluetoothLEHardwareInterface.DisconnectPeripheral(deviceList[side]._deviceAddress, (address) => {
            });
        }
        deviceList[side]._connected = false;
        deviceList[side]._isFound = false;
        startProccess(side);
    }

    Dictionary<string, Data> JsonExtract()
    {
        // Create a dictionary to hold the merged data
        Dictionary<string, Data> mergedData = new Dictionary<string, Data>();

        // Merge the Data objects with index-based keys
        foreach (string side in DIRECTIONS)
        {
            // Serialize the original dataJson object
            string jsonCopy = JsonConvert.SerializeObject(deviceList[side].dataJson);

            // Deserialize the serialized JSON to create a deep copy
            Data newDataJson = JsonConvert.DeserializeObject<Data>(jsonCopy);

            // Add the deep copy to the mergedData dictionary
            mergedData.Add(deviceList[side].DeviceName, newDataJson);
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
