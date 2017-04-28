using System;
using System.Windows.Forms;
using CrpViewer.Renderer;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D11;
using SharpDX;
using CrpViewer.Events;

namespace CrpViewer {
    public partial class MainWindow : Form {

        private SwapChain swapChain;

        private Renderer.Renderer renderer;

        private Renderer.Shader.VertexShader vShader;
        private Renderer.Shader.PixelShader pShader;

        Model model;

        private TextureLoader texLoader;

        private CrpLoader loader;

        Camera cam;

        Timer timer = new Timer();

        Vector3 lightDir = new Vector3(0.0f, 0.0f, 1.0f);
        Quaternion lightRot = Quaternion.Identity;

        int fpsCount;
        float fpsTimeCount;

        CrpViewer.Events.EventManager eventManager = new CrpViewer.Events.EventManager();

        private void MainWindow_SizeChanged(object sender, EventArgs e) {
            cam.SetProjection(1000.0f, 0.1f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);
            renderer.Parameters.SetProjectionMatrix(cam.ProjectionMatrix);
        }

        public MainWindow() {
            InitializeComponent();

            AddEvents();

            renderer = new Renderer.Renderer();
            swapChain = new SwapChain(this);
            swapChain.RenderTarget.AddDepthStencil();
            swapChain.RenderTarget.Activate(renderer);

            loader = new CrpLoader(renderer.Device);

            texLoader = new TextureLoader(renderer.Device);

//            var asset = CrpExtractor.CrpDeserializer.parseFile("Coconut Tree Tall.crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("DP's Harbor.crp");
            model = loader.LoadCrp("AKW mit Dampf.crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("Arjan's Medieval Keep (Uni).crp");
//            var asset = CrpExtractor.CrpDeserializer.parseFile("jet.crp");

            vShader = new Renderer.Shader.VertexShader("../Shader/shaders.hlsl", "VSMain");
            pShader = new Renderer.Shader.PixelShader("../Shader/shaders.hlsl", "PSMain");
            using (var inputLayout = new InputLayout(renderer.Device, vShader.InputSignature, model.Mesh.Elements.ToArray())) {
                renderer.DevContext.InputAssembler.SetVertexBuffers(0, model.Mesh.BufferBindings.ToArray());
                renderer.DevContext.InputAssembler.SetIndexBuffer(model.Mesh.IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
                renderer.DevContext.InputAssembler.InputLayout = inputLayout;

                renderer.DevContext.VertexShader.Set(vShader.VertexShaderPtr);
                renderer.DevContext.PixelShader.Set(pShader.PixelShaderPtr);

                var rasterizerStateDescription = RasterizerStateDescription.Default();
                rasterizerStateDescription.CullMode = CullMode.None;
                renderer.DevContext.Rasterizer.State = new RasterizerState(renderer.Device, rasterizerStateDescription);
                renderer.DevContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                cam = new Camera();
                cam.SetProjection(1000.0f, 0.1f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);
                var fm = new FreeMove(eventManager);
                fm.Speed = 5.0f;
                cam.AddComponent(new FreeLook(eventManager))
                    .AddComponent(fm);
                cam.Transform.Position = new Vector3(0, 30, -100.0f);
                var worldMatrix = Matrix.Identity;
                renderer.Parameters.SetWorldMatrix(worldMatrix);
                renderer.Parameters.SetViewMatrix(cam.ViewMatrix);
                renderer.Parameters.SetProjectionMatrix(cam.ProjectionMatrix);

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
                    Filter = Filter.MinMagMipLinear
                }));
                renderer.DevContext.PixelShader.SetShaderResource(0, model.Materials[0].GetTexture(TEXTURE_TYPE.DIFFUSE).SRV);
                renderer.DevContext.PixelShader.SetShaderResource(1, model.Materials[0].GetTexture(TEXTURE_TYPE.NORMAL).SRV);
                timer.Start();
            }
        }

        public void MainLoop() {
            fpsCount++;
            float elapsed = timer.Restart();
            fpsTimeCount += elapsed;
            if (fpsTimeCount > 1.0f) {
                this.Text = (fpsTimeCount/fpsCount*1000.0f).ToString();
                fpsTimeCount = 0;
                fpsCount = 0;
            }
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
            renderer.DevContext.ClearRenderTargetView(swapChain.RenderTarget.View, new RawColor4(0.7f, .7f, 0.7f, 1.0f));
            renderer.DevContext.ClearDepthStencilView(swapChain.RenderTarget.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 0.0f, 0);
            renderer.DevContext.OutputMerger.SetRenderTargets(swapChain.RenderTarget.DepthStencilView, swapChain.RenderTarget.View);
            swapChain.RenderTarget.Activate(renderer);
            renderer.DevContext.DrawIndexed(model.Mesh.NumIndices, 0, 0);
            swapChain.Present();
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
    }
}
