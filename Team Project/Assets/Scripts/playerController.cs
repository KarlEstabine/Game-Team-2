using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections;

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

    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;

    int HPOrig;
    int jumpCount;

    float currentLean;

    [SerializeField] float moveSpeed;
    [SerializeField] float speedMult;

    bool isSprinting;

    bool isLeaningRight;
    bool isLeaningLeft;

    bool isDashing = false;

    float dashTimeLeft;
    float dashCooldownTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    Vector3 dashDir;

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
        dashInput();

        updateAnimator();
        UpdateDashCooldown();
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

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        Debug.Log(controller.isGrounded);

        // Get movement input and normalize direction
        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward).normalized;

        // Apply movement
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // Handle jumping
        jump();


        // Apply vertical velocity (gravity or jump)
        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;

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
        // update health bar to go down if dmg taken
        HP -= amount;
        UpdatePlayerUI();

        // Flash damage panel
        StartCoroutine(FlashDamagePanel());

        // flash dmg panel because we took dmg
        if (HP <= 0)
        {
            //call you lose function in game manager
            gameManager.instance.YouLose();
        }
    }

    IEnumerator FlashDamagePanel()
    {
        // activate damage panel
        gameManager.instance.damagePanel.SetActive(true);

        // wait for 0.1 seconds to flash panel
        yield return new WaitForSeconds(0.1f);

        // turn dmg panel back off
        gameManager.instance.damagePanel.SetActive(false);
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
    void dashInput()
    {
        // Check if the dash button is pressed and all conditions are met
        if (Input.GetButtonDown("Dash") && dashCooldownTimer <= 0 && !isDashing)
        {
            Debug.Log("Dash initiated");
            StartDash();
        }

        // Handle the actual dashing movement
        if (isDashing)
        {
            PerformDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownTimer = dashCooldown;

        Vector3 cameraForward =Camera.main.transform.forward;
   
        dashDir = cameraForward.normalized;


        Debug.Log($"Dash direction: {dashDir}");


    }
    void PerformDash()
    {
        if (dashTimeLeft >0)
            {
            controller.Move(dashDir * dashSpeed * Time.deltaTime);
            dashTimeLeft -= Time.deltaTime;
            

            }
        else
        {
            isDashing = false;
        }
     }
    void UpdateDashCooldown()
    {
        if(dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    public void getGunStats(GunStats gun)
    {

    }

}
