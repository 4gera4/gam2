// ============================================
// ФИЗТЕХ ХОРРОР - 3D игра на Three.js
// ============================================

// Глобальные переменные
let scene, camera, renderer;
let player, playerVelocity, playerDirection;
let moveForward = false, moveBackward = false, moveLeft = false, moveRight = false;
let isRunning = false;
let stamina = 100;
let collectedIntegrals = 0;
const totalIntegrals = 5;
let gameTime = 0;
let isGameActive = false;
let isPaused = false;
let difficulty = 'normal';
let gameInitialized = false;

// Настройки
const WALK_SPEED = 5;
const RUN_SPEED = 10;
const STAMINA_DRAIN = 20;
const STAMINA_REGEN = 10;
const MOUSE_SENSITIVITY = 0.002;

// Объекты игры
let teachers = [];
let integrals = [];
let walls = [];
let hidingSpots = [];
let map = [];

// Размеры
const CELL_SIZE = 10;
const WALL_HEIGHT = 8;
const MAP_WIDTH = 15;
const MAP_HEIGHT = 15;

// Указатель мыши
let isPointerLocked = false;

// ============================================
// ИНИЦИАЛИЗАЦИЯ
// ============================================

function init() {
    console.log('Initializing game...');
    
    try {
        // Проверяем, загрузился ли Three.js
        if (typeof THREE === 'undefined') {
            console.error('Three.js not loaded!');
            showError('Three.js не загрузился. Проверьте интернет-соединение.');
            return;
        }
        
        // Сцена
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x111111);
        scene.fog = new THREE.Fog(0x111111, 10, 60);

        // Камера
        camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
        camera.position.y = 3;

        // Рендерер
        const canvas = document.getElementById('gameCanvas');
        if (!canvas) {
            console.error('Canvas not found!');
            return;
        }
        
        renderer = new THREE.WebGLRenderer({ 
            canvas: canvas,
            antialias: true 
        });
        renderer.setSize(window.innerWidth, window.innerHeight);
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        // Освещение
        setupLighting();

        // Создаем карту
        generateMap();

        // Создаем игрока
        createPlayer();

        // Создаем преподавателей
        spawnTeachers();

        // Создаем интегралы
        spawnIntegrals();

        // Обработчики событий
        setupEventListeners();

        // Обработка изменения размера окна
        window.addEventListener('resize', onWindowResize);

        gameInitialized = true;
        console.log('Game initialized successfully!');
        
        // Запускаем игровой цикл
        animate();
    } catch (error) {
        console.error('Error initializing game:', error);
        showError('Ошибка инициализации: ' + error.message);
    }
}

function showError(message) {
    const errorDiv = document.createElement('div');
    errorDiv.style.cssText = `
        position: fixed;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        background: rgba(255, 0, 0, 0.9);
        color: white;
        padding: 20px;
        border-radius: 10px;
        font-size: 18px;
        z-index: 10000;
        text-align: center;
    `;
    errorDiv.textContent = message;
    document.body.appendChild(errorDiv);
}

function setupLighting() {
    // Фоновый свет (темный)
    const ambientLight = new THREE.AmbientLight(0x404040, 0.3);
    scene.add(ambientLight);

    // Фонарик игрока
    const playerLight = new THREE.SpotLight(0xffffff, 1, 40, Math.PI / 4, 0.5, 1);
    playerLight.position.set(0, 3, 0);
    playerLight.castShadow = true;
    playerLight.shadow.mapSize.width = 1024;
    playerLight.shadow.mapSize.height = 1024;
    camera.add(playerLight);
    playerLight.target = camera;

    // Случайные лампы в коридорах
    for (let i = 0; i < 10; i++) {
        const x = Math.random() * MAP_WIDTH * CELL_SIZE - (MAP_WIDTH * CELL_SIZE) / 2;
        const z = Math.random() * MAP_HEIGHT * CELL_SIZE - (MAP_HEIGHT * CELL_SIZE) / 2;
        
        const lampLight = new THREE.PointLight(0xffaa00, 0.5, 20);
        lampLight.position.set(x, 6, z);
        scene.add(lampLight);
    }
}

// ============================================
// ГЕНЕРАЦИЯ КАРТЫ
// ============================================

