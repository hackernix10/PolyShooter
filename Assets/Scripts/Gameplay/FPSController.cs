using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour {
  CharacterController characterController;
  Transform cameraTransform;

  [Header("FPS Controller")]
  public float minimumX = -90.0f;
  public float maximumX = 90.0f;
  public Vector3 cameraOffset = Vector3.zero;

  public float sensitivity = 2.0f;
  public float speed = 5.0f;
  public float jumpPower = 10.0f;

  Quaternion cameraRotation;
  Quaternion characterRotation;

  Vector3 moveDir = new Vector3(0, 0, 0);

  public bool lockCursor = true;

  bool isCrouching = false;
  bool isMovementEnabled = true;

  void Start() {
    characterController = GetComponent<CharacterController>();

    cameraTransform = Camera.main.transform;
    cameraTransform.parent = transform;
    cameraTransform.localPosition = cameraOffset;
    cameraTransform.localRotation = new Quaternion(0, 0, 0, 1);

    cameraRotation = cameraTransform.localRotation;
    characterRotation = transform.localRotation;
  }

  void Update() {
    if (isMovementEnabled) {
      if (Input.GetKey(KeyCode.LeftControl)) {
        isCrouching = true;
      } else {
        isCrouching = false;
      }
      CameraUpdate();
    }

    if (isMovementEnabled) {
      if (lockCursor) {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
      } else {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
      }
    }
  }

  void FixedUpdate() {
    MovementUpdate();
  }

  void OnDisable() {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
  }

  public bool IsMovementEnabled() { return isMovementEnabled; }
  public void EnableMovement() {
    isMovementEnabled = true;
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;
  } 

  public void DisableMovement() {
    isMovementEnabled = false;
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
  }

  void CameraUpdate() {
    float yRot = Input.GetAxis("Mouse X") * sensitivity;
    float xRot = Input.GetAxis("Mouse Y") * sensitivity;

    characterRotation *= Quaternion.Euler(0, yRot, 0);
    cameraRotation *= Quaternion.Euler(-xRot, 0, 0);

    cameraRotation = ClampRotationAroundXAxis(cameraRotation);

    transform.localRotation = characterRotation;
    cameraTransform.localRotation = cameraRotation;

    cameraTransform.localPosition = cameraOffset;

    if (isCrouching) {
      cameraTransform.localPosition = new Vector3(0, 0, 0);
    }
  }

  void MovementUpdate() {
    Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    float currentSpeed = speed;

    if (!isMovementEnabled)
      input = Vector3.zero;

    if (isCrouching) {
      currentSpeed = speed * 0.25f;
    }

    Vector3 desiredMove = transform.forward * input.z + transform.right * input.x;

    moveDir.x = desiredMove.x * currentSpeed;
    moveDir.z = desiredMove.z * currentSpeed;

    if (characterController.isGrounded) {
      moveDir.y = -10.0f;

      if (Input.GetKey(KeyCode.Space) && isMovementEnabled) {
        moveDir.y = jumpPower;
      }
    } else {
      moveDir += Physics.gravity * Time.fixedDeltaTime;
    }

    characterController.Move(moveDir * Time.fixedDeltaTime);
  }

  Quaternion ClampRotationAroundXAxis(Quaternion q) {
    q.x /= q.w;
    q.y /= q.w;
    q.z /= q.w;
    q.w = 1.0f;

    float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
    angleX = Mathf.Clamp(angleX, minimumX, maximumX);
    q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
    return q;
  }

  public float ClampAngle(float angle, float min, float max) {
    if (angle > 360) angle -= 360; else if (angle < -360) angle += 360;
    return Mathf.Clamp(angle, min, max);
  }
}
