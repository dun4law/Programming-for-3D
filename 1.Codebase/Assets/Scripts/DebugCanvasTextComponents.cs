using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugCanvasTextComponents : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private bool logOnStart = true;

    [SerializeField]
    private bool includeInactive = true;

    void Start()
    {
        if (logOnStart)
        {
            LogAllTextComponents();
        }
    }

    [ContextMenu("Log All Text Components")]
    public void LogAllTextComponents()
    {
        Debug.Log("=== Canvas Text Components Debug ===");

        var tmpTexts = GetComponentsInChildren<TMP_Text>(includeInactive);
        Debug.Log($"Found {tmpTexts.Length} TMP_Text components:");
        foreach (var tmp in tmpTexts)
        {
            Debug.Log(
                $"  [TMP] {GetFullPath(tmp.transform)} | Text: '{tmp.text}' | Active: {tmp.gameObject.activeInHierarchy}"
            );
        }

        var legacyTexts = GetComponentsInChildren<Text>(includeInactive);
        Debug.Log($"\nFound {legacyTexts.Length} Legacy Text components:");
        foreach (var text in legacyTexts)
        {
            Debug.Log(
                $"  [Legacy] {GetFullPath(text.transform)} | Text: '{text.text}' | Active: {text.gameObject.activeInHierarchy}"
            );
        }

        Debug.Log("=== End of Canvas Debug ===");
    }

    string GetFullPath(Transform t)
    {
        if (t == null)
            return "null";
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            if (t.parent == null)
                break;
            path = t.name + " / " + path;
        }
        return path;
    }
}
