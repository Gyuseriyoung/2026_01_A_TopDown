using UnityEngine;

/// <summary>
/// 총알 이동 및 충돌 처리
/// Instantiate 후 Init()으로 초기화합니다
/// 나중에 오브젝트 풀링으로 교체할 수 있도록 비활성화 방식으로 작성
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BulletController : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 3f;    // 일정 시간 후 자동 제거

    private Rigidbody2D rb;
    private string ownerTag;  // "Player" or "Enemy" - 아군 충돌 방지용

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // 충돌체 설정 - Trigger로 사용
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    /// <summary>총알 초기화. Fire() 직후 호출</summary>
    public void Init(Vector2 direction, float speed, string shooter)
    {
        ownerTag = shooter;
        rb.linearVelocity = direction * speed;

        // 총알 방향으로 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 수명 타이머
        Invoke(nameof(DestroyBullet), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 발사자 본인과 충돌 무시
        if (other.CompareTag(ownerTag)) return;
        // 다른 총알과 충돌 무시
        if (other.CompareTag("Bullet")) return;

        // 체력 시스템이 있는 오브젝트에만 데미지
        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
            health.TakeDamage(damage);

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        CancelInvoke(); // 중복 Invoke 방지
        // TODO: 오브젝트 풀링 구현 시 Destroy → 풀에 반납으로 교체
        Destroy(gameObject);
    }

    public void SetDamage(float value) => damage = value;
}