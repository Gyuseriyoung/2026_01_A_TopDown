using UnityEngine;

/// <summary>
/// 플레이어 방향 애니메이션 컨트롤러
/// - Up / Down / Left 클립 사용
/// - Right는 Left 클립 + SpriteRenderer.flipX
///
/// [Animator 파라미터 설정]
///   Int  "Direction"  : 0=Down, 1=Up, 2=Left
///   Bool "IsMoving"   : 이동 중 여부
///
/// [Animator State 구성]
///   Idle_Down  (기본값)
///   Idle_Up
///   Idle_Left
///   Walk_Down
///   Walk_Up
///   Walk_Left
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimationController : MonoBehaviour
{
    // ── Animator 파라미터 이름 (Inspector에서 설정한 이름과 일치해야 함) ──
    [Header("Animator 파라미터 이름")]
    [SerializeField] private string paramDirection = "Direction";
    [SerializeField] private string paramIsMoving = "IsMoving";

    // 방향 값 (Animator Int 파라미터에 들어가는 값)
    private const int DIR_DOWN = 0;
    private const int DIR_UP = 1;
    private const int DIR_LEFT = 2;

    // 컴포넌트
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerController playerController;

    // 상태
    private int lastDirection = DIR_DOWN;
    private bool lastIsMoving = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        Vector2 input = playerController.MoveInput;
        bool isMoving = input.sqrMagnitude > 0.01f;

        // ── 방향 결정 ───────────────────────────────────────
        // 대각선 입력은 수직(Y) 우선 처리
        // (위/아래가 동시 입력되면 위쪽을 우선)
        int direction = lastDirection; // 멈춰도 마지막 방향 유지

        if (isMoving)
        {
            if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
            {
                // 수직 이동 우선
                direction = input.y > 0 ? DIR_UP : DIR_DOWN;
            }
            else
            {
                // 수평 이동 — Left 클립 사용 + flipX 처리
                direction = DIR_LEFT;
            }
        }

        // ── flipX 처리 (오른쪽 이동 시 Left 클립 반전) ──────
        if (isMoving && Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            // 오른쪽이면 flipX = true, 왼쪽이면 false
            spriteRenderer.flipX = input.x > 0;
        }
        else
        {
            // 수직 이동 / 정지 시에는 flipX 초기화
            spriteRenderer.flipX = false;
        }

        // ── Animator 파라미터 업데이트 (변경된 경우만) ────────
        if (direction != lastDirection)
        {
            animator.SetInteger(paramDirection, direction);
            lastDirection = direction;
        }

        if (isMoving != lastIsMoving)
        {
            animator.SetBool(paramIsMoving, isMoving);
            lastIsMoving = isMoving;
        }
    }
}