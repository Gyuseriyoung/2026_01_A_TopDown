using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 내 모든 UI 관리
/// 체력바 / 경험치바 / 레벨업 카드 / 게임오버 화면
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("웨이브 알림")]
    [SerializeField] private TextMeshProUGUI waveMessageText;

    [Header("레벨업 카드 UI")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;           // 카드 3장
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescTexts;

    [Header("게임오버 / 클리어 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverKillText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;
    [SerializeField] private GameObject stageClearPanel;

    [Header("일시정지 UI")]
    [SerializeField] private GameObject pausePanel;

    // 프로토타입용 강화 목록 (나중에 ScriptableObject로 교체)
    private string[] upgradeNames = {
        "공격력 강화", "이동속도 강화", "발사 속도 강화",
        "최대 체력 증가", "총알 관통", "체력 회복"
    };
    private string[] upgradeDescs = {
        "총알 데미지 +20%", "이동 속도 +15%", "발사 속도 +20%",
        "최대 체력 +30", "총알이 적을 관통함", "체력 20 회복"
    };

    private ExperienceSystem playerExpSystem;
    private PlayerController playerController;
    private HealthSystem playerHealth;
    private float gameTime;

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
            playerExpSystem = player.GetComponent<ExperienceSystem>();
            playerController = player.GetComponent<PlayerController>();
            playerHealth = player.GetComponent<HealthSystem>();

            // 이벤트 구독
            if (playerHealth != null)
                playerHealth.OnHpChanged.AddListener(UpdateHpBar);
            if (playerExpSystem != null)
                playerExpSystem.OnExpChanged.AddListener(UpdateExpBar);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (stageClearPanel != null) stageClearPanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void Update()
    {
        // 타이머 업데이트
        if (GameManager.Instance?.CurrentState == GameManager.GameState.Playing)
        {
            gameTime += Time.deltaTime;
            if (timerText != null)
            {
                int min = (int)(gameTime / 60);
                int sec = (int)(gameTime % 60);
                timerText.text = $"{min:00}:{sec:00}";
            }
        }

        // 처치 수 업데이트
        if (killCountText != null && GameManager.Instance != null)
            killCountText.text = $"Kill: {GameManager.Instance.KillCount}";
    }

    // ── HUD 업데이트 ──────────────────────────────────────

    private void UpdateHpBar(float current, float max)
    {
        if (hpBar != null)
            hpBar.value = current / max;
    }

    private void UpdateExpBar(int current, int required, int level)
    {
        if (expBar != null)
            expBar.value = (float)current / required;
        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }

    // ── 레벨업 카드 ───────────────────────────────────────

    public void ShowUpgradeCards()
    {
        if (upgradePanel == null) return;

        upgradePanel.SetActive(true);

        // 랜덤하게 3개의 강화 선택지 뽑기
        int[] selected = GetRandomUpgrades(3);

        for (int i = 0; i < upgradeButtons.Length && i < 3; i++)
        {
            int index = selected[i];
            int buttonIndex = i; // 클로저 캡처용

            if (upgradeNameTexts != null && i < upgradeNameTexts.Length)
                upgradeNameTexts[i].text = upgradeNames[index];
            if (upgradeDescTexts != null && i < upgradeDescTexts.Length)
                upgradeDescTexts[i].text = upgradeDescs[index];

            upgradeButtons[i].onClick.RemoveAllListeners();
            upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(index));
        }
    }

    private void OnUpgradeSelected(int upgradeIndex)
    {
        ApplyUpgrade(upgradeIndex);
        upgradePanel.SetActive(false);
        playerExpSystem?.OnUpgradeSelected();
    }

    private void ApplyUpgrade(int index)
    {
        // 프로토타입 강화 적용 (나중에 ScriptableObject 기반으로 교체)
        switch (index)
        {
            case 0: // 공격력 (BulletController 직접 수정은 복잡해서 스탯으로 관리)
                break;
            case 1: // 이동속도
                if (playerController != null)
                    playerController.SetMoveSpeed(playerController.GetMoveSpeed() * 1.15f);
                break;
            case 2: // 발사 속도
                if (playerController != null)
                    playerController.SetFireRate(0.3f * 0.8f);
                break;
            case 3: // 최대 체력
                playerHealth?.SetMaxHp(playerHealth.MaxHp + 30f);
                break;
            case 5: // 체력 회복
                playerHealth?.Heal(20f);
                break;
        }
    }

    private int[] GetRandomUpgrades(int count)
    {
        int[] result = new int[count];
        System.Collections.Generic.List<int> pool = new System.Collections.Generic.List<int>();
        for (int i = 0; i < upgradeNames.Length; i++) pool.Add(i);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int rand = Random.Range(0, pool.Count);
            result[i] = pool[rand];
            pool.RemoveAt(rand);
        }
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
        if (waveMessageText != null)
            waveMessageText.gameObject.SetActive(false);
    }

    // ── 게임오버 / 클리어 / 일시정지 ─────────────────────

    public void ShowGameOver(int kills, int seconds)
    {
        if (gameOverPanel == null) return;
        gameOverPanel.SetActive(true);
        if (gameOverKillText != null) gameOverKillText.text = $"처치 수: {kills}";
        if (gameOverTimeText != null) gameOverTimeText.text = $"생존 시간: {seconds / 60:00}:{seconds % 60:00}";
    }

    public void ShowStageClear(int kills, int seconds)
    {
        if (stageClearPanel != null) stageClearPanel.SetActive(true);
    }

    public void ShowPauseMenu() { if (pausePanel != null) pausePanel.SetActive(true); }
    public void HidePauseMenu() { if (pausePanel != null) pausePanel.SetActive(false); }

    // ── 버튼 이벤트 (Inspector에서 연결) ─────────────────

    public void OnRestartButton() => GameManager.Instance.RestartGame();
    public void OnMainMenuButton() => GameManager.Instance.GoToMainMenu();
}