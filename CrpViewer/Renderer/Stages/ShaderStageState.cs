using CrpViewer.Renderer.Shader;

namespace CrpViewer.Renderer.Stages {
    public class ShaderStageState : IStageState {
        private static int NUM_CONSTANTBUFFERS = 128;

        public TStateMonitor<ShaderBase> Shader {
            get; set;
        }

        public TStateArrayMonitor<ConstantBuffer> ConstantBuffer {
            get; private set;
        }

        private ShaderStageState sisterState;
        public ShaderStageState SisterState {
            get { return sisterState; }
            set {
                sisterState = value;
                Shader.Sister = sisterState.Shader;
            }
        }

        public ShaderStageState() {
            Shader = new TStateMonitor<ShaderBase>(null);
            ConstantBuffer = new TStateArrayMonitor<CrpViewer.Renderer.ConstantBuffer>(NUM_CONSTANTBUFFERS, null);
        }

        public void ResetTracking() {
            Shader.ResetTracking();
            ConstantBuffer.ResetTracking();
        }

        public void ClearState() {
            Shader.InitializeState();
            ConstantBuffer.InitializeState();
        }

        public void Clone(IStageState src) {
            Shader.State = ((ShaderStageState)src).Shader.State;
            for(var i=0; i < NUM_CONSTANTBUFFERS; i++) {
                ConstantBuffer.States[i] = ((ShaderStageState)src).ConstantBuffer.States[i];
            }
        }
    }
}
