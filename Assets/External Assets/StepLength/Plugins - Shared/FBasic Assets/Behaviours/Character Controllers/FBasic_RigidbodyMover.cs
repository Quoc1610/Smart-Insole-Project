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
        public float speedAnim = 0;
        public Rigidbody Rigb;
        public Joystick movementJoystick = null;
        public UIController uiController = null;
        public float weight = 70f;

        [Space(4)]
        public float MovementSpeed = 2f;

        [Range(0f, 1f)] public float Interia = 1f;

        [Space(10)]
        [Tooltip("Setting 'Grounded','Moving' and 'Speed' parameters for mecanim")]
        public Animator Mecanim;

        [Tooltip("Animator property which will not allowing character movement is set to true")]
        public string IsBusyProperty = "";

        [Space(6)]
        public bool UpdateInput = true;

        void Start()
        {
            if (!Rigb) Rigb = GetComponent<Rigidbody>();

            if (Rigb)
            {
                Rigb.maxAngularVelocity = 30f;
                Rigb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            isGrounded = true;
            targetInstantRotation = transform.rotation;

            if (Mecanim) Mecanim.SetBool("Grounded", true);
            if (isAutoMode) MovementSpeed = 0f;
        }


        // Movement Calculation Params

        [NonSerialized] public Vector2 moveDirectionLocal = Vector3.zero;

        public float moveDirectionLocalZ = 0;
        public Vector2 moveDirectionLocalNonZero { get; private set; }

        public Vector3 moveDirectionWorld { get; private set; }
        public Vector3 currentWorldAccel { get; private set; }

        //Quaternion targetRotation;
        Quaternion targetInstantRotation;

        private Vector3 rotationAdjustment = Vector3.zero;

        public bool isGrounded { get; private set; } = true;

        public void changeRotation(float rotateValue)
        {
            rotationAdjustment.y = rotateValue;
        }

        public void setSpeed(float spd)
        {
            MovementSpeed = spd;
        }

        public bool isAutoMode = false;

        protected virtual void Update()
        {

            if (Rigb == null) return;
            if (!isAutoMode)
            {
                manualMode();
            }
            else
            {
                transform.Rotate(rotationAdjustment * Time.deltaTime);
                bool isMoving = false;
                if (MovementSpeed > 0f)
                {
                    isMoving = true;
                    //transform.localPosition += transform.forward * (MovementSpeed * transform.localScale.magnitude) * Time.deltaTime;
                }
                if (Mecanim)
                {
                    Mecanim.SetBool("Moving", isMoving);
                    Mecanim.SetFloat("Speed", MovementSpeed);
                }
            }
            
        }

        void manualMode()
        {
            bool updateMovement = true;
            bool standDown = false;
            if (Mecanim) if (string.IsNullOrWhiteSpace(IsBusyProperty) == false) updateMovement = !Mecanim.GetBool(IsBusyProperty);
            if (UpdateInput && updateMovement)
            {

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
                if (rotationMovement.x != 0 || rotationMovement.z != 0) targetInstantRotation = Quaternion.LookRotation(rotationMovement);
            }
            else if (updateMovement == false) moveDirectionWorld = Vector3.zero;



            bool moving = false;
            if (moveDirectionWorld != Vector3.zero)
            {
                moving = true;
            }

            if (Mecanim) Mecanim.SetBool("Moving", moving);

            float spd = MovementSpeed;


            float accel = 5f * MovementSpeed;
            if (!moving) accel = 7f * MovementSpeed;
            if (Interia < 1f)
            {
                currentWorldAccel = Vector3.Lerp(Vector3.Slerp(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel), Vector3.MoveTowards(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel), Interia);
            }
            else
            {
                currentWorldAccel = Vector3.MoveTowards(currentWorldAccel, moveDirectionWorld * spd, Time.deltaTime * accel);
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

            if (Mecanim) if (moving) Mecanim.SetFloat("Speed", currentWorldAccel.magnitude);
            moveDirectionWorld = Vector3.zero;
        }

        int convertPressure(float p)
        {
            float result = (p / (float)((weight * 2.5 * 10) / 1000)) * 100;
            return (int)Mathf.Round(result);
        }
        public void OnReceivePressure(int side, float[] pressureMaps)
        {
            UIManager._instance.uiPressure.ResetHeightGrid(side);
            if (side == 0)
            {
                UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(convertPressure(pressureMaps[3]), 0);
                UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(convertPressure(pressureMaps[0]), 0);
                UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(convertPressure(pressureMaps[2]), 0);
                UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(convertPressure(pressureMaps[1]), 0);
            }
            else
            {
                UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(convertPressure(pressureMaps[0]), 1);
                UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(convertPressure(pressureMaps[1]), 1);
                UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(convertPressure(pressureMaps[3]), 1);
                UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(convertPressure(pressureMaps[2]), 1);
            }
        }

        public void OnPressure(int side)
        {
            //UIManager._instance.uiPressure.textDebug.text="";
            //UIManager._instance.uiPressure.ResetHeightGrid();
            //if (side == 0)
            //{
            //    UIManager._instance.uiPressure.gridLeftTiles[15,8].OnClicked(80,0);
            //    UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(80, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(80, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(80, 0);
            //}
            //else
            //{
            //    UIManager._instance.uiPressure.gridRightTiles[16,8].OnClicked(80,1);
            //    UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(80, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(80, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(80, 1);
            //}
            
        }
        public void OnPressureWalkZero(int side)
        {
            //UIManager._instance.uiPressure.textDebug.text="";
            //UIManager._instance.uiPressure.ResetHeightGrid();
            //if (side == 0)
            //{
            //    //right 60
            //    UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(60, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(60, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(60, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(60, 1);

            //    //left 80
            //    UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(80, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(80, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(80, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(80, 0);
            //}
            //else
            //{
            //    //right 80
            //    UIManager._instance.uiPressure.gridRightTiles[16, 8].OnClicked(80, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[22, 41].OnClicked(80, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[11, 40].OnClicked(80, 1);
            //    UIManager._instance.uiPressure.gridRightTiles[6, 55].OnClicked(80, 1);

            //    //left 60
            //    UIManager._instance.uiPressure.gridLeftTiles[15, 8].OnClicked(60, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[21, 43].OnClicked(60, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[9, 42].OnClicked(60, 0);
            //    UIManager._instance.uiPressure.gridLeftTiles[25, 57].OnClicked(60, 0);
            //}
            
        }
    }
   
}