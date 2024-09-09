using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class debuggerLog : MonoBehaviour
{
    public TextMeshProUGUI logText;
    void Start()
    {
        Application.logMessageReceived += HandleMessage;
    }

    // Update is called once per frame

    void OnDestroy()
    {
        // Unsubscribe from log message events
        Application.logMessageReceived -= HandleMessage;
    }
    void Update()
    {
    }

    void HandleMessage(string logMessage, string stackTrace, LogType logType)
    {
        if (logType == LogType.Log)
        {
            // Append log message to the UI Text component
            if (logMessage[0] == '!')
            {
                logText.SetText(logMessage + "\n");
            }
        }
    }
}
