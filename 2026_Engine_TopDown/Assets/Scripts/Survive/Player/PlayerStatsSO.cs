using UnityEngine;

/// <summary>
/// 상점 업그레이드 레벨들을 JSON으로 한 번에 묶기 위한 세이브 데이터 클래스
/// </summary>
[System.Serializable]
public class ShopSaveData
{
    public int dmgLevel = 0;
    public int speedLevel = 0;
    public int fireRateLevel = 0;
    public int penetrationLevel = 0;
}

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Game/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("기본 스탯 (에셋에 저장 — 게임 시작 기준값)")]
    public float baseDamage = 10f;
    public float baseMoveSpeed = 5f;
    public float baseFireRate = 0.5f;
    public float baseBulletSpeed = 10f;
    public int basePenetrationCount = 0;

    // ── 런타임 변수들 (기존 구조 유지) ──
    [System.NonSerialized] private float damageMultiplier = 1f;
    [System.NonSerialized] private float moveSpeedMultiplier = 1f;
    [System.NonSerialized] private float fireRateMultiplier = 1f;
    [System.NonSerialized] private int penetrationBonus = 0;

    [System.NonSerialized] private float permanentDamageBonus;
    [System.NonSerialized] private float permanentMoveSpeedBonus;
    [System.NonSerialized] private float permanentFireRateReduction;
    [System.NonSerialized] private int permanentPenetrationBonus;

    // 프로퍼티들 기존 구조 그대로 유지...
    public float BulletDamage => (baseDamage + permanentDamageBonus) * damageMultiplier;
    public float MoveSpeed => (baseMoveSpeed + permanentMoveSpeedBonus) * moveSpeedMultiplier;
    public float FireRate => Mathf.Max(0.05f, (baseFireRate - permanentFireRateReduction) * fireRateMultiplier);
    public int PenetrationCount => basePenetrationCount + permanentPenetrationBonus + penetrationBonus;

    public void ResetRuntimeStats()
    {
        damageMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        fireRateMultiplier = 1f;
        penetrationBonus = 0;

        // ⭐️ 중요: 이제 PlayerPrefs에서 개별로 읽지 않고, GameManager가 JSON으로 로드해 둔 데이터를 넘겨받아 적용합니다.
        LoadPermanentUpgradesFromJSON();
    }

    private void LoadPermanentUpgradesFromJSON()
    {
        // ⭐️ GameManager의 JSON 세이브 데이터 바구니에서 값을 가져옵니다.
        if (GameManager.Instance != null)
        {
            ShopSaveData saveData = GameManager.Instance.shopProgress;

            permanentDamageBonus = saveData.dmgLevel * 2f;
            permanentMoveSpeedBonus = saveData.speedLevel * 0.5f;
            permanentFireRateReduction = saveData.fireRateLevel * 0.03f;
            permanentPenetrationBonus = saveData.penetrationLevel;
        }
    }

    public void AddRuntimeDamage(float percent) => damageMultiplier += percent;
    public void AddRuntimeMoveSpeed(float percent) => moveSpeedMultiplier += percent;
    public void AddRuntimeFireRate(float percent) => fireRateMultiplier *= percent;
    public void AddRuntimePenetration(int amount) => penetrationBonus += amount;
}