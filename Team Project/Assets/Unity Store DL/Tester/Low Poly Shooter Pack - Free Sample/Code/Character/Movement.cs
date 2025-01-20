// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Audio Clips")]
        
        [Tooltip("The audio clip that is played while walking.")]
        [SerializeField]
        private AudioClip audioClipWalking;

        [Tooltip("The audio clip that is played while running.")]
        [SerializeField]
        private AudioClip audioClipRunning;

        [Header("Speeds")]

        [SerializeField]
        private float speedWalking = 5.0f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 9.0f;

        [Header("Jump Settings")]
        [Tooltip("Maximum number of jumps allowed.")]
        [SerializeField]
        private int maxJumpCount = 2;

        [Tooltip("Force applied during a jump.")]
        [SerializeField]
        private float jumpForce = 7.0f;

        [Header("Dash Settings")]
        [Tooltip("Force applied during a dash.")]
        [SerializeField]
        private float dashForce = 15.0f;

        [Tooltip("Duration of the dash.")]
        [SerializeField]
        private float dashDuration = 0.3f;

        [Tooltip("Cooldown between dashes.")]
        [SerializeField]
        private float dashCooldown = 1.5f;

        [Header("Custom Gravity Settings")]
        [SerializeField] private float customGravity = -30.0f; // Gravity strength
        [SerializeField] private float terminalVelocity = -50.0f; // Maximum fall speed

        #endregion

        #region PROPERTIES

        //Velocity.
        private Vector3 Velocity
        {
            //Getter.
            get => rigidBody.linearVelocity;
            //Setter.
            set => rigidBody.linearVelocity = value;
        }

        #endregion

        #region FIELDS

        private bool isDashing = false;
        private bool canDash = true;
        private Vector3 dashDirection;
        private float dashEndTime;
        private float dashCooldownEndTime;

        private int currentJumpCount;

        /// <summary>
        /// Attached Rigidbody.
        /// </summary>
        private Rigidbody rigidBody;
        /// <summary>
        /// Attached CapsuleCollider.
        /// </summary>
        private CapsuleCollider capsule;
        /// <summary>
        /// Attached AudioSource.
        /// </summary>
        private AudioSource audioSource;
        
        /// <summary>
        /// True if the character is currently grounded.
        /// </summary>
        private bool grounded;

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;
        
        /// <summary>
        /// Array of RaycastHits used for ground checking.
        /// </summary>
        private readonly RaycastHit[] groundHits = new RaycastHit[8];

        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Player Character.
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        /// Initializes the FpsController on start.
        protected override  void Start()
        {
            //Rigidbody Setup.
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            //Cache the CapsuleCollider.
            capsule = GetComponent<CapsuleCollider>();

            //Audio Source Setup.
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClipWalking;
            audioSource.loop = true;
        }

        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            //Bounds.
            Bounds bounds = capsule.bounds;
            //Extents.
            Vector3 extents = bounds.extents;
            //Radius.
            float radius = extents.x - 0.01f;
            
            //Cast. This checks whether there is indeed ground, or not.
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                groundHits, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            
            //We can ignore the rest if we don't have any proper hits.
            if (!groundHits.Any(hit => hit.collider != null && hit.collider != capsule)) 
                return;
            
            //Store RaycastHits.
            for (var i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();

            //Set grounded. Now we know for sure that we're grounded.
            grounded = true;

            if (grounded)
                ResetJumpCount();
        }
			
        protected override void FixedUpdate()
        {
            //Move.
            if (!isDashing)
                MoveCharacter();

            UpdateDash();

            //Unground.
            grounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        protected override  void Update()
        {
            //Get the equipped weapon!
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
            if (Input.GetButtonDown("Dash"))
                Dash();

            //Play Sounds!
            PlayFootstepSounds();
        }

        #endregion

        #region METHODS

        private void MoveCharacter()
        {
            #region Calculate Movement Velocity

            //Get Movement Input!
            Vector2 frameInput = playerCharacter.GetInputMovement();

            //Calculate local-space direction by using the player's input.
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);

            //Running speed calculation.
            movement *= playerCharacter.IsRunning() ? speedRunning : speedWalking;

            //World space velocity calculation. This allows us to add it to the rigidbody's velocity properly.
            movement = transform.TransformDirection(movement);

            float yVelocity = rigidBody.linearVelocity.y;

            // Apply custom gravity if not grounded
            if (!grounded)
            {
                yVelocity += customGravity * Time.deltaTime;
                yVelocity = Mathf.Max(yVelocity, terminalVelocity); // Clamp to terminal velocity
            }
            else if (rigidBody.linearVelocity.y < 0)
            {
                yVelocity = 0.0f;
            }

            #endregion

            //Update Velocity.
            rigidBody.linearVelocity = new Vector3(movement.x, yVelocity, movement.z);
            //Velocity = new Vector3(movement.x, yVelocity, movement.z);
        }

        private void Jump()
        {
            if (isDashing || currentJumpCount >= maxJumpCount)
                return;

            // Apply jump force to the Y velocity
            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, jumpForce, rigidBody.linearVelocity.z);

            // Increment jump count
            currentJumpCount++;
        }

        private void ResetJumpCount()
        {
            if (grounded)
                currentJumpCount = 0;
        }

        private void Dash()
        {
            if (isDashing || !canDash)
                return;

            isDashing = true;
            canDash = false;

            dashDirection = playerCharacter.GetCameraWorld().transform.forward.normalized;
            dashEndTime = Time.time + dashDuration;
            dashCooldownEndTime = Time.time + dashCooldown;

            rigidBody.useGravity = false;
            rigidBody.linearVelocity = dashDirection * dashForce;
        }

        private void UpdateDash()
        {
            if (isDashing)
            {
                if (Time.time >= dashEndTime)
                {
                    isDashing = false;
                    rigidBody.useGravity = true;
                }
            }
            else if (Time.time >= dashCooldownEndTime)
            {
                canDash = true;
            }
        }

        /// <summary>
        /// Plays Footstep Sounds. This code is slightly old, so may not be great, but it functions alright-y!
        /// </summary>
        private void PlayFootstepSounds()
        {
            //Check if we're moving on the ground. We don't need footsteps in the air.
            if (grounded && rigidBody.linearVelocity.sqrMagnitude > 0.1f)
            {
                //Select the correct audio clip to play.
                audioSource.clip = playerCharacter.IsRunning() ? audioClipRunning : audioClipWalking;
                //Play it!
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            //Pause it if we're doing something like flying, or not moving!
            else if (audioSource.isPlaying)
                audioSource.Pause();
        }

        #endregion
    }
}