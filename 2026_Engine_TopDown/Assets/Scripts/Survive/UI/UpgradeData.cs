using UnityEngine;

/// <summary>
/// 강화 카드 하나의 데이터를 담는 ScriptableObject
///
/// [에셋 생성]
/// Project 창 우클릭 → Create → Game/Upgrade Data
/// 강화 종류마다 파일 하나씩 만드세요 (예: Upgrade_MoveSpeed.asset)
///
/// [Inspector 설정 예시]
///   공격력 강화  : UpgradeType = AttackDamage,  multiplier = 1.2,  flatValue = 0
///   이동속도 강화: UpgradeType = MoveSpeed,      multiplier = 1.15, flatValue = 0
///   발사속도 강화: UpgradeType = FireRate,        multiplier = 0.8,  flatValue = 0  ← fireRate는 낮을수록 빠름
///   최대체력 증가: UpgradeType = MaxHp,           multiplier = 1,    flatValue = 30
///   즉시회복     : UpgradeType = HpHeal,          multiplier = 1,    flatValue = 20
/// </summary>
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("카드 표시 정보")]
    public string upgradeName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon;  // UIManager의 upgradeIcons[] 슬롯에 표시됩니다 (선택사항)

    [Header("강화 종류")]
    public UpgradeType upgradeType;

    [Header("강화 수치")]
    [Tooltip("곱연산 강화 배율. 사용하지 않으면 1로 두세요.\n" +
             "공격력·이동속도 +N% → 1.N 입력\n" +
             "발사속도 +20% 빠르게 → 0.8 입력 (간격이 줄어들어야 빨라짐)")]
    public float multiplier = 1f;

    [Tooltip("덧셈 강화 수치. 사용하지 않으면 0으로 두세요.\n" +
             "최대체력 +30 → 30 입력\n" +
             "즉시회복 20 → 20 입력")]
    public float flatValue = 0f;
}

public enum UpgradeType
{
    AttackDamage,       // 공격력  (PlayerStats.damageMultiplier에 multiplier 누적)
    MoveSpeed,          // 이동속도 (PlayerStats.moveSpeedMultiplier에 multiplier 누적)
    FireRate,           // 발사속도 (PlayerStats.fireRateMultiplier에 multiplier 누적)
    MaxHp,              // 최대 체력 (HealthSystem.SetMaxHp에 flatValue 덧셈)
    HpHeal,             // 즉시 회복 (HealthSystem.Heal에 flatValue)
    BulletPenetration,  // 관통 (확장용 — BulletController 수정 후 연결)
}