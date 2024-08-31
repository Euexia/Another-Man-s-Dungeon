using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapFolderController : NetworkBehaviour
{
    [ClientRpc]
    public void RpcSetName(string newName)
    {
        gameObject.name = newName;
    }
}
