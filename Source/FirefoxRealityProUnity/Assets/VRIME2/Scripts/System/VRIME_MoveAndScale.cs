using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using System.Collections.Generic;

namespace VRIME2
{
    public class VRIME_MoveAndScale : MonoBehaviour
    {
		public static VRIME_MoveAndScale Ins {
			get { return instance; }
			set {
				instance = value;				
			}
		}
		private static VRIME_MoveAndScale instance;

        [HideInInspector]
        public int leftControllerIndex;
        [HideInInspector]
        public int rightControllerIndex;

        [HideInInspector]
        public Transform leftControllerTransform;
        [HideInInspector]
        public Transform rightControllerTransform;


        enum State
        {
            Idle,
            Move,
            Rotate,
            Scale,
        }

        private State _state = State.Idle;

        // Move
        private Transform _moveControllerTransform;
        private Transform _idleControllerTransform;

        private int _moveControllerIndex;
        private int _idleControllerIndex;        

        private Vector3 _positionOffsetFromController;
        private Quaternion _rotationOffsetFromController;

        // Scale
        private Vector3 _positionOffset;
        private Quaternion _rotationOffset;
        private Vector3 _scaleOffset;

        // Animation
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private Vector3 _targetScale;

        public GameObject[] targetMove;
        public GameObject[] targetRotate;
        public GameObject[] targetScale;

        public MeshRenderer[] meshRenderers;
        public Material outlineMaterial;
        [HideInInspector]
        public Transform trackedObject;

        private const float limitDistanceLong = 0.9f;
        private const float limitDistanceShort = 0.5f;
        private const float limitHeightHigh = 0.975f;
        private const float limitHeightLow = 0.61f;
        private Vector3 limitScaleLarge = new Vector3(1.94f, 1.94f, 1.94f);
        private Vector3 limitScaleSmall = new Vector3(0.417f, 0.417f, 0.417f);
        private const float limitAngleFlat = 10f;
        private const float limitAngleStand = 320f;

        public enum DetectCollider
        {
            RotateLeft,
            RotateRight,
            ScaleLeftUp,
            ScaleLeftDown,
            ScaleRightUp,
            ScaleRightDown,
        }

        public bool rotateLeft = false;
        public bool rotateRight = false;

        public bool scaleLeftUp = false;
        public bool scaleLeftDown = false;
        public bool scaleRightUp = false;
        public bool scaleRightDown = false;

        public void SetDetectCollider(DetectCollider detectCollider, bool value)
        {
            switch (detectCollider)
            {
                case DetectCollider.RotateLeft:
                    rotateLeft = value;
                    break;
                case DetectCollider.RotateRight:
                    rotateRight = value;
                    break;
                case DetectCollider.ScaleLeftUp:
                    scaleLeftUp = value;
                    break;
                case DetectCollider.ScaleLeftDown:
                    scaleLeftDown = value;
                    break;
                case DetectCollider.ScaleRightUp:
                    scaleRightUp = value;
                    break;
                case DetectCollider.ScaleRightDown:
                    scaleRightDown = value;
                    break;
            }
        }

        #if steamvr_v2
        // for steamvr v2
        bool isLeftGrip = false;
        bool isRightGrip = false;

        public void GripChangeHandler(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState) {
            if(fromSource == SteamVR_Input_Sources.LeftHand) {
                isLeftGrip = newState;
            } else if(fromSource == SteamVR_Input_Sources.RightHand) {
                isRightGrip = newState;
            }
        }
        #endif

        void OnEnable()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _targetScale = transform.localScale;

