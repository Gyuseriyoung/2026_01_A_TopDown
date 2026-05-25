using System.Collections;
using UnityEngine;

/// <summary>
/// 웨이브 방식 적 스폰 관리자
/// 프로토타입: 코드 기반 웨이브 설정
/// 최종: ScriptableObject 기반 웨이브 데이터로 교체 예정
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("스폰 설정")]
    [SerializeField] private GameObject[] enemyPrefabs;    // 적 프리팹 배열
    [SerializeField] private float spawnRadius = 12f;      // 플레이어 기준 스폰 반경
    [SerializeField] private float spawnInterval = 1.5f;   // 스폰 간격

    [Header("웨이브 설정 (프로토타입용)")]
    [SerializeField] private int[] enemyCountPerWave = { 5, 10, 20 }; // 각 웨이브 적 수
    [SerializeField] private float timeBetweenWaves = 3f;

    // 내부 상태
    private int currentWave;
    private int remainingEnemies;
    private int totalEnemiesInWave;
    private bool isSpawning;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        StartCoroutine(StartWaveSequence());
    }

    // ── 웨이브 진행 ───────────────────────────────────────

    private IEnumerator StartWaveSequence()
    {
        for (int i = 0; i < enemyCountPerWave.Length; i++)
        {
            currentWave = i + 1;
            UIManager.Instance?.ShowWaveMessage($"Wave {currentWave}");

            yield return new WaitForSeconds(2f); // 웨이브 시작 알림 대기

            yield return StartCoroutine(SpawnWave(enemyCountPerWave[i]));

            // 모든 적이 죽을 때까지 대기
            yield return new WaitUntil(() => remainingEnemies <= 0);

            if (i < enemyCountPerWave.Length - 1)
            {
                UIManager.Instance?.ShowWaveMessage("Wave Clear!");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        // 모든 웨이브 클리어
        GameManager.Instance.OnStageClear();
    }

    private IEnumerator SpawnWave(int count)
    {
        totalEnemiesInWave = count;
        remainingEnemies = count;
        isSpawning = true;

        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (playerTransform == null || enemyPrefabs.Length == 0) return;

        // 플레이어 주위 랜덤 위치에 스폰 (화면 밖)
        Vector2 spawnPos = GetSpawnPosition();

        // 랜덤 적 선택
        int index = Random.Range(0, enemyPrefabs.Length);
        Instantiate(enemyPrefabs[index], spawnPos, Quaternion.identity);
    }

    private Vector2 GetSpawnPosition()
    {
        // 원형 범위에서 랜덤 각도 선택
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return (Vector2)playerTransform.position + new Vector2(
            Mathf.Cos(angle) * spawnRadius,
            Mathf.Sin(angle) * spawnRadius
        );
    }

    // ── 외부 호출 ─────────────────────────────────────────

    /// <summary>EnemyController.OnDead()에서 호출</summary>
    public void OnEnemyDead()
    {
        remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
    }

    public int GetCurrentWave() => currentWave;
    public int GetRemainingEnemies() => remainingEnemies;
}