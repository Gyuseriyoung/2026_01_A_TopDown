using UnityEngine;

/// <summary>
/// 플레이어 방향 애니메이션 컨트롤러
/// - Up / Down / Left 클립 사용
/// - Right는 Left 클립 + SpriteRenderer.flipX
///
/// [Animator 파라미터 설정]
///   Int  "Direction"  : 0=Down, 1=Up, 2=Left
///   Bool "IsMoving"   : 이동 중 여부
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayeranimationController : MonoBehaviour
{
    [Header("Animator 파라미터 이름 (에셋 파라미터명과 정확히 일치해야 함)")]
    [SerializeField] private string paramDirection = "Direction";
    [SerializeField] private string paramIsMoving = "IsMoving";

    private const int DIR_DOWN = 0;
    private const int DIR_UP = 1;
    private const int DIR_LEFT = 2;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    private int lastDirection = DIR_DOWN;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerController = GetComponentInParent<PlayerController>();

       
    }

    private void Update()
    {
        if (playerController == null || animator == null) return;

        // 회전 강제 초기화 (애니메이션 클립에 rotation 트랙이 있을 경우 방지)
        transform.localRotation = Quaternion.identity;

        Vector2 input = playerController.MoveInput;
        bool isMoving = input.sqrMagnitude > 0.01f;

        // ── 방향 결정 ──────────────────────────────────────────
        if (isMoving)
        {
            float absX = Mathf.Abs(input.x);
            float absY = Mathf.Abs(input.y);

            if (absY > absX)
            {
                // 수직 이동이 수평보다 클 때만 Up/Down 전환
                // (등호 제거 → 좌우 이동 중 미세한 y축 입력에 Up/Down이 끼어드는 현상 방지)
                lastDirection = input.y > 0 ? DIR_UP : DIR_DOWN;
            }
            else
            {
                // absX >= absY → 좌우 이동
                lastDirection = DIR_LEFT;
            }
        }
        // 멈추면 lastDirection 유지 → 마지막 방향 Idle

        // ── flipX 처리 ─────────────────────────────────────────
        if (spriteRenderer != null)
        {
            if (lastDirection == DIR_LEFT)
                spriteRenderer.flipX = isMoving && input.x > 0; // 오른쪽이면 반전
            else
                spriteRenderer.flipX = false; // 위/아래는 무조건 해제
        }

        // ── Animator 파라미터 전달 ──────────────────────────────
        animator.SetInteger(paramDirection, lastDirection);
        animator.SetBool(paramIsMoving, isMoving);
    }
}