using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Déplacements")]
    public float walkSpeed = 5.0f;
    public float sprintSpeed = 8.0f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Détection du Sol")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer; 

    [Header("Caméra et Souris")]
    public Transform playerCamera;
    public float mouseSensitivity = 15.0f;
    public float lookUpLimit = -80.0f;
    public float lookDownLimit = 80.0f;

    [Header("Interface Menu (Pause)")]
    public GameObject menuPausePanel; // Glissez votre panneau UI ici
    private bool isMenuOpen = false;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float currentSpeed;
    private bool isGrounded;

    // Référence pour vos animations
    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Tente de récupérer l'Animator sur le joueur ou sur ses mains enfants (Grabpack)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (groundLayer.value == 0) groundLayer = ~0;

        // Force la fermeture du menu au lancement
        ResumeGame(); 
    }

    void Update()
    {
        HandleMenuInput();

        // Si le menu est ouvert, on bloque les mouvements, la caméra et on force l'arrêt de l'animation
        if (isMenuOpen) 
        {
            if (animator != null) animator.SetBool("isWalking", false);
            return;
        }

        CheckGroundStatus();
        HandleRotation();
        HandleMovement();
    }

    void HandleMenuInput()
    {
        Keyboard currentKeyboard = InputSystem.GetDevice<Keyboard>();
        if (currentKeyboard == null) return;

        // Détection de la touche Échap (escapeKey)
        if (currentKeyboard.escapeKey.wasPressedThisFrame)
        {
            if (isMenuOpen)
                ResumeGame();
            else
                OpenMenu();
        }
    }

    public void OpenMenu()
    {
        isMenuOpen = true;
        Time.timeScale = 0f; // Fige le temps dans le jeu (physique, animations...)
        
        if (menuPausePanel != null) menuPausePanel.SetActive(true);

        // Libère le curseur pour pouvoir interagir avec l'UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isMenuOpen = false;
        Time.timeScale = 1f; // Reprend le temps normal du jeu
        
        if (menuPausePanel != null) menuPausePanel.SetActive(false);

        // Bloque et cache à nouveau la souris dans le jeu
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Fonctions publiques appelées par les Sliders de votre menu pour configurer en temps réel
    public void SetSensitivity(float newSensitivity) { mouseSensitivity = newSensitivity; }
    public void SetWalkSpeed(float newSpeed) { walkSpeed = newSpeed; }
    public void SetSprintSpeed(float newSpeed) { sprintSpeed = newSpeed; }
    public void SetJumpHeight(float newHeight) { jumpHeight = newHeight; }

    void CheckGroundStatus()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, groundCheckDistance + 0.1f, groundLayer);
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

    void HandleMovement()
    {
        Keyboard currentKeyboard = InputSystem.GetDevice<Keyboard>();
        if (currentKeyboard == null) return;

        if (currentKeyboard.leftShiftKey.isPressed)
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        float moveX = 0f;
        float moveZ = 0f;

        if (currentKeyboard.wKey.isPressed || currentKeyboard.upArrowKey.isPressed) moveZ = 1f;
        if (currentKeyboard.sKey.isPressed || currentKeyboard.downArrowKey.isPressed) moveZ = -1f;
        if (currentKeyboard.aKey.isPressed || currentKeyboard.leftArrowKey.isPressed) moveX = -1f;
        if (currentKeyboard.dKey.isPressed || currentKeyboard.rightArrowKey.isPressed) moveX = 1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        // --- GESTION STRICTE DE L'ANIMATION DE MARCHE ---
        if (animator != null)
        {
            // Si l'une des touches directionnelles est active
            if (moveX != 0f || moveZ != 0f)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                // Si aucune touche n'est détectée, on force impérativement le retour à l'Idle
                animator.SetBool("isWalking", false);
            }
        }

        if (isGrounded)
        {
            if (velocity.y < 0) velocity.y = -2f;

            if (currentKeyboard.spaceKey.isPressed)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}
