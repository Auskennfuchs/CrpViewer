using SharpDX.Direct3D11;
using VertexShader = CrpViewer.Renderer.Shader.VertexShader;

namespace CrpViewer.Renderer.Stages {
    public class VertexShaderStage : ShaderStage {

        protected override void BindShader(DeviceContext dc, ParameterManager paramManager) {
            if (DesiredState.Shader.State != null) {
                dc.VertexShader.Set(((VertexShader)DesiredState.Shader.State).VertexShaderPtr);
            } else {
                dc.VertexShader.Set(null);
            }
        }

        protected override void BindConstantBuffers(DeviceContext dc, ParameterManager paramManager) {
            dc.VertexShader.SetConstantBuffers(DesiredState.ConstantBuffer.StartSlot, DesiredState.ConstantBuffer.Range, cBuffers);
        }
    }
}
