using System;
using System.Windows.Forms;
using CrpViewer.Renderer;
using SharpDX.Mathematics.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;
using SharpDX;
using System.Collections.Generic;
using CrpViewer.Events;

namespace CrpViewer {
    public partial class MainWindow : Form {

        private SwapChain swapChain;

        private Renderer.Renderer renderer;

        private Buffer vertices, indices, normals, uv, tangents, binormals;
        private int numIndices;

        private Renderer.Shader.VertexShader vShader;
        private Renderer.Shader.PixelShader pShader;

        private TextureLoader texLoader;

        Camera cam;

        Timer timer = new Timer();

        Vector3 lightDir = new Vector3(-1.0f, 0.0f, 0.0f);
        Quaternion lightRot = Quaternion.Identity;

        CrpViewer.Events.EventManager eventManager = new CrpViewer.Events.EventManager();

        private void MainWindow_SizeChanged(object sender, EventArgs e) {
            cam.SetProjection(0.1f, 200.0f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);
            renderer.Parameters.SetProjectionMatrix(cam.ProjectionMatrix);
        }

        public MainWindow() {
            InitializeComponent();

            AddEvents();

            renderer = new Renderer.Renderer();
            swapChain = new SwapChain(this);
            swapChain.RenderTarget.AddDepthStencil();
            swapChain.RenderTarget.Activate(renderer);

            texLoader = new TextureLoader(renderer.Device);

//            var asset = CrpExtractor.CrpDeserializer.parseFile("Coconut Tree Tall.crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("DP's Harbor.crp");
            var asset = CrpExtractor.CrpDeserializer.parseFile("AKW mit Dampf.crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("Arjan's Medieval Keep (Uni).crp");

            foreach (var mesh in asset.Meshes) {
                var m = mesh.Value;
                normals = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, m.normals);
                vertices = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, m.vertices);
                uv = Buffer.Create<Vector2>(renderer.Device, BindFlags.VertexBuffer, m.uv);
                var ind = m.triangles.ToArray();
                indices = Buffer.Create<int>(renderer.Device, BindFlags.IndexBuffer, ind);
                numIndices = ind.Length;
                Vector3[] srcTangent, srcBiNormal;
                CalcTangentBiNormal(m.vertices, m.uv, ind, out srcTangent, out srcBiNormal);
                tangents = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, srcTangent);
                binormals = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, srcBiNormal);
                break;
            }

            var textures = LoadTextures(asset);
            var materials = LoadMaterials(asset, textures);

            vShader = new Renderer.Shader.VertexShader("shaders.hlsl", "VSMain");
            pShader = new Renderer.Shader.PixelShader("shaders.hlsl", "PSMain");
            using (var inputLayout = new InputLayout(renderer.Device, vShader.InputSignature, new[] {
                new InputElement("POSITION",0,SharpDX.DXGI.Format.R32G32B32_Float,0),
                new InputElement("NORMAL",0,SharpDX.DXGI.Format.R32G32B32_Float,1),
                new InputElement("TANGENT",0,SharpDX.DXGI.Format.R32G32B32_Float,2),
                new InputElement("TEXCOORD",0,SharpDX.DXGI.Format.R32G32_Float,3),
                new InputElement("BINORMAL",0,SharpDX.DXGI.Format.R32G32B32_Float,4),
                }))
                { 
                var depthStencilStateDescription = new DepthStencilStateDescription {
                    IsDepthEnabled = true,
                    DepthComparison = Comparison.Less,
                    DepthWriteMask = DepthWriteMask.All,
                    IsStencilEnabled = false,
                    StencilReadMask = 0xff,
                    StencilWriteMask = 0xff,
                    FrontFace = new DepthStencilOperationDescription {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment
                    },
                    BackFace = new DepthStencilOperationDescription {
                        Comparison = Comparison.Always,
                        PassOperation = StencilOperation.Keep,
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement
                    }
                };

                using (var depthStencilState = new DepthStencilState(renderer.Device, depthStencilStateDescription)) {
                    var vertexBufferBinding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector3>(), 0);
                    var normalBufferBinding = new VertexBufferBinding(normals, Utilities.SizeOf<Vector3>(), 0);
                    var tangentBufferBinding = new VertexBufferBinding(tangents, Utilities.SizeOf<Vector3>(), 0);
                    var binormalBufferBinding = new VertexBufferBinding(binormals, Utilities.SizeOf<Vector3>(), 0);
                    var uvBufferBinding = new VertexBufferBinding(uv, Utilities.SizeOf<Vector2>(), 0);
                    renderer.DevContext.InputAssembler.SetVertexBuffers(0, new[] { vertexBufferBinding, normalBufferBinding, tangentBufferBinding, uvBufferBinding, binormalBufferBinding });
                    renderer.DevContext.InputAssembler.SetIndexBuffer(indices, SharpDX.DXGI.Format.R32_UInt, 0);
                    renderer.DevContext.InputAssembler.InputLayout = inputLayout;

                    renderer.DevContext.VertexShader.Set(vShader.VertexShaderPtr);
                    renderer.DevContext.PixelShader.Set(pShader.PixelShaderPtr);

                    var rasterizerStateDescription = RasterizerStateDescription.Default();
                    rasterizerStateDescription.CullMode = CullMode.None;
                    renderer.DevContext.Rasterizer.State = new RasterizerState(renderer.Device, rasterizerStateDescription);
                    renderer.DevContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                    cam = new Camera();
                    cam.SetProjection(0.1f, 200.0f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);
                    var fm = new FreeMove(eventManager);
                    fm.Speed = 5.0f;
                    cam.AddComponent(new FreeLook(eventManager))
                        .AddComponent(fm);
                    cam.Transform.Position = new Vector3(0, 30, -100.0f);
                    var worldMatrix = Matrix.Identity;
                    renderer.Parameters.SetWorldMatrix(worldMatrix);
                    renderer.Parameters.SetViewMatrix(cam.ViewMatrix);
                    renderer.Parameters.SetProjectionMatrix(cam.ProjectionMatrix);

                    renderer.DevContext.OutputMerger.DepthStencilState = depthStencilState;
                }

                var samplerStateDescription = new SamplerStateDescription {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = Filter.Anisotropic,
                    MaximumAnisotropy = 16,
                };
                var samplerState = new SamplerState(renderer.Device, samplerStateDescription);
                renderer.DevContext.PixelShader.SetSampler(0, samplerState);
                renderer.DevContext.PixelShader.SetSampler(1, new SamplerState(renderer.Device, new SamplerStateDescription {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter=Filter.MinMagMipLinear
                }));
                renderer.DevContext.PixelShader.SetShaderResource(0, materials[0].GetTexture(TEXTURE_TYPE.DIFFUSE).SRV);
                renderer.DevContext.PixelShader.SetShaderResource(1, materials[0].GetTexture(TEXTURE_TYPE.NORMAL).SRV);

