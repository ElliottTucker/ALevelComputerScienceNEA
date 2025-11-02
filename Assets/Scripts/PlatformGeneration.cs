using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

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

    public int minBranchLength;
    public int maxBranchLength;
    public int maxBranchRetries; 
    private List<List<PlatformNode>> branches = new List<List<PlatformNode>>();
    

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
        
        GameObject firstPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        firstPlatform.transform.position = new Vector3(0, 0f, 0);
        firstPlatform.layer = 7;
        
        Platform firstPlatformComponent = firstPlatform.AddComponent<Platform>();
        firstPlatformComponent.CreatePlatform(PlatformType.Normal, new Vector3(5f, 0.2f, 5f));
        

        
        PlatformNode firstPlat = new PlatformNode(null, firstPlatform, 0, PlatformType.Normal);
        mainPathNodes.Add(firstPlat);

        currentPos = firstPlatform.transform.position;
        minHeight = currentPos.y;
        
        int totalPlats = Mathf.Max(1, Mathf.RoundToInt(NumPlats));
        int lastIndex  = totalPlats - 1;    
        
        for (int i = 0; i < totalPlats; i++)
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

           if (i == lastIndex)
           {
               nextPlat.GetComponent<Renderer>().material.color = Color.gold;
           }
           Platform platformComponent = nextPlat.AddComponent<Platform>();
           PlatformType platformType = PlatformType.Normal;

           if (i % 20 == 0 && i != 0)
           {
               platformType = PlatformType.Bouncy;
           }

           if (i == lastIndex)
           {
               platformType = PlatformType.Win;
           }
           
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
                    Vector3 incoming = mainPathNodes[i].platformObject.transform.position - mainPathNodes[i - 1].platformObject.transform.position;                  
                    startAngle = Mathf.Atan2(incoming.z, incoming.x) * Mathf.Rad2Deg;                           
                }                                                                                               
                else                                                                                            
                {                                                                                               
                    Vector3 toFinal = finalPlatGO.transform.position - mainPathNodes[i].platformObject.transform.position;                       
                    startAngle = Mathf.Atan2(toFinal.z, toFinal.x) * Mathf.Rad2Deg;                       
                }                                                                                            
                int steps = Random.Range(minBranchLength, maxBranchLength + 1);                                 
                GenerateBranch(mainPathNodes[i], startAngle, steps); 
                lastBranch = i;
            }
        }
    }

    private void GenerateBranch(PlatformNode startNode, float startAngle, int maxPlats)
{
    Vector3 targetPos = finalPlatGO.transform.position;
    Vector3 current = startNode.platformObject.transform.position;
    float branchAngle = startAngle;

    float topCut = minHeight + 0.8f * (maxHeight - minHeight);

    List<PlatformNode> branchNodes = new List<PlatformNode>();
    int retry = 0;                                     // ADDED: branch-level retry counter like main path

    for (int i = 0; i < maxPlats; i++)
    {
        if (current.y >= topCut) break;

        float branchProg = (i + 1f) / maxPlats;
        float quadraticProg = branchProg * branchProg * 1.2f;

        Vector3 target = targetPos - current;
        target.y = 0f;
        float targetXZDegree = Mathf.Atan2(target.z, target.x) * Mathf.Rad2Deg;

        float steer = 0.05f + 0.15f * quadraticProg;
        targetXZDegree = Mathf.LerpAngle(branchAngle, targetXZDegree, steer);

        Vector3 toFinalXZ = new Vector3(targetPos.x - current.x, 0f, targetPos.z - current.z);
        float distXZ = toFinalXZ.magnitude;
        float snapXZ = MaxXzDistance * 1.75f;
        float nearXZ = MaxXzDistance * 6f;

        float narrow = Mathf.Lerp(70f, 8f, branchProg);
        float localNarrow = Mathf.Lerp(5f, narrow, Mathf.Clamp01((distXZ - snapXZ) / Mathf.Max(nearXZ - snapXZ, 0.0001f)));

        float xzDistance = Random.Range(MinXzDistance, MaxXzDistance);
        float platsRemaining = Mathf.Max(1, maxPlats - i);

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

        int attempts = Mathf.Max(1, maxBranchRetries);
        for (int j = 0; j < attempts; j++)
        {
            float xzAngle;
            if (distXZ <= snapXZ)                         // same idea as main: snap when close
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

            if (platPosFinal.y > topCut) continue;        // keep under the cap

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
            if (col != null) { prevEnabled = col.enabled; col.enabled = false; }
            bool blocked = BouncyCoverCheck(boundMiddle, boundsize, castDistance + 20f) || PlatCoverCheck(boundMiddle, boundsize, 2f);
            if (col != null) { col.enabled = prevEnabled; }

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

            i--;                          // ADDED: retry this same step with new randoms
            retry++;                      // ADDED: count consecutive failures

            if (retry >= 2)             // ADDED: backtrack like main path
            {
                int removeCount = Mathf.Min(5, Mathf.Max(0, branchNodes.Count));
                for (int r = 0; r < removeCount; r++)
                {
                    int last = branchNodes.Count - 1;
                    PlatformNode lastNode = branchNodes[last];
                    if (lastNode != null && lastNode.platformObject != null) Destroy(lastNode.platformObject);
                    branchNodes.RemoveAt(last);
                }

                if (branchNodes.Count > 0)
                {
                    current = branchNodes[branchNodes.Count - 1].platformObject.transform.position;
                }
                else
                {
                    current = startNode.platformObject.transform.position;
                }

                branchAngle = (branchAngle + Random.Range(60f, 150f)) % 360f;   // ADDED: turn to escape dead end
                retry = 0;                                                      // ADDED
                i -= removeCount; if (i < 0) i = -1;                            // ADDED: align loop index
            }
            continue;                      // ADDED: go try again
        }

        nextPlat.layer = 7;

        PlatformNode newNode = new PlatformNode(startNode, nextPlat, startNode.depth + i + 1, PlatformType.Normal);
        branchNodes.Add(newNode);
        branchAngle = finalXZ;
        current = nextPlat.transform.position;
        retry = 0;                         // ADDED: reset on success

        Vector3 xzDistToFinal = new Vector3(targetPos.x - current.x, 0f, targetPos.z - current.z);
        float distXZAfter = xzDistToFinal.magnitude;
        float yGap = Mathf.Abs(targetPos.y - current.y);
        if (distXZAfter <= MaxXzDistance * 1.15f && yGap <= MaxYchange * 1.5f)
        {
            break;
        }
    }

    if (branchNodes.Count > 0) branches.Add(branchNodes);
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