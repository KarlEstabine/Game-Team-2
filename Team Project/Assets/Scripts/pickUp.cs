using UnityEngine;

public class pickup : MonoBehaviour
{
    enum pickupType { gun, HP, armor, ammo }

    [SerializeField] pickupType type;
    [SerializeField] GunStats gun;

    void Start()
    {
        if (type == pickupType.gun)
        {
            gun.ammoCur = gun.ammoMax;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == pickupType.gun)
            {
                gameManager.instance.playerScript.getGunStats(gun);
            }
            Destroy(gameObject);
        }
    }
}
