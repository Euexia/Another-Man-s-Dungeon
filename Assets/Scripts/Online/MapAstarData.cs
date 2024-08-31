using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GraphData", menuName = "Scriptable Objects/Custom Graph")]
public class MapAstarData : ScriptableObject
{
    public string mapName;

    public int width;
    public int depth;
    public float nodeSize;
    public Vector3 center;
    public Vector3 rotation;
    public bool cutCorners;
    public float maxClimb;
    public float maxSlope;
    public Pathfinding.ColliderType collisionType;
    public LayerMask collisionMask;
    public LayerMask collisionHeightMask;
    public float rayLength;
}
