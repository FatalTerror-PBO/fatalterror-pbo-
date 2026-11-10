using UnityEngine;
using HorrorGame.Core;

namespace HorrorGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Referensi")]
        [SerializeField] private Transform cameraRoot;

        [Header("Gerak")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float gravity = -20f;

        [Header("Kamera")]
        [SerializeField] private float mouseSensitivity = 2f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrain = 20f;
        [SerializeField] private float staminaRegen = 12f;

        private CharacterController controller;
        private float pitch;
        private float velocityY;
        private float stamina;
        private bool exhausted;

        /// <summary>PROPERTY get/set: setter private + validasi (clamp) + memicu event.</summary>
        public float Stamina
        {
            get => stamina;
            private set
            {
                stamina = Mathf.Clamp(value, 0f, maxStamina);
                GameEvents.StaminaChanged.Raise(stamina / maxStamina);
            }
        }

        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            Stamina = maxStamina;
        }

        private void Update()
        {
            if (!GameManager.IsPlaying) return;

            Look();
            Move();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch = Mathf.Clamp(pitch - mouseY, -80f, 80f);
            if (cameraRoot != null)
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 direction = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1f);

            if (Stamina <= 0.1f) exhausted = true;
            else if (Stamina >= 20f) exhausted = false;

            IsSprinting = Input.GetKey(KeyCode.LeftShift) && z > 0.1f && !exhausted;

            float speed = IsSprinting ? sprintSpeed : walkSpeed;
            Stamina += (IsSprinting ? -staminaDrain : staminaRegen) * Time.deltaTime;

            if (controller.isGrounded && velocityY < 0f) velocityY = -2f;
            velocityY += gravity * Time.deltaTime;

            controller.Move((direction * speed + Vector3.up * velocityY) * Time.deltaTime);
        }
    }
}
