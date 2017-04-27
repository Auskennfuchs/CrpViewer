using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace CrpViewer.Renderer {
    class RenderTarget {
        public DepthStencilView DepthStencilView {
            get; private set;
        }
        public DepthStencilState DepthStencilState {
            get; private set;
        }

        public RenderTargetView View {
            get; private set;
        }

        public Viewport Viewport {
            get; set;
        }

        private bool hasDepthStencil = false;

        public RenderTarget(Resource res) {
            ReCreate(res);
        }

        public RenderTarget(int width, int height, Format format) {
            var t = new Texture2D(Renderer.Instance.Device, new Texture2DDescription() {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Height = height,
                Width = width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(4, 0)                
            });
        }

        public void ReCreate(Resource res) {
            Dispose();
            View = new RenderTargetView(Renderer.Instance.Device, res);
            if (res.GetType() == typeof(Texture2D)) {
                SetViewport(0, 0, ((Texture2D)res).Description.Width, ((Texture2D)res).Description.Height);
            }
            if(hasDepthStencil) {
                AddDepthStencil();
            }
        }

        public void Dispose() {
            if (View != null) {
                View.Dispose();
            }
            if (DepthStencilView != null) {
                DepthStencilView.Dispose();
            }
            if (DepthStencilState != null) {
                DepthStencilState.Dispose();
            }
            Viewport = new Viewport();
        }

        public void Activate(Renderer pipeline) {
            pipeline.DevContext.Rasterizer.SetViewport(Viewport);
        }

        public void Resize(int width, int height, Resource src) {
            ReCreate(src);
        }

        public void SetViewport(int x, int y, int width, int height) {
            Viewport = new Viewport(x, y, width, height);
        }

        public void AddDepthStencil() {
            hasDepthStencil = true;
            using (var backBuffer = View.ResourceAs<Texture2D>()) {
                var depthBufferDescription = new Texture2DDescription {
                    Format = SharpDX.DXGI.Format.D32_Float_S8X24_UInt,
                    ArraySize = 1,
                    MipLevels = 1,
                    Width = backBuffer.Description.Width,
                    Height = backBuffer.Description.Height,
                    SampleDescription = backBuffer.Description.SampleDescription,
                    BindFlags = BindFlags.DepthStencil,
                };
                var depthStencilViewDescription = new DepthStencilViewDescription {
                    Dimension = backBuffer.Description.SampleDescription.Count > 1 || backBuffer.Description.SampleDescription.Quality > 0
                        ? DepthStencilViewDimension.Texture2DMultisampled
                        : DepthStencilViewDimension.Texture2D
                };
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
                using (var depthBuffer = new Texture2D(Renderer.Instance.Device, depthBufferDescription)) {
                    DepthStencilState = new DepthStencilState(Renderer.Instance.Device, depthStencilStateDescription);
                    DepthStencilView = new DepthStencilView(Renderer.Instance.Device, depthBuffer, depthStencilViewDescription);
                }
            }
        }
    }
}
