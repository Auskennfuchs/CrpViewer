using SharpDX.Direct3D11;
using PixelShader = CrpViewer.Renderer.Shader.PixelShader;

namespace CrpViewer.Renderer.Stages {
    public class PixelShaderStage : ShaderStage {

        protected override void BindShader(DeviceContext dc, ParameterManager paramManager) {
            if (DesiredState.Shader.State != null) {
                dc.PixelShader.Set(((PixelShader)DesiredState.Shader.State).PixelShaderPtr);
            } else {
                dc.PixelShader.Set(null);
            }
        }

        protected override void BindConstantBuffers(DeviceContext dc, ParameterManager paramManager) {
            dc.PixelShader.SetConstantBuffers(DesiredState.ConstantBuffer.StartSlot, DesiredState.ConstantBuffer.Range, cBuffers);
        }
    }
}
