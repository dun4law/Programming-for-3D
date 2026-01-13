using UnityEngine;
using UnityEngine.UI;

public class AutoAddButtonSounds : MonoBehaviour
{
    [SerializeField]
    private bool addOnStart = true;

    void Start()
    {
        if (addOnStart)
        {
            AddSoundsToAllButtons();
        }
    }

    public void AddSoundsToAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        int count = 0;

        foreach (Button button in allButtons)
        {
            if (button.GetComponent<UIButtonSound>() == null)
            {
                button.gameObject.AddComponent<UIButtonSound>();
                count++;
            }
        }

        Debug.Log($"AutoAddButtonSounds: Added sounds to {count} buttons");
    }
}
