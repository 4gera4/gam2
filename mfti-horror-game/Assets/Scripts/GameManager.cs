using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    public bool isGameActive = false;
    public bool isPaused = false;
    
    [Header("Game Settings")]
    public float gameTime = 0f;
    public int requiredIntegrals = 5;
    
    [Header("Difficulty")]
    public GameDifficulty currentDifficulty = GameDifficulty.Normal;
    
    public enum GameDifficulty
    {
        Easy,    // Меньше преподавателей, больше времени
        Normal,  // Стандартно
        Hard,    // Больше преподавателей, меньше времени
        Phystech // Хардкор - как настоящая сессия в МФТИ
    }
    
    [Header("References")]
    public GameObject player;
    public Transform[] teacherSpawnPoints;
    public GameObject[] teacherPrefabs;
    public GameObject[] integralPrefabs;
    public Transform[] integralSpawnPoints;
    
    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioClip ambientMusic;
    public AudioClip chaseMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;
    
    private int spawnedTeachers = 0;
    private int spawnedIntegrals = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        StartGame();
    }
    
    void Update()
    {
        if (isGameActive && !isPaused)
        {
            gameTime += Time.deltaTime;
        }
        
        // Пауза на Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    
    public void StartGame()
    {
        isGameActive = true;
        isPaused = false;
        gameTime = 0f;
        
        // Спавним преподавателей
        SpawnTeachers();
        
        // Спавним интегралы
        SpawnIntegrals();
        
        // Запускаем фоновую музыку
        if (backgroundMusic != null && ambientMusic != null)
        {
            backgroundMusic.clip = ambientMusic;
            backgroundMusic.loop = true;
            backgroundMusic.Play();
        }
        
        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Игра началась! Соберите все интегралы!");
    }
    
    void SpawnTeachers()
    {
        int teacherCount = GetTeacherCountForDifficulty();
        
        for (int i = 0; i < teacherCount && i < teacherSpawnPoints.Length; i++)
        {
            if (teacherPrefabs.Length > 0)
            {
                int randomTeacher = Random.Range(0, teacherPrefabs.Length);
                GameObject teacher = Instantiate(teacherPrefabs[randomTeacher], 
                    teacherSpawnPoints[i].position, teacherSpawnPoints[i].rotation);
                spawnedTeachers++;
            }
        }
        
        Debug.Log($"Спавн {spawnedTeachers} преподавателей");
    }
    
    void SpawnIntegrals()
    {
        for (int i = 0; i < requiredIntegrals && i < integralSpawnPoints.Length; i++)
        {
            if (integralPrefabs.Length > 0)
            {
                int randomIntegral = Random.Range(0, integralPrefabs.Length);
                GameObject integral = Instantiate(integralPrefabs[randomIntegral],
                    integralSpawnPoints[i].position, Quaternion.identity);
                spawnedIntegrals++;
            }
        }
        
        Debug.Log($"Спавн {spawnedIntegrals} интегралов");
    }
    
    int GetTeacherCountForDifficulty()
    {
        switch (currentDifficulty)
        {
            case GameDifficulty.Easy:
                return 2;
            case GameDifficulty.Normal:
                return 3;
            case GameDifficulty.Hard:
                return 5;
            case GameDifficulty.Phystech:
                return 7; // Все преподаватели на тебя!
            default:
                return 3;
        }
    }
    
    public void WinGame()
    {
        isGameActive = false;
        
        // Проигрываем победную музыку
        if (backgroundMusic != null && winMusic != null)
        {
            backgroundMusic.clip = winMusic;
            backgroundMusic.loop = false;
            backgroundMusic.Play();
        }
        
        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Показываем экран победы
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowWinScreen(gameTime);
        }
        
        Debug.Log($"ПОБЕДА! Время: {FormatTime(gameTime)}");
    }
    
    public void GameOver()
    {
        isGameActive = false;
        
        // Проигрываем музыку поражения
        if (backgroundMusic != null && loseMusic != null)
        {
            backgroundMusic.clip = loseMusic;
            backgroundMusic.loop = false;
            backgroundMusic.Play();
        }
        
        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Показываем экран поражения
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverScreen();
        }
        
        Debug.Log("ПОРАЖЕНИЕ! Вас поймали!");
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPauseMenu(isPaused);
        }
        
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (isGameActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void SetDifficulty(GameDifficulty difficulty)
    {
        currentDifficulty = difficulty;
    }
    
    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    public void PlayChaseMusic()
    {
        if (backgroundMusic != null && chaseMusic != null && backgroundMusic.clip != chaseMusic)
        {
            backgroundMusic.clip = chaseMusic;
            backgroundMusic.Play();
        }
    }
    
    public void PlayAmbientMusic()
    {
        if (backgroundMusic != null && ambientMusic != null && backgroundMusic.clip != ambientMusic)
        {
            backgroundMusic.clip = ambientMusic;
            backgroundMusic.Play();
        }
    }
}
