
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class FPSController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.5f;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private float slideHeight = 1f;

    // LOOK
    [Header("Look")]
    [SerializeField] private float lookSensitivity = 50f;
    [SerializeField] private float lookSmoothness = 0.08f;
    [SerializeField] private float maxLookAngle = 60f;

    private CharacterController controller;
    private DefaultInputActions inputActions;

    private float verticalVelocity;
    private float dashTimer;
    private float slideTimer;

    private float normalHeight;
    private Vector3 normalCenter;

    private bool canJump;
    private bool isDashing;
    private bool isSliding;

    private Vector3 dashDirection;
    private Vector3 slideDirection;

    // LOOK
    private float cameraPitch;
    private float currentLookX;
    private float currentLookY;
    private float lookVelocityX;
    private float lookVelocityY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        inputActions = new DefaultInputActions();

        Debug.Log("INPUT ASSET: " + inputActions.asset.name);

        foreach (var binding in inputActions.Player.Look.bindings)
        {
            Debug.Log("LOOK BINDING: " + binding.path);
        }

        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Move();
        UpdateDash();
        UpdateSlide();
        Look();
    }

    private void Move()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x
                     + transform.forward * input.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (canJump && controller.isGrounded && !isSliding)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        canJump = false;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = Vector3.zero;

        if (isDashing || isSliding)
        {
            velocity = dashDirection * 0f;
        }
        else
        {
            velocity = move * moveSpeed;
        }

        velocity.y = verticalVelocity;

        if (!isDashing && !isSliding)
        {
            controller.Move(velocity * Time.deltaTime);
        }

        if (animator != null)
        {
            animator.SetBool(
                "IsWalking",
                input.magnitude > 0.01f && !isDashing && !isSliding
            );
        }
    }

    public void Jump()
    {
        canJump = true;
    }

    // DASH

    public void StartDash()
    {
        if (isDashing || isSliding || !controller.isGrounded)
        {
            return;
        }

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction = transform.right * input.x
                          + transform.forward * input.y;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        dashDirection = direction.normalized;

        isDashing = true;
        dashTimer = dashDuration;

        if (animator != null)
        {
            animator.SetBool("IsDashing", true);
            animator.SetBool("IsWalking", false);
        }
    }

    private void UpdateDash()
    {
        if (!isDashing)
        {
            return;
        }

        dashTimer -= Time.deltaTime;

        controller.Move(
            (dashDirection * dashSpeed + Vector3.up * verticalVelocity)
            * Time.deltaTime
        );

        if (dashTimer <= 0f)
        {
            isDashing = false;

            if (animator != null)
            {
                animator.SetBool("IsDashing", false);
            }
        }
    }

    // SLIDE

    public void StartSlide()
    {
        if (isSliding || isDashing || !controller.isGrounded)
        {
            return;
        }

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction = transform.right * input.x
                          + transform.forward * input.y;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        slideDirection = direction.normalized;

        isSliding = true;
        slideTimer = slideDuration;

        controller.height = slideHeight;
        controller.center = new Vector3(
            normalCenter.x,
            slideHeight / 2f,
            normalCenter.z
        );

        if (animator != null)
        {
            animator.SetBool("IsSliding", true);
            animator.SetBool("IsWalking", false);
        }
    }

    private void UpdateSlide()
    {
        if (!isSliding)
        {
            return;
        }

        slideTimer -= Time.deltaTime;

        float currentSlideSpeed = slideSpeed *
            Mathf.Clamp01(slideTimer / slideDuration);

        controller.Move(
            (slideDirection * currentSlideSpeed
            + Vector3.up * verticalVelocity) * Time.deltaTime
        );

        if (slideTimer <= 0f)
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        isSliding = false;

        controller.height = normalHeight;
        controller.center = normalCenter;

        if (animator != null)
        {
            animator.SetBool("IsSliding", false);
        }
    }

    // LOOK

    private void Look()
    {
        Vector2 lookInput =
            inputActions.Player.Look.ReadValue<Vector2>();

        Debug.Log(
            "LOOK: " + lookInput +
            " | CONTROL: " + inputActions.Player.Look.activeControl?.path
        );
        // Suaviza la entrada del joystick.
        currentLookX = Mathf.SmoothDamp(
            currentLookX,
            lookInput.x,
            ref lookVelocityX,
            lookSmoothness
        );

        currentLookY = Mathf.SmoothDamp(
            currentLookY,
            lookInput.y,
            ref lookVelocityY,
            lookSmoothness
        );

        // Calcula la rotación horizontal y vertical.
        float yaw = currentLookX * lookSensitivity * Time.deltaTime;
        float pitch = currentLookY * lookSensitivity * Time.deltaTime;

        // Gira el personaje horizontalmente.
        transform.Rotate(Vector3.up * yaw);

        // Gira la cámara verticalmente.
        cameraPitch -= pitch;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}