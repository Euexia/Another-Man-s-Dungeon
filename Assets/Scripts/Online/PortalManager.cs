using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UIElements;

public class PortalManager : NetworkBehaviour
{
    private RoundManager roundManager;

    [SyncVar]
    private bool canEnter = true;

    // Start is called before the first frame update
    public override void OnStartClient()
    {
        roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canEnter) return;

        if (other.tag == "Player" && gameObject.name == "PortalEnd")
        {
            NetworkIdentity plrIdentity = other.GetComponent<NetworkIdentity>();

            if (plrIdentity)
            {
                other.GetComponent<PlayerLoadingScreen>().PlayPortalSound();
                CmdDisablePortal();
                roundManager.CmdInvadeWorld(gameObject, other.gameObject);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdDisablePortal()
    {
        canEnter = false;
    }
}
