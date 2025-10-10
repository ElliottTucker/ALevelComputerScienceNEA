using System.Net.Http.Headers;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    
    public TMP_Text height;
    public Transform playerTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    { 
        
    }

    // Update is called once per frame
    void Update()
    {
        float mapHeight = PlatformGeneration.maxHeight-PlatformGeneration.minHeight;
        float playerHeight = playerTransform.position.y - PlatformGeneration.minHeight;
        if (mapHeight <= 0)
        {
            return;
        }
        float heightPercent = playerHeight / mapHeight*100;
        if (heightPercent > 100)
        {
            heightPercent = 100;
        }
        height.text = heightPercent.ToString("F0")+"%";
    }
}

