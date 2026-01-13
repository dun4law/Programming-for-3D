using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyDownNotification : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]
    private GameObject notificationPanel;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private TextMeshProUGUI enemyNameText;

    [Header("Settings")]
    [SerializeField]
    private float displayDuration = 1.5f;

    [SerializeField]
    private float fadeInDuration = 0.2f;

    [SerializeField]
    private float fadeOutDuration = 0.3f;

    [Header("Audio (Optional)")]
    [SerializeField]
    private AudioClip killSound;

    private CanvasGroup canvasGroup;
    private Coroutine currentNotification;
    private bool isSubscribed = false;

    void Awake()
    {
        if (notificationPanel == null)
        {
            Debug.LogError(
                "[EnemyDownNotification] notificationPanel is not assigned. UI will not display."
            );
            enabled = false;
            return;
        }

        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();

        HideNotification();
    }

    void Start()
    {
        TrySubscribe();
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (KillTracker.Instance == null)
        {
            var killTracker = FindAnyObjectByType<KillTracker>();
            if (killTracker == null)
            {
                Debug.Log("[EnemyDownNotification] Automatically creating KillTracker");
                var go = new GameObject("KillTracker");
                go.AddComponent<KillTracker>();
            }
        }

        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.OnKillAnnounced += OnEnemyKilled;
            isSubscribed = true;
            Debug.Log("[EnemyDownNotification] Successfully subscribed to KillTracker events");
        }
        else
        {
            StartCoroutine(DelayedSubscribe());
        }
    }

    private IEnumerator DelayedSubscribe()
    {
        yield return new WaitForSeconds(0.5f);

        if (!isSubscribed && KillTracker.Instance != null)
        {
            KillTracker.Instance.OnKillAnnounced += OnEnemyKilled;
            isSubscribed = true;
            Debug.Log("[EnemyDownNotification] Delayed subscription successful");
        }
    }

    private void TryUnsubscribe()
    {
        if (!isSubscribed)
            return;

        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.OnKillAnnounced -= OnEnemyKilled;
        }
        isSubscribed = false;
    }

    private void OnEnemyKilled(string killerName, string victimName, bool isPlayerKill)
    {
        Debug.Log(
            $"[EnemyDownNotification] Kill event: killer={killerName}, victim={victimName}, isPlayerKill={isPlayerKill}"
        );

        if (!isPlayerKill)
            return;

        ShowNotification(victimName);
    }

    public void ShowNotification(string enemyName = "")
    {
        if (notificationPanel == null)
            return;

        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }

        if (enemyNameText != null && !string.IsNullOrEmpty(enemyName))
        {
            enemyNameText.text = enemyName;
        }

        currentNotification = StartCoroutine(ShowNotificationCoroutine());
    }

    private IEnumerator ShowNotificationCoroutine()
    {
        notificationPanel.SetActive(true);

        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        if (AudioManager.Instance != null) { }
        else if (killSound != null)
        {
            AudioSource.PlayClipAtPoint(killSound, Camera.main.transform.position);
        }

        yield return new WaitForSeconds(displayDuration);

        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }

        HideNotification();
        currentNotification = null;
    }

    private void HideNotification()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
}
