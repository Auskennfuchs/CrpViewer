using System;
using System.Windows.Forms;
using CrpViewer.Renderer;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D11;
using SharpDX;
using CrpViewer.Events;
using SharpDX.DXGI;

namespace CrpViewer {
    public partial class MainWindow : Form {

        private Renderer.SwapChain swapChain;

        private Renderer.Renderer renderer;

        private Renderer.Shader.VertexShader vShader, vDeferredCleanup, vDeferredDraw, vDeferredCombine, vDirectionalLight;
        private Renderer.Shader.PixelShader pShader, pDeferredCleanup, pDeferredDraw, pDeferredCombine, pDirectionalLight;

        Model model;

        private TextureLoader texLoader;

        private CrpLoader loader;

        Camera cam;

        Timer timer = new Timer();

        Vector3 lightDir = new Vector3(0.0f, -1.0f, 1.0f);
        Quaternion lightRot = Quaternion.Identity;

        int fpsCount;
        float fpsTimeCount;

        RenderTargetGroup deferredRT;

        CrpViewer.Events.EventManager eventManager = new CrpViewer.Events.EventManager();

        private void MainWindow_SizeChanged(object sender, EventArgs e) {
        }

        public MainWindow() {
            InitializeComponent();

            AddEvents();

            renderer = new Renderer.Renderer();
            swapChain = new Renderer.SwapChain(this);
//            swapChain.RenderTarget.AddDepthStencil();
            swapChain.RenderTarget.Activate(renderer);

            loader = new CrpLoader(renderer.Device);

            texLoader = new TextureLoader(renderer.Device);

//            model = loader.LoadCrp(s("Coconut Tree Tall.crp");
//            model = loader.LoadCrp("DP's Harbor.crp");
            model = loader.LoadCrp("AKW mit Dampf.crp");
//            model = loader.LoadCrp("Arjan's Medieval Keep (Uni).crp");
//            model = loader.LoadCrp("jet.crp");

            vShader = new Renderer.Shader.VertexShader("../Shader/shaders.hlsl", "VSMain");
            pShader = new Renderer.Shader.PixelShader("../Shader/shaders.hlsl", "PSMain");
            vDeferredCleanup = new Renderer.Shader.VertexShader("../Shader/deferredCleanup.hlsl", "VSMain");
            pDeferredCleanup = new Renderer.Shader.PixelShader("../Shader/deferredCleanup.hlsl", "PSMain");
            vDeferredDraw = new Renderer.Shader.VertexShader("../Shader/deferredRender.hlsl", "VSMain");
            pDeferredDraw = new Renderer.Shader.PixelShader("../Shader/deferredRender.hlsl", "PSMain");
            vDeferredCombine = new Renderer.Shader.VertexShader("../Shader/deferredCombine.hlsl", "VSMain");
            pDeferredCombine = new Renderer.Shader.PixelShader("../Shader/deferredCombine.hlsl", "PSMain");
            vDirectionalLight = new Renderer.Shader.VertexShader("../Shader/directionalLight.hlsl", "VSMain");
            pDirectionalLight = new Renderer.Shader.PixelShader("../Shader/directionalLight.hlsl", "PSMain");

            cam = new Camera();
            cam.SetProjection(0.1f, 1000.0f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);

            swapChain.Resize += (o, e) => {
                cam.SetProjection(0.1f, 1000.0f, (float)this.ClientSize.Width / (float)this.ClientSize.Height, (float)Math.PI / 4.0f);
                renderer.Parameters.SetProjectionMatrix(cam.ProjectionMatrix);
            };

            var fm = new FreeMove(eventManager);
            fm.Speed = 10.0f;
            cam.AddComponent(new FreeLook(eventManager))
                .AddComponent(fm);
            cam.Transform.Position = new Vector3(0, 30, -100.0f);

            using (var inputLayout = new InputLayout(renderer.Device, vShader.InputSignature, model.Mesh.Elements.ToArray())) {
                renderer.DevContext.InputAssembler.InputLayout = inputLayout;

                var rasterizerStateDescription = RasterizerStateDescription.Default();
                rasterizerStateDescription.CullMode = CullMode.None;
                renderer.DevContext.Rasterizer.State = new RasterizerState(renderer.Device, rasterizerStateDescription);
                renderer.DevContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

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
                timer.Start();
            }

            deferredRT = new RenderTargetGroup(swapChain,Format.R8G8B8A8_UNorm);
            deferredRT.AddRenderTarget(SharpDX.DXGI.Format.R8G8B8A8_UNorm);
            deferredRT.AddRenderTarget(SharpDX.DXGI.Format.R16G16B16A16_Float);
            deferredRT.AddDepthStencil();
        }

