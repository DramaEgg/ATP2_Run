﻿ using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.UI;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        float targetSpeed = 0f;
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 10.0f;
        public bool isSprinting = false;

        //---耐力---
        [Header("Player stamina - 耐力条")]

        public Slider staminaSlider;
        public GameObject staminaSliderGOBJ;
        public Image fillImage;
        public float currentStamina = 0;
        public float maxStamina = 5f;


        [Header("Player Chrouching - 蹲")]
        [Tooltip("Crouching Speed of Character in m/s")]
         float crouchSpeed = 3f;
         float crouchHeight = 0f;
        public bool isCrouching = false;
        public float standHeight = 1.58f;

        [Header("Player Hiding-隐匿")]
        public bool isHiding = false;

        [Header("Player Steap Sound")]
        public float currentSound;
        public float standSound = 0;
        public float walkSound = 5;
        public float sprintSound = 8;
        public float crouchSound = 3;

        private Collider playerCollider ;

        [Header("Animator-动画机")]
        public Animator animator;
        public PlayerInteraction playerInteraction;
        public SpriteRenderer spriteRenderer;




        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        //private int _animIDSpeed;
        //private int _animIDGrounded;
        //private int _animIDJump;
        //private int _animIDFreeFall;
        //private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        //private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            //_hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();

#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            //AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            currentStamina = maxStamina;
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
            staminaSliderGOBJ.SetActive(false);

            playerCollider = GetComponent<Collider>();
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        private void Update()
        {
            //_hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            MoveSoundVolume();
            AnimatonController();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        //private void AssignAnimationIDs()
        //{
        //    _animIDSpeed = Animator.StringToHash("Speed");
        //    _animIDGrounded = Animator.StringToHash("Grounded");
        //    _animIDJump = Animator.StringToHash("Jump");
        //    _animIDFreeFall = Animator.StringToHash("FreeFall");
        //    _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        //}

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            //// update animator if using character
            //if (_hasAnimator)
            //{
            //    _animator.SetBool(_animIDGrounded, Grounded);
            //}
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            //Original Speed

            //			==== States Check ====

            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching;
                isSprinting = false;

            }

            //			==== Speed & Height Change ====
            //Crouching and Dashing, 
            if (isCrouching)   //Crouch
            {
                targetSpeed = crouchSpeed;
                _controller.height = crouchHeight;

            }
            else if (_input.sprint && currentStamina > 3f) //Sprint
            {
                _controller.height = standHeight;
                targetSpeed = SprintSpeed;
                isSprinting = true;
            }
            else if (!_input.sprint || currentStamina <= 0f)  //Walk
            {
                _controller.height = standHeight;
                targetSpeed = MoveSpeed;
                isSprinting = false;

            }

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;  //不基于摄像头变化朝向
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;//基于摄像头改变方向
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            //if (_hasAnimator)
            //{
            //    _animator.SetFloat(_animIDSpeed, _animationBlend);
            //    _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            //}


            // Sprite Facing
            if (inputDirection.x > 0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (inputDirection.x < -0.1f)
            {
                spriteRenderer.flipX = false;
            }

            UpdateStamina();

        }

        void UpdateStamina()
        {
            staminaSlider.value = currentStamina;
            if (isSprinting)
            {
                //Decrease
                currentStamina -= Time.deltaTime;
                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                }
            }
            else
            {
                //Add
                currentStamina += Time.deltaTime;
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }

            //Color Ctrl
            if (currentStamina <= 3)
            {
                fillImage.color = Color.red;
            }
            else
            {
                fillImage.color = Color.green;
            }

            if (currentStamina >= maxStamina)
            {
                staminaSliderGOBJ.SetActive(false);
            }
            else
            {
                staminaSliderGOBJ.SetActive(true);
            }
        }


        void MoveSoundVolume()
        {
            if (targetSpeed == MoveSpeed)
            {
                currentSound = walkSound;
            }
            else if (targetSpeed == SprintSpeed)
            {
                currentSound = sprintSound;
            }
            else if (targetSpeed == crouchSpeed)
            {
                currentSound = crouchSound;
            }
            else
            {
                currentSound = standSound;
            }
        }

        /// <summary>
        /// Hiding in Stall Function 藏匿系统（进柜+出柜
        /// </summary>
        public void EnterHidingStall()
        { 
            isHiding= true;
            currentSound = 0;

        }

        public void ExitHidingStall()
        { 
            isHiding= false;
        }

        /// <summary>
        /// Jump 跳跃
        /// </summary>
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                //// update animator if using character
                //if (_hasAnimator)
                //{
                //    _animator.SetBool(_animIDJump, false);
                //    _animator.SetBool(_animIDFreeFall, false);
                //}

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    //// update animator if using character
                    //if (_hasAnimator)
                    //{
                    //    _animator.SetBool(_animIDJump, true);
                    //}
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    //// update animator if using character
                    //if (_hasAnimator)
                    //{
                    //    _animator.SetBool(_animIDFreeFall, true);
                    //}
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public void AnimatonController()
        {
            ////如果站着，则可以跑
            //if (!isCrouching)
            //{
            //    if (!aiming)
            //    {
            //        animator.SetBool("IsStand", true);
            //        if (targetSpeed !=0 )
            //        {
            //            animator.SetBool("IsRun", true);
            //        }
            //        else
            //        {
            //            animator.SetBool("IsRun", false);
            //        }
            //    }
            //    else
            //    {
            //        if (targetSpeed != 0)
            //        { 
            //            animator.
            //        }

            //    }


            //}
            //else  //蹲着的
            //{

            //}

            animator.SetBool("IsStand", !isCrouching && !playerInteraction.isHolding && targetSpeed == 0f);
            animator.SetBool("IsRun", !isCrouching && !playerInteraction.isHolding && targetSpeed != 0f);

            animator.SetBool("IsCrouchIdle", isCrouching && !playerInteraction.isHolding && targetSpeed == 0f);
            animator.SetBool("IsCrouchWalk", isCrouching && !playerInteraction.isHolding && targetSpeed != 0f);

            animator.SetBool("IsAimIdle", !isCrouching && playerInteraction.isHolding && targetSpeed == 0f);
            animator.SetBool("IsAimWalk", !isCrouching && playerInteraction.isHolding && targetSpeed != 0f);

            //// 参考你的建议，新增蹲下瞄准状态：
            //animator.SetBool("IsCrouchAimIdle", isCrouching && aiming && targetSpeed == 0f);
            //animator.SetBool("IsCrouchAimWalk", isCrouching && aiming && targetSpeed != 0f);

            // 最后处理开火：只在瞄准或蹲下瞄准时触发
            //if ((animator.GetBool("IsAimIdle") ||
            //     animator.GetBool("IsAimWalk") ||
            //     animator.GetBool("IsCrouchAimIdle") ||
            //     animator.GetBool("IsCrouchAimWalk"))
            //    && Input.GetMouseButtonDown(0))
            //{
            //    animator.SetTrigger("Shoot");
            //}
        }

    }
 }
