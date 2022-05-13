using UnityEngine;
using System;

namespace RTEditor
{
    [Serializable]
    public class ObjectSelectionSettings
    {
        #region Private Variables
        [SerializeField]
        private ObjectSelectionRenderMode _objectSelectionRenderMode = ObjectSelectionRenderMode.SelectionBoxes;
        [SerializeField]
        private int _selectableLayers = ~0;
        [SerializeField]
        private int _duplicatableLayers = ~0;

        [SerializeField]
        private bool _canSelectTerrainObjects = false;
        [SerializeField]
        private bool _canSelectLightObjects = false;
        [SerializeField]
        private bool _canSelectParticleSystemObjects = false;
        [SerializeField]
        private bool _canSelectSpriteObjects = true;
        [SerializeField]
        private bool _canSelectEmptyObjects = false;
        [SerializeField]
        private bool _canClickSelect = true;
        [SerializeField]
        private bool _canMultiSelect = true;

        [SerializeField]
        private ObjectSelectionBoxRenderSettings _objectSelectionBoxRenderSettings = new ObjectSelectionBoxRenderSettings();
        #endregion

        #region Public Properties
        public ObjectSelectionRenderMode ObjectSelectionRenderMode { get { return _objectSelectionRenderMode; } set { _objectSelectionRenderMode = value; } }
        public int SelectableLayers { get { return _selectableLayers; } set { _selectableLayers = value; } }
        public int DuplicatableLayers { get { return _duplicatableLayers; } set { _duplicatableLayers = value; } }
        public bool CanSelectTerrainObjects { get { return _canSelectTerrainObjects; } set { _canSelectTerrainObjects = value; } }
        public bool CanSelectLightObjects { get { return _canSelectLightObjects; } set { _canSelectLightObjects = value; } }
        public bool CanSelectParticleSystemObjects { get { return _canSelectParticleSystemObjects; } set { _canSelectParticleSystemObjects = value; } }
        public bool CanSelectSpriteObjects { get { return _canSelectSpriteObjects; } set { _canSelectSpriteObjects = value; } }
        public bool CanSelectEmptyObjects { get { return _canSelectEmptyObjects; } set { _canSelectEmptyObjects = value; } }
        public bool CanClickSelect { get { return _canClickSelect; } set { _canClickSelect = value; } }
        public bool CanMultiSelect { get { return _canMultiSelect; } set { _canMultiSelect = value; } }
        public ObjectSelectionBoxRenderSettings ObjectSelectionBoxRenderSettings { get { return _objectSelectionBoxRenderSettings; } }
        #endregion
    }
}
