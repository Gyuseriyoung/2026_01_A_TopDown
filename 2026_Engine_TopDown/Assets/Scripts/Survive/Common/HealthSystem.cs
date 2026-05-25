using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어/적 공통으로 사용하는 체력 시스템
/// PlayerController, EnemyController 모두 이 컴포넌트를 가집니다
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHp = 100f;

    // 외부에서 구독 가능한 이벤트
    public UnityEvent<float, float> OnHpChanged;  // (현재HP, 최대HP)
    public UnityEvent OnDeath;

    private float currentHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => maxHp;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHp = maxHp;
    }

    /// <summary>데미지를 받습니다. amount는 양수값으로 전달</summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0)
            Die();
    }

    /// <summary>체력을 회복합니다</summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        // 플레이어면 PlayerController.OnDead() 호출
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            player.OnDead();
            return;
        }

        // 적이면 EnemyController.OnDead() 호출
        EnemyController enemy = GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.OnDead();
            return;
        }
    }

    /// <summary>최대 체력을 변경하고 현재 체력도 비율에 맞게 조정</summary>
    public void SetMaxHp(float newMax, bool healToFull = false)
    {
        float ratio = currentHp / maxHp;
        maxHp = newMax;
        currentHp = healToFull ? maxHp : maxHp * ratio;
        OnHpChanged?.Invoke(currentHp, maxHp);
    }
}