function generateMap() {
    console.log('Generating map...');
    
    // Инициализируем карту
    for (let x = 0; x < MAP_WIDTH; x++) {
        map[x] = [];
        for (let z = 0; z < MAP_HEIGHT; z++) {
            map[x][z] = 1; // 1 = стена
        }
    }

    // Генерируем комнаты и коридоры
    const rooms = [];
    const numRooms = 8;

    for (let i = 0; i < numRooms; i++) {
        const roomWidth = Math.floor(Math.random() * 4) + 3;
        const roomHeight = Math.floor(Math.random() * 4) + 3;
        const roomX = Math.floor(Math.random() * (MAP_WIDTH - roomWidth - 2)) + 1;
        const roomZ = Math.floor(Math.random() * (MAP_HEIGHT - roomHeight - 2)) + 1;

        // Создаем комнату
        for (let x = roomX; x < roomX + roomWidth; x++) {
            for (let z = roomZ; z < roomZ + roomHeight; z++) {
                map[x][z] = 0; // 0 = пол
            }
        }

        rooms.push({ x: roomX + Math.floor(roomWidth / 2), z: roomZ + Math.floor(roomHeight / 2) });

        // Соединяем с предыдущей комнатой
        if (i > 0) {
            const prevRoom = rooms[i - 1];
            const currRoom = rooms[i];

            // Горизонтальный коридор
            const startX = Math.min(prevRoom.x, currRoom.x);
            const endX = Math.max(prevRoom.x, currRoom.x);
            for (let x = startX; x <= endX; x++) {
                map[x][prevRoom.z] = 0;
            }

            // Вертикальный коридор
            const startZ = Math.min(prevRoom.z, currRoom.z);
            const endZ = Math.max(prevRoom.z, currRoom.z);
            for (let z = startZ; z <= endZ; z++) {
                map[currRoom.x][z] = 0;
            }
        }
    }

    // Создаем 3D объекты
    createMapObjects();
    console.log('Map generated!');
}

function createMapObjects() {
    // Текстуры
    const wallTexture = createWallTexture();
    const floorTexture = createFloorTexture();

    // Материалы
    const wallMaterial = new THREE.MeshStandardMaterial({ 
        map: wallTexture,
        roughness: 0.8 
    });
    const floorMaterial = new THREE.MeshStandardMaterial({ 
        map: floorTexture,
        roughness: 0.9 
    });

    // Создаем стены и пол
    const wallGeometry = new THREE.BoxGeometry(CELL_SIZE, WALL_HEIGHT, CELL_SIZE);
    const floorGeometry = new THREE.PlaneGeometry(CELL_SIZE, CELL_SIZE);

    for (let x = 0; x < MAP_WIDTH; x++) {
        for (let z = 0; z < MAP_HEIGHT; z++) {
            const worldX = (x - MAP_WIDTH / 2) * CELL_SIZE;
            const worldZ = (z - MAP_HEIGHT / 2) * CELL_SIZE;

            if (map[x][z] === 1) {
                // Стена
                const wall = new THREE.Mesh(wallGeometry, wallMaterial);
                wall.position.set(worldX, WALL_HEIGHT / 2, worldZ);
                wall.castShadow = true;
                wall.receiveShadow = true;
                scene.add(wall);
                walls.push(wall);
            } else {
                // Пол
                const floor = new THREE.Mesh(floorGeometry, floorMaterial);
                floor.rotation.x = -Math.PI / 2;
                floor.position.set(worldX, 0, worldZ);
                floor.receiveShadow = true;
                scene.add(floor);

                // Потолок
                const ceiling = new THREE.Mesh(floorGeometry, new THREE.MeshStandardMaterial({ 
                    color: 0x333333 
                }));
                ceiling.rotation.x = Math.PI / 2;
                ceiling.position.set(worldX, WALL_HEIGHT, worldZ);
                scene.add(ceiling);
            }
        }
    }

    // Добавляем декорации
    addDecorations();
}

function createWallTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Фон
    ctx.fillStyle = '#555';
    ctx.fillRect(0, 0, 256, 256);

    // Кирпичи
    ctx.fillStyle = '#444';
    for (let y = 0; y < 256; y += 32) {
        const offset = (y / 32) % 2 === 0 ? 0 : 16;
        for (let x = -16; x < 256; x += 32) {
            ctx.fillRect(x + offset, y, 30, 30);
        }
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
}

