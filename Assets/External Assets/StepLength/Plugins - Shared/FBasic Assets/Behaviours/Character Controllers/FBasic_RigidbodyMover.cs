using System;
using UnityEngine;
using UnityEngine.UI;

namespace FIMSpace.RagdollAnimatorDemo
{
    [AddComponentMenu("FImpossible Creations/Demos/Fimpossible Demo Mover")]
    [DefaultExecutionOrder(100)]
    public class FBasic_RigidbodyMover : FimpossibleComponent
    {
        public bool ModeJoystick = true;
        public float stepLength = 0;
        public float speedAnim = 1f;
        private float timer = 0f;
        public Rigidbody Rigb;
        public Joystick movementJoystick = null;
        public UIController uiController = null;

        [Space(4)]
        public float MovementSpeed = 2f;
        [Range(0f, 1f)]
        public float RotateToSpeed = 0.8f;
        [Tooltip("When true, applying rotation by rigidbody.rotation = ...\nWhen false, applying rotation using angular velocity (smoother interpolation)")]
        public bool FixedRotation = true;

        [Range(0f, 1f)] public float DirectMovement = 0f;
        [Range(0f, 1f)] public float Interia = 1f;

        [Space(4)] public LayerMask GroundMask = 0 >> 1;

        [Space(4)] public float ExtraRaycastDistance = 0.01f;

        [Tooltip("Using Spherecast is Radius greater than zero")]
        public float RaycastRadius = 0f;

        [Space(10)]
        [Tooltip("Setting 'Grounded','Moving' and 'Speed' parameters for mecanim")]
        public Animator Mecanim;

        [Tooltip("Animator property which will not allowing character movement is set to true")]
        public string IsBusyProperty = "";
        public bool DisableRootMotion = false;

        internal void SetTargetRotation(Vector3 dir)
        {
            targetInstantRotation = Quaternion.LookRotation(dir);
            if (currentWorldAccel == Vector3.zero) currentWorldAccel = new Vector3(0.0000001f, 0f, 0f);
        }

        internal void SetRotation(Vector3 dir)
        {
            targetInstantRotation = Quaternion.LookRotation(dir);
            rotationAngle = targetInstantRotation.eulerAngles.y;
            targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        }

        public void MoveTowards(Vector3 wPos, bool setDir = true)
        {
            Vector3 tPos = new Vector3(wPos.x, 0f, wPos.z);
            Vector3 mPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 dir = (tPos - mPos).normalized;
            moveDirectionWorld = dir;
            if (setDir) SetTargetRotation(dir);
        }

        [Space(6)]
        public bool UpdateInput = true;
        [Space(1)]
        public float JumpPower = 3f;
        public float HoldShiftForSpeed = 0f;
        public float HoldCtrlForSpeed = 0f;

        public Action OnJump = null;

        bool wasInitialized = false;

        public void ResetTargetRotation()
        {
            targetRotation = transform.rotation;
            targetInstantRotation = transform.rotation;
            rotationAngle = transform.eulerAngles.y;

            currentWorldAccel = Vector3.zero;
            jumpRequest = 0f;
        }

