using UnityEngine;

/// <summary>
/// 플레이어 이동·발사 담당
/// 스탯은 [SerializeField] playerStats(PlayerStatsSO)에서 읽습니다.
///
/// [Inspector 설정]
/// playerStats 슬롯에 PlayerStats 에셋을 드래그하세요.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("스탯 (ScriptableObject)")]
    [SerializeField] private PlayerStatsSO playerStats;   // ← SO 슬롯

    [Header("발사 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 moveInput;
    private float nextFireTime;
    private bool isDead;

    public Vector2 MoveInput => moveInput;

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // SO는 에셋이라 이전 Play Mode 값이 남아있을 수 있으므로 반드시 초기화
        playerStats?.ResetRuntimeStats();
    }

    // ── 매 프레임 ─────────────────────────────────────────

    private void Update()
    {
        if (isDead) return;
        HandleMovementInput();
        HandleAutoFire();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        // SO에서 MoveSpeed를 직접 읽음 → 강화 적용 즉시 반영
        rb.linearVelocity = moveInput * (playerStats != null ? playerStats.MoveSpeed : 5f);
    }

    // ── 이동 ──────────────────────────────────────────────

    private void HandleMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }

    // ── 발사 ──────────────────────────────────────────────

    private void HandleAutoFire()
    {
        if (Time.time < nextFireTime) return;
        if (bulletPrefab == null || firePoint == null) return;

        // SO에서 FireRate를 직접 읽음 → 강화 적용 즉시 반영
        float fireRate = playerStats != null ? playerStats.FireRate : 0.3f;
        nextFireTime = Time.time + fireRate;

        Fire();
    }

    private void Fire()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();

        if (bullet != null)
        {
            float damage = playerStats != null ? playerStats.BulletDamage : 10f;
            float speed = playerStats != null ? playerStats.BulletSpeed : 10f;

            // ⭐️ 정상 컴파일된 SO에서 최종 관통력을 가져옵니다.
            int penetrationCount = playerStats != null ? playerStats.PenetrationCount : 0;

            bullet.SetDamage(damage);
            bullet.Init(direction, speed, gameObject.tag,penetrationCount);

            
        }
    }

    // ── 사망 ──────────────────────────────────────────────

    public void OnDead()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance?.OnPlayerDead();
    }

    // ── SO 외부 접근 (UIManager에서 강화 적용 시 사용) ───

    public PlayerStatsSO Stats => playerStats;

    public bool IsDead => isDead;
}