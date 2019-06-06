using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    private Vector3 baseScale;
    private Material material;
    private TrailRenderer trail;

    private Color newColor;

    public float logValue;


    void Start()
    {
        newColor = Color.black;
        baseScale = transform.localScale;
        material = GetComponent<Renderer>().material;
        trail = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeSize(GraphicsController.Instance.Time / 5f);

        Color targetColor = new Color(1, 1, 1, Mathf.Max(0.5f, Mathf.Log(GraphicsController.Instance.Weight, logValue)));
        material.color = targetColor;

        trail.colorGradient.colorKeys[0].color = targetColor;
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
