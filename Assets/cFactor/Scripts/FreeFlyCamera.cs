using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Cámara de vuelo libre: WASD para moverse, clic derecho + ratón para mirar.
/// Q/E o Ctrl/Espacio para bajar/subir. Shift para ir más rápido.
/// </summary>
[DisallowMultipleComponent]
public class FreeFlyCamera : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float fastMultiplier = 3f;
    [SerializeField] float lookSensitivity = 0.15f;

    float pitch;
    float yaw;

    void Start()
    {
        Vector3 euler = transform.eulerAngles;
        pitch = euler.x;
        yaw = euler.y;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.rightButton.isPressed)
        {
            if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * lookSensitivity;
        pitch -= delta.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        if (Keyboard.current == null)
            return;

        float speed = moveSpeed;
        if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
            speed *= fastMultiplier;

        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            input += transform.forward;
        if (Keyboard.current.sKey.isPressed)
            input -= transform.forward;
        if (Keyboard.current.dKey.isPressed)
            input += transform.right;
        if (Keyboard.current.aKey.isPressed)
            input -= transform.right;

        if (Keyboard.current.eKey.isPressed || Keyboard.current.spaceKey.isPressed)
            input += Vector3.up;
        if (Keyboard.current.qKey.isPressed || Keyboard.current.leftCtrlKey.isPressed)
            input -= Vector3.up;

        if (input.sqrMagnitude > 0f)
            transform.position += input.normalized * speed * Time.deltaTime;
    }
}
