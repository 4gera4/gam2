using UnityEngine;
using UnityEngine.AI;

public class TeacherAI : MonoBehaviour
{
    [Header("AI Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f;
    public float detectionRange = 15f;
    public float fieldOfView = 120f;
    public float loseSightRange = 20f;
    public float attackRange = 2f;
    
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 3f;
    
    [Header("Teacher Type")]
    public TeacherType teacherType;
    public string teacherName;
    
    public enum TeacherType
    {
        MathAnalyst,      // Математический анализ
        LinearAlgebra,    // Линейная алгебра
        TheoreticalMechanics, // Теормех
        Physics,          // Общая физика
        Programming       // Программирование
    }
    
    private NavMeshAgent agent;
    private Transform player;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isChasing = false;
    private bool isWaiting = false;
    private Vector3 lastKnownPlayerPosition;
    private float searchTimer = 0f;
    
    [Header("Audio")]
    public AudioClip[] detectionSounds;
    public AudioClip[] catchSounds;
    private AudioSource audioSource;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Установка имени преподавателя
        if (string.IsNullOrEmpty(teacherName))
        {
            teacherName = GetRandomTeacherName();
        }
        
        GoToNextPatrolPoint();
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CanSeePlayer();
        
        if (canSeePlayer)
        {
            // Увидел игрока - начинаем преследование
            isChasing = true;
            lastKnownPlayerPosition = player.position;
            ChasePlayer();
        }
        else if (isChasing)
        {
            // Потерял из виду
            if (distanceToPlayer > loseSightRange)
            {
                // Игрок слишком далеко - возвращаемся к патрулю
                isChasing = false;
                GoToNextPatrolPoint();
            }
            else
            {
                // Идем к последней известной позиции
                agent.SetDestination(lastKnownPlayerPosition);
                
                // Если дошли до позиции и не нашли игрока
                if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1f)
                {
                    searchTimer += Time.deltaTime;
                    if (searchTimer > 3f)
                    {
                        isChasing = false;
                        searchTimer = 0f;
                        GoToNextPatrolPoint();
                    }
                }
            }
        }
        else
        {
            // Патрулирование
            Patrol();
        }
        
        // Проверка на поимку
        if (distanceToPlayer < attackRange)
        {
            CatchPlayer();
        }
    }
    
    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angleToPlayer > fieldOfView / 2f) return false;
        
        // Raycast для проверки препятствий
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
        
        // Проигрываем звук обнаружения (один раз)
        if (!audioSource.isPlaying && detectionSounds.Length > 0)
        {
            audioSource.PlayOneShot(detectionSounds[Random.Range(0, detectionSounds.Length)]);
        }
    }
    
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;
        
        agent.speed = patrolSpeed;
        
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                GoToNextPatrolPoint();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }
    
    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
    
    void CatchPlayer()
    {
        // Проигрываем звук поимки
        if (catchSounds.Length > 0)
        {
            audioSource.PlayOneShot(catchSounds[Random.Range(0, catchSounds.Length)]);
        }
        
        // Вызываем поимку игрока
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.GetCaught();
        }
    }
    
    string GetRandomTeacherName()
    {
        string[] mathAnalystNames = { "Проф. Иванов", "Проф. Петров", "Доц. Сидоров" };
        string[] linearAlgebraNames = { "Проф. Кузнецов", "Доц. Смирнов" };
        string[] theormechNames = { "Проф. Марков", "Доц. Васильев" };
        string[] physicsNames = { "Проф. Лебедев", "Доц. Федоров" };
        string[] programmingNames = { "Проф. Козлов", "Доц. Новиков" };
        
        switch (teacherType)
        {
            case TeacherType.MathAnalyst:
                return mathAnalystNames[Random.Range(0, mathAnalystNames.Length)];
            case TeacherType.LinearAlgebra:
                return linearAlgebraNames[Random.Range(0, linearAlgebraNames.Length)];
            case TeacherType.TheoreticalMechanics:
                return theormechNames[Random.Range(0, theormechNames.Length)];
            case TeacherType.Physics:
                return physicsNames[Random.Range(0, physicsNames.Length)];
            case TeacherType.Programming:
                return programmingNames[Random.Range(0, programmingNames.Length)];
            default:
                return "Преподаватель";
        }
    }
    
    // Визуализация зоны обнаружения в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Поле зрения
        Vector3 forward = transform.forward * detectionRange;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-fieldOfView / 2f, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(fieldOfView / 2f, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up, leftRayDirection);
        Gizmos.DrawRay(transform.position + Vector3.up, rightRayDirection);
    }
}
