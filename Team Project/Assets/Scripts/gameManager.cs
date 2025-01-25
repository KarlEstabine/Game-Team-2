using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InfimaGames.LowPolyShooterPack;

public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("Meunes And UI")]
    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause, menuWin, menuLose;
    [SerializeField] GameObject tutorialUI, weaponUI;
    [SerializeField] TMP_Text goalText;
    public GameObject damagePanel;
    public Image playerHPBar;

    [Header("Player Info")]
    public GameObject player;
    public Character playerScript;

    bool isPaused;

    int goalCount;

    // Changed from start to awake to start before everything else
    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Character>();
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
        tutorialUI.SetActive(false);
        weaponUI.SetActive(false);
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

        tutorialUI.SetActive(true);
        weaponUI.SetActive(true);
    }

    public void UpdatedGameGoal(int amount)
    {
        //update goal count/ how many enemies are dead
        goalCount += amount;

        // Goal Text
        goalText.text = goalCount.ToString("F0");

        // win condition
        if (goalCount <= 0)
        {
            StatePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
            playerScript.cursorLocked = false;
        }
    }

    public void YouLose()
    {
        //pause game and display lose menu if player dies
        StatePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
        playerScript.cursorLocked = false;
    }
}
