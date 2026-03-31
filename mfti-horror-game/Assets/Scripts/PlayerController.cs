using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float mouseSensitivity = 2f;
    
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    private float currentStamina;
    
    [Header("References")]
    public Camera playerCamera;
    public CharacterController controller;
    
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;
    private Vector3 velocity;
    private bool isRunning = false;
    
    // Собранные интегралы
    public int collectedIntegrals = 0;
    public int totalIntegrals = 5;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        
        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleStamina();
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        horizontalRotation += mouseX;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        
        transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Бег на Left Shift
        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // Гравитация
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Сброс скорости падения
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
    
    void HandleStamina()
    {
        if (isRunning && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }
    }
    
    public void CollectIntegral()
    {
        collectedIntegrals++;
        Debug.Log($"Интеграл собран! {collectedIntegrals}/{totalIntegrals}");
        
        // Проверка победы
        if (collectedIntegrals >= totalIntegrals)
        {
            GameManager.Instance.WinGame();
        }
    }
    
    public void GetCaught()
    {
        Debug.Log("Вас поймал преподаватель! Игра окончена.");
        GameManager.Instance.GameOver();
    }
    
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }
}
