using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MapAstarManager : MonoBehaviour
{
    [Serializable]
    public class MapData
    {
        public string mapName;
        public MapAstarData graph;
    }

    public List<MapData> maps;

    public void CreateGraph(string mapName, Vector3 offset, bool scan)
    {
        foreach (MapData map in maps)
        {
            if (map.mapName == mapName)
            {
                AstarPath astarPath = GetComponent<AstarPath>();

                if (astarPath != null)
                {
                    GridGraph newGraph = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
                    newGraph.center = map.graph.center + offset;
                    newGraph.rotation = map.graph.rotation;
                    newGraph.nodeSize = map.graph.nodeSize;
                    newGraph.cutCorners = map.graph.cutCorners;
                    newGraph.collision.type = map.graph.collisionType;
                    newGraph.collision.mask = map.graph.collisionMask;
                    newGraph.maxSlope = map.graph.maxSlope;
                    newGraph.maxClimb = map.graph.maxClimb;
                    newGraph.collision.heightMask = map.graph.collisionHeightMask;
                    newGraph.collision.fromHeight = map.graph.rayLength;
                    newGraph.SetDimensions(map.graph.width, map.graph.depth, map.graph.nodeSize);

                    if (scan)
                    {
                        AstarPath.active.Scan(newGraph);
                    }

                }
            }
        }
    }
}
