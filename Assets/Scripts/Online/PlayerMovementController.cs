using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Mirror.Examples.MultipleMatch;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField] private RoundManager roundManager;

    [SerializeField] private GameObject PlayerModel;
    [SerializeField] private GameObject PlayerGui;
    [SerializeField] private GameObject UICamera;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private RawImage healthBar;
    [SerializeField] private TextMeshProUGUI healthText;

    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private Image bloodVignette;
    private Coroutine bloodVignetteCoroutine;

    [SerializeField] private Animator _animator;

    [Tooltip("Player Values")]

    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 200f;

    [SyncVar(hook = nameof(UpdateHealthBar))]
    [SerializeField] private float health = 100;

    [SyncVar]
    [SerializeField] private bool isDead = false;

    [SyncVar]
    [SerializeField] private float maxHealth = 100;

    [SerializeField] private AudioClip damageSound;
    private AudioSource audioSource;

    [Header("Audio Control")]
    [SerializeField] private bool useAnimationBasedFootsteps = true;
    [SerializeField] private float landVelocityThreshold = 5f;
    [SerializeField] private float footstepDistance = 0.2f;
    [Range(0f, 1f)]
    public float audioClipVolume = 0.1f;
    public float relativeRandomizedVolumeRange = 0.2f;
    public AudioClip[] footStepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;

    private float currentFootstepDistance = 0f;
    private float currentFootStepValue = 0f;
    private bool wasGrounded = true;
    private int lastFootStepClipIndex = -1;

    [SerializeField] private float footstepInterval = 0.5f;
    private float footstepTimer = 0f;
    private bool isMoving = false;

    private void Start()
    {
        PlayerModel.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "OnlineGame")
        {
            if (roundManager == null)
            {
                GameObject roundManagerGO = GameObject.Find("RoundManager");
                if (roundManagerGO != null)
                {
                    roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
                }
            }
            if (PlayerModel.activeSelf == false)
            {
                GameObject SelectedModel = PlayerModel.transform.Find("Model" + (1 + NetworkClient.localPlayer.netId % 2).ToString()).gameObject;
                _animator = SelectedModel.GetComponent<Animator>();

                if (!isLocalPlayer)
                {
                    Debug.Log("other player");
                    SelectedModel.SetActive(true);
                }

                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                PlayerModel.SetActive(true);
                if (isLocalPlayer)
                {
                    PlayerGui.SetActive(true);
                    UICamera.SetActive(true);
                }
                SetSpawnPosition();
                Debug.Log(gameObject.name + " Position is: " + gameObject.transform.position);
            }

            if (isLocalPlayer)
            {
                Movement();
            }

            if (!isLocalPlayer) {
                _animator.SetFloat("Forward", rb.velocity.z);
                _animator.SetFloat("Sided", rb.velocity.x);
            }

            HandleLandingSound();
        }
    }

    public void SetSpawnPosition()
    {
        Physics.SyncTransforms();
        rb.velocity = Vector3.zero;
        transform.position = NetworkManager.singleton.GetStartPosition().position;
    }

    public void Movement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = (forward  * zDirection) + (right * xDirection);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce);
            PlayJumpSound();
            wasGrounded = true;
        }

        rb.MovePosition(rb.position + moveDirection * Time.deltaTime * speed);

        if (transform.position.y < -2000)
        {
            transform.position = new Vector3(transform.position.x, 50, transform.position.z);
            rb.velocity = Vector3.zero;
        }

        if (isLocalPlayer)
        {
            HandleMovementInput();
        }
    }

    public void TakeDamage(GameObject enemy, float value)
    {
        health = Mathf.Max(health - value, 0);

        PlayDamageSound();

        RpcTakeDamage(enemy.GetComponent<NetworkIdentity>().connectionToClient);
    }

    void UpdateHealthBar(float oldValue, float newValue)
    {
        Debug.Log("Health changed: " + newValue.ToString());
        healthBar.rectTransform.sizeDelta = new Vector2((newValue / maxHealth) * 400, healthBar.rectTransform.sizeDelta.y);
        healthText.text = newValue.ToString() + " / " + maxHealth.ToString();

        if (newValue <= 0 && isDead == false) {
            Debug.Log("Dead");
            isDead = true;
            roundManager.PlayerDied();
        }
    }

    public void UpdateStatus(int round, int timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);

        timerText.text = "Time Left - " + string.Format("{0:00}:{1:00}", minutes, seconds);

        string indicator = "Round";

        if (round == 0)
        {
            indicator = "Intermission";
        } else if (round == -2)
        {
            indicator = "GAME ENDED !";
        } else
        {
            indicator = "Round " + round.ToString();
        }

        roundText.text = indicator;
    }

    [Server]
    public void SetHealth(float newHealth)
    {
        health = Mathf.Max(newHealth, maxHealth);
        if (health > 0)
        {
            isDead = false;
            Debug.Log("healed");
        }
    } 

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void HandleMovementInput()
    {
        bool isMovingNow = Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMovingNow)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                float movementSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

                PlayFootstepSound(movementSpeed);
                footstepTimer = footstepInterval;
            }
        }

        isMoving = isMovingNow;
    }


    private void PlayFootstepSound(float movementSpeed)
    {
        int footStepClipIndex;

        do
        {
            footStepClipIndex = Random.Range(0, footStepClips.Length);
        }
        while (footStepClipIndex == lastFootStepClipIndex && footStepClips.Length > 1);

        lastFootStepClipIndex = footStepClipIndex;

        float adjustedVolume = audioClipVolume + audioClipVolume * Random.Range(-relativeRandomizedVolumeRange, relativeRandomizedVolumeRange);

        audioSource.PlayOneShot(footStepClips[footStepClipIndex], adjustedVolume);
    }

    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    private void PlayJumpSound()
    {
        if (jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip, audioClipVolume);
        }
    }

    private void HandleLandingSound()
    {
        if (IsGrounded() && !wasGrounded)
        {
            PlayLandSound();
        }
        wasGrounded = IsGrounded();
    }

    private void PlayLandSound()
    {
        if (landClip != null)
        {
            audioSource.PlayOneShot(landClip, audioClipVolume);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    private void UpdateFootstepSound(Vector3 velocity)
    {
        float movementSpeed = new Vector3(velocity.x, 0, velocity.z).magnitude;
        float speedThreshold = 0.05f;

        if (useAnimationBasedFootsteps && _animator != null)
        {
            float newFootStepValue = _animator.GetFloat("FootStep");

            if ((currentFootStepValue <= 0f && newFootStepValue > 0f) || (currentFootStepValue >= 0f && newFootStepValue < 0f))
            {
                if (movementSpeed > speedThreshold && IsGrounded())
                    PlayFootstepSound(movementSpeed);
            }
            currentFootStepValue = newFootStepValue;
        }
        else
        {
            currentFootstepDistance += Time.deltaTime * movementSpeed;

            if (currentFootstepDistance > footstepDistance)
            {
                if (movementSpeed > speedThreshold && IsGrounded())
                    PlayFootstepSound(movementSpeed);

                currentFootstepDistance = 0f;
            }
        }
    }

    [TargetRpc]
    void RpcTakeDamage(NetworkConnectionToClient conn)
    {
        if (bloodVignetteCoroutine != null)
        {
            StopCoroutine(bloodVignetteCoroutine);
        }

        bloodVignetteCoroutine = StartCoroutine(PlayVignetteAnimation());
    }

    IEnumerator PlayVignetteAnimation()
    {
        bloodVignette.color = new Color(255, 255, 255, 0.5f);

        for (float i = 0.5f; i > 0; i -= 0.02f)
        {
            bloodVignette.color = new Color(255, 255, 255, Mathf.Max(i, 0));
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }
}
