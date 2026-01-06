using System.Text;

namespace AIOperator.Editor.Tools
{
    /// <summary>
    /// 工作流模板 - 包含常用脚本模板和预设
    /// </summary>
    public static class WorkflowTemplates
    {
        #region 角色控制脚本模板

        /// <summary>
        /// 生成 WASD 移动脚本
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="moveSpeed">移动速度</param>
        /// <param name="useRigidbody">是否使用 Rigidbody</param>
        public static string GeneratePlayerController(string className = "PlayerController", float moveSpeed = 5f, bool useRigidbody = true)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine($"    [Header(\"移动设置\")]");
            sb.AppendLine($"    public float moveSpeed = {moveSpeed}f;");

            if (useRigidbody)
            {
                sb.AppendLine("    ");
                sb.AppendLine("    private Rigidbody rb;");
                sb.AppendLine("    private Vector3 moveDirection;");
                sb.AppendLine();
                sb.AppendLine("    void Start()");
                sb.AppendLine("    {");
                sb.AppendLine("        rb = GetComponent<Rigidbody>();");
                sb.AppendLine("        if (rb == null)");
                sb.AppendLine("        {");
                sb.AppendLine("            rb = gameObject.AddComponent<Rigidbody>();");
                sb.AppendLine("        }");
                sb.AppendLine("        // 冻结旋转防止翻倒");
                sb.AppendLine("        rb.freezeRotation = true;");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    void Update()");
                sb.AppendLine("    {");
                sb.AppendLine("        // 获取输入");
                sb.AppendLine("        float horizontal = Input.GetAxisRaw(\"Horizontal\");");
                sb.AppendLine("        float vertical = Input.GetAxisRaw(\"Vertical\");");
                sb.AppendLine("        ");
                sb.AppendLine("        // 计算移动方向");
                sb.AppendLine("        moveDirection = new Vector3(horizontal, 0, vertical).normalized;");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    void FixedUpdate()");
                sb.AppendLine("    {");
                sb.AppendLine("        // 使用 Rigidbody 移动");
                sb.AppendLine("        if (moveDirection.magnitude > 0.1f)");
                sb.AppendLine("        {");
                sb.AppendLine("            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("    void Update()");
                sb.AppendLine("    {");
                sb.AppendLine("        // 获取输入");
                sb.AppendLine("        float horizontal = Input.GetAxisRaw(\"Horizontal\");");
                sb.AppendLine("        float vertical = Input.GetAxisRaw(\"Vertical\");");
                sb.AppendLine("        ");
                sb.AppendLine("        // 计算移动方向");
                sb.AppendLine("        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;");
                sb.AppendLine("        ");
                sb.AppendLine("        // Transform 移动");
                sb.AppendLine("        if (moveDirection.magnitude > 0.1f)");
                sb.AppendLine("        {");
                sb.AppendLine("            transform.position += moveDirection * moveSpeed * Time.deltaTime;");
                sb.AppendLine("        }");
                sb.AppendLine("    }");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 生成带跳跃的玩家控制脚本
        /// </summary>
        public static string GeneratePlayerControllerWithJump(string className = "PlayerController", float moveSpeed = 5f, float jumpForce = 8f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    [Header(\"移动设置\")]");
            sb.AppendLine($"    public float moveSpeed = {moveSpeed}f;");
            sb.AppendLine($"    public float jumpForce = {jumpForce}f;");
            sb.AppendLine("    ");
            sb.AppendLine("    [Header(\"地面检测\")]");
            sb.AppendLine("    public Transform groundCheck;");
            sb.AppendLine("    public float groundDistance = 0.2f;");
            sb.AppendLine("    public LayerMask groundMask;");
            sb.AppendLine("    ");
            sb.AppendLine("    private Rigidbody rb;");
            sb.AppendLine("    private Vector3 moveDirection;");
            sb.AppendLine("    private bool isGrounded;");
            sb.AppendLine();
            sb.AppendLine("    void Start()");
            sb.AppendLine("    {");
            sb.AppendLine("        rb = GetComponent<Rigidbody>();");
            sb.AppendLine("        if (rb == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            rb = gameObject.AddComponent<Rigidbody>();");
            sb.AppendLine("        }");
            sb.AppendLine("        rb.freezeRotation = true;");
            sb.AppendLine("        ");
            sb.AppendLine("        // 如果没有地面检测点，创建一个");
            sb.AppendLine("        if (groundCheck == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            GameObject gc = new GameObject(\"GroundCheck\");");
            sb.AppendLine("            gc.transform.SetParent(transform);");
            sb.AppendLine("            gc.transform.localPosition = new Vector3(0, -1f, 0);");
            sb.AppendLine("            groundCheck = gc.transform;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    void Update()");
            sb.AppendLine("    {");
            sb.AppendLine("        // 地面检测");
            sb.AppendLine("        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);");
            sb.AppendLine("        ");
            sb.AppendLine("        // 获取输入");
            sb.AppendLine("        float horizontal = Input.GetAxisRaw(\"Horizontal\");");
            sb.AppendLine("        float vertical = Input.GetAxisRaw(\"Vertical\");");
            sb.AppendLine("        moveDirection = new Vector3(horizontal, 0, vertical).normalized;");
            sb.AppendLine("        ");
            sb.AppendLine("        // 跳跃");
            sb.AppendLine("        if (Input.GetButtonDown(\"Jump\") && isGrounded)");
            sb.AppendLine("        {");
            sb.AppendLine("            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    void FixedUpdate()");
            sb.AppendLine("    {");
            sb.AppendLine("        if (moveDirection.magnitude > 0.1f)");
            sb.AppendLine("        {");
            sb.AppendLine("            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 生成简单的敌人 AI 脚本（追踪玩家）
        /// </summary>
        public static string GenerateEnemyController(string className = "EnemyController", float moveSpeed = 3f, float detectionRange = 10f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    [Header(\"AI 设置\")]");
            sb.AppendLine($"    public float moveSpeed = {moveSpeed}f;");
            sb.AppendLine($"    public float detectionRange = {detectionRange}f;");
            sb.AppendLine("    public string playerTag = \"Player\";");
            sb.AppendLine("    ");
            sb.AppendLine("    private Transform target;");
            sb.AppendLine("    private Rigidbody rb;");
            sb.AppendLine();
            sb.AppendLine("    void Start()");
            sb.AppendLine("    {");
            sb.AppendLine("        rb = GetComponent<Rigidbody>();");
            sb.AppendLine("        if (rb == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            rb = gameObject.AddComponent<Rigidbody>();");
            sb.AppendLine("        }");
            sb.AppendLine("        rb.freezeRotation = true;");
            sb.AppendLine("        ");
            sb.AppendLine("        // 查找玩家");
            sb.AppendLine("        GameObject player = GameObject.FindGameObjectWithTag(playerTag);");
            sb.AppendLine("        if (player != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            target = player.transform;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    void FixedUpdate()");
            sb.AppendLine("    {");
            sb.AppendLine("        if (target == null) return;");
            sb.AppendLine("        ");
            sb.AppendLine("        float distance = Vector3.Distance(transform.position, target.position);");
            sb.AppendLine("        ");
            sb.AppendLine("        // 在检测范围内追踪玩家");
            sb.AppendLine("        if (distance <= detectionRange)");
            sb.AppendLine("        {");
            sb.AppendLine("            Vector3 direction = (target.position - transform.position).normalized;");
            sb.AppendLine("            direction.y = 0; // 保持水平移动");
            sb.AppendLine("            ");
            sb.AppendLine("            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);");
            sb.AppendLine("            ");
            sb.AppendLine("            // 面向玩家");
            sb.AppendLine("            if (direction != Vector3.zero)");
            sb.AppendLine("            {");
            sb.AppendLine("                transform.rotation = Quaternion.LookRotation(direction);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("    ");
            sb.AppendLine("    // 在 Scene 视图显示检测范围");
            sb.AppendLine("    void OnDrawGizmosSelected()");
            sb.AppendLine("    {");
            sb.AppendLine("        Gizmos.color = Color.red;");
            sb.AppendLine("        Gizmos.DrawWireSphere(transform.position, detectionRange);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 生成巡逻 AI 脚本
        /// </summary>
        public static string GeneratePatrolController(string className = "PatrolController", float moveSpeed = 2f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    [Header(\"巡逻设置\")]");
            sb.AppendLine($"    public float moveSpeed = {moveSpeed}f;");
            sb.AppendLine("    public Transform[] waypoints;");
            sb.AppendLine("    public float waypointThreshold = 0.5f;");
            sb.AppendLine("    ");
            sb.AppendLine("    private int currentWaypointIndex = 0;");
            sb.AppendLine("    private Rigidbody rb;");
            sb.AppendLine();
            sb.AppendLine("    void Start()");
            sb.AppendLine("    {");
            sb.AppendLine("        rb = GetComponent<Rigidbody>();");
            sb.AppendLine("        if (rb == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            rb = gameObject.AddComponent<Rigidbody>();");
            sb.AppendLine("        }");
            sb.AppendLine("        rb.freezeRotation = true;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    void FixedUpdate()");
            sb.AppendLine("    {");
            sb.AppendLine("        if (waypoints == null || waypoints.Length == 0) return;");
            sb.AppendLine("        ");
            sb.AppendLine("        Transform targetWaypoint = waypoints[currentWaypointIndex];");
            sb.AppendLine("        if (targetWaypoint == null) return;");
            sb.AppendLine("        ");
            sb.AppendLine("        Vector3 direction = (targetWaypoint.position - transform.position).normalized;");
            sb.AppendLine("        direction.y = 0;");
            sb.AppendLine("        ");
            sb.AppendLine("        // 移动向目标点");
            sb.AppendLine("        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);");
            sb.AppendLine("        ");
            sb.AppendLine("        // 面向移动方向");
            sb.AppendLine("        if (direction != Vector3.zero)");
            sb.AppendLine("        {");
            sb.AppendLine("            transform.rotation = Quaternion.LookRotation(direction);");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        // 检查是否到达目标点");
            sb.AppendLine("        float distance = Vector3.Distance(transform.position, targetWaypoint.position);");
            sb.AppendLine("        if (distance < waypointThreshold)");
            sb.AppendLine("        {");
            sb.AppendLine("            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion

        #region UI 脚本模板

        /// <summary>
        /// 生成简单的按钮点击脚本
        /// </summary>
        public static string GenerateButtonHandler(string className = "ButtonHandler")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    private Button button;");
            sb.AppendLine();
            sb.AppendLine("    void Start()");
            sb.AppendLine("    {");
            sb.AppendLine("        button = GetComponent<Button>();");
            sb.AppendLine("        if (button != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            button.onClick.AddListener(OnButtonClick);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void OnButtonClick()");
            sb.AppendLine("    {");
            sb.AppendLine("        Debug.Log(\"Button clicked: \" + gameObject.name);");
            sb.AppendLine("        // 在这里添加按钮点击逻辑");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 生成血条/生命值 UI 脚本
        /// </summary>
        public static string GenerateHealthBar(string className = "HealthBar", float maxHealth = 100f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    [Header(\"生命值设置\")]");
            sb.AppendLine($"    public float maxHealth = {maxHealth}f;");
            sb.AppendLine("    public float currentHealth;");
            sb.AppendLine("    ");
            sb.AppendLine("    [Header(\"UI 引用\")]");
            sb.AppendLine("    public Slider healthSlider;");
            sb.AppendLine("    public Image fillImage;");
            sb.AppendLine("    ");
            sb.AppendLine("    [Header(\"颜色设置\")]");
            sb.AppendLine("    public Color healthyColor = Color.green;");
            sb.AppendLine("    public Color damagedColor = Color.yellow;");
            sb.AppendLine("    public Color criticalColor = Color.red;");
            sb.AppendLine();
            sb.AppendLine("    void Start()");
            sb.AppendLine("    {");
            sb.AppendLine("        currentHealth = maxHealth;");
            sb.AppendLine("        UpdateHealthBar();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void TakeDamage(float damage)");
            sb.AppendLine("    {");
            sb.AppendLine("        currentHealth = Mathf.Max(0, currentHealth - damage);");
            sb.AppendLine("        UpdateHealthBar();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void Heal(float amount)");
            sb.AppendLine("    {");
            sb.AppendLine("        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);");
            sb.AppendLine("        UpdateHealthBar();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    void UpdateHealthBar()");
            sb.AppendLine("    {");
            sb.AppendLine("        if (healthSlider != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            healthSlider.value = currentHealth / maxHealth;");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        // 根据生命值比例改变颜色");
            sb.AppendLine("        if (fillImage != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            float healthPercent = currentHealth / maxHealth;");
            sb.AppendLine("            if (healthPercent > 0.5f)");
            sb.AppendLine("                fillImage.color = healthyColor;");
            sb.AppendLine("            else if (healthPercent > 0.25f)");
            sb.AppendLine("                fillImage.color = damagedColor;");
            sb.AppendLine("            else");
            sb.AppendLine("                fillImage.color = criticalColor;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion

        #region 游戏管理脚本模板

        /// <summary>
        /// 生成简单的游戏管理器脚本
        /// </summary>
        public static string GenerateGameManager(string className = "GameManager")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.SceneManagement;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    public static GameManager Instance { get; private set; }");
            sb.AppendLine("    ");
            sb.AppendLine("    [Header(\"游戏状态\")]");
            sb.AppendLine("    public bool isGamePaused = false;");
            sb.AppendLine("    public int score = 0;");
            sb.AppendLine();
            sb.AppendLine("    void Awake()");
            sb.AppendLine("    {");
            sb.AppendLine("        // 单例模式");
            sb.AppendLine("        if (Instance == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            Instance = this;");
            sb.AppendLine("            DontDestroyOnLoad(gameObject);");
            sb.AppendLine("        }");
            sb.AppendLine("        else");
            sb.AppendLine("        {");
            sb.AppendLine("            Destroy(gameObject);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void PauseGame()");
            sb.AppendLine("    {");
            sb.AppendLine("        isGamePaused = true;");
            sb.AppendLine("        Time.timeScale = 0f;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void ResumeGame()");
            sb.AppendLine("    {");
            sb.AppendLine("        isGamePaused = false;");
            sb.AppendLine("        Time.timeScale = 1f;");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void AddScore(int points)");
            sb.AppendLine("    {");
            sb.AppendLine("        score += points;");
            sb.AppendLine("        Debug.Log(\"Score: \" + score);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void RestartGame()");
            sb.AppendLine("    {");
            sb.AppendLine("        Time.timeScale = 1f;");
            sb.AppendLine("        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void LoadScene(string sceneName)");
            sb.AppendLine("    {");
            sb.AppendLine("        Time.timeScale = 1f;");
            sb.AppendLine("        SceneManager.LoadScene(sceneName);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 生成简单的物体旋转脚本
        /// </summary>
        public static string GenerateRotator(string className = "Rotator", float speed = 50f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public class {className} : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine($"    public float rotationSpeed = {speed}f;");
            sb.AppendLine("    public Vector3 rotationAxis = Vector3.up;");
            sb.AppendLine();
            sb.AppendLine("    void Update()");
            sb.AppendLine("    {");
            sb.AppendLine("        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        #endregion
    }
}
