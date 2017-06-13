using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace CrpViewer.Renderer.Stages {
    public class InputAssemblerStageState : IStageState {
        public TStateMonitor<PrimitiveTopology> PrimitiveTopology {
            get; private set;
        }

        public TStateMonitor<InputLayout> InputLayout {
            get; private set;
        }

        public InputAssemblerStageState() {
            PrimitiveTopology = new TStateMonitor<SharpDX.Direct3D.PrimitiveTopology>(SharpDX.Direct3D.PrimitiveTopology.Undefined);
            InputLayout = new TStateMonitor<SharpDX.Direct3D11.InputLayout>(null);
        }

        public void ClearState() {
            PrimitiveTopology.InitializeState();
            InputLayout.InitializeState();
        }

        public void Clone(IStageState src) {
            PrimitiveTopology.State = ((InputAssemblerStageState)src).PrimitiveTopology.State;
            InputLayout.State = ((InputAssemblerStageState)src).InputLayout.State;
        }

        public void ResetTracking() {
            PrimitiveTopology.ResetTracking();
            InputLayout.ResetTracking();
        }
    }
}
