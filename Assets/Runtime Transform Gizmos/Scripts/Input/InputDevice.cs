#if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
#define INPUT_MOBILE
#else
#define INPUT_PC
#endif
using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// This class wraps all input specific functionality and its main purpose is to
    /// relieve the client code from differentiating between PC and mobile devices.
    /// </summary>
    public class InputDevice : MonoSingletonBase<InputDevice>
    {
        #region Private Variables
        /// <summary>
        /// Holds the device positions in the previous frame. For a mouse dvice, the
        /// first array element will hold the mouse position. For a mobile device, there 
        /// is an element for each possible touch.
        /// </summary>
        private Vector2[] _previousFramePositions = new Vector2[MaxNumberOfTouches];

        /// <summary>
        /// Holds the device offsets since the last frame. For a mouse device, the first 
        /// array element will hold the mouse offset. For a mobile device, there is an 
        /// element for each possible touch. For mobile, when no touches exist, all elements
        /// are set to the zero vector.
        /// </summary>
        private Vector2[] _deltaSinceLastFrame = new Vector2[MaxNumberOfTouches];

        /// <summary>
        /// For a mouse device, the first 3 elements of this array hold the mouse offset
        /// since the left, right and middle mouse buttons were pressed. For a mobile device,
        /// each array element holds the offset of the touch since the touch began. For mouse
        /// buttons or touches that are not currently active, the offset is the zero vector.
        /// </summary>
        private Vector2[] _deltaSincePressed = new Vector2[MaxNumberOfTouches];
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the maximum number of allowed touches for a mobile device. A random
        /// value of 10 was used in this case as for the moment I can not figure out how
        /// to retrieve this value using the Unity API.
        /// </summary>
        public static int MaxNumberOfTouches { get { return 10; } }
        #endregion

        #region Public Properties
        public bool UsingMobile
        {
            get
            {
                #if INPUT_MOBILE
                return true;
                #else
                return false;
                #endif
            }
        }

        public int TouchCount { get { return Input.touchCount; } }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the device offset since the specified device button was pressed.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// For a mouse, this has to be a value in the [0, 2] interval and it identifies
        /// one of the mouse buttons. For a mobile device, this value can reside in the 
        /// [0, MaxNumberOfTouches] interval and it identifies a touch.
        /// </param>
        /// <returns>
        /// The device offset since the specified device button was pressed. For example,
        /// if a mouse is used and the value passed is 0, the method will return the mouse
        /// offset since the left mouse button was pressed. For a mobile device, this will
        /// return the offset of the touch with index 'deviceButtonIndex' since the touch began. 
        /// If the specified device button is not pressed, the method will return the zero vector.
        /// The same value is returned if the specified index is out of range. The allowed
        /// interval for this value is [0, MaxNumberOfTouches - 1].
        /// </returns>
        public Vector2 GetDeltaSincePressed(int deviceButtonIndex)
        {
            if (deviceButtonIndex < 0 || deviceButtonIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSincePressed[deviceButtonIndex];
        }

        /// <summary>
        /// Returns the device offset since the last frame.
        /// </summary>
        /// <param name="deviceIndex">
        /// Identifies the device. For a mouse this has to be set to 0. Otherwise, this 
        /// represents the index of a touch.
        /// </param>
        /// <returns>
        /// The offset of the device since the last frame. When a mobile device is used,
        /// if the touch with index 'deviceIndex' is not present, the returned value is
        /// the zero vector. The same value is returned if the specified device index
        /// is out of range. The allowed interval for this value is [0, MaxNumberOfTouches - 1].
        /// </returns>
        public Vector2 GetDeltaSinceLastFrame(int deviceIndex)
        {
            if (deviceIndex < 0 || deviceIndex >= MaxNumberOfTouches) return Vector2.zero;
            return _deltaSinceLastFrame[deviceIndex];
        }

        /// <summary>
        /// Can be used to check if the device button with the specified index is
        /// currently pressed.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For a mobile, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public bool IsPressed(int deviceButtonIndex)
        {
            #if INPUT_MOBILE
            return deviceButtonIndex < Input.touchCount;
            #else
            return Input.GetMouseButton(deviceButtonIndex);
            #endif
        }

        /// <summary>
        /// Can be used to check if the device button with the specified index was
        /// pressed in the current frame.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For a mobile, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public bool WasPressedInCurrentFrame(int index)
        {
            #if INPUT_MOBILE
            if (index >= Input.touchCount) return false;
            return Input.GetTouch(index).phase == TouchPhase.Began;
            #else
            return Input.GetMouseButtonDown(index);
            #endif
        }

        /// <summary>
        /// Can be used to check if the device button with the specified index was
        /// released in the current frame.
        /// </summary>
        /// <param name="deviceButtonIndex">
        /// Identifies the device button. For a mouse this has to reside in the 
        /// [0, 2] interval (left, right, middle mouse button). For a mobile, this
        /// is the index of a touch.
        /// </param>
        /// <returns>
        /// True if the specified button is pressed and false otherwise.
        /// </returns>
        public bool WasReleasedInCurrentFrame(int index)
        {
            #if INPUT_MOBILE
            if (index >= Input.touchCount) return false;
            Touch touch = Input.GetTouch(index);
            return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
            #else
            return Input.GetMouseButtonUp(index);
            #endif
        }

        /// <summary>
        /// Returns the device position. For a mouse this is the mouse cursor position
        /// and for a mobile device this is the position of the first touch.
        /// </summary>
        /// <param name="position">
        /// The device position. If the method returns false, this position will have
        /// both components set to 'float.MaxValue' and should be ignored.
        /// </param>
        /// <returns>
        /// True if the position can be retreived and false otherwise. The method can
        /// return false if a mobile device is used and no touches are available.
        /// </returns>
        public bool GetPosition(out Vector2 position)
        {
            #if INPUT_MOBILE
            position = new Vector2(float.MaxValue, float.MaxValue);
            if (Input.touchCount != 0) 
            {
                position = Input.GetTouch(0).position;
                return true;
            }
            return false;
            #else
            position = Input.mousePosition;
            return true;
            #endif
        }

        /// <summary>
        /// Returns a pick ray from the device position. This ray can be used to pick entities
        /// in the scene. For a mouse device the ray is constructed using the mouse cursor
        /// position. For a mobile device, the ray is constructed using the first available
        /// touch.
        /// </summary>
        /// <param name="ray">
        /// The pick ray. If the method returns false, this ray will have a zero vector origin
        /// and a zero vector direction and should be ignored.
        /// </param>
        /// <returns>
        /// True if the ray can be constructed and false otherwise. The method can return false
        /// if a mobile device is used and no touches are available.
        /// </returns>
        public bool GetPickRay(Camera camera, out Ray ray)
        {
            #if INPUT_MOBILE
            ray = new Ray(Vector3.zero, Vector3.zero);
            if (Input.touchCount != 0)
            {
                Touch touch = Input.GetTouch(0);
                ray = camera.ScreenPointToRay(touch.position);
                return true;
            }
            return false;
            #else
            ray = camera.ScreenPointToRay(Input.mousePosition);
            return true;
            #endif
        }

        /// <summary>
        /// Can be used to check if the device was moved. For a mouse device, this
        /// method will return true when the mouse cursor is moved. For a mobile
        /// device it returns true only if the first touch was moved.
        /// </summary>
        /// <returns></returns>
        public bool WasMoved()
        {
            #if INPUT_MOBILE
            if (Input.touchCount != 0)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Moved;
            }
            return false;
            #else
            return Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f;
            #endif
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Called every frame to perform any necessary updates.
        /// </summary>
        private void Update()
        {
            #if MOBILE_INPUT
            int touchCount = Input.touchCount;
            for (int touchIndex = 0; touchIndex < touchCount; ++touchIndex)
            {
                if(touchIndex >= touchCount) 
                {
                    _deltaSinceLastFrame[0] = Vector2.zero;
                    _previousFramePositions[0] = Vector2.zero;
                    continue;
                }

                // If the touch has begun or ended in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(touchIndex) ||
                    WasReleasedInCurrentFrame(touchIndex))
                {
                    _deltaSincePressed[touchIndex] = Vector2.zero;
                    continue;
                }
                else
                {
                    // If the touch is still active, we will add the delta since the last frame to get the
                    // delta since it was pressed.
                    if (IsPressed(touchIndex)) _deltaSincePressed[touchIndex] += _deltaSinceLastFrame[touchIndex];
                }
            }
            #else
            // Calculate the mouse delta
            Vector2 mousePos = Input.mousePosition;
            _deltaSinceLastFrame[0] = mousePos - _previousFramePositions[0];

            // Store the current mouse position as the previous position for the next frame
            _previousFramePositions[0] = mousePos;

            // Now we will loop through all possible mouse buttons and update the mouse offset
            // since each button was pressed.
            for(int mouseBtnIndex = 0; mouseBtnIndex < 3; ++mouseBtnIndex)
            {
                // If the button was pressed or released in the current frame, there is nothing to
                // do except to reset the corresponding offset to the zero vector.
                if (WasPressedInCurrentFrame(mouseBtnIndex) ||
                    WasReleasedInCurrentFrame(mouseBtnIndex))
                {
                    _deltaSincePressed[mouseBtnIndex] = Vector2.zero;
                    continue;
                }
                else
                {
                    // If the mouse button wasn't pressed or released, we will check to see if it is
                    // still pressed (it may have been pressed in a previous frame). If it is, we will
                    // add the delta since the last frame to get the delta since it was pressed.
                    if (IsPressed(mouseBtnIndex)) _deltaSincePressed[mouseBtnIndex] += _deltaSinceLastFrame[0];
                }
            }
            #endif
        }
        #endregion
    }
}
