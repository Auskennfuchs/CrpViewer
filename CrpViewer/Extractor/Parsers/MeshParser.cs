using CrpExtractor.Types;

namespace CrpExtractor.Parsers
{
    class MeshParser : BinaryParser
    {
        public static Mesh parseMesh(CrpReader reader, long fileSize)
        {
            long fileContentBegin = reader.BaseStream.Position;

            Mesh retVal = new Mesh();
            retVal.vertices = reader.readUnityArray("UnityEngine.Vector3");
            retVal.colors = reader.readUnityArray("UnityEngine.Color");
            retVal.uv = reader.readUnityArray("UnityEngine.Vector2");
            retVal.normals = reader.readUnityArray("UnityEngine.Vector3");
            retVal.tangents = reader.readUnityArray("UnityEngine.Vector4");
            retVal.boneWeights = reader.readUnityArray("UnityEngine.BoneWeight");
            retVal.bindPoses = reader.readUnityArray("UnityEngine.Matrix4x4");
            retVal.subMeshCount = reader.ReadInt32();
            for (int i = 0; i < retVal.subMeshCount; i++)
            {
                int[] triangles = reader.readUnityArray("System.Int32");
                retVal.triangles.AddRange(triangles);
            }

            ReadUntil( reader, fileContentBegin, fileSize );

            return retVal;
        }

    }
}
