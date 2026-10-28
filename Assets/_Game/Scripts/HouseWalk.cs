using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HouseWalk : MonoBehaviour
{
    public Camera view;
    public GameObject roof;
    public float walkSpeed = 2.4f;
    public float runSpeed = 4f;
    public float mouseSensitivity = 2f;
    float pitch, verticalSpeed;
    CharacterController controller;
    Vector3 spawn;
    void Start() {
        controller = GetComponent<CharacterController>();
        spawn = transform.position;
        if (!view) view = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) ReleaseMouse();
        if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        bool captured = Cursor.lockState == CursorLockMode.Locked;
        if (captured && view) {
            transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity, 0);
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -85, 85);
            view.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        Vector3 move = Vector3.zero;
        if (captured) move = transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical");
        if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
        verticalSpeed = Mathf.Max(verticalSpeed - 20 * Time.deltaTime, -30);
        controller.Move((move.normalized * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed) + Vector3.up * verticalSpeed) * Time.deltaTime);
        if (transform.position.y < -5) { controller.enabled = false; transform.position = spawn; controller.enabled = true; verticalSpeed = 0; }
        if (Input.GetKeyDown(KeyCode.R) && roof) roof.SetActive(!roof.activeSelf);
    }
    void ReleaseMouse() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    void OnApplicationFocus(bool focused) { if (!focused) ReleaseMouse(); }
    void OnDisable() { ReleaseMouse(); }
    void OnGUI() {
        GUI.Box(new Rect(12, 12, 660, 32), "WASD / Arrows: move  |  Mouse: look  |  Shift: run  |  Esc: cursor  |  R: roof");
        if (Cursor.lockState != CursorLockMode.Locked) GUI.Box(new Rect(Screen.width/2-150, Screen.height/2-22, 300, 44), "Click the game view to walk");
    }
}
