using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class GameManager : MonoBehaviour
{
    // ==================== Singleton Setup ====================
    public static GameManager instance;

    // ==================== Menus ====================
    [Header("Menus")]
    [SerializeField] GameObject menuActive;     // Tracks the current active menu
    [SerializeField] GameObject menuPause;      // Pause Menu
    [SerializeField] GameObject menuWin;        // Win Menu
    [SerializeField] GameObject menuLose;       // Lose Menu
    [SerializeField] GameObject menuNextLevel;  // Next Level Menu

    // ==================== UI Elements ====================
    [Header("UI Elements")]
    [SerializeField] TMP_Text goalText;         // Displays the goal count
    public GameObject player;                   // Player GameObject
    public PlayerController playerScript;       // Reference to player's controller script
    public GameObject damagePanel;              // UI feedback when the player takes damage
    public Image playerHPBar;                   // player health bar
    public Image playerStaminaBar;              // player stamina bar
    public Sprite weaponSprite;                 // sprite representing the player's weapon
    public GameObject butttonInteract;
    public TMP_Text buttonInfo;

    // ==================== Game State ====================
    [Header("Game State")]
    public bool isPaused;                       // Tracks whether the game is paused
    int goalCount;                              // Track the remaining goal

    // ==================== Level Management ====================
    [Header("Level Management")]
    [SerializeField] public string[] levels;    // Array of level names
    private int currentLevelIndex;              // Index of the current level

    // Changed from start to awake to start before everything else
    void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);      // Persist across scenes
            LoadScenesFromBuildSettings();      // Dynamically load scenes from build settings
        }
        else
        {
            Destroy(gameObject);                // Destroy duplicate instances
        }
    }

    void Start()
    {
        // Find and assign the Player and its scripts (for gameplay scenes only)
        player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerScript = player.GetComponent<PlayerController>();
        }

        // also, set currentLevelIndex based on the currently loaded scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        currentLevelIndex = System.Array.IndexOf(levels, currentSceneName);
        if(currentLevelIndex == -1)
        {
            Debug.LogError("Current scene is not in levels array: " + currentSceneName);
            currentLevelIndex = 0;
        } else
        {
            Debug.Log("Current level index: " + currentLevelIndex);
        }
    }

    // ==================== Update (Handles Pause) ====================
    void Update()
    {
        // Toggle Pause Menu with the "Cancel" button (default: "ESC")
        if (Input.GetButtonDown("Cancel"))
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

    // ==================== Pause/Unpause Methods ====================
    public void StatePause()
    {
        isPaused = !isPaused;                           // Freeze time when paused
        Time.timeScale = 0;                             // Show the cursor when paused
        Cursor.visible = true;                          // Show cursor
        Cursor.lockState = CursorLockMode.Confined;     // Confines the cursor to the game window
    }

    public void StateUnpause()
    {

        isPaused = !isPaused;                           // Unfreeze time when unpaused
        Time.timeScale = 1;                             // Hide the cursor when unpaused
        Cursor.visible = false;                         // Locks the cursor to the center of the game window
        Cursor.lockState = CursorLockMode.Locked;

        if (menuActive != null)
        {
            menuActive.SetActive(false);                // Hide the active menu
            menuActive = null;
        }
    }

    // ==================== Goal Management ====================
    public void UpdatedGameGoal(int amount)
    {
        goalCount += amount;                                // Update goal count/ how many enemies are dead
        goalText.text = goalCount.ToString("F0");           // Update goal text display

        // Check if the player has completed all goals
        if (goalCount <= 0)
        {
            StatePause(); // Pause the game

            if (currentLevelIndex + 1 < levels.Length)
            {
                if (menuNextLevel != null)
                {
                    Debug.Log("Displaying Next Level Menu.");
                    menuActive = menuNextLevel; // Set Next Level Menu as the active menu
                    menuActive.SetActive(true); // Activate the Next Level Menu
                }
                else
                {
                    Debug.LogError("Next Level Menu is not assigned in the GameManager.");
                }
            }
            else
            {
                if (menuWin != null)
                {
                    Debug.Log("Displaying Win Menu.");
                    menuActive = menuWin;       // Set the Win Menu as the active menu
                    menuActive.SetActive(true); // Activate the Win Menu
                }
                else
                {
                    Debug.LogError("Win Menu is not assigned in the GameManager.");
                }
            }
        }
    }

    public void YouLose()
    {
        StatePause();                                       // Pause game and display lose menu if player dies
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    // ==================== Level Management Methods ====================
    private void LoadScenesFromBuildSettings()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;  // Get Total Scenes in build Settings
        levels = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            // Extract scene name from path
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            levels[i] = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            currentLevelIndex = levelIndex;
            goalCount = 0;                                      // Reset the goal count for the new level
            SceneManager.LoadScene(levels[levelIndex]);         // load the scene by name
            StateUnpause();                                     // unpause the game
        }
        else
        {
            Debug.LogError("Invalid level index: " + levelIndex);
        }
    }

    public void LoadNextLevel()
    {
        // Only proceed if there are more levels in the array
        if (currentLevelIndex + 1 < levels.Length)
        {
            currentLevelIndex++; // Increment to the next level

            // Skip invalid levels, like "MainMenu"
            while (currentLevelIndex < levels.Length && levels[currentLevelIndex] == "MainMenu")
            {
                Debug.Log($"Skipping Main Menu at index {currentLevelIndex}.");
                currentLevelIndex++; // Increment to skip Main Menu
            }

            // Ensure there are still valid levels to load
            if (currentLevelIndex < levels.Length)
            {
                Debug.Log($"Loading level: {levels[currentLevelIndex]} (Index: {currentLevelIndex}).");
                LoadLevel(currentLevelIndex); // Load the next valid level
            }
            else
            {
                // No more valid levels - Show the Win Menu
                Debug.Log("No valid levels remaining. Showing win menu.");
                ShowWinMenu();
            }
        }
        else
        {
            // Already at the last level - Show the Win Menu
            Debug.Log("Already at the last level. Showing win menu.");
            ShowWinMenu();
        }
    }

    // Handle the logic for displaying the win menu
    private void ShowWinMenu()
    {
        StatePause(); // Pause the game
        if (menuWin != null)
        {
            menuActive = menuWin; // Show the Win Menu
            menuActive.SetActive(true);
        }
        else
        {
            Debug.LogError("Win Menu is not assigned in GameManager.");
        }
    }

    public void LoadMainMenu()
    {
        // Reset game state
        Time.timeScale = 1; // Ensure time is running
        isPaused = false;   // Reset paused state
        menuActive = null;  // Clear any active menus

        // Reset cursor for Main Menu
        Cursor.visible = true; // Show the cursor
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor

        // Load the Main Menu scene
        SceneManager.LoadScene("MainMenu");

        Debug.Log("Loaded Main Menu, time scale reset, and cursor restored.");
    }
}
