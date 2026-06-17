using System.Collections;
using UnityEngine;

/// <summary>
/// 웨이브 방식 적 스폰 관리자
/// WaveData ScriptableObject 배열로 웨이브를 설정합니다
/// 
/// [설정 방법]
/// 1. Project 창에서 Create → Game/Wave Data 로 웨이브 에셋 생성
/// 2. 이 컴포넌트의 waves 배열에 순서대로 드래그
/// 3. defaultEnemyPrefabs: WaveData에 프리팹이 없을 때 사용할 기본 프리팹
/// </summary>
public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("웨이브 데이터 (ScriptableObject)")]
    [SerializeField] private WaveData[] waves;

    [Header("기본 스폰 설정")]
    [SerializeField] private GameObject[] defaultEnemyPrefabs; // WaveData에 프리팹 없을 때 사용
    [SerializeField] private float spawnRadius = 12f;
    [SerializeField] private float timeBetweenWaves = 3f;

    // 내부 상태
    private int currentWaveIndex;
    private int remainingEnemies;
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
        for (int i = 0; i < waves.Length; i++)
        {
            currentWaveIndex = i;
            WaveData data = waves[i];

            UIManager.Instance?.ShowWaveMessage($"Wave {i + 1}");
            yield return new WaitForSeconds(data.startDelay);

            yield return StartCoroutine(SpawnWave(data));

            // 모든 적 처치 대기
            yield return new WaitUntil(() => remainingEnemies <= 0);

            if (i < waves.Length - 1)
            {
                UIManager.Instance?.ShowWaveMessage("Wave Clear!");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        GameManager.Instance.OnStageClear();
    }

    private IEnumerator SpawnWave(WaveData data)
    {
        remainingEnemies = data.enemyCount;

        for (int i = 0; i < data.enemyCount; i++)
        {
            SpawnEnemy(data);
            yield return new WaitForSeconds(data.spawnInterval);
        }
    }

    private void SpawnEnemy(WaveData data)
    {
        if (playerTransform == null) return;

        // WaveData 프리팹 우선, 없으면 기본 프리팹
        GameObject prefab = data.GetRandomEnemyPrefab();
        if (prefab == null)
        {
            if (defaultEnemyPrefabs == null || defaultEnemyPrefabs.Length == 0) return;
            prefab = defaultEnemyPrefabs[Random.Range(0, defaultEnemyPrefabs.Length)];
        }

        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 웨이브 배율 적용
        ApplyWaveMultipliers(enemy, data);
    }

    private void ApplyWaveMultipliers(GameObject enemy, WaveData data)
    {
        if (data.enemyHpMultiplier != 1f)
        {
            HealthSystem hp = enemy.GetComponent<HealthSystem>();
            if (hp != null)
                hp.SetMaxHp(hp.MaxHp * data.enemyHpMultiplier, true);
        }

        if (data.enemySpeedMultiplier != 1f)
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
                ec.SetMoveSpeed(ec.GetMoveSpeed() * data.enemySpeedMultiplier);
        }
    }

    private Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return (Vector2)playerTransform.position + new Vector2(
            Mathf.Cos(angle) * spawnRadius,
            Mathf.Sin(angle) * spawnRadius
        );
    }

    // ── 외부 호출 ─────────────────────────────────────────

    public void OnEnemyDead()
    {
        remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
        GameManager.Instance?.OnEnemyKilled();
    }

    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetRemainingEnemies() => remainingEnemies;
}