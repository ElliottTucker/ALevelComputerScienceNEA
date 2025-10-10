using UnityEngine;

public class Platform : MonoBehaviour
{
    public void CreatePlatform(PlatformType platformType, Vector3 size)
    {
        transform.localScale = size;
        
        Renderer platformRenderer = gameObject.GetComponent<Renderer>();
        switch (platformType)
        {
            case PlatformType.Normal:
                gameObject.tag = "Platform";
                break;
            case PlatformType.Bouncy:
                gameObject.tag = "BouncyPlatform";
                break;
            case PlatformType.Win:
                gameObject.tag = "WinPlatform";
                break;
        }
    }
}
