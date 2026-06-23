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
        if (mainCamera == null || bulletPrefab == null || firePoint == null) return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();

        if (bullet != null)
        {
            // ⭐️ [관통력 버그 해결 핵심]: ScriptableObject로부터 실시간 최종 스탯을 추출합니다.
            float damage = playerStats != null ? playerStats.BulletDamage : 10f;
            float speed = playerStats != null ? playerStats.baseBulletSpeed : 10f;
            int penetrationCount = playerStats != null ? playerStats.PenetrationCount : 0;

            // ⭐️ 새로 바뀐 Init 규격으로 데이터를 순서대로 한 번에 넘겨줍니다!
            bullet.Init(direction, speed, gameObject.tag, penetrationCount, damage);
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