function createFloorTexture() {
    const canvas = document.createElement('canvas');
    canvas.width = 256;
    canvas.height = 256;
    const ctx = canvas.getContext('2d');

    // Фон
    ctx.fillStyle = '#333';
    ctx.fillRect(0, 0, 256, 256);

    // Плитка
    ctx.strokeStyle = '#444';
    ctx.lineWidth = 2;
    for (let i = 0; i <= 256; i += 64) {
        ctx.beginPath();
        ctx.moveTo(i, 0);
        ctx.lineTo(i, 256);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(0, i);
        ctx.lineTo(256, i);
        ctx.stroke();
    }

    const texture = new THREE.CanvasTexture(canvas);
    texture.wrapS = THREE.RepeatWrapping;
    texture.wrapT = THREE.RepeatWrapping;
    return texture;
}

function addDecorations() {
    // Добавляем парты
    const deskGeometry = new THREE.BoxGeometry(2, 1.5, 1);
    const deskMaterial = new THREE.MeshStandardMaterial({ color: 0x8B4513 });

    for (let i = 0; i < 15; i++) {
        let x, z;
        let attempts = 0;
        do {
            x = Math.floor(Math.random() * MAP_WIDTH);
            z = Math.floor(Math.random() * MAP_HEIGHT);
            attempts++;
        } while ((map[x][z] !== 0) && attempts < 50);
        
        if (attempts >= 50) continue;

        const desk = new THREE.Mesh(deskGeometry, deskMaterial);
        desk.position.set(
            (x - MAP_WIDTH / 2) * CELL_SIZE,
            0.75,
            (z - MAP_HEIGHT / 2) * CELL_SIZE
        );
        desk.rotation.y = Math.random() * Math.PI * 2;
        desk.castShadow = true;
        scene.add(desk);
    }

    // Добавляем доски
    const boardGeometry = new THREE.BoxGeometry(4, 2, 0.1);
    const boardMaterial = new THREE.MeshStandardMaterial({ color: 0x222222 });

    for (let i = 0; i < 5; i++) {
        let x, z;
        let attempts = 0;
        do {
            x = Math.floor(Math.random() * MAP_WIDTH);
            z = Math.floor(Math.random() * MAP_HEIGHT);
            attempts++;
        } while ((map[x][z] !== 0) && attempts < 50);
        
        if (attempts >= 50) continue;

        const board = new THREE.Mesh(boardGeometry, boardMaterial);
        board.position.set(
            (x - MAP_WIDTH / 2) * CELL_SIZE,
            3,
            (z - MAP_HEIGHT / 2) * CELL_SIZE - 4
        );
        scene.add(board);
    }
}

// ============================================
// ИГРОК
// ============================================

function createPlayer() {
    console.log('Creating player...');
    player = new THREE.Object3D();
    player.position.set(0, 3, 0);
    scene.add(player);
    player.add(camera);

    playerVelocity = new THREE.Vector3();
    playerDirection = new THREE.Vector3();
}

// ============================================
// ПРЕПОДАВАТЕЛИ
// ============================================

