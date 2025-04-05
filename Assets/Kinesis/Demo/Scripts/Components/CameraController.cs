using UnityEditor;
using UnityEngine;

namespace Kinesis.Demo {
    public class CameraController : MonoBehaviour {
        [Tooltip("Start Play Mode in Scene view.")]
        public bool startInSceneView = true;
        [Tooltip("Start Play Mode with Game view camera locked.")]
        public bool startLockedCamera;
        [Tooltip("Lock mouse cursor in Play Mode.")]
        public bool lockCursor = true;
        [Tooltip("Show controls on start.")]
        public bool showControlsAtStart = true;
        [Tooltip("Camera keyboard movement speed.")]
        public float moveSpeed = 2.5f;
        [Tooltip("Camera mouselook sensitivity.")]
        public float mouseSensitivity = 1000.0f;
        bool isCameraLocked;
        bool cachedCameraLockState;
        bool escaped;
        Canvas controlsCanvas;

        Vector3 GetDirection() {
            Vector3 direction = Vector3.zero;

            if (Input.GetKey(KeyCode.Space)) {
                direction += Vector3.up;
            }
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                direction += Vector3.down;
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                direction += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                direction += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                direction += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)){
                direction += Vector3.right;
            }

            return direction;
        }

        void OnApplicationFocus(bool hasFocus) {
            EditorWindow focusedWindow = EditorWindow.focusedWindow;
            if (focusedWindow == SceneView.currentDrawingSceneView) {
                // Ensure cursor is unlocked in scene view.
                Cursor.lockState = CursorLockMode.None;
                return;
            }
        }

        void OnValidate() {
            if (Application.isPlaying) {
                Cursor.lockState = (this.lockCursor) ? CursorLockMode.Locked : CursorLockMode.None;
            }
        }

        void Start() {
            this.controlsCanvas = this.GetComponentInChildren<Canvas>();
            this.controlsCanvas.enabled = this.showControlsAtStart;

            if (Application.isEditor && this.startInSceneView) {
                SceneView.FocusWindowIfItsOpen(typeof(SceneView));
            }

            // Camera is locked and considered escaped from game view if starting in scene view.
            // This prevents the minor annoyance of the camera processing mouselook controls while the Game View has not
            // yet fully captured the mouse, but is technically in focus.
            this.isCameraLocked = this.startInSceneView ? this.startInSceneView : this.startLockedCamera;
            this.escaped = this.startInSceneView;
            Cursor.lockState = (!this.startInSceneView && this.lockCursor) ? CursorLockMode.Locked : CursorLockMode.None;
        }

        void Update() {
            if (!Application.isFocused) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                // Store configured camera lock state before escaping game view.
                this.cachedCameraLockState = this.isCameraLocked;
                // Lock camera when escaping game view.
                this.isCameraLocked = true;
                this.escaped = true;
            }

            if (Input.GetKeyDown(KeyCode.H)) {
                this.controlsCanvas.enabled = !this.controlsCanvas.enabled;
            }

            if (Input.GetMouseButtonUp(0) && this.escaped) {
                Cursor.lockState = (this.lockCursor) ? CursorLockMode.Locked : CursorLockMode.None;
                // Restore previously configured camera lock state.
                this.isCameraLocked = this.cachedCameraLockState;
                this.escaped = false;
            }

            if (Input.GetKeyDown(KeyCode.P)) {
                EditorApplication.isPaused = !EditorApplication.isPaused;
            }

            if (Input.GetKeyDown(KeyCode.Tab)) {
                this.isCameraLocked = !this.isCameraLocked;
            }

            // Don't run camera controls if paused or camera is locked.
            if (EditorApplication.isPaused || this.isCameraLocked) {
                return;
            }

            // Handle camera mouselook controls.
            float deltaYaw = -Input.GetAxis("Mouse Y") * this.mouseSensitivity * Time.deltaTime;
            float deltaPitch = Input.GetAxis("Mouse X") * this.mouseSensitivity * Time.deltaTime;
            Vector3 rotation = new Vector3(
                this.transform.eulerAngles.x + deltaYaw,
                this.transform.eulerAngles.y + deltaPitch,
                0.0f
            );
            this.transform.eulerAngles = rotation;

            // Handle camera keyboard controls.
            Vector3 direction = GetDirection();
            this.transform.Translate(direction * this.moveSpeed * Time.deltaTime);
        }
    }
}