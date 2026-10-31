using UnityEngine;
using UnityEngine.InputSystem;

public class Grabpack : MonoBehaviour
{
    public enum HandState { Idle, Shooting, Returning }

    [Header("Bras et Origines - Main Gauche (Bleue)")]
    public Transform leftHandPivot;
    public GameObject leftHandVisual;

    [Header("Bras et Origines - Main Droite (Rouge)")]
    public Transform rightHandPivot;
    public GameObject rightHandVisual;

    [Header("Projectiles Prefabs")]
    [Tooltip("Glisse ici ton prefab de main bleue (Hand_Blue)")]
    public GameObject blueHandPrefab;

    [Tooltip("Glisse ici ton prefab de main rouge (Hand_Red)")]
    public GameObject redHandPrefab;

    [Header("Line Renderers (Câbles)")]
    public LineRenderer blueLine;
    public LineRenderer redLine;

    [Header("Paramètres")]
    public float shootDistance = 25f;
    public float speed = 35f;
    public float returnSpeed = 40f;

    [Header("Ajustements de Visée")]
    [Tooltip("Décale le point de spawn par rapport à la caméra (X=gauche/droite, Y=haut/bas, Z=avant)")]
    public Vector3 cameraSpawnOffset = new Vector3(-0.3f, 0f, 0.5f);

    // État Main Gauche
    private HandState blueState = HandState.Idle;
    private GameObject activeBlueHand;
    private Vector3 blueTargetPoint;

    // État Main Droite
    private HandState redState = HandState.Idle;
    private GameObject activeRedHand;
    private Vector3 redTargetPoint;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main != null ? Camera.main.transform : transform;

        if (blueLine != null)
        {
            blueLine.enabled = true;
            blueLine.positionCount = 0;
        }

        if (redLine != null)
        {
            redLine.enabled = true;
            redLine.positionCount = 0;
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

        // --- CLIC GAUCHE : MAIN BLEUE ---
        if (currentMouse.leftButton.wasPressedThisFrame)
        {
            if (blueState == HandState.Idle)
                ShootLeftHand();
            else
                RecallLeftHand();
        }

        // --- CLIC DROIT : MAIN ROUGE ---
        if (currentMouse.rightButton.wasPressedThisFrame)
        {
            if (redState == HandState.Idle)
                ShootRightHand();
            else
                RecallRightHand();
        }
    }

    // ============ MAIN GAUCHE (BLEUE) ============

    void ShootLeftHand()
    {
        if (cameraTransform == null || leftHandPivot == null || blueHandPrefab == null) return;

        blueState = HandState.Shooting;
        if (leftHandVisual != null) leftHandVisual.SetActive(false);

        // 1. CALCUL DE LA CIBLE
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            blueTargetPoint = hit.point;
        }
        else
        {
            blueTargetPoint = ray.GetPoint(shootDistance);
        }

        // 2. SPAWN AU MILIEU DE L'ÉCRAN
        Vector3 centerScreenSpawn = cameraTransform.TransformPoint(new Vector3(0f, 0f, 0.5f));

        // 3. ROTATION
        Quaternion spawnRotation = cameraTransform.rotation;
        spawnRotation *= Quaternion.Euler(-90f, 0f, 0f);

        activeBlueHand = Instantiate(blueHandPrefab, centerScreenSpawn, spawnRotation);
        activeBlueHand.SetActive(true);
        activeBlueHand.transform.SetParent(null);

        HandProjectile proj = activeBlueHand.GetComponent<HandProjectile>();
        if (proj != null)
        {
            proj.Setup(blueTargetPoint, speed, this, HandSide.Left);
        }
    }

    public void RecallLeftHand()
    {
        if (blueState == HandState.Idle) return;
        blueState = HandState.Returning;

        if (activeBlueHand != null)
        {
            HandProjectile proj = activeBlueHand.GetComponent<HandProjectile>();
            if (proj != null) proj.StartReturn(leftHandPivot, returnSpeed);
        }
    }

    public void OnLeftHandReturned()
    {
        blueState = HandState.Idle;
        if (blueLine != null) blueLine.positionCount = 0;
        if (leftHandVisual != null) leftHandVisual.SetActive(true);
        if (activeBlueHand != null) Destroy(activeBlueHand);
        activeBlueHand = null;
    }

    // ============ MAIN DROITE (ROUGE) ============

    void ShootRightHand()
    {
        if (cameraTransform == null || rightHandPivot == null || redHandPrefab == null) return;

        redState = HandState.Shooting;
        if (rightHandVisual != null) rightHandVisual.SetActive(false);

        // 1. CALCUL DE LA CIBLE
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            redTargetPoint = hit.point;
        }
        else
        {
            redTargetPoint = ray.GetPoint(shootDistance);
        }

        // 2. SPAWN AU MILIEU DE L'ÉCRAN
        Vector3 centerScreenSpawn = cameraTransform.TransformPoint(new Vector3(0f, 0f, 0.5f));

        // 3. ROTATION
        Quaternion spawnRotation = cameraTransform.rotation;
        spawnRotation *= Quaternion.Euler(-90f, 0f, 0f);

        activeRedHand = Instantiate(redHandPrefab, centerScreenSpawn, spawnRotation);
        activeRedHand.SetActive(true);
        activeRedHand.transform.SetParent(null);

        HandProjectile proj = activeRedHand.GetComponent<HandProjectile>();
        if (proj != null)
        {
            proj.Setup(redTargetPoint, speed, this, HandSide.Right);
        }
    }

    public void RecallRightHand()
    {
        if (redState == HandState.Idle) return;
        redState = HandState.Returning;

        if (activeRedHand != null)
        {
            HandProjectile proj = activeRedHand.GetComponent<HandProjectile>();
            if (proj != null) proj.StartReturn(rightHandPivot, returnSpeed);
        }
    }

    public void OnRightHandReturned()
    {
        redState = HandState.Idle;
        if (redLine != null) redLine.positionCount = 0;
        if (rightHandVisual != null) rightHandVisual.SetActive(true);
        if (activeRedHand != null) Destroy(activeRedHand);
        activeRedHand = null;
    }

    // ============ CÂBLES ============

    void HandleCableRendering()
    {
        // Câble bleu (main gauche)
        if (blueState != HandState.Idle && activeBlueHand != null && blueLine != null)
        {
            blueLine.positionCount = 2;
            blueLine.SetPosition(0, leftHandPivot.position);

            MeshRenderer meshRenderer = activeBlueHand.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                blueLine.SetPosition(1, meshRenderer.bounds.center);
            }
            else
            {
                blueLine.SetPosition(1, activeBlueHand.transform.position);
            }
        }
        else if (blueLine != null)
        {
            blueLine.positionCount = 0;
        }

        // Câble rouge (main droite)
        if (redState != HandState.Idle && activeRedHand != null && redLine != null)
        {
            redLine.positionCount = 2;
            redLine.SetPosition(0, rightHandPivot.position);

            MeshRenderer meshRenderer = activeRedHand.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                redLine.SetPosition(1, meshRenderer.bounds.center);
            }
            else
            {
                redLine.SetPosition(1, activeRedHand.transform.position);
            }
        }
        else if (redLine != null)
        {
            redLine.positionCount = 0;
        }
    }

    // ============ MÉTHODES PUBLIQUES APPELÉES PAR HandProjectile ============

    public void OnHandReturned(HandSide side)
    {
        if (side == HandSide.Left)
            OnLeftHandReturned();
        else
            OnRightHandReturned();
    }
}

// Côté de la main pour distinguer bleu/rouge
public enum HandSide { Left, Right }