        public void CleanUp() {
            vShader.Dispose();
            pShader.Dispose();
            vDeferredCleanup.Dispose();
            pDeferredCleanup.Dispose();
            vDeferredDraw.Dispose();
            pDeferredDraw.Dispose();
            deferredRT.Dispose();
            renderer.Dispose();
        }

        public void MainLoop() {
            fpsCount++;
            float elapsed = timer.Restart();
            fpsTimeCount += elapsed;
            if (fpsTimeCount > 1.0f) {
                var fps = fpsCount / fpsTimeCount;
                var renderTime = (fpsTimeCount / fpsCount * 1000.0f);
                this.Text = renderTime.ToString()+"ms ("+fps.ToString()+" fps)";
                fpsTimeCount = 0;
                fpsCount = 0;
            }
            cam.OnUpdate(elapsed);
            renderer.Parameters.SetParameter("viewPosition", cam.Transform.GetTransformedPosition());
            var worldMatrix = Matrix.Multiply(Matrix.RotationY(0.2f*elapsed),renderer.Parameters.GetMatrixParameter("worldMatrix"));
//            renderer.Parameters.SetWorldMatrix(worldMatrix);
            renderer.Parameters.SetViewMatrix(cam.ViewMatrix);

            lightRot = Quaternion.RotationYawPitchRoll(0.2f * elapsed, 0, 0);

            var light = Vector3.Transform(lightDir, lightRot);

            lightDir = light;
            renderer.Parameters.SetParameter("lightDir", light);
            renderer.DevContext.InputAssembler.SetVertexBuffers(0, model.Mesh.BufferBindings.ToArray());
            renderer.DevContext.InputAssembler.SetIndexBuffer(model.Mesh.IndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

            deferredRT.Activate(renderer,false);
            CleanDeferredTargets();
            deferredRT.Activate(renderer);
            renderer.SetVertexShader(vDeferredDraw);
            renderer.SetPixelShader(pDeferredDraw);
            renderer.DevContext.PixelShader.SetShaderResource(0, model.Materials[0].GetTexture(TEXTURE_TYPE.DIFFUSE).SRV);
            renderer.DevContext.PixelShader.SetShaderResource(1, model.Materials[0].GetTexture(TEXTURE_TYPE.NORMAL).SRV);
            renderer.ApplyResources(renderer.Parameters);
            for (var j = 0; j < 3; j++) {
                for (var i = 0; i < 3; i++) {
                    worldMatrix = Matrix.Translation(i * 130.0f, 0, j * 100.0f);
                    renderer.Parameters.SetWorldMatrix(worldMatrix);
//                    renderer.VertexShaderStage.DesiredState.Shader.State.Apply(renderer.DevContext, renderer.Parameters);
                    renderer.ApplyResources(renderer.Parameters);
                    renderer.DevContext.DrawIndexed(model.Mesh.NumIndices, 0, 0);
                }
            }

            //lightBuffer aufbauen
            swapChain.RenderTarget.Activate(renderer);
            renderer.SetVertexShader(vDirectionalLight);
            renderer.SetPixelShader(pDirectionalLight);
            renderer.ApplyResources(renderer.Parameters);
//            renderer.PixelShaderStage.DesiredState.Shader.State.Apply(renderer.DevContext, renderer.Parameters);
            renderer.DevContext.PixelShader.SetShaderResource(0, deferredRT.ShaderResourceViews[0]);
            renderer.DevContext.PixelShader.SetShaderResource(1, deferredRT.ShaderResourceViews[1]);
            renderer.DevContext.PixelShader.SetShaderResource(2, deferredRT.ShaderResourceViews[2]);
            renderer.DevContext.Draw(3, 0);

            swapChain.Present();
        }

        private void CleanDeferredTargets() {
            renderer.DevContext.ClearDepthStencilView(deferredRT.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            renderer.ClearResources();
            renderer.SetVertexShader(vDeferredCleanup);
            renderer.SetPixelShader(pDeferredCleanup);
            renderer.ApplyResources(renderer.Parameters);
            renderer.DevContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            renderer.DevContext.Draw(3, 0);
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
