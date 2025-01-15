using UnityEngine;

public class damage : MonoBehaviour
{
    enum DamageType
    {
        moving,
        stationary
    }

    [SerializeField] DamageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (type == DamageType.moving)
        {
            rb.linearVelocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    private void FixedUpdate()
    {
        if (type == DamageType.moving)
        {
            RaycastHit hit;
            float travelDistance = speed * Time.fixedDeltaTime;

            // Cast a ray in the direction the bullet is moving
            if (Physics.Raycast(transform.position, transform.forward, out hit, travelDistance))
            {
                OnBulletHit(hit.collider);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnBulletHit(other);
    }

    private void OnBulletHit(Collider other)
    {
        Debug.Log($"Collided with: {other.name}");
        if (other.isTrigger || other.CompareTag("Enemy"))
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null)
        {
            dmg.takeDamage(damageAmount);

            Debug.Log($"Dealt {damageAmount} to: {other.name}");
        }

        if (type == DamageType.moving)
        {
            Destroy(gameObject);
        }
    }
}
