using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("External Dependencies")]
    public LabanDescriptors labanDescriptors;

    [Header("UI Elements")]
    public TextMeshProUGUI WeightEffortDisplay;
    public TextMeshProUGUI TimeEffortDisplay;
    public TextMeshProUGUI SpaceEffortDisplay;
    public TextMeshProUGUI FlowEffortDisplay;


    // Update is called once per frame
    void Update()
    {
        WeightEffortDisplay.text = "Weight Effort: " + labanDescriptors.WeightEffort;
        TimeEffortDisplay.text = "Time Effort: " + labanDescriptors.TimeEffort;
        SpaceEffortDisplay.text = "Space Effort: " + labanDescriptors.SpaceEffort;
        FlowEffortDisplay.text = "Flow Effort: " + labanDescriptors.FlowEffort;
    }
}
