using UnityEngine;

namespace RTEditor
{
    public class ObjectDeselectEventArgs
    {
        private ObjectDeselectActionType _deselectActionType;
        private GizmoType _gizmoType;
        private bool _isGizmoActive;

        public ObjectDeselectActionType DeselectActionType { get { return _deselectActionType; } }
        public GizmoType GizmoType { get { return _gizmoType; } }
        public bool IsGizmoActive { get { return _isGizmoActive; } }

        public ObjectDeselectEventArgs(ObjectDeselectActionType deselectActionType)
        {
            _deselectActionType = deselectActionType;
            _gizmoType = EditorGizmoSystem.Instance.ActiveGizmoType;
            _isGizmoActive = !EditorGizmoSystem.Instance.AreGizmosTurnedOff;
        }
    }
}
