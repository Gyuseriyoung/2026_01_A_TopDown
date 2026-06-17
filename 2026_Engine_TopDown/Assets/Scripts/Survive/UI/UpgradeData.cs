using UnityEngine;


[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("카드 표시 정보")]
    public string upgradeName;

    [TextArea(2, 4)]
    public string description;

    public Sprite icon; // 카드 아이콘 (선택사항)

    [Header("강화 종류")]
    public UpgradeType upgradeType;

    [Header("강화 수치")]
    [Tooltip("곱연산 강화 (이동속도 +15% → 1.15 입력). 덧셈 강화는 0으로 두세요")]
    public float multiplier = 1f;

    [Tooltip("덧셈 강화 (최대체력 +30 → 30 입력). 곱연산 강화는 0으로 두세요")]
    public float flatValue = 0f;
}

public enum UpgradeType
{
    AttackDamage,       // 공격력
    MoveSpeed,          // 이동속도
    FireRate,           // 발사속도
    MaxHp,              // 최대 체력
    HpHeal,             // 즉시 회복
    BulletPenetration,  // 관통 (미구현 시 확장용)
}