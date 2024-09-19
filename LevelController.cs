using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;

    [SerializeField] private bool tutorial = false;

    SpeedUpDebug speedUpDebug;
    private PlayerMovement playerMovement;
    private GameManager gameManager;
    private DestroyOnHit[] destroyOnHit;
    private PowerUpTimer powerUpTimer;
    [SerializeField] private BarController barController;

    private int destructablesLeftAmount;
    private GameObject player;
    private int scoreAmount;
    private HighScoreKeeper highScoreKeeper;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI lvlIndicatorText;
    private TextMeshProUGUI gameOverTextTMP;
    private GameObject gameOverText;
    public bool gameOver;

    [SerializeField] public int level = 1;
    [SerializeField] private int levelEnemyAmount = 10;
    private bool switchingLevel = false;

    [SerializeField] private GameObject bossPrefab;
    private Vector3 bossSpawnPoint = new Vector3(27, 1, 0);
    public bool bossLevel = false;

    [SerializeField] private GameObject levelBar;
    [SerializeField] private GameObject bossBar;
    [SerializeField] private GameObject bossShieldBar;

    private GameObject intromusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            if (!tutorial)
            {
                DontDestroyOnLoad(gameObject);
            }
            highScoreKeeper = FindObjectOfType<HighScoreKeeper>();
            intromusic = GameObject.Find("IntroMusic");
            Destroy(intromusic);
            speedUpDebug = FindObjectOfType<SpeedUpDebug>();
            playerMovement = FindObjectOfType<PlayerMovement>();
            powerUpTimer = FindObjectOfType<PowerUpTimer>();
            gameManager = FindObjectOfType<GameManager>();
            scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
            player = GameObject.FindGameObjectWithTag("Player");
            gameOverText = GameObject.FindGameObjectWithTag("GameOverText");
            gameOverTextTMP = GameObject.Find("GameOverText").GetComponent<TextMeshProUGUI>();
            lvlIndicatorText = GameObject.Find("Level Indicator").GetComponent<TextMeshProUGUI>();

            levelBar = GameObject.Find("LevelBar");
            levelBar.SetActive(true);

            bossBar = GameObject.Find("BossBar");
            bossShieldBar = GameObject.Find("BossShieldBar");
            bossBar.SetActive(false);
            bossShieldBar.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        gameOverText.SetActive(false);
        lvlIndicatorText.text = $"{level}";
    }

    private void Update()
    {
        if (gameOver)
        {
            gameOverTextTMP.text = $"GAME OVER! \n Press *R* to restart! \n Score: {scoreAmount} \n High Score: {highScoreKeeper.highScore}";
            gameOverText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.R) && !speedUpDebug.paused)
            {
                ResetGame();
                powerUpTimer.UseShield();
                powerUpTimer.ResetMachinegunTimer();
                powerUpTimer.ResetBigBulletTimer();
                player.SetActive(true);
                player.transform.position = new Vector3(2, 4, transform.position.z);
                bossBar.SetActive(false);
                bossShieldBar.SetActive(false);
                bossLevel = false;
                levelBar.SetActive(true);
                gameOverText.SetActive(false);
                gameOver = false;
            }
        }

        if (!speedUpDebug.paused && Input.GetKey(KeyCode.O) && Input.GetKey(KeyCode.P))
        {
            Destroy(gameObject);
            SceneManager.LoadScene(0);
        }

        if (speedUpDebug.paused && Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.L))
        {
            speedUpDebug.UnPauseGame();
        }

        if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.U))
        {
            SceneManager.LoadScene(0);
            Destroy(gameObject);
        }

        if (Input.GetKey(KeyCode.RightBracket))
        {
            Application.Quit();
        }
    }

    public void UpdateScore(int addAmount)
    {
        scoreAmount += addAmount;
        if (scoreAmount <= 9)
        {
            scoreText.text = $"000{scoreAmount}";
        }
        else if (scoreAmount <= 99)
        {
            scoreText.text = $"00{scoreAmount}";
        }
        else if (scoreAmount <= 999)
        {
            scoreText.text = $"0{scoreAmount}";
        }
        else if (scoreAmount <= 9999)
        {
            scoreText.text = scoreAmount.ToString();

        }
    }

    public void AddDestructable()
    {
        destructablesLeftAmount++;
    }
    public void RemoveDestructable()
    {
        if (!bossLevel)
        {
            if (levelEnemyAmount > 0)
            {
                levelEnemyAmount--;
                barController.AddValue(1);
            }

            if (levelEnemyAmount <= 0)
            {
                if (!switchingLevel)
                {
                    Invoke("StartNextLevel", 1);
                }
                switchingLevel = true;
            }
        }
        else if (bossLevel)
        {
            if (!switchingLevel)
            {
                bossBar.SetActive(false);
                bossShieldBar.SetActive(false);
                levelBar.SetActive(true);
                Invoke("StartNextLevel", 1);
            }
            switchingLevel = true;
        }
    }

    private void StartNextLevel()
    {
        if (level == 5 || level == 10 || level == 15 || level == 20 || level == 25 || level == 30 || level == 35 || level == 40 || level == 45 || level == 50 || level == 55 || level == 60 || level == 65 || level == 70 || level == 75 || level == 80 || level == 85 || level == 90 || level == 95 || level == 100)
        {
            playerMovement.RegenFullHealth();
        }
        else
        {
            playerMovement.RegenOneHealth();
        }
        level++;
        levelEnemyAmount = level * 10;
        gameManager.NewLevel();
        SpawnBoss();
        barController.SetMaxValue(levelEnemyAmount);
        barController.SetValue(0);
        lvlIndicatorText.text = $"{level}";
        switchingLevel = false;
    }

    public void ResetGame()
    {
        switchingLevel = true;
        destroyOnHit = FindObjectsOfType<DestroyOnHit>();
        foreach (DestroyOnHit forceDestroy in destroyOnHit)
        {
            forceDestroy.ForceDestroy();
        }
        level = 1;
        levelEnemyAmount = level * 10;
        gameManager.ResetSpawnRate();
        barController.SetMaxValue(levelEnemyAmount);
        barController.SetValue(0);
        playerMovement.RegenHealth();
        scoreAmount = 0;
        lvlIndicatorText.text = $"{level}";
        scoreText.text = "0000";
        switchingLevel = false;
    }

    public void RestartGame()
    {
        if (scoreAmount > highScoreKeeper.highScore)
        {
            highScoreKeeper.highScore = scoreAmount;
        }
        gameOver = true;
        gameOverText.SetActive(true);
    }

    private void SpawnBoss()
    {
        if (level == 5 || level == 10 || level == 15 || level == 20 || level == 25 || level == 30 || level == 35 || level == 40 || level == 45 || level == 50 || level == 55 || level == 60 || level == 65 || level == 70 || level == 75 || level == 80 || level == 85 || level == 90 || level == 95 || level == 100)
        {
            bossLevel = true;
            bossBar.SetActive(true);
            bossShieldBar.SetActive(true);
            bossSpawnPoint.y = Random.Range(-2f, 12f);
            Instantiate(bossPrefab, bossSpawnPoint, Quaternion.identity);
        }
        if (level == 100)
        {
            Application.Quit();
        }
    }
}
