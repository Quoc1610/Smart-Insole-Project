using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements; // Requires Barracuda package

public class TF_Model : MonoBehaviour
{
    [Header("Model Configuration")]
    [SerializeField] private Noedify.Net modelAsset;
    public string modelName;
    
    [SerializeField] private int inputSize = 95;
    [SerializeField] private int outputSize = 72;

    private Noedify_Solver solver;
    public bool modelImportComplete = false;

    private float[] outputBuffer = new float[72];

    public SetSkeleton skeleton;
    public SetPressure pressure;

    private List<float> inputStream = new List<float>();
    private string[] Segments = {"Head", "L3", "L5", "LeftFoot",
                     "LeftForeArm", "LeftHand", "LeftLowerLeg", "LeftShoulder",
                     "LeftToe", "LeftUpperArm", "LeftUpperLeg", "Neck", "Pelvis", "RightFoot",
                     "RightForeArm", "RightHand", "RightLowerLeg", "RightShoulder", "RightToe",
                     "RightUpperArm", "RightUpperLeg", "T12", "T8"};

    private string[] Directions = { "Left", "Right" };

    void Start()
    {
        BuildAndImportModel();
    }

    private void Update()
    {
        Run();
    }

    void BuildAndImportModel()
    {
        modelImportComplete = false;

        modelAsset = new Noedify.Net();
        bool status = modelAsset.LoadModel(modelName);
        if (status == false)
        {
            Debug.Log("Binary file not found");
        }

        // Attempt to load network saved as a binary file
        // This is much faster than importing form a parameters file
        solver = Noedify.CreateSolver();
        solver.suppressMessages = true;
        modelImportComplete = true;
    }

    private List<float> toList(float[] list)
    {
        List<float> result = new List<float>();
        foreach(var item in list)
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
        int count = 0;
        foreach (var segment in Segments)
        {
            skeletonInfo.position[segment] = new float[] { output[count], output[count + 1], output[count + 2] };
            count += 3;
        }
        skeletonInfo.centerOfMass = new float[] { output[count], output[count + 1], output[count + 2] };
        return skeletonInfo;
    }

    public void Run()
    {
        if (skeleton.predictFlag) // Assuming skeleton is a reference to an object with predictFlag
        {
            createInput();

            float[] modelInputs = inputStream.ToArray();
            float[,,] modelInputsFormatted = new float[1, 1, inputSize];
            for (int i = 0; i < inputSize; i++)
                modelInputsFormatted[0, 0, i] = modelInputs[i];

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            solver.Evaluate(modelAsset, modelInputsFormatted, Noedify_Solver.SolverMethod.MainThread);
            sw.Stop();

            StartCoroutine(UpdateWhenComplete());
        }
    }


    private float GetObservationValue(int index)
    {
        // Customize with your observation logic
        return inputStream[index];
    }

    IEnumerator UpdateWhenComplete()
    {
        while (solver.evaluationInProgress)
        {
            yield return null;
        }
        outputBuffer = solver.prediction;
        skeleton.updateSkeleton(returnSkeleton(outputBuffer));
    }
}