function spawnTeachers() {
    console.log('Spawning teachers...');
    const teacherCount = getTeacherCount();
    
    // Очищаем старых преподавателей
    teachers.forEach(t => {
        if (t.mesh && t.mesh.parent) {
            scene.remove(t.mesh);
        }
    });
    teachers = [];

    for (let i = 0; i < teacherCount; i++) {
        let x, z;
        let attempts = 0;
        do {
            x = Math.floor(Math.random() * MAP_WIDTH);
            z = Math.floor(Math.random() * MAP_HEIGHT);
            attempts++;
        } while ((map[x][z] !== 0 || getDistanceToPlayer(x, z) < 5) && attempts < 50);
        
        if (attempts >= 50) continue;

        const teacherColor = getTeacherColor(i);

        // Создаем преподавателя из группы объектов (вместо CapsuleGeometry)
        const teacher = new THREE.Group();
        
        // Тело (цилиндр)
        const bodyGeometry = new THREE.CylinderGeometry(0.8, 0.8, 2, 16);
        const bodyMaterial = new THREE.MeshStandardMaterial({ color: teacherColor });
        const body = new THREE.Mesh(bodyGeometry, bodyMaterial);
        body.castShadow = true;
        teacher.add(body);
        
        // Верхняя полусфера
        const topSphere = new THREE.Mesh(
            new THREE.SphereGeometry(0.8, 16, 8, 0, Math.PI * 2, 0, Math.PI / 2),
            bodyMaterial
        );
        topSphere.position.y = 1;
        topSphere.castShadow = true;
        teacher.add(topSphere);
        
        // Нижняя полусфера
        const bottomSphere = new THREE.Mesh(
            new THREE.SphereGeometry(0.8, 16, 8, 0, Math.PI * 2, Math.PI / 2, Math.PI / 2),
            bodyMaterial
        );
        bottomSphere.position.y = -1;
        bottomSphere.castShadow = true;
        teacher.add(bottomSphere);
        
        teacher.position.set(
            (x - MAP_WIDTH / 2) * CELL_SIZE,
            2,
            (z - MAP_HEIGHT / 2) * CELL_SIZE
        );
        scene.add(teacher);

        // Добавляем глаза (светящиеся)
        const eyeGeometry = new THREE.SphereGeometry(0.15, 8, 8);
        const eyeMaterial = new THREE.MeshBasicMaterial({ color: 0xff0000 });
        
        const leftEye = new THREE.Mesh(eyeGeometry, eyeMaterial);
        leftEye.position.set(-0.3, 0.5, 0.7);
        teacher.add(leftEye);
        
        const rightEye = new THREE.Mesh(eyeGeometry, eyeMaterial);
        rightEye.position.set(0.3, 0.5, 0.7);
        teacher.add(rightEye);

        teachers.push({
            mesh: teacher,
            position: new THREE.Vector3(teacher.position.x, teacher.position.y, teacher.position.z),
            speed: getTeacherSpeed(),
            detectionRange: getDetectionRange(),
            chaseRange: 20,
            isChasing: false,
            patrolTarget: null,
            waitTime: 0
        });
    }
    console.log(`Spawned ${teachers.length} teachers`);
}

function getTeacherColor(index) {
    const colors = [0x4169E1, 0x228B22, 0x8B4513, 0x800080, 0xFF6347, 0x2F4F4F, 0x8B0000];
    return colors[index % colors.length];
}

function getTeacherCount() {
    switch (difficulty) {
        case 'easy': return 2;
        case 'normal': return 3;
        case 'hard': return 5;
        case 'phystech': return 7;
        default: return 3;
    }
}

function getTeacherSpeed() {
    switch (difficulty) {
        case 'easy': return 2;
        case 'normal': return 3;
        case 'hard': return 4;
        case 'phystech': return 5;
        default: return 3;
    }
}

function getDetectionRange() {
    switch (difficulty) {
        case 'easy': return 10;
        case 'normal': return 15;
        case 'hard': return 20;
        case 'phystech': return 25;
        default: return 15;
    }
}

function updateTeachers(delta) {
    if (!player) return;
    
    const playerPos = player.position;
    let playerDetected = false;

    teachers.forEach(teacher => {
        const distance = teacher.mesh.position.distanceTo(playerPos);
        const canSeePlayer = distance < teacher.detectionRange && !isWallBetween(teacher.mesh.position, playerPos);

        if (canSeePlayer) {
            teacher.isChasing = true;
            playerDetected = true;
        } else if (distance > teacher.chaseRange) {
            teacher.isChasing = false;
        }

        if (teacher.isChasing) {
            // Преследование
            const direction = new THREE.Vector3()
                .subVectors(playerPos, teacher.mesh.position)
                .normalize();
            direction.y = 0;
            
            teacher.mesh.position.add(direction.multiplyScalar(teacher.speed * delta));
            teacher.mesh.lookAt(playerPos.x, teacher.mesh.position.y, playerPos.z);

            // Проверка поимки
            if (distance < 1.5) {
                gameOver();
            }
        } else {
            // Патрулирование
            patrol(teacher, delta);
        }
    });

    // Показываем/скрываем предупреждение
    const warning = document.getElementById('detection-warning');
    if (warning) {
        warning.style.display = playerDetected ? 'block' : 'none';
    }
}

