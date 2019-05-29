using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    private Vector3 baseScale;
    private Material material;

    private Color newColor;

    public float logValue;
    void Start()
    {
        newColor = Color.black;
        baseScale = transform.localScale;
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (GraphicsController.Instance.Time > 2f)
        {
            ChangeSize(GraphicsController.Instance.Time / 10f);
        }
        //        print(Mathf.Log(GraphicsController.Instance.Weight, logValue));
        Color targetColor = new Color(
            Mathf.Log(GraphicsController.Instance.Weight, logValue), //material.color.r,
            Mathf.Log(GraphicsController.Instance.Weight, logValue), //material.color.g,
            Mathf.Log(GraphicsController.Instance.Weight, logValue), //material.color.b,
            1);
        //        newColor = Color.Lerp(newColor, targetColor, Time.deltaTime);
        material.color = targetColor;// newColor;
    }

    private void ReturnToBaseScale()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 10);
    }

    public void ChangeSize(float TimeEffort)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * TimeEffort, Time.deltaTime * TimeEffort);
    }
}
