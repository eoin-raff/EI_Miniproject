using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour
{
    private Vector3 baseScale;
    void Start()
    {
        baseScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (GraphicsController.Instance.Time > 2f)
        {
            ChangeSize(GraphicsController.Instance.Time / 10f);
        }
        //ReturnToBaseScale();
    }

    private void ReturnToBaseScale()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime);
    }

    public void ChangeSize(float TimeEffort)
    {
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * TimeEffort, Time.deltaTime * TimeEffort);
    }
}
