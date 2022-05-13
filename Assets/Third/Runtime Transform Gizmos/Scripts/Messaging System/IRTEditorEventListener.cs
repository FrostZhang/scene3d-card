using UnityEngine;

namespace RTEditor
{
    /// <summary>
    /// Monobehaviours can implement this interface so that they can be able to 
    /// listen to different types of events and take action as needed.
    /// </summary>
    public interface IRTEditorEventListener
    {
        /// <summary>
        /// Called before an object is about to be selected. Must return true if the
        /// object can be selected and false otherwise.
        /// </summary>
        bool OnCanBeSelected(ObjectSelectEventArgs selectEventArgs);

        /// <summary>
        /// Called when the object has been selected.
        /// </summary>
        void OnSelected(ObjectSelectEventArgs selectEventArgs);

        /// <summary>
        /// Called when the object has been deselected.
        /// </summary>
        void OnDeselected(ObjectDeselectEventArgs deselectEventArgs);

        /// <summary>
        /// Called when the object is altered (moved, rotated or scaled) by a transform gizmo.
        /// </summary>
        /// <param name="gizmo">
        /// The transform gzimo which alters the object.
        /// </param>
        void OnAlteredByTransformGizmo(Gizmo gizmo);
    }
}