            //Debug.LogError("transform.position = " + transform.position);
            //Debug.LogError("transform.localPosition = " + transform.localPosition);
            //Debug.LogError("transform.rotation = " + transform.rotation);
            //Debug.LogError("transform.localRotation = " + transform.localRotation);
            //Debug.LogError("transform.localEulerAngles = " + transform.localEulerAngles);
            //Debug.LogError("transform.localScale = " + transform.localScale);
        }
        // 這段可能是在測移動之後的結果，並且輸出到TextInfo上
        // 因為用不到了所以註解掉
        // public Text textInfo;
        // void OnDisable()
        // {
            // string info = "trackedObject.position = " + trackedObject.position.ToString("F3") + "\n" +
            //     "transform.position = " + transform.position.ToString("F3") + "\n" +
            //     "trackedObject.position - transform.position = " + (trackedObject.position - transform.position).ToString("F3") + "\n" +
            //     "Vector3.Distance(trackedObject.position, transform.position) = " + Vector3.Distance(trackedObject.position, transform.position).ToString("F3") + "\n" +
            //     "transform.localScale = " + transform.localScale.ToString("F3") + "\n" +
            //     "transform.rotation = " + transform.rotation.ToString("F3") + "\n" +
            //     "transform.eulerAngles = " + transform.eulerAngles.ToString("F3") + "\n" +
            //     "transform.localRotation = " + transform.localRotation.ToString("F3") + "\n" +
            //     "transform.localEulerAngles = " + transform.localEulerAngles.ToString("F3") + "\n";

            // Debug.LogError("info = " + info);
            // textInfo.text = info;

            //Debug.LogError("trackedObject.position = " + trackedObject.position);
            //Debug.LogError("transform.position = " + transform.position);
            //Debug.LogError("trackedObject.position - transform.position = " + (trackedObject.position - transform.position));
            //Debug.LogError("transform.localScale = " + transform.localScale);
            //Debug.LogError("transform.rotation = " + transform.rotation);
            //Debug.LogError("transform.eulerAngles = " + transform.eulerAngles);
            //Debug.LogError("transform.localRotation = " + transform.localRotation);
            //Debug.LogError("transform.localEulerAngles = " + transform.localEulerAngles);
        // }

        void Update()
        {
            HandleGripState();
        }

        void HandleGripState()
        {
            // Run the correct update operation for each state. Check whether the current state is valid.
            if (_state == State.Idle)
            {
                BeginMoveOrScaleIfNeeded();

            }
            else if (_state == State.Move)
            {
                bool moveGrip = GetGrip(_moveControllerIndex);
                bool idleGrip = GetGrip(_idleControllerIndex);

                // Do we need to transition to scaling or rotating? Are we still moving?
                if(moveGrip && idleGrip && _moveControllerIndex == _idleControllerIndex) {
                    // special case, for only one controller, these controller will be the same value
                    // Continue moving
                    Move();
                }
                else if (moveGrip && !idleGrip)
                {
                    // Continue moving
                    Move();
                }
                else
                {
                    // Stop moving
                    EndMove();

                    // Begin scaling or begin moving with the opposite hand if needed
                    BeginMoveOrScaleIfNeeded();
                }

            }
            else if (_state == State.Rotate)
            {
                bool leftGrip = GetGrip(leftControllerIndex);
                bool rightGrip = GetGrip(rightControllerIndex);

                // Do we need to transition to moving or scaling? Are we still rotating?
                if (leftGrip && rightGrip)
                {
                    // Continue scaling
                    Rotate();
                }
                else
                {
                    // Stop scaling
                    EndRotate();

                    //two hand grip become one hand
                    if (leftGrip || rightGrip)
                    {
                        pressedTime = 0f;
                    }

                    // Begin other action if needed
                    BeginMoveOrScaleIfNeeded();
                }
            }
            else if (_state == State.Scale)
            {
                bool leftGrip = GetGrip(leftControllerIndex);
                bool rightGrip = GetGrip(rightControllerIndex);

                // Do we need to transition to moving or rotating? Are we still scaling?
                if (leftGrip && rightGrip)
                {
                    // Continue scaling
                    Scale();
                }
                else
                {
                    // Stop scaling
                    EndScale();

                    //two hand grip become one hand
                    if (leftGrip || rightGrip)
                    {
                        pressedTime = 0f;
                    }

                    // Begin other action if needed
                    BeginMoveOrScaleIfNeeded();
                }
            }
        }

