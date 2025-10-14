using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class SaveData : MonoBehaviour
{
    public static SaveData instance;//this line took 3 hours to write 
    
    private PlatformGeneration platformGeneration;
    public Transform playerCordinates;
    public Transform cameraCordinates;
    
    
    private PlayerData playerData;
    
    void Awake()
    {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                
                playerData = SaveSystem.LoadData();
            }
            else
            {
//if alr exist DESTROY IT
                Destroy(gameObject);
            }
    }

    public void SaveCurrentGame()
    {
        platformGeneration = GameObject.FindFirstObjectByType<PlatformGeneration>();
        playerCordinates = GameObject.FindGameObjectWithTag("Player").transform;
        CameraScript camScript = Camera.main.GetComponent<CameraScript>();
        Transform mainCam = Camera.main.transform;
        Rigidbody rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        
        
        
        PlayerData dataToSave = new PlayerData(
            platformGeneration.seed, 
            playerCordinates.position, 
            mainCam.position, 
            mainCam.rotation, 
            camScript.currentX, 
            camScript.currentY,
            rb.linearVelocity,
            platformGeneration.finalPlatGO.transform.position
            );
        
        SaveSystem.SaveData(dataToSave);
        
        playerData = SaveSystem.LoadData();
    }

    public void ResaveData()
    {
        playerData = SaveSystem.LoadData();
    }

    public void ResetData()
    {
        playerData = null;
    }

    public int GetSeed()
    {
        if (playerData != null)
        {
            return playerData.seed;
        }
        else
        {
            return Random.Range(0, 1000000);
        }
    }

    public Vector3 GetPlayerCordinates()
    {
        if (playerData != null)
        {
            Vector3 pos = new Vector3();
            pos.x = playerData.playerPosition[0];
            pos.y = playerData.playerPosition[1];
            pos.z = playerData.playerPosition[2];
            return pos;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 GetCameraCordinates()
    {
        if (playerData != null)
        {
            Vector3 pos = new Vector3();
            pos.x = playerData.cameraPosition[0];
            pos.y = playerData.cameraPosition[1];
            pos.z = playerData.cameraPosition[2];
            return pos;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Quaternion GetCameraRotation()
    {
        if (playerData != null)
        {
            Quaternion rot = new Quaternion();
            rot.x = playerData.cameraRotation[0];
            rot.y = playerData.cameraRotation[1];
            rot.z = playerData.cameraRotation[2];
            rot.w = playerData.cameraRotation[3];
            return rot;
        }
        else
        {
            
            return Quaternion.identity;
        }
    }

    public float GetCamX()
    {
        if (playerData != null)
        {
            return playerData.cameraX;
        }
        else
        {
            return 0f;
        }
    }

    public float GetCamY()
    {
        if (playerData != null)
        {
            return playerData.cameraY;
        }
        else
        {
            return 0f;
        }
    }

    public Vector3 GetPlayerVelocity()
    {
        if (playerData != null)
        {
            Vector3 velocity = new Vector3();
            velocity.x = playerData.playerVelocity[0];
            velocity.y = playerData.playerVelocity[1];
            velocity.z = playerData.playerVelocity[2];
            return velocity;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public float GetHeightPercent()
    {
        if (playerData != null)
        {
            return playerData.heightPercent;
        }
        else
        {
            return 0f;
        }
    }
}
