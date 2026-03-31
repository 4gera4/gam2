using UnityEngine;

public class Integral : MonoBehaviour
{
    [Header("Visual")]
    public float rotationSpeed = 50f;
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.3f;
    
    [Header("Effects")]
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    
    [Header("Integral Info")]
    public string integralFormula;
    public Difficulty difficulty;
    
    public enum Difficulty
    {
        Easy,      // Простой интеграл
        Medium,    // Средний
        Hard,      // Сложный
        Legendary  // Легендарный (бонусный)
    }
    
    private Vector3 startPosition;
    private float floatOffset;
    private AudioSource audioSource;
    private bool isCollected = false;
    
    // Массив формул интегралов для разных сложностей
    private static readonly string[] easyIntegrals = {
        "∫x dx", "∫2x dx", "∫dx", "∫x² dx", "∫3x² dx"
    };
    
    private static readonly string[] mediumIntegrals = {
        "∫sin(x) dx", "∫cos(x) dx", "∫e^x dx", "∫1/x dx", "∫ln(x) dx"
    };
    
    private static readonly string[] hardIntegrals = {
        "∫x·sin(x) dx", "∫x·e^x dx", "∫e^x·sin(x) dx", "∫ln²(x) dx", "∫x²·e^x dx"
    };
    
    private static readonly string[] legendaryIntegrals = {
        "∫e^(-x²) dx", "∫sin(x²) dx", "∫cos(x²) dx", "∫dx/ln(x)", "∫√(1-k²sin²x) dx"
    };
    
    void Start()
    {
        startPosition = transform.position;
        floatOffset = Random.Range(0f, Mathf.PI * 2f);
        audioSource = GetComponent<AudioSource>();
        
        // Если формула не задана, выбираем случайную
        if (string.IsNullOrEmpty(integralFormula))
        {
            integralFormula = GetRandomIntegralFormula();
        }
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Вращение
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Парение вверх-вниз
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed + floatOffset) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        if (other.CompareTag("Player"))
        {
            Collect(other.GetComponent<PlayerController>());
        }
    }
    
    void Collect(PlayerController player)
    {
        isCollected = true;
        
        // Собираем интеграл
        if (player != null)
        {
            player.CollectIntegral();
        }
        
        // Эффект сбора
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect.gameObject, 2f);
        }
        
        // Звук сбора
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Показываем UI с формулой
        ShowIntegralFormula();
        
        // Уничтожаем объект
        Destroy(gameObject, 0.1f);
    }
    
    string GetRandomIntegralFormula()
    {
        string[] formulas;
        
        switch (difficulty)
        {
            case Difficulty.Easy:
                formulas = easyIntegrals;
                break;
            case Difficulty.Medium:
                formulas = mediumIntegrals;
                break;
            case Difficulty.Hard:
                formulas = hardIntegrals;
                break;
            case Difficulty.Legendary:
                formulas = legendaryIntegrals;
                break;
            default:
                formulas = easyIntegrals;
                break;
        }
        
        return formulas[Random.Range(0, formulas.Length)];
    }
    
    void ShowIntegralFormula()
    {
        // Здесь можно вызвать UI менеджер для показа формулы
        Debug.Log($"Собран интеграл: {integralFormula}");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowIntegralCollected(integralFormula, difficulty);
        }
    }
    
    public static string GetIntegralDescription(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.Easy:
                return "Простой интеграл";
            case Difficulty.Medium:
                return "Средний интеграл";
            case Difficulty.Hard:
                return "Сложный интеграл";
            case Difficulty.Legendary:
                return "Легендарный интеграл!";
            default:
                return "Интеграл";
        }
    }
}
