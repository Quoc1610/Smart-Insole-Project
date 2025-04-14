using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPressure : MonoBehaviour
{
    public TextAsset jsonTextAsset;
    public UIPressure uiPressue;
    private Dictionary<string, SensorInfo> jsonData;
    // Start is called before the first frame update
    [System.Serializable]
    public class SensorInfo
    {
        public Dictionary<string, float[]> pressureMapping;
        public Dictionary<string, float[]> gyro;
        public Dictionary<string, float[]> accel;
        public Dictionary<string, float> whs;
        public Dictionary<string, Dictionary<string,bool>> groundDetect;
    }
    private int count = 0;
    float timeLimit = 1 / 60;
    float currentTime = 0;
    bool isStop = false;

    void Start()
    {
        if (jsonTextAsset != null)
        {
            string jsonContent = jsonTextAsset.text;

            // Deserialize the JSON data into a C# object
            jsonData = JsonConvert.DeserializeObject<Dictionary<string, SensorInfo>>(jsonContent);
            count = 0;
        }
    }

    void SetFrame(string id)
    {
        SensorInfo dataObject = jsonData[id];
        //uiPressue.setGridValue(0,dataObject.pressureMapping["Left"]);
        //uiPressue.setGridValue(1, dataObject.pressureMapping["Right"]);
    }

    public SensorInfo getSensor()
    {
        return jsonData[(count-1).ToString()];
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
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                count--;
            }
            if (count >= jsonData.Count)
            {
                count--;
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
                    if (count >= jsonData.Count)
                    {
                        count--;
                    }
                    currentTime = 0;
                }
            }
        }

    }
}
