using System;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using PShader = SharpDX.Direct3D11.PixelShader;

namespace CrpViewer.Renderer.Shader {
    public class PixelShader : ShaderBase {
        private PShader pixelShader;
        public PShader PixelShaderPtr {
            get { return pixelShader; }
        }

        public PixelShader(string file, string entryfunction):base() {
            try {
                ShaderFlags sFlags = ShaderFlags.PackMatrixRowMajor;
#if DEBUG
                sFlags |= ShaderFlags.Debug;
#endif
                using (var bytecode = ShaderBytecode.CompileFromFile(file, entryfunction, "ps_5_0", sFlags, EffectFlags.None)) {
                    InputSignature = ShaderSignature.GetInputSignature(bytecode);
                    pixelShader = new PShader(Renderer.Instance.Device, bytecode);
                    ReflectBytecode(bytecode);
                }
            }
            catch (Exception exc) {
                throw CrpRendererException.Create("Error loading PixelShader", exc);
            }
        }

        public new void Dispose() {
            base.Dispose();
            if(pixelShader!=null) {
                pixelShader.Dispose();
            }
        }

        public override void Apply(DeviceContext context, ParameterManager paramManager) {
            context.PixelShader.Set(pixelShader);
            for (int i = 0; i < constantBuffers.Count; i++) {
                constantBuffers[i].UpdateBuffer(context, paramManager);
                context.PixelShader.SetConstantBuffer(i, constantBuffers[i].Buffer);
            }
        }
    }
}
