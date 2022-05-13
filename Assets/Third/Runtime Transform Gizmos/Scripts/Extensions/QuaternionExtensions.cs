using UnityEngine;
using System.Collections;
using System.IO;

namespace RTEditor
{
    public static class QuaternionExtensions
    {
        #region Public Static Functions
        public static void WriteBinary(this Quaternion quaternion, BinaryWriter writer)
        {
            writer.Write(quaternion.x);
            writer.Write(quaternion.y);
            writer.Write(quaternion.z);
            writer.Write(quaternion.w);
        }

        public static Quaternion ReadBinary(BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();

            return new Quaternion(x, y, z, w);
        }
        #endregion
    }
}
