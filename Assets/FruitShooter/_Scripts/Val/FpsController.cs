/*
Reference: ChatGPT Conversation
Conversation: Adaptar cámara y disparo
Game: Fruit Shooter
Game Type: 3D
Date: 13/11/2026
Purpose: La IA me ayudo a cambiar el metodo de Look ya que ya no quieren que haya un joystick derecho
https://chatgpt.com/share/6af79f90-4eb0-83e8-b707-d1db2f414168
*/

using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;

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
    [SerializeField] private float lookSensitivity = .15f;
    [SerializeField] private float lookSmoothness = 0.05f;
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
    private bool isDead;

    private Vector3 dashDirection;
    private Vector3 slideDirection;

    // LOOK
    private float cameraPitch;
    private float currentLookX;
    private float currentLookY;
    private float lookVelocityX;
    private float lookVelocityY;
    private List<int> lookFingerIds = new List<int>();
    [SerializeField] private RectTransform[] uiAreasToIgnore;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (!cameraTransform) { cameraTransform=Camera.main.transform;}

        inputActions = new DefaultInputActions();

        normalHeight = controller.height;
        normalCenter = controller.center;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
        EnhancedTouchSupport.Disable();
        lookFingerIds.Clear();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }
        Move();
        UpdateDash();
        UpdateSlide();
        Look();
    }

    private void Move()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move = transform.right*input.x+ transform.forward*input.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity= -2f;
        }

        if (canJump && controller.isGrounded && !isSliding && !isDashing)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight*-2f*gravity);
        }

        canJump = false;

        verticalVelocity += gravity*Time.deltaTime;

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
            animator.SetBool("IsWalking",input.magnitude > 0.01f && !isDashing && !isSliding);
        }

        if (animator != null)
        {
            animator.SetBool("IsWalking",input.magnitude > 0.01f && !isDashing && !isSliding);

            animator.SetBool("IsJumping",!controller.isGrounded && !isDashing && !isSliding);
        }
    }

    public void Jump()
    {
        if (isDead || isDashing || isSliding)
        {
            return;
        }
        
        canJump = true;
    }

    // DASH

    public void StartDash()
    {
        if (isDead || isDashing || isSliding || !controller.isGrounded)
        {
            return;
        }

        Vector2 input =inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction= transform.right*input.x+transform.forward*input.y;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction= transform.forward;
        }

        dashDirection= direction.normalized;

        isDashing= true;
        dashTimer= dashDuration;

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

        controller.Move((dashDirection * dashSpeed + Vector3.up * verticalVelocity)*Time.deltaTime);

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
        if (isDead || isDashing || isSliding || !controller.isGrounded)
        {
            return;
        }

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 direction = transform.right*input.x+transform.forward*input.y;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = transform.forward;
        }

        slideDirection = direction.normalized;

        isSliding = true;
        slideTimer = slideDuration;

        controller.height = slideHeight;
        controller.center = new Vector3(normalCenter.x,slideHeight / 2f,normalCenter.z);

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

        float currentSlideSpeed = slideSpeed*Mathf.Clamp01(slideTimer / slideDuration);

        controller.Move((slideDirection * currentSlideSpeed+Vector3.up*verticalVelocity)*Time.deltaTime);

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

    //DIE

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        canJump = false;
        isDashing = false;
        isSliding = false;

        verticalVelocity = 0f;

        controller.height = normalHeight;
        controller.center = normalCenter;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsDashing", false);
            animator.SetBool("IsSliding", false);
            animator.SetBool("IsJumping", false);

            animator.SetBool("IsDead", true);
        }

        Debug.Log("Player has died.");
    }

    // LOOK

    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        for (int i=0; i<uiAreasToIgnore.Length;i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(uiAreasToIgnore[i], touchPosition))
            {
                return true;
            }
        }

        return false;
    }
    private void Look()
    {
        Vector2 lookInput = Vector2.zero;

        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                if (!IsTouchOverUI(touch.screenPosition))
                {
                    lookFingerIds.Add(touch.touchId);
                }
            }

            if (lookFingerIds.Contains(touch.touchId))
            {
                if (touch.phase==UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    lookInput += touch.delta;
                }
            }

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended||touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                lookFingerIds.Remove(touch.touchId);
            }
        }

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

        float yaw = currentLookX*lookSensitivity;
        float pitch = currentLookY*lookSensitivity;

        transform.Rotate(Vector3.up*yaw);

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