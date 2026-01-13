using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField]
    private bool playHoverSound = true;

    [SerializeField]
    private bool playClickSound = true;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonHover();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }
}
