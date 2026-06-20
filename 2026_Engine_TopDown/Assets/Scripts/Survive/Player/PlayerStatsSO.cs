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
    public float baseFireRate = 0.5f;   // 낮을수록 빠름
    public float baseBulletSpeed = 10f;
    public int basePenetrationCount = 0; 

    // ── 런타임 누적값 (Play Mode 시작마다 초기화) ────────
    // SO는 에셋이라 씬 재시작해도 값이 유지되므로 명시적 초기화 필수
    [System.NonSerialized] private float damageMultiplier = 1f;
    [System.NonSerialized] private float moveSpeedMultiplier = 1f;
    [System.NonSerialized] private float fireRateMultiplier = 1f;
    [System.NonSerialized] private int penetrationBonus = 0;

    // ── 영구 업그레이드 추가 보너스값 (상점 레벨에 의해 결정됨) ──
    private float permanentDamageBonus = 0f;
    private float permanentMoveSpeedBonus = 0f;
    private float permanentFireRateReduction = 0f; // 발사 간격 감소량
    private int permanentPenetrationBonus = 0;     // 관통력 보너스
    // ── 최종 스탯 프로퍼티 ───────────────────────────────
    public float BulletDamage => (baseDamage + permanentDamageBonus) * damageMultiplier;
    public float MoveSpeed => (baseMoveSpeed + permanentMoveSpeedBonus) * moveSpeedMultiplier;
    public float FireRate => Mathf.Max(0.05f, (baseFireRate - permanentFireRateReduction) * fireRateMultiplier); 
    public float BulletSpeed => baseBulletSpeed;
    public int PenetrationCount => basePenetrationCount + permanentPenetrationBonus + penetrationBonus;
    /// <summary>
    /// Play Mode 시작마다 런타임 배율을 기본값으로 되돌립니다.
    /// PlayerController.Awake()에서 호출하세요.
    /// </summary>
    public void ResetRuntimeStats()
    {
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
        penetrationBonus = 0;

        LoadPermanentUpgrades();

    }


    private void LoadPermanentUpgrades()
    {
        int dmgLevel = PlayerPrefs.GetInt("Shop_Upgrade_Damage", 0);
        int speedLevel = PlayerPrefs.GetInt("Shop_Upgrade_MoveSpeed", 0);
        int fireRateLevel = PlayerPrefs.GetInt("Shop_Upgrade_FireRate", 0);   
        int penetrationLevel = PlayerPrefs.GetInt("Shop_Upgrade_Penetration", 0); 

        // 1레벨당 공격력 +2
        permanentDamageBonus = dmgLevel * 2f;

        // 1레벨당 이동 속도 +0.5
        permanentMoveSpeedBonus = speedLevel * 0.5f;

        // ⭐️ 1레벨당 발사 간격 -0.03초 (공속 증가)
        permanentFireRateReduction = fireRateLevel * 0.03f;

        // ⭐️ 1레벨당 관통 횟수 +1회
        permanentPenetrationBonus = penetrationLevel;
    }

    public void AddRuntimePenetration(int amount)
    {
        penetrationBonus += amount;
    }
    // ── 강화 적용 ────────────────────────────────────────
    public void ApplyDamageMultiplier(float multiplier) => damageMultiplier *= multiplier;
    public void ApplyMoveSpeedMultiplier(float multiplier) => moveSpeedMultiplier *= multiplier;
    public void ApplyFireRateMultiplier(float multiplier) => fireRateMultiplier *= multiplier;
    public void ApplyPenetrationBonus(int value) => penetrationBonus += value;

 
}