/*                var bs = new BlendStateDescription();
                bs.RenderTarget[0].IsBlendEnabled = true;
                bs.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                bs.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                bs.RenderTarget[0].BlendOperation = BlendOperation.Add;
                bs.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
                bs.RenderTarget[0].DestinationAlphaBlend = BlendOption.One;
                bs.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                bs.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

                renderer.DevContext.OutputMerger.BlendState = new BlendState(renderer.Device, bs);*/
            }
            timer.Start();
        }

        public void MainLoop() {
            float elapsed = timer.Restart();
            cam.OnUpdate(elapsed);
            renderer.Parameters.SetParameter("viewPosition", cam.Transform.GetTransformedPosition());
            var worldMatrix = Matrix.Multiply(Matrix.RotationY(0.2f*elapsed),renderer.Parameters.GetMatrixParameter("worldMatrix"));
            renderer.Parameters.SetWorldMatrix(worldMatrix);
            renderer.Parameters.SetViewMatrix(cam.ViewMatrix);

            lightRot = lightRot*Quaternion.RotationYawPitchRoll(0.0f * elapsed, 0, 0);

            var light = Vector3.Transform(lightDir, lightRot);

//            lightDir = Vector3.Multiply(Quaternion.RotationYawPitchRoll(0.02f*elapsed,0,0);
            renderer.Parameters.SetParameter("lightDir", light);

            vShader.Apply(renderer.DevContext, renderer.Parameters);
            pShader.Apply(renderer.DevContext, renderer.Parameters);
            //rendering here
            renderer.DevContext.ClearRenderTargetView(swapChain.RenderTarget.View, new RawColor4(0.0f, .0f, 0.5f, 1.0f));
            renderer.DevContext.ClearDepthStencilView(swapChain.RenderTarget.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            renderer.DevContext.OutputMerger.SetRenderTargets(swapChain.RenderTarget.DepthStencilView, swapChain.RenderTarget.View);
            swapChain.RenderTarget.Activate(renderer);
            renderer.DevContext.DrawIndexed(numIndices, 0, 0);
            swapChain.Present();
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

        private List<Material> LoadMaterials(CrpExtractor.Types.CrpAssetInfo asset,List<Texture> textures) {
            var materials = new List<Material>();
            foreach(var mat in asset.Materials) {
                var m = new Material();
                foreach( var t in mat.Value.textures) {
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

        private Texture FindTexture(string checkSum,List<Texture> textures) {
            foreach(var t in textures) {
                if(t.Checksum.Equals(checkSum)) {
                    return t;
                }
            }
            return null;
        }

        private void AddEvents() {
            this.KeyDown += (o, e) => {
                eventManager.ProcessEvent(new EventKeyDown(new SKeyEvent() {
                    keyCode = e.KeyCode,
                    alt = e.Alt,
                    control = e.Control,
                    shift = e.Shift
                }));
            };
            this.KeyUp += (o, e) => {
                eventManager.ProcessEvent(new EventKeyUp(new SKeyEvent() {
                    keyCode = e.KeyCode,
                    alt = e.Alt,
                    control = e.Control,
                    shift = e.Shift
                }));
            };

            this.MouseDown += (o, e) => {
                eventManager.ProcessEvent(new EventMouseDown(new SMouseEvent() {
                    position = e.Location,
                    button = e.Button
                }));
            };
            this.MouseUp += (o, e) => {
                eventManager.ProcessEvent(new EventMouseUp(new SMouseEvent() {
                    position = e.Location,
                    button = e.Button
                }));
            };
            this.MouseMove += (o, e) => {
                eventManager.ProcessEvent(new EventMouseMove(new SMouseEvent() {
                    position = e.Location
                }));
            };
        }

        private void CalcTangentBiNormal(Vector3[] vertex, Vector2[] uv, int[] indices, out Vector3[] tangents, out Vector3[] biNormal) {
            tangents = new Vector3[vertex.Length];
            biNormal = new Vector3[vertex.Length];
            for(var i = 0; i < indices.Length/3; i++) {
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
            Vector2 uv1,Vector2 uv2, Vector2 uv3,
            out Vector3 tangent, out Vector3 binormal) {
            var vector1 = vertex2 - vertex1;
            var vector2 = vertex3 - vertex1;

            var tuVector = uv2 - uv1;
            var tvVector = uv3 - uv1;

            // Calculate the denominator of the tangent/binormal equation.
            float den = 1.0f / (tuVector.X*tvVector.Y-tuVector.Y*tvVector.X);

            tangent = new Vector3(  (tvVector.Y * vector1.X - tvVector.X * vector2.X) * den,
                                    (tvVector.Y * vector1.Y - tvVector.X * vector2.Y) * den,
                                    (tvVector.Y * vector1.Z - tvVector.X * vector2.Z) * den);
            tangent.Normalize();

            binormal = new Vector3( (tuVector.X * vector2.X - tuVector.Y * vector1.X) * den,
                                    (tuVector.X * vector2.Y - tuVector.Y * vector1.Y) * den,
                                    (tuVector.X * vector2.Z - tuVector.Y * vector1.Z) * den);
            binormal.Normalize();
        }

    }
}
