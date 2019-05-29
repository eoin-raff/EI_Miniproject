using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsController : Singleton<GraphicsController>
{
    public LabanDescriptors laban;

    public float Weight { get; private set; }
    public float Time { get; private set; }
    public float Flow { get; private set; }
    public float Space { get; private set; }

    void Update()
    {
        Weight = laban.WeightEffort;
        Time = laban.TimeEffort;
        Flow = laban.FlowEffort;
        Space = laban.SpaceEffort;
    }
}
