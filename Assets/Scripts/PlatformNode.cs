using UnityEngine;
using System.Collections.Generic;

public class PlatformNode
{
    public Vector3 position;
    public PlatformNode parent;
    public GameObject platformObject;
    public int depth;
    public PlatformType platType;
    public List<PlatformNode> allChildren;

    public PlatformNode(PlatformNode parent, GameObject platformObject, int depth, PlatformType platType )//constructor 
    {
        this.parent = parent;
        this.platformObject = platformObject;
        this.depth = depth;
        this.platType = platType;
        this.allChildren = new List<PlatformNode>();
    }
}
