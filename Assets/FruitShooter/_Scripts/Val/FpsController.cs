
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class FPSController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        inputActions = new DefaultInputActions();

        // Guarda las dimensiones originales del personaje.
        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    private void OnEnable()
    {
        //if (IsOwner) 
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        //if (IsOwner)
         inputActions.Player.Disable();
    }

    private void Start()
    {
        //if (IsOwner) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
       // }
    }

    private void Update()
    {
        Move();
        UpdateDash();
        UpdateSlide();
        //if (IsOwner) Move();
        // Look();
    }

    private void Move()
    {
        Vector2 input= inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move= transform.right * input.x
                     + transform.forward * input.y;

        if (move.sqrMagnitude >1f)
        {
            move.Normalize();
        }

        // Mantiene al personaje en el suelo.
        if (controller.isGrounded && verticalVelocity<0f)
        {
            verticalVelocity = -2f;
        }

        // Salto normal, excepto mientras se desliza.
        if (canJump && controller.isGrounded && !isSliding)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        canJump= false;

        verticalVelocity+= gravity*Time.deltaTime;

        // No permite que el movimiento normal interfiera
        // con el dash o el slide.
        Vector3 velocity = Vector3.zero;

        if (isDashing || isSliding)
        {
            velocity= dashDirection*0f;
        }
        else
        {
            velocity= move*moveSpeed;
        }

        velocity.y= verticalVelocity;

        if (!isDashing && !isSliding)
        {
            controller.Move(velocity*Time.deltaTime);
        }

        // Animación de caminar.
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
        canJump =true;
    }

    // DASH

    public void StartDash()
    {
        if (isDashing || isSliding || !controller.isGrounded)
        {
            return;
        }

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction= transform.right*input.x + transform.forward*input.y;

        // Si no hay dirección, realiza el dash hacia adelante.
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

        dashTimer-= Time.deltaTime;

        // Avanza rápidamente durante el dash.
        controller.Move((dashDirection*dashSpeed + Vector3.up*verticalVelocity) *Time.deltaTime);

        if (dashTimer <= 0f)
        {
            isDashing=false;

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

        Vector3 direction= transform.right*input.x + transform.forward*input.y;

        // Si no hay movimiento, se desliza hacia adelante.
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        slideDirection = direction.normalized;

        isSliding = true;
        slideTimer = slideDuration;

        // Reduce la cápsula para adoptar una postura baja.
        controller.height = slideHeight;
        controller.center = new Vector3(normalCenter.x, slideHeight / 2f, normalCenter.z);

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

        // Reduce progresivamente la velocidad del slide.
        float currentSlideSpeed = slideSpeed*Mathf.Clamp01(slideTimer / slideDuration);

        controller.Move((slideDirection*currentSlideSpeed + Vector3.up*verticalVelocity)*Time.deltaTime
        );

        if (slideTimer <= 0f)
        {
            EndSlide();
        }
    }

    private void EndSlide()
    {
        isSliding = false;

        // Restaura la cápsula original.
        controller.height = normalHeight;
        controller.center = normalCenter;

        if (animator != null)
        {
            animator.SetBool("IsSliding", false);
        }
    }
}
