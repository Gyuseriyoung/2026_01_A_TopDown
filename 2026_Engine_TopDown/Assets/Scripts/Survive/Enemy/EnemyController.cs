using UnityEngine;

/// <summary>
/// 기본 적 AI — 플레이어를 향해 이동하며 접촉 시 데미지
/// 사망 시 경험치 드랍
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

    [Header("골드 보상 설정")]
    [SerializeField] private GameObject goldCoinPrefab;
    [Range(0f, 1f)][SerializeField] private float goldDropChance = 0.3f;
    [SerializeField] private int goldValue = 5;


    private Rigidbody2D rb;
    private Transform playerTransform;
    private float nextDamageTime;
    private bool isDead;
    private float expMultiplier = 1f;   // WaveManager가 웨이브 배율로 주입

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    // ── AI ────────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (isDead || playerTransform == null) return;

        Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    // ── 접촉 데미지 ───────────────────────────────────────

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

    // ── 사망 ──────────────────────────────────────────────

    /// <summary>HealthSystem.Die()에서 호출됩니다</summary>
    public void OnDead()
    {
        if (isDead) return; // 중복 사망 방지용 안전장치
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (TryGetComponent<Collider2D>(out var col)) col.enabled = false; // 이전 관통 버그 수정용 코드

        DropExpOrb();
        DropGoldCoin(); // ⭐️ 골드 드랍 함수 호출

        WaveManager.Instance?.OnEnemyDead();
        Destroy(gameObject, 0.05f);
    }

    private void DropGoldCoin()
    {
        if (goldCoinPrefab == null) return;

        // 랜덤 확률 체크 (0.0 ~ 1.0)
        if (Random.value <= goldDropChance)
        {
            GameObject coinObj = Instantiate(goldCoinPrefab, transform.position, Quaternion.identity);
            GoldCoin coin = coinObj.GetComponent<GoldCoin>();
            if (coin != null) coin.SetValue(goldValue);
        }
    }

    private void DropExpOrb()
    {
        if (expOrbPrefab == null) return;

        GameObject orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        ExpOrb expOrb = orb.GetComponent<ExpOrb>();
        if (expOrb != null)
            expOrb.SetValue(Mathf.RoundToInt(expValue * expMultiplier));
    }

    // ── Getter / Setter (WaveManager 배율 적용용) ─────────

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float value) => moveSpeed = value;

    public int GetExpValue() => expValue;
    public void SetExpMultiplier(float multiplier) => expMultiplier = multiplier;
}