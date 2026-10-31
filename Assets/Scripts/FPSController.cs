using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Déplacements")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.0f;
    public float crouchSpeed = 4.0f;
    public float jumpHeight = 3.0f;
    public float gravity = -9.81f;

    [Header("Options Accroupi (Crouch)")]
    public float normalHeight = 2.0f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 10f;

    [Header("Détection du Sol")]
    public float groundCheckDistance = 0.5f;
    public LayerMask groundLayer; 

    [Header("Caméra et Souris")]
    public Transform playerCamera;
    public float mouseSensitivity = 15.0f;
    public float lookUpLimit = -80.0f;
    public float lookDownLimit = 80.0f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float currentSpeed;
    private bool isGrounded;
    private float targetHeight;

    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (groundLayer.value == 0) groundLayer = ~0;

        targetHeight = normalHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMenuInput();

        if (Time.timeScale == 0f) return;

        CheckGroundStatus();
        HandleRotation();
        HandleCrouchTransition();
        HandleMovement();
    }

    void HandleMenuInput()
    {
        Keyboard currentKeyboard = InputSystem.GetDevice<Keyboard>();
        if (currentKeyboard == null) return;

        if (currentKeyboard.escapeKey.wasPressedThisFrame)
        {
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void SetSensitivity(float newSensitivity) { mouseSensitivity = newSensitivity; }
    public void SetWalkSpeed(float newSpeed) { walkSpeed = newSpeed; }
    public void SetSprintSpeed(float newSpeed) { sprintSpeed = newSpeed; }
    public void SetJumpHeight(float newHeight) { jumpHeight = newHeight; }

    void CheckGroundStatus()
    {
        isGrounded = characterController.isGrounded;
        if (!isGrounded)
        {
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance + 0.1f, groundLayer);
        }
    }

    void HandleRotation()
    {
        if (playerCamera == null) return;

        Mouse currentMouse = InputSystem.GetDevice<Mouse>();
        if (currentMouse == null) return;

        Vector2 mouseDelta = currentMouse.delta.ReadValue();

        float rotateHorizontal = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float rotateVertical = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * rotateHorizontal);

        verticalRotation -= rotateVertical;
        verticalRotation = Mathf.Clamp(verticalRotation, lookUpLimit, lookDownLimit);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    void HandleCrouchTransition()
    {
        characterController.height = Mathf.Lerp(characterController.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
    }

    void HandleMovement()
    {
        Keyboard currentKeyboard = InputSystem.GetDevice<Keyboard>();
        if (currentKeyboard == null) return;

        bool isCrouching = currentKeyboard.leftCtrlKey.isPressed;

        if (isCrouching)
        {
            targetHeight = crouchHeight;
            currentSpeed = crouchSpeed;
        }
        else
        {
            targetHeight = normalHeight;
            if (currentKeyboard.leftShiftKey.isPressed)
                currentSpeed = sprintSpeed;
            else
                currentSpeed = walkSpeed;
        }

        float moveX = 0f;
        float moveZ = 0f;

        if (currentKeyboard.wKey.isPressed || currentKeyboard.upArrowKey.isPressed) moveZ = 1f;
        if (currentKeyboard.sKey.isPressed || currentKeyboard.downArrowKey.isPressed) moveZ = -1f;
        if (currentKeyboard.aKey.isPressed || currentKeyboard.leftArrowKey.isPressed) moveX = -1f;
        if (currentKeyboard.dKey.isPressed || currentKeyboard.rightArrowKey.isPressed) moveX = 1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        // --- ANIMATIONS ---
        if (animator != null)
        {
            bool isMoving = (moveX != 0f || moveZ != 0f);
            bool isRunning = isMoving && currentKeyboard.leftShiftKey.isPressed;

            // isWalking = vrai seulement si on marche ET qu'on ne court pas
            animator.SetBool("isWalking", isMoving && !isRunning);
            animator.SetBool("isRunning", isRunning);
        }

        if (isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;

            if (currentKeyboard.spaceKey.isPressed && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}