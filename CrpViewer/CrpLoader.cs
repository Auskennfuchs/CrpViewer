using System.Collections.Generic;
using CrpViewer.Renderer;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CrpViewer {
   class CrpLoader {
        private TextureLoader texLoader;

        private Device dev;

        public CrpLoader(Device dev) { 
            texLoader = new TextureLoader(dev);
            this.dev = dev;
        }

        public Model LoadCrp(string filename) {
            var asset = CrpExtractor.CrpDeserializer.parseFile(filename);
            var model = new Model();
            model.Textures = LoadTextures(asset);
            model.Materials = LoadMaterials(asset, model.Textures);

            model.Mesh = new StaticMesh();
            var staticMesh = model.Mesh;

            foreach (var mesh in asset.Meshes) {
                var m = mesh.Value;
                var vertices = Buffer.Create<Vector3>(dev, BindFlags.VertexBuffer, m.vertices);
                var normals = Buffer.Create<Vector3>(dev, BindFlags.VertexBuffer, m.normals);
                var uv = Buffer.Create<Vector2>(dev, BindFlags.VertexBuffer, m.uv);
                var ind = m.triangles.ToArray();
                var indices = Buffer.Create<int>(dev, BindFlags.IndexBuffer, ind);
                staticMesh.SetIndexBuffer(indices, ind.Length);
                Vector3[] srcTangent, srcBiNormal;
                CalcTangentBiNormal(m.vertices, m.uv, ind, out srcTangent, out srcBiNormal);
                var tangents = Buffer.Create<Vector3>(dev, BindFlags.VertexBuffer, srcTangent);
                var binormals = Buffer.Create<Vector3>(dev, BindFlags.VertexBuffer, srcBiNormal);

                staticMesh.AddBuffer(vertices, "POSITION", SharpDX.DXGI.Format.R32G32B32_Float, Utilities.SizeOf<Vector3>());
                staticMesh.AddBuffer(normals, "NORMAL", SharpDX.DXGI.Format.R32G32B32_Float, Utilities.SizeOf<Vector3>());
                staticMesh.AddBuffer(tangents, "TANGENT", SharpDX.DXGI.Format.R32G32B32_Float, Utilities.SizeOf<Vector3>());
                staticMesh.AddBuffer(uv, "TEXCOORD", SharpDX.DXGI.Format.R32G32_Float, Utilities.SizeOf<Vector2>());
                staticMesh.AddBuffer(binormals, "BINORMAL", SharpDX.DXGI.Format.R32G32B32_Float, Utilities.SizeOf<Vector3>());
                break;
            }

            return model;
        }

        private void CalcTangentBiNormal(Vector3[] vertex, Vector2[] uv, int[] indices, out Vector3[] tangents, out Vector3[] biNormal) {
            tangents = new Vector3[vertex.Length];
            biNormal = new Vector3[vertex.Length];
            for (var i = 0; i < indices.Length / 3; i++) {
                var v1 = vertex[indices[i * 3 + 0]];
                var v2 = vertex[indices[i * 3 + 1]];
                var v3 = vertex[indices[i * 3 + 2]];

                var uv1 = uv[indices[i * 3 + 0]];
                var uv2 = uv[indices[i * 3 + 1]];
                var uv3 = uv[indices[i * 3 + 2]];

                Vector3 tangent, binormal;
                _CalcTangentBiNormal(v1, v2, v3, uv1, uv2, uv3, out tangent, out binormal);
                tangents[indices[i * 3 + 0]] = tangent;
                tangents[indices[i * 3 + 1]] = tangent;
                tangents[indices[i * 3 + 2]] = tangent;

                biNormal[indices[i * 3 + 0]] = binormal;
                biNormal[indices[i * 3 + 1]] = binormal;
                biNormal[indices[i * 3 + 2]] = binormal;
            }
        }

        private void _CalcTangentBiNormal(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
            Vector2 uv1, Vector2 uv2, Vector2 uv3,
            out Vector3 tangent, out Vector3 binormal) {
            var vector1 = vertex2 - vertex1;
            var vector2 = vertex3 - vertex1;

            var tuVector = uv2 - uv1;
            var tvVector = uv3 - uv1;

            // Calculate the denominator of the tangent/binormal equation.
            float den = 1.0f / (tuVector.X * tvVector.Y - tuVector.Y * tvVector.X);

            tangent = new Vector3((tvVector.Y * vector1.X - tvVector.X * vector2.X) * den,
                                    (tvVector.Y * vector1.Y - tvVector.X * vector2.Y) * den,
                                    (tvVector.Y * vector1.Z - tvVector.X * vector2.Z) * den);
            tangent.Normalize();

            binormal = new Vector3((tuVector.X * vector2.X - tuVector.Y * vector1.X) * den,
                                    (tuVector.X * vector2.Y - tuVector.Y * vector1.Y) * den,
                                    (tuVector.X * vector2.Z - tuVector.Y * vector1.Z) * den);
            binormal.Normalize();
        }

        private List<Texture> LoadTextures(CrpExtractor.Types.CrpAssetInfo asset) {
            var textures = new List<Texture>();
            foreach (var atex in asset.Textures) {
                var t = texLoader.loadFromByteArray(atex.Value);
                var tex = new Texture(atex.Key, t);
                textures.Add(tex);
            }

            return textures;
        }

        private List<Material> LoadMaterials(CrpExtractor.Types.CrpAssetInfo asset, List<Texture> textures) {
            var materials = new List<Material>();
            foreach (var mat in asset.Materials) {
                var m = new Material();
                foreach (var t in mat.Value.textures) {
                    switch (t.Key) {
                        case "_MainTex":
                            m.AddTexture(TEXTURE_TYPE.DIFFUSE, FindTexture(t.Value, textures));
                            break;
                        case "_XYSMap":
                        case "_XYCAMap":
                            m.AddTexture(TEXTURE_TYPE.NORMAL, FindTexture(t.Value, textures));
                            break;
                        case "_ACIMap":
                            break;
                    }
                }
                materials.Add(m);
            }
            return materials;
        }

        private Texture FindTexture(string checkSum, List<Texture> textures) {
            foreach (var t in textures) {
                if (t.Checksum.Equals(checkSum)) {
                    return t;
                }
            }
            return null;
        }
    }
}
