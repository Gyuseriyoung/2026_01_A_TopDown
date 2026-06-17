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
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageInterval = 1f;

    [Header("보상 설정")]
    [SerializeField] private int expValue = 10;
    [SerializeField] private GameObject expOrbPrefab;

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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

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

    public void OnDead()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        DropExpOrb();

        // WaveManager.OnEnemyDead() 안에서 GameManager.OnEnemyKilled()도 호출됩니다
        WaveManager.Instance?.OnEnemyDead();

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

    // ── Getter / Setter (WaveManager 배율 적용용) ─────────

    public float GetMoveSpeed() => moveSpeed;         // WaveManager에서 배율 적용 시 사용
    public void SetMoveSpeed(float value) => moveSpeed = value;
    public int GetExpValue() => expValue;
}