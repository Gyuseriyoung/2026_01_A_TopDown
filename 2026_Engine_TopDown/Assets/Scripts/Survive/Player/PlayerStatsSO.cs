using UnityEngine;

/// <summary>
/// 플레이어 스탯 ScriptableObject
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

    // ── 런타임 누적값 (인게임 카드 버프 — Play Mode 시작마다 초기화) ────────
    [System.NonSerialized] private float damageMultiplier = 1f;
    [System.NonSerialized] private float moveSpeedMultiplier = 1f;
    [System.NonSerialized] private float fireRateMultiplier = 1f;
    [System.NonSerialized] private int penetrationBonus = 0; // 인게임 카드 전용

    // ── 상점 영구 업그레이드 보너스 (JSON에서 로드됨) ───────────────────
    private float permanentDamageBonus = 0f;
    private float permanentMoveSpeedBonus = 0f;
    private float permanentFireRateReduction = 0f;
    private int permanentPenetrationBonus = 0; // 상점 영구 관통 보너스

    // ── 외부 출력 프로퍼티 (최종 연산 스탯) ──────────────────────────
    public float BulletDamage => (baseDamage + permanentDamageBonus) * damageMultiplier;
    public float MoveSpeed => (baseMoveSpeed + permanentMoveSpeedBonus) * moveSpeedMultiplier;

    // 발사 속도 간격 (최소 0.1초 제한으로 버그 방지)
    public float FireRate => Mathf.Max(0.1f, (baseFireRate - permanentFireRateReduction) * fireRateMultiplier);

    // ⭐️ [버그 해결 핵심] 기본값 + 상점(JSON) 보너스 + 인게임 카드 버프를 모두 더한 최종 관통력 반환!
    public int PenetrationCount => basePenetrationCount + permanentPenetrationBonus + penetrationBonus;

    /// <summary>
    /// 게임 시작(Awake)이나 재시작할 때 반드시 호출하세요.
    /// </summary>
    public void ResetRuntimeStats()
    {
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
        penetrationBonus = 0;

        // 게임 시작 시 JSON 세이브 파일로부터 상점 능력치 로드
        LoadPermanentUpgradesFromTxt();
    }

    /// <summary> ⭐️ JSON 데이터 구조체에서 영구 상점 업그레이드 수치 반영 </summary>
    private void LoadPermanentUpgradesFromTxt()
    {
        if (GameManager.Instance != null && GameManager.Instance.shopProgress != null)
        {
            var progress = GameManager.Instance.shopProgress;

            // 1레벨당 공격력 +2
            permanentDamageBonus = progress.dmgLevel * 2f;

            // 1레벨당 이동 속도 +0.5
            permanentMoveSpeedBonus = progress.speedLevel * 0.5f;

            // 1레벨당 발사 간격 -0.03초 (낮아질수록 빨라짐)
            permanentFireRateReduction = progress.fireRateLevel * 0.03f;

            // ⭐️ [중요] 상점 JSON 데이터 세이브 파일의 관통 레벨을 그대로 보너스로 주입!
            permanentPenetrationBonus = progress.penetrationLevel;

            Debug.Log($"[SO 스탯 로드 완료] JSON 데이터 연동 성공. 영구 관통 보너스: +{permanentPenetrationBonus}회");
        }
    }

    // ── 인게임 레벨업 카드 강화 적용 함수들 ──────────────────────────────────
    public void AddRuntimeDamage(float percent) => damageMultiplier += percent;
    public void AddRuntimeMoveSpeed(float percent) => moveSpeedMultiplier += percent;
    public void AddRuntimeFireRate(float factor) => fireRateMultiplier *= factor;

    // 인게임 레벨업 카드로 관통력을 올렸을 때 작동하는 함수
    public void AddRuntimePenetration(int amount) => penetrationBonus += amount;
}