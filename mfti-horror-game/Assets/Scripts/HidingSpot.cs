using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Hiding Spot")]
    public Transform hidePosition;
    public float hideTransitionTime = 0.5f;
    
    [Header("Detection")]
    public float detectionRadius = 3f;
    private bool playerInRange = false;
    private bool isHiding = false;
    private GameObject player;
    private Vector3 playerOriginalPosition;
    private CharacterController playerController;
    
    [Header("Audio")]
    public AudioClip hideSound;
    public AudioClip exitSound;
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (hidePosition == null)
        {
            hidePosition = transform;
        }
    }
    
    void Update()
    {
        // Проверяем, находится ли игрок в радиусе
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerInRange = distance < detectionRadius;
            
            // Подсказка для игрока
            if (playerInRange && !isHiding)
            {
                ShowHidePrompt();
            }
        }
        
        // Нажатие клавиши для прятаться/выйти
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isHiding)
            {
                ExitHiding();
            }
            else if (playerInRange)
            {
                EnterHiding();
            }
        }
    }
    
    void EnterHiding()
    {
        if (player == null) return;
        
        isHiding = true;
        playerController = player.GetComponent<CharacterController>();
        
        // Сохраняем оригинальную позицию
        playerOriginalPosition = player.transform.position;
        
        // Отключаем контроллер
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Перемещаем игрока в точку прятания
        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;
        
        // Скрываем игрока (опционально)
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }
        
        // Звук
        if (hideSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hideSound);
        }
        
        // Уведомление
        Debug.Log("Вы спрятались! Нажмите E чтобы выйти.");
        
        // Сообщаем преподавателям, что игрок спрятан
        NotifyTeachersPlayerHiding(true);
    }
    
    void ExitHiding()
    {
        if (player == null) return;
        
        isHiding = false;
        
        // Возвращаем игрока
        player.transform.position = playerOriginalPosition;
        
        // Включаем контроллер
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        // Показываем игрока
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }
        
        // Звук
        if (exitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitSound);
        }
        
        Debug.Log("Вы вышли из укрытия.");
        
        // Сообщаем преподавателям
        NotifyTeachersPlayerHiding(false);
    }
    
    void NotifyTeachersPlayerHiding(bool hiding)
    {
        // Находим всех преподавателей и сообщаем им
        TeacherAI[] teachers = FindObjectsOfType<TeacherAI>();
        foreach (TeacherAI teacher in teachers)
        {
            // Можно добавить метод в TeacherAI для обработки этого
            // teacher.OnPlayerHiding(hiding);
        }
    }
    
    void ShowHidePrompt()
    {
        // Показать подсказку "Нажмите E чтобы спрятаться"
        // Можно использовать UI Manager
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isHiding)
            {
                player = null;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        if (hidePosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(hidePosition.position, 0.3f);
        }
    }
    
    public bool IsPlayerHiding()
    {
        return isHiding;
    }
}
