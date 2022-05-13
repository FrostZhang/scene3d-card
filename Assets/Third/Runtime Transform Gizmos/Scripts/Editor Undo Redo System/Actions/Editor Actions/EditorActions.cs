using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This action can be executed to delete the current object selection.
    /// </summary>
    public class DeleteSelectedObjectsAction : IUndoableAndRedoableAction, IAction
    {
        /// <summary>
        /// After the action is executed, this will hold all objects which were deeted.
        /// </summary>
        private List<GameObject> _deletedObjects;
        /// <summary>
        /// The selection snapshot before the objects were deleted. Allows us to undo.
        /// </summary>
        private ObjectSelectionSnapshot _preDeleteSnapshot;
        /// <summary>
        /// The selection snapshot after the objects were deleted. Allows us to redo.
        /// </summary>
        private ObjectSelectionSnapshot _postDeleteSnapshot;

        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            // Only delete if we didn't delete already and if we are allowed to delete
            if (_postDeleteSnapshot == null)
            {
                // Take a pre delete selection snapshot which will allow us to undo
                _preDeleteSnapshot = new ObjectSelectionSnapshot();
                _preDeleteSnapshot.TakeSnapshot();

                // Store the currently selected objects (we'll need them later) and clear the selection
                List<GameObject> allSelectedObjects = new List<GameObject>(EditorObjectSelection.Instance.SelectedGameObjects);

                // Loop through each selected object
                _deletedObjects = new List<GameObject>(allSelectedObjects.Count);
                foreach (var selectedObject in allSelectedObjects)
                {
                    // Add the object to the deleted list and make it inactive
                    _deletedObjects.Add(selectedObject);
                    selectedObject.SetActive(false);
                }

                // Take a post delete snapshot to allow us to redo
                _postDeleteSnapshot = new ObjectSelectionSnapshot();
                _postDeleteSnapshot.TakeSnapshot();

                // Inform the selection module that the objects were deleted
                EditorObjectSelection.Instance.OnDeleted();

                // Record th action with the Undo/Redo system
                EditorUndoRedoSystem.Instance.RegisterAction(this);
            }
        }

        /// <summary>
        /// Undo action.
        /// </summary>
        public void Undo()
        {
            // Only undo if we deleted anything
            if (_deletedObjects != null)
            {
                // Enable the objects
                foreach (var deletedObject in _deletedObjects)
                {
                    deletedObject.SetActive(true);
                }

                // Inform the selection about the Undo event
                EditorObjectSelection.Instance.UndoRedoSelection(_preDeleteSnapshot, UndoRedoActionType.Undo);
            }
        }

        /// <summary>
        /// Redo action.
        /// </summary>
        public void Redo()
        {
            // Only redo if we deleted anything
            if (_deletedObjects != null)
            {
                // Disable the objects
                foreach (var deletedObject in _deletedObjects)
                {
                    deletedObject.SetActive(false);
                }

                // Inform the selection about the Redo event
                EditorObjectSelection.Instance.UndoRedoSelection(_postDeleteSnapshot, UndoRedoActionType.Redo);
            }
        }

        /// <summary>
        /// Called when the action is removed from the Undo/Redo stack.
        /// </summary>
        public void OnRemovedFromUndoRedoStack()
        {
            // When the action is popped of the stack, we need to make sure that if the
            // objects are in their delete state (they might not be if the action was
            // undone), we actually delete them from the scene. Otherwise, they would 
            // just be left behind.
            if (_deletedObjects != null && _deletedObjects.Count != 0)
            {
                // We can check if the objects are in the deleted state, if the first
                // (or any) element inside the array is inactive.
                if (!_deletedObjects[0].activeInHierarchy)
                {
                    // Delete the objects
                    foreach (var gameObject in _deletedObjects) GameObject.Destroy(gameObject);
                    _deletedObjects.Clear();
                    _deletedObjects = null;
                }
            }
        }
    }

    public class ObjectDuplicationAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        private List<GameObject> _sourceParents = new List<GameObject>();
        private List<GameObject> _duplicateObjectParents = new List<GameObject>();
        private ObjectSelectionSnapshot _preDuplicateSelectionSnapshot;
        #endregion

        #region Constructors
        public ObjectDuplicationAction(List<GameObject> sourceObjects)
        {
            if(sourceObjects != null && sourceObjects.Count != 0)
            {
                _sourceParents = GameObjectExtensions.GetParentsFromObjectCollection(sourceObjects);
            }
        }
        #endregion

        #region Public Methods
        public void Execute()
        {
            if(_sourceParents.Count != 0)
            {
                foreach(var parent in _sourceParents)
                {
                    Transform parentTransform = parent.transform;
                    GameObject clone = parent.CloneAbsTRS(parentTransform.position, parentTransform.rotation, parentTransform.lossyScale, parentTransform.parent);
                    _duplicateObjectParents.Add(clone);
                }

                if(RuntimeEditorApplication.Instance.EnableUndoRedo) EditorUndoRedoSystem.Instance.RegisterAction(this);
                EditorObjectSelection.Instance.OnDuplicated(_duplicateObjectParents);
            }
        }

        public void Undo()
        {
            if(_duplicateObjectParents.Count != 0)
            {
                foreach(var parent in _duplicateObjectParents)
                {
                    MonoBehaviour.Destroy(parent);
                }
                _duplicateObjectParents.Clear();
            }
        }

        public void Redo()
        {
            if (_sourceParents.Count != 0)
            {
                _duplicateObjectParents.Clear();
                foreach (var parent in _sourceParents)
                {
                    Transform parentTransform = parent.transform;
                    GameObject clone = parent.CloneAbsTRS(parentTransform.position, parentTransform.rotation, parentTransform.lossyScale, parentTransform.parent);
                    _duplicateObjectParents.Add(clone);
                }
            }
        }

        public void OnRemovedFromUndoRedoStack()
        {
        }
        #endregion
    }

    /// <summary>
    /// This action is executed after the object selection has changed.
    /// </summary>
    public class PostObjectSelectionChangedAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// The object selection snapshot which was taken before the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _preChangeSelectionSnapshot;

        /// <summary>
        /// The object selection snapshot which was taken after the object selection was changed.
        /// </summary>
        private ObjectSelectionSnapshot _postChangeSelectionSnapshot;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preChangeSelectionSnapshot">
        /// The object selection snapshot taken before the selection was changed.
        /// </param>
        /// <param name="postChangeSelectionSnapshot">
        /// The object selection snapshot taken after the selection was changed.
        /// </param>
        public PostObjectSelectionChangedAction(ObjectSelectionSnapshot preChangeSelectionSnapshot, 
                                                ObjectSelectionSnapshot postChangeSelectionSnapshot)
        {
            _preChangeSelectionSnapshot = preChangeSelectionSnapshot;
            _postChangeSelectionSnapshot = postChangeSelectionSnapshot;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
             if(RuntimeEditorApplication.Instance.EnableUndoRedo) EditorUndoRedoSystem.Instance.RegisterAction(this);
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            EditorObjectSelection.Instance.UndoRedoSelection(_preChangeSelectionSnapshot, UndoRedoActionType.Undo);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            EditorObjectSelection.Instance.UndoRedoSelection(_postChangeSelectionSnapshot, UndoRedoActionType.Redo);
        }

        public void OnRemovedFromUndoRedoStack()
        {
        }
        #endregion
    }

    /// <summary>
    /// This action is executed after a gizmo has finished transforming game objects.
    /// </summary>
    public class PostGizmoTransformedObjectsAction : IUndoableAndRedoableAction, IAction
    {
        #region Private Variables
        /// <summary>
        /// This is the list of object trasnform snapshots which were taken before the objects were transformed.
        /// </summary>
        private List<ObjectTransformSnapshot> _preTransformObjectSnapshots;

        /// <summary>
        /// This is the list of object transform snapshots which were taken after the objects were transformed.
        /// </summary>
        private List<ObjectTransformSnapshot> _postTransformObjectSnapshot;

        /// <summary>
        /// This is the gizmo which transformed the game objects.
        /// </summary>
        private Gizmo _gizmoWhichTransformedObjects;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preTransformObjectSnapshots">
        /// This is the list of object trasnform snapshots which were taken before the objects were transformed.
        /// </param>
        /// <param name="postTransformObjectSnapshot">
        /// This is the list of object transform snapshots which were taken after the objects were transformed.
        /// </param>
        /// <param name="gizmoWhichTransformedObjects">
        /// This is the gizmo which transformed the game objects.
        /// </param>
        public PostGizmoTransformedObjectsAction(List<ObjectTransformSnapshot> preTransformObjectSnapshots,
                                                 List<ObjectTransformSnapshot> postTransformObjectSnapshot,
                                                 Gizmo gizmoWhichTransformedObjects)
        {
            _preTransformObjectSnapshots = new List<ObjectTransformSnapshot>(preTransformObjectSnapshots);
            _postTransformObjectSnapshot = new List<ObjectTransformSnapshot>(postTransformObjectSnapshot);
            _gizmoWhichTransformedObjects = gizmoWhichTransformedObjects;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Executes the action.
        /// </summary>
        public void Execute()
        {
            GizmoTransformedObjectsMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
            if (RuntimeEditorApplication.Instance.EnableUndoRedo) EditorUndoRedoSystem.Instance.RegisterAction(this);
        }

        /// <summary>
        /// This method can be called to undo the action.
        /// </summary>
        public void Undo()
        {
            // In order to undo this kind of action, we will loop through all pre transform object snapshots
            // and apply them to the corresponding objects.
            foreach(ObjectTransformSnapshot snapshot in _preTransformObjectSnapshots)
            {
                snapshot.ApplySnapshot();
            }

            // Send a gizmo transform operation undone message
            GizmoTransformOperationWasUndoneMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
        }

        /// <summary>
        /// This method can be called to redo the action.
        /// </summary>
        public void Redo()
        {
            // In order to redo this kind of action, we will loop through all post transform object snapshots
            // and apply them to the corresponding objects.
            foreach (ObjectTransformSnapshot snapshot in _postTransformObjectSnapshot)
            {
                snapshot.ApplySnapshot();
            }

            // Send a gizmo transform operation redone message
            GizmoTransformOperationWasRedoneMessage.SendToInterestedListeners(_gizmoWhichTransformedObjects);
        }

        public void OnRemovedFromUndoRedoStack()
        {
        }
        #endregion
    }
}
