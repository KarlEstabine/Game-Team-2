using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    // UI Elements starting with menuPause and menuActive
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;

    public GameObject player;
    public playerController playerScript;

    public bool isPaused;

    // Changed from start to awake to start before everything else
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                StatePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
            }
        }
    }

    public void StatePause()
    {
        isPaused = !isPaused;
        // Freeze time when paused
        Time.timeScale = 0;
        // Show the cursor when paused
        Cursor.visible = true;
        // Confines the cursor to the game window
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void StateUnpause()
    {
        isPaused = !isPaused;
        // Unfreeze time when unpaused
        Time.timeScale = 1;
        // Hide the cursor when unpaused
        Cursor.visible = false;
        // Locks the cursor to the center of the game window
        Cursor.lockState = CursorLockMode.Locked;
  
        menuActive.SetActive(false);
        menuActive = null;
    }
}
