using UnityEngine;

/// <summary>
/// 플레이어 스탯 ScriptableObject
///
/// [에셋 생성]
/// Project 창 우클릭 → Create → Game/Player Stats
/// 생성한 에셋을 PlayerController와 UIManager의 playerStats 슬롯에 드래그하세요
///
/// [동작 방식]
/// - baseStat : Inspector에서 설정한 기본값 (에셋에 영구 저장)
/// - 런타임 배율/추가값은 게임 시작 시 초기화되고, 강화 카드 적용마다 누적됩니다
/// - SO는 에셋이므로 Play Mode가 끝나도 값이 남습니다 → OnEnable에서 반드시 초기화
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("기본 스탯 (에셋에 저장 — 게임 시작 기준값)")]
    public float baseDamage = 10f;
    public float baseMoveSpeed = 5f;
    public float baseFireRate = 0.3f;   // 낮을수록 빠름
    public float baseBulletSpeed = 10f;

    // ── 런타임 누적값 (Play Mode 시작마다 초기화) ────────
    // SO는 에셋이라 씬 재시작해도 값이 유지되므로 명시적 초기화 필수
    [System.NonSerialized] private float damageMultiplier = 1f;
    [System.NonSerialized] private float moveSpeedMultiplier = 1f;
    [System.NonSerialized] private float fireRateMultiplier = 1f;

    // ── 최종 스탯 프로퍼티 ───────────────────────────────
    public float BulletDamage => baseDamage * damageMultiplier;
    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;
    public float FireRate => baseFireRate * fireRateMultiplier;
    public float BulletSpeed => baseBulletSpeed;

    /// <summary>
    /// Play Mode 시작마다 런타임 배율을 기본값으로 되돌립니다.
    /// PlayerController.Awake()에서 호출하세요.
    /// </summary>
    public void ResetRuntimeStats()
    {
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
    }

    // ── 강화 적용 ────────────────────────────────────────
    public void ApplyDamageMultiplier(float multiplier) => damageMultiplier *= multiplier;
    public void ApplyMoveSpeedMultiplier(float multiplier) => moveSpeedMultiplier *= multiplier;
    public void ApplyFireRateMultiplier(float multiplier) => fireRateMultiplier *= multiplier;
}