using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태 관리 싱글톤
/// PlayerPrefs로 최고 기록(최다 처치, 최장 생존)을 저장합니다
/// 
/// [PlayerPrefs 키 목록]
/// "BestKill"  : int  — 역대 최고 처치 수
/// "BestTime"  : int  — 역대 최장 생존 시간 (초)
/// "BestWave"  : int  — 역대 최고 도달 웨이브
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
    public int ReachedWave { get; private set; }

    // 최고 기록 (PlayerPrefs에서 로드)
    public int BestKill { get; private set; }
    public int BestTime { get; private set; }
    public int BestWave { get; private set; }

    private float gameStartTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬 전환 필요 시 활성화
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadBestRecords();
    }

    private void Start()
    {
        gameStartTime = Time.time;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (CurrentState == GameState.Playing)
            SurvivedTime = Time.time - gameStartTime;

        if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.Playing)
            TogglePause();
    }

    // ── 기록 저장 / 불러오기 (PlayerPrefs) ───────────────

    private void LoadBestRecords()
    {
        BestKill = PlayerPrefs.GetInt("BestKill", 0);
        BestTime = PlayerPrefs.GetInt("BestTime", 0);
        BestWave = PlayerPrefs.GetInt("BestWave", 0);
    }

    private void SaveBestRecords()
    {
        bool updated = false;

        if (KillCount > BestKill)
        {
            BestKill = KillCount;
            PlayerPrefs.SetInt("BestKill", BestKill);
            updated = true;
        }

        int survivedSeconds = Mathf.RoundToInt(SurvivedTime);
        if (survivedSeconds > BestTime)
        {
            BestTime = survivedSeconds;
            PlayerPrefs.SetInt("BestTime", BestTime);
            updated = true;
        }

        if (ReachedWave > BestWave)
        {
            BestWave = ReachedWave;
            PlayerPrefs.SetInt("BestWave", BestWave);
            updated = true;
        }

        if (updated)
            PlayerPrefs.Save(); // 즉시 디스크에 기록
    }

    /// <summary>저장된 모든 최고 기록을 초기화합니다</summary>
    public void ResetBestRecords()
    {
        PlayerPrefs.DeleteKey("BestKill");
        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.DeleteKey("BestWave");
        PlayerPrefs.Save();
        LoadBestRecords();
        Debug.Log("[GameManager] 최고 기록 초기화 완료");
    }

    // ── 상태 전환 ─────────────────────────────────────────

    public void OnPlayerDead()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        ReachedWave = WaveManager.Instance?.GetCurrentWave() ?? 0;

        SaveBestRecords();

        UIManager.Instance?.ShowGameOver(KillCount, Mathf.RoundToInt(SurvivedTime));
    }

    public void OnStageClear()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.StageClear;
        Time.timeScale = 0f;

        ReachedWave = WaveManager.Instance?.GetCurrentWave() ?? 0;

        SaveBestRecords();

        UIManager.Instance?.ShowStageClear(KillCount, Mathf.RoundToInt(SurvivedTime));
    }

    public void OnEnemyKilled()
    {
        KillCount++;
    }

    private void TogglePause()
    {
        if (CurrentState == GameState.Paused)
        {
            CurrentState = GameState.Playing;
            Time.timeScale = 1f;
            UIManager.Instance?.HidePauseMenu();
        }
        else
        {
            CurrentState = GameState.Paused;
            Time.timeScale = 0f;
            UIManager.Instance?.ShowPauseMenu();
        }
    }

    // ── 씬 전환 ───────────────────────────────────────────

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }
}