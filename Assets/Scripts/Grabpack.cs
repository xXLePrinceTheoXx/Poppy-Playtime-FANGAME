using UnityEngine;
using UnityEngine.InputSystem;

public class Grabpack : MonoBehaviour
{
    public enum HandState { Idle, Shooting, Returning }

    [Header("Main Gauche (Bleue)")]
    public Transform leftHandPivot;
    public GameObject leftHandVisual;
    public GameObject leftHandPrefab;

    [Header("Main Droite (Violette)")]
    public Transform rightHandPivot;
    public GameObject rightHandVisual;
    public GameObject rightHandPrefab;

    [Header("Câbles")]
    public LineRenderer leftLine;
    public LineRenderer rightLine;

    [Header("Réglages")]
    public float shootDistance = 25f;
    public float speed = 35f;
    public float returnSpeed = 40f;

    private HandState leftState = HandState.Idle;
    private HandState rightState = HandState.Idle;
    private GameObject activeLeft;
    private GameObject activeRight;
    private Vector3 leftTarget;
    private Vector3 rightTarget;
    private Transform cam;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : transform;

        // Debug pour vérifier ce qui est configuré
        if (leftHandPrefab == null) Debug.LogError("[Grabpack] Left Hand Prefab manquant !");
        if (rightHandPrefab == null) Debug.LogError("[Grabpack] Right Hand Prefab manquant !");
        if (leftHandPivot == null) Debug.LogError("[Grabpack] Left Hand Pivot manquant !");
        if (rightHandPivot == null) Debug.LogError("[Grabpack] Right Hand Pivot manquant !");
        if (leftLine == null) Debug.LogWarning("[Grabpack] Left Line manquant !");
        if (rightLine == null) Debug.LogWarning("[Grabpack] Right Line manquant !");
    }

    void Update()
    {
        HandleInput();
        DrawLines();
    }

    void HandleInput()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[Grabpack] Clic gauche détecté");
            if (leftState == HandState.Idle) Shoot(HandSide.Left);
            else Recall(HandSide.Left);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            Debug.Log("[Grabpack] Clic droit détecté");
            if (rightState == HandState.Idle) Shoot(HandSide.Right);
            else Recall(HandSide.Right);
        }
    }

    void Shoot(HandSide side)
    {
        if (cam == null)
        {
            Debug.LogError("[Grabpack] Caméra introuvable !");
            return;
        }

        Transform pivot = side == HandSide.Left ? leftHandPivot : rightHandPivot;
        GameObject visual = side == HandSide.Left ? leftHandVisual : rightHandVisual;
        GameObject prefab = side == HandSide.Left ? leftHandPrefab : rightHandPrefab;

        string sideName = side == HandSide.Left ? "GAUCHE" : "DROITE";

        if (pivot == null)
        {
            Debug.LogError($"[Grabpack] Pivot {sideName} manquant !");
            return;
        }
        if (prefab == null)
        {
            Debug.LogError($"[Grabpack] Prefab {sideName} manquant !");
            return;
        }

        if (side == HandSide.Left) leftState = HandState.Shooting;
        else rightState = HandState.Shooting;

        if (visual != null) visual.SetActive(false);

        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, shootDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (side == HandSide.Left) leftTarget = hit.point;
            else rightTarget = hit.point;
        }
        else
        {
            if (side == HandSide.Left) leftTarget = ray.GetPoint(shootDistance);
            else rightTarget = ray.GetPoint(shootDistance);
        }

        float sideX = side == HandSide.Left ? -0.2f : 0.2f;
        Vector3 spawnPos = cam.TransformPoint(new Vector3(sideX, 0f, 0.5f));
        Quaternion spawnRot = cam.rotation * Quaternion.Euler(-90f, 0f, 0f);

        Debug.Log($"[Grabpack] Spawn main {sideName} à {spawnPos}");

        GameObject hand = Instantiate(prefab, spawnPos, spawnRot);
        hand.transform.SetParent(null);

        var proj = hand.GetComponent<HandProjectile>();
        if (proj != null)
            proj.Setup(side == HandSide.Left ? leftTarget : rightTarget, speed, this, side);
        else
            Debug.LogError($"[Grabpack] Le préfab {sideName} n'a pas de HandProjectile !");

        if (side == HandSide.Left) activeLeft = hand;
        else activeRight = hand;
    }

    public void Recall(HandSide side)
    {
        if (side == HandSide.Left)
        {
            if (leftState == HandState.Idle) return;
            leftState = HandState.Returning;
            if (activeLeft != null)
            {
                var proj = activeLeft.GetComponent<HandProjectile>();
                if (proj != null) proj.StartReturn(leftHandPivot, returnSpeed);
            }
        }
        else
        {
            if (rightState == HandState.Idle) return;
            rightState = HandState.Returning;
            if (activeRight != null)
            {
                var proj = activeRight.GetComponent<HandProjectile>();
                if (proj != null) proj.StartReturn(rightHandPivot, returnSpeed);
            }
        }
    }

    public void OnHandReturned(HandSide side)
    {
        if (side == HandSide.Left)
        {
            leftState = HandState.Idle;
            if (leftLine != null) leftLine.positionCount = 0;
            if (leftHandVisual != null) leftHandVisual.SetActive(true);
            if (activeLeft != null) Destroy(activeLeft);
            activeLeft = null;
        }
        else
        {
            rightState = HandState.Idle;
            if (rightLine != null) rightLine.positionCount = 0;
            if (rightHandVisual != null) rightHandVisual.SetActive(true);
            if (activeRight != null) Destroy(activeRight);
            activeRight = null;
        }
    }

    void DrawLines()
    {
        if (leftState != HandState.Idle && activeLeft != null && leftLine != null)
        {
            leftLine.positionCount = 2;
            leftLine.SetPosition(0, leftHandPivot.position);
            var mr = activeLeft.GetComponentInChildren<MeshRenderer>();
            leftLine.SetPosition(1, mr != null ? mr.bounds.center : activeLeft.transform.position);
        }
        else if (leftLine != null) leftLine.positionCount = 0;

        if (rightState != HandState.Idle && activeRight != null && rightLine != null)
        {
            rightLine.positionCount = 2;
            rightLine.SetPosition(0, rightHandPivot.position);
            var mr = activeRight.GetComponentInChildren<MeshRenderer>();
            rightLine.SetPosition(1, mr != null ? mr.bounds.center : activeRight.transform.position);
        }
        else if (rightLine != null) rightLine.positionCount = 0;
    }
}

public enum HandSide { Left, Right }