using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyAI : MonoBehaviour, IDamage
{
    // References and Components
    [Header("References")]
    Animator anim;
    [SerializeField] Renderer model;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform shootPos;
    [SerializeField] GameObject bullet;

    // UI Elements
    [Header("UI Elements")]
    [SerializeField] GameObject HealthUI;
    [SerializeField] Image HpFill;

    // Enemy Stats
    [Header("Enemy Stats")]
    [SerializeField] float shootRate;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int HP;
    int HPOrig;

    // State Variables
    [Header("State Variables")]
    bool isShooting;
    bool playerInRange;

    // Visual Feedback
    [Header("Visual Feedback")]
    Color colorOrig;

    // Other Variables
    [Header("Other Variables")]
    Vector3 playerDir;

    // Start is called before the first frame update
    void Start()
    {
        HPOrig = HP;
        colorOrig = model.material.color;
        anim = GetComponent<Animator>();
        gameManager.instance.UpdatedGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        HpFill.fillAmount = (float)HP / HPOrig;

        if (playerInRange)
        {
            HealthUI.SetActive(true);
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
        else
        {
            HealthUI.SetActive(false);
            anim.SetBool("Walking", false);
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
        Debug.Log($"Enemy took damage: {amount}");
        HP -= amount;

        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            Destroy(gameObject);
            gameManager.instance.UpdatedGameGoal(-1);
        }
    }

    private IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    private IEnumerator shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
}
