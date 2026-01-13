using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    public enum FillDirection
    {
        Right,
        Left,
        Up,
        Down,
    }

    [SerializeField]
    FillDirection fillDirection;

    [SerializeField]
    RectTransform fill;

    [SerializeField]
    string labelText;

    TMP_Text labelTMP;
    Text labelLegacy;

    void Start()
    {
        if (fill == null)
        {
            var fillTransform = transform.Find("Fill");
            if (fillTransform != null)
            {
                fill = fillTransform.GetComponent<RectTransform>();
            }
        }

        if (fill == null)
        {
            Debug.LogWarning(
                $"[Bar] {gameObject.name}: 'fill' RectTransform is not assigned!",
                this
            );
        }

        if (!string.IsNullOrEmpty(labelText))
        {
            var labelTransform = transform.Find("Label");
            if (labelTransform != null)
            {
                labelTMP = labelTransform.GetComponent<TMP_Text>();
                labelLegacy = labelTransform.GetComponent<Text>();
            }

            if (labelTMP == null && labelLegacy == null)
            {
                labelTMP = GetComponentInChildren<TMP_Text>();
                if (labelTMP == null)
                {
                    labelLegacy = GetComponentInChildren<Text>();
                }
            }

            if (labelTMP == null && labelLegacy == null)
            {
                GameObject labelObj = new GameObject("BarLabel");
                labelObj.transform.SetParent(transform, false);

                RectTransform rectTransform = labelObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.pivot = new Vector2(0.5f, 0);
                rectTransform.anchoredPosition = new Vector2(0, 5);
                rectTransform.sizeDelta = new Vector2(0, 20);

                labelLegacy = labelObj.AddComponent<Text>();
                labelLegacy.fontSize = 12;
                labelLegacy.alignment = TextAnchor.LowerCenter;
                labelLegacy.color = Color.white;
                labelLegacy.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                labelLegacy.fontStyle = FontStyle.Bold;

                Debug.Log(
                    $"[Bar] Created label text for '{gameObject.name}' with text: '{labelText}'"
                );
            }

            if (labelTMP != null)
            {
                labelTMP.text = labelText;
            }
            else if (labelLegacy != null)
            {
                labelLegacy.text = labelText;
            }
        }
    }

    public void SetValue(float value)
    {
        if (fill == null)
            return;

        value = Mathf.Clamp01(value);

        if (fillDirection == FillDirection.Right)
        {
            fill.anchorMin = new Vector2(0, 0);
            fill.anchorMax = new Vector2(value, 1);
        }
        else if (fillDirection == FillDirection.Left)
        {
            fill.anchorMin = new Vector2(1 - value, 0);
            fill.anchorMax = new Vector2(1, 1);
        }
        else if (fillDirection == FillDirection.Up)
        {
            fill.anchorMin = new Vector2(0, 0);
            fill.anchorMax = new Vector2(1, value);
        }
        else if (fillDirection == FillDirection.Down)
        {
            fill.anchorMin = new Vector2(0, 1 - value);
            fill.anchorMax = new Vector2(1, 1);
        }
    }
}
