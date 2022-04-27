namespace RTEditor
{
    /// <summary>
    /// This enum holds the possible types of messages that can be sent to listeners.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// This message can be sent when a gizmo transforms the game objects it controls.
        /// </summary>
        GizmoTransformedObjects = 0,

        /// <summary>
        /// This message is sent when the transform applied to a collection of game objects
        /// via a transform gizmo is undone.
        /// </summary>
        GizmoTransformOperationWasUndone,

        /// <summary>
        /// This message is sent when the transform applied to a collection of game objects
        /// via a transform gizmo is redone.
        /// </summary>
        GizmoTransformOperationWasRedone,

        /// <summary>
        /// This message is sent when vertex snapping is enabled.
        /// </summary>
        VertexSnappingEnabled,

        /// <summary>
        /// This message is sent when vertex snapping is disabled.
        /// </summary>
        VertexSnappingDisabled,
    }
}