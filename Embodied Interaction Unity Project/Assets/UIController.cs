using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : Singleton<UIController>
{
    [Header("External Dependencies")]
    public LabanDescriptors labanDescriptors;

    [Header("Text Elements")]
    public TextMeshProUGUI WeightEffortDisplay;
    public TextMeshProUGUI TimeEffortDisplay;
    public TextMeshProUGUI SpaceEffortDisplay;
    public TextMeshProUGUI FlowEffortDisplay;

    [Header("Graphs")]
    public int maxLength;
    public WindowGraph weightGraph;
    public WindowGraph spaceGraph;
    public WindowGraph timeGraph;
    public WindowGraph flowGraph;

    public List<float> weightData;
    public List<float> timeData;
    public List<float> spaceData;
    public List<float> flowData;
    // Update is called once per frame
    void Update()
    {
        WeightEffortDisplay.text = "Weight Effort: " + labanDescriptors.WeightEffort;
        TimeEffortDisplay.text = "Time Effort: " + labanDescriptors.TimeEffort;
        SpaceEffortDisplay.text = "Space Effort: " + labanDescriptors.SpaceEffort;
        FlowEffortDisplay.text = "Flow Effort: " + labanDescriptors.FlowEffort;

    }
}
