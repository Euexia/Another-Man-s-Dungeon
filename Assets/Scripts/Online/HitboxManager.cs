using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxManager : NetworkBehaviour
{
    public NetworkIdentity plrIdentity;
    private SyncList<GameObject> debounce = new SyncList<GameObject>();

    [SerializeField] private AudioClip hitSound;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other && (other.tag == "Enemy" || (other.tag == "Player" && other.GetComponent<NetworkIdentity>().netId != plrIdentity.netId)))
        {
            if (debounce.Find((x) => x == other.gameObject))
            {
                Debug.Log(other.gameObject.name + " already in list");
                return;
            }

            AddToDebounceCmd(other.gameObject);

            if (other.tag == "Enemy")
            {
                MonsterController monsterHealth = other.GetComponent<MonsterController>();
                monsterHealth.TakeDamage(10);

                PlayHitSound(other.gameObject);
            }
            else if (other.tag == "Player")
            {
                PlayerMovementController playerHealth = other.GetComponent<PlayerMovementController>();
                playerHealth.TakeDamage(other.gameObject, 10);

                PlayHitSound(other.gameObject);
            }

            //monsterHealth.LocalUpdateHealthBar();
        }
    }

    [Command]
    void AddToDebounceCmd(GameObject target)
    {
        Debug.Log("addztatzat");
        debounce.Add(target);
    }

    private void PlayHitSound(GameObject enemy)
    {
        AudioSource audioSource = enemy.GetComponent<AudioSource>();

        if (audioSource != null && hitSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(hitSound);
        }
    }
}
