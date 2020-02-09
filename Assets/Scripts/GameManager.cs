using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;
    public float turnDelay = .1f;
    public static GameManager instance = null;                //Static instance of GameManager which allows it to be accessed by any other script.
    private BoardManager boardScript;                        //Store a reference to our BoardManager which will set up the level.
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    public static int highscore;

    private Text levelText;
    private Text highScoreText;
    private Button retryButton;
    private GameObject levelImage;
    public int level = 1;                                    //Current level number, expressed in game as "Day 1".
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetUp;
    private bool wasRestarted = false;

    //Awake is always called before any Start functions
    void Awake()
    {
        //Check if instance already exists
        if (instance == null)
            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);    

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        enemies = new List<Enemy>();

        //Get a component reference to the attached BoardManager script
        boardScript = GetComponent<BoardManager>();

        //Call the InitGame function to initialize the first level 
        InitGame();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (!SoundManager.instance.musicSource.isPlaying)
            SoundManager.instance.musicSource.Play();

        wasRestarted = true;
    }

    private void OnLevelWasLoaded(int index)
    {
        enabled = true;

        if (wasRestarted)
        {
            level = 0;
            playerFoodPoints = 100;
            playersTurn = true;

            wasRestarted = false;
        }

        level++;

        InitGame();
    }

    //void OnEnable()
    //{
    //    //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    //void OnDisable()
    //{
    //    //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    level++;

    //    InitGame();
    //}

    //Initializes the game for each level.
    void InitGame()
    {
        doingSetUp = true;

        highscore = (int) PlayerPrefs.GetFloat("High Score", 0);

        if (!levelImage)
        {
            levelImage = GameObject.Find("LevelImage");
        }

        if (!levelText)
        {
            levelText = GameObject.Find("LevelText").GetComponent<Text>();
        }
        levelText.text = "Day " + level;

        if (!highScoreText)
        {
            highScoreText = GameObject.Find("HighScoreText").GetComponent<Text>();
        }
        highScoreText.text = "";

        if (!retryButton)
        {
            retryButton = GameObject.Find("RetryButton").GetComponent<Button>();
            retryButton.onClick.AddListener(() => RestartGame());
        }
        
        retryButton.gameObject.SetActive(false);

        levelImage.SetActive(true);

        Invoke("HideLevelImage", levelStartDelay);

        enemies.Clear();

        //Call the SetupScene function of the BoardManager script, pass it current level number.
        boardScript.SetupScene(level);
    }

    private void HideLevelImage() 
    {
        levelImage.SetActive(false);
        doingSetUp = false;
    }

    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved.";

        if (level > highscore)
        {
            highscore = level;
            //You need this to save high score across game sessions
            PlayerPrefs.SetFloat("High Score", level);

            highScoreText.text = "New high score\n" + highscore + " days";
        }
        else
        {
            highScoreText.text = "High score\n" + highscore + " days";
        }

        retryButton.gameObject.SetActive(true);

        levelImage.SetActive(true);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
        if (playersTurn || enemiesMoving || doingSetUp)
            //If any of these are true, return and do not start MoveEnemies.
            return;

        //Start moving enemies.
        StartCoroutine (MoveEnemies ());
    }

    //Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy script)
    {
        //Add Enemy to List enemies.
        enemies.Add(script);
    }

    //Coroutine to move enemies in sequence.
    IEnumerator MoveEnemies()
    {
        //While enemiesMoving is true player is unable to move.
        enemiesMoving = true;

        //Wait for turnDelay seconds, defaults to .1 (100 ms).
        yield return new WaitForSeconds(turnDelay);

        //If there are no enemies spawned (IE in first level):
        if (enemies.Count == 0) 
        {
            //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
            yield return new WaitForSeconds(turnDelay);
        }

        //Loop through List of Enemy objects.
        for (int i = 0; i < enemies.Count; i++)
        {
            //Call the MoveEnemy function of Enemy at index i in the enemies List.
            enemies[i].MoveEnemy ();

            //Wait for Enemy's moveTime before moving next Enemy, 
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        //Once Enemies are done moving, set playersTurn to true so player can move.
        playersTurn = true;
        
        //Enemies are done moving, set enemiesMoving to false.
        enemiesMoving = false;
    }
}
