using Org.BouncyCastle.Cms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;

public class CombatController : NetworkBehaviour
{
    bool isClicking = false;
    [SerializeField] private GameObject HitboxPrefab;
    [SerializeField] private Transform ModelRoot;
    [SerializeField] private Transform CameraRoot;
    private Animator PlayerAnimator;
    private int comboCount = 0;
    private Coroutine comboCoroutine;

    // TEMP VALUES
    public string weaponType = "Axe";
    float cooldown = 0.3f;
    float currCooldown = 0;
    public bool isRange = false;
    public int damage = 10;

    [SerializeField] private AudioClip swordSound1;
    [SerializeField] private AudioClip swordSound2;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null )
        {
            audioSource = GetComponent<AudioSource>();
        }
        //PlayerAnimator = ModelRoot.transform.Find("Model").GetComponent<Animator>();
        //SetAnimation(weaponType + "Idle");
    }

    // Update is called once per frame
    void Update()
    {
        if (currCooldown > 0)
        {
            currCooldown = Math.Max(0, currCooldown - Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        SendDebugRaycast();
    }

    private void SetAnimation(string animationName)
    {
        PlayerAnimator.SetTrigger(animationName);
    }

    [Command]
    private void CmdCreateHitbox(NetworkIdentity playerIdentity, Vector3 size)
    {
        GameObject NewHitbox = Instantiate(HitboxPrefab);
        NewHitbox.GetComponent<HitboxManager>().plrIdentity = playerIdentity;
        NewHitbox.transform.position = ModelRoot.position + ModelRoot.forward * 3;
        NewHitbox.transform.rotation = ModelRoot.rotation;
        NewHitbox.transform.localScale = size;
        NetworkServer.Spawn(NewHitbox);
        Destroy(NewHitbox, 0.1f);
    }

    [Command]
    private void CmdDealMonsterDamage(GameObject enemy)
    {
        if (enemy == null) return;

        if (enemy.tag == "Enemy")
        {
            enemy.GetComponent<MonsterController>().TakeDamage(damage);
        } else if (enemy.tag == "Player")
        {
            enemy.GetComponent<PlayerMovementController>().TakeDamage(enemy, damage);
        }
    }

    private void SendDebugRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(ModelRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(ModelRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(ModelRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //Debug.Log("Did not Hit");
        }
    }

    private RaycastHit SendRaycast()
    {

        RaycastHit hit;
        Physics.Raycast(ModelRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);

        return hit;
    }

    public void HandleMouseClick(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name != "OnlineGame") { return; }
        if (!isLocalPlayer) { return; }

        float state = context.action.ReadValue<float>();
        if (isClicking && state == 0)
        {
            isClicking = false;
        }
        else if (!isClicking && state > 0)
        {
            isClicking = true;
        }
        else
        {
            return;
        }

        HandleCombat(isClicking);
    }

    private void ResetCombo()
    {
        comboCount = 0;
        //SetAnimation(weaponType + "Idle");
    }

    private IEnumerator WaitAndResetCombo()
    {
        // Attendre 1 seconde
        yield return new WaitForSeconds(1f);
        ResetCombo();
    }

    private IEnumerator ComboTick()
    {
        //SetAnimation(weaponType + "Attack" + comboCount.ToString());
        yield return new WaitForSeconds(1f);
        ResetCombo();
    }

    private void HandleCombo()
    {
        if (!isLocalPlayer) { return; }

        if (comboCount >= 4)
        {
            return;
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
        }

        comboCount++;

        if (comboCount >= 4)
        {
            comboCoroutine = StartCoroutine(WaitAndResetCombo());
        }
        else
        {
            comboCoroutine = StartCoroutine(ComboTick());
        }

        PlaySwordSound();
        CmdCreateHitbox(NetworkClient.localPlayer, new Vector3(4, 4, 4));

        //CreateHitbox(new Vector3(1, 1, 1));
    }
    private void HandleCombat(bool pressing)
    {
        if (pressing && currCooldown <= 0)
        {
            if (isRange)
            {
                RaycastHit result = SendRaycast();
                if (result.transform != null && (result.transform.tag == "Enemy" || result.transform.tag == "Player"))
                {
                    CmdDealMonsterDamage(result.transform.gameObject);
                };
            }
            else
            {
                HandleCombo();
            }

            currCooldown = cooldown;
        }
    }

    private void PlaySwordSound()
    {
        if (audioSource != null)
        {
            AudioClip chosenClip = UnityEngine.Random.value > 0.5f ? swordSound1 : swordSound2;
            audioSource.PlayOneShot(chosenClip);
        }
    }
}
