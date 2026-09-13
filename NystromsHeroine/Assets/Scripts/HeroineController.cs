using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HeroineFSMDemo
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HeroineController : MonoBehaviour
    {
        public Transform visual;

        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 7f;
        public float jumpSpeed = 8f;
        public float gravity = 22f;
        public float diveAcceleration = 80f;

        // One instance of each state; only CurrentState receives input and ticks.
        public IHeroineState Standing { get; } = new StandingState();
        public IHeroineState Jumping { get; } = new JumpingState();
        public IHeroineState Ducking { get; } = new DuckingState();
        public IHeroineState Diving { get; } = new DivingState();
        public IHeroineState CurrentState { get; private set; }

        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool DuckPressed { get; private set; }
        public bool DuckHeld { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool DoubleJumpAvailable { get; set; }
        public float VerticalSpeed { get; private set; }
        public bool IsGrounded => motor.isGrounded;
        public bool IsPaused { get; private set; }

        private CharacterController motor;
        private float moveSpeed;
        private float downwardAcceleration;
        private float timeScaleBeforePause = 1f;
        private bool pausePressed;
        private GUIStyle hudStyle;

        private void Start()
        {
            motor = GetComponent<CharacterController>();
            if (visual == null)
            {
                Debug.LogError("Assign the Visual child on HeroineController, or run Create Demo Scene.", this);
                enabled = false;
                return;
            }

            SetCrouching(false);
            ChangeState(Standing);
            // The generated character starts 0.1 units above the floor.
            motor.Move(Vector3.down * 0.2f);
        }

        private void Update()
        {
            ReadInput();
            if (pausePressed)
                SetPaused(!IsPaused);
            if (IsPaused)
                return;

            // Input may change the state before physics. Tick sees fresh collisions.
            CurrentState.HandleInput(this);
            ApplyMovement();
            CurrentState.Tick(this);
        }

        public void ChangeState(IHeroineState nextState)
        {
            if (CurrentState == nextState)
                return;

            CurrentState?.Exit(this);
            CurrentState = nextState;
            CurrentState.Enter(this);
        }

        public void ConfigureMovement(float speed, float acceleration)
        {
            moveSpeed = speed;
            downwardAcceleration = acceleration;
        }

        public void Jump()
        {
            VerticalSpeed = jumpSpeed;
        }

        public void BeginDive()
        {
            VerticalSpeed = -10f;
        }

        public void SetCrouching(bool crouching)
        {
            float height = crouching ? 1f : 2f;
            motor.height = height;
            motor.center = new Vector3(0f, height / 2f, 0f);
            visual.localScale = new Vector3(1f, height / 2f, 1f);
            visual.localPosition = motor.center;
        }

        private void ApplyMovement()
        {
            float dt = Time.deltaTime;
            if (IsGrounded && VerticalSpeed < 0f)
                VerticalSpeed = -2f;

            // CharacterController.Move does not apply gravity on its own.
            VerticalSpeed -= downwardAcceleration * dt;
            Vector2 input = Vector2.ClampMagnitude(MoveInput, 1f);
            Vector3 delta = new Vector3(input.x * moveSpeed, VerticalSpeed, input.y * moveSpeed) * dt;

            // Keep the demo on the flat floor and within the fixed camera's view.
            Vector3 position = transform.position;
            delta.x = Mathf.Clamp(position.x + delta.x, -6f, 6f) - position.x;
            delta.z = Mathf.Clamp(position.z + delta.z, -4f, 4f) - position.z;

            CollisionFlags collisions = motor.Move(delta);
            if ((collisions & CollisionFlags.Above) != 0 && VerticalSpeed > 0f)
                VerticalSpeed = 0f;
        }

        private void SetPaused(bool paused)
        {
            if (paused)
            {
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
                Time.timeScale = timeScaleBeforePause;

            // Preserve the current state and velocity, including in midair.
            IsPaused = paused;
        }

        private void OnDisable()
        {
            if (IsPaused)
                SetPaused(false);
        }

        private void ReadInput()
        {
            MoveInput = Vector2.zero;
            JumpPressed = DuckPressed = DuckHeld = SprintHeld = pausePressed = false;

            // Use the project's active input backend; no Input Actions asset is needed.
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            MoveInput = new Vector2(
                (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f),
                (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f));
            JumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            DuckPressed = keyboard.leftCtrlKey.wasPressedThisFrame;
            DuckHeld = keyboard.leftCtrlKey.isPressed;
            SprintHeld = keyboard.leftShiftKey.isPressed;
            pausePressed = keyboard.escapeKey.wasPressedThisFrame;
#else
            MoveInput = new Vector2(
                (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f),
                (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f));
            JumpPressed = Input.GetKeyDown(KeyCode.Space);
            DuckPressed = Input.GetKeyDown(KeyCode.LeftControl);
            DuckHeld = Input.GetKey(KeyCode.LeftControl);
            SprintHeld = Input.GetKey(KeyCode.LeftShift);
            pausePressed = Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private void OnGUI()
        {
            if (CurrentState == null)
                return;
            if (hudStyle == null)
            {
                hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                hudStyle.normal.textColor = Color.white;
            }

            bool sprinting = !IsPaused && CurrentState == Standing && SprintHeld && MoveInput.sqrMagnitude > 0f;
            string title = IsPaused ? "PAUSED - Esc to resume" : "Heroine FSM";
            string text = title + "\nState: " + CurrentState.Name
                + "\nExtra jump: " + (DoubleJumpAvailable ? "Ready" : "Used")
                + "\nSprint: " + (sprinting ? "On" : "Off")
                + "\n\nWASD  Move"
                + "\nSpace  Jump / double jump"
                + "\nLeft Ctrl  Duck / ground pound"
                + "\nLeft Shift  Sprint"
                + "\nEsc  Pause / resume";

            // Keep the panel proportional when the Game view is resized.
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(16f, 16f, 310f, 244f), GUIContent.none);
            GUI.Label(new Rect(28f, 24f, 288f, 230f), text, hudStyle);
            GUI.matrix = previousMatrix;
        }
    }
}
