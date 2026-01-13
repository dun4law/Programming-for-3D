using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AIToolkitLogFilter
{
    private static readonly string[] SuppressedPatterns = new string[]
    {
        "CustomStringEnumConverter exception",
        "Failed to parse enum value for CategoryEnumV1",
        "Error converting value \"Textures\" to type 'SuperProxyClientV1Namespace.CategoryEnumV1'",
        "ApiNoLongerSupported",
        "generators-beta.ai.unity.com",
    };

    static AIToolkitLogFilter()
    {
        Application.logMessageReceived += HandleLog;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type) { }
}

public class AIToolkitLogHandler : ILogHandler
{
    private readonly ILogHandler defaultHandler;

    private static readonly string[] SuppressedPatterns = new string[]
    {
        "CustomStringEnumConverter exception",
        "Failed to parse enum value for CategoryEnumV1",
        "Error converting value \"Textures\" to type",
        "ApiNoLongerSupported",
        "generators-beta.ai.unity.com",
    };

    private static bool isInstalled = false;

    public AIToolkitLogHandler(ILogHandler defaultHandler)
    {
        this.defaultHandler = defaultHandler;
    }

    public void LogFormat(
        LogType logType,
        UnityEngine.Object context,
        string format,
        params object[] args
    )
    {
        string message = args.Length > 0 ? string.Format(format, args) : format;

        foreach (var pattern in SuppressedPatterns)
        {
            if (message.Contains(pattern))
            {
                return;
            }
        }

        defaultHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        if (exception != null && exception.Message != null)
        {
            foreach (var pattern in SuppressedPatterns)
            {
                if (exception.Message.Contains(pattern))
                {
                    return;
                }
            }
        }

        defaultHandler.LogException(exception, context);
    }

    [InitializeOnLoadMethod]
    public static void Install()
    {
        if (isInstalled)
            return;

        var currentHandler = Debug.unityLogger.logHandler;
        if (!(currentHandler is AIToolkitLogHandler))
        {
            Debug.unityLogger.logHandler = new AIToolkitLogHandler(currentHandler);
            isInstalled = true;
        }
    }
}
