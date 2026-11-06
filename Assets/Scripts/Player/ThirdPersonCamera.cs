using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform to follow")]
    public Transform Target;

    [Header("Camera Positioning")]
    [Tooltip("Target look-at height on the player (meters)")]
    public float TargetHeight = 1.0f;

    [Tooltip("Distance behind the player")]
    public float Distance = 3.2f;

    [Tooltip("Height above the player")]
    public float Height = 3.4f;

    [Tooltip("How far ahead down the path the camera looks (meters)")]
    public float LookAheadDistance = 3.5f;

    [Tooltip("How smoothly camera position catches up with the player")]
    public float PositionSmoothTime = 0.08f;

    [Tooltip("How smoothly camera rotation aligns behind player heading")]
    public float RotationSmoothTime = 5.0f;

    [Tooltip("Layers that block camera line of sight (Maze walls)")]
    public LayerMask ObstacleLayers = ~0;

    private Vector3 _currentVelocity;
    private float _currentYaw;
    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null)
        {
            _cam.fieldOfView = 68.0f;
        }

        if (Target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) Target = p.transform;
        }

        if (Target != null)
        {
            _currentYaw = Target.eulerAngles.y;
            SnapToTarget();
        }

        // Keep cursor unlocked and visible for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SnapToTarget()
    {
        if (Target == null) return;
        _currentYaw = Target.eulerAngles.y;
        Vector3 focusPoint = Target.position + Vector3.up * TargetHeight;
        Quaternion rot = Quaternion.Euler(0f, _currentYaw, 0f);
        Vector3 desiredPos = focusPoint - rot * Vector3.forward * Distance + Vector3.up * (Height - TargetHeight);
        transform.position = desiredPos;

        Vector3 lookTarget = focusPoint + rot * Vector3.forward * LookAheadDistance + Vector3.up * 0.2f;
        transform.LookAt(lookTarget);
    }

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 focusPoint = Target.position + Vector3.up * TargetHeight;

        // Smoothly align yaw with the player's facing direction
        _currentYaw = Mathf.LerpAngle(_currentYaw, Target.eulerAngles.y, Time.deltaTime * RotationSmoothTime);
        Quaternion currentRotation = Quaternion.Euler(0f, _currentYaw, 0f);

        // Desired position behind player
        Vector3 targetPos = focusPoint - currentRotation * Vector3.forward * Distance;
        targetPos.y = focusPoint.y + (Height - TargetHeight);

        // Wall collision check: if a wall blocks line of sight behind, smoothly pull closer and pitch up
        Vector3 dirFromFocus = targetPos - focusPoint;
        float dist = dirFromFocus.magnitude;
        RaycastHit hit;
        if (Physics.SphereCast(focusPoint, 0.22f, dirFromFocus.normalized, out hit, dist, ObstacleLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("Player"))
            {
                float hitDist = Mathf.Max(1.6f, hit.distance - 0.2f);
                float pushFactor = 1.0f - Mathf.Clamp01(hitDist / dist);
                targetPos = focusPoint + dirFromFocus.normalized * hitDist + Vector3.up * (pushFactor * 0.8f);
            }
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, PositionSmoothTime);

        // Aim camera ahead down the path so player sees where they are going, not blocking their view
        Vector3 lookTarget = focusPoint + currentRotation * Vector3.forward * LookAheadDistance + Vector3.up * 0.2f;
        transform.LookAt(lookTarget);
    }
}
