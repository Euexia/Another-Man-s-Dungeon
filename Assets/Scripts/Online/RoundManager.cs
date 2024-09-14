using Mirror;
using Newtonsoft.Json.Linq;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static RootMotion.FinalIK.GenericPoser;
using static RootMotion.FinalIK.RagdollUtility;

public class RoundManager : NetworkBehaviour
{
    [Serializable]
    public class MapData
    {
        public string name;
        public GameObject gameObject;
    }

    [SerializeField] private GameObject mapFolderPrefab;

    [SerializeField] private AstarPath astarPath;

    [Header("GAME SETTINGS")]

    [SerializeField] private int rounds = 5;

    [SerializeField] private int maxRoundTimer = 30;

    [SerializeField] private List<MapData> maps;

    [SerializeField] private List<MapData> maps1v1;

    [SerializeField] private int mapSpacing = 10000;

    [Header("GAME PROGRESSION (DO NOT TOUCH)")]

    [SyncVar]
    private string currentMap = "";

    [SyncVar]
    private int currentRound = 0;

    [SyncVar]
    private int secondsLeft = 0;

    [SyncVar]
    private int playerThroughPortal = 0;

    [SyncVar]
    private int playersAlive = 0;

    private GameObject current1v1map;

    public readonly SyncDictionary<int, GameObject> playerMapFolders = new SyncDictionary<int, GameObject>();

    // Start is called before the first frame update

    private void Awake()
    {
        
    }

    void Start()
    {
        StartCoroutine(HandleRounds());
    }

    // Update is called once per frame
    void Update()
    {

    }

    MapData SelectRandomMap()
    {
        MapData chosenMap = maps[UnityEngine.Random.Range(0, maps.Count)];
        return chosenMap;
    }

    MapData SelectRandom1v1Map()
    {
        MapData chosenMap = maps1v1[UnityEngine.Random.Range(0, maps1v1.Count)];
        return chosenMap;
    }

