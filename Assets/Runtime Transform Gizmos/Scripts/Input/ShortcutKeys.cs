using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTEditor
{
    /// <summary>
    /// Wraps storage and functionality for shortcut keys. Shortcut keys are
    /// a combination of keys, mouse buttons and modifiers.
    /// </summary>
    [Serializable]
    public class ShortcutKeys
    {
        /// <summary>
        /// This holds a list of all possible keys which can be specified. The list is
        /// necessary so that we can filter certain keys which are less common.
        /// </summary>
        private static List<KeyCode> _availableKeys;

        /// <summary>
        /// Each element in this array has a corresponding entry in '_availableKeys' and 
        /// stores the name (string representation) of each key code.
        /// </summary>
        private static List<string> _availableKeyNames;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ShortcutKeys()
        {
            // Create the list of available keys. Start with common ones.
            _availableKeys = new List<KeyCode>();
            _availableKeys.Add(KeyCode.Space);
            _availableKeys.Add(KeyCode.Backspace);
            _availableKeys.Add(KeyCode.Return);
            _availableKeys.Add(KeyCode.Tab);
            _availableKeys.Add(KeyCode.Delete);

            // Add alpha keys
            for(int keyCode = (int)KeyCode.A; keyCode <= (int)KeyCode.Z; ++keyCode)
            {
                _availableKeys.Add((KeyCode)keyCode);
            }

            // Add alpha-numeric keys
            for (int keyCode = (int)KeyCode.Alpha0; keyCode <= (int)KeyCode.Alpha9; ++keyCode)
            {
                _availableKeys.Add((KeyCode)keyCode);
            }

            // It's also useful to be able to specify none in some situations. For example, when
            // a shortcut uses mouse buttons and mofifiers but no regular keys.
            _availableKeys.Add(KeyCode.None);

            // Build the list of available key names. This is just a matter of looping through all
            // available keys, calling 'ToString()' and adding the result in '_availableKeyNames'
            // so that a one-to-one mapping is obtained.
            _availableKeyNames = new List<string>();
            for(int keyIndex = 0; keyIndex < _availableKeys.Count; ++keyIndex)
            {
                _availableKeyNames.Add(_availableKeys[keyIndex].ToString());
            }
        }

        /// <summary>
        /// The maximum number of keys which can reside in '_keys'.
        /// </summary>
        private const int _maxNumberOfKeys = 2;

        /// <summary>
        /// The keys which are assigned to the shortcut. The size of this array will always
        /// be '_maxNumberOfKeys', but the actual number of keys which are used is dictated
        /// by '_numberOfKeys'. A shortcut is considered to be active when all '_numberOfKeys'
        /// in this array are pressed.
        /// </summary>
        [SerializeField]
        private KeyCode[] _keys = new KeyCode[_maxNumberOfKeys];

        /// <summary>
        /// Only the first '_numberOfKeys' keys in '_keys' will be taken into account. Most of the
        /// times this will probably be set to 1, but it can be modified in the inspector if more
        /// than one key is needed. The value of this varaible will always be <= to '_maxNumberOfKeys'.
        /// </summary>
        [SerializeField]
        private int _numberOfKeys = 1;

        /// <summary>
        /// Boolean flags for all modifier keys. When a flag is true, it means that
        /// the corresponding modifier key must be pressed in order for the shortcut
        /// key to be active.
        /// </summary>
        [SerializeField]
        private bool _lCtrl = false;
        [SerializeField]
        private bool _lCmd = false;
        [SerializeField]
        private bool _lAlt = false;
        [SerializeField]
        private bool _lShift = false;

        /// <summary>
        /// Useful when the user would like to turn on/off all modifier keys. If set to false,
        /// all modifier keys are ignored.
        /// </summary>
        [SerializeField]
        private bool _useModifiers = true;

        /// <summary>
        /// Normally, if a shortcut doesn't use modifiers (i.e. _useModifiers = false or all modifier flags 
        /// are set to false), it is considered to be active if all its keys and mouse buttons are pressed, 
        /// regardless of the state of the modifier keys. However, it may sometimes be desirable to enforce 
        /// a stricter type of checking. Namely, if no modifiers are specified for a shortcut key, then it
        /// should only be active when no modifier keys are pressed. This rule can be enforced by settings
        /// this property to true.
        /// </summary>
        [SerializeField]
        private bool _useStrictModifierCheck = false;

        /// <summary>
        /// Boolean flags for all mouse buttons.
        /// </summary>
        [SerializeField]
        private bool _lMouseBtn = false;
        [SerializeField]
        private bool _rMouseBtn = false;
        [SerializeField]
        private bool _mMouseBtn = false;

        /// <summary>
        /// Useful when the user would like to turn on/off all mouse buttons. If set to false,
        /// all mouse buttons are ignored.
        /// </summary>
        [SerializeField]
        private bool _useMouseButtons = true;

        /// <summary>
        /// Same as '_useStrictModifierCheck'.
        /// </summary>
        [SerializeField]
        private bool _useStrictMouseCheck = false;

        /// <summary>
        /// The shortcut name.
        /// </summary>
        [SerializeField]
        private string _name = "ShortcutKeys";

        public static List<KeyCode> AvailableKeys { get { return new List<KeyCode>(_availableKeys); } }
        public static List<string> AvailableKeyNames { get { return new List<string>(_availableKeyNames); } }

        public string Name { get { return _name; } }
        public KeyCode Key0 { get { return _keys[0]; } set { if (_availableKeys.Contains(value)) _keys[0] = value; } }
        public KeyCode Key1 { get { return _keys[1]; } set { if (_availableKeys.Contains(value)) _keys[1] = value; } }
        public bool LCtrl { get { return _lCtrl; } set { _lCtrl = value; } }
        public bool LCmd { get { return _lCmd; } set { _lCmd = value; } }
        public bool LAlt { get { return _lAlt; } set { _lAlt = value; } }
        public bool LShift { get { return _lShift; } set { _lShift = value; } }
        public bool LMouseButton { get { return _lMouseBtn; } set { _lMouseBtn = value; } }
        public bool RMouseButton { get { return _rMouseBtn; } set { _rMouseBtn = value; } }
        public bool MMouseButton { get { return _mMouseBtn; } set { _mMouseBtn = value; } }
        public bool UseModifiers { get { return _useModifiers; } set { _useModifiers = value; } }
        public bool UseMouseButtons { get { return _useMouseButtons; } set { _useMouseButtons = value; } }
        public bool UseStrictMouseCheck { get { return _useStrictMouseCheck; } set { _useStrictMouseCheck = value; } }
        public bool UseStrictModifierCheck { get { return _useStrictModifierCheck; } set { _useStrictModifierCheck = value; } }
        public int NumberOfKeys { get { return _numberOfKeys; } set { _numberOfKeys = Mathf.Min(Mathf.Max(0, value), _maxNumberOfKeys); } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">
        /// The name of the shortcut.
        /// </param>
        /// <param name="numberOfKeys">
        /// The initial number of keys.
        /// </param>
        public ShortcutKeys(string name, int numberOfKeys)
        {
            // Store the name and the number of keys
            _name = name;
            _numberOfKeys = numberOfKeys;

            // Initialize all keys to None
            for(int keyIndex = 0; keyIndex < _maxNumberOfKeys; ++keyIndex)
            {
                _keys[keyIndex] = KeyCode.None;
            }
        }

        /// <summary>
        /// Returns true if the shortcut keys are active (all keys, modifiers and
        /// mouse buttons are pressed) at the moment this method is called. The 
        /// method will take into account the strict checking flags.
        /// </summary>
        public bool IsActive()
        {
            // If no keys, mouse buttons or modifiers were specified, the shortcut can not possibly be active
            if (IsEmpty()) return false;

            // Loop through each key and check if it is active. If we find a key that is not active, we can
            // return false. Note: Keys that are set to 'None' can be ignored as they have no contribution.
            for (int keyIndex = 0; keyIndex < _numberOfKeys; ++keyIndex )
            {
                if (_keys[keyIndex] != KeyCode.None && !Input.GetKey(_keys[keyIndex])) return false;
            }
         
            // If strict modifier check is used and no modifiers are used, but at least one modifier key is pressed,
            // it means the key is not active and we return false.
            if (UseStrictModifierCheck && (!_useModifiers || HasNoModifiers()) && IsAnyModifierKeyPressed()) return false;

            // If modifiers are used, we have to check if the corresponding modifier keys are pressed
            if (_useModifiers)
            {
                if (_lCtrl && !Input.GetKey(KeyCode.LeftControl)) return false;
                if (_lCmd && !Input.GetKey(KeyCode.LeftCommand)) return false;
                if (_lAlt && !Input.GetKey(KeyCode.LeftAlt)) return false;
                if (_lShift && !Input.GetKey(KeyCode.LeftShift)) return false;
            }
          
            // Perform the mouse button strict check in the same way we did for the modifier keys
            if (UseStrictMouseCheck && (!_useMouseButtons || HasNoMouseButtons()) && IsAnyMouseButtonPressed()) return false;

            // If mouse buttons are used, check if the corresponding mouse buttons are pressed
            if(_useMouseButtons)
            {
                if (_lMouseBtn && !Input.GetMouseButton((int)MouseButton.Left)) return false;
                if (_rMouseBtn && !Input.GetMouseButton((int)MouseButton.Right)) return false;
                if (_mMouseBtn && !Input.GetMouseButton((int)MouseButton.Middle)) return false;
            }

            // All tests have passed, so the shortcut is active
            return true;
        }

        /// <summary>
        /// Returns true if the shortcut keys are active (all keys, modifiers and
        /// mouse buttons are pressed) during the current frame. The method will
        /// take into account the strict checking flags.
        /// </summary>
        public bool IsActiveInCurrentFrame()
        {
            // If no keys, mouse buttons or modifiers were specified, the shortcut can not possibly be active
            if (IsEmpty()) return false;

            // Loop through each key and check if it is active. If we find a key that is not active, we can
            // return false. Note: Keys that are set to 'None' can be ignored as they have no contribution.
            for (int keyIndex = 0; keyIndex < _numberOfKeys; ++keyIndex)
            {
                if (_keys[keyIndex] != KeyCode.None && !Input.GetKeyDown(_keys[keyIndex])) return false;
            }

            // If strict modifier check is used and no modifiers are used, but at least one modifier key is pressed,
            // it means the key is not active and we return false.
            if (UseStrictModifierCheck && (!_useModifiers || HasNoModifiers()) && IsAnyModifierKeyPressed()) return false;

            // If modifiers are used, we have to check if the corresponding modifier keys are pressed
            if(_useModifiers)
            {
                if (_lCtrl && !Input.GetKey(KeyCode.LeftControl)) return false;
                if (_lCmd && !Input.GetKey(KeyCode.LeftCommand)) return false;
                if (_lAlt && !Input.GetKey(KeyCode.LeftAlt)) return false;
                if (_lShift && !Input.GetKey(KeyCode.LeftShift)) return false;
            }

            // Perform the mouse button strict check in the same way we did for the modifier keys
            if (UseStrictMouseCheck && (!_useMouseButtons || HasNoMouseButtons()) && IsAnyMouseButtonPressed()) return false;

            // If mouse buttons are used, check if the corresponding mouse buttons are pressed
            if(_useMouseButtons)
            {
                if (_lMouseBtn && !Input.GetMouseButtonDown((int)MouseButton.Left)) return false;
                if (_rMouseBtn && !Input.GetMouseButtonDown((int)MouseButton.Right)) return false;
                if (_mMouseBtn && !Input.GetMouseButtonDown((int)MouseButton.Middle)) return false;
            }

            // All tests have passed, so the shortcut is active
            return true;
        }

        /// <summary>
        /// Checks if the shortcut has any keys assigned to it. Note: This method
        /// checks if all keys inside the keys array are set to 'None'. 
        /// </summary>
        public bool HasNoKeys()
        {
            foreach(var key in _keys)
            {
                if (key != KeyCode.None) return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the shortcut has any modifiers assigned to it. Note: This method
        /// checks if all modifier flags are set to false. The 'UseModifiers' property 
        /// is ignored.
        /// </summary>
        public bool HasNoModifiers()
        {
            return !_lAlt && !_lCmd && !_lCtrl && !_lShift;
        }

        /// <summary>
        /// Checks if the shortcut has any mouse buttons assigned to it. Note: This method
        /// checks if all mouse button flags are set to false. The 'UseMouseButtons' property 
        /// is ignored.
        /// </summary>
        public bool HasNoMouseButtons()
        {
            return !_lMouseBtn && !_rMouseBtn && !_mMouseBtn;
        }

        /// <summary>
        /// Checks if no keys, mouse buttons or modifier keys are available for this shortcut. 
        /// The method takes the 'UseModifiers' and 'UseMouseButtons' into account. So, for 
        /// example, if the shortcut has 2 modifiers assigned to it, but 'UseModifiers' is
        /// set to false, it is as if no modifiers were specified.
        /// </summary>
        public bool IsEmpty()
        {
            return HasNoKeys() && (!_useModifiers || HasNoModifiers()) && (!_useMouseButtons || HasNoMouseButtons());
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Renders the shortcut view inside the Inspector.
        /// </summary>
        /// <param name="parentMono">
        /// The parent Mono object which holds a reference to the shortcut.
        /// </param>
        public void RenderView(MonoBehaviour parentMono)
        {
            const int indentAmount = 1;

            // Shortcut name
            GUIStyle style = new GUIStyle("label");
            style.fontStyle = FontStyle.Bold;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("[" + Name + "]", style);

            // Number of keys
            int newNumKeys = EditorGUILayout.IntField("Num keys", NumberOfKeys);
            if (newNumKeys != NumberOfKeys)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                NumberOfKeys = newNumKeys;
            }

            // For each possible key, let the user specify its key code
            for(int keyIndex = 0; keyIndex < _numberOfKeys; ++keyIndex)
            {
                int selectedIndex = _availableKeyNames.IndexOf(_keys[keyIndex].ToString());
                int newIndex = EditorGUILayout.Popup("Key" + keyIndex.ToString(), selectedIndex, _availableKeyNames.ToArray());
                if (newIndex != selectedIndex)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _keys[keyIndex] = _availableKeys[newIndex];
                }
            }

            // Modifiers
            bool newBool = EditorGUILayout.ToggleLeft("<Modifiers>", _useModifiers);
            if (_useModifiers != newBool)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _useModifiers = newBool;
            }
            if(_useModifiers)
            {
                EditorGUI.indentLevel += indentAmount;
                EditorGUILayout.BeginVertical();
                newBool = EditorGUILayout.ToggleLeft(KeyCode.LeftControl.ToString(), _lCtrl);
                if (newBool != _lCtrl)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _lCtrl = newBool;
                }
                newBool = EditorGUILayout.ToggleLeft(KeyCode.LeftCommand.ToString(), _lCmd);
                if (newBool != _lCmd)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _lCmd = newBool;
                }
                newBool = EditorGUILayout.ToggleLeft(KeyCode.LeftAlt.ToString(), _lAlt);
                if (newBool != _lAlt)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _lAlt = newBool;
                }
                newBool = EditorGUILayout.ToggleLeft(KeyCode.LeftShift.ToString(), _lShift);
                if (newBool != _lShift)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _lShift = newBool;
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= indentAmount;
            }

            // Mouse buttons
            newBool = EditorGUILayout.ToggleLeft("<Mouse buttons>", _useMouseButtons);
            if (_useMouseButtons != newBool)
            {
                UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                _useMouseButtons = newBool;
            }
            if(_useMouseButtons)
            {
                EditorGUI.indentLevel += indentAmount;
                EditorGUILayout.BeginVertical();
                newBool = EditorGUILayout.ToggleLeft(MouseButton.Left.ToString(), _lMouseBtn);
                if (newBool != _lMouseBtn)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _lMouseBtn = newBool;
                }
                newBool = EditorGUILayout.ToggleLeft(MouseButton.Right.ToString(), _rMouseBtn);
                if (newBool != _rMouseBtn)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _rMouseBtn = newBool;
                }
                newBool = EditorGUILayout.ToggleLeft(MouseButton.Middle.ToString(), _mMouseBtn);
                if (newBool != _mMouseBtn)
                {
                    UnityEditorUndoHelper.RecordObjectForInspectorPropertyChange(parentMono);
                    _mMouseBtn = newBool;
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel -= indentAmount;
            }

            EditorGUILayout.EndVertical();
        }
        #endif

        #region Private Methods
        /// <summary>
        /// Checks if at least one modifier key is pressed.
        /// </summary>
        private bool IsAnyModifierKeyPressed()
        {
            if (Input.GetKey(KeyCode.LeftControl)) return true;
            if (Input.GetKey(KeyCode.LeftCommand)) return true;
            if (Input.GetKey(KeyCode.LeftAlt)) return true;
            if (Input.GetKey(KeyCode.LeftShift)) return true;

            return false;
        }

        /// <summary>
        /// Checks if at least one mouse button is pressed.
        /// </summary>
        private bool IsAnyMouseButtonPressed()
        {
            if (Input.GetMouseButton((int)MouseButton.Left)) return true;
            if (Input.GetMouseButton((int)MouseButton.Right)) return true;
            if (Input.GetMouseButton((int)MouseButton.Middle)) return true;

            return false;
        }
        #endregion
    }
}
