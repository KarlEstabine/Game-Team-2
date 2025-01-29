using NUnit.Framework.Interfaces;
using UnityEngine;

public class pickup : MonoBehaviour
{
    [SerializeField] GunStats gun;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        IPickup pick = other.GetComponent<IPickup>();

        if (pick != null)
        {
            // Transfer gun to the object that entered the trigger
            pick.getGunStats(gun);
            Destroy(gameObject);
        }
    }
}