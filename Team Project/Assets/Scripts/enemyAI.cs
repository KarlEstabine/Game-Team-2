using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    Animator anim;
    [SerializeField] Renderer model;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;

    [SerializeField] float shootRate;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] int HP;

    bool isShooting;
    bool playerInRange;

    Color colorOrig;

    Vector3 playerDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        anim = GetComponent<Animator>();

        // Update the goal count
        gameManager.instance.UpdatedGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange)
        {

            playerDir = gameManager.instance.player.transform.position - transform.position;
            agent.SetDestination(gameManager.instance.player.transform.position);

            anim.SetBool("Walking", true);

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                faceTarget();
                anim.SetBool("Walking", false);
            }

            if (!isShooting)
            {
                StartCoroutine(shoot());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        Debug.Log("HP: " + HP + "\nDamage: " + amount);

        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            // Update the goal count
            gameManager.instance.UpdatedGameGoal(-1);
            Destroy(gameObject);
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
}
