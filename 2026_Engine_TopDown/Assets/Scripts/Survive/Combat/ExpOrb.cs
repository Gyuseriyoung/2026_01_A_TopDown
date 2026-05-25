using UnityEngine;

/// <summary>
/// 경험치 구슬 - 플레이어 주변에 있으면 자동으로 빨려듦
/// 플레이어와 접촉 시 경험치 지급
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class ExpOrb : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float attractRange = 3f;     // 흡수 시작 거리
    [SerializeField] private float attractSpeed = 8f;     // 이동 속도
    [SerializeField] private float lifetime = 10f;        // 자동 소멸 시간

    private int expValue;
    private Transform playerTransform;
    private bool isAttracting;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        GetComponent<CircleCollider2D>().isTrigger = true;
        Invoke(nameof(DestroySelf), lifetime);
    }

    public void SetValue(int value) => expValue = value;

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 범위 안에 들어오면 플레이어 방향으로 이동
        if (dist <= attractRange)
        {
            isAttracting = true;
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attractSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 경험치 지급
        ExperienceSystem expSystem = other.GetComponent<ExperienceSystem>();
        if (expSystem != null)
            expSystem.AddExp(expValue);

        DestroySelf();
    }

    private void DestroySelf()
    {
        CancelInvoke();
        Destroy(gameObject);
    }
}