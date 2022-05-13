using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// The gizmo system manages all the gizmos. It controls their position and orientation
    /// based on what happens in the scene (i.e. how the user moves the mouse, what objects
    /// are selected etc). 
    /// </summary>
    public class EditorGizmoSystem : MonoSingletonBase<EditorGizmoSystem>, IMessageListener
    {
        public delegate void ActiveGizmoTypeChangedHandler(GizmoType newGizmoType);
        public event ActiveGizmoTypeChangedHandler ActiveGizmoTypeChanged;

        #region Private Variables
        /// <summary>
        /// Shortcut keys.
        /// </summary>
        [SerializeField]
        private ShortcutKeys _activateTranslationGizmoShortcut = new ShortcutKeys("Activate move gizmo", 1)
        {
            Key0 = KeyCode.W,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _activateRotationGizmoShortcut = new ShortcutKeys("Activate rotation gizmo", 1)
        {
            Key0 = KeyCode.E,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _activateScaleGizmoShortcut = new ShortcutKeys("Activate scale gizmo", 1)
        {
            Key0 = KeyCode.R,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _activateVolumeScaleGizmoShortcut = new ShortcutKeys("Activate volume scale gizmo", 1)
        {
            Key0 = KeyCode.U,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _activateGlobalTransformShortcut = new ShortcutKeys("Activate global transform", 1)
        {
            Key0 = KeyCode.G,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _activateLocalTransformShortcut = new ShortcutKeys("Activate local transform", 1)
        {
            Key0 = KeyCode.L,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _turnOffGizmosShortcut = new ShortcutKeys("Turn off gizmos", 1)
        {
            Key0 = KeyCode.Q,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };
        [SerializeField]
        private ShortcutKeys _togglePivotShortcut = new ShortcutKeys("Toggle pivot", 1)
        {
            Key0 = KeyCode.P,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictMouseCheck = true
        };

        /// <summary>
        /// The translation gizmo which is used to move objects in the scene.
        /// </summary>
        [SerializeField]
        private TranslationGizmo _translationGizmo;

        /// <summary>
        /// The rotation gizmo which is used to rotate objects in the scene.
        /// </summary>
        [SerializeField]
        private RotationGizmo _rotationGizmo;

        /// <summary>
        /// The scale gizmo which is used to scale objects in the scene.
        /// </summary>
        [SerializeField]
        private ScaleGizmo _scaleGizmo;

        [SerializeField]
        private VolumeScaleGizmo _volumeScaleGizmo;

        /// <summary>
        /// This is the gizmo that is currently being used to transform objects in the scene.
        /// </summary>
        /// <remarks>
        /// By 'active' it is meant that it was selected by the user to be used for object manipulation.
        /// It does not necessarily mean that it is active in the scene. For example, when there is no
        /// object selected in the scene, '_activeGizmo' will reference a gizmo object which is inactive
        /// in the scene.
        /// </remarks>
        private Gizmo _activeGizmo;

        /// <summary>
        /// This is the transform space in which the gizmos will be transforming their controlled objects. You
        /// can change this from the Inspector GUI to establish the initial transform space.
        /// </summary>
        [SerializeField]
        private TransformSpace _transformSpace = TransformSpace.Global;

        /// <summary>
        /// Stores the type of the currently active gizmo. You can change this in the Inspector GUI to establish
        /// the initial transform gizmo that must be activated on the first object selection operation.
        /// </summary>
        [SerializeField]
        private GizmoType _activeGizmoType = GizmoType.Translation;

        /// <summary>
        /// This is the transform pivot point which must be used by all gizmos to transform their objects.
        /// </summary>
        [SerializeField]
        private TransformPivotPoint _transformPivotPoint = TransformPivotPoint.Center;

        [SerializeField]
        private bool[] _gizmoTypeAvailableFlags = new bool[]
        {
            true, true, true, true
        };

        /// <summary>
        /// If this variable is set to true, gizmos are turned off. This means that when objects are selected,
        /// no gizmo will be shown. This mode is useful when the user would like to perform simple object selections
        /// without having to worry about the gizmos.
        /// </summary>
        private bool _areGizmosTurnedOff = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets/sets the associated translation gizmo. Settings the gizmo to null or to a prefab,
        /// will have no effect. Only non-null scene object instances are allowed.
        /// </summary>
        public TranslationGizmo TranslationGizmo
        {
            get { return _translationGizmo; }
            set
            {
                if (value == null) return;

                // Allow only scene objects
#if UNITY_EDITOR
                if (value.gameObject.IsSceneObject()) _translationGizmo = value;
                else Debug.LogWarning("RTEditorGizmoSystem.TranslationGizmo: Only scene gizmo object instances are allowed.");
#else
                _translationGizmo = value;
#endif
            }
        }

        /// <summary>
        /// Gets/sets the associated rotation gizmo. Settings the gizmo to null or to a prefab,
        /// will have no effect. Only non-null scene object instances are allowed.
        /// </summary>
        public RotationGizmo RotationGizmo
        {
            get { return _rotationGizmo; }
            set
            {
                if (value == null) return;

                // Allow only scene objects
#if UNITY_EDITOR
                if (value.gameObject.IsSceneObject()) _rotationGizmo = value;
                else Debug.LogWarning("EditorGizmoSystem.RotationGizmo: Only scene gizmo object instances are allowed.");
#else
                _rotationGizmo = value;
#endif
            }
        }

        /// <summary>
        /// Gets/sets the associated scale gizmo. Settings the gizmo to null or to a prefab,
        /// will have no effect. Only non-null scene object instances are allowed.
        /// </summary>
        public ScaleGizmo ScaleGizmo
        {
            get { return _scaleGizmo; }
            set
            {
                if (value == null) return;

                // Allow only scene objects
#if UNITY_EDITOR
                if (value.gameObject.IsSceneObject()) _scaleGizmo = value;
                else Debug.LogWarning("EditorGizmoSystem.ScaleGizmo: Only scene gizmo object instances are allowed.");
#else
                _scaleGizmo = value;
#endif
            }
        }

        public VolumeScaleGizmo VolumeScaleGizmo
        {
            get { return _volumeScaleGizmo; }
            set
            {
                if (value == null) return;

                // Allow only scene objects
#if UNITY_EDITOR
                if (value.gameObject.IsSceneObject()) _volumeScaleGizmo = value;
                else Debug.LogWarning("EditorGizmoSystem.VolumeScaleGizmo: Only scene gizmo object instances are allowed.");
#else
                _volumeScaleGizmo = value;
#endif
            }
        }

        public TransformSpace TransformSpace { get { return _transformSpace; } set { ChangeTransformSpace(value); } }
        public GizmoType ActiveGizmoType { get { return _activeGizmoType; } set { ChangeActiveGizmo(value); } }
        public Gizmo ActiveGizmo { get { return _activeGizmo; } }
        public TransformPivotPoint TransformPivotPoint { get { return _transformPivotPoint; } set { ChangeTransformPivotPoint(value); } }
        public bool AreGizmosTurnedOff { get { return _areGizmosTurnedOff; } }

        public ShortcutKeys ActivateTranslationGizmoShortcut { get { return _activateTranslationGizmoShortcut; } }
        public ShortcutKeys ActivateRotationGizmoShortcut { get { return _activateRotationGizmoShortcut; } }
        public ShortcutKeys ActivateScaleGizmoShortcut { get { return _activateScaleGizmoShortcut; } }
        public ShortcutKeys ActivateVolumeScaleGizmoShortcut { get { return _activateVolumeScaleGizmoShortcut; } }
        public ShortcutKeys ActivateGlobalTransformShortcut { get { return _activateGlobalTransformShortcut; } }
        public ShortcutKeys ActivateLocalTransformShortcut { get { return _activateLocalTransformShortcut; } }
        public ShortcutKeys TurnOffGizmosShortcut { get { return _turnOffGizmosShortcut; } }
        public ShortcutKeys TogglePivotShortcut { get { return _togglePivotShortcut; } }
        #endregion

        #region Public Methods
        public void SetGizmoTypeAvailable(GizmoType gizmoType, bool available)
        {
            _gizmoTypeAvailableFlags[(int)gizmoType] = available;

            if (!available && gizmoType == _activeGizmoType)
            {
                int firstAvailable = GetFirstAvailableGizmoTypeIndex();
                if (firstAvailable >= 0) ActiveGizmoType = (GizmoType)firstAvailable;
                else DeactivateAllGizmoObjects();
            }
        }

        public bool IsGizmoTypeAvailable(GizmoType gizmoType)
        {
            return _gizmoTypeAvailableFlags[(int)gizmoType];
        }

        public bool IsAnyGizmoTypeAvailable()
        {
            foreach (var availableFlag in _gizmoTypeAvailableFlags)
            {
                if (availableFlag) return true;
            }

            return false;
        }

        public int GetFirstAvailableGizmoTypeIndex()
        {
            for (int typeIndex = 0; typeIndex < _gizmoTypeAvailableFlags.Length; ++typeIndex)
            {
                if (_gizmoTypeAvailableFlags[typeIndex]) return typeIndex;
            }

            return -1;
        }

        /// <summary>
        /// Checks if the active gizmo is ready for object manipulation.
        /// </summary>
        /// <remarks>
        /// If the active gizmo is not active in the scene, the method returns false.
        /// </remarks>
        public bool IsActiveGizmoReadyForObjectManipulation()
        {
            if (_activeGizmo == null || !_activeGizmo.gameObject.activeSelf) return false;
            if (ActiveGizmoType == GizmoType.VolumeScale)
            {
                return VolumeScaleGizmo.IsReadyForObjectManipulation() || TranslationGizmo.IsReadyForObjectManipulation();
            }
            else
                return _activeGizmo.IsReadyForObjectManipulation();
        }

        /// <summary>
        /// This method will turn of all gizmo objects. After calling this method, no gizmo
        /// will be active in the scene, not even when the user is selecting objects.
        /// </summary>
        /// <remarks>
        /// Gizmos can be turned on again by setting the 'ActiveGizmoType' property.
        /// </remarks>
        public void TurnOffGizmos()
        {
            _areGizmosTurnedOff = true;
            DeactivateAllGizmoObjects();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Performs any necessary initializations.
        /// </summary>
        private void Start()
        {
            // Make sure all properties are valid
            ValidatePropertiesForRuntime();

            DeactivateAllGizmoObjects();                        // Initially, all gizmo objects are deactivated. Whenever the user selects the first object (or group of objects), the correct gizmo will be activated).
            ConnectObjectSelectionToGizmos();                   // Make sure the gizmos know which objects they control
            ChangeActiveGizmo(_activeGizmoType);                // Make sure we are using the correct gizmo initially 
            ChangeTransformPivotPoint(_transformPivotPoint);    // Make sure the transform pivot point is setup correctly

            // Register as listener
            MessageListenerDatabase listenerDatabase = MessageListenerDatabase.Instance;
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformedObjects, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformOperationWasUndone, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.GizmoTransformOperationWasRedone, this);
            listenerDatabase.RegisterListenerForMessage(MessageType.VertexSnappingDisabled, this);
            EditorObjectSelection.Instance.SelectionChanged += OnSelectionChanged;
        }

        private void Update()
        {
            if (_activateTranslationGizmoShortcut.IsActiveInCurrentFrame()) ChangeActiveGizmo(GizmoType.Translation);
            else
            if (_activateRotationGizmoShortcut.IsActiveInCurrentFrame()) ChangeActiveGizmo(GizmoType.Rotation);
            else
            if (_activateScaleGizmoShortcut.IsActiveInCurrentFrame()) ChangeActiveGizmo(GizmoType.Scale);
            else
            if (_activateVolumeScaleGizmoShortcut.IsActiveInCurrentFrame()) ChangeActiveGizmo(GizmoType.VolumeScale);

            if (_activateGlobalTransformShortcut.IsActiveInCurrentFrame()) ChangeTransformSpace(TransformSpace.Global);
            else if (_activateLocalTransformShortcut.IsActiveInCurrentFrame()) ChangeTransformSpace(TransformSpace.Local);

            if (_turnOffGizmosShortcut.IsActiveInCurrentFrame()) TurnOffGizmos();
            else
            if (_togglePivotShortcut.IsActiveInCurrentFrame())
            {
                TransformPivotPoint newPivotPoint = _transformPivotPoint == TransformPivotPoint.Center ? TransformPivotPoint.MeshPivot : TransformPivotPoint.Center;
                ChangeTransformPivotPoint(newPivotPoint);
            }
        }

        /// <summary>
        /// The method ensures that all properties are valid such that the gizmo system
        /// can be used at runtime.
        /// </summary>
        private void ValidatePropertiesForRuntime()
        {
            // Make sure all properties have been set up correctly
            bool allPropertiesAreValid = true;
            if (_translationGizmo == null)
            {
                Debug.LogError("EditorGizmoSystem.Start: Missing translation gizmo. Please assign a game object with the 'TranslationGizmo' script attached to it.");
                allPropertiesAreValid = false;
            }

            if (_rotationGizmo == null)
            {
                Debug.LogError("EditorGizmoSystem.Start: Missing rotation gizmo. Please assign a game object with the 'RotationGizmo' script attached to it.");
                allPropertiesAreValid = false;
            }

            if (_scaleGizmo == null)
            {
                Debug.LogError("EditorGizmoSystem.Start: Missing scale gizmo. Please assign a game object with the 'ScaleGizmo' script attached to it.");
                allPropertiesAreValid = false;
            }

            if (_volumeScaleGizmo == null)
            {
                Debug.LogError("EditorGizmoSystem.Start: Missing volume scale gizmo. Please assign a game object with the 'VolumeScaleGizmo' script attached to it.");
                allPropertiesAreValid = false;
            }

            // If not all properties have been set up correctly, we will quit the application
            if (!allPropertiesAreValid) ApplicationHelper.Quit();
        }

        /// <summary>
        /// This method is called from 'Start' in order to connect the object selection collection
        /// to each of the 3 gizmos. We need to do this because the gizmos need to know about the
        /// objects that they control.
        /// </summary>
        private void ConnectObjectSelectionToGizmos()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            objectSelection.ConnectObjectSelectionToGizmo(_translationGizmo);
            objectSelection.ConnectObjectSelectionToGizmo(_rotationGizmo);
            objectSelection.ConnectObjectSelectionToGizmo(_scaleGizmo);
            objectSelection.ConnectObjectSelectionToGizmo(_volumeScaleGizmo);
        }

        /// <summary>
        /// Deactivates all gizmo objects.
        /// </summary>
        private void DeactivateAllGizmoObjects()
        {
            _translationGizmo.gameObject.SetActive(false);
            _rotationGizmo.gameObject.SetActive(false);
            _scaleGizmo.gameObject.SetActive(false);
            _volumeScaleGizmo.gameObject.SetActive(false);
        }

        /// <summary>
        /// Changes the active gizmo to the gizmo which is identified by the specified type.
        /// </summary>
        /// <remarks>
        /// Calling this method will set the '_areGizmosTurnedOff' boolean to false (i.e. will
        /// reenable gizmos).
        /// </remarks>
        private void ChangeActiveGizmo(GizmoType gizmoType)
        {
            if (!IsGizmoTypeAvailable(gizmoType)) return;

            // Gizmos are no longer turned off
            _areGizmosTurnedOff = false;

            // We will need this later
            Gizmo oldActiveGizmo = _activeGizmo;

            // Change the active gizmo type
            bool sameGizmoType = (gizmoType == _activeGizmoType);
            _activeGizmoType = gizmoType;
            _activeGizmo = GetGizmoByType(gizmoType);

            // Deactivate the old active gizmo and make sure that the new gizmo has its position and
            // orientation updated accordingly.
            if (oldActiveGizmo != null)
            {
                // Deactivate the old gizmo
                oldActiveGizmo.gameObject.SetActive(false);

                EstablishActiveGizmoPosition();
                UpdateActiveGizmoRotation();
            }

            // If there are any objects selected, we will make sure that the new active gizmo is active in the scene.
            // If no objects are selected, we will deactivate it. We do this because we only want to draw the active
            // gizmo when there are selected objects in the scene. If no objects are selected, there is nothing to
            // transform.
            if (EditorObjectSelection.Instance.NumberOfSelectedObjects != 0) _activeGizmo.gameObject.SetActive(true);
            else _activeGizmo.gameObject.SetActive(false);

            // When the active gizmo is changed, always make sure that vertex snapping is disabled for the translation gizmo.
            // Otherwise, if you change from translation to rotation while vertex snapping is enabled and then enable the
            // translation gizmo again, it will be activated with vertex snapping enabled, which is not really desirable.
            // TODO: Remove this????
            //_translationGizmo.SnapSettings.IsVertexSnappingEnabled = false;

            if (!sameGizmoType && ActiveGizmoTypeChanged != null) ActiveGizmoTypeChanged(_activeGizmoType);
        }

        /// <summary>
        /// Changes the active transform space to the specified value.
        /// </summary>
        private void ChangeTransformSpace(TransformSpace transformSpace)
        {
            if (transformSpace == _transformSpace) return;

            // Set the new transform space and make sure the active gizmo has its rotation updated accordingly
            _transformSpace = transformSpace;
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// Changes the transform pivot point to the specified value.
        /// </summary>
        private void ChangeTransformPivotPoint(TransformPivotPoint transformPivotPoint)
        {
            if (_transformPivotPoint == transformPivotPoint) return;

            // Store the new pivot point
            _transformPivotPoint = transformPivotPoint;

            // Set the pivot point for each gizmo
            _translationGizmo.TransformPivotPoint = _transformPivotPoint;
            _rotationGizmo.TransformPivotPoint = _transformPivotPoint;
            _scaleGizmo.TransformPivotPoint = _transformPivotPoint;
            _volumeScaleGizmo.TransformPivotPoint = _transformPivotPoint;

            // Establish the position of the active gizmo
            EstablishActiveGizmoPosition();
        }

        /// <summary>
        /// The method will return one of the gizmos managed by the gizmo system which corresponds
        /// to the specified gizmo type.
        /// </summary>
        private Gizmo GetGizmoByType(GizmoType gizmoType)
        {
            if (gizmoType == GizmoType.Translation) return _translationGizmo;
            else if (gizmoType == GizmoType.Rotation) return _rotationGizmo;
            else if (gizmoType == GizmoType.Scale) return _scaleGizmo;
            return _volumeScaleGizmo;
        }

        /// <summary>
        /// This method is called whenever the position of the active gizmo needs to be updated.
        /// </summary>
        private void EstablishActiveGizmoPosition()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            if (_activeGizmo.GetGizmoType() != GizmoType.VolumeScale && _activeGizmo != null /*&& !_activeGizmo.IsTransformingObjects()*/) // TODO: Why was this here? Seems that it is not necessary...
            {
                // Update the position based on the specified transform pivot point. If the transform pivot
                // point is set to 'MeshPivot', we will set the position of the gizmo to the position of the
                // last selected game objects. Otherwise, we will set it to the center of the selection.
                if (_transformPivotPoint == TransformPivotPoint.MeshPivot && objectSelection.LastSelectedGameObject != null) _activeGizmo.transform.position = objectSelection.LastSelectedGameObject.transform.position;
                else _activeGizmo.transform.position = objectSelection.GetSelectionWorldCenter();
            }

            if (_volumeScaleGizmo != null && objectSelection.NumberOfSelectedObjects == 1)
            {
                _volumeScaleGizmo.transform.position = objectSelection.LastSelectedGameObject.transform.position;
                _volumeScaleGizmo.RefreshTargets();
            }
        }

        /// <summary>
        /// Updates the rotation of the active gizmo by taking into consideration
        /// all necessary factors such as the active gizmo transform space.
        /// </summary>
        private void UpdateActiveGizmoRotation()
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            if (_activeGizmoType == GizmoType.VolumeScale)
            {
                if (objectSelection.NumberOfSelectedObjects == 1)
                    _activeGizmo.transform.rotation = objectSelection.LastSelectedGameObject.transform.rotation;
                return;
            }
            else
            {
                // If the global transform space is used, we will set the gizmo's rotation to identity. Otherwise,
                // we will set the rotation to the rotation of the last object which was selected in the scene.
                // Note: The scale gizmo will always be oriented in the last selected object's local space because
                //       the scale gizmo always scales along the objects' local axes.
                if ((_transformSpace == TransformSpace.Global && _activeGizmoType != GizmoType.Scale) || objectSelection.LastSelectedGameObject == null) _activeGizmo.transform.rotation = Quaternion.identity;
                else _activeGizmo.transform.rotation = objectSelection.LastSelectedGameObject.transform.rotation;
            }
        }
        #endregion

        /// <summary>
        ///当主动更改模型的位置坐标等信息时，Gizmo没有更新，手动刷新  Asher
        /// </summary>
        public void Reflush()
        {
            EstablishActiveGizmoPosition();
            UpdateActiveGizmoRotation();
        }

        private void OnSelectionChanged(ObjectSelectionChangedEventArgs selChangedEventArgs)
        {
            EditorObjectSelection objectSelection = EditorObjectSelection.Instance;
            if (_activeGizmo == null) return;

            // If no objects are selected, we will deactivate the active gizmo
            if (objectSelection.NumberOfSelectedObjects == 0)
            {
                if (ActiveGizmoType == GizmoType.VolumeScale)
                {
                    _activeGizmo.gameObject.SetActive(false);
                    TranslationGizmo.gameObject.SetActive(false);
                }
                else
                    _activeGizmo.gameObject.SetActive(false);
            }
            else
            // If the gizmos are not turned off, we may need to enable the active gizmo in the scene
            if (!_areGizmosTurnedOff)
            {
                // If there are objects selected, we will make sure the active gizmo is enabled in the scene.
                if (objectSelection.NumberOfSelectedObjects != 0 && !_activeGizmo.gameObject.activeSelf) _activeGizmo.gameObject.SetActive(true);
            }

            // Make sure the position of the active gizmo is updated correctly
            EstablishActiveGizmoPosition();

            // Now we must make sure that the active gizmo is oriented accordingly
            UpdateActiveGizmoRotation();
        }

        #region Message Handlers
        /// <summary>
        /// 'IMessageListener' interface method implementation.
        /// </summary>
        public void RespondToMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.GizmoTransformedObjects:

                    RespondToMessage(message as GizmoTransformedObjectsMessage);
                    break;

                case MessageType.GizmoTransformOperationWasUndone:

                    RespondToMessage(message as GizmoTransformOperationWasUndoneMessage);
                    break;

                case MessageType.GizmoTransformOperationWasRedone:

                    RespondToMessage(message as GizmoTransformOperationWasRedoneMessage);
                    break;

                case MessageType.VertexSnappingDisabled:

                    RespondToMessage(message as VertexSnappingDisabledMessage);
                    break;
            }
        }

        private void RespondToMessage(GizmoTransformedObjectsMessage message)
        {
            UpdateActiveGizmoRotation();
            EstablishActiveGizmoPosition();
        }

        /// <summary>
        /// This method is called to respond to a gizmo transform operation undone message.
        /// </summary>
        private void RespondToMessage(GizmoTransformOperationWasUndoneMessage message)
        {
            // When the transform operation is undone, it means the objects position/rotation/scale
            // has changed, so we have to recalculate the position and orientation of the active gizmo.
            EstablishActiveGizmoPosition();
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// This method is called to respond to a gizmo transform operation redone message.
        /// </summary>
        private void RespondToMessage(GizmoTransformOperationWasRedoneMessage message)
        {
            // When the transform operation is redone, it means the objects position/rotation/scale
            // has changed, so we have to recalculate the position and orientation of the active gizmo.
            EstablishActiveGizmoPosition();
            UpdateActiveGizmoRotation();
        }

        /// <summary>
        /// This method is called to respond to a vertex snapping disabled message.
        /// </summary>
        private void RespondToMessage(VertexSnappingDisabledMessage message)
        {
            // When vertex snapping is disabled, make sure that the active gizmo is positioned
            // accordingly because when vertex snapping is used, its position my change to that
            // of the object mesh vertices.
            EstablishActiveGizmoPosition();
        }
        #endregion
    }
}