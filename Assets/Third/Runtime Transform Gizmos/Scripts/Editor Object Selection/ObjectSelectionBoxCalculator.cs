using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    public static class ObjectSelectionBoxCalculator 
    {
        public static List<ObjectSelectionBox> CalculatePerObject(IEnumerable<GameObject> selectedObjects)
        {
            var objectSelectionBoxes = new List<ObjectSelectionBox>(20);
            foreach(var gameObject in selectedObjects)
            {
                Box objectModelSpaceBox = gameObject.GetModelSpaceBox();
                if(objectModelSpaceBox.IsValid())
                {
                    objectSelectionBoxes.Add(new ObjectSelectionBox(objectModelSpaceBox, gameObject.transform.localToWorldMatrix));
                }
            }
            return objectSelectionBoxes;
        }

        public static List<ObjectSelectionBox> CalculateFromParentsToBottom(IEnumerable<GameObject> selectedObjects)
        {
            List<GameObject> parents = GameObjectExtensions.GetParentsFromObjectCollection(selectedObjects);
            var objectSelectionBoxes = new List<ObjectSelectionBox>(20);
            foreach (var parent in parents)
            {
                Box hierarchyModelSpaceBox = parent.GetHierarchyModelSpaceBox();
                if (hierarchyModelSpaceBox.IsValid())
                {
                    objectSelectionBoxes.Add(new ObjectSelectionBox(hierarchyModelSpaceBox, parent.transform.localToWorldMatrix));
                }
            }
            return objectSelectionBoxes;
        }
    }
}
