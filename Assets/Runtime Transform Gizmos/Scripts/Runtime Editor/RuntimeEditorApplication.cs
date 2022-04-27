using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// Implements the behaviour for the runtime editor application. You can add functionality
    /// to this class to suit your own needs.
    /// </summary>
    [Serializable] [ExecuteInEditMode]
    public class RuntimeEditorApplication : MonoSingletonBase<RuntimeEditorApplication>
    {
        #region Private Variables
        /// <summary>
        /// Shortcut keys.
        /// </summary>
        [SerializeField]
        private ShortcutKeys _scrollGridUpDownShortcut = new ShortcutKeys("Scroll grid up/down", 1)
        {
            Key0 = KeyCode.Space,
            UseModifiers = false,
            UseMouseButtons = false,
            UseStrictModifierCheck = true
        };
        [SerializeField]
        private ShortcutKeys _scrollGridUpDownStepShortcut = new ShortcutKeys("Scroll grid up/down (STEP)", 1)
        {
            Key0 = KeyCode.Space,
            LCtrl = true,
            UseMouseButtons = false,
            UseStrictModifierCheck = true
        };

        [SerializeField]
        private bool _useUnityColliders = false;

        [SerializeField]
        private bool _enableUndoRedo = true;
        [SerializeField]
        private bool _useCustomCamera = false;
        [SerializeField]
        private Camera _customCamera;

        [SerializeField]
        private Vector3 _volumeSizeForLightObjects = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField]
        private Vector3 _volumeSizeForParticleSystemObjects = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField]
        private Vector3 _volumeSizeForEmptyObjects = new Vector3(0.5f, 0.5f, 0.5f);

        [SerializeField]
        private XZGrid _xzGrid = new XZGrid();
        #endregion

        #region Public Static Properties
        public static Vector3 MinObjectVolumeSize { get { return new Vector3(0.001f, 0.001f, 0.001f); } }
        #endregion

        #region Public Properties
        public ShortcutKeys ScrollGridUpDownShortcut { get { return _scrollGridUpDownShortcut; } }
        public ShortcutKeys ScrollGridUpDownStepShortcut { get { return _scrollGridUpDownStepShortcut; } }
        public bool UseUnityColliders { get { return _useUnityColliders; } set { _useUnityColliders = value; } }
        public bool EnableUndoRedo
        {
            get { return _enableUndoRedo; }
            set
            {
                #if UNITY_EDITOR
                if (Application.isEditor && !Application.isPlaying && _enableUndoRedo != value)
                {
                    _enableUndoRedo = value;
                    if (!_enableUndoRedo) DestroyAllEntities<EditorUndoRedoSystem>(FindObjectsOfType<EditorUndoRedoSystem>());
                    else CreateSubsystemObject<EditorUndoRedoSystem>(this.transform);
                }
                #endif
            }
        }
        public bool UseCustomCamera
        {
            get { return _useCustomCamera; }
            set
            {
                if (Application.isEditor && !Application.isPlaying && _useCustomCamera != value)
                {
                    _useCustomCamera = value;
                }
            }
        }
        public Camera CustomCamera
        {
            get { return _customCamera; }
            set
            {
                #if UNITY_EDITOR
                if (value != null && !value.gameObject.IsSceneObject())
                {
                    Debug.LogWarning("Only scene camera objects are allowed.");
                }
                else _customCamera = value;
                #endif
            }
        }
        public Vector3 VolumeSizeForLightObjects { get { return _volumeSizeForLightObjects; } set { _volumeSizeForLightObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public Vector3 VolumeSizeForParticleSystemObjects { get { return _volumeSizeForParticleSystemObjects; } set { _volumeSizeForParticleSystemObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public Vector3 VolumeSizeForEmptyObjects { get { return _volumeSizeForEmptyObjects; } set { _volumeSizeForEmptyObjects = Vector3.Max(MinObjectVolumeSize, value.GetVectorWithAbsComponents()); } }
        public XZGrid XZGrid { get { return _xzGrid; } }
        #endregion

        #region Private Methods
        private void Start()
        {
            // Perform init here.
            // Example:
            /*
            MeshFilter[] meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
            List<GameObject> meshObjects = new List<GameObject>();
            foreach (var meshFilter in meshFilters) meshObjects.Add(meshFilter.gameObject);
            EditorScene.Instance.MaskObjectCollectionForInteraction(meshObjects);
            */
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            EditorScene.Instance.Update(); 

            float scrollValue = Input.GetAxis("Mouse ScrollWheel");
            if(scrollValue != 0.0f)
            {
                if (_scrollGridUpDownStepShortcut.IsActive()) XZGrid.ScrollUpDownStep(scrollValue);
                else if (_scrollGridUpDownShortcut.IsActive()) XZGrid.ScrollUpDown(scrollValue);
            }
        }

        private void OnRenderObject()
        {
            if (!Application.isPlaying) return;
            _xzGrid.Render();
        }
        #endregion

        #if UNITY_EDITOR
        #region Menu Items
        /// <summary>
        /// Creates all the necessary subsystems which are needed for the runtime editor.
        /// </summary>
        [MenuItem("Tools/Runtime Transform Gizmos/Initialize")]
        private static void CreateSubsystems()
        {
            CreateRuntimeEditorApplicationSubsystems();
        }
        #endregion
        #endif

        #region Private Static Functions
        #if UNITY_EDITOR
        /// <summary>
        /// Creates all the necessary runtime editor subsystems.
        /// </summary>
        private static void CreateRuntimeEditorApplicationSubsystems()
        {
            // First, make sure all existing subsystems are destroyed
            DestroyExistingSubsystems();

            // Now, create each subsystem  
            RuntimeEditorApplication runtimeEditorApplication = CreateSubsystemObject<RuntimeEditorApplication>(null);
            Transform runtimeEditorApplicationTransform = runtimeEditorApplication.transform;

            EditorGizmoSystem editorGizmoSystem = CreateSubsystemObject<EditorGizmoSystem>(runtimeEditorApplicationTransform);
            CreateSubsystemObject<EditorObjectSelection>(runtimeEditorApplicationTransform);
            EditorCamera editorCamera = CreateSubsystemObject<EditorCamera>(runtimeEditorApplicationTransform);
            editorCamera.gameObject.AddComponent<Camera>();

            CreateSubsystemObject<EditorUndoRedoSystem>(runtimeEditorApplicationTransform);
            CreateSubsystemObject<EditorMeshDatabase>(runtimeEditorApplicationTransform);
            CreateSubsystemObject<MessageListenerDatabase>(runtimeEditorApplicationTransform);
            CreateSubsystemObject<InputDevice>(runtimeEditorApplicationTransform);
            CreateSubsystemObject<SceneGizmo>(runtimeEditorApplicationTransform);

            // Create all transform gizmos and attach them to the gizmo system
            GameObject gizmoObject = new GameObject();
            gizmoObject.name = "Translation Gizmo";
            gizmoObject.transform.parent = runtimeEditorApplicationTransform;
            TranslationGizmo translationGizmo = gizmoObject.AddComponent<TranslationGizmo>();
            editorGizmoSystem.TranslationGizmo = translationGizmo;

            gizmoObject = new GameObject();
            gizmoObject.name = "Rotation Gizmo";
            gizmoObject.transform.parent = runtimeEditorApplicationTransform;
            RotationGizmo rotationGizmo = gizmoObject.AddComponent<RotationGizmo>();
            rotationGizmo.GizmoBaseScale = 1.3f;
            editorGizmoSystem.RotationGizmo = rotationGizmo;

            gizmoObject = new GameObject();
            gizmoObject.name = "Scale Gizmo";
            gizmoObject.transform.parent = runtimeEditorApplicationTransform;
            ScaleGizmo scaleGizmo = gizmoObject.AddComponent<ScaleGizmo>();
            editorGizmoSystem.ScaleGizmo = scaleGizmo;

            gizmoObject = new GameObject();
            gizmoObject.name = "Volume Scale Gizmo";
            gizmoObject.transform.parent = runtimeEditorApplicationTransform;
            VolumeScaleGizmo volumeScaleGizmo = gizmoObject.AddComponent<VolumeScaleGizmo>();
            editorGizmoSystem.VolumeScaleGizmo = volumeScaleGizmo;
        }

        /// <summary>
        /// Destroys all existing editor subsystems.
        /// </summary>
        private static void DestroyExistingSubsystems()
        {
            DestroyAllEntities(FindObjectsOfType<RuntimeEditorApplication>());
            DestroyAllEntities(FindObjectsOfType<InputDevice>());
            DestroyAllEntities(FindObjectsOfType<EditorGizmoSystem>());
            DestroyAllEntities(FindObjectsOfType<EditorMeshDatabase>());
            DestroyAllEntities(FindObjectsOfType<EditorObjectSelection>());
            DestroyAllEntities(FindObjectsOfType<EditorCamera>());
            DestroyAllEntities(FindObjectsOfType<EditorUndoRedoSystem>());
            DestroyAllEntities(FindObjectsOfType<TranslationGizmo>());
            DestroyAllEntities(FindObjectsOfType<RotationGizmo>());
            DestroyAllEntities(FindObjectsOfType<ScaleGizmo>());
        }

        private static DataType CreateSubsystemObject<DataType>(Transform parentTransform) where DataType : MonoBehaviour
        {
            GameObject subsystemObject = new GameObject("(Singleton)" + typeof(DataType).ToString());
            subsystemObject.transform.parent = parentTransform;
            return subsystemObject.AddComponent<DataType>();
        }

        /// <summary>
        /// This function recieves a list of entities whose type must derive from 'MonoBehaviour'
        /// and destorys their associated game objects.
        /// </summary>
        private static void DestroyAllEntities<DataType>(DataType[] entitiesToDestroy) where DataType : MonoBehaviour
        {
            foreach (DataType entity in entitiesToDestroy)
            {
                DestroyImmediate(entity.gameObject);
            }
        }
        #endif
        #endregion
    }
}
