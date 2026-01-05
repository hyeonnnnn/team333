using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
   [Header("Target")]
    [SerializeField] private Transform target; // 플레이어

    [Header("Camera Distance")]
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float followSpeed = 10f;

    [Header("Mouse Rotation")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Collision")]
    [SerializeField] private bool checkCollision = true;
    [SerializeField] private float collisionRadius = 0.3f;
    [SerializeField] private LayerMask collisionLayers = -1;

    // 회전 각도
    private float _rotationX = 0f;
    private float _rotationY = 0f;

    // 현재 거리 (충돌 시 조정됨)
    private float _currentDistance;

    private void Start()
    {
        _currentDistance = distance;

        // 초기 회전 설정
        Vector3 angles = transform.eulerAngles;
        _rotationX = angles.y;
        _rotationY = angles.x;

        // 커서 숨기기 (옵션)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        UpdateCameraPosition();
    }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 수평 회전
        _rotationX += mouseX;

        // 수직 회전 (제한)
        _rotationY -= mouseY;
        _rotationY = Mathf.Clamp(_rotationY, minVerticalAngle, maxVerticalAngle);
    }

    /// <summary>
    /// 카메라 위치 업데이트
    /// </summary>
    private void UpdateCameraPosition()
    {
        // 타겟 위치 (높이 추가)
        Vector3 targetPosition = target.position + Vector3.up * height;

        // 카메라 회전
        Quaternion rotation = Quaternion.Euler(_rotationY, _rotationX, 0f);

        // 원하는 카메라 위치
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // 충돌 체크
        if (checkCollision)
        {
            _currentDistance = CheckCollision(targetPosition, desiredPosition);
            desiredPosition = targetPosition - (rotation * Vector3.forward * _currentDistance);
        }
        else
        {
            _currentDistance = distance;
        }

        // 부드럽게 이동
        float smoothFactor = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothFactor);

        // 카메라 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smoothFactor);
    }

    /// <summary>
    /// 카메라 충돌 체크 (벽 통과 방지)
    /// </summary>
    private float CheckCollision(Vector3 targetPosition, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - targetPosition;
        float targetDistance = direction.magnitude;

        // SphereCast로 충돌 감지
        if (Physics.SphereCast(targetPosition, collisionRadius, direction.normalized,
            out RaycastHit hit, targetDistance, collisionLayers))
        {
            // 충돌 지점까지의 거리 반환
            return Mathf.Max(hit.distance - collisionRadius, 0.5f);
        }

        return distance;
    }

    /// <summary>
    /// 디버그 기즈모
    /// </summary>
    private void OnDrawGizmos()
    {
        if (target == null) return;

        // 타겟 위치
        Gizmos.color = Color.yellow;
        Vector3 targetPos = target.position + Vector3.up * height;
        Gizmos.DrawWireSphere(targetPos, 0.2f);

        // 카메라 연결선
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(targetPos, transform.position);

        // 충돌 체크 반경
        if (checkCollision)
        {
            Gizmos.color = Color.red;
            Vector3 direction = (transform.position - targetPos).normalized;
            Gizmos.DrawWireSphere(targetPos + direction * _currentDistance, collisionRadius);
        }
    }
}
