using UnityEngine;
using System;

namespace RTEditor
{
    /// <summary>
    /// Holds snap settings for a translation gizmo.
    /// </summary>
    /// <remarks>
    /// The translation gizmo supports 2 types of snapping: step and vertex snapping. These are
    /// mutually exclusive so activating one will deactivate the other.
    /// </remarks>
    [Serializable]
    public class TranslationGizmoSnapSettings
    {
        #region Private Variables
        /// <summary>
        /// This is the step value in world units. When step snapping is enabled, translations will be performed 
        /// in increments of this step value. That is, whenever the accumulated translation becomes >= than this 
        /// value, a translation will be applied to the translation gizmo and the objects that it controls.
        /// </summary>
        private float _stepValueInWorldUnits = 1.0f;
        #endregion

        #region Public Static Properties
        /// <summary>
        /// Returns the minimum step value.
        /// </summary>
        public static float MinStepValue { get { return 0.1f; } }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the step value in world units. The minimum value for this variable is given by the
        /// 'MinStepValue' property. Values smaller than that will be clamped accordingly.
        /// </summary>
        public float StepValueInWorldUnits { get { return _stepValueInWorldUnits; } set { _stepValueInWorldUnits = Mathf.Max(MinStepValue, value); } }
        #endregion
    }
}