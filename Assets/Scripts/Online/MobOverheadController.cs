using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobOverheadController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isClient) return;

        Vector3 position = gameObject.transform.position;
        Vector3 target = NetworkClient.localPlayer.gameObject.transform.position;
        Vector3 inverseHeight = new Vector3(0, (position.y - target.y) * 2, 0);
        gameObject.transform.LookAt(2 * (position + inverseHeight) - target);
    }
}
