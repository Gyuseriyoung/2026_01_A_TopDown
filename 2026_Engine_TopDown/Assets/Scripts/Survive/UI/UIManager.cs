using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 내 모든 UI 관리
/// 강화 카드는 UpgradeData ScriptableObject 배열로 관리합니다
/// 
/// [설정 방법]
/// upgradePool: Project 창에서 만든 UpgradeData 에셋들을 여기에 드래그
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
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescTexts;

    [Header("강화 카드 데이터 (ScriptableObject)")]
    [SerializeField] private UpgradeData[] upgradePool; // Project에서 만든 UpgradeData 에셋 배열

    [Header("게임오버 / 클리어 UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverKillText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;
    [SerializeField] private TextMeshProUGUI gameOverBestKillText;
    [SerializeField] private TextMeshProUGUI gameOverBestTimeText;
    [SerializeField] private GameObject stageClearPanel;

    [Header("일시정지 UI")]
    [SerializeField] private GameObject pausePanel;

    private ExperienceSystem playerExpSystem;
    private PlayerController playerController;
    private HealthSystem playerHealth;
    private float gameTime;

    // 이번 카드 선택에서 뽑힌 강화 목록 (버튼 콜백용)
    private UpgradeData[] currentUpgradeChoices;

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

        if (killCountText != null && GameManager.Instance != null)
            killCountText.text = $"Kill: {GameManager.Instance.KillCount}";
    }

    // ── HUD 업데이트 ──────────────────────────────────────

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
        if (upgradePanel == null || upgradePool == null || upgradePool.Length == 0) return;

        upgradePanel.SetActive(true);

        int choiceCount = Mathf.Min(upgradeButtons.Length, upgradePool.Length, 3);
        currentUpgradeChoices = GetRandomUpgrades(choiceCount);

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].onClick.RemoveAllListeners();

            if (i < choiceCount && currentUpgradeChoices[i] != null)
            {
                UpgradeData data = currentUpgradeChoices[i];

                if (upgradeNameTexts != null && i < upgradeNameTexts.Length)
                    upgradeNameTexts[i].text = data.upgradeName;
                if (upgradeDescTexts != null && i < upgradeDescTexts.Length)
                    upgradeDescTexts[i].text = data.description;

                int capturedIndex = i;
                upgradeButtons[i].onClick.AddListener(() => OnUpgradeSelected(capturedIndex));
                upgradeButtons[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnUpgradeSelected(int choiceIndex)
    {
        if (currentUpgradeChoices == null || choiceIndex >= currentUpgradeChoices.Length) return;

        ApplyUpgrade(currentUpgradeChoices[choiceIndex]);
        upgradePanel.SetActive(false);
        playerExpSystem?.OnUpgradeSelected();
    }

    private void ApplyUpgrade(UpgradeData data)
    {
        if (data == null) return;

        switch (data.upgradeType)
        {
            case UpgradeType.AttackDamage:
                // BulletController는 Instantiate 시점에 데미지 주입 필요
                // 추후 PlayerStats 컴포넌트로 관리 권장
                Debug.Log($"[Upgrade] 공격력 강화 (multiplier: {data.multiplier}) — PlayerStats 구현 후 연결");
                break;

            case UpgradeType.MoveSpeed:
                if (playerController != null)
                    playerController.SetMoveSpeed(playerController.GetMoveSpeed() * data.multiplier);
                break;

            case UpgradeType.FireRate:
                if (playerController != null)
                    playerController.SetFireRate(playerController.GetFireRate() * data.multiplier);
                break;

            case UpgradeType.MaxHp:
                if (playerHealth != null)
                    playerHealth.SetMaxHp(playerHealth.MaxHp + data.flatValue);
                break;

            case UpgradeType.HpHeal:
                if (playerHealth != null)
                    playerHealth.Heal(data.flatValue);
                break;

            case UpgradeType.BulletPenetration:
                Debug.Log("[Upgrade] 관통 강화 — BulletController 확장 후 연결");
                break;
        }
    }

    private UpgradeData[] GetRandomUpgrades(int count)
    {
        UpgradeData[] result = new UpgradeData[count];
        System.Collections.Generic.List<int> pool = new System.Collections.Generic.List<int>();
        for (int i = 0; i < upgradePool.Length; i++) pool.Add(i);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int rand = Random.Range(0, pool.Count);
            result[i] = upgradePool[pool[rand]];
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

        // 최고 기록 표시 (PlayerPrefs에서 불러옴)
        int bestKill = PlayerPrefs.GetInt("BestKill", 0);
        int bestTime = PlayerPrefs.GetInt("BestTime", 0);
        if (gameOverBestKillText != null) gameOverBestKillText.text = $"최고 처치: {bestKill}";
        if (gameOverBestTimeText != null) gameOverBestTimeText.text = $"최고 기록: {bestTime / 60:00}:{bestTime % 60:00}";
    }

    public void ShowStageClear(int kills, int seconds)
    {
        if (stageClearPanel != null) stageClearPanel.SetActive(true);
    }

    public void ShowPauseMenu() { if (pausePanel != null) pausePanel.SetActive(true); }
    public void HidePauseMenu() { if (pausePanel != null) pausePanel.SetActive(false); }

    // ── 버튼 이벤트 ───────────────────────────────────────

    public void OnRestartButton() => GameManager.Instance.RestartGame();
    public void OnMainMenuButton() => GameManager.Instance.GoToMainMenu();
}