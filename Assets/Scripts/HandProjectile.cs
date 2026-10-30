using UnityEngine;

public class HandProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si la main a déjà touché un vrai mur, on ne fait rien
        if (hasHit) return;

        // --- SÉCURITÉ ANTI-COLLISION JOUEUR ---
        // On ignore si l'objet s'appelle "Player", a le tag "Player", 
        // ou si son parent ou ses enfants contiennent le script du joueur
        if (collision.gameObject.CompareTag("Player") || 
            collision.gameObject.name.Contains("Player") ||
            collision.gameObject.GetComponentInParent<FPSController>() != null)
        {
            return; // On sort de la fonction sans bloquer la main !
        }

        // Si on arrive ici, c'est qu'on a touché un vrai décor (sol, mur, bouton...)
        hasHit = true;
        
        // Bloque instantanément la physique pour que la main reste figée sur le mur
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Attache la main à l'objet touché pour qu'elle reste collée au mur
        transform.SetParent(collision.transform);
    }
}