function patrol(teacher, delta) {
    if (teacher.waitTime > 0) {
        teacher.waitTime -= delta;
        return;
    }

    if (teacher.patrolTarget === null || 
        teacher.mesh.position.distanceTo(teacher.patrolTarget) < 1) {
        // Выбираем новую цель
        let tx, tz;
        let attempts = 0;
        do {
            tx = Math.floor(Math.random() * MAP_WIDTH);
            tz = Math.floor(Math.random() * MAP_HEIGHT);
            attempts++;
        } while (map[tx][tz] !== 0 && attempts < 20);
        
        if (attempts >= 20) return;

        teacher.patrolTarget = new THREE.Vector3(
            (tx - MAP_WIDTH / 2) * CELL_SIZE,
            2,
            (tz - MAP_HEIGHT / 2) * CELL_SIZE
        );
        teacher.waitTime = Math.random() * 2;
    }

    const direction = new THREE.Vector3()
        .subVectors(teacher.patrolTarget, teacher.mesh.position)
        .normalize();
    direction.y = 0;

    teacher.mesh.position.add(direction.multiplyScalar(teacher.speed * 0.5 * delta));
    teacher.mesh.lookAt(teacher.patrolTarget.x, teacher.mesh.position.y, teacher.patrolTarget.z);
}

function isWallBetween(pos1, pos2) {
    // Упрощенная проверка
    const direction = new THREE.Vector3().subVectors(pos2, pos1).normalize();
    const distance = pos1.distanceTo(pos2);
    
    for (let d = 0; d < distance; d += 1) {
        const checkPos = pos1.clone().add(direction.clone().multiplyScalar(d));
        const gx = Math.floor((checkPos.x / CELL_SIZE) + MAP_WIDTH / 2);
        const gz = Math.floor((checkPos.z / CELL_SIZE) + MAP_HEIGHT / 2);
        
        if (gx >= 0 && gx < MAP_WIDTH && gz >= 0 && gz < MAP_HEIGHT) {
            if (map[gx][gz] === 1) return true;
        }
    }
    return false;
}

function getDistanceToPlayer(x, z) {
    const worldX = (x - MAP_WIDTH / 2) * CELL_SIZE;
    const worldZ = (z - MAP_HEIGHT / 2) * CELL_SIZE;
    return Math.sqrt(worldX * worldX + worldZ * worldZ);
}

// ============================================
// ИНТЕГРАЛЫ
// ============================================

function spawnIntegrals() {
    console.log('Spawning integrals...');
    
    // Очищаем старые интегралы
    integrals.forEach(i => {
        if (i.mesh && i.mesh.parent) scene.remove(i.mesh);
        if (i.light && i.light.parent) scene.remove(i.light);
    });
    integrals = [];
    
    const integralFormulas = [
        { formula: '∫x dx', difficulty: 'Простой', color: 0x00ff00 },
        { formula: '∫x² dx', difficulty: 'Простой', color: 0x00ff00 },
        { formula: '∫sin(x) dx', difficulty: 'Средний', color: 0xffff00 },
        { formula: '∫cos(x) dx', difficulty: 'Средний', color: 0xffff00 },
        { formula: '∫e^x dx', difficulty: 'Средний', color: 0xffff00 },
        { formula: '∫ln(x) dx', difficulty: 'Сложный', color: 0xff0000 },
        { formula: '∫x·e^x dx', difficulty: 'Сложный', color: 0xff0000 },
        { formula: '∫e^(-x²) dx', difficulty: 'Легендарный', color: 0xff00ff }
    ];

    for (let i = 0; i < totalIntegrals; i++) {
        let x, z;
        let attempts = 0;
        do {
            x = Math.floor(Math.random() * MAP_WIDTH);
            z = Math.floor(Math.random() * MAP_HEIGHT);
            attempts++;
        } while ((map[x][z] !== 0) && attempts < 50);
        
        if (attempts >= 50) continue;

        const formulaData = integralFormulas[Math.floor(Math.random() * integralFormulas.length)];

        // Создаем меш интеграла
        const geometry = new THREE.OctahedronGeometry(0.5, 0);
        const material = new THREE.MeshStandardMaterial({ 
            color: formulaData.color,
            emissive: formulaData.color,
            emissiveIntensity: 0.5
        });

        const integral = new THREE.Mesh(geometry, material);
        integral.position.set(
            (x - MAP_WIDTH / 2) * CELL_SIZE,
            2,
            (z - MAP_HEIGHT / 2) * CELL_SIZE
        );
        scene.add(integral);

        // Добавляем свечение
        const light = new THREE.PointLight(formulaData.color, 0.5, 5);
        light.position.copy(integral.position);
        scene.add(light);

        integrals.push({
            mesh: integral,
            light: light,
            formula: formulaData.formula,
            difficulty: formulaData.difficulty,
            collected: false,
            rotationSpeed: Math.random() * 2 + 1,
            floatOffset: Math.random() * Math.PI * 2
        });
    }
    console.log(`Spawned ${integrals.length} integrals`);
}

