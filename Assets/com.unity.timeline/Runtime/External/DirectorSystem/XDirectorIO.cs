using System.IO;

namespace UnityEngine.Timeline
{
    public static class BinaryReaderExtend
    {
        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            Vector3 vec = Vector3.zero;
            vec.x = reader.ReadSingle();
            vec.y = reader.ReadSingle();
            vec.z = reader.ReadSingle();
            return vec;
        }


        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            Vector2 vec = Vector2.zero;
            vec.x = reader.ReadSingle();
            vec.y = reader.ReadSingle();
            return vec;
        }


        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            Vector4 vec = Vector4.zero;
            vec.x = reader.ReadSingle();
            vec.y = reader.ReadSingle();
            vec.z = reader.ReadSingle();
            vec.w = reader.ReadSingle();
            return vec;
        }

        public static Vector3Int ReadVector3Int(this BinaryReader reader)
        {
            Vector3Int vec = Vector3Int.zero;
            vec.x = reader.ReadInt32();
            vec.y = reader.ReadInt32();
            vec.z = reader.ReadInt32();
            return vec;
        }

        public static Vector2Int ReadVector2Int(this BinaryReader reader)
        {
            Vector2Int vec = Vector2Int.zero;
            vec.x = reader.ReadInt32();
            vec.y = reader.ReadInt32();
            return vec;
        }

    }

    public static class BinaryWriterExtend
    {
        public static void Write(this BinaryWriter writer, Vector3 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
        }

        public static void Write(this BinaryWriter writer, Vector2 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
        }

        public static void Write(this BinaryWriter writer, Vector4 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
            writer.Write(vec.w);
        }

        public static void Write(this BinaryWriter writer, Vector3Int vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
        }

        public static void Write(this BinaryWriter writer, Vector2Int vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
        }
        

    }

}