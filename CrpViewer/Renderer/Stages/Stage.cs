using SharpDX.Direct3D11;

namespace CrpViewer.Renderer.Stages {

    public interface IStageState {
        void ClearState();
        void Clone(IStageState src);
        void ResetTracking();
    }

    public abstract class Stage<T> where T : IStageState, new() {
        public T DesiredState {
            get; private set;
        }

        protected T currentState;

        public Stage() {
            DesiredState = new T();
            currentState = new T();
        }

        public void ApplyDesiredState(DeviceContext dc, ParameterManager paramManager) {
            OnApplyDesiredState(dc, paramManager);
            DesiredState.ResetTracking();
            currentState.Clone(DesiredState);
        }

        public abstract void OnApplyDesiredState(DeviceContext dc, ParameterManager paramManager);

        public void ClearDesiredState() {
            DesiredState.ClearState();
        }

        public void ClearCurrentState() {
            currentState.ClearState();
        }
    }
}
