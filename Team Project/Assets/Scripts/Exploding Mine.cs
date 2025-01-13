using UnityEngine;

public class ExplodingMine : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] float triggerRadius;  // Activation radius of the mine
    [SerializeField] float explosionRadius; // Damage radius of the mine
    [SerializeField] int explosionDamage;

    private SphereCollider triggerCollider;  // The trigger collider for detecting players/enemies
    private bool isTriggered = false;  // Preventing multiple activations

    private void Awake()
    {
        // Setup the trigger collider
        triggerCollider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // When a player or enemy enters the trigger radius, the mine activates
        if (!isTriggered && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            isTriggered = true;  // Prevent multiple activations
            ActivateMine();
        }
    }

    private void ActivateMine()
    {
        // When the mine is stepped on, it takes enough damage to activate
        takeDamage(HP);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Disable the trigger collider to prevent further triggers during the explosion
        triggerCollider.enabled = false;

        // Get all colliders in the explosion radius and apply damage
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            // Check if the collider has the IDamage interface and is not a trigger
            IDamage damageable = collider.GetComponent<IDamage>();
            // Apply explosion damage to anything that is damageable
            if (damageable != null && !collider.isTrigger)
            {
                damageable.takeDamage(explosionDamage);
            }
            // Activate nearby mines in explosion radius
            if (collider.gameObject.CompareTag("Damageable Proximity Mine"))
            {
                ExplodingMine nearbyMine = collider.GetComponent<ExplodingMine>();
                if (nearbyMine != null && !nearbyMine.isTriggered)
                {
                    nearbyMine.ActivateMine();
                }
            }
        }

        // Destroy the mine after the explosion
        Destroy(gameObject);
    }
}