using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 상태 관리 싱글톤
/// GameOver / StageClear / Pause 처리
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, StageClear }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("씬 이름")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "GameScene";

    // 세션 통계 (추후 JSON 저장에 활용)
    public int KillCount { get; private set; }
    public float SurvivedTime { get; private set; }
    public int ReachedWave { get; private set; }

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
        }
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

        // ESC → 일시정지 (레벨업 UI가 없을 때만)
        if (Input.GetKeyDown(KeyCode.Escape) && CurrentState == GameState.Playing)
            TogglePause();
    }

    // ── 상태 전환 ─────────────────────────────────────────

    public void OnPlayerDead()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        ReachedWave = WaveManager.Instance?.GetCurrentWave() ?? 0;

        // 세이브 (추후 JSON 연동)
        // SaveManager.Instance.SaveResult(KillCount, SurvivedTime, ReachedWave);

        UIManager.Instance?.ShowGameOver(KillCount, Mathf.RoundToInt(SurvivedTime));
    }

    public void OnStageClear()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.StageClear;
        Time.timeScale = 0f;
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