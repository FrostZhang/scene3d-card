using UnityEngine;
using System.Collections.Generic;

namespace RTEditor
{
    /// <summary>
    /// This message is sent when a gizmo transforms (i.e. translates, rotates or scales) its controlled game objects.
    /// </summary>
    public class GizmoTransformedObjectsMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo that had its controlled game objects transformed. The objects
        /// can be retrieved via a call to the 'Gizmo.ControlledObjects' property.
        /// </summary>
        private Gizmo _gizmo;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo that transformed its controlled game objects.
        /// </summary>
        public Gizmo Gizmo { get { return _gizmo; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public GizmoTransformedObjectsMessage(Gizmo gizmo)
            : base(MessageType.GizmoTransformedObjects)
        {
            _gizmo = gizmo;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transformed objects message to 
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmo">
        /// The gizmo which transformed objects.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmo)
        {
            var message = new GizmoTransformedObjectsMessage(gizmo);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when a gizmo transform operation is undone.
    /// </summary>
    public class GizmoTransformOperationWasUndoneMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </summary>
        private Gizmo _gizmoInvolvedInTransformOperation;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo which is involved in the transform operation which was undone.
        /// </summary>
        public Gizmo GizmoInvolvedInTransformOperation { get { return _gizmoInvolvedInTransformOperation; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </param>
        public GizmoTransformOperationWasUndoneMessage(Gizmo gizmoInvolvedInTransformOperation)
            : base(MessageType.GizmoTransformOperationWasUndone)
        {
            _gizmoInvolvedInTransformOperation = gizmoInvolvedInTransformOperation;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transform operation undone message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was undone.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmoInvolvedInTransformOperation)
        {
            var message = new GizmoTransformOperationWasUndoneMessage(gizmoInvolvedInTransformOperation);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when a gizmo transform operation is redone.
    /// </summary>
    public class GizmoTransformOperationWasRedoneMessage : Message
    {
        #region Private Variables
        /// <summary>
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </summary>
        private Gizmo _gizmoInvolvedInTransformOperation;
        #endregion

        #region Public Properties
        /// <summary>
        /// Returns the gizmo which is involved in the transform operation which was redone.
        /// </summary>
        public Gizmo GizmoInvolvedInTransformOperation { get { return _gizmoInvolvedInTransformOperation; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </param>
        public GizmoTransformOperationWasRedoneMessage(Gizmo gizmoInvolvedInTransformOperation)
            : base(MessageType.GizmoTransformOperationWasRedone)
        {
            _gizmoInvolvedInTransformOperation = gizmoInvolvedInTransformOperation;
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a gizmo transform operation redone message to
        /// all interested listeners.
        /// </summary>
        /// <param name="gizmoInvolvedInTransformOperation">
        /// This is the gizmo which is involved in the transform operation which was redone.
        /// </param>
        public static void SendToInterestedListeners(Gizmo gizmoInvolvedInTransformOperation)
        {
            var message = new GizmoTransformOperationWasRedoneMessage(gizmoInvolvedInTransformOperation);
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when vertex snapping is enabled.
    /// </summary>
    public class VertexSnappingEnabledMessage : Message
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexSnappingEnabledMessage()
            : base(MessageType.VertexSnappingEnabled)
        {
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a vertex snapping enabled message to
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners()
        {
            var message = new VertexSnappingEnabledMessage();
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }

    /// <summary>
    /// This message is sent when vertex snapping is disabled.
    /// </summary>
    public class VertexSnappingDisabledMessage : Message
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public VertexSnappingDisabledMessage()
            : base(MessageType.VertexSnappingDisabled)
        {
        }
        #endregion

        #region Public Static Functions
        /// <summary>
        /// Convenience function for sending a vertex snapping disabled message to
        /// all interested listeners.
        /// </summary>
        public static void SendToInterestedListeners()
        {
            var message = new VertexSnappingDisabledMessage();
            MessageListenerDatabase.Instance.SendMessageToInterestedListeners(message);
        }
        #endregion
    }
}