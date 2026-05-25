using UnityEngine;

/// <summary>
/// 기본 적 AI - 플레이어를 향해 이동하며 접촉 시 데미지
/// 사망 시 경험치 드롭
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class EnemyController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("전투 설정")]
    [SerializeField] private float contactDamage = 10f;    // 플레이어와 접촉 시 데미지
    [SerializeField] private float damageInterval = 1f;   // 데미지 쿨타임 (초)

    [Header("보상 설정")]
    [SerializeField] private int expValue = 10;            // 처치 시 경험치
    [SerializeField] private GameObject expOrbPrefab;      // 경험치 구슬 프리팹

    // 내부 상태
    private Rigidbody2D rb;
    private Transform playerTransform;
    private float nextDamageTime;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // 씬에서 플레이어 탐색
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;
        ChasePlayer();
    }

    // ── AI ───────────────────────────────────────────────

    private void ChasePlayer()
    {
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 이동 방향으로 회전 (선택사항)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // ── 충돌 데미지 ───────────────────────────────────────

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (Time.time < nextDamageTime) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        HealthSystem playerHealth = collision.gameObject.GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            nextDamageTime = Time.time + damageInterval;
        }
    }

    // ── 사망 처리 ─────────────────────────────────────────

    /// <summary>HealthSystem.Die()에서 호출</summary>
    public void OnDead()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        DropExpOrb();

        // WaveManager에 사망 알림
        WaveManager.Instance?.OnEnemyDead();

        // TODO: 사망 애니메이션/이펙트 재생 후 제거로 교체
        Destroy(gameObject);
    }

    private void DropExpOrb()
    {
        if (expOrbPrefab == null) return;

        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        ExpOrb expOrb = orb.GetComponent<ExpOrb>();
        if (expOrb != null)
            expOrb.SetValue(expValue);
    }

    public void SetMoveSpeed(float value) => moveSpeed = value;
    public int GetExpValue() => expValue;
}