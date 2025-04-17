using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using TMPro;

public class SetSkeleton : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform head;
    public Transform neck;
    public Transform upperChest;
    public Transform chest;
    public Transform upperSpine;
    public Transform lowerSpine;
    public Transform hip;

    public Transform rightUpperLeg;
    public Transform rightLowerLeg;
    public Transform rightFoot;
    public Transform rightToes;

    public Transform leftUpperLeg;
    public Transform leftLowerLeg;
    public Transform leftFoot;
    public Transform leftToes;

    public Transform rightShoulder;
    public Transform rightUpperArm;
    public Transform rightLowerArm;
    public Transform rightHand;

    public Transform leftShoulder;
    public Transform leftUpperArm;
    public Transform leftLowerArm;
    public Transform leftHand;

    public Transform centerOfGravity;

    public TextAsset jsonTextAsset;

    private Dictionary<string, SkeletonInfo> jsonData;

    private string[] Segments = {"Head", "L3", "L5", "LeftFoot",
                     "LeftForeArm", "LeftHand", "LeftLowerLeg", "LeftShoulder",
                     "LeftToe", "LeftUpperArm", "LeftUpperLeg", "Neck", "Pelvis", "RightFoot",
                     "RightForeArm", "RightHand", "RightLowerLeg", "RightShoulder", "RightToe",
                     "RightUpperArm", "RightUpperLeg", "T12", "T8"};
    private int count = 0;
    public bool isPredict = false;
    public bool isDebug = false;
    void SetFrame(string id)
    {
        SkeletonInfo dataObject;
        if (!isPredict) dataObject = jsonData[id];
        else
        {
            if (currentInfo == null) dataObject = jsonData["0"];
            else dataObject = currentInfo;
        }
        foreach (string segmentName in Segments)
        {
            if (dataObject.position.ContainsKey(segmentName))
            {
                Transform targetTransform = GetTransformByName(segmentName);
                if (targetTransform != null)
                {
                    Vector3 segmentPosition = new Vector3(dataObject.position[segmentName][0], dataObject.position[segmentName][2], dataObject.position[segmentName][1]);
                    targetTransform.localPosition = segmentPosition;
                }
                else
                {
                    Debug.LogWarning("Transform not found for segment: " + segmentName);
                }
            }
        }
        Vector3 CoG = new Vector3(dataObject.centerOfMass[0], dataObject.centerOfMass[2], dataObject.centerOfMass[1]);
        centerOfGravity.localPosition = CoG;
    }
    void Start()
    {
        if (jsonTextAsset != null)
        {
            string jsonContent = jsonTextAsset.text;
            // Deserialize the JSON data into a C# object
            jsonData = JsonConvert.DeserializeObject<Dictionary<string,SkeletonInfo>>(jsonContent);
            count = 0;
            if (!isPredict) isDebug = false;
            if (isPredict && !isDebug)
            {
                SkeletonInfo dataObject = jsonData["0"];
                jsonData = new Dictionary<string, SkeletonInfo>();
                jsonData["0"] = dataObject;
            }
        }
        else
        {
            jsonData = new Dictionary<string,SkeletonInfo>();
            jsonData["0"] = new SkeletonInfo();
            jsonData["0"].position = new Dictionary<string, float[]>();
            foreach (string segmentName in Segments)
            {
                Transform targetTransform = GetTransformByName(segmentName);
                if (targetTransform != null)
                {
                    Vector3 segmentPosition = new Vector3(targetTransform.localPosition.x, targetTransform.localPosition.z, targetTransform.localPosition.y);
                    jsonData["0"].position[segmentName] = new float[] { segmentPosition.x, segmentPosition.y, segmentPosition.z };
                }
            }
            Vector3 CoG = new Vector3(centerOfGravity.localPosition.x, centerOfGravity.localPosition.z, centerOfGravity.localPosition.y);
            jsonData["0"].centerOfMass = new float[] {CoG.x, CoG.y, CoG.z };
            isDebug = false;
            isPredict = true;
        }
    }

    Transform GetTransformByName(string name)
    {
        switch (name)
        {
            case "Head":
                return head;
            case "Neck":
                return neck;
            case "T8":
                return upperChest;
            case "T12":
                return chest;
            case "L3":
                return upperSpine;
            case "L5":
                return lowerSpine;
            case "Pelvis":
                return hip;
            case "RightUpperLeg":
                return rightUpperLeg;
            case "RightLowerLeg":
                return rightLowerLeg;
            case "RightFoot":
                return rightFoot;
            case "RightToe":
                return rightToes;
            case "LeftUpperLeg":
                return leftUpperLeg;
            case "LeftLowerLeg":
                return leftLowerLeg;
            case "LeftFoot":
                return leftFoot;
            case "LeftToe":
                return leftToes;
            case "RightShoulder":
                return rightShoulder;
            case "RightUpperArm":
                return rightUpperArm;
            case "RightForeArm":
                return rightLowerArm;
            case "RightHand":
                return rightHand;
            case "LeftShoulder":
                return leftShoulder;
            case "LeftUpperArm":
                return leftUpperArm;
            case "LeftForeArm":
                return leftLowerArm;
            case "LeftHand":
                return leftHand;
            default:
                return null;
        }
    }

    [System.Serializable]
    public class SkeletonInfo
    {
        public Dictionary<string, float[]> position;
        public float[] centerOfMass;
    }

    private SkeletonInfo currentInfo = null;

    public SkeletonInfo getSkeleton()
    {
        if (isDebug) return jsonData[(count - 1).ToString()];
        else
        {
            if (currentInfo == null) return jsonData[(0).ToString()];
            return currentInfo;
        }
    }

    public void updateSkeleton(SkeletonInfo info)
    {
        currentInfo = info;
        predictFlag = false;
    }

    float timeLimit = 1 / 60;
    float currentTime = 0;
    bool isStop = false;
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
                Debug.Log("Pose: " + count.ToString());
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
