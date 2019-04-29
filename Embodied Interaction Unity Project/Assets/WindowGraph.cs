using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class WindowGraph : MonoBehaviour
{

    public RectTransform graphContainer;
    [SerializeField]
    private Sprite circleSprite;

    public List<float> Data { get; set; }

    private void Update()
    {
        //List<float> values = new List<float> { 33f, 13f, 75f, 10f, 15f, 16f, 17f, 26f, 37f, 27f, 38f, 87f, 75f, 50 };
        if (Data == null || Data.Count < 1)
            return;
        if (Data.Count > UIController.Instance.maxLength)
        {
            Data.RemoveRange(0, Data.Count - UIController.Instance.maxLength);
        }
        ShowGraph(Data);
    }
    private GameObject createCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        return gameObject;
    }

    private void ShowGraph(List<float> values)
    {
        float graphHeight = graphContainer.sizeDelta.y;

        float yMaximum = 0f;

        foreach (float value in values)
        {
            if (value > yMaximum)
            {
                yMaximum = value;
            }
        }
        yMaximum *= 1.2f;

        float xSize = 50f;
        GameObject previousCircle = null;
        for (int i = 0; i < values.Count; i++)
        {
            float xPosition = i * xSize;
            float yPosition = (values[i] / yMaximum) * graphHeight;
            GameObject circle = createCircle(new Vector2(xPosition, yPosition));
            if (previousCircle != null)
            {
                CreateDotConnection(previousCircle.GetComponent<RectTransform>().anchoredPosition, circle.GetComponent<RectTransform>().anchoredPosition);
            }
            previousCircle = circle;
        }
    }
    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("Dot Connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 direction = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(distance, 3);
        rectTransform.anchoredPosition = dotPositionA + direction * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(direction));



    }
}
