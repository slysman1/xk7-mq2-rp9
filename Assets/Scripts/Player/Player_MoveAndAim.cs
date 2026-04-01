using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player_MoveAndAim : MonoBehaviour
{
    private Player player;
    private Player_Inventory inventory;
    private InputSystem_Actions input;
    private CharacterController controller;


    [Header("Movement")]
    public float moveSpeed = 5f;
    [Range(0, 1)] public float mediumWeightCarryMultiplier = 0.8f; // Multiplier for medium weight items
    [Range(0, 1)] public float heavyWeightCarryMultiplier = 0.5f; // Multiplier for heavy weight items
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;


    [Header("Look")]
    public Transform cameraTransform;
    public float lookSensitivity = 1f;
    public float verticalLookLimit = 80f;
    public bool IsLooking => lookInput.sqrMagnitude > 0.001f;


    [Header("Crouch")]
    public bool canCrouch;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchTransitionSpeed = 10f;
    private bool isCrouching;

    [Header("Sprint")]
    public float sprintMultiplier = 1.8f;
    private bool isSprinting;


    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation;

    private float currentHeight;

    private Vector3 cameraDefaultLocalPos;
    [SerializeField] private float crouchCameraOffsetY = -0.5f;

    [Header("Footsteps SFX")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float movementThreshold = 0.1f;
    private bool wasMoving;
    private float stepTimer;





    private void Awake()
    {
        player = GetComponent<Player>();
        inventory = GetComponent<Player_Inventory>();
        controller = GetComponent<CharacterController>();
        currentHeight = standHeight;
        cameraDefaultLocalPos = cameraTransform.localPosition;
    }

    private void Start()
    {
        input = player.input;

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Jump.performed += ctx => TryJump();

        input.Player.Crouch.started += ctx => SetCrouch(true);
        input.Player.Crouch.canceled += ctx => SetCrouch(false);

        input.Player.Sprint.started += ctx => SetSprint(true);
        input.Player.Sprint.canceled += ctx => SetSprint(false);
    }
    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleCrouchHeight();
        HandleFootsteps();
    }

    private void HandleFootsteps()
    {
        if (!controller.isGrounded)
        {
            stepTimer = 0f;
            wasMoving = false;
            return;
        }

        bool isMovingNow = moveInput.magnitude > movementThreshold;

        if (isMovingNow)
        {
            // If just started moving → play instantly
            if (!wasMoving)
            {
                footstepSource.pitch = Random.Range(0.95f, 1.05f);
                footstepSource.PlayOneShot(footstepClip);
                stepTimer = 0f;
            }

            stepTimer += Time.deltaTime;

            if (stepTimer >= GetStepInterval())
            {
                footstepSource.pitch = Random.Range(0.95f, 1.05f);
                footstepSource.PlayOneShot(footstepClip);
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }

        wasMoving = isMovingNow;
    }



    private void HandleMovement()
    {
        float speed = GetMoveSpeed();


        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void TryJump()
    {
        if (controller.isGrounded)
            velocity.y = Mathf.Sqrt(GetJumpHeight() * -2f * gravity);
    }
    private void SetCrouch(bool crouch)
    {
        if (canCrouch == false)
            return;

        isCrouching = crouch;
    }
    private void HandleCrouchHeight()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 center = controller.center;
        center.y = controller.height / 2f;
        controller.center = center;

        Vector3 targetCameraPos = cameraDefaultLocalPos;
        if (isCrouching)
            targetCameraPos += Vector3.up * crouchCameraOffsetY;

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetCameraPos,
            Time.deltaTime * crouchTransitionSpeed
        );

    }
    private void SetSprint(bool sprint)
    {
        isSprinting = sprint;
        // Optional: if crouching, cancel sprint
        if (isCrouching)
            isSprinting = false;
    }
    private float GetJumpHeight()
    {
        float currentJumpHeight = jumpHeight;
        switch (inventory.weightInHands)
        {
            case ItemWeightType.Heavy:
                currentJumpHeight = currentJumpHeight * heavyWeightCarryMultiplier;
                break;
            case ItemWeightType.Medium:
                currentJumpHeight = currentJumpHeight * mediumWeightCarryMultiplier;
                break;
            case ItemWeightType.Light:
            case ItemWeightType.None:
            default:
                // no change
                break;
        }

        return currentJumpHeight;
    }
    private float GetMoveSpeed()
    {
        // start with your base speed…
        float speed = moveSpeed;

        // then scale by carry‐weight
        switch (inventory.weightInHands)
        {
            case ItemWeightType.Heavy:
                speed = speed * heavyWeightCarryMultiplier;
                break;
            case ItemWeightType.Medium:
                speed = speed * mediumWeightCarryMultiplier;
                break;
            case ItemWeightType.Light:
            case ItemWeightType.None:
            default:
                // no change
                break;
        }

        // finally, account for sprint/crouch
        if (isCrouching)
            speed = speed * crouchSpeedMultiplier;
        else if (isSprinting)
            speed = speed * sprintMultiplier;

        return speed;
    }
    private float GetStepInterval()
    {
        float currentSpeed = GetMoveSpeed();

        if (currentSpeed <= 0.01f)
            return walkStepInterval;

        // Normalize against base walking speed
        float speedRatio = currentSpeed / moveSpeed;

        // Faster speed = smaller interval
        float interval = walkStepInterval / speedRatio;

        return interval;
    }

}
