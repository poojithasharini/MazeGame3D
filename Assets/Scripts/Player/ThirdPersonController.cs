using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Forward walk speed in m/s")]
        public float MoveSpeed = 3.4f;

        [Tooltip("Reverse / backward speed in m/s")]
        public float BackwardSpeed = 2.4f;

        [Tooltip("Sprint speed in m/s when holding Shift")]
        public float SprintSpeed = 6.2f;

        [Tooltip("Turning rotation speed in degrees per second")]
        public float TurnSpeed = 160.0f;

        [Space(10)]
        [Header("Jumping & Gravity")]
        [Tooltip("Jump height in meters")]
        public float JumpHeight = 1.35f;

        [Tooltip("Gravity acceleration")]
        public float Gravity = -22.0f;

        [Tooltip("Double tap threshold in seconds for Up Arrow jump")]
        public float DoubleTapTimeThreshold = 0.32f;

        [Header("Obstacles")]
        public LayerMask ObstacleLayers;

        [Header("Audio")]
        public AudioSource BackgroundMusic;

        // Internal movement variables
        private CharacterController _controller;
        private float _currentForwardSpeed = 0.0f;
        private float _verticalVelocity = 0.0f;
        private bool _isRestarting = false;

        // Double tap tracking for Up Arrow jump
        private float _lastUpArrowPressTime = -10.0f;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (_controller != null)
            {
                _controller.stepOffset = 0.35f;
                _controller.slopeLimit = 50.0f;
            }
        }

        private void Start()
        {
            if (BackgroundMusic != null)
            {
                BackgroundMusic.loop = true;
                if (!BackgroundMusic.isPlaying)
                {
                    BackgroundMusic.Play();
                }
            }
        }

        private void Update()
        {
            if (_isRestarting) return;

            ReadInputAndMove();
        }

        private void ReadInputAndMove()
        {
            bool up = false;
            bool down = false;
            bool left = false;
            bool right = false;
            bool sprint = false;
            bool jump = false;

            // 1. Standard Input Manager
            try
            {
                up = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
                down = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
                left = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                right = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
                sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                // Spacebar Jump
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jump = true;
                }

                // Double-tap Up Arrow (or W) to Jump!
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    float timeSinceLastPress = Time.time - _lastUpArrowPressTime;
                    if (timeSinceLastPress > 0.05f && timeSinceLastPress <= DoubleTapTimeThreshold)
                    {
                        jump = true; // Double tap detected!
                    }
                    _lastUpArrowPressTime = Time.time;
                }
            }
            catch { }

            // 2. New Input System fallback
#if ENABLE_INPUT_SYSTEM
            try
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null)
                {
                    if (kb.upArrowKey.isPressed || kb.wKey.isPressed) up = true;
                    if (kb.downArrowKey.isPressed || kb.sKey.isPressed) down = true;
                    if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) left = true;
                    if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) right = true;
                    if (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) sprint = true;

                    if (kb.spaceKey.wasPressedThisFrame) jump = true;

                    if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
                    {
                        float timeSince = Time.time - _lastUpArrowPressTime;
                        if (timeSince > 0.05f && timeSince <= DoubleTapTimeThreshold)
                        {
                            jump = true;
                        }
                        _lastUpArrowPressTime = Time.time;
                    }
                }
            }
            catch { }
#endif

            // Turning (Left / Right)
            float turnInput = 0f;
            if (right) turnInput += 1f;
            if (left) turnInput -= 1f;

            if (Mathf.Abs(turnInput) > 0.01f)
            {
                transform.Rotate(0f, turnInput * TurnSpeed * Time.deltaTime, 0f);
            }

            // Forward / Backward Movement (Along player's facing direction)
            float moveTarget = 0f;
            if (up)
            {
                moveTarget = sprint ? SprintSpeed : MoveSpeed;
            }
            else if (down)
            {
                moveTarget = -BackwardSpeed;
            }

            _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, moveTarget, Time.deltaTime * 14.0f);

            // Ground & Vertical Jump Handling
            bool isGrounded = _controller.isGrounded;
            if (isGrounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2.0f; // Firm ground clamping
                }

                if (jump)
                {
                    // Calculate velocity needed to reach JumpHeight
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2.0f * Gravity);
                }
            }
            else
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            // Move in facing direction
            Vector3 movement = transform.forward * (_currentForwardSpeed * Time.deltaTime);
            movement.y = _verticalVelocity * Time.deltaTime;

            _controller.Move(movement);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (_isRestarting) return;

            // Deadly hazards: Tree or Rock restarts the level (Slide 4)
            // Low jumping boxes (Crates) are NOT deadly hazards and can be hopped over!
            if (hit.collider.CompareTag("Tree") || hit.collider.CompareTag("Rock"))
            {
                RestartGame();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isRestarting) return;

            if (other.CompareTag("Tree") || other.CompareTag("Rock"))
            {
                RestartGame();
            }
        }

        public void RestartGame()
        {
            if (_isRestarting) return;
            _isRestarting = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.TriggerCollisionRestart();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public float GetCurrentSpeed()
        {
            return Mathf.Abs(_currentForwardSpeed);
        }

        public bool IsGrounded()
        {
            return _controller != null && _controller.isGrounded;
        }
    }
}
