using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : NetworkBehaviour
{
    [SyncVar]
    public bool isPicked;

    public ItemSO itemScriptableObject;
}
