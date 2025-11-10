using System;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PlatformGeneration : MonoBehaviour
{
    //possition of current platform during mainpath generation
    public Vector3 currentPos = Vector3.zero;
    
    public Transform playerPosition;
    
    
    public float NumPlats=100f;//main path
    public float MinSize=10f;
    public float MaxSize=50f;
    //range of angles when generating next platform in main path. 
    public float MinAngle;
    public float MaxAngle;
    
    public float MinYchange = 0.2f;
    public float MaxYchange = 2f;
    public float MinXzDistance = 3f;
    public float MaxXzDistance = 6f;
    //platform layer
    public LayerMask platLayerMask;
    
    public int seed;

    public int minBranchLength;
    public int maxBranchLength;
    public int maxBranchStepRetries; 
    public int maxBranchAttempts;
    //stores each branch as a list of platform nodes inside a list 
    private List<List<PlatformNode>> branches = new List<List<PlatformNode>>();
    
    //the bottom and top of the level 
    public static float minHeight;
    public static float maxHeight;
    
    
    public PauseUI pauseUI;
    
    private SaveData saveData;
    
    private float currentAngle = 0f;
    
    public GameObject finalPlatGO;
    
    private List<PlatformNode> mainPathNodes;
    

    public void GenerateLevel()
    {
        branches.Clear();
        GenerateMainPath();
        ChooseBranches();
    }
    
    private void GenerateMainPath()
    {

        int retry = 0;
        
        mainPathNodes = new List<PlatformNode>();
        
        //create first platform and give it layer,shape
        GameObject firstPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstPlatform.transform.position = new Vector3(0, 0f, 0);
        firstPlatform.layer = 7;
        
        Platform firstPlatformComponent = firstPlatform.AddComponent<Platform>();
        firstPlatformComponent.CreatePlatform(PlatformType.Normal, new Vector3(5f, 0.2f, 5f));
        
        //wrap first platform in platformnode without parent and 0 depth 
        PlatformNode firstPlat = new PlatformNode(null, firstPlatform, 0, PlatformType.Normal);
        mainPathNodes.Add(firstPlat);

        currentPos = firstPlatform.transform.position;
        minHeight = currentPos.y;
        
        //clamp and convert to int
        int totalPlats = Mathf.Max(1, Mathf.RoundToInt(NumPlats));
        
        int lastIndex  = totalPlats - 1;    
        
        for (int i = 0; i < totalPlats; i++)
        {
            //angle of next platform 
            float randomOffset = Random.Range(MinAngle, MaxAngle); 
            float RandomAngle = currentAngle + randomOffset; 
            
            //platform size+distance
            float RandomX = Random.Range(MinSize, MaxSize); 
           float RandomZ = Random.Range(MinSize, MaxSize); 
           float XZdistance = Random.Range(MinXzDistance, MaxXzDistance);
           float yChange = Random.Range(MinYchange, MaxYchange);
           
           //angle direction to vector
           Vector3 direction = new Vector3(Mathf.Cos(RandomAngle*Mathf.Deg2Rad), 0, Mathf.Sin(RandomAngle * Mathf.Deg2Rad));
           Vector3 horizontalOffset = direction.normalized * XZdistance;
           //calculates the position of the next platform
           Vector3 nextPos = currentPos + horizontalOffset + new Vector3(0, yChange, 0);
           //if platform is bouncy then nextplatform is higher up
           if ((i - 1) % 20 == 0 && i != 1)
           {
               nextPos.y += 20f;
           }
           //creates next platform object 
           GameObject nextPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
           nextPlat.layer = 7;
           nextPlat.transform.position = nextPos;
           
           PlatformType platformType;
           
           if (i % 20 == 0 && i != 0)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.blue;
               platformType = PlatformType.Bouncy;
           }

           else if (Random.Range(1, 10) == 2)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.cyan;
               platformType = PlatformType.slippery;
           }

           else if (i == lastIndex)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.gold;
               platformType = PlatformType.Win;
           }
           else
           {
               platformType = PlatformType.Normal;
           }
           Platform platformComponent = nextPlat.AddComponent<Platform>();
           
           
           platformComponent.CreatePlatform(platformType, new Vector3(RandomX, 0.2f, RandomZ));
           
           
           Bounds bound = nextPlat.GetComponent<Renderer>().bounds;
           
           Vector3 boundMiddle = nextPlat.GetComponent<Renderer>().bounds.center;
           Vector3 boundsize = new Vector3(bound.extents.x+0.6f, bound.extents.y, bound.extents.z+0.6f);
           float castDistance = bound.extents.y + 4f;
           
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
               if (i < 0)
               {
                   i = -1;
               }  
               continue;
           }
           
           if (BouncyCoverCheck(boundMiddle, boundsize, castDistance + 20f) || PlatCoverCheck(boundMiddle, boundsize, castDistance))
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
                
                float startAngle;                                                                               
                if (i > 0)                                                                                      
                {    
                    //direction towards from last plat into this plat
                    Vector3 incoming = mainPathNodes[i].platformObject.transform.position - mainPathNodes[i - 1].platformObject.transform.position;                  
                    startAngle = Mathf.Atan2(incoming.z, incoming.x) * Mathf.Rad2Deg;                           
                }                                                                                               
                else                                                                                            
                {            
                    //if first platform point to final platform
                    Vector3 toFinal = finalPlatGO.transform.position - mainPathNodes[i].platformObject.transform.position;                       
                    startAngle = Mathf.Atan2(toFinal.z, toFinal.x) * Mathf.Rad2Deg;                       
                }                                                                                            
                int steps = Random.Range(minBranchLength, maxBranchLength + 1);
                GenerateBranch(mainPathNodes[i], startAngle);
                lastBranch = i;
            }
        }
    }