        // public VRTK.VRTK_UICanvas UICanvas;
        private void SetFunctionState(bool enable)
        {
            GetKeyboardOutlineMesh();
            //Debug.LogError("enable = " + enable);
            //show/hide outline
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                List<Material> tmpMaterials = new List<Material>(meshRenderers[i].materials);
                if (enable)
                {
                    if (tmpMaterials.Count < 2)
                    {
                        tmpMaterials.Add(outlineMaterial);
                    }
                }
                else
                {
                    if (tmpMaterials.Count > 1)
                    {
                        tmpMaterials.RemoveAt(tmpMaterials.Count - 1);
                    }
                }
                meshRenderers[i].materials = tmpMaterials.ToArray();
            }

            //enable/disable keyboard function
            VRIME_KeyboardSetting.BlockDrumstick = enable;
            // VRIME2 Add
            VRIME2.VRIME_KeyboardSetting.BlockDrumstick = enable;
        }
        /// <summary>
        /// meshRenderers要有東西，outline才能掛上去
        /// meshRenderers指的是整組鍵盤
        /// </summary>
        private void GetKeyboardOutlineMesh()
        {
            if(meshRenderers.Length > 0)
                return;
            if(VRIME_KeyboardOversee.Ins == null)
                return;
            
            MeshRenderer aUsing = VRIME_KeyboardOversee.Ins.UsingPanelMeshRender;

            meshRenderers = new MeshRenderer[]{ aUsing };
        }

        private float pressedTime = 0f;
        const float intervalLimit = 0.1f;

        // Move / Scale state change
        void BeginMoveOrScaleIfNeeded()
        {
            bool leftGrip = GetGrip(leftControllerIndex);
            bool rightGrip = GetGrip(rightControllerIndex);

            //if (leftGrip || rightGrip)
            //{
            //    SetFunctionState(true);
            //}
            if (leftGrip && rightGrip && leftControllerIndex == rightControllerIndex) {
                // special case for only one controller
                 SetFunctionState(true);
                pressedTime = 0f;
                if(leftControllerIndex == ((int)Valve.VR.ETrackedControllerRole.LeftHand)) { 
                    BeginMove(rightControllerIndex, leftControllerIndex, rightControllerTransform, leftControllerTransform);
                } else {
                    BeginMove(leftControllerIndex, rightControllerIndex, leftControllerTransform, rightControllerTransform);
                }

                
            }
            else if (leftGrip && rightGrip)
            {
                SetFunctionState(true);
                pressedTime = 0f;
            }
            else if (!leftGrip && !rightGrip)
            {
                SetFunctionState(false);
                pressedTime = 0f;
            }
            else
            {
                pressedTime += Time.deltaTime;
            }

            if (leftGrip && rightGrip)
            {
                if ((scaleLeftDown && scaleRightUp) || (scaleLeftUp && scaleRightDown))
                {
                    BeginScale();
                }
                else if (rotateLeft && rotateRight)
                {
                    BeginRotate();
                }
            }
            else if (leftGrip && (pressedTime > intervalLimit))
            {
                SetFunctionState(true);
                pressedTime = 0f;
                BeginMove(leftControllerIndex, rightControllerIndex, leftControllerTransform, rightControllerTransform);
            }
            else if (rightGrip && (pressedTime > intervalLimit))
            {
                SetFunctionState(true);
                pressedTime = 0f;
                // Begin moving with the right controller.
                BeginMove(rightControllerIndex, leftControllerIndex, rightControllerTransform, leftControllerTransform);
            }
        }

        // Move
        void BeginMove(int moveControllerIndex, int idleControllerIndex, Transform moveControllerTransform, Transform idleControllerTransform)
        {
            _state = State.Move;
            _moveControllerIndex = moveControllerIndex;
            _idleControllerIndex = idleControllerIndex;
            _moveControllerTransform = moveControllerTransform;
            _idleControllerTransform = idleControllerTransform;

            // Save current position / rotation offset
            _positionOffsetFromController = _moveControllerTransform.InverseTransformPoint(transform.position);
            //_rotationOffsetFromController = Quaternion.Inverse(_moveController.transform.rotation) * transform.rotation;

            Move();
        }

        void Move()
        {
            if (_state != State.Move)
                return;

            // Take the current orientation of the controller, ask it to convert our offsets from earlier to world space.
            // This will apply any position/rotation changes that have happened since we started grabbing.
            // If the hand hasn't moved at all, we should end up positioning the object exactly where it started.
            _targetPosition = _moveControllerTransform.TransformPoint(_positionOffsetFromController);

            //check limit
            float distance = Vector3.Distance(trackedObject.position, _targetPosition);
            Vector3 v = _targetPosition - trackedObject.position;
            if (distance > limitDistanceLong)
            {
                _targetPosition = v.normalized * limitDistanceLong + trackedObject.position;
            }
            else if (distance < limitDistanceShort)
            {
                _targetPosition = v.normalized * limitDistanceShort + trackedObject.position;
            }

            for (int i = 0; i < targetMove.Length; i++)
            {
                targetMove[i].transform.position = _targetPosition;
            }

            Vector3 position = new Vector3(trackedObject.position.x, transform.position.y, trackedObject.position.z);

            for (int i = 0; i < targetMove.Length; i++)
            {
                Vector3 orilocal = targetMove[i].transform.localEulerAngles;
                targetMove[i].transform.rotation = Quaternion.LookRotation(transform.position - position);
                targetMove[i].transform.localEulerAngles = new Vector3(orilocal.x, targetMove[i].transform.localEulerAngles.y, targetMove[i].transform.localEulerAngles.z);
            }
            // VRIME2 Add
            // 目前第一頁的說明就是Move，所以直接呼叫換頁
            if(VRIME2.VRIME_Manager.Ins.ShowState)
            if(VRIME2.VRIME_KeyboardOversee.Ins != null)
                VRIME2.VRIME_KeyboardOversee.Ins.TooltipMove();
        }

        void EndMove()
        {
            _state = State.Idle;
            _moveControllerTransform = null;
            _idleControllerTransform = null;
            _positionOffsetFromController = Vector3.zero;
            //_rotationOffsetFromController = Quaternion.identity;

            SetFunctionState(false);
        }

        GameObject drumstick;
        GameObject rotateCenter;
        //Vector3 preAngle;
        //Quaternion start;
        //float initRotation;
        // Rotate
        void BeginRotate()
        {
            _state = State.Rotate;

            foreach (Transform child in rightControllerTransform)
            {
                //child is your child transform
                if (child.gameObject.layer == LayerMask.NameToLayer("Controller"))
                {
                    drumstick = child.gameObject;
                    break;
                }
            }

            rotateCenter = drumstick.transform.Find("HandPosition").gameObject;

            // Save rotation offset
            _rotationOffsetFromController = Quaternion.Inverse(rotateCenter.transform.rotation) * transform.rotation;

            //start = rotateCenter.transform.rotation;
            //preAngle = rotateCenter.transform.eulerAngles;
            //initRotation = rotateCenter.transform.eulerAngles.x;

            Rotate();
        }

        void Rotate()
        {
            if (_state != State.Rotate)
                return;

            //float currentRotation = rotateCenter.transform.eulerAngles.x;
            _targetRotation = (rotateCenter.transform.rotation * _rotationOffsetFromController);

            //Vector3 offset = rotateCenter.transform.eulerAngles - preAngle;
            //preAngle = rotateCenter.transform.eulerAngles;
            //string info = "";

            for (int i = 0; i < targetMove.Length; i++)
            {
                Vector3 orilocal = targetMove[i].transform.localEulerAngles;
                //info += "targetMove[i].transform.rotation = " + targetMove[i].transform.rotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localRotation = " + targetMove[i].transform.localRotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.eulerAngles = " + targetMove[i].transform.eulerAngles.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localEulerAngles = " + targetMove[i].transform.localEulerAngles.ToString("F3") + "\n";
                //info += "=====\n";
                //Quaternion orilocalRotation = targetMove[i].transform.localRotation;
                targetMove[i].transform.rotation = _targetRotation;
                //info += "targetMove[i].transform.rotation = " + targetMove[i].transform.rotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localRotation = " + targetMove[i].transform.localRotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.eulerAngles = " + targetMove[i].transform.eulerAngles.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localEulerAngles = " + targetMove[i].transform.localEulerAngles.ToString("F3") + "\n";
                //info += "=====\n";
                //targetMove[i].transform.localRotation = Quaternion.Euler(targetMove[i].transform.localEulerAngles.x - orilocal.x, orilocal.y, orilocal.z);

                //check limit
                float tmpX;
                if (targetMove[i].transform.localEulerAngles.x < limitAngleStand && targetMove[i].transform.localEulerAngles.x > 180)
                {
                    tmpX = limitAngleStand;
                }
                else if (targetMove[i].transform.localEulerAngles.x > limitAngleFlat && targetMove[i].transform.localEulerAngles.x < 180)
                {
                    tmpX = limitAngleFlat;
                }
                else
                {
                    tmpX = targetMove[i].transform.localEulerAngles.x;
                }

                targetMove[i].transform.localRotation = Quaternion.Euler(tmpX, orilocal.y, orilocal.z);
                //info += "targetMove[i].transform.rotation = " + targetMove[i].transform.rotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localRotation = " + targetMove[i].transform.localRotation.ToString("F3") + "\n";
                //info += "targetMove[i].transform.eulerAngles = " + targetMove[i].transform.eulerAngles.ToString("F3") + "\n";
                //info += "targetMove[i].transform.localEulerAngles = " + targetMove[i].transform.localEulerAngles.ToString("F3") + "\n";
                //targetMove[i].transform.rotation = Quaternion.Euler(_targetRotation.eulerAngles.x, targetMove[i].transform.eulerAngles.y, targetMove[i].transform.eulerAngles.z);
                //targetMove[i].transform.rotation = Quaternion.AngleAxis(Quaternion.Angle(start, rotateCenter.transform.rotation), targetMove[i].transform.right);
                //targetMove[i].transform.localEulerAngles = new Vector3(targetMove[i].transform.localEulerAngles.x, orilocal.y, orilocal.z);
                //targetMove[i].transform.RotateAround(transform.position, transform.up, currentRotation - initRotation);
                //Debug.LogError("currentRotation = " + currentRotation);
                //Debug.LogError("initRotation = " + initRotation);
                //Debug.LogError("currentRotation - initRotation = " + (currentRotation - initRotation));
                //targetMove[i].transform.localEulerAngles = new Vector3(targetMove[i].transform.localEulerAngles.x + (currentRotation - initRotation), targetMove[i].transform.localEulerAngles.y, targetMove[i].transform.localEulerAngles.z);
                //targetMove[i].transform.Rotate(new Vector3(currentRotation - initRotation, 0, 0), Space.Self);
                //targetMove[i].transform.RotateAround(transform.position, transform.right, currentRotation - initRotation);
                //targetMove[i].transform.Rotate(new Vector3(offset.x, 0, 0), Space.Self);
                //float angle = Quaternion.Angle(start, rotateCenter.transform.rotation);
                //targetMove[i].transform.Rotate(new Vector3(angle, 0, 0), Space.Self);
            }

            //textInfo.text = info;
            //start = rotateCenter.transform.rotation;
        }

        void EndRotate()
        {
            _state = State.Idle;
            _rotationOffsetFromController = Quaternion.identity;

            SetFunctionState(false);
        }

        List<GameObject> drumstickScale;
        // Scale
        void BeginScale()
        {
            drumstickScale = new List<GameObject>();
            foreach (Transform child in leftControllerTransform)
            {
                //child is your child transform
                if (child.gameObject.layer == LayerMask.NameToLayer("Controller"))
                {
                    drumstickScale.Add(child.gameObject);
                }
            }

            foreach (Transform child in rightControllerTransform)
            {
                //child is your child transform
                if (child.gameObject.layer == LayerMask.NameToLayer("Controller"))
                {
                    drumstickScale.Add(child.gameObject);
                }
            }

            _state = State.Scale;

            // Create a matrix for the centroid of the two controllers.
            //Matrix4x4 centroid = GetControllerCentroidTransform();

            // Get the position/rotation/scale in local space of the centroid matrix.
            //_positionOffset = centroid.inverse.MultiplyPoint(transform.position);
            //_rotationOffset = Quaternion.Inverse(GetControllerOrientation()) * transform.rotation;
            _scaleOffset = 1.0f / GetControllerDistance() * transform.localScale;
        }

        void Scale()
        {
            if (_state != State.Scale)
                return;

            // Use it to transform the offsets calculated at the start of the scale operation.
            //_targetPosition = GetControllerCentroidTransform().MultiplyPoint(_positionOffset);
            //_targetRotation = GetControllerOrientation() * _rotationOffset;
            _targetScale = GetControllerDistance() * _scaleOffset;

            //check limit
            if (_targetScale.x > limitScaleLarge.x)
            {
                _targetScale = limitScaleLarge;
            }
            else if (_targetScale.x < limitScaleSmall.x)
            {
                _targetScale = limitScaleSmall;
            }

            for (int i = 0; i < targetScale.Length; i++)
            {
                targetScale[i].transform.localScale = _targetScale;
            }

            foreach (var drumstick in drumstickScale)
            {
                drumstick.transform.localScale = _targetScale;
            }
        }

        void EndScale()
        {
            _state = State.Idle;
            SetFunctionState(false);
        }

        // SteamVR
        bool GetGrip(int controllerIndex)
        {
#if steamvr_v2
            if(controllerIndex == leftControllerIndex) {
                return isLeftGrip;
            } else if(controllerIndex == rightControllerIndex) {
                return isRightGrip;
            }
            return false;
#else            
            if(controllerIndex < 0)// If Not StemVR Case will be null.
                return false;
            if (controllerIndex == (int)Valve.VR.ETrackedControllerRole.Invalid)
                return false;

            int i = (int)controllerIndex;

            VRControllerState_t t = new VRControllerState_t();

            OpenVR.System.GetControllerState((uint)i, ref t, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VRControllerState_t)));

            if((t.ulButtonPressed & (1UL << ((int)EVRButtonId.k_EButton_Grip))) > 0) {
                return true;
            } else {
                return false;
            }

            // SteamVR_Controller.Device device = SteamVR_Controller.Input(i);

            // return device.GetPress(EVRButtonId.k_EButton_Grip);
#endif            
        }

        Vector3 GetControllerCentroid()
        {
            return (leftControllerTransform.position + rightControllerTransform.position) / 2.0f;
        }

        Quaternion GetControllerOrientation()
        {
            Vector3 direction = rightControllerTransform.position - leftControllerTransform.position;
            Vector3 up = (leftControllerTransform.forward + rightControllerTransform.forward) / 2.0f;
            return Quaternion.LookRotation(direction, up);
        }

        float GetControllerDistance()
        {
            return Vector3.Distance(leftControllerTransform.position, rightControllerTransform.position);
        }

        Matrix4x4 GetControllerCentroidTransform()
        {
            Vector3 position = GetControllerCentroid();
            Quaternion rotation = GetControllerOrientation();
            float scale = GetControllerDistance();
            Matrix4x4 centroid = Matrix4x4.TRS(position, rotation, new Vector3(scale, scale, scale));

            return centroid;
        }
    }
}