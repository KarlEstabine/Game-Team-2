using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;

public class playerController : MonoBehaviour, IDamage
{

    [Header("--- Components ---")]
    [Space]

    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] LayerMask groundMask;          // Layers considered "ground"

    [Space]
    [Header("--- Player Settings ---")]
    [Space]

    [SerializeField] int HP;
    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    [SerializeField] float groundCheckDistance;     // Distance to check for ground
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;
    [SerializeField] float crouchHeight;
    [SerializeField] float moveSpeed;
    [SerializeField] float speedMult;
    [SerializeField] int leanAngle;
    [SerializeField] int leanSpeed;

    [Space]
    [Header("--- Shooting Settings ---")]
    [Space]

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] private float fireRate;
    [SerializeField] GameObject gunModel;



    int HPOrig;
    int jumpCount;

    float currentLean;
    float originalCameraHeight;
    float originalmoveSpeed;
    float originalControllerHeight;
    float dashTimeLeft;
    float dashCooldownTimer;

    bool isSprinting;
    bool isLeaningRight;
    bool isLeaningLeft;
    bool isDashing = false;
    bool isCrouching = false;
    private bool isShooting;


    Vector3 moveDir;
    Vector3 playerVel;

    Vector3 dashDir;

    Animator anim;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        HPOrig = HP;
        UpdatePlayerUI();

        originalCameraHeight = Camera.main.transform.localPosition.y;
        originalmoveSpeed = moveSpeed;
        originalControllerHeight = controller.height;
        isShooting = false;
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
        handleCrouch();
    }

    void handleCrouch()
    {
        if (Input.GetButtonDown("Crouch") && !isSprinting)
        {
            if (!isCrouching)
            {
                crouch();
            }
            else
            {
                stand();
            }
        }
    }

    void crouch()
    {
        isCrouching = true;
        Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, Camera.main.transform.localPosition.y / 2, Camera.main.transform.localPosition.z);
        moveSpeed = moveSpeed / 2;
        controller.height = controller.height * crouchHeight;
    }

    void stand()
    {
        isCrouching = false;
        Camera.main.transform.localPosition = new Vector3(Camera.main.transform.localPosition.x, originalCameraHeight, Camera.main.transform.localPosition.z);
        moveSpeed = originalmoveSpeed;
        controller.height = originalControllerHeight;
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
        if (Input.GetButtonDown("Shoot") && !isShooting)
        {
            StartCoroutine(shoot());
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            stand();
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
            stand();
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    private IEnumerator shoot()
    {
        isShooting = true;
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
        yield return new WaitForSeconds(fireRate);
        isShooting = false;

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

        // Get the player's input direction
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 cameraForward = Camera.main.transform.forward;
        // Combine input direction with camera's forward and right directions
        Vector3 dashDirection = (cameraForward * verticalInput + Camera.main.transform.right * horizontalInput).normalized;

        dashDir = dashDirection;

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
    
    public void getGunStats(GunStats weapon)
    {
        shootDamage = weapon.shootDamage;
        shootDist = weapon.shootDist;
        fireRate = weapon.shootDist;
        anim = weapon.animator;
        gameManager.instance.weaponSprite = weapon.spriteBody;

        gunModel.GetComponent<MeshFilter>().sharedMesh = weapon.weaponModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = weapon.weaponModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

}