function updateIntegrals(delta) {
    integrals.forEach(integral => {
        if (integral.collected) return;

        // Вращение
        integral.mesh.rotation.y += integral.rotationSpeed * delta;
        integral.mesh.rotation.x += integral.rotationSpeed * 0.5 * delta;

        // Парение
        integral.mesh.position.y = 2 + Math.sin(Date.now() * 0.003 + integral.floatOffset) * 0.3;
        integral.light.position.copy(integral.mesh.position);

        // Проверка сбора
        if (player) {
            const distance = player.position.distanceTo(integral.mesh.position);
            if (distance < 2) {
                collectIntegral(integral);
            }
        }
    });
}

function collectIntegral(integral) {
    integral.collected = true;
    scene.remove(integral.mesh);
    scene.remove(integral.light);

    collectedIntegrals++;
    updateHUD();

    // Показываем уведомление
    showNotification(integral.formula, integral.difficulty);

    // Проверка победы
    if (collectedIntegrals >= totalIntegrals) {
        winGame();
    }
}

function showNotification(formula, difficultyText) {
    const notification = document.getElementById('notification');
    const formulaEl = document.getElementById('formula');
    const difficultyEl = document.getElementById('difficulty');

    if (!notification || !formulaEl || !difficultyEl) return;

    formulaEl.textContent = formula;
    difficultyEl.textContent = difficultyText + ' интеграл';
    
    // Цвет в зависимости от сложности
    if (difficultyText === 'Простой') {
        notification.style.borderColor = '#0f0';
        notification.style.color = '#0f0';
    } else if (difficultyText === 'Средний') {
        notification.style.borderColor = '#ff0';
        notification.style.color = '#ff0';
    } else if (difficultyText === 'Сложный') {
        notification.style.borderColor = '#f00';
        notification.style.color = '#f00';
    } else {
        notification.style.borderColor = '#f0f';
        notification.style.color = '#f0f';
    }

    notification.style.display = 'block';
    setTimeout(() => {
        notification.style.display = 'none';
    }, 3000);
}

// ============================================
// УПРАВЛЕНИЕ
// ============================================

function setupEventListeners() {
    // Клавиатура
    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('keyup', onKeyUp);

    // Мышь (Pointer Lock)
    document.addEventListener('click', () => {
        if (isGameActive && !isPaused) {
            document.body.requestPointerLock();
        }
    });

    document.addEventListener('pointerlockchange', () => {
        isPointerLocked = document.pointerLockElement === document.body;
    });

    document.addEventListener('mousemove', onMouseMove);
}

function onKeyDown(event) {
    if (!isGameActive || isPaused) return;
    
    switch (event.code) {
        case 'KeyW': moveForward = true; break;
        case 'KeyS': moveBackward = true; break;
        case 'KeyA': moveLeft = true; break;
        case 'KeyD': moveRight = true; break;
        case 'ShiftLeft': isRunning = true; break;
        case 'Escape': togglePause(); break;
    }
}

function onKeyUp(event) {
    switch (event.code) {
        case 'KeyW': moveForward = false; break;
        case 'KeyS': moveBackward = false; break;
        case 'KeyA': moveLeft = false; break;
        case 'KeyD': moveRight = false; break;
        case 'ShiftLeft': isRunning = false; break;
    }
}

function onMouseMove(event) {
    if (!isPointerLocked || !isGameActive || isPaused) return;

    const movementX = event.movementX || event.mozMovementX || event.webkitMovementX || 0;
    const movementY = event.movementY || event.mozMovementY || event.webkitMovementY || 0;

    player.rotation.y -= movementX * MOUSE_SENSITIVITY;
    camera.rotation.x -= movementY * MOUSE_SENSITIVITY;
    camera.rotation.x = Math.max(-Math.PI / 2, Math.min(Math.PI / 2, camera.rotation.x));
}

