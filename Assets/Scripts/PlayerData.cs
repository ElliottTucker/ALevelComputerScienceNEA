using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public int seed;
    public float[] playerPosition;
    public float[] cameraPosition;
    public float[] cameraRotation;
    public float cameraX;
    public float cameraY;
    public float[] playerVelocity;
    public float heightPercent;
    public PlayerData(int currentSeed, Vector3 playerPosition, Vector3 camPos, Quaternion camRos, float camX, float camY, Vector3 playerVel, Vector3 finalPlatPos)//constructor
    {
        //seed
        seed = currentSeed;
        
        //Player Postion
        this.playerPosition = new float[3];
        this.playerPosition[0] = playerPosition.x;
        this.playerPosition[1] = playerPosition.y;
        this.playerPosition[2] = playerPosition.z;
        
        //camera postion vector
        cameraPosition = new float[3];
        cameraPosition[0] = camPos.x;
        cameraPosition[1] = camPos.y;
        cameraPosition[2] = camPos.z;
        
        //camera rotation quaternion 
        cameraRotation = new float[4];
        cameraRotation[0] = camRos.x;
        cameraRotation[1] = camRos.y;
        cameraRotation[2] = camRos.z;
        cameraRotation[3] = camRos.w;

        cameraX = camX; 
        cameraY = camY;

        playerVelocity = new float[3];
        playerVelocity[0] = playerVel.x;
        playerVelocity[1] = playerVel.y;
        playerVelocity[2] = playerVel.z;
        
        heightPercent = playerPosition.y / finalPlatPos.y*100;
        heightPercent = Mathf.Clamp(heightPercent, 0f, 100f);
    }
}
