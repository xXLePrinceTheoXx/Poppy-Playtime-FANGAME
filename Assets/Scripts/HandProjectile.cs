using UnityEngine;

public class HandProjectile : MonoBehaviour
{
    private Vector3 target;
    private float moveSpeed;
    private Grabpack controller;
    
    private Transform returnTarget;
    private bool isReturning = false;
    private bool isAnchored = false;

    public void Setup(Vector3 shootTarget, float speed, Grabpack ctrl)
    {
        target = shootTarget;
        moveSpeed = speed;
        controller = ctrl;
        isReturning = false;
        isAnchored = false;
    }

    void Update()
    {
        if (isReturning)
        {
            // Retour absolu vers le bras du joueur (Recul en ligne droite)
            transform.position = Vector3.MoveTowards(transform.position, returnTarget.position, moveSpeed * Time.deltaTime);
            
            // On ne modifie plus la rotation ici : la main reste droite et recule proprement sans se retourner !

            if (Vector3.Distance(transform.position, returnTarget.position) < 0.2f)
            {
                controller.OnHandReturned();
            }
        }
        else if (!isAnchored)
        {
            // Avance mathématique en ligne droite vers l'impact
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            // FORCE LA ROTATION EN CONTINU PENDANT LE VOL
            if (target - transform.position != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(target - transform.position);
                // On applique le -90° sur l'axe X à chaque image pour que la main garde sa correction
                transform.rotation = targetRot * Quaternion.Euler(-90f, 0f, 0f); 
            }

            // Si la portée max est atteinte sans mur, la main revient toute seule
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                controller.RecallHand();
            }
        }
    }

    public void StartReturn(Transform playerPivot, float speed)
    {
        returnTarget = playerPivot;
        moveSpeed = speed;
        isReturning = true;
        isAnchored = false;
        transform.SetParent(null); // Se détache du mur
    }

    // Gestion universelle des impacts (Trigger et Collision standard)
    private void OnTriggerEnter(Collider other) { CheckImpact(other.gameObject, other.transform); }
    private void OnCollisionEnter(Collision collision) { CheckImpact(collision.gameObject, collision.transform); }

    void CheckImpact(GameObject hitObject, Transform hitTransform)
    {
        if (isReturning || isAnchored) return;

        // Anti-collision joueur pour ne pas se bloquer au départ
        if (hitObject.CompareTag("Player") || hitObject.GetComponentInParent<CharacterController>() != null || hitObject.name.Contains("Player") || hitObject.name.Contains("Camera"))
        {
            return;
        }

        // Accrochage de la main sur l'objet touché
        isAnchored = true;
        transform.SetParent(hitTransform); 
    }
}
