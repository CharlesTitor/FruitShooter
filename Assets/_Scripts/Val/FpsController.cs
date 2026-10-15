using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
public class FPSController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    
    // [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 2f;

    // [Header("Look")]
    // [SerializeField] private float mouseSensitivity = 0.08f;
    // [SerializeField] private float gamepadLookSpeed = 180f;
    // [SerializeField] private float maxLookAngle = 85f;

    private CharacterController controller;
    private DefaultInputActions inputActions;

    private float verticalVelocity;
    private bool canJump;

    // private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        inputActions = new DefaultInputActions();
    }

    private void OnEnable()
    {
        if (IsOwner) inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (IsOwner) inputActions.Player.Disable();
    }

    private void Start()
    {
        if (IsOwner) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (IsOwner) Move();
        // Look();
    }

    private void Move()
    {
        // X = izquierda / derecha
        // Y = adelante / atrás
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move =transform.right * input.x + transform.forward * input.y;

        // Evita que el movimiento diagonal sea más rápido.
        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        // Mantiene al personaje pegado al suelo.
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Comprueba si el jugador pidió saltar.
        if (canJump && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight*-4f*gravity);
        }

        // Reinicia la petición para no saltar repetidamente.
        canJump = false;

        // Aplica gravedad.
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);


        // ANIMACIÓN
        if (input.magnitude > 0.01f)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
    }

    public void Jump()
    {
        canJump=true;
    }
    
    /*
    private void Look()
    {
        Vector2 lookInput =
            inputActions.Player.Look.ReadValue<Vector2>();

        float yaw;
        float pitchDelta;

        // Mouse y joystick necesitan tratamiento ligeramente diferente.
        // Mouse entrega delta de movimiento.
        // Gamepad entrega una posición del stick.
        bool usingMouse =
            inputActions.Player.Look.activeControl?.device is Mouse;

        if (usingMouse)
        {
            yaw = lookInput.x * mouseSensitivity;
            pitchDelta = lookInput.y * mouseSensitivity;
        }
        else
        {
            yaw =
                lookInput.x *
                gamepadLookSpeed *
                Time.deltaTime;

            pitchDelta =
                lookInput.y *
                gamepadLookSpeed *
                Time.deltaTime;
        }

        // Rotación horizontal:
        // gira todo el personaje.
        transform.Rotate(Vector3.up * yaw);

        // Rotación vertical:
        // solamente gira la cámara.
        cameraPitch -= pitchDelta;

        cameraPitch = Mathf.Clamp(
            cameraPitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }
    */
}