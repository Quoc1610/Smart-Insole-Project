using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.InteropServices;

public class bleMain : MonoBehaviour
{
    [SerializeField] private string DeviceName = "Kinis 003";
    //private string ServiceUUID = "6e400001"; // 6e400001-b5a3-f393-e0a9-e50e24dcca9e
    //private string Characteristic = "6e400003";

    [SerializeField] private string ServiceUUID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    [SerializeField] private string Characteristic = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";
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

    public Data dataJson = new Data
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

    enum States
    {
        None,
        Scan,
        Connect,
        RequestMTU,
        Subscribe,
        Unsubscribe,
        Disconnect,
        Communication,
    }

    private string[] CHARGER_STATE = { "No Charger", "Charging", "Charge Termination" };
    private string[] INSOLE_STATE = { "STOPPING", "WALKING", "RUNNING" };

    private bool _workingFoundDevice = true;
    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private bool _foundID = false;
    private string _deviceAddress;

    [SerializeField] private TextMeshProUGUI Name;
    [SerializeField] private TextMeshProUGUI status;

    [SerializeField] private GameObject icon;
    private bool isIcon = false;


    [SerializeField] private TextMeshProUGUI field;
    [SerializeField] private TextMeshProUGUI box;

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
    void Reset()
    {
        _workingFoundDevice = false;    // used to guard against trying to connect to a second device while still connecting to the first
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _foundID = false;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    void setFieldText(string text)
    {
        if (field == null) return;
        field.text = text;
    }

    void setStatusText(string text)
    {
        if (status == null) return;
        status.text = text;
    }

    void toggleIcon()
    {
        if (icon == null) return;
        isIcon = !isIcon;
        icon.SetActive(isIcon);
    }

    public void StartProcess()
    {
        //Name.text = DeviceName;
        setFieldText("Initializing...");
        //setStatusText("Initializing");
        //Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);
            setFieldText("Initializing...");
            //setStatusText("Initializing");

        }, (error) => {

            BluetoothLEHardwareInterface.Log("Error: " + error);
        });
    }

    // Use this for initialization
    void Start()
    {
        StartProcess();
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                switch (_state)
                {
                    case States.None:
                        break;

                    case States.Scan:
                        setFieldText("Scanning for ESP32 devices...");
                        //setStatusText("Scanning");

                        BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) => {

                            // we only want to look at devices that have the name we are looking for
                            // this is the best way to filter out devices
                            if (name.Contains(DeviceName))
                            {
                                _workingFoundDevice = true;

                                // it is always a good idea to stop scanning while you connect to a device
                                // and get things set up
                                BluetoothLEHardwareInterface.StopScan();

                                // add it to the list and set to connect to it
                                _deviceAddress = address;

                                SetState(States.Connect, 0.5f);

                                _workingFoundDevice = false;
                            }

                        }, null, false, false);
                        break;

                    case States.Connect:
                        // set these flags
                        _foundID = false;
                        setFieldText("Connecting to esp32");
                        //setStatusText("Connecting");

                        // note that the first parameter is the address, not the name. I have not fixed this because
                        // of backwards compatiblity.
                        // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                        // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                        // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                        BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null, (address, serviceUUID, characteristicUUID) => {

                            if (IsEqual(serviceUUID, ServiceUUID))
                            {
                                // if we have found the characteristic that we are waiting for
                                // set the state. make sure there is enough timeout that if the
                                // device is still enumerating other characteristics it finishes
                                // before we try to subscribe
                                if (IsEqual(characteristicUUID, Characteristic))
                                {
                                    _connected = true;
                                    SetState(States.Subscribe, 2f);

                                    setFieldText("Connected to ESP32");
                                    //setStatusText("Connected");
                                    toggleIcon();
                                }
                            }
                        }, (disconnectedAddress) => {
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            setFieldText("Disconnected");
                            //setStatusText("Disconnected");
                            toggleIcon();
                        });
                        break;

                    case States.Subscribe:
                        setFieldText("Subscribing to ESP32");

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, ServiceUUID, Characteristic, null, (address, characteristicUUID, bytes) => {
                            string hexString = BitConverter.ToString(bytes).Replace("-", ""); // Convert bytes to hex string
                            //setFieldText("byte length: " + bytes.Length + ", Data: 0x" + hexString);
                            //box.text += "\nbyte length: " + bytes.Length + ", Data: 0x" + hexString;

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
                                    batteryField.text = "Battery: " + batteryValue.ToString("D3") + "%";
                                    dataJson.batteryValue = batteryValue;
                                    break;
                                // Charger
                                case 0x31:
                                    byte charger = bytes[3];
                                    int chargerValue = charger;
                                    chargerField.text = "Charger State: " + CHARGER_STATE[chargerValue];
                                    dataJson.chargerValue = chargerValue;
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
                                    IMUField.text = "Gyro: " + gyroValue[0].ToString("000.0") + ", " + gyroValue[1].ToString("000.0") + ", " + gyroValue[2].ToString("000.0")
                                                        + "\nAccel: " + accelValue[0].ToString("000.0") + ", " + accelValue[1].ToString("000.0") + ", " + accelValue[2].ToString("000.0")
                                                        + "\nTemperature(IMU): " + temperatureIMUValue.ToString("F2") + " celcius";
                                    dataJson.gyroValue = gyroValue;
                                    dataJson.accelValue = accelValue;
                                    break;
                                // Temperature
                                case 0x34:
                                    byte[] temperature = new byte[2];
                                    Array.Copy(bytes, 3, temperature, 0, 2);
                                    Array.Reverse(temperature);
                                    //Debug.Log("OK");
                                    float temperatureValue = (float)(BitConverter.ToUInt16(temperature, 0) * 0.01);
                                    //Debug.Log("OK1");
                                    temperatureField.text = "Temperature: " + temperatureValue.ToString("F2") + " celcius";
                                    dataJson.temperatureValue = temperatureValue;
                                    break;
                                // UWB
                                case 0x35:
                                    byte[] uwb = new byte[4];
                                    Array.Copy(bytes, 3, uwb, 0, 4);
                                    Array.Reverse(uwb);
                                    float uwbValue = (float)(BitConverter.ToSingle(uwb, 0));
                                    uwbField.text = "Ranging (uwb): " + uwbValue.ToString("F2") + " m";
                                    dataJson.uwbValue = uwbValue;
                                    break;
                                // Pressure Mapping
                                case 0x36:
                                    setFieldText("Pressure: byte length: " + bytes.Length + ", Data: 0x" + hexString);
                                    box.text += "\nPressure: byte length: " + bytes.Length + ", Data: 0x" + hexString;
                                    byte[][] pressureMapping = new byte[4][];
                                    float[] pressureMappingValue = new float[4];
                                    for (int i = 0; i < 4; i++)
                                    {
                                        pressureMapping[i] = new byte[4];
                                        Array.Copy(bytes, 3 + (i * 4), pressureMapping[i], 0, 4);
                                        Array.Reverse(pressureMapping[i]);
                                        pressureMappingValue[i] = (float)(BitConverter.ToUInt32(pressureMapping[i], 0) * 0.01 * 0.001);
                                    }
                                    // Convert float array to string array
                                    string[] stringValues = Array.ConvertAll(pressureMappingValue, x => x.ToString());

                                    // Join the string array with commas and new lines
                                    string pressureMappingString = string.Join(",\n", stringValues);

                                    pressureField.text = "Pressure (kPa): \n"+pressureMappingString;
                                    dataJson.pressureMappingValue = pressureMappingValue;
                                    break;
                                // State and Speed
                                case 0x50:
                                    byte stateInsole = bytes[3];
                                    int stateInsoleValue = stateInsole;
                                    byte[] speed = new byte[4];
                                    Array.Copy(bytes, 4, speed, 0, 4);
                                    Array.Reverse(speed);
                                    float speedValue = (float)BitConverter.ToSingle(speed, 0);
                                    stateAndSpeedField.text = "State: " + INSOLE_STATE[stateInsoleValue]
                                                        + "\nSpeed: " + speedValue.ToString("F2") + " m/s";
                                    dataJson.stateInsoleValue = stateInsoleValue;
                                    dataJson.speedValue = speedValue;
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
                                    stepEventField.text = "Step Length: " + stepLengthValue.ToString("F2") + " m"
                                                        + "\nStride Length: " + strideLengthValue.ToString("F2") + " m";
                                    dataJson.stepLengthValue = stepLengthValue;
                                    dataJson.strideLengthValue = strideLengthValue;
                                    break;
                                // Step Width
                                case 0x52:
                                    byte[] stepWidth = new byte[4];
                                    Array.Copy(bytes, 3, stepWidth, 0, 4);
                                    Array.Reverse(stepWidth);
                                    float stepWidthValue = (float)(BitConverter.ToSingle(stepWidth, 0));
                                    stepWidthField.text = "Angle: " + stepWidthValue.ToString("F2") + " m";
                                    dataJson.stepWidthValue = stepWidthValue;
                                    break;
                                // Step Angle
                                case 0x53:
                                    byte[] stepAngle = new byte[4];
                                    Array.Copy(bytes, 3, stepAngle, 0, 4);
                                    Array.Reverse(stepAngle);
                                    float stepAngleValue = (float)(BitConverter.ToSingle(stepAngle, 0));
                                    stepAngleField.text = "Angle: " + stepAngleValue.ToString("F2") + " radian";
                                    dataJson.stepAngleValue = stepAngleValue;
                                    break;
                                // Foot Clearance
                                case 0x54:
                                    byte[] footClearance = new byte[4];
                                    Array.Copy(bytes, 3, footClearance, 0, 4);
                                    Array.Reverse(footClearance);
                                    float footClearanceValue = (float)(BitConverter.ToSingle(footClearance, 0));
                                    footClearanceField.text = "Foot Height: " + footClearanceValue.ToString("F2") + " radian";
                                    dataJson.footClearanceValue = footClearanceValue;
                                    break;
                                default:
                                    setFieldText("Unkown Protocol: " + (protocolID).ToString());
                                    box.text += "\nbyte length: " + bytes.Length + ", Data: 0x" + hexString;
                                    break;
                            }
                            // Creating a JSON object with the extracted fields
                            string jsonData = JsonConvert.SerializeObject(dataJson);
                        });

                        // set to the none state and the user can start sending and receiving data
                        _state = States.None;
                        setFieldText("Waiting...");
                        break;

                    case States.Unsubscribe:
                        BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID, Characteristic, null);
                        SetState(States.Disconnect, 4f);
                        break;

                    case States.Disconnect:
                        if (_connected)
                        {
                            BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) => {
                                BluetoothLEHardwareInterface.DeInitialize(() => {

                                    _connected = false;
                                    _state = States.None;
                                });
                            });
                        }
                        else
                        {
                            BluetoothLEHardwareInterface.DeInitialize(() => {

                                _state = States.None;
                            });
                        }
                        break;
                }
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
