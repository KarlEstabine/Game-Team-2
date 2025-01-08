using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] int jumpMax;
    [SerializeField] int jumpSpeed;
    [SerializeField] int gravity;
    int jumpCount;

    [SerializeField] float moveSpeed;
    [SerializeField] float speedMult;

    bool isSprinting;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        move();
        sprint();
    }

    void move()
    {

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
                  (Input.GetAxis("Vertical") * transform.forward);
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        jump();

        controller.Move(playerVel * Time.deltaTime);
        playerVel.y -= gravity * Time.deltaTime;
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
}
