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
    private GameObject PlayerModel;
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
    [SerializeField] private AudioClip gunSound;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Assigner le parent contenant Model1 et Model2
        GameObject parentModel = transform.Find("Model")?.gameObject;

        if (parentModel == null)
        {
            Debug.LogError("Le parent Model est introuvable !");
            return;
        }

        // Sélectionner le bon modèle en fonction de l'ID du joueur (Model1 ou Model2)
        GameObject SelectedModel = parentModel.transform.Find("Model" + (1 + NetworkClient.localPlayer.netId % 2).ToString())?.gameObject;

        if (SelectedModel == null)
        {
            Debug.LogError("Modèle sélectionné introuvable !");
            return;
        }

        // Assigner l'Animator du modèle sélectionné
        PlayerAnimator = SelectedModel.GetComponentInChildren<Animator>();

        if (PlayerAnimator == null)
        {
            Debug.LogError("Animator introuvable sur le modèle sélectionné !");
        }
        else
        {
            SelectedModel.SetActive(true);  // Activer le bon modèle
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currCooldown > 0)
        {
            currCooldown = Math.Max(0, currCooldown - Time.deltaTime);
        }

        SendDebugRaycast();
    }

    private void FixedUpdate()
    {
        SendDebugRaycast();
    }

    private void SetAnimation(string animationName)
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetTrigger(animationName);
        }
        else
        {
            Debug.LogError("PlayerAnimator est null !");
        }
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
        }
        else if (enemy.tag == "Player")
        {
            enemy.GetComponent<PlayerMovementController>().TakeDamage(enemy, damage);
        }
    }

    private void SendDebugRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(CameraRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(CameraRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        }
        else
        {
            Debug.DrawRay(CameraRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward) * 1000, Color.white);
        }
    }

    private RaycastHit SendRaycast()
    {
        RaycastHit hit;
        Physics.Raycast(CameraRoot.transform.position, CameraRoot.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
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
        SetAnimation("StopAttacking");
    }

    private IEnumerator WaitAndResetCombo()
    {
        yield return new WaitForSeconds(1f);
        ResetCombo();
    }

    private IEnumerator ComboTick()
    {
        SetAnimation("Attacking" + comboCount.ToString());
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
                }

                audioSource.PlayOneShot(gunSound);
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
