using System;
using Unity.Mathematics;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform target;
    public Vector3 distance = new Vector3(0f, 2f, -5f);

    public float sens;

    public float currentY = 0f;
    public float currentX = 0f;

    public void SetCameraAngles(float x, float y)
    {
        currentX = x;
        currentY = y;
    }
    
    private void LateUpdate()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        currentY  -= Input.GetAxis("Mouse Y") * sens;
        currentX += Input.GetAxis("Mouse X") * sens;
        
        currentY = Mathf.Clamp(currentY, -25, 60);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        Vector3 wantedPos = target.position + (rotation*distance);
        
        transform.position = wantedPos;
        transform.rotation = rotation;
        
    }
}
