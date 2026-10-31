using UnityEngine;

public class HandProjectile : MonoBehaviour
{
    private Vector3 target;
    private float speed;
    private Grabpack controller;
    private HandSide side;

    private bool returning = false;
    private bool anchored = false;
    private Transform returnTarget;
    private Rigidbody rb;

    public void Setup(Vector3 shootTarget, float spd, Grabpack ctrl, HandSide s)
    {
        target = shootTarget;
        speed = spd;
        controller = ctrl;
        side = s;
        returning = false;
        anchored = false;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (returning)
        {
            transform.position = Vector3.MoveTowards(transform.position, returnTarget.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, returnTarget.position) < 0.2f)
                controller.OnHandReturned(side);
        }
        else if (!anchored)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, target) < 0.05f)
                controller.OnHandReturned(side);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (returning || anchored) return;
        if (other.CompareTag("Player") || other.GetComponentInParent<CharacterController>() != null) return;

        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = Vector3.up;

        Ray ray = new Ray(transform.position, (target - transform.position).normalized);
        if (other.Raycast(ray, out RaycastHit hit, 2f))
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
        }

        Anchor(hitPoint, hitNormal, other.transform);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (returning || anchored) return;
        if (collision.gameObject.CompareTag("Player")) return;

        var contact = collision.contacts[0];
        Anchor(contact.point, contact.normal, collision.transform);
    }

    void Anchor(Vector3 point, Vector3 normal, Transform parent)
    {
        anchored = true;
        transform.SetParent(null);
        transform.position = point;
        transform.rotation = Quaternion.LookRotation(-normal) * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.SetParent(parent);
    }

    public void StartReturn(Transform pivot, float spd)
    {
        returning = true;
        anchored = false;
        speed = spd;
        returnTarget = pivot;
        transform.SetParent(null);
    }
}