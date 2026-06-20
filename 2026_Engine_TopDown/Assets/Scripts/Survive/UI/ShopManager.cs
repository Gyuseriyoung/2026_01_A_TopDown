using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI totalGoldText;

    [Header("공격력 강화 항목")]
    [SerializeField] private TextMeshProUGUI damageLevelText;
    [SerializeField] private TextMeshProUGUI damageCostText;
    [SerializeField] private Button damageBuyButton;

    [Header("이동속도 강화 항목")]
    [SerializeField] private TextMeshProUGUI moveSpeedLevelText;
    [SerializeField] private TextMeshProUGUI moveSpeedCostText;
    [SerializeField] private Button moveSpeedBuyButton;

    [Header("⭐️ 발사속도 강화 항목")]
    [SerializeField] private TextMeshProUGUI fireRateLevelText;
    [SerializeField] private TextMeshProUGUI fireRateCostText;
    [SerializeField] private Button fireRateBuyButton;

    [Header("⭐️ 관통력 강화 항목")]
    [SerializeField] private TextMeshProUGUI penetrationLevelText;
    [SerializeField] private TextMeshProUGUI penetrationCostText;
    [SerializeField] private Button penetrationBuyButton;

    [Header("가격 및 레벨 설정")]
    [SerializeField] private int baseUpgradeCost = 100;
    [SerializeField] private int costIncreasePerLevel = 50;
    [SerializeField] private int maxLevel = 5;

    private int damageLevel;
    private int moveSpeedLevel;
    private int fireRateLevel;
    private int penetrationLevel;

    private void OnEnable()
    {
        UpdateShopUI();
    }

    public void UpdateShopUI()
    {
        int totalGold = PlayerPrefs.GetInt("TotalGold", 0);
        damageLevel = PlayerPrefs.GetInt("Shop_Upgrade_Damage", 0);
        moveSpeedLevel = PlayerPrefs.GetInt("Shop_Upgrade_MoveSpeed", 0);
        fireRateLevel = PlayerPrefs.GetInt("Shop_Upgrade_FireRate", 0);         // ⭐️ 로드
        penetrationLevel = PlayerPrefs.GetInt("Shop_Upgrade_Penetration", 0);   // ⭐️ 로드

        totalGoldText.text = $"보유 골드: {totalGold} G";

        // 1. 공격력 UI 세팅
        UpdateItemUI(damageLevel, damageLevelText, damageCostText, damageBuyButton, totalGold);

        // 2. 이동속도 UI 세팅
        UpdateItemUI(moveSpeedLevel, moveSpeedLevelText, moveSpeedCostText, moveSpeedBuyButton, totalGold);

        // 3. 발사속도 UI 세팅
        UpdateItemUI(fireRateLevel, fireRateLevelText, fireRateCostText, fireRateBuyButton, totalGold);

        // 4. 관통력 UI 세팅
        UpdateItemUI(penetrationLevel, penetrationLevelText, penetrationCostText, penetrationBuyButton, totalGold);
    }

    // UI 코드가 중복되므로 깔끔하게 묶어주는 유틸 함수입니다.
    private void UpdateItemUI(int currentLevel, TextMeshProUGUI lvlText, TextMeshProUGUI costText, Button buyButton, int totalGold)
    {
        lvlText.text = $"LV. {currentLevel} / {maxLevel}";
        int currentCost = baseUpgradeCost + (currentLevel * costIncreasePerLevel);

        if (currentLevel >= maxLevel)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            costText.text = $"{currentCost} G";
            buyButton.interactable = totalGold >= currentCost;
        }
    }

    // ── 구매 버튼 연동 함수들 ───────────────────────────

    public void BuyDamageUpgrade() => HandlePurchase("Shop_Upgrade_Damage", ref damageLevel);
    public void BuyMoveSpeedUpgrade() => HandlePurchase("Shop_Upgrade_MoveSpeed", ref moveSpeedLevel);
    public void BuyFireRateUpgrade() => HandlePurchase("Shop_Upgrade_FireRate", ref fireRateLevel);       // ⭐️ 버튼 연동용
    public void BuyPenetrationUpgrade() => HandlePurchase("Shop_Upgrade_Penetration", ref penetrationLevel); // ⭐️ 버튼 연동용

    // 구매 처리를 통합 관리하는 함수
    private void HandlePurchase(string saveKey, ref int currentLevel)
    {
        if (currentLevel >= maxLevel) return;

        int cost = baseUpgradeCost + (currentLevel * costIncreasePerLevel);

        if (GameManager.Instance != null && GameManager.Instance.SpendGold(cost))
        {
            currentLevel++;
            PlayerPrefs.SetInt(saveKey, currentLevel);
            PlayerPrefs.Save();
            UpdateShopUI();
        }
    }
}