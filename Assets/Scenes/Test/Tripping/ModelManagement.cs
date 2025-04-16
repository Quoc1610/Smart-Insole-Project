using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Interactions.SectorInteraction;

public class ModelManagement : MonoBehaviour
{
    // Start is called before the first frame update
    public HumanPoseModel poseModel;
    public GroundDetectModel groundModel;

    public SetSkeleton skeleton;
    public SetPressure pressure;

    private bool isOn = false;

    private List<float> inputStream = new List<float>();
    private string[] Segments = {"Head", "L3", "L5", "LeftFoot",
                     "LeftForeArm", "LeftHand", "LeftLowerLeg", "LeftShoulder",
                     "LeftToe", "LeftUpperArm", "LeftUpperLeg", "Neck", "Pelvis", "RightFoot",
                     "RightForeArm", "RightHand", "RightLowerLeg", "RightShoulder", "RightToe",
                     "RightUpperArm", "RightUpperLeg", "T12", "T8"};

    private string[] Directions = { "Left", "Right" };

    void Start()
    {
        if (poseModel == null)
        {
            poseModel = gameObject.GetComponent<HumanPoseModel>();
        }
        if (groundModel == null)
        {
            groundModel = gameObject.GetComponent<GroundDetectModel>(); 
        }
        if (skeleton == null)
        {
            skeleton = gameObject.GetComponent<SetSkeleton>();
        }
        if (pressure == null)
        {
            pressure = gameObject.GetComponent<SetPressure>();
        }

        if (skeleton != null && pressure != null) isOn = true;

    }

    private List<float> toList(float[] list)
    {
        List<float> result = new List<float>();
        foreach (var item in list)
        {
            result.Add(item);
        }
        return result;
    }

    private void createInput()
    {
        SetSkeleton.SkeletonInfo skeletonInfo = skeleton.getSkeleton();
        SetPressure.SensorInfo sensorInfo = pressure.getSensor();
        inputStream.Clear();
        foreach (var segment in Segments)
        {
            inputStream.AddRange(toList(skeletonInfo.position[segment]));
        }
        inputStream.AddRange(toList(skeletonInfo.centerOfMass));

        foreach (var direction in Directions)
        {
            inputStream.AddRange(toList(sensorInfo.pressureMapping[direction]));

            inputStream.AddRange(toList(sensorInfo.gyro[direction]));
            inputStream.AddRange(toList(sensorInfo.accel[direction]));
        }
        inputStream.AddRange(toList(new float[] { sensorInfo.whs["Weight"], sensorInfo.whs["Height"], sensorInfo.whs["Sex"] }));
    }

    private SetSkeleton.SkeletonInfo returnSkeleton(float[] output)
    {
        SetSkeleton.SkeletonInfo skeletonInfo = new SetSkeleton.SkeletonInfo();
        skeletonInfo.position = new Dictionary<string, float[]>();

        int count = 0;
        foreach (var segment in Segments)
        {
            //skeletonInfo.position.Add(segment, new float[] { output[count], output[count + 1], output[count + 2] });
            skeletonInfo.position[segment] = new float[] { output[count], output[count + 1], output[count + 2] };
            count += 3;
        }
        skeletonInfo.centerOfMass = new float[] { output[count], output[count + 1], output[count + 2] };
        return skeletonInfo;
    }

    // Update is called once per frame
    void Update()
    {
        //if (isOn && skeleton.predictFlag && pressure.predictFlag)
        //{
        //    createInput();
        //    float[] modelInputs = inputStream.ToArray();
        //    if (poseModel)
        //    {
        //        skeleton.updateSkeleton(returnSkeleton(poseModel.Run(modelInputs)));
        //    }
        //    if (groundModel)
        //    {
        //        pressure.updateSensor(groundModel.Run(modelInputs));
        //    }

        //}
        if (isOn && skeleton.predictFlag && pressure.predictFlag && !isRunning)
        {
            StartCoroutine(RunModelsCoroutine());
        }
    }

    bool isRunning = false;

    IEnumerator RunModelsCoroutine()
    {
        isRunning = true;

        createInput();
        float[] modelInputs = inputStream.ToArray();

        float[] poseResult = null;
        float[] groundResult = null;

        // Run both models, yield in between to keep things non-blocking
        yield return new WaitForEndOfFrame(); // optional yield

        if (poseModel)
            poseResult = poseModel.Run(modelInputs);

        yield return new WaitForEndOfFrame(); // optional yield

        if (groundModel)
            groundResult = groundModel.Run(modelInputs);

        // Now update the values after both are done
        if (poseResult != null)
            skeleton.updateSkeleton(returnSkeleton(poseResult));

        if (groundResult != null)
            pressure.updateSensor(groundResult);

        isRunning = false;
    }
}
