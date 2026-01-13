using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField]
    private string fallbackTargetSceneName = "Main";

    [Header("Activation")]
    [SerializeField]
    private float minimumLoadingScreenSeconds = 0.25f;

    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        var target = string.IsNullOrWhiteSpace(LoadingSceneState.TargetSceneName)
            ? fallbackTargetSceneName
            : LoadingSceneState.TargetSceneName;

        if (string.IsNullOrWhiteSpace(target))
        {
            Debug.LogError(
                $"{nameof(LoadingSceneController)}: No target scene name provided.",
                this
            );
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(target))
        {
            Debug.LogError(
                $"{nameof(LoadingSceneController)}: Scene '{target}' is not in Build Settings.",
                this
            );
            yield break;
        }

        yield return null;

        var elapsed = 0f;
        var loadOp = SceneManager.LoadSceneAsync(target, LoadSceneMode.Single);
        if (loadOp == null)
            yield break;

        loadOp.allowSceneActivation = false;

        while (!loadOp.isDone)
        {
            elapsed += Time.unscaledDeltaTime;

            if (loadOp.progress >= 0.9f && elapsed >= Mathf.Max(0f, minimumLoadingScreenSeconds))
            {
                loadOp.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