    [Server]
    IEnumerator HandleRounds()
    {
        for (int i = 5; i > 3; i--)
        {
            secondsLeft = i;
            RpcUpdateStatus();
            yield return new WaitForSeconds(1);
        }

        // Creation des dossiers parent pour les map des joueurs

        foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
        {
            NetworkConnectionToClient conn = entry.Value;

            GameObject newFolder = Instantiate(mapFolderPrefab);
            newFolder.transform.position = new Vector3(0, 0, conn.identity.netId * mapSpacing + mapSpacing);
            //newFolder.name = "Map" + entry.Key.ToString();

            NetworkServer.Spawn(newFolder, conn);

            playerMapFolders.Add((int)conn.identity.netId, newFolder);

            //RpcSetMapName(newFolder, "Map" + entry.Key.ToString());
        }

        for (int i = 3; i >= 1; i--)
        {
            secondsLeft = i;
            RpcUpdateStatus();
            yield return new WaitForSeconds(1);
        }

        yield return new WaitForSeconds(1);

        while (currentRound <= rounds)
        {
            if (secondsLeft > 0 && playersAlive > 0)
            {
                if (((currentRound == 3 || currentRound == 5) && NetworkManager.singleton.numPlayers > 1 && playersAlive == 1) || playersAlive == 0)
                {
                    secondsLeft = 0;
                }
                else
                {
                    secondsLeft -= 1;
                }
                RpcUpdateStatus();
            }
            else
            {
                // Remise a zero des variables

                currentRound += 1;

                if (currentRound > rounds) break;

                playerThroughPortal = 0;
                playersAlive = 0;
                secondsLeft = maxRoundTimer;

                // Suppression d'une map 1v1 si elle existe
                if (current1v1map != null)
                {
                    NetworkServer.UnSpawn(current1v1map);
                    current1v1map = null;
                }

                // Si la manche doit etre un 1v1 s'il y a plus d'un joueur
                if ((currentRound == 3 || currentRound == 5) && NetworkManager.singleton.numPlayers > 1)
                {
                    MapData chosenMap = SelectRandom1v1Map();
                    RpcStartLoadingScreen(true, chosenMap.name);

                    Debug.Log("1V1 START");

                    yield return new WaitForSeconds(0.1f);

                    GameObject NewMap = Instantiate(chosenMap.gameObject);
                    NewMap.isStatic = true;
                    NetworkServer.Spawn(NewMap);

                    current1v1map = NewMap;

                    foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
                    {
                        NetworkConnectionToClient conn = entry.Value;
                        GameObject player = conn.identity.gameObject;

                        Transform mapFolder = playerMapFolders[(int)conn.identity.netId].transform;

                        foreach (Transform child in mapFolder)
                        {
                            if (mapFolder != null)
                            {
                                NetworkServer.Destroy(child.gameObject);
                            }
                        }

                        PlayerMovementController plrData = player.GetComponent<PlayerMovementController>();
                        plrData.SetHealth(plrData.GetMaxHealth());

                        string portalName = "Portal_Player" + (1 + (int)conn.identity.netId % 2).ToString();
                        Debug.Log("Player id: " + conn.identity.netId.ToString() + ". portal name = " + portalName);
                        //player.transform.position = NewMap.transform.Find(portalName).position;

                        RpcTeleportToSpawn(conn, NewMap, portalName);

                        playersAlive += 1;
                    }
                }
                // Génération d'un donjon
                else
                {
                    // Choix de la map

                    MapData chosenMap = SelectRandomMap();
                    RpcStartLoadingScreen(true, chosenMap.name);

                    // Suppression de tous les graphs

                    foreach (KeyValuePair<int, GameObject> mapToRemove in playerMapFolders)
                    {
                        foreach (Transform child in mapToRemove.Value.transform)
                        {
                            if (child != null)
                            {
                                NetworkServer.Destroy(child.gameObject);
                            }
                        }
                    }

                    yield return new WaitForSeconds(0.1f);

                    // Itération à travers chaque joueur pour instancier sa map, la déplacer localement, puis téléporter le joueur sur sa map en le remettant en vie

                    foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
                    {
                        NetworkConnectionToClient conn = entry.Value;
                        GameObject player = conn.identity.gameObject;

                        //Transform mapFolder = GameObject.Find("Map" + entry.Key.ToString()).transform;
                        Transform mapFolder = playerMapFolders[(int)conn.identity.netId].transform;
                        GameObject NewMap = Instantiate(chosenMap.gameObject, mapFolder);
                        NewMap.isStatic = true;

                        //NewMap.transform.SetParent(mapFolder);
                        //NewMap.transform.position = mapFolder.transform.position;

                        NetworkServer.Spawn(NewMap);

                        RpcSwitchMap(NewMap, mapFolder);
                        RpcTeleportToSpawn(conn, NewMap, "PortalStart");

                        yield return new WaitForSeconds(1);

                        astarPath.gameObject.GetComponent<MapAstarManager>().CreateGraph(chosenMap.name, mapFolder.transform.position, true);
                        //RpcScanGraph(NewMap);
                        Debug.Log("map scannée");

                        PlayerMovementController plrData = player.GetComponent<PlayerMovementController>();
                        plrData.SetHealth(plrData.GetMaxHealth());

                        playersAlive += 1;
                    }
                }

                RpcStartLoadingScreen(false, "");
                RpcUpdateStatus();

                playerThroughPortal = 0;
            }

            yield return new WaitForSeconds(1);
        }

        currentRound = -2;
        RpcUpdateStatus();

        yield return new WaitForSeconds(5);

        RpcStartLoadingScreen(true, "");

        yield return new WaitForSeconds(1);

        EndGameAndReturnToLobby();
    }

    private void CreateNewGraph(GameObject NewMap, Transform mapFolder)
    {
        AstarPath astarReference = NewMap.transform.Find("Astar").GetComponent<AstarPath>();
        GridGraph graphReference = astarReference.graphs[0] as GridGraph;

        GridGraph newGraph = AstarPath.active.data.AddGraph(typeof(GridGraph)) as GridGraph;
        newGraph.center = graphReference.center;
        newGraph.nodeSize = graphReference.nodeSize;
        newGraph.cutCorners = graphReference.cutCorners;
        newGraph.collision.type = graphReference.collision.type;
        newGraph.collision.mask = graphReference.collision.mask;
        newGraph.maxSlope = graphReference.maxSlope;
        newGraph.maxClimb = graphReference.maxClimb;
        newGraph.collision.heightMask = graphReference.collision.heightMask;
        newGraph.collision.fromHeight = graphReference.collision.fromHeight;
        newGraph.SetDimensions(graphReference.width, graphReference.depth, graphReference.nodeSize);

        NewMap.transform.Find("Astar").gameObject.SetActive(false);

        Debug.Log("reference pos: " + graphReference.center.ToString() + "; newmap pos: " + NewMap.transform.position.ToString() + "; mapFolder pos: " + mapFolder.position.ToString());

        AstarPath.active.Scan(newGraph);
    }

