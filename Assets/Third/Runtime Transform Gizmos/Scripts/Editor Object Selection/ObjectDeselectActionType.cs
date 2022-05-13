namespace RTEditor
{
    public enum ObjectDeselectActionType
    {
        ClearSelectionCall = 0,
        SetSelectedObjectsCall,
        RemoveObjectFromSelectionCall,
        ClearClickAir,
        ClickAlreadySelected,
        ClickSelectedOther,
        MultiSelectNotInRect,
        MultiDeselect,
        SelectionDeleted,
        Undo,
        Redo,
        DeselectInactive,
        None
    }
}
