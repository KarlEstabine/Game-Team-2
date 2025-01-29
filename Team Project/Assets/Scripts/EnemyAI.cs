using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;

    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Transform shootPos, headPOS;
    [SerializeField] GameObject bullet;

    [SerializeField] float shootRate;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] int sightFOV, shootFOV;
    [SerializeField] int animeSpeedTrans;

    [SerializeField] int HP;

    float angleToPlayer;

    bool isShooting;
    bool playerInRange;

    Color colorOrig;

    Vector3 playerDir;
    Vector3 height = new Vector3(0, 1.2f, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;

        // Update the goal count in spawner now
       // gameManager.instance.UpdatedGameGoal(1);
    }

    // Update is called once per frame
    void Update()
    {
        float agentSpeed = agent.velocity.normalized.magnitude;
        float animSpeed = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.MoveTowards(animSpeed, agentSpeed, Time.deltaTime * animeSpeedTrans));
        if (playerInRange && canSeePlayer())
        {

            
        }
    }

    bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - headPOS.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);
        
        Debug.DrawRay(headPOS.position, playerDir + height);

        RaycastHit hit;
        if (Physics.Raycast(headPOS.position, playerDir + height, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= sightFOV)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    faceTarget();
                }

                if (!isShooting)
                {
                    if (angleToPlayer <= shootFOV)
                    {
                        StartCoroutine(shoot());
                    }
                    
                }
                return true;
            }
        }
        return false;
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
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            // Update the goal count
            GameManager.instance.UpdatedGameGoal(-1);
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
        Quaternion playerRot = Quaternion.LookRotation(playerDir + height);
        isShooting = true;
        Instantiate(bullet, shootPos.position, playerRot );

        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }
}
