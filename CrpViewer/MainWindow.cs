using System;
using System.Windows.Forms;
using CrpViewer.Renderer;
using SharpDX.Mathematics.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;
using SharpDX;

namespace CrpViewer {
    public partial class MainWindow : Form {

        private SwapChain swapChain;

        private Renderer.Renderer renderer;

        private Buffer vertices, indices, normals;
        private int numIndices;

        private Renderer.Shader.VertexShader vShader;
        private Renderer.Shader.PixelShader pShader;

        Timer timer = new Timer();

        private void MainWindow_SizeChanged(object sender, EventArgs e) {
            var mat = Matrix.PerspectiveFovLH((float)Math.PI / 4f, this.ClientSize.Width / (float)this.ClientSize.Height, .5f, 1000f);
            renderer.Parameters.SetProjectionMatrix(mat);
        }

        public MainWindow() {
            InitializeComponent();

            renderer = new Renderer.Renderer();
            swapChain = new SwapChain(this);
            swapChain.RenderTarget.AddDepthStencil();
            swapChain.RenderTarget.Activate(renderer);

            var asset = CrpExtractor.CrpDeserializer.parseFile("jet.crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("AKW mit Dampf.crp");

            foreach (var mesh in asset.Meshes) {
                var m = mesh.Value;
                normals = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, m.normals);
                vertices = Buffer.Create<Vector3>(renderer.Device, BindFlags.VertexBuffer, m.vertices);
                var ind = m.triangles.ToArray();
                indices = Buffer.Create<int>(renderer.Device, BindFlags.IndexBuffer, ind);
                numIndices = ind.Length;
                break;
            }
            vShader = new Renderer.Shader.VertexShader("shaders.hlsl", "VSMain");
            pShader = new Renderer.Shader.PixelShader("shaders.hlsl", "PSMain");
            using (var inputLayout = new InputLayout(renderer.Device, vShader.InputSignature, new[] {
                new InputElement("POSITION",0,SharpDX.DXGI.Format.R32G32B32_Float,0),
                new InputElement("NORMAL",0,SharpDX.DXGI.Format.R32G32B32_Float,1),
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
                    renderer.DevContext.InputAssembler.SetVertexBuffers(0, new[] { vertexBufferBinding, normalBufferBinding });
                    renderer.DevContext.InputAssembler.SetIndexBuffer(indices, SharpDX.DXGI.Format.R32_UInt, 0);
                    renderer.DevContext.InputAssembler.InputLayout = inputLayout;

                    renderer.DevContext.VertexShader.Set(vShader.VertexShaderPtr);
                    renderer.DevContext.PixelShader.Set(pShader.PixelShaderPtr);

                    var rasterizerStateDescription = RasterizerStateDescription.Default();
                    rasterizerStateDescription.CullMode = CullMode.None;
                    //                renderer.DevContext.Rasterizer.State = new RasterizerState(renderer.Device, rasterizerStateDescription);
                    renderer.DevContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                    var cameraPosition = new Vector3(0, 80, 150.0f);
                    var cameraTarget = Vector3.Zero;
                    var cameraUp = Vector3.UnitY;
                    var worldMatrix = Matrix.Identity;
                    var viewMatrix = Matrix.LookAtLH(cameraPosition, cameraTarget, cameraUp); // reorient everything to camera space
                    var projectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4f, this.ClientSize.Width / (float)this.ClientSize.Height, .5f, 1000f); // create a generic perspective projection matrix
                    renderer.Parameters.SetWorldMatrix(worldMatrix);
                    renderer.Parameters.SetViewMatrix(viewMatrix);
                    renderer.Parameters.SetProjectionMatrix(projectionMatrix);

                    renderer.DevContext.OutputMerger.SetRenderTargets(swapChain.RenderTarget.DepthStencilView, swapChain.RenderTarget.View);
                    renderer.DevContext.OutputMerger.DepthStencilState = depthStencilState;
                }
            }
            timer.Start();
        }

        public void MainLoop() {
            float elapsed = timer.Restart();
            var worldMatrix = Matrix.Multiply(Matrix.RotationY(0.01f*elapsed),renderer.Parameters.GetMatrixParameter("worldMatrix"));
            renderer.Parameters.SetWorldMatrix(worldMatrix);
            vShader.Apply(renderer.DevContext, renderer.Parameters);
            //rendering here
            renderer.DevContext.ClearRenderTargetView(swapChain.RenderTarget.View, new RawColor4(0.0f, .0f, 0.5f, 1.0f));
            renderer.DevContext.ClearDepthStencilView(swapChain.RenderTarget.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            renderer.DevContext.OutputMerger.SetRenderTargets(swapChain.RenderTarget.DepthStencilView, swapChain.RenderTarget.View);
            swapChain.RenderTarget.Activate(renderer);
            renderer.DevContext.DrawIndexed(numIndices, 0, 0);
            swapChain.Present();
        }

    }
}
