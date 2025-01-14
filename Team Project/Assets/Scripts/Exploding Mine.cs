using UnityEngine;

public class ExplodingMine : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] float triggerRadius;  // Activation radius of the mine
    [SerializeField] float explosionRadius; // Damage radius of the mine
    [SerializeField] int explosionDamage;

    private bool isActivated = false;  // For preventing multiple activations

    private void OnTriggerEnter(Collider other)
    {
        // When a player or enemy enters the trigger radius, the mine activates
        if (!isActivated && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {
            // When the mine is stepped on, it takes enough damage to explode
            takeDamage(HP);
        }
    }
    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            isActivated = true; // The mine has exploded
            Explode();
        }
    }

    private void Explode()
    {
        // Get all colliders in the explosion radius and apply damage
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            // Check if the collider has the IDamage interface
            IDamage damageable = collider.GetComponent<IDamage>();
            // Apply explosion damage to anything that is damageable and is not a trigger
            if (damageable != null && !collider.isTrigger)
            {
                damageable.takeDamage(explosionDamage);
            }
            // Activate nearby mines in explosion radius
            if (collider.gameObject.CompareTag("Damageable Proximity Mine"))
            {
                ExplodingMine nearbyMine = collider.GetComponent<ExplodingMine>();
                if (nearbyMine != null && !nearbyMine.isActivated)
                {
                    nearbyMine.takeDamage(nearbyMine.HP);
                }
            }
        }

        // Destroy the mine after the explosion
        Destroy(gameObject);
    }
}