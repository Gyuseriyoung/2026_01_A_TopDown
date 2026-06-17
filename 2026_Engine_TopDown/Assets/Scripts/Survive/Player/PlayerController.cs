using UnityEngine;

/// <summary>
/// 플레이어 이동, 마우스 조준 발사를 담당합니다.
/// Rigidbody2D 기반 물리 이동. 스프라이트 회전 없음 — 방향 애니메이션은
/// PlayerAnimationController가 별도로 처리합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(HealthSystem))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("발사 설정")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.3f;
    [SerializeField] private float bulletSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime;
    private Camera mainCamera;
    private bool isDead;

    public Vector2 MoveInput => moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (isDead) return;
        HandleMovementInput();
        HandleAutoFire();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    private void HandleMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void HandleAutoFire()
    {
        if (Time.time < nextFireTime) return;
        if (bulletPrefab == null || firePoint == null) return;

        nextFireTime = Time.time + fireRate;
        Fire();
    }

    private void Fire()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();

        if (bullet != null)
            bullet.Init(direction, bulletSpeed, gameObject.tag);
    }

    public void OnDead()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance.OnPlayerDead();
    }

    // ── Getter / Setter ───────────────────────────────────

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float value) => moveSpeed = value;

    public float GetFireRate() => fireRate;           // UIManager ApplyUpgrade에서 사용
    public void SetFireRate(float value) => fireRate = value;

    public float GetBulletSpeed() => bulletSpeed;
    public void SetBulletSpeed(float value) => bulletSpeed = value;

    public bool IsDead => isDead;
}