function updatePlayer(delta) {
    if (!isGameActive || isPaused || !player) return;

    // Выносливость
    if (isRunning && (moveForward || moveBackward || moveLeft || moveRight)) {
        stamina -= STAMINA_DRAIN * delta;
        stamina = Math.max(0, stamina);
    } else {
        stamina += STAMINA_REGEN * delta;
        stamina = Math.min(100, stamina);
    }

    if (stamina <= 0) {
        isRunning = false;
    }

    // Скорость
    const speed = (isRunning && stamina > 0) ? RUN_SPEED : WALK_SPEED;

    // Направление движения
    playerDirection.set(0, 0, 0);

    if (moveForward) playerDirection.z -= 1;
    if (moveBackward) playerDirection.z += 1;
    if (moveLeft) playerDirection.x -= 1;
    if (moveRight) playerDirection.x += 1;

    playerDirection.normalize();
    playerDirection.applyAxisAngle(new THREE.Vector3(0, 1, 0), player.rotation.y);

    // Движение с проверкой столкновений
    const moveX = playerDirection.x * speed * delta;
    const moveZ = playerDirection.z * speed * delta;

    if (!checkWallCollision(player.position.x + moveX, player.position.z)) {
        player.position.x += moveX;
    }
    if (!checkWallCollision(player.position.x, player.position.z + moveZ)) {
        player.position.z += moveZ;
    }

    // Обновляем HUD
    updateHUD();
}

function checkWallCollision(x, z) {
    const gx = Math.floor((x / CELL_SIZE) + MAP_WIDTH / 2);
    const gz = Math.floor((z / CELL_SIZE) + MAP_HEIGHT / 2);

    if (gx < 0 || gx >= MAP_WIDTH || gz < 0 || gz >= MAP_HEIGHT) {
        return true;
    }

    return map[gx][gz] === 1;
}

// ============================================
// UI ФУНКЦИИ
// ============================================

