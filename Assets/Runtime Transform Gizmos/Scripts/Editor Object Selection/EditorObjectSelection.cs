using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// Implements the object selection mechanism.
    /// </summary>
    [Serializable]
    public class EditorObjectSelection : MonoSingletonBase<EditorObjectSelection>
    {
        /// <summary>
        /// Event handler which can be used to handle a selection changed event.
        /// </summary>
        public delegate void SelectionChangedHandler(ObjectSelectionChangedEventArgs selectionChangedEventArgs);
        public event SelectionChangedHandler SelectionChanged;

        /// <summary>
        /// Event handler which can be used to handle a selection duplicated event.
        /// </summary>
        public delegate void SelectionDuplicatedHandler(List<GameObject> sourceObjects, List<GameObject> duplicatedRoots);
        public event SelectionDuplicatedHandler SelectionDuplicated;

        public delegate void SelectionDeletedHandler(List<GameObject> deletedObjects);
        public event SelectionDeletedHandler SelectionDeleted;

        /// <summary>
        /// Event handler which can be used to handle the click of an object.
        /// </summary>
        public delegate void GameObjectClickedHandler(GameObject clickedObject);
        public event GameObjectClickedHandler GameObjectClicked;

        #region Private Variables
        /// <summary>
        /// Holds all object selection related settings.
        /// </summary>
        [SerializeField]
        private ObjectSelectionSettings _objectSelectionSettings = new ObjectSelectionSettings();

        /// <summary>
        /// Shortcut keys.
        /// </summary>
        [SerializeField]
        private ShortcutKeys _appendToSelectionShortcut = new ShortcutKeys("Append to selection", 0)
        {
            LCtrl = true,
            UseMouseButtons = false
        };
        [SerializeField]
        private ShortcutKeys _multiDeselectShortcut = new ShortcutKeys("Multi deselect", 0)
        {
            LShift = true,
            UseMouseButtons = false
        };
        [SerializeField]
        private ShortcutKeys _duplicateSelectionShortcut = new ShortcutKeys("Duplicate selection", 1)
        {
            Key0 = KeyCode.D,
            LCtrl = true,
            UseMouseButtons = false
        };
        [SerializeField]
        private ShortcutKeys _deleteSelectionShortcut = new ShortcutKeys("Delete selection", 1)
        {
            Key0 = KeyCode.Delete,
            UseModifiers = false,
            UseMouseButtons = false
        };

        /// <summary>
        /// The currently selected objects.
        /// </summary>
        private HashSet<GameObject> _selectedObjects = new HashSet<GameObject>();

        /// <summary>
        /// Holds the masked objects. Objects which reside inside this collection can not be selected.
        /// </summary>
        private HashSet<GameObject> _maskedObjects = new HashSet<GameObject>();

        /// <summary>
        /// The last selected game object. Note: When the selection is adjusted using the selection rectangle,
        /// this object will be the first object available in the selected objects hash.
        /// </summary>
        private GameObject _lastSelectedGameObject;

        /// <summary>
        /// The object selection rectangle which is used to perform multi-select operations.
        /// </summary>
        [SerializeField]
        private ObjectSelectionRectangle _objectSelectionRectangle = new ObjectSelectionRectangle();

        /// <summary>
        /// When the user presses the left mouse button, this will hold the a snapshot of all selected
        /// objects. When the button is released, this will be used to execute a post selection change 
        /// action.
        /// </summary>
        private ObjectSelectionSnapshot _multiSelectPreChangeSnapshot;

        /// <summary>
        /// When the user releases the left mouse button, this will hold the a snapshot of all selected
        /// objects and together with '_multiSelectPreChangeSnapshot' it will be used to execute a post
        /// selection changed action. 
        /// </summary>
        private ObjectSelectionSnapshot _multiSelectPostChangeSnapshot;

        /// <summary>
        /// When the user performs a multi-select/deselect operation while dragging the mouse, this will be set to true.
        /// This information is necessary to decide if a post selection changed action needs to be executed.
        /// </summary>
        private bool _selectionChangedWithShape;

        /// <summary>
        /// These 2 will be modified whenever objects are selected/deselected and they are used when firing the 
        /// object selection changed event.
        /// </summary>
        private ObjectSelectActionType _lastSelectActionType = ObjectSelectActionType.None;
        private ObjectDeselectActionType _lastDeselectActionType = ObjectDeselectActionType.None;
        #endregion

        #region Public Properties
        public int NumberOfSelectedObjects { get { return _selectedObjects.Count; } }
        public GameObject LastSelectedGameObject { get { return _lastSelectedGameObject; } }

        public HashSet<GameObject> SelectedGameObjects { get { return new HashSet<GameObject>(_selectedObjects); } }
        public HashSet<GameObject> MaskedObjects { get { return new HashSet<GameObject>(_maskedObjects); } }

        public ShortcutKeys AppendToSelectionShortcut { get { return _appendToSelectionShortcut; } }
        public ShortcutKeys MultiDeselectShortcut { get { return _multiDeselectShortcut; } }
        public ShortcutKeys DuplicateSelectionShortcut { get { return _duplicateSelectionShortcut; } }
        public ShortcutKeys DeleteSelectionShortcut { get { return _deleteSelectionShortcut; } }

        public ObjectSelectionSettings ObjectSelectionSettings { get { return _objectSelectionSettings; } }
        public ObjectSelectionRectangleRenderSettings ObjectSelectionRectangleRenderSettings { get { return _objectSelectionRectangle.RenderSettings; } }
        #endregion

        #region Public Methods
        public void OnDuplicated(List<GameObject> duplicatedRoots)
        {
            if (SelectionDuplicated != null) SelectionDuplicated(new List<GameObject>(_selectedObjects), duplicatedRoots);
        }

        public void OnDeleted()
        {
            if (NumberOfSelectedObjects != 0)
            {
                List<GameObject> deletedObjects = new List<GameObject>(_selectedObjects);
                ClearSelection(false, ObjectDeselectActionType.SelectionDeleted);
                if (SelectionDeleted != null) SelectionDeleted(deletedObjects);
            }
        }

        /// <summary>
        /// Adds the specified game object to the selection. The method will take into account
        /// any restrictions which are dictated by the object selection settings. Note that 
        /// inactive objects are always ignored.
        /// </summary>
        /// <param name="gameObj">
        /// The game object which must be added to the selection.
        /// </param>
        /// <param name="allowUndoRedo">
        /// If this is true, the selection change which occurs from calling this function 
        /// will be registered with the Undo/Redo system.
        /// </param>
        /// <returns>
        /// True if the object was added to the selection and false otherwise. The method
        /// will always return false if the specified game object is inactive. The method
        /// also returns false if the current selection already contains the specified object.
        /// </returns>
        public bool AddObjectToSelection(GameObject gameObj, bool allowUndoRedo)
        {
            if (gameObj == null) return false;

            // If the current selection is composed of this object only, we exit. This allows us to 
            // avoid registering an Undo operation for what essentially is a no-op.
            if (IsSelectionExactMatch(new List<GameObject> { gameObj })) return false;

            // Continue based on whether or not Undo/Redo is allowed
            if (allowUndoRedo)
            {
                // Take a pre-change snapshot
                var preChangeSnapshot = new ObjectSelectionSnapshot();
                preChangeSnapshot.TakeSnapshot();

                // Attempt to add the object to the selection. If the method returns true, it means
                // the object was added and we have some more work to do.
                if (AddObjectToSelection(gameObj, ObjectSelectActionType.AddObjectToSelectionCall))
                {
                    // Take a post-change snapshot
                    var postChangeSnapshot = new ObjectSelectionSnapshot();
                    postChangeSnapshot.TakeSnapshot();

                    // Execute the post-change action to allow for undo/redo
                    var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                    action.Execute();

                    // The selection has changed
                    var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.AddObjectToSelectionCall, new List<GameObject> { gameObj },
                                                                                  ObjectDeselectActionType.None, new List<GameObject>());
                    OnSelectionChanged(selChangedEventArgs);
                    return true;
                }
            }
            else
            {
                // Attempt to add the object to the selection. If the method returns true, it means
                // the object was added and we have some more work to do.
                if (AddObjectToSelection(gameObj, ObjectSelectActionType.AddObjectToSelectionCall))
                {
                    // The selection has changed
                    var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.AddObjectToSelectionCall, new List<GameObject> { gameObj },
                                                                                  ObjectDeselectActionType.None, new List<GameObject>());
                    OnSelectionChanged(selChangedEventArgs);
                    return true;
                }
            }

            // The object could not be added to the selection
            return false;
        }

        /// <summary>
        /// Removes the specified object from the selection.
        /// </summary>
        /// <param name="gameObj">
        /// The object which must be removed from selection.
        /// </param>
        /// <param name="allowUndoRedo">
        /// If this is true, the selection change which occurs from calling this function 
        /// will be registered with the Undo/Redo system.
        /// </param>
        /// <returns>
        /// True if the object was removed and false otherwise.
        /// </returns>
        public bool RemoveObjectFromSelection(GameObject gameObj, bool allowUndoRedo)
        {
            if (gameObj == null) return false;

            // If the object is not selected, there is nothing to do and we just return false
            // to inform the caller that the object can not be removed from the selection.
            if (!IsObjectSelected(gameObj)) return false;

            // Continue based on whether or not Undo/Redo is supported
            if (allowUndoRedo)
            {
                // Take a pre-change snapshot
                var preChangeSnapshot = new ObjectSelectionSnapshot();
                preChangeSnapshot.TakeSnapshot();

                // Attempt to remove the object from the selection
                if (RemoveObjectFromSelection(gameObj, ObjectDeselectActionType.RemoveObjectFromSelectionCall))
                {
                    // Take a post-change snapshot
                    var postChangeSnapshot = new ObjectSelectionSnapshot();
                    postChangeSnapshot.TakeSnapshot();

                    // Execute the post-change action to allow for undo/redo
                    var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                    action.Execute();

                    // The selection has changed
                    var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.None, new List<GameObject>(),
                                                                                  ObjectDeselectActionType.RemoveObjectFromSelectionCall, new List<GameObject> { gameObj });
                    OnSelectionChanged(selChangedEventArgs);
                    return true;
                }
            }
            else
            {
                // Attempt to remove the object from the selection
                if (RemoveObjectFromSelection(gameObj, ObjectDeselectActionType.RemoveObjectFromSelectionCall))
                {
                    // The selection has changed
                    var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.None, new List<GameObject>(),
                                                                                  ObjectDeselectActionType.RemoveObjectFromSelectionCall, new List<GameObject> { gameObj });
                    OnSelectionChanged(selChangedEventArgs);
                    return true;
                }
            }

            // The object could not be removed from the selection
            return false;
        }

        /// <summary>
        /// Can be called by the client code to manually modify the object selection. For example, this could
        /// be useful when implementing a CTRL+A (select all) feature in your application. Note however, that
        /// inactive objects will always be ignored.
        /// </summary>
        /// <param name="selectedObjects">
        /// The objects which must be selected. If null or empty, the selection will be cleared. Otherwise, 
        /// the objects which reside inside this collection will be selected based on the restrictions
        /// imposed by the selection settings (masks, can select empty, can select mesh etc).
        /// </param>
        /// <param name="allowUndoRedo">
        /// If this is true, the selection change which occurs from calling this function will be registered
        /// with the Undo/Redo system.
        /// </param>
        public void SetSelectedObjects(List<GameObject> selectedObjects, bool allowUndoRedo)
        {
            // If the current selection matches the specified object collection, we will exit as there
            // is nothing to do. This allows us to avoid registering an Undo operation for what essentially
            // is a no-op.
            if (IsSelectionExactMatch(selectedObjects)) return;

            // Save a copy of the currently selected objects. We will need this later.
            List<GameObject> previouslySelectedObjects = new List<GameObject>(SelectedGameObjects);

            // If the selected objects collection is either null or empty, we just clear the selection
            if (selectedObjects == null || selectedObjects.Count == 0) ClearSelection(allowUndoRedo, ObjectDeselectActionType.SetSelectedObjectsCall);
            else
            // We must proceed based on whether or not undo/redo is allowed
            if (allowUndoRedo)
            {
                // Take a pre-change snapshot
                var preChangeSnapshot = new ObjectSelectionSnapshot();
                preChangeSnapshot.TakeSnapshot();

                // First clear the current selection.
                // Note: Pass false as the first param because Undo/Redo will be handled here inside this method.
                ClearSelection(false, ObjectDeselectActionType.SetSelectedObjectsCall);

                // Select the objects
                AddObjectCollectionToSelection(selectedObjects, ObjectSelectActionType.SetSelectedObjectsCall);

                // Take a post-change snapshot
                var postChangeSnapshot = new ObjectSelectionSnapshot();
                postChangeSnapshot.TakeSnapshot();

                // Execute the post-change action to allow for undo/redo
                var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                action.Execute();
            }
            else AddObjectCollectionToSelection(selectedObjects, ObjectSelectActionType.SetSelectedObjectsCall);

            // The selection has changed
            var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.SetSelectedObjectsCall, new List<GameObject>(_selectedObjects),
                                                                          ObjectDeselectActionType.SetSelectedObjectsCall, previouslySelectedObjects);
            OnSelectionChanged(selChangedEventArgs);
        }

        /// <summary>
        /// Clears the object selection.
        /// </summary>
        /// <param name="allowUndoRedo">
        /// If true, the clear action can be undone/redone. Set this to false, if you don't
        /// want the clear operation to be undone/redone.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise. This method will return 
        /// false if there are no objects selected when this method is called.
        /// </returns>
        public bool ClearSelection(bool allowUndoRedo)
        {
            return ClearSelection(allowUndoRedo, ObjectDeselectActionType.ClearSelectionCall);
        }

        /// <summary>
        /// Adds the specified game objects to the object selection mask. After this method
        /// is called, the specified objects can not be selected. 
        /// </summary>
        public void AddGameObjectCollectionToSelectionMask(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObj in gameObjects)
            {
                _maskedObjects.Add(gameObj);
            }
        }

        /// <summary>
        /// Removes the specified game objects from the object selection mask.
        /// </summary>
        /// <remarks>
        /// Just because an object doesn't belong to the selection mask, doesn't mean that
        /// it can be selected. For example, if the object belongs to a layer which was masked,
        /// it will still not be selected. Also, if the object has a IRTEditorEventListener
        /// component attached to it and it's OnCanBeSelected implementation returns false, it
        /// will not be selected.
        /// </remarks>
        public void RemoveGameObjectCollectionFromSelectionMask(List<GameObject> gameObjects)
        {
            foreach (GameObject gameObj in gameObjects)
            {
                _maskedObjects.Remove(gameObj);
            }
        }

        /// <summary>
        /// Can be used to check if the current object selection matches the specified
        /// object collection.
        /// </summary>
        /// <returns>
        /// True if the specified object collection is the same as the collection of
        /// selected objects. The order in which the objects are stored inside the 2 
        /// collections doesn't matter. 
        /// </returns>
        /// <remarks>
        /// The method also returns true if 'gameObjectsToMatch' is null and no objects
        /// are selected when when method is invoked. If 'gameObjectsToMatch' is null,
        /// but the selection contains objects, the method will return false.
        /// </remarks>
        public bool IsSelectionExactMatch(List<GameObject> gameObjectsToMatch)
        {
            // Handle the null case
            if (gameObjectsToMatch == null)
            {
                // If the specified object collection is null and there are no objects currently
                // selected, we will consider this a match and return true.
                if (NumberOfSelectedObjects == 0) return true;

                // If null but the selection contains objects, we will return false
                return false;
            }

            // If the number of elements don't match, we can return false
            if (NumberOfSelectedObjects != gameObjectsToMatch.Count) return false;

            // Make sure that every game object in 'gameObjectsToMatch' is selected.
            // If we find at least one object which is not selected, we can return false.
            foreach (GameObject objectToMatch in gameObjectsToMatch)
            {
                if (!IsObjectSelected(objectToMatch)) return false;
            }

            // We have a match
            return true;
        }

        /// <summary>
        /// This method will be called whenever a change in selection is undone or redone.
        /// </summary>
        /// <remarks>
        /// It is advised to ignore this method and NOT use it in your own code.
        /// </remarks>
        public void UndoRedoSelection(ObjectSelectionSnapshot objectSelectionSnapshot, UndoRedoActionType undoRedoActionType)
        {
            // Store the currently selected objects. We will need to inform them that they have been deselected
            List<GameObject> previouslySelectedOjects = new List<GameObject>(SelectedGameObjects);

            // Apply the snapshot
            _selectedObjects = new HashSet<GameObject>(objectSelectionSnapshot.SelectedGameObjects);
            _lastSelectedGameObject = objectSelectionSnapshot.LastSelectedGameObject;

            // Inform the previosuly selected objects that they are no longer selected
            ObjectDeselectActionType deselectActionType = undoRedoActionType == UndoRedoActionType.Undo ? ObjectDeselectActionType.Undo : ObjectDeselectActionType.Redo;
            var deselectEventArgs = new ObjectDeselectEventArgs(deselectActionType);
            foreach (var gameObj in previouslySelectedOjects)
            {
                IRTEditorEventListener editorEventListener = gameObj.GetComponent<IRTEditorEventListener>();
                if (editorEventListener != null) editorEventListener.OnDeselected(deselectEventArgs);
            }

            // Inform the newly selected objects that they have been selected
            ObjectSelectActionType selectActionType = undoRedoActionType == UndoRedoActionType.Undo ? ObjectSelectActionType.Undo : ObjectSelectActionType.Redo;
            var selectEventArgs = new ObjectSelectEventArgs(selectActionType);
            foreach (var gameObj in _selectedObjects)
            {
                IRTEditorEventListener editorEventListener = gameObj.GetComponent<IRTEditorEventListener>();
                if (editorEventListener != null) editorEventListener.OnSelected(selectEventArgs);
            }

            // When the snapshot was applied the collection of selected objects was changed and
            // now the gizmos have to know about the new object collection.
            EditorGizmoSystem gizmoSystem = EditorGizmoSystem.Instance;
            ConnectObjectSelectionToGizmo(gizmoSystem.TranslationGizmo);
            ConnectObjectSelectionToGizmo(gizmoSystem.RotationGizmo);
            ConnectObjectSelectionToGizmo(gizmoSystem.ScaleGizmo);
            ConnectObjectSelectionToGizmo(gizmoSystem.VolumeScaleGizmo);

            // The selection has changed
            var selChangedEventArgs = new ObjectSelectionChangedEventArgs(selectActionType, new List<GameObject>(_selectedObjects), deselectActionType, previouslySelectedOjects);
            OnSelectionChanged(selChangedEventArgs);
        }

        /// <summary>
        /// Can be used to check if the specified object is selected.
        /// </summary>
        public bool IsObjectSelected(GameObject gameObj)
        {
            return _selectedObjects.Contains(gameObj);
        }

        /// <summary>
        /// Connects the specified gizmo to the object selection. After this method is
        /// called, the gizmo will be able to manipulate the selected objects.
        /// </summary>
        /// <remarks>
        /// This method connects the internal object selection collection (not a copy of it). So
        /// it can be called only once per gizmo and the gizmo will essentially be able to 
        /// manipulate the original (internal) object selection buffer.
        /// </remarks>
        public void ConnectObjectSelectionToGizmo(Gizmo gizmo)
        {
            gizmo.ControlledObjects = _selectedObjects;
        }

        /// <summary>
        /// Returns a box which describes the object selection boundaries.
        /// </summary>
        /// <returns>
        /// The box which describes the object selection volume in 3D space. The box
        /// can be invalid if there are no selected objects or if none of the selected
        /// objects has a valid box.
        /// </returns>
        public Box GetWorldBox()
        {
            // No objects selected? Return an invalid box.
            if (NumberOfSelectedObjects == 0) return Box.GetInvalid();

            // Initialize a selection world box by making it invalid
            Box selectionWorldBox = Box.GetInvalid();
            foreach (GameObject selectedObject in _selectedObjects)
            {
                // Retrieve the object's world box and if it is valid, use it to adjust the selection box
                Box objectWorldBox = selectedObject.GetWorldBox();
                if (objectWorldBox.IsValid())
                {
                    // The object's box is valid, so update the selection box. If the selection box is valid, it means
                    // it already contains at least one box from a previous object, so we can call the 'Encapsulate'
                    // method. Otherwise, it means that this is the first valid object that we encountered, so we need
                    // to perform an assignment.
                    if (selectionWorldBox.IsValid()) selectionWorldBox.Encapsulate(objectWorldBox);
                    else selectionWorldBox = objectWorldBox;
                }
            }

            return selectionWorldBox;
        }

        /// <summary>
        /// Returns the center of the selection volume.
        /// </summary>
        /// <remarks>
        /// It may be more convenient to call 'GetWorldBox' instead because it returns a 'Box' instance
        /// which can be checked for validity.
        /// </remarks>
        /// <returns>
        /// The center of the selection world volume. Can be the zero vector when no 
        /// objects are selected or when none of the selected objects have a valid 
        /// volume.
        /// </returns>
        public Vector3 GetSelectionWorldCenter()
        {
            return CalculateSelectionWorldCenter();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Returns true if the object selection mechanism can operate. 
        /// </summary>
        private bool CanOperate()
        {
            return gameObject.activeSelf && enabled &&
                   !EditorGizmoSystem.Instance.IsActiveGizmoReadyForObjectManipulation()
                   //Asher 本项目不显示SceneGizmo
                   /*&&!SceneGizmo.Instance.IsHovered()*/;
        }

        /// <summary>
        /// Returns true if a multi-select operation can be performed.
        /// </summary>
        private bool CanPerformMultiSelect()
        {
            return ObjectSelectionSettings.CanMultiSelect &&
                   gameObject.activeSelf
                   && enabled
                   && InputDevice.Instance.IsPressed(0)
                   //Asher SceneGizmo   本项目不适用
                   //&& !SceneGizmo.Instance.IsHovered()
                   && _objectSelectionRectangle.IsVisible;
        }

        /// <summary>
        /// Returns true if the specified object is masked. An object is masked if at least one of the
        /// following is true: a) it can be found inside the selection mask; b) it belongs to a layers
        /// which has been masked.
        /// </summary>
        private bool IsObjectMasked(GameObject gameObj)
        {
            return _maskedObjects.Contains(gameObj) || !LayerHelper.IsLayerBitSet(_objectSelectionSettings.SelectableLayers, gameObj.layer);
        }

        /// <summary>
        /// Returns true if the specified object can be selected. 
        /// </summary>
        /// <param name="selectActionType">
        /// Describes the type of selection action which is used to select the object.
        /// </param>
        private bool CanSelectObject(GameObject gameObj, ObjectSelectActionType selectActionType)
        {
            // Ignore null and inactive objects. Also check if the selection mechansim can operate.
            if (gameObj == null || gameObj.activeSelf == false || !CanOperate()) return false;

            // Objects which are part of the RTEditor hierarchy, can never be selected
            if (gameObj.IsRTEditorSystemObject()) return false;

            // Take settings into account
            bool objectHasMesh = gameObj.HasMesh();
            if (!_objectSelectionSettings.CanSelectTerrainObjects && gameObj.HasTerrain()) return false;
            if (!objectHasMesh && (!_objectSelectionSettings.CanSelectLightObjects && gameObj.HasLight())) return false;
            if (!objectHasMesh && (!_objectSelectionSettings.CanSelectParticleSystemObjects && gameObj.HasParticleSystem())) return false;
            if (!_objectSelectionSettings.CanSelectEmptyObjects && gameObj.IsEmpty()) return false;
            if (!_objectSelectionSettings.CanSelectSpriteObjects && gameObj.IsSprite()) return false;

            // If the object is already selected or if it is masked, we can not select it
            if (IsObjectSelected(gameObj) || IsObjectMasked(gameObj)) return false;

            // Check if the object has a 'IRTEditorEventListener' component attached and if it does, use its
            // 'OnCanBeSelected' handler to decide if the object can be selected.
            IRTEditorEventListener editorEventListener = gameObj.GetComponent<IRTEditorEventListener>();
            if (editorEventListener != null)
            {
                var selectEventArgs = new ObjectSelectEventArgs(selectActionType);
                return editorEventListener.OnCanBeSelected(selectEventArgs);
            }

            // If we reached this point, it means all tests passed, so we can return true
            return true;
        }

        /// <summary>
        /// Called every frame to perform any necessary updates.
        /// </summary>
        private void Update()
        {
            // Remove null or inactive object references. Inactive objects can not be selected. Null object
            // entries can become available when objects are deleted from the scene.
            //RemoveNullAndInactiveObjectRefs();

            //if (DeleteSelectionShortcut.IsActiveInCurrentFrame())
            //{
            //    var deleteAction = new DeleteSelectedObjectsAction();
            //    deleteAction.Execute();
            //    return;
            //}

            //// Duplicate objects if needed
            //if (_duplicateSelectionShortcut.IsActiveInCurrentFrame())
            //{
            //    // Gather all selected objects which can be duplicated
            //    List<GameObject> objectsToDuplicate = new List<GameObject>(EditorObjectSelection.Instance.SelectedGameObjects);
            //    objectsToDuplicate.RemoveAll(item => !CanObjectBeDuplicated(item));

            //    // Duplicate objects
            //    var action = new ObjectDuplicationAction(objectsToDuplicate);
            //    action.Execute();
            //}

            // If no UI elements were hovered, we will analyze the input device's (mouse or touch) activity
            if (!WereAnyUIElementsHovered())
            {
                if (InputDevice.Instance.WasPressedInCurrentFrame(0)) OnInputDeviceFirstButtonDown();
                if (InputDevice.Instance.WasMoved()) OnInputDeviceMoved();
            }

            if (InputDevice.Instance.WasReleasedInCurrentFrame(0)) OnInputDeviceFirstButtonUp();
        }

        /// <summary>
        /// Checks if the specified object can be duplicated.
        /// </summary>
        private bool CanObjectBeDuplicated(GameObject gameObj)
        {
            return LayerHelper.IsLayerBitSet(_objectSelectionSettings.DuplicatableLayers, gameObj.layer);
        }

        /// <summary>
        /// Removes all null or inactive object references from the object selection collection.
        /// </summary>
        private void RemoveNullAndInactiveObjectRefs()
        {
            List<GameObject> inactiveObjects = new List<GameObject>(_selectedObjects).FindAll(item => item != null && !item.activeSelf);
            _selectedObjects.RemoveWhere(item => item == null || !item.activeSelf);
            _maskedObjects.RemoveWhere(item => item == null);

            // If any inactive objects were found, fire an object selection changed event
            if (inactiveObjects.Count != 0)
            {
                var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.None, new List<GameObject>(),
                                                                              ObjectDeselectActionType.DeselectInactive, inactiveObjects);
                OnSelectionChanged(selChangedEventArgs);
            }
        }

        /// <summary>
        /// Called when the first button of the input device is pressed.
        /// </summary>
        private void OnInputDeviceFirstButtonDown()
        {
            // Can we operate and can we select by clicking?
            if (CanOperate() && ObjectSelectionSettings.CanClickSelect)
            {
                // Retrieve the object which is picked by the mouse cursor
                GameObject pickedGameObject = PickObjectInScene();
                if (pickedGameObject != null)
                {
                    // Send a game object clicked event
                    if (GameObjectClicked != null) GameObjectClicked(pickedGameObject);

                    // If a game object was picked, the selection may need to change. It all depends on how the click
                    // handler handles the pick operation. So we will take a snapshot of the current object selection
                    // in order to use it later if the selection has indeed changed.
                    var preChangeSnapshot = new ObjectSelectionSnapshot();
                    preChangeSnapshot.TakeSnapshot();

                    // Handle the object click
                    if (HandleObjectClick(pickedGameObject))
                    {
                        // The selection has changed. Take a snapshot post-change.
                        var postChangeSnapshot = new ObjectSelectionSnapshot();
                        postChangeSnapshot.TakeSnapshot();

                        // Execute a post selection changed action to allow for undo/redo
                        var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                        action.Execute();

                        // The selection has changed
                        ObjectSelectionChangedEventArgs selChangedArgs = ObjectSelectionChangedEventArgs.FromSnapshots(_lastSelectActionType, _lastDeselectActionType, preChangeSnapshot, postChangeSnapshot);
                        OnSelectionChanged(selChangedArgs);
                    }
                }
                else
                {
                    if (GameObjectClicked != null) GameObjectClicked(null);
                }
                //Asher 用不上
                //else
                //// The user clicked in the air, so we clear the selection
                //if (!_multiDeselectShortcut.IsActive() && !_appendToSelectionShortcut.IsActive()) ClearSelection(true, ObjectDeselectActionType.ClearClickAir);

                //// When the left mouse button is pressed, we have to prepare for the possibility that the user
                //// is trying to select objects using the object selection shape. So we will take a snapshot of
                //// the current selection here just in case. When the button is released, another snapshot will
                //// be taken and these 2 snapshots will be used to take appropriate action.
                //_multiSelectPreChangeSnapshot = new ObjectSelectionSnapshot();
                //_multiSelectPreChangeSnapshot.TakeSnapshot();
            }

            //Asher 用不上
            //Vector2 inputDevPos;
            //if (!InputDevice.Instance.GetPosition(out inputDevPos)) return;

            //// Adjust the selection shape's corners
            //ObjectSelectionShape objectSelectionShape = GetObjectSelectionShape();
            //objectSelectionShape.SetEnclosingRectBottomRightPoint(inputDevPos);
            //objectSelectionShape.SetEnclosingRectTopLeftPoint(inputDevPos);

            //// When the first button of the input device is pressed, we will show the selection shape
            //// because the user may later start dragging the device (mouse or touch).
            //if (CanOperate()) objectSelectionShape.IsVisible = true;
            //else objectSelectionShape.IsVisible = false;
        }

        /// <summary>
        /// Clears the object selection.
        /// </summary>
        /// <param name="allowUndoRedo">
        /// Specifies whether or not undo/redo is supported for this operation.
        /// </param>
        /// <param name="deselectActionType">
        /// Specifies the way in which the objects are deselected (the type of clear action
        /// we are dealing with).
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool ClearSelection(bool allowUndoRedo, ObjectDeselectActionType deselectActionType)
        {
            if (NumberOfSelectedObjects != 0)
            {
                List<GameObject> previouslySelectedObjects = new List<GameObject>(SelectedGameObjects);
                if (allowUndoRedo)
                {
                    // Take a pre-change snapshot
                    var preChangeSnapshot = new ObjectSelectionSnapshot();
                    preChangeSnapshot.TakeSnapshot();

                    // Clear the selection
                    _selectedObjects.Clear();
                    _lastSelectedGameObject = null;

                    // Take a post-change snapshot
                    var postChangeSnapshot = new ObjectSelectionSnapshot();
                    postChangeSnapshot.TakeSnapshot();

                    // Execute the post-change action to allow for undo/redo
                    var action = new PostObjectSelectionChangedAction(preChangeSnapshot, postChangeSnapshot);
                    action.Execute();
                }
                else
                {
                    _selectedObjects.Clear();
                    _lastSelectedGameObject = null;
                }

                // Loop through all objects which were previously selected, and call the 'OnDeselected'
                // handler for those objects that have a 'IRTEditorEventListener' component attached to them.
                var deselectEventArgs = new ObjectDeselectEventArgs(deselectActionType);
                foreach (var selectedObject in previouslySelectedObjects)
                {
                    if (selectedObject == null) continue;
                    IRTEditorEventListener editorEventListener = selectedObject.GetComponent<IRTEditorEventListener>();
                    if (editorEventListener != null) editorEventListener.OnDeselected(deselectEventArgs);
                }

                // The selection has changed
                var selChangedEventArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.None, null, deselectActionType, previouslySelectedObjects);
                OnSelectionChanged(selChangedEventArgs);

                // The selection has changed
                return true;
            }

            // The selection hasn't changed
            return false;
        }

        /// <summary>
        /// Called then the first button of the input device was released.
        /// </summary>
        private void OnInputDeviceFirstButtonUp()
        {
            // Ensure that the selection shape is no hidden
            GetObjectSelectionShape().IsVisible = false;

            // If a multi-select was performed while dragging the input device, we have more work to do
            if (_selectionChangedWithShape)
            {
                // Take post-change snapshot.
                // Note: The pre-change snapshot was taken when the left mouse button was pressed.
                _multiSelectPostChangeSnapshot = new ObjectSelectionSnapshot();
                _multiSelectPostChangeSnapshot.TakeSnapshot();

                // Execute the post seelection changed action to allow for undo/redo
                var action = new PostObjectSelectionChangedAction(_multiSelectPreChangeSnapshot, _multiSelectPostChangeSnapshot);
                action.Execute();

                // Prepare for the next multi-select sesssion
                _selectionChangedWithShape = false;
            }
        }

        /// <summary>
        /// Called when the input device was moved.
        /// </summary>
        private void OnInputDeviceMoved()
        {
            // Can we perform multi-select?
            if (CanPerformMultiSelect())
            {
                // Adjust the selection shape's top left point
                Vector2 inputDevPos;
                if (!InputDevice.Instance.GetPosition(out inputDevPos)) return;
                GetObjectSelectionShape().SetEnclosingRectTopLeftPoint(inputDevPos);

                // Retrieve all the objects which are visible to the camera, and then check which one of these
                // resides inside the selection shape. The resulting object collection is fed into the 
                // 'HandleObjectsEnteredSelectionShape' to decide what needs to be done.
                List<GameObject> objectsVisibleToCamera = EditorCamera.Instance.GetVisibleGameObjects();
                List<GameObject> objectsInSelectionShape = GetObjectSelectionShape().GetIntersectingGameObjects(objectsVisibleToCamera, EditorCamera.Instance.Camera);
                if (HandleObjectsInSelectionShape(objectsInSelectionShape)) _selectionChangedWithShape = true;
            }
        }

        /// <summary>
        /// Called for a collection of objects which reside inside the selection shape.
        /// </summary>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool HandleObjectsInSelectionShape(List<GameObject> objectCollection)
        {
            // Proceed based on what keys are active. If multi deselect is active, remove the objects from the selection.
            if (_multiDeselectShortcut.IsActive())
            {
                bool wasChanged = RemoveObjectCollectionFromSelection(objectCollection, ObjectDeselectActionType.MultiDeselect);
                if (wasChanged)
                {
                    var selChangedArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.None, new List<GameObject>(_selectedObjects),
                                                                             ObjectDeselectActionType.MultiDeselect, objectCollection);
                    OnSelectionChanged(selChangedArgs);
                }

                return wasChanged;
            }
            else
            if (_appendToSelectionShortcut.IsActive())
            {
                bool wasChanged = AddObjectCollectionToSelection(objectCollection, ObjectSelectActionType.MultiSelectAppend);   // Append the objects to the selection if the append key is active
                if (wasChanged)
                {
                    var selChangedArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.MultiSelectAppend, new List<GameObject>(_selectedObjects),
                                                                             ObjectDeselectActionType.None, new List<GameObject>());
                    OnSelectionChanged(selChangedArgs);
                }

                return wasChanged;
            }
            else
            {
                bool objectsWereRemoved = false;
                bool objectsWereAdded = false;

                var preChangeSnapshot = new ObjectSelectionSnapshot();
                preChangeSnapshot.TakeSnapshot();

                // Deselect the objects which are no longer inside the selection shape
                var currentlySelected = SelectedGameObjects;
                foreach (var selectedObject in currentlySelected)
                {
                    if (!objectCollection.Contains(selectedObject))
                    {
                        bool changed = RemoveObjectFromSelection(selectedObject, ObjectDeselectActionType.MultiSelectNotInRect);
                        if (changed) objectsWereRemoved = true;
                    }
                }

                // Select the ones inside the shape, but which are not alread selected
                List<GameObject> filteredCollection = new List<GameObject>(objectCollection);
                filteredCollection.RemoveAll(item => IsObjectSelected(item));
                objectsWereAdded = AddObjectCollectionToSelection(filteredCollection, ObjectSelectActionType.MultiSelect);

                bool wasChanged = objectsWereRemoved || objectsWereAdded;
                if (wasChanged)
                {
                    var postChangeSnapshot = new ObjectSelectionSnapshot();
                    postChangeSnapshot.TakeSnapshot();

                    var selChangedArgs = new ObjectSelectionChangedEventArgs(ObjectSelectActionType.MultiSelect, new List<GameObject>(_selectedObjects),
                                                                             ObjectDeselectActionType.MultiSelectNotInRect, preChangeSnapshot.GetDiff(postChangeSnapshot));
                    OnSelectionChanged(selChangedArgs);
                }

                return wasChanged;
            }
        }

        /// <summary>
        /// Attempts to pick a game object in the scene based on the input device's 
        /// poisition (mouse cursor or touch).
        /// </summary>
        /// <returns>
        /// The picked game object or null if no object was picked.
        /// </returns>
        private GameObject PickObjectInScene()
        {
            GameObject pickedObject = null;
            GameObjectRayHit gameObjectRayHit = null;

            // Inform the system that no terrain objects must be picked if 'CanSelectTerrainObjects' is false.
            bool requireFilter = !ObjectSelectionSettings.CanSelectTerrainObjects;
            if (requireFilter) MouseCursor.Instance.PushObjectPickMaskFlags(MouseCursorObjectPickFlags.ObjectTerrain);

            // Retrieve the cursor ray hit instance and check if an object was hit
            MouseCursorRayHit cursorRayHit = MouseCursor.Instance.GetRayHit(ObjectSelectionSettings.SelectableLayers);
            if (cursorRayHit.WasAnObjectHit)
            {
                // Retrieve all object ray hits and remove all object hits that reference objects which can not be selected
                List<GameObjectRayHit> objectRayHits = cursorRayHit.SortedObjectRayHits;
                if (!ObjectSelectionSettings.CanSelectLightObjects) objectRayHits.RemoveAll(item => !item.HitObject.HasMesh() && item.HitObject.HasLight());
                if (!ObjectSelectionSettings.CanSelectParticleSystemObjects) objectRayHits.RemoveAll(item => !item.HitObject.HasMesh() && item.HitObject.HasParticleSystem());
                if (!ObjectSelectionSettings.CanSelectSpriteObjects) objectRayHits.RemoveAll(item => item.HitObject.IsSprite());
                if (!ObjectSelectionSettings.CanSelectEmptyObjects) objectRayHits.RemoveAll(item => item.HitObject.IsEmpty());

                // If we still have any ray hits, it means we have a valid picked object
                if (objectRayHits.Count != 0)
                {
                    gameObjectRayHit = objectRayHits[0];
                    pickedObject = gameObjectRayHit.HitObject;
                }
            }

            return pickedObject;
        }

        /// <summary>
        /// Retrieves the object selection shape which can be used to perform multi-select
        /// operations. 
        /// </summary>
        private ObjectSelectionShape GetObjectSelectionShape()
        {
            return _objectSelectionRectangle;
        }

        /// <summary>
        /// Calculates and returns the center of the object selection's volume.
        /// </summary>
        /// <returns>
        /// The center of the selection's volume in 3D space. The method will return
        /// the zero vector if no objects are selected or if none of the selected objects
        /// have a valid bounding volume.
        /// </returns>
        private Vector3 CalculateSelectionWorldCenter()
        {
            // When no objects are selected, return the zero vector
            if (NumberOfSelectedObjects == 0) return Vector3.zero;
            else
            {
                // Loop through all selected objects
                int numberOfObjects = 0;
                Vector3 objectCenterSum = Vector3.zero;
                foreach (GameObject selectedObject in _selectedObjects)
                {
                    // Retrieve the object's bounding box in world space. If it's valid,
                    // add it's center to the center sum.
                    Box worldAABB = selectedObject.GetWorldBox();
                    if (worldAABB.IsValid())
                    {
                        objectCenterSum += worldAABB.Center;
                        ++numberOfObjects;
                    }
                }

                // If no objects with a valid bounding box were found, return the zero vector. Otherwise,
                // divide the center sum by the number of objects found to get the average center and return
                // that to the client.
                if (numberOfObjects == 0) return Vector3.zero;
                return objectCenterSum / numberOfObjects;
            }
        }

        /// <summary>
        /// Retrieves the first object which resides inside the selected objects buffer.
        /// </summary>
        private GameObject RetrieveAGameObjectFromObjectSelectionCollection()
        {
            foreach (GameObject selectedGameObject in _selectedObjects)
            {
                return selectedGameObject;
            }
            return null;
        }

        /// <summary>
        /// Called by Unity after the scene has been rendered. It allows us to perform
        /// all necessary rendering.
        /// </summary>
        private void OnRenderObject()
        {
            // Make sure that this method is called for the right camera
            if (Camera.current != EditorCamera.Instance.Camera) return;

            // Draw the object selection boxes
            if (ObjectSelectionSettings.ObjectSelectionBoxRenderSettings.DrawBoxes)
            {
                ObjectSelectionRenderer objectSelectionRenderer = ObjectSelectionRendererFactory.Create(_objectSelectionSettings.ObjectSelectionRenderMode);
                objectSelectionRenderer.RenderObjectSelection(_selectedObjects, _objectSelectionSettings);
            }

            // Render the selectino shape
            if (ObjectSelectionSettings.CanMultiSelect) GetObjectSelectionShape().Render();
        }

        /// <summary>
        /// Returns true if any UI elements are hovered by the mouse cursor. This method
        /// is useful because it allows us to avoid selecting objects when clicking on UI 
        /// elements.
        /// </summary>
        private bool WereAnyUIElementsHovered()
        {
            if (EventSystem.current == null) return false;

            Vector2 inputDevPos;
            if (!InputDevice.Instance.GetPosition(out inputDevPos)) return false;

            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(inputDevPos.x, inputDevPos.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return results.Count != 0;
        }

        /// <summary>
        /// Called when an object is clicked in order to take appropriate action.
        /// </summary>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool HandleObjectClick(GameObject clickedObject)
        {
            // Is append to selection key active?
            if (_appendToSelectionShortcut.IsActive())
            {
                // If the object is already selected, deselect it. Otherwise, append it to the current selection
                if (IsObjectSelected(clickedObject)) return RemoveObjectFromSelection(clickedObject, ObjectDeselectActionType.ClickAlreadySelected);
                else return AddObjectToSelection(clickedObject, ObjectSelectActionType.ClickAppend);
            }
            else return ClearAndAddObjectToSelection(clickedObject, ObjectSelectActionType.Click);  // Just clear the selection and select this object only
        }

        /// <summary>
        /// Adds the specified object to the selection.
        /// </summary>
        /// <param name="selectActionType">
        /// Specifies the way in which the object is being selected.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool AddObjectToSelection(GameObject gameObj, ObjectSelectActionType selectActionType)
        {
            if (CanSelectObject(gameObj, selectActionType))
            {
                _selectedObjects.Add(gameObj);
                _lastSelectedGameObject = gameObj;
                _lastSelectActionType = selectActionType;
                _lastDeselectActionType = ObjectDeselectActionType.None;

                // If an 'IRTEditorEventListener' component is attached to the game object, call its 'OnSelected' handler
                IRTEditorEventListener editorEventListener = gameObj.GetComponent<IRTEditorEventListener>();
                if (editorEventListener != null)
                {
                    var selectEventArgs = new ObjectSelectEventArgs(selectActionType);
                    editorEventListener.OnSelected(selectEventArgs);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds the specified object collection to the selection.
        /// </summary>
        /// <param name="selectActionType">
        /// Specifies the way in which the objects are being selected.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool AddObjectCollectionToSelection(List<GameObject> objectCollection, ObjectSelectActionType selectActionType)
        {
            bool selectionWasChanged = false;
            foreach (GameObject gameObj in objectCollection)
            {
                bool wasChanged = AddObjectToSelection(gameObj, selectActionType);
                if (!selectionWasChanged && wasChanged) selectionWasChanged = true;
            }

            return selectionWasChanged;
        }

        /// <summary>
        /// Removes the specified object from the selection.
        /// </summary>
        /// <param name="deselectActionType">
        /// Specifies the way in which object is deselected.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool RemoveObjectFromSelection(GameObject gameObj, ObjectDeselectActionType deselectActionType)
        {
            if (IsObjectSelected(gameObj))
            {
                _selectedObjects.Remove(gameObj);
                _lastSelectedGameObject = RetrieveAGameObjectFromObjectSelectionCollection();
                _lastDeselectActionType = deselectActionType;
                _lastSelectActionType = ObjectSelectActionType.None;

                // If an 'IRTEditorEventListener' component is attached to the game object, call its 'OnDeselected' handler
                IRTEditorEventListener editorEventListener = gameObj.GetComponent<IRTEditorEventListener>();
                if (editorEventListener != null)
                {
                    var deselectEventArgs = new ObjectDeselectEventArgs(deselectActionType);
                    editorEventListener.OnDeselected(deselectEventArgs);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes the specified objects from the selection.
        /// </summary>
        /// <param name="deselectActionType">
        /// Specifies the way in which objects are deselected.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool RemoveObjectCollectionFromSelection(List<GameObject> objectCollection, ObjectDeselectActionType deselectActionType)
        {
            bool selectionWasChanged = false;
            foreach (GameObject gameObj in objectCollection)
            {
                bool wasChanged = RemoveObjectFromSelection(gameObj, deselectActionType);
                if (!selectionWasChanged && wasChanged) selectionWasChanged = true;
            }

            return selectionWasChanged;
        }

        /// <summary>
        /// Clears the selection and adds the specified object to the selection. 
        /// </summary>
        /// <param name="selectActionType">
        /// Specifies the way in which the object is being selected.
        /// </param>
        /// <returns>
        /// True if the selection has changed and false otherwise.
        /// </returns>
        private bool ClearAndAddObjectToSelection(GameObject gameObj, ObjectSelectActionType selectActionType)
        {
            if (!IsSelectionExactMatch(new List<GameObject> { gameObj }))
            {
                foreach (var selectedObject in _selectedObjects)
                {
                    IRTEditorEventListener editorEventListener = selectedObject.GetComponent<IRTEditorEventListener>();
                    if (editorEventListener != null)
                    {
                        var deselectEventArgs = new ObjectDeselectEventArgs(ObjectDeselectActionType.ClickSelectedOther);
                        editorEventListener.OnDeselected(deselectEventArgs);
                    }
                }

                _selectedObjects.Clear();
                AddObjectToSelection(gameObj, selectActionType);

                // We always return true, because the selection has been cleared
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the selection has changed.
        /// </summary>
        private void OnSelectionChanged(ObjectSelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (SelectionChanged != null) SelectionChanged(selectionChangedEventArgs);
        }
        #endregion

        public void JustSelect()
        {
            ObjectSelectionSettings.CanClickSelect = true;
            EditorGizmoSystem.Instance.TranslationGizmo.enabled = false;
            EditorGizmoSystem.Instance.RotationGizmo.enabled = false;
            EditorGizmoSystem.Instance.ScaleGizmo.enabled = false;
            EditorGizmoSystem.Instance.VolumeScaleGizmo.enabled = false;
        }

        public void JustSelect(Transform target)
        {
            if (target)
            {
                ClearSelection(false);
                AddObjectToSelection(target.gameObject, false);
            }
            JustSelect();
        }

        public GizmoType GetGizmoType()
        {
            return EditorGizmoSystem.Instance.ActiveGizmoType;
        }

        public void FixedSelectObj(Transform tr, GizmoType gizmoType = GizmoType.Translation)
        {
            ClearSelection(false);
            AddObjectToSelection(tr.gameObject, false);
            EditorGizmoSystem.Instance.ActiveGizmoType = gizmoType;
        }
    }
}
