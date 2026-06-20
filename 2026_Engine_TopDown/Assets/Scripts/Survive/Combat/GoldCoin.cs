using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class GoldCoin : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float attractRange = 3f;     // 흡수 시작 거리
    [SerializeField] private float attractSpeed = 8f;     // 이동 속도
    [SerializeField] private float lifetime = 15f;        // 자동 소멸 시간

    private int goldValue = 5;
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        GetComponent<CircleCollider2D>().isTrigger = true;
        Invoke(nameof(DestroySelf), lifetime);
    }

    public void SetValue(int value) => goldValue = value;

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= attractRange)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attractSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ⭐️ 획득한 골드를 GameManager를 통해 영구 저장소에 추가
        GameManager.Instance?.AddGold(goldValue);

        Destroy(gameObject);
    }

    private void DestroySelf() => Destroy(gameObject);
}