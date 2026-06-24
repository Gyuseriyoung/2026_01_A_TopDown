using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태 및 JSON/PlayerPrefs 세이브 시스템 관리 싱글톤
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, StageClear }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("씬 이름")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "GameScene";

    // 세션 통계
    public int KillCount { get; private set; }
    public float SurvivedTime { get; private set; }
    public int TotalGold { get; private set; }
    public int CurrentSessionGold { get; private set; } // 이번 판에서 번 돈 (UI 표시용)

    // 최고 기록 (PlayerPrefs 독립 저장)
    public int BestKill { get; private set; }
    public int BestTime { get; private set; }

    [Header("── JSON 세이브 데이터 바구니 ──")]
    // PlayerStatsSO와 ShopManager가 참조할 데이터 구조체
    public ShopSaveData shopProgress = new ShopSaveData();
    private const string JSON_SAVE_KEY = "PlayerShopJsonData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 게임 시작 시 모든 데이터(최고기록 및 JSON 상점) 로드
            LoadBestRecords();
            LoadShopJsonData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
        {
            SurvivedTime += Time.deltaTime;
            UIManager.Instance?.UpdateTimer(SurvivedTime);
        }

        // ESC 키로 일시정지 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }


    /// <summary> [JSON 저장] 상점 업그레이드 레벨들을 JSON 텍스트로 말아서 저장합니다. </summary>
    public void SaveShopJsonData()
    {
        if (shopProgress == null) return;

        string jsonText = JsonUtility.ToJson(shopProgress, true);

        PlayerPrefs.SetString(JSON_SAVE_KEY, jsonText);
        PlayerPrefs.Save();

        Debug.Log($"[JSON 직렬화 세이브 성공]:\n{jsonText}");
    }

    /// <summary> [JSON 로드] 로컬에 저장된 JSON 텍스트를 디코딩하여 복원합니다. </summary>
    public void LoadShopJsonData()
    {
        if (PlayerPrefs.HasKey(JSON_SAVE_KEY))
        {
            string jsonText = PlayerPrefs.GetString(JSON_SAVE_KEY);

            // JSON 문자열을 다시 원래 C# 데이터 객체 구조로 완벽 복원
            shopProgress = JsonUtility.FromJson<ShopSaveData>(jsonText);
            Debug.Log("[JSON 로드 성공] 기존 유저 스탯 데이터를 성공적으로 파싱했습니다.");
        }
        else
        {
            // 세이브가 없다면 깨끗한 새 데이터 생성
            shopProgress = new ShopSaveData();
            Debug.Log("[JSON 데이터 없음] 새로운 세이브 데이터를 기본값으로 생성했습니다.");
        }

        // 보유 골드 복구
        TotalGold = PlayerPrefs.GetInt("TotalGold", 0);
    }

    // ── 💰 재화 시스템 ────────────────────────────────────────

    public void AddGold(int amount)
    {
        TotalGold += amount;
        CurrentSessionGold += amount;
        PlayerPrefs.SetInt("TotalGold", TotalGold);
        PlayerPrefs.Save();
        UIManager.Instance?.UpdateGoldText(TotalGold);
    }

    public bool SpendGold(int amount)
    {
        if (TotalGold >= amount)
        {
            TotalGold -= amount;
            PlayerPrefs.SetInt("TotalGold", TotalGold);
            PlayerPrefs.Save();
            UIManager.Instance?.UpdateGoldText(TotalGold);
            return true;
        }
        return false;
    }

    // ── ☠️ 사망 및 클리어 (타이밍 버그 완벽 수정) ──────────────────

    public void OnPlayerDead()
    {
        // 중복 진입 방지 및 유효성 체크
        if (CurrentState != GameState.Playing) return;

        // ⭐️ 원인 해결: 시간이 멈추고 UI가 뜨기 전에 최고 기록 및 데이터를 먼저 디스크에 굽습니다!
        SaveBestRecords();

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f; // 물리/타이밍 정지

        UIManager.Instance?.ShowGameOver(KillCount, Mathf.RoundToInt(SurvivedTime));
    }

    public void OnStageClear()
    {
        if (CurrentState != GameState.Playing) return;

        SaveBestRecords();

        CurrentState = GameState.StageClear;
        Time.timeScale = 0f;

        UIManager.Instance?.ShowStageClear(KillCount, Mathf.RoundToInt(SurvivedTime));
    }

    public void OnEnemyKilled()
    {
        KillCount++;
        UIManager.Instance?.UpdateKillCount(KillCount);
    }

    private void TogglePause()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            UIManager.Instance?.HidePauseMenu();
        }
        else if (CurrentState == GameState.Playing)
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            UIManager.Instance?.ShowPauseMenu();
        }
    }

    private void SaveBestRecords()
    {
        // 이번 판 기록이 최다 처치 기록보다 크다면 갱신
        if (KillCount > BestKill)
        {
            BestKill = KillCount;
            PlayerPrefs.SetInt("BestKill", BestKill);
        }

        // 이번 판 기록이 최장 생존 기록보다 크다면 갱신
        int currentSec = Mathf.RoundToInt(SurvivedTime);
        if (currentSec > BestTime)
        {
            BestTime = currentSec;
            PlayerPrefs.SetInt("BestTime", BestTime);
        }

        PlayerPrefs.Save();
    }

    private void LoadBestRecords()
    {
        BestKill = PlayerPrefs.GetInt("BestKill", 0);
        BestTime = PlayerPrefs.GetInt("BestTime", 0);
    }

    // ── 🎬 씬 제어 및 리셋 ────────────────────────────────────

    public void RestartGame()
    {
        KillCount = 0;
        SurvivedTime = 0f;
        CurrentSessionGold = 0;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.Paused;
        SceneManager.LoadScene(mainMenuScene);
    }
}