        void Start()
        {
            if (!Rigb) Rigb = GetComponent<Rigidbody>();

            if (Rigb)
            {
                Rigb.maxAngularVelocity = 30f;
                //if (Rigb.interpolation == RigidbodyInterpolation.None) Rigb.interpolation = RigidbodyInterpolation.Interpolate;
                Rigb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            isGrounded = true;
            targetRotation = transform.rotation;
            targetInstantRotation = transform.rotation;
            rotationAngle = transform.eulerAngles.y;

            if (Mecanim) Mecanim.SetBool("Grounded", true);

            wasInitialized = true;
        }
        
        private void OnEnable()
        {
            if (!wasInitialized) return;
            ResetTargetRotation();

            isGrounded = true;
            if (Mecanim) isGrounded = Mecanim.GetBool("Grounded");
            CheckGroundedState();
        }


        // Movement Calculation Params

        [NonSerialized] public Vector2 moveDirectionLocal = Vector3.zero;

        public float moveDirectionLocalZ = 0;
        public Vector2 moveDirectionLocalNonZero { get; private set; }

        public Vector3 moveDirectionWorld { get; private set; }
        public Vector3 currentWorldAccel { get; private set; }

        [NonSerialized] public float jumpRequest = 0f;

        Quaternion targetRotation;
        Quaternion targetInstantRotation;

        float rotationAngle = 0f;
        float sd_rotationAngle = 0f;
        float toJump = 0f;

        private Vector3 rotationAdjustment = Vector3.zero;

        public bool isGrounded { get; private set; } = true;

        public void changeRotation(Vector3 gyroData)
        {
            //// Integrate gyro data to update rotation angles
            rotationAdjustment.x = gyroData.x;
            rotationAdjustment.z = gyroData.y;
        }

        protected virtual void Update()
        {

            if (Rigb == null) return;

            bool updateMovement = true;
            bool standDown = false;
            if (Mecanim) if (string.IsNullOrWhiteSpace(IsBusyProperty) == false) updateMovement = !Mecanim.GetBool(IsBusyProperty);
            if (UpdateInput && updateMovement)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (toJump <= 0f)
                    {
                        jumpRequest = JumpPower;
                        toJump = 0f;
                    }
                }

                moveDirectionLocal = Vector2.zero;
                moveDirectionLocalZ = 0f;

                // Add code
                if (ModeJoystick)
                {
                    moveDirectionLocal.x = movementJoystick.Direction.x;
                    moveDirectionLocal.y = movementJoystick.Direction.y;
                }
                else
                {
                    //if (Input.GetKey(KeyCode.A)) moveDirectionLocal += Vector2.left;
                    //else if (Input.GetKey(KeyCode.D)) moveDirectionLocal += Vector2.right;

                    //if (Input.GetKey(KeyCode.W)) moveDirectionLocal += Vector2.up;
                    //else if (Input.GetKey(KeyCode.S)) moveDirectionLocal += Vector2.down;

                    if (uiController.GetButtonValue(1)) moveDirectionLocal += Vector2.left;
                    else if (uiController.GetButtonValue(3)) moveDirectionLocal += Vector2.right;

                    if (uiController.GetButtonValue(0)) moveDirectionLocal += Vector2.up;
                    else if (uiController.GetButtonValue(2)) moveDirectionLocal += Vector2.down;

                    if (uiController.GetButtonValue(4)) moveDirectionLocalZ += 1;
                    else if (uiController.GetButtonValue(5)) moveDirectionLocalZ += -1;
                }

                //if (Input.GetKey(KeyCode.A)) moveDirectionLocal += Vector2.left;
                //else if (Input.GetKey(KeyCode.D)) moveDirectionLocal += Vector2.right;

                //if (Input.GetKey(KeyCode.W)) moveDirectionLocal += Vector2.up;
                //else if (Input.GetKey(KeyCode.S)) moveDirectionLocal += Vector2.down;

                Quaternion flatCamRot = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);

                if (moveDirectionLocal != Vector2.zero)
                {
                    moveDirectionLocal.Normalize();
                    moveDirectionWorld = flatCamRot * new Vector3(moveDirectionLocal.x, moveDirectionLocalZ, moveDirectionLocal.y);
                    moveDirectionLocalNonZero = moveDirectionLocal;
                }
                else
                {
                    moveDirectionWorld = Vector3.zero;
                }
                //Debug.Log("FBasic: " + moveDirectionWorld.ToString());
                if (moveDirectionWorld == Vector3.zero)
                {
                    moveDirectionWorld = new Vector3(Mathf.Abs(stepLength), 0f, 0f);
                    standDown = true;
                }
                Vector3 rotationMovement = moveDirectionWorld;
                rotationMovement.y = 0f;
                if (rotationMovement.x !=0 || rotationMovement.z != 0) targetInstantRotation = Quaternion.LookRotation(rotationMovement);
            }
            else if (updateMovement == false) moveDirectionWorld = Vector3.zero;



