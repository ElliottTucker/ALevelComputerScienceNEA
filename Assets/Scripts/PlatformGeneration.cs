using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class PlatformGeneration : MonoBehaviour
{
    
    public Vector3 currentPos = Vector3.zero;
    
    public Transform playerPosition;
    
    public float NumPlats=100f;
    public float MinSize=10f;
    public float MaxSize=50f;
    public float MinAngle;
    public float MaxAngle;
    public float MinYchange = 0.2f;
    public float MaxYchange = 2f;
    public float MinXzDistance = 3f;
    public float MaxXzDistance = 6f;
    public LayerMask platLayerMask;
    public int seed;
    

    public static float minHeight;
    public static float maxHeight;
    
    
    public PauseUI pauseUI;
    private SaveData saveData;
    private float currentAngle = 0f;
    
    public GameObject finalPlatGO;
    private List<PlatformNode> mainPathNodes;
    

    public void GenerateLevel()
    {
        GenerateMainPath();
    }
    
    private void GenerateMainPath()
    {

        int retry = 0;
        mainPathNodes = new List<PlatformNode>();
        
        GameObject firstPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstPlatform.transform.position = new Vector3(0, 0f, 0);
        firstPlatform.layer = 7;
        
        Platform firstPlatformComponent = firstPlatform.AddComponent<Platform>();
        firstPlatformComponent.CreatePlatform(PlatformType.Normal, new Vector3(5f, 0.2f, 5f));
        

        
        PlatformNode firstPlat = new PlatformNode(null, firstPlatform, 0, PlatformType.Normal);
        mainPathNodes.Add(firstPlat);

        currentPos = firstPlatform.transform.position;
        minHeight = currentPos.y;
        
        
        for (int i = 0; i < NumPlats; i++)
        {
            
            
            float randomOffset = Random.Range(MinAngle, MaxAngle); 
            float RandomAngle = currentAngle + randomOffset; 
            
            float RandomX = Random.Range(MinSize, MaxSize); 
           float RandomZ = Random.Range(MinSize, MaxSize); 
           float XZdistance = Random.Range(MinXzDistance, MaxXzDistance);
           float yChange = Random.Range(MinYchange, MaxYchange);
           
           Vector3 direction = new Vector3(Mathf.Cos(RandomAngle*Mathf.Deg2Rad), 0, Mathf.Sin(RandomAngle * Mathf.Deg2Rad));
           Vector3 horizontalOffset = direction.normalized * XZdistance;
           Vector3 nextPos = currentPos + horizontalOffset + new Vector3(0, yChange, 0);
           if ((i - 1) % 20 == 0 && i != 1)
           {
               nextPos.y += 20f;
           }
           
           GameObject nextPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
           nextPlat.layer = 7;
           nextPlat.transform.position = nextPos;
           if (i % 20 == 0 && i != 0)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.blue;
           }

           if (i == 99)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.gold;
           }
           Platform platformComponent = nextPlat.AddComponent<Platform>();
           PlatformType platformType = PlatformType.Normal;

           if (i % 20 == 0 && i != 0)
           {
               platformType = PlatformType.Bouncy;
               
           }

           if (i == 99)
           {
               platformType = PlatformType.Win;
           }
           
           platformComponent.CreatePlatform(platformType, new Vector3(RandomX, 0.2f, RandomZ));
           
           
           Bounds bound = nextPlat.GetComponent<Renderer>().bounds;
           
           Vector3 boundMiddle = nextPlat.GetComponent<Renderer>().bounds.center;
           Vector3 boundsize = new Vector3(bound.extents.x+1f, bound.extents.y, bound.extents.z+1f);
           float castDistance = bound.extents.y + 10f;
           
           Physics.SyncTransforms();

           if (retry >= 200)
           {
               //remove last platform
               if (nextPlat != null)
               {
                   Destroy(nextPlat);
                   nextPlat = null;
               }

               //remove upto the last 5 platforms but not the root node
               int removeCount = Mathf.Min(5, Mathf.Max(0, mainPathNodes.Count-1));

               for (int j = 0; j < removeCount; j++)
               {
                   int lastNodeIndex = mainPathNodes.Count - 1;
                   PlatformNode lastNode = mainPathNodes[lastNodeIndex];
                   if (lastNode != null && lastNode.platformObject != null)
                   {
                       Destroy(lastNode.platformObject);
                   }
                   mainPathNodes.RemoveAt(lastNodeIndex);
               }

               // take current pos to correct node
               currentPos = mainPathNodes[mainPathNodes.Count - 1].platformObject.transform.position;

               // change direction to stop deadends
               currentAngle = (currentAngle+Random.Range(60f, 150f)) % 360f;

               //reset retry counter
               retry = 0;
               i -= removeCount;
               continue;
           }
           
           if (PlatCoverCheck(boundMiddle, boundsize, castDistance)|| BouncyCoverCheck(boundMiddle, boundsize, castDistance + 20f))
           {
               if (nextPlat != null)
               {
                   Destroy(nextPlat);
                   nextPlat = null;
               }
               i--;
               retry++;
               Debug.Log(retry+" retries");
               continue;
           }
           
           
           PlatformNode parentNode = mainPathNodes[mainPathNodes.Count - 1];
           PlatformNode newNode = new PlatformNode(parentNode, nextPlat, parentNode.depth+1 , platformType);
           mainPathNodes.Add(newNode);
           currentPos = nextPlat.transform.position;
           retry = 0;
           
           maxHeight= nextPlat.transform.position.y;
           if (platformType == PlatformType.Bouncy)
           {
               float randAngleChange = Random.Range(130, 220);
               currentAngle = (currentAngle + randAngleChange) % 360;
               if (currentAngle < 0) currentAngle += 360;
           }
        }

        retry = 0;
        finalPlatGO = mainPathNodes[mainPathNodes.Count - 1].platformObject;
    }

    private void ChooseBranches()
    {
        int lastBranch=0;
        for (int i = 0; i < mainPathNodes.Count; i++)
        {
            bool isNormalPlat = mainPathNodes[i].platType == PlatformType.Normal;
            if (isNormalPlat&& Random.Range(0, 15) == 1 && i<60&& i-lastBranch>4)
            {
                mainPathNodes[i].platformObject.GetComponent<Renderer>().material.color = Color.teal;
                lastBranch = i;
            }
        }
    }

    private void GenerateBranch(PlatformNode startNode, float startAngle, int maxPlats)
    {
        int generatedplats = 0;
        while (generatedplats < maxPlats)
        {
            
            
            generatedplats++;
        }
    }
    
    
    bool PlatCoverCheck(Vector3 boundMiddle, Vector3 boundSize, float castDistance)
    {
        return (Physics.BoxCast(
            boundMiddle,
            boundSize,
            Vector3.down,
            out RaycastHit hit,
            Quaternion.identity,
            castDistance,
            platLayerMask
        ));
    }

    bool BouncyCoverCheck(Vector3 boundMiddle, Vector3 boundSize, float castDistance)
    {
        if (Physics.BoxCast(
                boundMiddle,
                boundSize,
                Vector3.down,
                out RaycastHit hit,
                Quaternion.identity,
                castDistance,
                platLayerMask
            ))
        {
            return (hit.collider != null && hit.collider.gameObject.CompareTag("BouncyPlatform"));
        }
        return false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseUI.ResumeGame();
        saveData = SaveData.instance; 
        Transform mainCam = Camera.main.transform;
        
        if (saveData != null) 
        {
            seed = saveData.GetSeed();
            Random.InitState(seed);
            GenerateLevel();
            ChooseBranches();
            playerPosition.position = saveData.GetPlayerCordinates();
            mainCam.position = saveData.GetCameraCordinates();
            mainCam.rotation = saveData.GetCameraRotation();
            CameraScript camScript = Camera.main.GetComponent<CameraScript>();
            if (camScript != null)
            {
                camScript.SetCameraAngles(saveData.GetCamX(), saveData.GetCamY());
            }
        }
        else
        {
            Debug.Log("your stupid code doesn't work");
        }
        

    }
    
}
