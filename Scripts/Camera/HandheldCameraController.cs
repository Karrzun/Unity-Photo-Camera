using UnityEngine;
using UnityEngine.InputSystem;

public enum MouseButton
{
    Left,
    Right,
    Middle
}

public class HandheldCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera controlledCamera;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private bool invertY = false;

    [Header("Horizontal Rotation")]
    [SerializeField] private bool limitYaw = false;
    [SerializeField] private float minYaw = -120f;
    [SerializeField] private float maxYaw = 120f;

    [Header("Vertical Rotation")]
    [SerializeField] private float minPitch = -55f;
    [SerializeField] private float maxPitch = 75f;

    [Header("Zoom")]
    [SerializeField] private float minFieldOfView = 25f;
    [SerializeField] private float maxFieldOfView = 65f;
    [SerializeField] private float zoomStep = 5f;

    [Header("Smoothing")]
    [SerializeField] private bool useSmoothing = true;
    [SerializeField] private float rotationSmoothing = 18f;
    [SerializeField] private float zoomSmoothing = 15f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;
    [SerializeField] private Key unlockCursorKey = Key.Escape;
    [SerializeField] private MouseButton lockCursorButton = MouseButton.Left;

    private float targetYaw;
    private float targetPitch;
    private float currentYaw;
    private float currentPitch;

    private float targetFieldOfView;

    private void Awake()
    {
        if (controlledCamera == null)
            controlledCamera = GetComponentInChildren<Camera>();

        Vector3 rigEuler = transform.localEulerAngles;
        Vector3 cameraEuler = controlledCamera != null
            ? controlledCamera.transform.localEulerAngles
            : Vector3.zero;

        targetYaw = currentYaw = NormalizeAngle(rigEuler.y);
        targetPitch = currentPitch = NormalizeAngle(cameraEuler.x);

        if (controlledCamera != null)
            targetFieldOfView = controlledCamera.fieldOfView;
    }

    private void Start()
    {
        if (lockCursorOnStart)
            LockCursor();
    }

    private void Update()
    {
        HandleCursorInput();

        if (controlledCamera == null)
            return;

        HandleLookInput();
        HandleZoomInput();
        ApplyRotation();
        ApplyZoom();
    }

    private void HandleCursorInput()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null && keyboard[unlockCursorKey].wasPressedThisFrame)
        {
            UnlockCursor();
        }

        if (mouse != null && WasMouseButtonPressed(mouse, lockCursorButton))
        {
            LockCursor();
        }
    }

    private bool WasMouseButtonPressed(Mouse mouse, MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => mouse.leftButton.wasPressedThisFrame,
            MouseButton.Right => mouse.rightButton.wasPressedThisFrame,
            MouseButton.Middle => mouse.middleButton.wasPressedThisFrame,
            _ => false
        };
    }

    private void HandleLookInput()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        Vector2 mouseDelta = mouse.delta.ReadValue();

        targetYaw += mouseDelta.x * mouseSensitivity;

        float yDirection = invertY ? 1f : -1f;
        targetPitch += mouseDelta.y * mouseSensitivity * yDirection;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        if (limitYaw)
            targetYaw = Mathf.Clamp(targetYaw, minYaw, maxYaw);
    }

    private void HandleZoomInput()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        float scrollValue = mouse.scroll.ReadValue().y;

        if (Mathf.Approximately(scrollValue, 0f))
            return;

        float scrollSteps = NormalizeScrollValue(scrollValue);

        targetFieldOfView -= scrollSteps * zoomStep;
        targetFieldOfView = Mathf.Clamp(targetFieldOfView, minFieldOfView, maxFieldOfView);
    }

    private void ApplyRotation()
    {
        if (useSmoothing)
        {
            float t = 1f - Mathf.Exp(-rotationSmoothing * Time.deltaTime);

            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, t);
            currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, t);
        }
        else
        {
            currentYaw = targetYaw;
            currentPitch = targetPitch;
        }

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
        controlledCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    private void ApplyZoom()
    {
        if (useSmoothing)
        {
            float t = 1f - Mathf.Exp(-zoomSmoothing * Time.deltaTime);
            controlledCamera.fieldOfView = Mathf.Lerp(controlledCamera.fieldOfView, targetFieldOfView, t);
        }
        else
        {
            controlledCamera.fieldOfView = targetFieldOfView;
        }
    }

    private float NormalizeScrollValue(float scrollValue)
    {
        if (Mathf.Abs(scrollValue) > 1f)
            return scrollValue / 120f;

        return scrollValue;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (controlledCamera == null)
            controlledCamera = GetComponentInChildren<Camera>();

        minPitch = Mathf.Clamp(minPitch, -89f, 89f);
        maxPitch = Mathf.Clamp(maxPitch, -89f, 89f);

        if (maxPitch < minPitch)
            maxPitch = minPitch;

        minFieldOfView = Mathf.Clamp(minFieldOfView, 1f, 179f);
        maxFieldOfView = Mathf.Clamp(maxFieldOfView, 1f, 179f);

        if (maxFieldOfView < minFieldOfView)
            maxFieldOfView = minFieldOfView;

        rotationSmoothing = Mathf.Max(0.01f, rotationSmoothing);
        zoomSmoothing = Mathf.Max(0.01f, zoomSmoothing);
        zoomStep = Mathf.Max(0.1f, zoomStep);
    }
#endif
}