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


    // Update is called once per frame
    void Update()
    {
        WeightEffortDisplay.text = "Weight Effort: " + labanDescriptors.WeightEffortValue;
    }
}
