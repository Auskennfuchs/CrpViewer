using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CrpViewer.Renderer {
    class RenderTargetGroup : IDisposable {

        public List<RenderTargetView> RenderTargets {
            get; private set;
        }

        public List<ShaderResourceView> ShaderResourceViews {
            get; private set;
        }

        public DepthStencilView DepthStencilView {
            get; private set;
        }

        private DepthStencilState depthStencilState;

        public int Width {
            get; private set;
        }

        public int Height {
            get; private set;
        }

        public Viewport Viewport {
            get;set;
        }

        public RenderTargetGroup(int width, int height, Format firstFormat) {
            RenderTargets = new List<RenderTargetView>();
            ShaderResourceViews = new List<ShaderResourceView>();
            Width = width;
            Height = height;
            AddRenderTarget(firstFormat);

            Viewport = new Viewport(0, 0, width, height);
        }

        public RenderTargetGroup(SwapChain swapChain, Format firstFormat) : this(swapChain.RenderTarget.Viewport.Width,swapChain.RenderTarget.Viewport.Height,firstFormat) {
            swapChain.Resize += (o, e) => {
                Resize(e.Size.Width, e.Size.Height);
            };
        }

        public void AddRenderTarget(Format format) {
            using (var t = CreateRenderTargetTexture(format)) {
                var rtv = new RenderTargetView(Renderer.Instance.Device, t);
                RenderTargets.Add(rtv);
                var srv = new ShaderResourceView(Renderer.Instance.Device, t);
                ShaderResourceViews.Add(srv);
            }
        }

        private Texture2D CreateRenderTargetTexture(Format format) {
            return new Texture2D(Renderer.Instance.Device, new Texture2DDescription() {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Height = Height,
                Width = Width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0) // no MSAA
            });
        }

        public void Dispose() {
            foreach(var rt in RenderTargets) {
                rt.Dispose();
            }
            foreach (var srv in ShaderResourceViews) {
                srv.Dispose();
            }
        }

        public void Activate(Renderer pipeline, bool activateDepthStencil=true) {
            pipeline.DevContext.Rasterizer.SetViewport(Viewport);
            pipeline.DevContext.OutputMerger.SetRenderTargets(activateDepthStencil?DepthStencilView: null, RenderTargets.ToArray());
            if(depthStencilState != null && activateDepthStencil) {
                pipeline.DevContext.OutputMerger.DepthStencilState = depthStencilState;
            } else {
                pipeline.DevContext.OutputMerger.DepthStencilState = null;
            }
        }

        public void Deactivate(Renderer pipeline) {
            var rts = new RenderTargetView[RenderTargets.Count];
            pipeline.DevContext.OutputMerger.SetRenderTargets(null, rts);

        }

        public void AddDepthStencil() {
            var depthBufferDescription = new Texture2DDescription {
                Format = SharpDX.DXGI.Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Width,
                Height = Height,
                SampleDescription = new SampleDescription(1,0),
                BindFlags = BindFlags.DepthStencil,
            };
            var depthStencilViewDescription = new DepthStencilViewDescription {
                Dimension = DepthStencilViewDimension.Texture2D
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
                depthStencilState = new DepthStencilState(Renderer.Instance.Device, depthStencilStateDescription);
                DepthStencilView = new DepthStencilView(Renderer.Instance.Device, depthBuffer, depthStencilViewDescription);
            }
        }

        private void Resize(int newWidth, int newHeight) {
            Width = newWidth;
            Height = newHeight;
            Viewport = new Viewport(0, 0, Width, Height);
            foreach (var srv in ShaderResourceViews) {
                srv.Dispose();
            }
            ShaderResourceViews.Clear();
            var newRenderTargets = new List<RenderTargetView>();
            foreach(var rtv in RenderTargets) {
                Format format = rtv.Description.Format;
                rtv.Dispose();
                using (var t = CreateRenderTargetTexture(format)) {
                    var rt = new RenderTargetView(Renderer.Instance.Device, t);
                    newRenderTargets.Add(rt);
                    var srv = new ShaderResourceView(Renderer.Instance.Device, t);
                    ShaderResourceViews.Add(srv);
                }
            }
            RenderTargets.Clear();
            RenderTargets.AddRange(newRenderTargets);

            if(DepthStencilView != null) {
                DepthStencilView.Dispose();
                AddDepthStencil();
            }
        }
    }
}
