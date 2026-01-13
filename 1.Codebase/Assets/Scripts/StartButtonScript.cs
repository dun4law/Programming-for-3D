using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButtonScript : MonoBehaviour
{
    [SerializeField]
    private Button startButton;

    [SerializeField]
    private string sceneToLoad = "Main";

    [SerializeField]
    private string loadingSceneName = "Loading";

    [SerializeField]
    private bool clearOtherOnClickHandlers = true;

    private void Awake()
    {
        AutoAssignButtonIfNeeded();
    }

    private void OnEnable()
    {
        if (!AutoAssignButtonIfNeeded())
            return;
        if (clearOtherOnClickHandlers)
            UIClearAllListeners(startButton.onClick);
        startButton.onClick.AddListener(StartGame);
    }

    private void OnDisable()
    {
        if (startButton == null)
            return;
        startButton.onClick.RemoveListener(StartGame);
    }

    private void StartGame()
    {
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogError($"{nameof(StartButtonScript)}: sceneToLoad is empty.", this);
            return;
        }

        Time.timeScale = 1f;

        if (
            !string.IsNullOrWhiteSpace(loadingSceneName)
            && Application.CanStreamedLevelBeLoaded(loadingSceneName)
        )
        {
            LoadingSceneState.TargetSceneName = sceneToLoad;
            SceneManager.LoadScene(loadingSceneName);
            return;
        }

        if (
            !string.IsNullOrWhiteSpace(loadingSceneName)
            && !Application.CanStreamedLevelBeLoaded(loadingSceneName)
        )
        {
            Debug.LogWarning(
                $"{nameof(StartButtonScript)}: Loading scene '{loadingSceneName}' is not in Build Settings. Falling back to direct scene load.",
                this
            );
        }

        SceneManager.LoadScene(sceneToLoad);
    }

    private bool AutoAssignButtonIfNeeded()
    {
        if (startButton != null)
            return true;

        if (TryGetComponent(out Button ownButton))
        {
            startButton = ownButton;
            return true;
        }

        startButton = GetComponentInChildren<Button>(true);
        if (startButton != null)
            return true;

        Debug.LogError(
            $"{nameof(StartButtonScript)}: No {nameof(Button)} found to hook up. Assign '{nameof(startButton)}' in the Inspector or add this component to a UI Button GameObject.",
            this
        );
        return false;
    }

    private static void UIClearAllListeners(UnityEngine.Events.UnityEventBase unityEvent)
    {
        if (unityEvent == null)
            return;

        try
        {
            var removeAll = unityEvent
                .GetType()
                .GetMethod("RemoveAllListeners", BindingFlags.Instance | BindingFlags.Public);
            removeAll?.Invoke(unityEvent, null);
        }
        catch { }

        try
        {
            var baseType = typeof(UnityEngine.Events.UnityEventBase);
            var persistentCallsField = baseType.GetField(
                "m_PersistentCalls",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            var callsField = baseType.GetField(
                "m_Calls",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            var persistentCalls = persistentCallsField?.GetValue(unityEvent);
            if (persistentCalls != null)
            {
                var listField = persistentCalls
                    .GetType()
                    .GetField("m_Calls", BindingFlags.Instance | BindingFlags.NonPublic);
                if (listField?.GetValue(persistentCalls) is IList list)
                    list.Clear();
            }

            var calls = callsField?.GetValue(unityEvent);
            if (calls != null)
            {
                var clearPersistent = calls
                    .GetType()
                    .GetMethod(
                        "ClearPersistent",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                clearPersistent?.Invoke(calls, null);

                var clear = calls
                    .GetType()
                    .GetMethod(
                        "Clear",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );
                clear?.Invoke(calls, null);
            }
        }
        catch { }
    }
}
