using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 내 모든 UI 관리
///
/// [Inspector 설정]
/// upgradePool : Project에서 만든 UpgradeData 에셋들을 드래그
/// upgradeButtons·upgradeNameTexts·upgradeDescTexts : 같은 인덱스끼리 카드 1장을 구성
/// upgradeIconImages : Image 컴포넌트 연결 시 아이콘 표시 (선택)
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

    [Header("웨이브 알림")]
    [SerializeField] private TextMeshProUGUI waveMessageText;

    [Header("레벨업 카드 UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescTexts;
    [SerializeField] private Image[] upgradeIconImages;  // 선택

    [Header("강화 풀 (ScriptableObject)")]
    [Tooltip("Project에서 만든 UpgradeData 에셋들을 드래그하세요")]
    [SerializeField] private UpgradeData[] upgradePool;             // ← SO 배열 슬롯

    [Header("게임오버 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverKillText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;
    [SerializeField] private TextMeshProUGUI gameOverBestKillText;
    [SerializeField] private TextMeshProUGUI gameOverBestTimeText;

    [Header("스테이지 클리어 UI")]
    [SerializeField] private GameObject stageClearPanel;

    [Header("일시정지 UI")]
    [SerializeField] private GameObject pausePanel;

    // 플레이어 컴포넌트 캐시
    private PlayerController playerController;
    private ExperienceSystem playerExpSystem;
    private HealthSystem playerHealth;

    // 이번에 뽑힌 카드 선택지
    private UpgradeData[] currentChoices;

    private float gameTime;

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerExpSystem = player.GetComponent<ExperienceSystem>();
            playerHealth = player.GetComponent<HealthSystem>();

            if (playerHealth != null)
                playerHealth.OnHpChanged.AddListener(UpdateHpBar);
            if (playerExpSystem != null)
                playerExpSystem.OnExpChanged.AddListener(UpdateExpBar);
        }

        SetActive(gameOverPanel, false);
        SetActive(stageClearPanel, false);
        SetActive(upgradePanel, false);
        SetActive(pausePanel, false);
    }

    private void Update()
    {
        if (GameManager.Instance?.CurrentState == GameManager.GameState.Playing)
        {
            gameTime += Time.deltaTime;
            if (timerText != null)
                timerText.text = FormatTime(Mathf.RoundToInt(gameTime));
        }

        if (killCountText != null && GameManager.Instance != null)
            killCountText.text = $"Kill: {GameManager.Instance.KillCount}";
    }

    // ── HUD ───────────────────────────────────────────────

    private void UpdateHpBar(float current, float max)
    {
        if (hpBar != null) hpBar.value = current / max;
    }

    private void UpdateExpBar(int current, int required, int level)
    {
        if (expBar != null) expBar.value = (float)current / required;
        if (levelText != null) levelText.text = $"Lv.{level}";
    }

    // ── 레벨업 카드 ───────────────────────────────────────

    public void ShowUpgradeCards()
    {
        if (upgradePool == null || upgradePool.Length == 0)
        {
            Debug.LogWarning("[UIManager] upgradePool이 비어 있습니다. " +
                             "UpgradeData 에셋을 만들고 Inspector에 연결하세요");
            playerExpSystem?.OnUpgradeSelected();
            return;
        }

        if (upgradePanel == null || upgradeButtons == null || upgradeButtons.Length == 0)
        {
            Debug.LogWarning("[UIManager] upgradePanel 또는 upgradeButtons가 연결되지 않았습니다");
            playerExpSystem?.OnUpgradeSelected();
            return;
        }

        int count = Mathf.Min(upgradeButtons.Length, upgradePool.Length, 3);
        currentChoices = PickRandom(count);
        upgradePanel.SetActive(true);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].onClick.RemoveAllListeners();

            bool valid = i < count && currentChoices[i] != null;
            upgradeButtons[i].gameObject.SetActive(valid);
            if (!valid) continue;

            UpgradeData data = currentChoices[i];

            if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
                upgradeNameTexts[i].text = data.upgradeName;
            if (upgradeDescTexts != null && i < upgradeDescTexts.Length && upgradeDescTexts[i] != null)
                upgradeDescTexts[i].text = data.description;
            if (upgradeIconImages != null && i < upgradeIconImages.Length && upgradeIconImages[i] != null)
            {
                upgradeIconImages[i].sprite = data.icon;
                upgradeIconImages[i].enabled = data.icon != null;
            }

            int captured = i;
            upgradeButtons[i].onClick.AddListener(() => OnCardClicked(captured));
        }
    }

    private void OnCardClicked(int index)
    {
        if (currentChoices == null || index >= currentChoices.Length) return;

        ApplyUpgrade(currentChoices[index]);
        upgradePanel.SetActive(false);
        playerExpSystem?.OnUpgradeSelected();
    }

    // ── 강화 적용 ─────────────────────────────────────────
    // PlayerController.Stats(PlayerStatsSO)에 직접 누적합니다.
    // SO를 통해 값이 쌓이므로 PlayerController가 매 프레임 SO를 읽으면 즉시 반영됩니다.

    private void ApplyUpgrade(UpgradeData data)
    {
        if (data == null) return;

        // PlayerController에서 SO 참조를 가져옴
        PlayerStatsSO stats = playerController != null ? playerController.Stats : null;

        switch (data.upgradeType)
        {
            case UpgradeType.AttackDamage:
                stats?.ApplyDamageMultiplier(data.multiplier);
                break;

            case UpgradeType.MoveSpeed:
                stats?.ApplyMoveSpeedMultiplier(data.multiplier);
                break;

            case UpgradeType.FireRate:
                stats?.ApplyFireRateMultiplier(data.multiplier);
                break;

            case UpgradeType.MaxHp:
                playerHealth?.SetMaxHp(playerHealth.MaxHp + data.flatValue);
                break;

            case UpgradeType.HpHeal:
                playerHealth?.Heal(data.flatValue);
                break;

            case UpgradeType.BulletPenetration:
                Debug.Log("[Upgrade] 관통 강화 — BulletController 확장 후 연결 예정");
                break;
        }

        Debug.Log($"[Upgrade] 적용: {data.upgradeName}");
    }

    // ── 랜덤 카드 선택 (Fisher-Yates 셔플) ───────────────

    private UpgradeData[] PickRandom(int count)
    {
        UpgradeData[] pool = (UpgradeData[])upgradePool.Clone();
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        UpgradeData[] result = new UpgradeData[count];
        for (int i = 0; i < count; i++) result[i] = pool[i];
        return result;
    }

    // ── 웨이브 알림 ───────────────────────────────────────

    public void ShowWaveMessage(string message)
    {
        if (waveMessageText == null) return;
        StopCoroutine(nameof(HideWaveMessage));
        waveMessageText.text = message;
        waveMessageText.gameObject.SetActive(true);
        StartCoroutine(nameof(HideWaveMessage));
    }

    private IEnumerator HideWaveMessage()
    {
        yield return new WaitForSeconds(2f);
        if (waveMessageText != null) waveMessageText.gameObject.SetActive(false);
    }

    // ── 게임오버 / 클리어 / 일시정지 ─────────────────────

    public void ShowGameOver(int kills, int seconds)
    {
        SetActive(gameOverPanel, true);

        if (gameOverKillText != null) gameOverKillText.text = $"처치 수: {kills}";
        if (gameOverTimeText != null) gameOverTimeText.text = $"생존 시간: {FormatTime(seconds)}";
        if (gameOverBestKillText != null) gameOverBestKillText.text = $"최고 처치: {GameManager.Instance?.BestKill}";
        if (gameOverBestTimeText != null) gameOverBestTimeText.text = $"최고 기록: {FormatTime(GameManager.Instance?.BestTime ?? 0)}";
    }

    public void ShowStageClear(int kills, int seconds) => SetActive(stageClearPanel, true);

    public void ShowPauseMenu() => SetActive(pausePanel, true);
    public void HidePauseMenu() => SetActive(pausePanel, false);

    // ── 버튼 이벤트 ───────────────────────────────────────

    public void OnRestartButton() => GameManager.Instance?.RestartGame();
    public void OnMainMenuButton() => GameManager.Instance?.GoToMainMenu();

    // ── 유틸 ──────────────────────────────────────────────

    private static void SetActive(GameObject go, bool active) { if (go != null) go.SetActive(active); }

    private static string FormatTime(int s) => $"{s / 60:00}:{s % 60:00}";
}