private void GenerateBranch(PlatformNode startNode, float startAngle) 
{
    Vector3 targetPos = finalPlatGO.transform.position;
    int maxBranchSteps = 300; 

    //loop for generting branch
    for (int attempt = 0; attempt < maxBranchAttempts; attempt++)
    {
        Vector3 current = startNode.platformObject.transform.position;
        float branchAngle = startAngle;                               

        List<PlatformNode> branchNodes = new List<PlatformNode>();    

        int retry = 0; 

        //loop for gerating step
        for (int i = 0; i < maxBranchSteps; i++) 
        {
   
            //usees how high up in the level we are to change how much the generation is directed to the end
            float branchProg = (i + 1f) / maxBranchSteps; 
            float quadraticProg = branchProg * branchProg * 1.2f;

            Vector3 target = targetPos - current;
            target.y = 0f;
            float targetXZDegree = Mathf.Atan2(target.z, target.x) * Mathf.Rad2Deg;
            //how strongly to steer towards last platform
            float steer = 0.05f + 0.15f * quadraticProg;
            //makes distance smooth
            targetXZDegree = Mathf.LerpAngle(branchAngle, targetXZDegree, steer);

            
            Vector3 toFinalXZ = new Vector3(targetPos.x - current.x, 0f, targetPos.z - current.z);
            float distXZ = toFinalXZ.magnitude;
            
            //distance to just generate dirrecaly to end
            float snapXZ = MaxXzDistance * 1.75f;
            //distance for very far to medium
            float nearXZ = MaxXzDistance * 6f;
            //makes it so the start of the branch is more 'free' and the end is more 'scrict' 
            float narrow = Mathf.Lerp(70f, 8f, branchProg);
            // Scale angle from 5 to narrow, prevent div 0 error
            float localNarrow = Mathf.Lerp(5f, narrow, Mathf.Clamp01((distXZ - snapXZ) / Mathf.Max(nearXZ - snapXZ, 0.0001f)));
            
            float xzDistance = Random.Range(MinXzDistance, MaxXzDistance);
            //used to changed plat hieght 
            float platsRemaining = Mathf.Max(1, maxBranchSteps - i);
            //calculates y platoform change
            float targetYDistance = Mathf.Clamp((targetPos.y - current.y) / platsRemaining, -MaxYchange, MaxYchange);
            float randomY = Random.Range(MinYchange, MaxYchange);
            float yDistance = Mathf.Lerp(randomY, targetYDistance, branchProg);

            float yMargin = 0.25f * MaxYchange;
            float remainingY = targetPos.y - current.y;
            if (distXZ > nearXZ) yDistance *= 0.4f;
            if (remainingY > 0f && yDistance > remainingY - yMargin) yDistance = Mathf.Max(remainingY - yMargin, 0f);
            if (remainingY < 0f && yDistance < remainingY + yMargin) yDistance = Mathf.Min(remainingY + yMargin, 0f);

            bool placed = false;
            float finalXZ = targetXZDegree;
            GameObject nextPlat = null;

            int attempts = Mathf.Max(1, maxBranchStepRetries);

            for (int j = 0; j < attempts; j++)
            {
                float xzAngle;
                xzAngle = Random.Range(0f, 360f);
                if (distXZ <= snapXZ)
                {
                    xzAngle = targetXZDegree;
                    xzDistance = Mathf.Min(xzDistance, distXZ);
                }
                else
                {
                    xzAngle = targetXZDegree + Random.Range(-localNarrow, localNarrow);
                }

                Vector3 dir = new Vector3(Mathf.Cos(xzAngle * Mathf.Deg2Rad), 0f, Mathf.Sin(xzAngle * Mathf.Deg2Rad)).normalized;
                Vector3 platPosFinal = current + dir * xzDistance + new Vector3(0f, yDistance, 0f);

               // if (platPosFinal.y > topCut) continue;

                if (nextPlat == null)
                {
                    nextPlat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    nextPlat.layer = 0;
                    Platform platformComponent = nextPlat.AddComponent<Platform>();
                    Vector3 platSize = new Vector3(Random.Range(MinSize, MaxSize), 0.2f, Random.Range(MinSize, MaxSize));
                    platformComponent.CreatePlatform(PlatformType.Normal, platSize);
                }

                nextPlat.transform.position = platPosFinal;
                Physics.SyncTransforms();

                Bounds bound = nextPlat.GetComponent<Renderer>().bounds;
                Vector3 boundMiddle = bound.center;
                Vector3 boundsize = new Vector3(bound.extents.x + 0.4f, bound.extents.y, bound.extents.z + 0.4f);
                float castDistance = bound.extents.y + 2f;

                Collider col = nextPlat.GetComponent<Collider>();
                bool prevEnabled = false;
                if (col != null)
                {
                    prevEnabled = col.enabled;
                    col.enabled = false;
                }
                bool blocked = BouncyCoverCheck(boundMiddle, boundsize, castDistance + 20f) || PlatCoverCheck(boundMiddle, boundsize, castDistance);
                if (col != null) col.enabled = prevEnabled;

                if (!blocked)
                {
                    placed = true;
                    finalXZ = xzAngle;
                    break;
                }
            }

            if (!placed)
            {
                if (nextPlat != null) Destroy(nextPlat);

                if (retry >= 200)
                {
                    int removeCount = Mathf.Min(5, Mathf.Max(0, branchNodes.Count)); 
                    for (int r = 0; r < removeCount; r++) 
                    {
                        int lastIdx = branchNodes.Count - 1; 
                        PlatformNode lastNode = branchNodes[lastIdx]; 
                        if (lastNode != null && lastNode.platformObject != null) Destroy(lastNode.platformObject);
                        branchNodes.RemoveAt(lastIdx); 
                    }

                    if (branchNodes.Count > 0) 
                    {
                        current = branchNodes[branchNodes.Count - 1].platformObject.transform.position; 
                    }
                    else
                    {
                        current = startNode.platformObject.transform.position; 
                    }

                    branchAngle = (branchAngle + Random.Range(60f, 150f)) % 360f; 
                    retry = 0; 

                    i -= removeCount;
                    if (i < 0) i = -1; 
                    continue; 
                }

                retry++;
                i--;     
                branchAngle = (branchAngle + Random.Range(20f, 45f)) % 360f; 
                continue; 
            }

            nextPlat.layer = 7;

            PlatformNode newNode = new PlatformNode(startNode, nextPlat, startNode.depth + i + 1, PlatformType.Normal);
            branchNodes.Add(newNode);
            branchAngle = finalXZ;
            current = nextPlat.transform.position;

            retry = 0; 

            Vector3 xzDistToFinal = new Vector3(targetPos.x - current.x, 0f, targetPos.z - current.z);
            float distXZAfter = xzDistToFinal.magnitude;
            float yGap = Mathf.Abs(targetPos.y - current.y);

            if (distXZAfter <= MaxXzDistance * 1.15f && yGap <= MaxYchange * 1.5f)
            {
                break; 
            }
        }

        bool reached = false; 
        if (branchNodes.Count > 0) 
        {
            PlatformNode tail = branchNodes[branchNodes.Count - 1]; 
            if (tail != null && tail.platformObject != null) 
            {
                Vector3 tailPos = tail.platformObject.transform.position; 
                Vector3 tailXZ = new Vector3(targetPos.x - tailPos.x, 0f, targetPos.z - tailPos.z);
                float distXZAfter = tailXZ.magnitude; 
                float yGap = Mathf.Abs(targetPos.y - tailPos.y); 

                if (distXZAfter <= MaxXzDistance * 1.15f && yGap <= MaxYchange * 1.5f) 
                {
                    reached = true; 
                    Renderer r = tail.platformObject.GetComponent<Renderer>();
                    if (r != null) r.material.color = Color.red; 
                    branches.Add(branchNodes);
                    return; 
                }
            }
        }

        // if not reached, destroy this attempt's platforms and try again if attempts remain 
        for (int k = 0; k < branchNodes.Count; k++) 
        {
            PlatformNode n = branchNodes[k];
            if (n != null && n.platformObject != null) Destroy(n.platformObject); 
        }
    }
}
    
    bool PlatCoverCheck(Vector3 boundMiddle, Vector3 boundSize, float castDistance)
    {
        if (Physics.BoxCast(boundMiddle, boundSize, Vector3.down, out RaycastHit hit, Quaternion.identity, castDistance, platLayerMask))
        {
            //dont hit self
            if (hit.collider.gameObject == null || hit.collider.gameObject == this)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    bool BouncyCoverCheck(Vector3 boundMiddle, Vector3 boundSize, float castDistance)
    {
        if (Physics.BoxCast(boundMiddle, boundSize, Vector3.down, out RaycastHit hit, Quaternion.identity, castDistance, platLayerMask))
        {
            if (hit.collider != null && hit.collider.CompareTag("BouncyPlatform"))
            {
                return true;
            }
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