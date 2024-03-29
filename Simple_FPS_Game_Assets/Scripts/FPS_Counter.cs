using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class FPS_Counter : MonoBehaviour
{
    [SerializeField] float UpdateTime = 0.3f;
    TextMeshProUGUI Text;

    int FpsCount = 0;
    float Timer = 0;

    private int currentFps;
    private int totalFps;
    private int totalUpdates;

    void Start()
    {
        Text = GetComponent<TextMeshProUGUI>();
        Text.color = Color.yellow;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Timer >= UpdateTime)
        {
            Text.text = (FpsCount / Timer).ConvertTo<int>().ToString();
            currentFps = (FpsCount / Timer).ConvertTo<int>();
            totalFps = totalFps + currentFps;
            totalUpdates++;
            Timer = 0;
            FpsCount = 0;
        }
        else
        {
            Timer += Time.deltaTime;
            FpsCount++;
        }
    }
    private void OnApplicationQuit()
    {
        if (totalUpdates > 1)
        {
            int final = totalFps / (totalUpdates - 1);
            Debug.Log("Avg fps last run = " + final);
        }
    }
}
