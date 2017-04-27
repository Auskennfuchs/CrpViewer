using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using DXSwapChain = SharpDX.DXGI.SwapChain;
using Resource = SharpDX.Direct3D11.Resource;

namespace CrpViewer.Renderer {
    class SwapChain {
        private DXSwapChain swapChain;

        private RenderTarget renderTarget;
        public RenderTarget RenderTarget {
            get { return renderTarget; }
        }

        public Viewport Viewport {
            get { return renderTarget.Viewport; }
            set { renderTarget.Viewport = value; }
        }

        public DXSwapChain DXSwapChain {
            get { return swapChain; }
        }

        private int formWidth, formHeight, fullScreenWidth, fullScreenHeight;

        private bool isResizing = false;

//        public event EventHandler<SResizeEvent> Resize;

        public SwapChain(Form form)
            : this(form, 0, 0) { }

        public SwapChain(Form form, int fullScreenWidth, int fullScreenHeight) {
            if (fullScreenWidth == 0 || fullScreenHeight == 0) {
                fullScreenWidth = SystemInformation.VirtualScreen.Width;
                fullScreenHeight = SystemInformation.VirtualScreen.Height;
            }
            this.fullScreenWidth = fullScreenWidth;
            this.fullScreenHeight = fullScreenHeight;

            formWidth = form.ClientSize.Width;
            formHeight = form.ClientSize.Height;

            var swapChainDescriptor = new SwapChainDescription() {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                Flags = SwapChainFlags.AllowModeSwitch,
                IsWindowed = true,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(0, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(4, 0),
                SwapEffect = SwapEffect.Discard
            };

            var factory = new Factory1();
            swapChain = new DXSwapChain(factory, Renderer.Instance.Device, swapChainDescriptor);

            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0)) {
                renderTarget = new RenderTarget(resource);
            }

            using (var fac = swapChain.GetParent<Factory>()) {
                fac.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
            }

            form.ResizeBegin += (o, e) => {
                formHeight = ((Form)o).Height;
                formWidth = ((Form)o).Width;
            };
            form.ResizeBegin += (o, e) => {
                isResizing = true;
            };
            form.ResizeEnd += (o, e) => {
                isResizing = false;
                HandleResize(o, e);
            };
            form.KeyDown += HandleKeyDown;

            form.SizeChanged += HandleResize;
        }

        public void Dispose() {
            if (renderTarget != null) {
                renderTarget.Dispose();
            }
            if (swapChain != null) {
                swapChain.Dispose();
            }
        }

        public void Present() {
            swapChain.Present(0, PresentFlags.None);
        }

        private void HandleResize(object sender, System.EventArgs e) {
            Form f = (Form)sender;
            if ((f.ClientSize.Width != formWidth || f.ClientSize.Height != formHeight)
                && !isResizing
                && !(f.WindowState == FormWindowState.Minimized)) {
                formWidth = f.ClientSize.Width;
                formHeight = f.ClientSize.Height;

                DoResize(formWidth, formHeight);
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e) {
            if (e.Alt && e.KeyCode == Keys.Enter) {
                if (!swapChain.IsFullScreen) {
                    DoResize(fullScreenWidth, fullScreenHeight);
                } else {
                    DoResize(formWidth, formHeight);
                }
                swapChain.IsFullScreen = !swapChain.IsFullScreen;
            }
        }

        private void DoResize(int width, int height) {
            renderTarget.Dispose();
            swapChain.ResizeBuffers(1, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
            using (var resource = Resource.FromSwapChain<Texture2D>(swapChain, 0)) {
                renderTarget.Resize(width, height, resource);
            }

/*            Resize(this, new SResizeEvent {
                Size = new System.Drawing.Size(width, height)
            });

            ProcessEvent(new EventResize(new SResizeEvent {
                Size = new System.Drawing.Size(width, height)
            }));*/
        }
    }
}
