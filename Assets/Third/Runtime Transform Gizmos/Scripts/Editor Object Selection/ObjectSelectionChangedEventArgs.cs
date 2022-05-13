using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public class ObjectSelectionChangedEventArgs
    {
        private ObjectSelectActionType _selectActionType;
        private List<GameObject> _selectedObjects;

        private ObjectDeselectActionType _deselectActionType;
        private List<GameObject> _deselectedObjects;

        private GizmoType _gizmoType;
        private bool _isGizmoActive;

        public ObjectSelectActionType SelectActionType { get { return _selectActionType; } }
        public List<GameObject> SelectedObjects { get { return new List<GameObject>(_selectedObjects); } }
        public ObjectDeselectActionType DeselectActionType { get { return _deselectActionType; } }
        public List<GameObject> DeselectedObjects { get { return new List<GameObject>(_deselectedObjects); } }
        public GizmoType GizmoType { get { return _gizmoType; } }
        public bool IsGizmoActive { get { return _isGizmoActive; } }

        public ObjectSelectionChangedEventArgs(ObjectSelectActionType selectActionType, List<GameObject> selectedObjects,
                                               ObjectDeselectActionType deselectActionType, List<GameObject> deselectedObjects)
        {
            _selectActionType = selectActionType;
            _selectedObjects = new List<GameObject>();
            if (selectedObjects != null) _selectedObjects = new List<GameObject>(selectedObjects);

            _deselectActionType = deselectActionType;
            _deselectedObjects = new List<GameObject>();
            if (_deselectedObjects != null) _deselectedObjects = new List<GameObject>(deselectedObjects);

            _gizmoType = EditorGizmoSystem.Instance.ActiveGizmoType;
            _isGizmoActive = !EditorGizmoSystem.Instance.AreGizmosTurnedOff;
        }

        public static ObjectSelectionChangedEventArgs FromSnapshots(ObjectSelectActionType selectActionType, ObjectDeselectActionType deselectActionType,
                                                                    ObjectSelectionSnapshot preChangeSnapshot, ObjectSelectionSnapshot postChangeSnapshot)
        {
            List<GameObject> preDiff = preChangeSnapshot.GetDiff(postChangeSnapshot);
            List<GameObject> postDiff = postChangeSnapshot.GetDiff(preChangeSnapshot);
            return new ObjectSelectionChangedEventArgs(selectActionType, postDiff, deselectActionType, preDiff);
        }
    }
}
