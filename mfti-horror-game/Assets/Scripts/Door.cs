using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public bool isOpen = false;
    public bool isSlidingDoor = false;
    
    [Header("Animation")]
    public float openAngle = 90f;
    public float slideDistance = 2f;
    public float animationSpeed = 2f;
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockedSound;
    private AudioSource audioSource;
    
    [Header("Key")]
    public string requiredKeyId = "";
    
    private Vector3 closedPosition;
    private Vector3 closedRotation;
    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private bool isAnimating = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        closedPosition = transform.position;
        closedRotation = transform.eulerAngles;
        
        targetPosition = closedPosition;
        targetRotation = closedRotation;
    }
    
    void Update()
    {
        // Плавная анимация
        if (isAnimating)
        {
            if (isSlidingDoor)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * animationSpeed);
                if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                {
                    isAnimating = false;
                }
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * animationSpeed);
                if (Quaternion.Angle(transform.rotation, Quaternion.Euler(targetRotation)) < 0.5f)
                {
                    isAnimating = false;
                }
            }
        }
    }
    
    public void Interact()
    {
        if (isLocked)
        {
            // Проверяем, есть ли ключ у игрока
            if (InventoryManager.Instance != null && InventoryManager.Instance.HasKey(requiredKeyId))
            {
                Unlock();
                ToggleDoor();
            }
            else
            {
                PlayLockedSound();
                ShowLockedMessage();
            }
        }
        else
        {
            ToggleDoor();
        }
    }
    
    void ToggleDoor()
    {
        isOpen = !isOpen;
        isAnimating = true;
        
        if (isSlidingDoor)
        {
            // Раздвижная дверь
            if (isOpen)
            {
                targetPosition = closedPosition + transform.right * slideDistance;
                PlayOpenSound();
            }
            else
            {
                targetPosition = closedPosition;
                PlayCloseSound();
            }
        }
        else
        {
            // Обычная дверь
            if (isOpen)
            {
                targetRotation = closedRotation + new Vector3(0, openAngle, 0);
                PlayOpenSound();
            }
            else
            {
                targetRotation = closedRotation;
                PlayCloseSound();
            }
        }
    }
    
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("Дверь разблокирована!");
    }
    
    public void Lock()
    {
        isLocked = true;
    }
    
    void PlayOpenSound()
    {
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }
    }
    
    void PlayCloseSound()
    {
        if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }
    
    void PlayLockedSound()
    {
        if (lockedSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lockedSound);
        }
    }
    
    void ShowLockedMessage()
    {
        if (UIManager.Instance != null)
        {
            // Можно добавить показ сообщения "Дверь заперта"
            Debug.Log("Дверь заперта! Нужен ключ.");
        }
    }
    
    // Для триггера
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Автоматическое открытие при приближении (опционально)
            // if (!isLocked && !isOpen) ToggleDoor();
        }
    }
}
