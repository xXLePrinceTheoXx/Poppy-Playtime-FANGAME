using UnityEngine;
using UnityEngine.InputSystem;

public class Grabpack : MonoBehaviour
{
    [Header("Mains et Pivot")]
    public GameObject handPrefab;          
    public Transform leftHandPivot;        // "JNT_L_HandAttachment_Pivot"
    public GameObject leftHandVisual;      // La main fixe à masquer
    public Transform playerCamera;         // Glisse ta caméra principale ici !

    [Header("Paramètres du Câble")]
    public float shootForce = 35f;
    
    private LineRenderer cableRenderer;
    private GameObject activeHand;
    private bool isHandOut = false;

    void Start()
    {
        if (leftHandPivot != null)
        {
            cableRenderer = leftHandPivot.GetComponent<LineRenderer>();
            if (cableRenderer != null) cableRenderer.positionCount = 0;
        }

        // Si la caméra n'est pas assignée, on cherche la caméra principale
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        Mouse currentMouse = Mouse.current;
        if (currentMouse == null) return;

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            if (!isHandOut)
                LaunchHand();
            else
                RetractHand();
        }

        // MISE À JOUR DU CÂBLE EN COORDONNÉES MONDE
        if (isHandOut && activeHand != null && cableRenderer != null)
        {
            cableRenderer.positionCount = 2;
            cableRenderer.SetPosition(0, leftHandPivot.position);
            cableRenderer.SetPosition(1, activeHand.transform.position);
        }
    }

    void LaunchHand()
    {
        if (leftHandPivot == null || handPrefab == null || playerCamera == null) return;

        isHandOut = true;

        if (leftHandVisual != null) leftHandVisual.SetActive(false);

        // Crée la main au niveau du pivot, mais l'oriente vers là où regarde la caméra
        activeHand = Instantiate(handPrefab, leftHandPivot.position, playerCamera.rotation);
        Rigidbody rb = activeHand.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // Supprime la gravité pour qu'elle file tout droit comme dans le vrai jeu
            rb.useGravity = false; 
            // Propulse la main dans la direction de la caméra
            rb.linearVelocity = playerCamera.forward * shootForce;
        }
    }

    void RetractHand()
    {
        isHandOut = false;

        if (cableRenderer != null) cableRenderer.positionCount = 0;
        if (activeHand != null) Destroy(activeHand);
        if (leftHandVisual != null) leftHandVisual.SetActive(true);
    }
}
