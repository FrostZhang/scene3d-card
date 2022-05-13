using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This class can be used to render object selection boxes for a group
    /// of selected objects using the 'ObjectSelectionBoxStyle.WireBox' style.
    /// </summary>
    public class WireObjectSelectionBoxRenderer : ObjectSelectionBoxRenderer
    {
        #region Public Methods
        /// <summary>
        /// Renders the selection boxes for the specified selected game objects.
        /// </summary>
        public override void RenderObjectSelectionBoxes(HashSet<GameObject> selectedObjects)
        {
            // Cache needed data
            EditorObjectSelection editorObjectSelecton = EditorObjectSelection.Instance;
            Material lineRenderingMaterial = MaterialPool.Instance.GLLine;
            ObjectSelectionSettings objectSelectionSettings = editorObjectSelecton.ObjectSelectionSettings;
            ObjectSelectionBoxRenderSettings objectSelectionBoxRenderSettings = objectSelectionSettings.ObjectSelectionBoxRenderSettings;

            if (objectSelectionBoxRenderSettings.SelectionBoxRenderMode == ObjectSelectionBoxRenderMode.PerObject)
            {
                List<ObjectSelectionBox> objectSelectionBoxes = ObjectSelectionBoxCalculator.CalculatePerObject(selectedObjects);
                GLPrimitives.DrawWireSelectionBoxes(objectSelectionBoxes, objectSelectionBoxRenderSettings.BoxSizeAdd,
                                                              EditorCamera.Instance.Camera, objectSelectionBoxRenderSettings.SelectionBoxLineColor, lineRenderingMaterial);
            }
            else
            if (objectSelectionBoxRenderSettings.SelectionBoxRenderMode == ObjectSelectionBoxRenderMode.FromParentToBottom)
            {
                List<ObjectSelectionBox> objectSelectionBoxes = ObjectSelectionBoxCalculator.CalculateFromParentsToBottom(selectedObjects);
                GLPrimitives.DrawWireSelectionBoxes(objectSelectionBoxes, objectSelectionBoxRenderSettings.BoxSizeAdd,
                                                    EditorCamera.Instance.Camera, objectSelectionBoxRenderSettings.SelectionBoxLineColor, lineRenderingMaterial);
            }         
        }
        #endregion
    }
}
