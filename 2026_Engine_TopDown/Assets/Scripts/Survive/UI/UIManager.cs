using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 내 모든 UI 관리
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("결과 창 UI 요소들")]
    [SerializeField] private TextMeshProUGUI sessionTimeText;
    [SerializeField] private TextMeshProUGUI sessionKillsText;

    [Header("웨이브 알림")]
    [SerializeField] private TextMeshProUGUI waveMessageText;

    [Header("레벨업 카드 UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescTexts;
    [SerializeField] private Image[] upgradeIconImages;

    [Header("전체 카드 풀 (ScriptableObject들)")]
    [SerializeField] private List<UpgradeData> upgradePool = new List<UpgradeData>();

    [Header("참조")]
    [SerializeField] private PlayerStatsSO playerStats;

    [Header("게임 오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverKillText;
    [SerializeField] private TextMeshProUGUI gameOverBestKillText;
    [SerializeField] private TextMeshProUGUI gameOverBestTimeText;

    [Header("게임 클리어 UI")]
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject ShopPanel;

    // 현재 화면에 표시된 랜덤 카드들의 목록을 기억하는 바구니
    private List<UpgradeData> currentDisplayedUpgrades = new List<UpgradeData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetActive(upgradePanel, false);
        SetActive(gameOverPanel, false);
        SetActive(stageClearPanel, false);
        SetActive(pausePanel, false);
        SetActive(ShopPanel, false);
    }

    public void UpdateHP(float current, float max) { if (hpBar != null) hpBar.value = current / max; }
    public void UpdateEXP(float current, float max) { if (expBar != null) expBar.value = current / max; }
    public void UpdateEXPFromInts(int currentExp, int maxExp ,int currentLevel)
    {
        if (expBar != null && maxExp > 0)
        {
            // 정수형 데이터를 나눗셈 처리를 위해 float로 형변환하여 슬라이더 바에 반영
            expBar.value = (float)currentExp / maxExp;
        }

        if (levelText != null)
        {
            // 혹시 모르니 레벨 텍스트도 여기서 실시간으로 안전하게 동기화해 줍니다.
            levelText.text = $"LV.{currentLevel}";
        }
    }
    public void UpdateLevel(int level) { if (levelText != null) levelText.text = $"LV.{level}"; }
    public void UpdateKillCount(int count) { if (killCountText != null) killCountText.text = $"{count}"; }
    public void UpdateGoldText(int gold) { if (goldText != null) goldText.text = $"{gold}"; }

    public void UpdateTimer(float totalSeconds)
    {
        if (timerText != null) timerText.text = FormatTime(totalSeconds);
    }

    private string FormatTime(float totalSeconds)
    {
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    public void ShowWaveMessage(string message)
    {
        if (waveMessageText != null)
        {
            waveMessageText.text = message;
            StopAllCoroutines();
            StartCoroutine(WaveMessageRoutine());
        }
    }

    private IEnumerator WaveMessageRoutine()
    {
        waveMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        waveMessageText.gameObject.SetActive(false);
    }

    // ── 🎲 레벨업 카드 시스템 (완벽 랜덤 셔플 적용) ───────────────────────
    public void ShowUpgradeSelection()
    {
        if (upgradePool == null || upgradePool.Count == 0) return;

        Time.timeScale = 0f; // 게임 일시정지
        SetActive(upgradePanel, true);

        // 이전 리스트 비우기
        currentDisplayedUpgrades.Clear();

        // 1. 원본 풀이 망가지지 않도록 복사본 풀을 생성합니다.
        List<UpgradeData> tempPool = new List<UpgradeData>(upgradePool);

        // UI 버튼 개수와 현재 카드 풀 크기 중 작은 값을 선택 (보통 3개)
        int countToDisplay = Mathf.Min(upgradeButtons.Length, tempPool.Count);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            if (i < countToDisplay)
            {
                // 2. 남은 카드 풀에서 무작위로 인덱스를 하나 뽑습니다. (정밀한 랜덤 함수 활용)
                int randIndex = Random.Range(0, tempPool.Count);
                UpgradeData selectedUpgrade = tempPool[randIndex];

                // 3. 뽑은 카드를 현재 화면 표시 바구니에 넣고, 복사본 풀에서는 지워버립니다. (★중복 방지 핵심)
                currentDisplayedUpgrades.Add(selectedUpgrade);
                tempPool.RemoveAt(randIndex);

                // 4. UI 컴포넌트에 텍스트 및 데이터 대입
                if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
                    upgradeNameTexts[i].text = selectedUpgrade.upgradeName;

                if (upgradeDescTexts != null && i < upgradeDescTexts.Length && upgradeDescTexts[i] != null)
                    upgradeDescTexts[i].text = selectedUpgrade.description;

                if (upgradeIconImages != null && i < upgradeIconImages.Length && upgradeIconImages[i] != null)
                    upgradeIconImages[i].sprite = selectedUpgrade.icon;

                upgradeButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // 표시할 카드가 모자라면 버튼을 비활성화합니다.
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnUpgradeCardClicked(int index)
    {
        // ⭐️ 예외 처리 체크
        if (currentDisplayedUpgrades == null || index >= currentDisplayedUpgrades.Count) return;

        // 1. ⭐️ [버그 해결 핵심] 카드가 클릭되자마자 시간 스케일부터 먼저 1f로 복구합니다!
        // 이래야 유니티의 물리, 타이밍, UI 액션이 정상 루프로 돌아와 렉 걸리지 않고 다음 코드를 실행합니다.
        Time.timeScale = 1f;

        // 2. 능력치 강화 적용
        UpgradeData chosen = currentDisplayedUpgrades[index];
        ApplyUpgrade(chosen);

        // 3. 레벨업 선택창 패널 닫기
        SetActive(upgradePanel, false);

        Debug.Log($"[클릭 액션 완료] 인덱스 {index}번 카드 정상 처리됨");
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        if (playerStats == null || data == null) return;

        switch (data.upgradeType)
        {
            case UpgradeType.AttackDamage:
                playerStats.AddRuntimeDamage(data.multiplier - 1f);
                break;

            case UpgradeType.MoveSpeed:
                playerStats.AddRuntimeMoveSpeed(data.multiplier - 1f);
                break;

            case UpgradeType.FireRate:
                playerStats.AddRuntimeFireRate(data.multiplier);
                break;

            case UpgradeType.MaxHp:
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    HealthSystem playerHealth = playerObj.GetComponent<HealthSystem>();
                    playerHealth?.Heal(data.flatValue);
                }
                break;

            case UpgradeType.HpHeal:
                GameObject targetPlayer = GameObject.FindGameObjectWithTag("Player");
                if (targetPlayer != null)
                {
                    HealthSystem pHealth = targetPlayer.GetComponent<HealthSystem>();
                    pHealth?.Heal(data.flatValue);
                }
                break;

            case UpgradeType.BulletPenetration: // 혹은 프로젝트에 정의된 관통 카드 타입 이름
                                          // 인게임 누적 관통력 증가 카드 효과 주입
                playerStats.AddRuntimePenetration((int)data.flatValue);
                break;
        }

        Debug.Log($"[인게임 강화 성공] 적용된 카드: {data.upgradeName}");
    }

    // ── 패널 제어 유틸리티 ───────────────────────────────────
    private void SetActive(GameObject panel, bool state)
    {
        if (panel != null) panel.SetActive(state);
    }

    public void ShowGameOver(int finalKills, int finalTimeSeconds)
    {
        if (sessionTimeText != null) sessionTimeText.text = $"생존시간 : {FormatTime(finalTimeSeconds)}";
        if (sessionKillsText != null) sessionKillsText.text = $"처치한 적: {finalKills} 마리";

        SetActive(gameOverPanel, true);

        if (gameOverKillText != null) gameOverKillText.text = $"이번 판 처치: {finalKills}";
        if (gameOverBestKillText != null) gameOverBestKillText.text = $"최고 처치: {GameManager.Instance?.BestKill}";
        if (gameOverBestTimeText != null) gameOverBestTimeText.text = $"최고 기록: {FormatTime(GameManager.Instance?.BestTime ?? 0)}";
    }

    public void ShowStageClear(int finalKills, int finalTimeSeconds)
    {
        if (sessionTimeText != null) sessionTimeText.text = $"생존시간 : {FormatTime(finalTimeSeconds)}";
        if (sessionKillsText != null) sessionKillsText.text = $"처치한 적: {finalKills} 마리";

        SetActive(stageClearPanel, true);
    }

    public void ShowPauseMenu() => SetActive(pausePanel, true);
    public void HidePauseMenu() => SetActive(pausePanel, false);
    public void OnRestartButton() => GameManager.Instance?.RestartGame();
    public void OnMainMenuButton() => GameManager.Instance?.GoToMainMenu();
    public void ShowShopMenu() => SetActive(ShopPanel, true);
    public void HideShopMenu() => SetActive(ShopPanel, false);
}