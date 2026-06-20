using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("웨이브 데이터 (ScriptableObject)")]
    [Tooltip("Project에서 만든 WaveData 에셋을 순서대로 드래그하세요")]
    [SerializeField] private WaveData[] waves;

    [Header("기본 스폰 설정")]
    [Tooltip("WaveData에 프리팹이 없을 때 사용할 기본 적 프리팹")]
    [SerializeField] private GameObject[] defaultEnemyPrefabs;
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
        if (player != null) playerTransform = player.transform;

        StartWave();
    }

    public void StartWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            GameManager.Instance?.OnStageClear();
            return;
        }

        StartCoroutine(nameof(SpawnLoop));
    }

    // ── ⭐️ 수정된 스폰 루프 코루틴 ───────────────────────────
    private IEnumerator SpawnLoop()
    {
        WaveData data = waves[currentWaveIndex];
        remainingEnemies = data.enemyCount;

        UIManager.Instance?.ShowWaveMessage($"Wave {currentWaveIndex + 1}");
        yield return new WaitForSeconds(data.startDelay);

        // [분기 1] 보스 웨이브일 때
        if (data.waveType == WaveType.Boss)
        {
            remainingEnemies = 1; // 보스는 단 한 마리만 타겟으로 잡음
            SpawnBossEnemy(data);
            while (remainingEnemies > 0)
            {
                yield return null;
            }
        }

        // [분기 2] 원형 포위 스폰 웨이브일 때
        if (data.waveType == WaveType.CircleSurround)
        {
            int spawnedCount = 0;
            while (spawnedCount < data.enemyCount)
            {
                // 한 번에 소환할 적 마리 수 계산 (남은 적 수 고려)
                int countToSpawn = Mathf.Min(data.circleSpawnCount, data.enemyCount - spawnedCount);

                SpawnEnemyCircle(data, countToSpawn);
                spawnedCount += countToSpawn;

                yield return new WaitForSeconds(data.spawnInterval);
            }
            while (remainingEnemies > 0)
            {
                yield return null;
            }
        }

        // [분기 3] 일반 웨이브일 때 (기존 방식 유지)
        for (int i = 0; i < data.enemyCount; i++)
        {
            SpawnEnemy(data);
            yield return new WaitForSeconds(data.spawnInterval);
        }
    }

    // ── ⭐️ 스폰 실제 처리 함수들 (Instantiate 담당) ────────────────

    // 1) 기존의 일반 랜덤 사방 스폰 함수
    private void SpawnEnemy(WaveData data)
    {
        GameObject prefab = data.GetRandomEnemyPrefab() ?? defaultEnemyPrefabs[Random.Range(0, defaultEnemyPrefabs.Length)];
        if (prefab == null) return;

        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        ApplyWaveModifiers(enemy, data);
    }

    // 2) 플레이어를 중점으로 동그랗게 원을 그리며 스폰하는 신규 함수
    private void SpawnEnemyCircle(WaveData data, int count)
    {
        if (playerTransform == null) return;

        for (int i = 0; i < count; i++)
        {
            // 360도(PI * 2)를 배치할 마리 수만큼 나누어 정교한 라디안 각도 계산
            float angle = i * (Mathf.PI * 2f) / count;

            // 삼각함수로 원주 상의 위치 구하기
            Vector2 spawnOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
            Vector2 spawnPos = (Vector2)playerTransform.position + spawnOffset;

            GameObject prefab = data.GetRandomEnemyPrefab() ?? defaultEnemyPrefabs[Random.Range(0, defaultEnemyPrefabs.Length)];
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
                ApplyWaveModifiers(enemy, data);
            }
        }
    }

    // 3) 보스를 스폰하는 신규 함수
    private void SpawnBossEnemy(WaveData data)
    {
        if (playerTransform == null) return;

        Vector2 spawnPos = GetSpawnPosition();
        GameObject prefab = data.GetRandomEnemyPrefab() ?? defaultEnemyPrefabs[Random.Range(0, defaultEnemyPrefabs.Length)];

        if (prefab != null)
        {
            GameObject boss = Instantiate(prefab, spawnPos, Quaternion.identity);
            ApplyWaveModifiers(boss, data);

            // 보스답게 스케일 키우기 (2배 확대)
            boss.transform.localScale = prefab.transform.localScale * 2f;
        }
    }

    // ── 유틸 및 기존 함수 유지 ───────────────────────────────────────

    private void ApplyWaveModifiers(GameObject enemy, WaveData data)
    {
        if (enemy == null || data == null) return;

        if (!Mathf.Approximately(data.enemyHpMultiplier, 1f))
        {
            HealthSystem hp = enemy.GetComponent<HealthSystem>();
            if (hp != null)
                hp.SetMaxHp(hp.MaxHp * data.enemyHpMultiplier, healToFull: true);
        }

        if (!Mathf.Approximately(data.enemySpeedMultiplier, 1f))
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
                ec.SetMoveSpeed(ec.GetMoveSpeed() * data.enemySpeedMultiplier);
        }

        if (!Mathf.Approximately(data.enemyExpMultiplier, 1f))
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
                ec.SetExpMultiplier(data.enemyExpMultiplier);
        }
    }

    private Vector2 GetSpawnPosition()
    {
        if (playerTransform == null) return Vector2.zero;
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return (Vector2)playerTransform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
    }

    public void OnEnemyDead()
    {
        // 이미 0 이하인데 또 들어오는 언더플로우 방지
        if (remainingEnemies <= 0) return;

        remainingEnemies--;
        GameManager.Instance?.OnEnemyKilled();

        if (remainingEnemies <= 0)
        {
            StartCoroutine(nameof(NextWaveRoutine));
        }
    }

    private IEnumerator NextWaveRoutine()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWaveIndex++;
        StartWave();
    }

    public int GetCurrentWave() => currentWaveIndex + 1;
}