            bool moving = false;
            if (moveDirectionWorld != Vector3.zero)
            {
                moving = true;
            }

            if (RotateToSpeed > 0f)
                if (currentWorldAccel != Vector3.zero)
                {
                    rotationAngle = Mathf.SmoothDampAngle(rotationAngle, targetInstantRotation.eulerAngles.y, ref sd_rotationAngle, Mathf.Lerp(0.5f, 0.01f, RotateToSpeed));
                    targetRotation = Quaternion.Euler(0f, rotationAngle, 0f);// Quaternion.RotateTowards(targetRotation, targetInstantRotation, Time.deltaTime * 90f * RotateToSpeed);
                }

            //if (Mecanim) Mecanim.SetBool("Moving", moving);

            float spd = MovementSpeed;

            if (UpdateInput)
            {
                if (HoldShiftForSpeed != 0f) if (Input.GetKey(KeyCode.LeftShift)) spd = HoldShiftForSpeed;
                if (HoldCtrlForSpeed != 0f) if (Input.GetKey(KeyCode.LeftControl)) spd = HoldCtrlForSpeed;
            }

            float accel = 5f * MovementSpeed;
            if (!moving) accel = 7f * MovementSpeed;
            if (Interia < 1f)
            {
                //Debug.Log("currentWorldAccel: " + currentWorldAccel);
                //Debug.Log("moveDirectionWorld: " + moveDirectionWorld);
                //Debug.Log("spd: " + spd);
                //Debug.Log("Interia: " + Interia);
                currentWorldAccel = Vector3.Lerp(Vector3.Slerp(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel), Vector3.MoveTowards(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel), Interia);
                //Debug.Log("Small: " + currentWorldAccel.ToString());
            }
            else
            {
                //Debug.Log("currentWorldAccel: " + currentWorldAccel);
                //Debug.Log("moveDirectionWorld: " + moveDirectionWorld);
                //Debug.Log("spd: " + spd);
                //Debug.Log("Interia: " + Interia);
                currentWorldAccel = Vector3.MoveTowards(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel);
                //Debug.Log("Larger: " + currentWorldAccel.ToString());
            }

            // Add code
            if (!standDown)
            {
                transform.localPosition += currentWorldAccel * 0.001f;
                transform.rotation = targetInstantRotation;
            }
            else
            {
                transform.rotation = targetInstantRotation;
            }

