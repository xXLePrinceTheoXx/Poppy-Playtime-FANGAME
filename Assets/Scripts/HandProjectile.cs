using UnityEngine;

public class HandProjectile : MonoBehaviour
{
    private Vector3 target;
    private float moveSpeed;
    private Grabpack controller;
    private HandSide handSide;
    
    private Transform returnTarget;
    private bool isReturning = false;
    private bool isAnchored = false;
    private Rigidbody rb;
    private Collider myCollider;

    public void Setup(Vector3 shootTarget, float speed, Grabpack ctrl, HandSide side)
    {
        target = shootTarget;
        moveSpeed = speed;
        controller = ctrl;
        handSide = side;
        isReturning = false;
        isAnchored = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (isReturning)
        {
            transform.position = Vector3.MoveTowards(transform.position, returnTarget.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, returnTarget.position) < 0.2f)
            {
                controller.OnHandReturned(handSide);
            }
        }
        else if (!isAnchored)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                // Rappel automatique quand la portée est atteinte
                controller.OnHandReturned(handSide);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isReturning || isAnchored) return;

        if (other.CompareTag("Player") || other.name.Contains("Player") || other.name.Contains("Camera") || other.GetComponentInParent<CharacterController>() != null)
        {
            return;
        }

        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = Vector3.up;

        Ray ray = new Ray(transform.position + (target - transform.position).normalized * -0.5f, (target - transform.position).normalized);
        if (other.Raycast(ray, out RaycastHit hit, 2f))
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
        }

        LockOnHand(hitPoint, hitNormal, other.transform);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isReturning || isAnchored) return;

        GameObject hitObject = collision.gameObject;

        if (hitObject.CompareTag("Player") || hitObject.name.Contains("Player") || hitObject.name.Contains("Camera"))
        {
            return;
        }

        ContactPoint contact = collision.contacts[0];
        LockOnHand(contact.point, contact.normal, collision.transform);
    }

    void LockOnHand(Vector3 hitPoint, Vector3 surfaceNormal, Transform hitTransform)
    {
        isAnchored = true;

        transform.SetParent(null);
        transform.position = hitPoint;

        Quaternion surfaceRotation = Quaternion.LookRotation(-surfaceNormal);
        transform.rotation = surfaceRotation * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.SetParent(hitTransform);
    }

    public void StartReturn(Transform playerPivot, float speed)
    {
        returnTarget = playerPivot;
        moveSpeed = speed;
        isReturning = true;
        isAnchored = false;
        transform.SetParent(null);
    }
}