using System;
using System.Windows.Forms;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

using VShader = SharpDX.Direct3D11.VertexShader;

namespace CrpViewer.Renderer.Shader {
    public class VertexShader : ShaderBase {        
        public VShader VertexShaderPtr {
            get; private set;
        }

        public VertexShader(string file,string entryfunction):base() {
            try {
                ShaderFlags sFlags = ShaderFlags.PackMatrixRowMajor;
#if DEBUG
                sFlags |= ShaderFlags.Debug;
#endif
                using (var bytecode = ShaderBytecode.CompileFromFile(file, entryfunction, "vs_5_0", sFlags, EffectFlags.None)) {
                    if (bytecode.Message != null) {
                        MessageBox.Show(bytecode.Message);
                    }
                    InputSignature = ShaderSignature.GetInputSignature(bytecode);
                    VertexShaderPtr = new VShader(Renderer.Instance.Device, bytecode);
                    ReflectBytecode(bytecode);
                }
            }
            catch (Exception exc) {
                throw CrpRendererException.Create("Error loading VertexShader", exc);
            }
        }

        public new void Dispose() {
            base.Dispose();
            if(VertexShaderPtr!=null) {
                VertexShaderPtr.Dispose();
            }
        }

        public override void Apply(DeviceContext context, ParameterManager paramManager) {
            context.VertexShader.Set(VertexShaderPtr);
            for(int i=0;i<constantBuffers.Count;i++) {
                constantBuffers[i].UpdateBuffer(context, paramManager);
                context.VertexShader.SetConstantBuffer(i,constantBuffers[i].Buffer);
            }
        }
    }
}
