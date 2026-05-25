using UnityEngine;

/// <summary>
/// 플레이어를 부드럽게 따라가는 카메라
/// Camera 오브젝트에 부착합니다
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [Header("추적 설정")]
    [SerializeField] private Transform target;         // 플레이어 Transform
    [SerializeField] private float smoothSpeed = 5f;  // 클수록 빠르게 따라옴
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    [Header("경계 설정 (선택)")]
    [SerializeField] private bool useBounds;
    [SerializeField] private float minX, maxX, minY, maxY;

    private void LateUpdate()
    {
        if (target == null)
        {
            // 런타임에 플레이어 자동 탐색
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            return;
        }

        Vector3 desired = target.position + offset;

        // 경계 클램핑
        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        // Lerp로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}