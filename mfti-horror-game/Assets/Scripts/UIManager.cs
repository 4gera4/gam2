using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("HUD")]
    public GameObject hudPanel;
    public TextMeshProUGUI integralsText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI staminaText;
    public Slider staminaSlider;
    public Image staminaFillImage;
    
    [Header("Integral Collection")]
    public GameObject integralNotificationPanel;
    public TextMeshProUGUI integralFormulaText;
    public TextMeshProUGUI integralDifficultyText;
    public float notificationDuration = 3f;
    
    [Header("Win Screen")]
    public GameObject winPanel;
    public TextMeshProUGUI winTimeText;
    public TextMeshProUGUI winMessageText;
    
    [Header("Game Over Screen")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverMessageText;
    public Image caughtByImage;
    
    [Header("Pause Menu")]
    public GameObject pausePanel;
    
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button phystechButton;
    
    [Header("Teacher Detection")]
    public GameObject detectionWarningPanel;
    public Image detectionIndicator;
    public float warningPulseSpeed = 2f;
    
    [Header("Colors")]
    public Color staminaHighColor = Color.green;
    public Color staminaMediumColor = Color.yellow;
    public Color staminaLowColor = Color.red;
    
    private float notificationTimer = 0f;
    private bool showingNotification = false;
    private PlayerController player;
    
    // Сообщения для экрана победы
    private readonly string[] winMessages = {
        "Отлично! Вы сдали сессию!",
        "Физтех пройден! Можно отдыхать.",
        "Все интегралы решены! Ты гений!",
        "Сессия сдана! Лето свободно!"
    };
    
    // Сообщения для экрана поражения
    private readonly string[] loseMessages = {
        "Вас отчислили...",
        "Сессия не сдана. Пересдача через год.",
        "Профессор вас поймал! Конец игры.",
        "Вы не справились с интегралами..."
    };
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        
        // Скрываем все панели
        HideAllPanels();
        
        // Показываем главное меню
        ShowMainMenu();
        
        // Настраиваем кнопки сложности
        SetupDifficultyButtons();
    }
    
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            UpdateHUD();
        }
        
        // Обработка уведомления
        if (showingNotification)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0)
            {
                HideIntegralNotification();
            }
        }
        
        // Пульсация индикатора обнаружения
        if (detectionWarningPanel != null && detectionWarningPanel.activeSelf)
        {
            float alpha = 0.5f + 0.5f * Mathf.Sin(Time.time * warningPulseSpeed);
            detectionIndicator.color = new Color(1, 0, 0, alpha);
        }
    }
    
    void UpdateHUD()
    {
        if (player != null)
        {
            // Обновляем счетчик интегралов
            if (integralsText != null)
            {
                integralsText.text = $"∫: {player.collectedIntegrals}/{player.totalIntegrals}";
            }
            
            // Обновляем выносливость
            float staminaPercent = player.GetStaminaPercentage();
            if (staminaSlider != null)
            {
                staminaSlider.value = staminaPercent;
            }
            
            // Цвет выносливости
            if (staminaFillImage != null)
            {
                if (staminaPercent > 0.5f)
                    staminaFillImage.color = staminaHighColor;
                else if (staminaPercent > 0.25f)
                    staminaFillImage.color = staminaMediumColor;
                else
                    staminaFillImage.color = staminaLowColor;
            }
            
            if (staminaText != null)
            {
                staminaText.text = $"Выносливость: {staminaPercent * 100:0}%";
            }
        }
        
        // Обновляем время
        if (timeText != null && GameManager.Instance != null)
        {
            timeText.text = GameManager.Instance.FormatTime(GameManager.Instance.gameTime);
        }
    }
    
    public void ShowIntegralCollected(string formula, Integral.Difficulty difficulty)
    {
        if (integralNotificationPanel != null)
        {
            integralNotificationPanel.SetActive(true);
            
            if (integralFormulaText != null)
            {
                integralFormulaText.text = formula;
            }
            
            if (integralDifficultyText != null)
            {
                integralDifficultyText.text = Integral.GetIntegralDescription(difficulty);
                
                // Цвет в зависимости от сложности
                switch (difficulty)
                {
                    case Integral.Difficulty.Easy:
                        integralDifficultyText.color = Color.green;
                        break;
                    case Integral.Difficulty.Medium:
                        integralDifficultyText.color = Color.yellow;
                        break;
                    case Integral.Difficulty.Hard:
                        integralDifficultyText.color = Color.red;
                        break;
                    case Integral.Difficulty.Legendary:
                        integralDifficultyText.color = Color.magenta;
                        break;
                }
            }
            
            notificationTimer = notificationDuration;
            showingNotification = true;
        }
    }
    
    void HideIntegralNotification()
    {
        if (integralNotificationPanel != null)
        {
            integralNotificationPanel.SetActive(false);
        }
        showingNotification = false;
    }
    
    public void ShowWinScreen(float gameTime)
    {
        HideAllPanels();
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winTimeText != null)
            {
                winTimeText.text = $"Время: {GameManager.Instance.FormatTime(gameTime)}";
            }
            
            if (winMessageText != null)
            {
                winMessageText.text = winMessages[Random.Range(0, winMessages.Length)];
            }
        }
    }
    
    public void ShowGameOverScreen()
    {
        HideAllPanels();
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = loseMessages[Random.Range(0, loseMessages.Length)];
            }
        }
    }
    
    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(show);
        }
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(!show);
        }
    }
    
    public void ShowMainMenu()
    {
        HideAllPanels();
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ShowDetectionWarning(bool show)
    {
        if (detectionWarningPanel != null)
        {
            detectionWarningPanel.SetActive(show);
        }
    }
    
    void HideAllPanels()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (integralNotificationPanel != null) integralNotificationPanel.SetActive(false);
        if (detectionWarningPanel != null) detectionWarningPanel.SetActive(false);
    }
    
    void SetupDifficultyButtons()
    {
        if (easyButton != null)
            easyButton.onClick.AddListener(() => StartGameWithDifficulty(GameManager.GameDifficulty.Easy));
        
        if (normalButton != null)
            normalButton.onClick.AddListener(() => StartGameWithDifficulty(GameManager.GameDifficulty.Normal));
        
        if (hardButton != null)
            hardButton.onClick.AddListener(() => StartGameWithDifficulty(GameManager.GameDifficulty.Hard));
        
        if (phystechButton != null)
            phystechButton.onClick.AddListener(() => StartGameWithDifficulty(GameManager.GameDifficulty.Phystech));
    }
    
    void StartGameWithDifficulty(GameManager.GameDifficulty difficulty)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDifficulty(difficulty);
        }
        
        HideAllPanels();
        
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
    
    // Кнопки UI
    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }
    
    public void OnMainMenuButton()
    {
        GameManager.Instance?.LoadMainMenu();
    }
    
    public void OnResumeButton()
    {
        GameManager.Instance?.TogglePause();
    }
    
    public void OnQuitButton()
    {
        Application.Quit();
    }
}
