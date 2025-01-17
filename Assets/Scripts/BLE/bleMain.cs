using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using Newtonsoft.Json;
using System.Linq;

public class bleMain : MonoBehaviour
{
    private string DeviceName = "Kinis 003";
    //private string ServiceUUID = "6e400001"; // 6e400001-b5a3-f393-e0a9-e50e24dcca9e
    //private string Characteristic = "6e400003";

    private string ServiceUUID = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";
    private string Characteristic = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

    public class PressureData
    {
        public string Start_byte;
        public string Protocol_ID;
        public int Data_len;
        public string[][] Pressure_mapping;
        public string Check_sum;
        public string Stop_byte;
    }

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

    private bool _workingFoundDevice = true;
    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private bool _foundID = false;
    private string _deviceAddress;

    [SerializeField] private TextMeshProUGUI field;
    [SerializeField] private TextMeshProUGUI box;

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

    void StartProcess()
    {
        setFieldText("Initializing...");

        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () => {

            SetState(States.Scan, 0.1f);
            setFieldText("Initializing...");

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
                                }
                            }
                        }, (disconnectedAddress) => {
                            BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
                            setFieldText("Disconnected");
                        });
                        break;

                    case States.Subscribe:
                        setFieldText("Subscribing to ESP32");

                        BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, ServiceUUID, Characteristic, null, (address, characteristicUUID, bytes) => {
                            //string converted = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                            // Extracting individual fields from the byte array
                            byte startByte = bytes[0];
                            byte protocolID = bytes[1];
                            byte dataLen = bytes[2];
                            byte[][] pressureMapping = new byte[7][];
                            for (int i = 0; i < 7; i++)
                            {
                                pressureMapping[i] = new byte[4];
                                Array.Copy(bytes, 3 + (i * 4), pressureMapping[i], 0, 4);
                            }
                            byte checkSum = bytes[32];
                            byte stopByte = bytes[33];

                            // Check if protocolID is not equal to 0x32
                            //box.text += "\n" + protocolID.ToString("X2");
                            if (protocolID != 0x32)
                            {
                                //setFieldText("Protocol ID does not match the condition.");
                                //box.text += "\nProtocol ID does not match the condition.";
                                return;
                            }
                            else
                            {
                                //box.text += "\nProtocol ID match the condition.";
                            }

                            // Convert pressureMapping to an array of string arrays
                            string[][] pressureMappingDec = new string[pressureMapping.Length][];
                            for (int i = 0; i < pressureMapping.Length; i++)
                            {
                                pressureMappingDec[i] = new string[pressureMapping[i].Length];
                                for (int j = 0; j < pressureMapping[i].Length; j++)
                                {
                                    pressureMappingDec[i][j] = pressureMapping[i][j].ToString();
                                }
                            }

                            // Creating a JSON object with the extracted fields
                            var dataJson = new PressureData
                            {
                                Start_byte = startByte.ToString("X2"),
                                Protocol_ID = protocolID.ToString("X2"),
                                Data_len = dataLen,
                                Pressure_mapping = pressureMappingDec, // Convert each inner array to a string and make it an array
                                Check_sum = checkSum.ToString("X2"),
                                Stop_byte = stopByte.ToString("X2")
                            };

                            string jsonData = JsonConvert.SerializeObject(dataJson);
                            string hexString = BitConverter.ToString(bytes).Replace("-", ""); // Convert bytes to hex string
                                                                                              // Displaying the Pressure_mapping property in the PressureData object
                            string pressureMappingString = string.Join(",\n", dataJson.Pressure_mapping.Select(arr => string.Join(" ", arr)));

                            //setFieldText("Received JSON Data: " + jsonData + "\nbyte length: " + bytes.Length + ", Data: 0x" + hexString);
                            //box.text += "\nReceived JSON Data: " + jsonData + "\nbyte length: " + bytes.Length + ", Data: 0x" + hexString;
                            setFieldText(pressureMappingString);
                            box.text += "\n"+ pressureMappingString;
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