    private void TeleportToPortal(Transform player, GameObject map, string name)
    {
        Transform PortalStart = map.transform.Find(name);
        if (PortalStart)
        {
            player.position = PortalStart.transform.position;
        }
        else
        {
            Debug.LogWarning("No SpawnLocation found!");
            player.position = map.transform.position;
        }
    }

    [Server]
    public void EndGameAndReturnToLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }

        NetworkManager.singleton.ServerChangeScene("LobbyScene");
    }

    [Command(requiresAuthority = false)]
    public void CmdInvadeWorld(GameObject portal, GameObject player)
    {
        //Debug.Log("received portal request on server");
        playerThroughPortal += 1;

        //Debug.Log(playerThroughPortal);
        //Debug.Log(NetworkManager.singleton.numPlayers);

        if (playerThroughPortal == NetworkManager.singleton.numPlayers)
        {
            secondsLeft = 0;
        }
        else
        {
            NetworkIdentity playerIdentity = player.GetComponent<NetworkIdentity>();

            foreach (KeyValuePair<int, NetworkConnectionToClient> entry in NetworkServer.connections)
            {
                NetworkConnectionToClient conn = entry.Value;

                if (playerIdentity.netId != conn.identity.netId)
                {
                    GameObject mapFolder = playerMapFolders[(int)playerIdentity.netId];

                    if (mapFolder != null)
                    {
                        NetworkServer.UnSpawn(mapFolder);
                    }

                    Debug.Log("Teleporting player " + playerIdentity.netId.ToString() + " to map id: " + conn.identity.netId.ToString());

                    GameObject invadedMap = playerMapFolders[(int)conn.identity.netId].transform.GetChild(0).gameObject;

                    RpcTeleportToSpawn(playerIdentity.connectionToClient, invadedMap, "PortalStart");
                    //RpcScanGraph(invadedMap);
                    RpcInvadeWorld(conn);

                    break;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcUpdateStatus()
    {
        NetworkClient.localPlayer.GetComponent<PlayerMovementController>().UpdateStatus(currentRound, secondsLeft);
    }

    [ClientRpc]
    public void RpcSetMapName(GameObject map, string newName)
    {
        map.name = newName;
    }

    [ClientRpc]
    public void RpcSwitchMap(GameObject map, Transform parent)
    {
        Vector3 offset = new Vector3(0, 0, parent.transform.position.z);

        map.transform.parent = parent;
        if (isClient && !isServer)
        {
            map.transform.position += offset;
        }
    }

    [ClientRpc]
    public void RpcScanGraph(GameObject map)
    {
        Debug.Log(map.transform.Find("Astar").gameObject);
        map.transform.Find("Astar").gameObject.SetActive(false);

        if (isServer && isClient)
        {
            CreateNewGraph(map, map.transform.parent);
        };

/*        AstarPath astarPath = map.transform.Find("Astar").GetComponent<AstarPath>();
        GridGraph graph = astarPath.data.gridGraph;

        AstarPath.active.Scan(graph);
        Debug.Log("Initial scan completed.");*/
    }

    [TargetRpc]
    public void RpcTeleportToSpawn(NetworkConnectionToClient target, GameObject map, string portalName)
    {
        if (map == null)
        {
            Debug.LogError("Map is null, cannot teleport!");
            return;
        }

        Debug.Log("teleporting to " + map.name + " with portal name: " + portalName);

        TeleportToPortal(NetworkClient.localPlayer.transform, map, portalName);
    }

    [ClientRpc]
    public void RpcStartLoadingScreen(bool load, string mapName)
    {
        PlayerLoadingScreen playerLoadingScreen = NetworkClient.localPlayer.GetComponent<PlayerLoadingScreen>();
        if (load)
        {
            StartCoroutine(playerLoadingScreen.Load(mapName));
        }
        else
        {
            playerLoadingScreen.Complete();
        }
    }

    [TargetRpc]
    public void RpcInvadeWorld(NetworkConnectionToClient target)
    {
        Debug.Log("Your world is invaded !");
    }

    [Server]
    public void PlayerDied()
    {
        if (isServer)
        {
            playersAlive -= 1;
            Debug.Log("server died");
            Debug.Log("There are " + playersAlive.ToString() + " players remaining.");
        }
    }
}
