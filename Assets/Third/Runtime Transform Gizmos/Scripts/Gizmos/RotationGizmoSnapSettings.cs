using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds snap settings for a rotation gizmo.
    /// </summary>
    [Serializable]
    public class RotationGizmoSnapSettings
    {
        #region Private Variables
        /// <summary>
        /// The rotation snap step value in degrees. When snapping is turned on, rotations will 
        /// be performed in increments of this value. That is, whenever the accumulated rotation
        /// becomes >= than this value, a rotation will be applied to the rotation gizmo and the
        /// objects that it controls.
        /// </summary>
        [SerializeField]
        private float _stepValueInDegrees = 15.0f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// The minimum value of the degree step.
        /// </summary>
        public static float MinStepValue { get { return 0.1f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the step value in degrees. The minimum value that this variable can have is given
        /// by the 'MinStepValue' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float StepValueInDegrees { get { return _stepValueInDegrees; } set { _stepValueInDegrees = Mathf.Max(MinStepValue, value); } }
        #endregion
    }
}
