using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
    //This script handles and plays audio cues like footsteps, jump and land audio clips based on character movement speed and events; 
    public class AudioControl : MonoBehaviour
    {
        // References to components
        Controller controller;
        Animator animator;
        Mover mover;
        Transform tr;
        public AudioSource audioSource;

        // Whether footsteps will be based on the currently playing animation or calculated based on walked distance
        public bool useAnimationBasedFootsteps = true;

        // Velocity threshold for landing sound effect
        public float landVelocityThreshold = 5f;

        // Footsteps will be played every time the traveled distance reaches this value (if 'useAnimationBasedFootsteps' is set to 'true')
        public float footstepDistance = 0.2f;
        float currentFootstepDistance = 0f;

        private float currentFootStepValue = 0f;

        // Volume of all audio clips
        [Range(0f, 1f)]
        public float audioClipVolume = 0.1f;

        // Range of random volume deviation used for footsteps
        public float relativeRandomizedVolumeRange = 0.2f;

        // Audio clips
        public AudioClip[] footStepClips;
        public AudioClip jumpClip;
        public AudioClip landClip;

        // Setup
        // Setup
        void Start()
        {
            // Get component references
            controller = GetComponent<Controller>();
            Debug.Log("Trying to find Controller on " + gameObject.name);

            if (controller == null)
            {
                Debug.Log("Controller not found on this GameObject, searching in parents or children.");
                controller = GetComponentInParent<Controller>();
                if (controller == null)
                {
                    controller = GetComponentInChildren<Controller>();
                }
            }

            if (controller == null)
            {
                Debug.LogError("Controller component not found on " + gameObject.name + " or its parents/children.");
            }
            else
            {
                Debug.Log("Controller found: " + controller.gameObject.name);
            }
        }


        void Update()
        {
            if (controller != null)
            {
                // Get controller velocity
                Vector3 _velocity = controller.GetVelocity();

                // Calculate horizontal velocity
                Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);

                FootStepUpdate(_horizontalVelocity.magnitude);
            }
            else
            {
                Debug.LogError("Controller is null in AudioControl.Update for " + gameObject.name);
            }
        }


        // Method to play footstep sound based on movement speed
        private void FootStepUpdate(float _movementSpeed)
        {
            float _speedThreshold = 0.05f;

            if (useAnimationBasedFootsteps && animator != null)  // Ajout de la vérification de null pour l'animator
            {
                // Get current foot step value from animator
                float _newFootStepValue = animator.GetFloat("FootStep");

                // Play a foot step audio clip whenever the foot step value changes its sign
                if ((currentFootStepValue <= 0f && _newFootStepValue > 0f) || (currentFootStepValue >= 0f && _newFootStepValue < 0f))
                {
                    // Only play footstep sound if mover is grounded and movement speed is above the threshold
                    if (mover != null && mover.IsGrounded() && _movementSpeed > _speedThreshold)
                        PlayFootstepSound(_movementSpeed);
                }
                currentFootStepValue = _newFootStepValue;
            }
            else
            {
                currentFootstepDistance += Time.deltaTime * _movementSpeed;

                // Play foot step audio clip if a certain distance has been traveled
                if (currentFootstepDistance > footstepDistance)
                {
                    // Only play footstep sound if mover is grounded and movement speed is above the threshold
                    if (mover != null && mover.IsGrounded() && _movementSpeed > _speedThreshold)
                        PlayFootstepSound(_movementSpeed);
                    currentFootstepDistance = 0f;
                }
            }
        }


        // Public method to play jump sound
        public void PlayJumpSound(Vector3 velocity)
        {
            if (jumpClip != null)
            {
                audioSource.PlayOneShot(jumpClip, audioClipVolume);
            }
        }

        public void PlayFootstepSound(Vector3 velocity)
        {
            if (controller != null)
            {
                float movementSpeed = velocity.magnitude;
                FootStepUpdate(movementSpeed);
            }
            else
            {
/*                Debug.LogError("Controller is null in PlayFootstepSound");
*/            }
        }

        // Private method to play footstep sound
        private void PlayFootstepSound(float movementSpeed)
        {
            int _footStepClipIndex = Random.Range(0, footStepClips.Length);
            audioSource.PlayOneShot(footStepClips[_footStepClipIndex], audioClipVolume + audioClipVolume * Random.Range(-relativeRandomizedVolumeRange, relativeRandomizedVolumeRange));
        }

        // Event methods
        void OnLand(Vector3 _v)
        {
            // Only trigger sound if downward velocity exceeds threshold
            if (VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
                return;

            // Play land audio clip
            audioSource.PlayOneShot(landClip, audioClipVolume);
        }

        void OnJump(Vector3 _v)
        {
            // Play jump audio clip
            audioSource.PlayOneShot(jumpClip, audioClipVolume);
        }
    }
}
