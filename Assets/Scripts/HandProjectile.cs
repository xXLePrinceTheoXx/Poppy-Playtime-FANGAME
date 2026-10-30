using UnityEngine;

public class HandProjectile : MonoBehaviour
{
    private Vector3 target;
    private float moveSpeed;
    private Grabpack controller;
    
    private Transform returnTarget;
    private bool isReturning = false;
    private bool isAnchored = false;
    private Rigidbody rb;
    private Collider myCollider;

    public void Setup(Vector3 shootTarget, float speed, Grabpack ctrl)
    {
        target = shootTarget;
        moveSpeed = speed;
        controller = ctrl;
        isReturning = false;
        isAnchored = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        // Force les réglages du Rigidbody pour la précision physique
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true; // On gère le mouvement par code
        }
    }

    void Update()
    {
        if (isReturning)
        {
            // Retour absolu vers le bras du joueur (Recul en ligne droite)
            transform.position = Vector3.MoveTowards(transform.position, returnTarget.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, returnTarget.position) < 0.2f)
            {
                controller.OnHandReturned();
            }
        }
        else if (!isAnchored)
        {
            // Déplacement linéaire vers la cible
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            // Si la portée maximale est atteinte aux coordonnées de visée, elle revient
            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                controller.RecallHand();
            }
        }
    }

    // GESTION INFAILLIBLE DES COLLISONS PHYSIQUES (Quand Is Trigger est COCHÉ)
    private void OnTriggerEnter(Collider other)
    {
        if (isReturning || isAnchored) return;

        // Anti-collision joueur et caméra
        if (other.CompareTag("Player") || other.name.Contains("Player") || other.name.Contains("Camera") || other.GetComponentInParent<CharacterController>() != null)
        {
            return;
        }

        // Calcule le point d'impact de surface exact et sa normale sur l'objet touché
        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = Vector3.up; // Valeur par défaut (sol horizontal)

        // Tente de projeter un rayon très court en arrière pour chopper la face du mur traversé
        Ray ray = new Ray(transform.position + (target - transform.position).normalized * -0.5f, (target - transform.position).normalized);
        if (other.Raycast(ray, out RaycastHit hit, 2f))
        {
            hitPoint = hit.point;
            hitNormal = hit.normal;
        }

        // Bloque et oriente la main
        LockOnHand(hitPoint, hitNormal, other.transform);
    }

    // SÉCURITÉ SI IS TRIGGER EST DÉCOCHÉ
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

        // Éjecte le parent de toute hiérarchie pour figer ses coordonnées globales
        transform.SetParent(null);

        // Positionne la main exactement à l'endroit de l'impact de surface
        transform.position = hitPoint;

        // ALIGNEMENT : Applique l'angle du mur + le redressement à -90 sur l'axe X de la main
        Quaternion surfaceRotation = Quaternion.LookRotation(-surfaceNormal);
        transform.rotation = surfaceRotation * Quaternion.Euler(-90f, 0f, 0f);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Accroche définitivement la main sur l'élément du décor
        transform.SetParent(hitTransform);
    }

    public void StartReturn(Transform playerPivot, float speed)
    {
        returnTarget = playerPivot;
        moveSpeed = speed;
        isReturning = true;
        isAnchored = false;
        transform.SetParent(null); // Se détache du mur
    }
}