function updateHUD() {
    const integralCount = document.getElementById('integral-count');
    const staminaFill = document.getElementById('stamina-fill');
    const gameTimeEl = document.getElementById('game-time');

    if (integralCount) {
        integralCount.textContent = `${collectedIntegrals}/${totalIntegrals}`;
    }
    
    if (staminaFill) {
        staminaFill.style.width = `${stamina}%`;
        
        // Цвет выносливости
        if (stamina > 50) {
            staminaFill.style.background = 'linear-gradient(90deg, #0f0, #0a0)';
        } else if (stamina > 25) {
            staminaFill.style.background = 'linear-gradient(90deg, #ff0, #aa0)';
        } else {
            staminaFill.style.background = 'linear-gradient(90deg, #f00, #a00)';
        }
    }

    // Время
    if (gameTimeEl) {
        const minutes = Math.floor(gameTime / 60);
        const seconds = Math.floor(gameTime % 60);
        gameTimeEl.textContent = 
            `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
}

function startGame(diff) {
    console.log('Starting game with difficulty:', diff);
    
    if (!gameInitialized) {
        console.error('Game not initialized yet!');
        return;
    }
    
    difficulty = diff;
    
    const mainMenu = document.getElementById('mainMenu');
    const hud = document.getElementById('hud');
    
    if (mainMenu) mainMenu.classList.add('hidden');
    if (hud) hud.style.display = 'block';
    
    isGameActive = true;
    isPaused = false;
    
    // Сброс игры
    resetGame();
    
    // Захватываем мышь
    document.body.requestPointerLock();
}

function resetGame() {
    console.log('Resetting game...');
    
    // Сброс переменных
    collectedIntegrals = 0;
    stamina = 100;
    gameTime = 0;
    
    // Удаляем старых преподавателей
    teachers.forEach(t => {
        if (t.mesh && t.mesh.parent) {
            scene.remove(t.mesh);
        }
    });
    teachers = [];
    
    // Удаляем старые интегралы
    integrals.forEach(i => {
        if (i.mesh && i.mesh.parent) scene.remove(i.mesh);
        if (i.light && i.light.parent) scene.remove(i.light);
    });
    integrals = [];
    
    // Пересоздаем
    spawnTeachers();
    spawnIntegrals();
    
    // Сброс позиции игрока
    if (player) {
        player.position.set(0, 3, 0);
        player.rotation.set(0, 0, 0);
    }
    if (camera) {
        camera.rotation.set(0, 0, 0);
    }
}

function togglePause() {
    if (!isGameActive) return;
    
    isPaused = !isPaused;
    
    const pauseMenu = document.getElementById('pauseMenu');
    
    if (isPaused) {
        document.exitPointerLock();
        if (pauseMenu) pauseMenu.classList.remove('hidden');
    } else {
        document.body.requestPointerLock();
        if (pauseMenu) pauseMenu.classList.add('hidden');
    }
}

function resumeGame() {
    togglePause();
}

function restartGame() {
    const winScreen = document.getElementById('winScreen');
    const gameOverScreen = document.getElementById('gameOverScreen');
    const pauseMenu = document.getElementById('pauseMenu');
    const hud = document.getElementById('hud');

    if (winScreen) winScreen.classList.add('hidden');
    if (gameOverScreen) gameOverScreen.classList.add('hidden');
    if (pauseMenu) pauseMenu.classList.add('hidden');
    if (hud) hud.style.display = 'block';
    
    isGameActive = true;
    isPaused = false;
    
    resetGame();
    document.body.requestPointerLock();
}

function backToMenu() {
    const winScreen = document.getElementById('winScreen');
    const gameOverScreen = document.getElementById('gameOverScreen');
    const pauseMenu = document.getElementById('pauseMenu');
    const hud = document.getElementById('hud');
    const mainMenu = document.getElementById('mainMenu');

    if (winScreen) winScreen.classList.add('hidden');
    if (gameOverScreen) gameOverScreen.classList.add('hidden');
    if (pauseMenu) pauseMenu.classList.add('hidden');
    if (hud) hud.style.display = 'none';
    if (mainMenu) mainMenu.classList.remove('hidden');
    
    isGameActive = false;
    isPaused = false;
    
    document.exitPointerLock();
}

function winGame() {
    isGameActive = false;
    document.exitPointerLock();
    
    const minutes = Math.floor(gameTime / 60);
    const seconds = Math.floor(gameTime % 60);
    
    const winTime = document.getElementById('win-time');
    const winMessage = document.getElementById('win-message');
    const winScreen = document.getElementById('winScreen');
    const hud = document.getElementById('hud');

    if (winTime) {
        winTime.textContent = `Время: ${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }
    
    const messages = [
        'Отлично! Вы сдали сессию!',
        'Физтех пройден! Можно отдыхать.',
        'Все интегралы решены! Ты гений!',
        'Сессия сдана! Лето свободно!'
    ];
    if (winMessage) {
        winMessage.textContent = messages[Math.floor(Math.random() * messages.length)];
    }
    
    if (hud) hud.style.display = 'none';
    if (winScreen) winScreen.classList.remove('hidden');
}

function gameOver() {
    isGameActive = false;
    document.exitPointerLock();
    
    const loseMessage = document.getElementById('lose-message');
    const gameOverScreen = document.getElementById('gameOverScreen');
    const hud = document.getElementById('hud');

    const messages = [
        'Вас отчислили...',
        'Сессия не сдана. Пересдача через год.',
        'Профессор вас поймал! Конец игры.',
        'Вы не справились с интегралами...'
    ];
    if (loseMessage) {
        loseMessage.textContent = messages[Math.floor(Math.random() * messages.length)];
    }
    
    if (hud) hud.style.display = 'none';
    if (gameOverScreen) gameOverScreen.classList.remove('hidden');
}

function onWindowResize() {
    if (!camera || !renderer) return;
    
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}

// ============================================
// ИГРОВОЙ ЦИКЛ
// ============================================

function animate() {
    requestAnimationFrame(animate);

    const delta = 0.016; // ~60fps

    if (isGameActive && !isPaused) {
        gameTime += delta;
        updatePlayer(delta);
        updateTeachers(delta);
        updateIntegrals(delta);
    }

    if (renderer && scene && camera) {
        renderer.render(scene, camera);
    }
}

// Запускаем игру когда страница загрузится
window.addEventListener('load', init);
