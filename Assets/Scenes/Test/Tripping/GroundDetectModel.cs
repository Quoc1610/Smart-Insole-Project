using UnityEngine;
using System.Collections.Generic;
using TensorFlowLite;
using System.Diagnostics;
using System;
using TMPro;

public class GroundDetectModel : MonoBehaviour
{
    [Header("Model Configuration")]
    [SerializeField, FilePopup("*.tflite")] string fileName = "modelCal_all_K.tflite";
    Interpreter interpreter;

    [SerializeField] private int inputSize = 95;
    [SerializeField] private int outputSize = 2;

    public bool modelImportComplete = false;

    private float[,] outputBuffer = new float[1, 2];
    public TextMeshProUGUI timeCalculated;
    int count = 0;
    Double avg_time = 0;

    void Start()
    {
        BuildAndImportModel();
    }

    void BuildAndImportModel()
    {
        modelImportComplete = false;

        var options = new InterpreterOptions()
        {
            threads = 2,
        };
        interpreter = new Interpreter(FileUtil.LoadFile(fileName), options);
        interpreter.AllocateTensors();
        modelImportComplete = true;
    }

    public float[] Run(float[] modelInputs)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        float[,,] modelInputsFormatted = new float[1, 1, inputSize];
        for (int i = 0; i < inputSize; i++)
            modelInputsFormatted[0, 0, i] = modelInputs[i];

        // Run inference
        interpreter.SetInputTensorData(0, modelInputsFormatted);
        interpreter.Invoke();
        outputBuffer = new float[1, outputSize];
        interpreter.GetOutputTensorData(0, outputBuffer);
        interpreter.Dispose();
        BuildAndImportModel();
        float[] outputFlat = new float[outputSize];
        for (int i = 0; i < outputSize; i++)
        {
            outputFlat[i] = outputBuffer[0, i];
        }
        stopwatch.Stop();
        TimeSpan elapsedTime = stopwatch.Elapsed;
        double roundedTime = Math.Round(elapsedTime.TotalMilliseconds, 2);
        avg_time += roundedTime;
        count++;
        if (timeCalculated) timeCalculated.text = "Avg Elapsed time (GroundDetect): " + (avg_time / count).ToString("0.00") + " ms";
        return outputFlat;
    }

    void OnDestroy()
    {
        interpreter?.Dispose();
    }
}
