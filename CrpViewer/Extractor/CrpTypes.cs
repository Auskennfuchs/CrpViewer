using System.Collections.Generic;
using System.Text;
using SharpDX;

namespace CrpExtractor.Types {

    public struct Vector2 {
        public float x;
        public float y;

        public Vector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public override string ToString() {
            return string.Format("X:{0} Y:{0}\n", x, y);
        }
    }

    public struct Color {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a) {
            this.r = r;
            this.b = g;
            this.g = b;
            this.a = a;
        }
    }

    public class Matrix4x4 {
        public float[] entries = new float[16];
    }

    public class Boneweight {
        public int[] indicies = new int[4];
        public float[] weights = new float[4];
    }

    public struct Transform {
        public Vector3 position;
        public Vector4 rotation;
        public Vector3 scale;
    }

    public class Mesh {
        public Matrix4x4[] bindPoses;
        public Boneweight[] boneWeights;
        public Color[] colors;
        public Vector3[] normals;
        public int subMeshCount;
        public Vector4[] tangents;
        public List<int> triangles = new List<int>();
        public Vector2[] uv;
        public Vector3[] vertices;

        //Taken from:http://wiki.unity3d.com/index.php?title=ObjExporter
        public string exportObj() {
            StringBuilder sb = new StringBuilder();
            if (vertices != null) {
                foreach (Vector3 v in vertices) {
                    sb.Append(string.Format("v {0:0.000000000} {1:0.000000000} {2:0.000000000}\n", v.X, v.Y, v.Z));
                }
            }

            sb.Append("\n");
            if (normals != null) {
                foreach (Vector3 v in normals) {
                    sb.Append(string.Format("vn {0:0.000000000} {1:0.000000000} {2:0.000000000}\n", v.X, v.Y, v.Z));
                }
            }

            sb.Append("\n");
            if (uv != null) {
                foreach (Vector2 v in uv) {
                    sb.Append(string.Format("vt {0:0.000000000} {1:0.000000000}\n", v.x, v.y));
                }
            }

            if (subMeshCount != 0) {
                for (int i = 0; i < subMeshCount; i++) {
                    sb.Append(string.Format("g {0}", i));
                    sb.Append("\n");

                    for (int j = 0; j < triangles.Count; j += 3) {


                        sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                            triangles[j] + 1, triangles[j + 1] + 1, triangles[j + 2] + 1));
                    }
                }
            }

            return sb.ToString();
        }
    }

    public class MaterialStub {
        public string shaderName;
        public int numProperties;
        public Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public Dictionary<string, Vector4> vectors = new Dictionary<string, Vector4>();
        public Dictionary<string, float> floats = new Dictionary<string, float>();
        public Dictionary<string, string> textures = new Dictionary<string, string>();

    }
}
