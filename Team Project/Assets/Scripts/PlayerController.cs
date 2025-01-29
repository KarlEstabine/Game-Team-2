using UnityEngine;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour, IDamage, IPickup
{
    [Header("--- Components ---")]
    [Space]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreMask;
    [SerializeField] LayerMask groundMask;          // Layers considered "ground"

    [Space]
    [Header("--- Player Settings ---")]
    [Space]

    [Range(1, 10)] [SerializeField] int HP;
    [Range(1, 100)] [SerializeField] float stamina;
    [Range(0, 10)][SerializeField] float staminaRegen;
    [Range(1, 2)] [SerializeField] int jumpMax;
    [Range(0, 5)][SerializeField] int jumpStaminaUse;
    [Range(1, 10)] [SerializeField] int jumpSpeed;
    [Range(15, 45)] [SerializeField] int gravity;
    [Range(15, 45)] [SerializeField] int leanAngle;
    [Range(1, 50)] [SerializeField] int leanSpeed;
    [Range(0, 5)] [SerializeField] float sprintStaminaUse;

    [SerializeField] float groundCheckDistance;
    [Range(2, 20)] [SerializeField] float dashSpeed;
    [Range(0, 10)] [SerializeField] float dashDuration;
    [Range(0, 10)] [SerializeField] float dashCooldown;
    [Range(0, 5)] [SerializeField] float dashStaminaUse;

    [Range(0, 5)] [SerializeField] float crouchHeight;
    [Range(1, 10)] [SerializeField] float moveSpeed;
    [Range(0, 10)] [SerializeField] float speedMult;

    [Header("--- Shock Push Settings ---")]
    [Range(1, 20)] [SerializeField] float shockPushRange;
    [Range(1, 10)] [SerializeField] float shockPushStrength;
    [Range(0, 5)] [SerializeField] float shockPushCooldown;
    private float shockPushCooldownTimer;
    [SerializeField] LayerMask targetLayer;
    [Range(1,10)] [SerializeField] int shockPushDamage;

    [Space]
    [Header("--- Shooting Settings ---")]
    [Space]
    [SerializeField] List<GunStats> gunList = new List<GunStats>();
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] private float fireRate;
    [SerializeField] GameObject gunModel;

    int HPOrig;
    float staminaOrig;
    int jumpCount;
    int gunListPos;

    float currentLean;
    float originalCameraHeight;
    float originalmoveSpeed;
    float originalControllerHeight;
    float dashTimeLeft;
    float dashCooldownTimer;

    bool isSprinting;
    bool isLeaningRight;
    bool isLeaningLeft;
    bool isDashing;
    bool isCrouching;
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
        staminaOrig = stamina;
        UpdatePlayerUI();

        originalCameraHeight = Camera.main.transform.localPosition.y;
        originalmoveSpeed = moveSpeed;
        originalControllerHeight = controller.height;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        HandleStaminaRegeneration();
        move();
        sprint();
        dashInput();
        shockPushInput();
        selectWeapon();

        updateAnimator();
        UpdateDashCooldown();
        handleLean();
        handleCrouch();
    }
    void HandleStaminaRegeneration()
    {
        if (!isSprinting && !isDashing)
        {
            stamina = Mathf.Min(stamina + staminaRegen * Time.deltaTime, staminaOrig);
            UpdatePlayerUI();
        }
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
            playerVel.y = -gravity * Time.deltaTime;
        }

        // Get movement input and normalize direction
        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward).normalized;
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

    public void staminaUse(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0, staminaOrig);
        UpdatePlayerUI();
    }

    void sprint()
    {
        if (Input.GetButton("Sprint") && stamina >= sprintStaminaUse)
        {
            if (!isSprinting)
            {
                stand();
                moveSpeed *= speedMult;
                isSprinting = true;
            }
            staminaUse(sprintStaminaUse * Time.deltaTime);
        }
        else if(isSprinting)
        {
            moveSpeed /= speedMult;
            isSprinting = false;
        }
    }

    void jump()
    {
        if(controller.isGrounded)
        {
            jumpCount = 0;
            playerVel.y = -gravity * Time.deltaTime;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax && stamina >= jumpStaminaUse)
        {
            stand();
            jumpCount++;
            playerVel.y = jumpSpeed;
            staminaUse(jumpStaminaUse);
        }
    }

    private IEnumerator shoot()
    {
        isShooting = true;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreMask))
        {

            IDamage dmg = hit.collider.GetComponent<IDamage>();
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

        if (HP <= 0)
        {
            GameManager.instance.YouLose();
        }
    }

    IEnumerator FlashDamagePanel()
    {
        // activate damage panel
        GameManager.instance.damagePanel.SetActive(true);

        // wait for 0.1 seconds to flash panel
        yield return new WaitForSeconds(0.1f);

        // turn dmg panel back off
        GameManager.instance.damagePanel.SetActive(false);
    }

    void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        GameManager.instance.playerStaminaBar.fillAmount = stamina / staminaOrig;
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
        if (Input.GetButtonDown("Dash") && dashCooldownTimer <= 0 && !isDashing && stamina >= dashStaminaUse)
        {
            StartDash();
        }

        if (isDashing)
        {
            PerformDash();
        }
    }

    void StartDash()
    {
        if (stamina >= dashStaminaUse)
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

            staminaUse(dashStaminaUse);
        }
    }

    void PerformDash()
    {
        if (dashTimeLeft > 0)
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
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    void shockPushInput()
    {
        if (shockPushCooldownTimer > 0)
        {
            shockPushCooldownTimer -= Time.deltaTime;
        }

        // tigger ability when f is pressed if cooldown is over
        if (Input.GetButtonDown("ShockPush") && shockPushCooldownTimer <= 0)
        {
            PerformShockPush();
        }

        void PerformShockPush()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, shockPushRange, targetLayer);
            foreach (Collider col in colliders)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = col.transform.position - transform.position;
                    direction.Normalize();
                    rb.AddForce(direction * shockPushStrength, ForceMode.Impulse);
                    Debug.Log("Pushed" + col.name + "with force of" + shockPushStrength);
                }
                IDamage dmg = col.GetComponent<IDamage>();
                if (dmg != null)
                {
                    dmg.takeDamage(shockPushDamage);
                    Debug.Log("Dealt " + shockPushDamage + " damage to " + col.name);
                }
            }

            ReflectBullets();
            shockPushCooldownTimer = shockPushCooldown;
        }
        
    }
        void ReflectBullets()
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, shockPushRange);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Bullet"))
                {
                    Rigidbody bulletRb = hit.collider.GetComponent<Rigidbody>();
                    if (bulletRb != null)
                    {
                        Vector3 reflectionDirection = Vector3.Reflect(bulletRb.linearVelocity, hit.normal);
                        bulletRb.linearVelocity = reflectionDirection;
                        Debug.Log("Reflected bullet from " + hit.collider.name);
                    }

                }
                
            }
        }
    void OnDrawGizmos()
    {
        // Only draw the shock push range if it is positive
        if (shockPushRange > 0)
        {
            // Set the color for the shock push range visualization
            Gizmos.color = Color.blue;

            // Draw a wireframe sphere to visualize the shock push range
            Gizmos.DrawWireSphere(transform.position, shockPushRange);
        }
    }

    public void getGunStats(GunStats weapon)
    {
        gunList.Add(weapon);
        gunListPos = gunList.Count - 1;

        changeWeapon();
    }

    void selectWeapon()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            changeWeapon();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            changeWeapon();
        }
    }

    void changeWeapon()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        fireRate = gunList[gunListPos].shootRate;

        //anim = weapon.animator;
        //GameManager.instance.weaponSprite = weapon.spriteBody;

        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].weaponModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].weaponModel.GetComponent<MeshRenderer>().sharedMaterial;
    }

}