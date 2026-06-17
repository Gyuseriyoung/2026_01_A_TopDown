using System.Collections;
using UnityEngine;

/// <summary>
/// 웨이브 방식 적 스폰 관리자
/// WaveData ScriptableObject 배열로 웨이브를 설정합니다
///
/// [Inspector 설정 방법]
/// 1. Project 창 우클릭 → Create → Game/Wave Data 로 웨이브 에셋 생성
/// 2. Waves 배열에 순서대로 드래그 (Wave1.asset, Wave2.asset ...)
/// 3. Default Enemy Prefabs: WaveData에 프리팹이 없을 때 사용할 기본 프리팹
/// </summary>
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

    // ── 초기화 ────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (waves == null || waves.Length == 0)
        {
            Debug.LogError("[WaveManager] waves 배열이 비어 있습니다. " +
                           "Project에서 WaveData 에셋을 만들고 Inspector에 연결하세요");
            return;
        }

        StartCoroutine(RunWaveSequence());
    }

    // ── 웨이브 시퀀스 ─────────────────────────────────────

    private IEnumerator RunWaveSequence()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            currentWaveIndex = i;
            WaveData data = waves[i];

            UIManager.Instance?.ShowWaveMessage($"Wave {i + 1}");
            yield return new WaitForSeconds(data.startDelay);

            yield return StartCoroutine(SpawnWave(data));
            yield return new WaitUntil(() => remainingEnemies <= 0);

            bool isLastWave = i == waves.Length - 1;
            if (!isLastWave)
            {
                UIManager.Instance?.ShowWaveMessage("Wave Clear!");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        GameManager.Instance?.OnStageClear();
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

    // ── 스폰 ──────────────────────────────────────────────

    private void SpawnEnemy(WaveData data)
    {
        if (playerTransform == null) return;

        GameObject prefab = data.GetRandomEnemyPrefab();

        // WaveData에 프리팹 없으면 기본 프리팹 사용
        if (prefab == null)
        {
            if (defaultEnemyPrefabs == null || defaultEnemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[WaveManager] 스폰할 프리팹이 없습니다");
                remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
                return;
            }
            prefab = defaultEnemyPrefabs[Random.Range(0, defaultEnemyPrefabs.Length)];
        }

        Vector2 spawnPos = GetSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        ApplyWaveMultipliers(enemy, data);
    }

    /// <summary>WaveData의 배율을 스폰된 적에게 적용합니다</summary>
    private void ApplyWaveMultipliers(GameObject enemy, WaveData data)
    {
        // HP 배율
        if (!Mathf.Approximately(data.enemyHpMultiplier, 1f))
        {
            HealthSystem hp = enemy.GetComponent<HealthSystem>();
            if (hp != null)
                hp.SetMaxHp(hp.MaxHp * data.enemyHpMultiplier, healToFull: true);
        }

        // 이동속도 배율
        if (!Mathf.Approximately(data.enemySpeedMultiplier, 1f))
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
                ec.SetMoveSpeed(ec.GetMoveSpeed() * data.enemySpeedMultiplier);
        }

        // 경험치 드랍 배율
        if (!Mathf.Approximately(data.enemyExpMultiplier, 1f))
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            if (ec != null)
                ec.SetExpMultiplier(data.enemyExpMultiplier);
        }
    }

    private Vector2 GetSpawnPosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return (Vector2)playerTransform.position
               + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
    }

    // ── 외부 호출 ─────────────────────────────────────────

    /// <summary>EnemyController.OnDead()에서 호출</summary>
    public void OnEnemyDead()
    {
        remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
        GameManager.Instance?.OnEnemyKilled();
    }

    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetRemainingEnemies() => remainingEnemies;
}