            //if (Mecanim) if (moving) Mecanim.SetFloat("Speed", currentWorldAccel.magnitude);
            //Debug.Log(currentWorldAccel.magnitude);
            moveDirectionWorld = Vector3.zero;
        }

        public void setSpeed(float spd)
        {
            if (Mecanim)
            {
                if (spd==0) Mecanim.SetBool("Moving", false);
                else
                {
                    Mecanim.SetBool("Moving", true);
                    Mecanim.SetFloat("Speed", spd);
                }
                
            }
        }


        private void FixedUpdate()
        {
            if (Rigb == null) return;

            Vector3 targetVelo = currentWorldAccel;


            float yAngleDiff = Mathf.DeltaAngle(Rigb.rotation.eulerAngles.y, targetInstantRotation.eulerAngles.y);
            float directMovement = DirectMovement;

            directMovement *= Mathf.Lerp(1f, Mathf.InverseLerp(180f, 50f, Mathf.Abs(yAngleDiff)), Interia);

            targetVelo = Vector3.Lerp(targetVelo, (transform.forward) * targetVelo.magnitude, directMovement);
            targetVelo.y = Rigb.velocity.y;

            toJump -= Time.fixedDeltaTime;

            if (jumpRequest != 0f && toJump <= 0f)
            {
                Rigb.position += transform.up * jumpRequest * 0.01f;
                targetVelo.y = jumpRequest;
                isGrounded = false;
                jumpRequest = 0f;
                jumpTime = Time.time;
                if (Mecanim) Mecanim.SetBool("Grounded", false);
                if (OnJump != null) OnJump.Invoke();
            }
            else
            {
                if (isGrounded) // Basic not recommended but working solution - snapping to the ground (this approach will push player down quick when loosing ground)
                {
                    targetVelo.y -= 2.5f * Time.fixedDeltaTime;
                }
            }

            if (wasRootmotion == false)
            {
                //if (Rigb.isKinematic == false) Rigb.velocity = targetVelo;
                Mecanim.speed = speedAnim;
                if ( Rigb.isKinematic == false) Rigb.velocity = new Vector3(0, 0, 0);
            }

            if (FixedRotation)
                Rigb.rotation = targetRotation;
            else
                Rigb.angularVelocity = FEngineering.QToAngularVelocity(Rigb.rotation, targetRotation, true);

            if (Time.time - jumpTime > 0.2f)
            {
                CheckGroundedState();
            }
            else
            {
                if (isGrounded == true)
                {
                    isGrounded = false;
                    if (Mecanim) Mecanim.SetBool("Grounded", false);
                }
            }
        }

        public void CheckGroundedState()
        {
            if (DoRaycasting())
            {
                if (isGrounded == false)
                {
                    isGrounded = true;
                    if (Mecanim) Mecanim.SetBool("Grounded", true);
                }
            }
            else
            {
                if (isGrounded == true)
                {
                    isGrounded = false;
                    if (Mecanim) Mecanim.SetBool("Grounded", false);
                }
            }
        }

        bool wasRootmotion = false;
        private void OnAnimatorMove()
        {
            if (DisableRootMotion) return;
            if (Mecanim.deltaPosition.magnitude > float.Epsilon) wasRootmotion = true; else wasRootmotion = false;
            Mecanim.ApplyBuiltinRootMotion();
        }

        bool DoRaycasting()
        {
            if (RaycastRadius <= 0f)
            {
                return Physics.Raycast(transform.position + transform.up, -transform.up, (isGrounded ? 1.2f : 1.01f) + ExtraRaycastDistance, GroundMask, QueryTriggerInteraction.Ignore);
            }
            else
            {
                return Physics.SphereCast(new Ray(transform.position + transform.up, -transform.up), RaycastRadius, (isGrounded ? 1.2f : 1.01f) + ExtraRaycastDistance - RaycastRadius * 0.5f, GroundMask, QueryTriggerInteraction.Ignore);
            }
        }

        float jumpTime = -1f;

        /// <summary>
        /// Step length 0->1
        /// 0 idle => min length
        /// 1 run => max length
        /// </summary>
        /// <param name="stepLength"></param>
        public void UpdateStepLength(float stepLength)
        {
            this.stepLength = stepLength < 0 ? 0 : stepLength > 1 ? 1 : stepLength;
        }
        
        public void UpdateSpeedAnim(float speedAnim)
        {
            this.speedAnim = speedAnim < 0 ? 0 : speedAnim;
        }
        public void OnPressure(int side)
        {
            //UIManager._instance.uiPressure.textDebug.text="";
            UIManager._instance.uiPressure.ResetHeightGrid();
            if (side == 0)
            {
                UIManager._instance.uiPressure.gridLeftTiles[15,8].OnClicked(80,0);
                UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(80, 0);
                UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(80, 0);
                UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(80, 0);
            }
            else
            {
                UIManager._instance.uiPressure.gridRightTiles[16,8].OnClicked(80,1);
                UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(80, 1);
                UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(80, 1);
                UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(80, 1);
            }
            
        }
        public void OnPressureWalkZero(int side)
        {
            //UIManager._instance.uiPressure.textDebug.text="";
            UIManager._instance.uiPressure.ResetHeightGrid();
            if (side == 0)
            {
                //right 60
                UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(60, 1);
                UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(60, 1);
                UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(60, 1);
                UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(60, 1);

                //left 80
                UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(80, 0);
                UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(80, 0);
                UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(80, 0);
                UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(80, 0);
            }
            else
            {
                //right 80
                UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(80, 1);
                UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(80, 1);
                UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(80, 1);
                UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(80, 1);

                //left 60
                UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(60, 0);
                UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(60, 0);
                UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(60, 0);
                UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(60, 0);
            }
            
        }
    }
   
}