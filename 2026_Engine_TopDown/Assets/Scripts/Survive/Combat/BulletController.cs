using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 총알 이동 및 관통 충돌 처리 (중복 충돌 및 먹통 방지 완벽 보정판)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class BulletController : MonoBehaviour
{
    [Header("총알 설정")]
    private float damage = 10f;
    [SerializeField] private float lifetime = 3f;

    private int remainingPenetration;
    private Rigidbody2D rb;
    private string ownerTag;

    // ⭐️ [중요] 한 명의 적을 지나갈 때 매 프레임 다중 충돌하여 관통 횟수가 순간 삭제되는 것을 방지하는 바구니
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // 💡 트리거 체크 및 강제 충돌 레이어 무시 설정을 코드단에서 리셋
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    public void Init(Vector2 direction, float speed, string shooter, int maxPenetration, float finalDamage)
    {
        ownerTag = shooter;
        rb.linearVelocity = direction * speed;

        damage = finalDamage;
        remainingPenetration = maxPenetration;

        // 리스트 초기화
        hitTargets.Clear();

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Invoke(nameof(DestroyBullet), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(ownerTag)) return;
        if (other.CompareTag("Bullet")) return;

        // 이미 이번 발사에서 한 번 대미지를 준 적이라면 관통 카운트를 또 깎지 않고 통과시킵니다.
        if (hitTargets.Contains(other)) return;

        HealthSystem health = other.GetComponent<HealthSystem>();
        if (health != null)
        {
            // 이 대상을 타격 목록에 기록 (중복 처리 방지)
            hitTargets.Add(other);

            // ⭐️ 디버그 로그 로그를 통해 인스펙터 콘솔 창에서 데이터가 정확히 찍히는지 검증합니다.
            Debug.Log($"[총알 충돌 감지] 대상: {other.name} | 남은 관통 횟수: {remainingPenetration}");

            if (remainingPenetration > 0)
            {
                // 관통 횟수가 있으면 1 차감하고 총알을 소멸시키지 않고 그대로 유지!
                remainingPenetration--;
                health.TakeDamage(damage);
            }
            else
            {
                // 관통 횟수가 0이면 마지막 대미지를 주며 완전 소멸
                health.TakeDamage(damage);
                DestroyBullet();
            }
        }
    }

    private void DestroyBullet()
    {
        CancelInvoke(nameof(DestroyBullet));
        Destroy(gameObject);
    }
}