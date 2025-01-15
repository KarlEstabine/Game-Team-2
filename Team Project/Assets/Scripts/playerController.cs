using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;

    [SerializeField] float groundCheckDistance;     // Distance to check for ground
    [SerializeField] LayerMask groundMask;          // Layers considered "ground"

    [SerializeField] int HP;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;

    [SerializeField] int leanAngle;
    [SerializeField] int leanSpeed;

    int HPOrig;
    int jumpCount;

    float currentLean;

    [SerializeField] float moveSpeed;
    [SerializeField] float speedMult;

    bool isSprinting;

    bool isLeaningRight;
    bool isLeaningLeft;

    Vector3 moveDir;
    Vector3 playerVel;

    Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
        HPOrig = HP;
        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        move();
        sprint();

        updateAnimator();
        handleLean();
    }

    void handleLean()
    {
        if (Input.GetButtonDown("LeanRight"))
        {
            isLeaningRight = !isLeaningRight;
            isLeaningLeft = false;
        }

        if (Input.GetButtonDown("LeanLeft"))
        {
            isLeaningLeft = !isLeaningLeft;
            isLeaningRight = false;
        }

        int targetLean = 0;

        if (isLeaningRight)
        {
            targetLean = -leanAngle;
        }
        else if (isLeaningLeft)
        {
            targetLean = leanAngle;
        }

        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, currentLean);
    }

    void move()
    {

        /*  if (IsGrounded())
          {
              jumpCount = 0;

              // Reset Y velocity only when grounded
              if (playerVel.y < 0)
              {
                  playerVel.y = -0.1f; // Slight downward force to ensure grounded state
              }
          }
          else
          {
              Debug.Log("Not Grounded");
              // Apply gravity only when not grounded
              playerVel.y -= gravity * Time.deltaTime;
          }*/
        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;

        }
        // Get movement input and normalize direction
        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);

        // Apply movement
        controller.Move(moveDir * moveSpeed * Time.deltaTime);
        
        
        // Handle jumping
        jump();
        

        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;
        // Apply vertical velocity (gravity or jump)
      

        // Shooting logic
        if (Input.GetButtonDown("Shoot"))
        {
            shoot();
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            moveSpeed *= speedMult;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            moveSpeed /= speedMult;
            isSprinting = false;
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;

        }
    }

    void shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreMask))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            // Manually adding mine since having a collider that is not a trigger on the mine prevents it from working properly
            if (dmg != null && hit.collider.CompareTag("Damageable Proximity Mine"))
            {
                dmg.takeDamage(shootDamage);
            }
            if (dmg != null && !hit.collider.isTrigger)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        UpdatePlayerUI();
        //update health bar to go down if dmg taken
        StartCoroutine(FlashDamagePanel());
        //flash dmg panel because we took dmg
        if (HP <= 0)
        {
            gameManager.instance.YouLose();
            //call you lose function in game manager
        }
    }

    IEnumerator FlashDamagePanel()
    {
        gameManager.instance.damagePanel.SetActive(true);
        //activate damage panel
        yield return new WaitForSeconds(0.1f);
        //wait for 0.1 seconds to flash panel
        gameManager.instance.damagePanel.SetActive(false);
        //turn dmg panel back off
    }

    void UpdatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
    }

    void updateAnimator()
    {
        Vector3 localMoveDir = transform.InverseTransformDirection(moveDir);

        if (moveDir != Vector3.zero)
        {
            anim.SetBool("Walking", true);

            // Update MoveX and MoveY based on local direction

            anim.SetFloat("MoveX", localMoveDir.x);
            anim.SetFloat("MoveY", localMoveDir.z);
        }
        else
        {
            anim.SetBool("Walking", false);
        }

        // Update running animation
        anim.SetBool("Running", isSprinting);

        // Update jumping animation
        anim.SetBool("Jumping", playerVel.y > 0);

        // Update falling animation
        anim.SetBool("Falling", playerVel.y < -0.2);

        // Update landing animation
        anim.SetBool("Landed", IsGrounded());
    }

    bool IsGrounded()
    {
        // Cast a ray down from the character's position
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }
}
