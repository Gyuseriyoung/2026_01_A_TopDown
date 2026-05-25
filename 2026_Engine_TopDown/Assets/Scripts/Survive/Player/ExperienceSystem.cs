using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 경험치 및 레벨업 시스템
/// 레벨업 시 UIManager를 통해 강화 카드 UI를 띄움
/// </summary>
public class ExperienceSystem : MonoBehaviour
{
    [Header("레벨업 설정")]
    [SerializeField] private int baseExpRequired = 50;     // 1레벨 요구 경험치
    [SerializeField] private float expGrowthRate = 1.4f;  // 레벨당 증가 배율

    public UnityEvent<int, int, int> OnExpChanged;        // (현재EXP, 필요EXP, 레벨)
    public UnityEvent<int> OnLevelUp;                     // (새 레벨)

    private int currentLevel = 1;
    private int currentExp;
    private int expToNextLevel;

    public int Level => currentLevel;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => expToNextLevel;

    private void Start()
    {
        expToNextLevel = baseExpRequired;
        OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);

        // 연속 레벨업 처리 (경험치가 많이 쌓인 경우 대비)
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel++;
        expToNextLevel = Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expGrowthRate, currentLevel - 1));

        OnLevelUp?.Invoke(currentLevel);

        // 게임 일시 정지 후 강화 UI 표시
        Time.timeScale = 0f;
        UIManager.Instance?.ShowUpgradeCards();
    }

    /// <summary>강화 선택 완료 후 UIManager에서 호출</summary>
    public void OnUpgradeSelected()
    {
        Time.timeScale = 1f;
        OnExpChanged?.Invoke(currentExp, expToNextLevel, currentLevel);
    }
}