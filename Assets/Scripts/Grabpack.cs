using UnityEngine;
using UnityEngine.InputSystem;

public class Grabpack : MonoBehaviour
{
    public enum HandState { Idle, Shooting, Returning }

    [Header("Bras et Origines")]
    public Transform leftHandPivot;       // Ton JNT_L_HandAttachment_Pivot
    public GameObject leftHandVisual;     // Ta main fixe attachée à ton bras

    [Header("Projectiles Prefabs")]
    [Tooltip("Glisse ici ton prefab de projectile nommé Hand_Blue")]
    public GameObject blueHandPrefab;     

    [Header("Line Renderers (Câbles)")]
    public LineRenderer blueLine;         

    [Header("Paramètres")]
    public float shootDistance = 25f;      
    public float speed = 35f;             
    public float returnSpeed = 40f;       

    [Header("Ajustements de Visée")]
    [Tooltip("Décale le point de spawn par rapport à la caméra (X=gauche/droite, Y=haut/bas, Z=avant)")]
    public Vector3 cameraSpawnOffset = new Vector3(-0.3f, 0f, 0.5f);     

    private HandState blueState = HandState.Idle;
    private GameObject activeBlueHand;
    private Vector3 targetPoint;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : transform;
        
        if (blueLine != null)
        {
            blueLine.enabled = true;
            blueLine.positionCount = 0;
        }
    }

    void Update()
    {
        HandleInput();
        HandleCableRendering();
    }

    void HandleInput()
    {
        Mouse currentMouse = Mouse.current;
        if (currentMouse == null) return;

        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            if (blueState == HandState.Idle)
            {
                ShootHand();
            }
            else
            {
                RecallHand();
            }
        }
    }

    void ShootHand()
    {
        if (cameraTransform == null || leftHandPivot == null || blueHandPrefab == null) return;

        blueState = HandState.Shooting;
        if (leftHandVisual != null) leftHandVisual.SetActive(false);

        // 1. CALCUL DE LA CIBLE (Pile au milieu du viseur de l'écran)
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(shootDistance);
        }

        // 2. FORCE LE DÉPART AU MILIEU DE L'ÉCRAN
        // On fait apparaître le projectile pile sur la ligne de visée centrale de la caméra (X=0, Y=0)
        // On le décale juste un tout petit peu vers l'avant (Z=0.5) pour qu'il ne sorte pas de derrière ta tête
        Vector3 centerScreenSpawn = cameraTransform.TransformPoint(new Vector3(0f, 0f, 0.5f));

        // 3. APPLIQUE LA ROTATION DE DÉPART ALIGNÉE SUR LA CAMÉRA
        Quaternion spawnRotation = cameraTransform.rotation;

        // 4. APPLIQUE LE REDRESSEMENT À X: -90
        // On ajoute la rotation sur l'axe X pour corriger le modèle 3D au moment précis du clic
        spawnRotation *= Quaternion.Euler(-90f, 0f, 0f);

        // Crée le clone avec le centrage parfait et la bonne rotation
        activeBlueHand = Instantiate(blueHandPrefab, centerScreenSpawn, spawnRotation);
        
        activeBlueHand.SetActive(true);
        activeBlueHand.transform.SetParent(null);

        // Envoie les infos de course au script de la main volante
        HandProjectile proj = activeBlueHand.GetComponent<HandProjectile>();
        if (proj != null)
        {
            proj.Setup(targetPoint, speed, this);
        }
    }

    public void RecallHand()
    {
        if (blueState == HandState.Idle) return;
        blueState = HandState.Returning;

        if (activeBlueHand != null)
        {
            HandProjectile proj = activeBlueHand.GetComponent<HandProjectile>();
            if (proj != null) proj.StartReturn(leftHandPivot, returnSpeed);
        }
    }

    public void OnHandReturned()
    {
        blueState = HandState.Idle;
        if (blueLine != null) blueLine.positionCount = 0;
        if (leftHandVisual != null) leftHandVisual.SetActive(true);
        if (activeBlueHand != null) Destroy(activeBlueHand);
    }

    void HandleCableRendering()
    {
        if (blueState != HandState.Idle && activeBlueHand != null && blueLine != null)
        {
            blueLine.positionCount = 2;
            // Point 0 : Fixé sur ton bras
            blueLine.SetPosition(0, leftHandPivot.position);

            // SOLUTION SANS CABLE ANCHOR : On va chercher le centre géométrique réel (Bounds) du Mesh
            MeshRenderer meshRenderer = activeBlueHand.GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
            {
                // Le script calcule le centre visuel exact de la main, là où se trouve la géométrie 3D !
                blueLine.SetPosition(1, meshRenderer.bounds.center);
            }
            else
            {
                // Sécurité par défaut sur le centre du clone
                blueLine.SetPosition(1, activeBlueHand.transform.position);
            }
        }
    }
}