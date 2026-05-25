using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] private Transform firePoint;      // 총구 위치 (자식 오브젝트)
    [SerializeField] private float fireRate = 0.3f;   // 발사 간격 (초)
    [SerializeField] private float bulletSpeed = 10f;

    // 내부 상태
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float nextFireTime;
    private Camera mainCamera;
    private bool isDead;

    /// <summary>PlayerAnimationController가 읽어가는 현재 입력 방향</summary>
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

    // ── 이동 ──────────────────────────────────────────────

    private void HandleMovementInput()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float y = Input.GetAxisRaw("Vertical");   // W/S
        moveInput = new Vector2(x, y).normalized; // 대각선 이동 속도 정규화
    }

    private void Move()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    // ── 자동 발사 (총알 방향은 마우스 기준 유지) ──────────

    private void HandleAutoFire()
    {
        if (Time.time < nextFireTime) return;
        if (bulletPrefab == null || firePoint == null) return;

        nextFireTime = Time.time + fireRate;
        Fire();
    }

    private void Fire()
    {
        // 마우스 방향으로 총알 생성
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWorldPos - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        BulletController bullet = bulletObj.GetComponent<BulletController>();

        if (bullet != null)
            bullet.Init(direction, bulletSpeed, gameObject.tag);
    }

    // ── 외부 호출 ─────────────────────────────────────────

    /// <summary>HealthSystem에서 사망 시 호출</summary>
    public void OnDead()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance.OnPlayerDead();
    }

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float value) => moveSpeed = value;
    public void SetFireRate(float value) => fireRate = value;
    public bool IsDead => isDead;
}