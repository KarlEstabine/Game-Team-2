using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;

    public bool isShooting;

    public bool IsShooting => isShooting;

    public void performAttack()
    {
        if (!isShooting)
        {
            StartCoroutine(shoot());
        }
    }

    private IEnumerator shoot()
    {
        isShooting = true;
        //yield return new WaitForSeconds(0.1f);
        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
}
