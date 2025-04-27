using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SetSkeleton;
using static UnityEngine.Rendering.HableCurve;

public class SetPressure : MonoBehaviour
{
    public TextAsset jsonTextAsset;
    public UIPressure uiPressue;
    public BaseOfSupport BoS;
    private Dictionary<string, SensorInfo> jsonData;
    private string[] Segments = new string[] { "Left", "Right" };
    // Start is called before the first frame update
    [System.Serializable]
    public class SensorInfo
    {
        public Dictionary<string, float[]> pressureMapping;
        public Dictionary<string, float[]> gyro;
        public Dictionary<string, float[]> accel;
        public Dictionary<string, float> whs;
        //public Dictionary<string, Dictionary<string,bool>> groundDetect;
        public Dictionary<string, bool> groundDetect;
    }
    private int count = 0;
    float timeLimit = 1 / 60;
    float currentTime = 0;
    bool isStop = false;
    public bool isPredict = false;
    public bool isDebug = false;

    void Start()
    {
        if (jsonTextAsset != null)
        {
            string jsonContent = jsonTextAsset.text;

            // Deserialize the JSON data into a C# object
            jsonData = JsonConvert.DeserializeObject<Dictionary<string, SensorInfo>>(jsonContent);
            count = 0;
            if (isPredict) isDebug = true;
        }
        else
        {
            jsonData = new Dictionary<string, SensorInfo>();
            jsonData["0"] = new SensorInfo();
            jsonData["0"].pressureMapping = new Dictionary<string, float[]>();
            jsonData["0"].gyro = new Dictionary<string, float[]>();
            jsonData["0"].accel = new Dictionary<string, float[]>();
            jsonData["0"].whs = new Dictionary<string, float>();
            jsonData["0"].groundDetect = new Dictionary<string, bool>();
            foreach (string segmentName in Segments)
            {
                jsonData["0"].pressureMapping[segmentName] = new float[] { 0, 0, 0, 0 };
                jsonData["0"].gyro[segmentName] = new float[] { 0, 0, 0};
                jsonData["0"].accel[segmentName] = new float[] { 0, 0, 0 };
                jsonData["0"].groundDetect[segmentName] = true;
            }
            jsonData["0"].whs["Weight"] = 60;
            jsonData["0"].whs["Height"] = 176;
            jsonData["0"].whs["Sex"] = 1;
            isDebug = false;
            isPredict = true;
        }
    }

    float toFloat(bool value)
    {
        return value ? 1.0f : 0.0f;
    }

    void SetFrame(string id)
    {
        float[] outputFlat;
        if (isPredict)
        {
            if (groundDetectOuput == null)
            {
                SensorInfo dataObject = jsonData["0"];
                //outputFlat = new float[] { toFloat(dataObject.groundDetect["Left"]["Heel"]), toFloat(dataObject.groundDetect["Left"]["Toe"]), toFloat(dataObject.groundDetect["Right"]["Heel"]), toFloat(dataObject.groundDetect["Right"]["Toe"]) };
                outputFlat = new float[] { toFloat(dataObject.groundDetect["Left"]), toFloat(dataObject.groundDetect["Right"]) };
            }
            else outputFlat = groundDetectOuput;
        }
        else
        {
            SensorInfo dataObject = jsonData[id];
            //outputFlat = new float[] { toFloat(dataObject.groundDetect["Left"]["Heel"]), toFloat(dataObject.groundDetect["Left"]["Toe"]), toFloat(dataObject.groundDetect["Right"]["Heel"]), toFloat(dataObject.groundDetect["Right"]["Toe"]) };
            outputFlat = new float[] { toFloat(dataObject.groundDetect["Left"]), toFloat(dataObject.groundDetect["Right"]) };
        }
        
        BoS.assginBool(outputFlat);
        //uiPressue.setGridValue(0,dataObject.pressureMapping["Left"]);
        //uiPressue.setGridValue(1, dataObject.pressureMapping["Right"]);
    }
    SensorInfo currentInfo = null;
    public void setSensor(SensorInfo sensor)
    {
        currentInfo = sensor;
    }
    public SensorInfo getSensor()
    {
        //return jsonData[(count).ToString()];
        if (isDebug) return jsonData[(count).ToString()];
        else
        {
            if (currentInfo == null) return jsonData[(0).ToString()];
            return currentInfo;
        }
    }

    public SensorInfo getDebugSensor()
    {
        return jsonData[(count).ToString()];
    }


    float[] groundDetectOuput;
    public void updateSensor(float[] output)
    {
        groundDetectOuput = output;
        predictFlag = false;
    }

    public bool manualMode = false;
    public bool predictFlag = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStop = !isStop;
        }
        SetFrame(count.ToString());
        if ((manualMode || isStop) && !predictFlag)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                count++;
                if (isDebug) predictFlag = true;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                count--;
                if (isDebug) predictFlag = true;
            }
            if (count >= jsonData.Count)
            {
                if (isPredict && !isDebug)
                {
                    predictFlag = true;
                }
                else count--;
            }
            else if (count < 0) count = 0;
        }
        else
        {
            if (!isStop)
            {
                currentTime += Time.deltaTime;
                if (currentTime > timeLimit)
                {
                    count++;
                    if (isDebug) predictFlag = true;
                    if (count >= jsonData.Count)
                    {
                        if (isPredict && !isDebug)
                        {
                            predictFlag = true;
                        }
                        else count--;
                    }
                    currentTime = 0;
                }
            }
        }
        //SetFrame(count.ToString());
    }
}
