using SharpDX.Direct3D11;

namespace CrpViewer.Renderer.Stages {

    public abstract class ShaderStage : Stage<ShaderStageState> {

        protected SharpDX.Direct3D11.Buffer[] cBuffers = new SharpDX.Direct3D11.Buffer[ShaderStageState.NUM_CONSTANTBUFFERS];
        protected SharpDX.Direct3D11.ShaderResourceView[] SRVs = new ShaderResourceView[ShaderStageState.NUM_SHADERRESOURCES];

        public ShaderStage() : base() {
        }

        public override void OnApplyDesiredState(DeviceContext dc, ParameterManager paramManager) {
            if (DesiredState.Shader.NeedUpdate) {
                BindShader(dc, paramManager);
            }
            UpdateConstantBufferParameter(dc, paramManager);
            if (DesiredState.ConstantBuffer.NeedUpdate) {
                UpdateConstantBufferArray();
                BindConstantBuffers(dc, paramManager);
            }
            if(DesiredState.Resources.NeedUpdate) {
                BindShaderResources(dc);
            }
        }

        protected abstract void BindShader(DeviceContext dc, ParameterManager paramManager);
        protected abstract void BindConstantBuffers(DeviceContext dc, ParameterManager paramManager);
        protected abstract void BindShaderResources(DeviceContext dc);

        public void UpdateConstantBufferParameter(DeviceContext dc, ParameterManager paramManager) {
            for (var i = 0; i < DesiredState.ConstantBuffer.NumSlots; i++) {
                if (DesiredState.ConstantBuffer.States[i] != null) {
                    DesiredState.ConstantBuffer.States[i].UpdateBuffer(dc, paramManager);
                }
            }
        }

        private void UpdateConstantBufferArray() {
            for (var i = 0; i < DesiredState.ConstantBuffer.Range; i++) {
                var slotPos = DesiredState.ConstantBuffer.StartSlot + i;
                if (DesiredState.ConstantBuffer.States[slotPos] != null) {
                    cBuffers[i] = DesiredState.ConstantBuffer.States[slotPos].Buffer;
                } else {
                    cBuffers[i] = null;
                }
            }
        }
    }
}
