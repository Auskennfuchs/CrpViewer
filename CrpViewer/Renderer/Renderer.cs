using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

using InputAssemblerStage = CrpViewer.Renderer.Stages.InputAssemblerStage;
using VertexShaderStage = CrpViewer.Renderer.Stages.VertexShaderStage;
using PixelShaderStage = CrpViewer.Renderer.Stages.PixelShaderStage;
using VertexShader = CrpViewer.Renderer.Shader.VertexShader;
using PixelShader = CrpViewer.Renderer.Shader.PixelShader;
using SharpDX.Direct3D;

namespace CrpViewer.Renderer {
    public class Renderer : IDisposable{
        public Device Device {
            get; private set;
        }

        public static Renderer Instance {
            get; private set;
        }

        public DeviceContext DevContext {
            get; private set;
        }

        public ParameterManager Parameters {
            get; private set;
        }

        public VertexShaderStage VertexShaderStage {
            get; private set;
        }

        public PixelShaderStage PixelShaderStage {
            get; private set;
        }

        public InputAssemblerStage InputAssemblerStage {
            get; private set;
        }

        public Renderer() {
            Instance = this;
            DeviceCreationFlags dcf = DeviceCreationFlags.None;
#if DEBUG
            dcf |= DeviceCreationFlags.Debug;
#endif
            Device = new Device(SharpDX.Direct3D.DriverType.Hardware, dcf);

            DevContext = Device.ImmediateContext;

            Parameters = new ParameterManager();

            VertexShaderStage = new VertexShaderStage();
            PixelShaderStage = new PixelShaderStage();
            InputAssemblerStage = new InputAssemblerStage();
        }

        public void Dispose() {
            Instance = null;

            if (Device != null) {
                Device.Dispose();
            }
        }

        public void SetVertexShader(VertexShader shader) {
            VertexShaderStage.DesiredState.Shader.State = shader;
            if (shader != null) {
                for(var i = 0; i < shader.ConstantBuffers.Count; i++) {  
                    VertexShaderStage.DesiredState.ConstantBuffer.SetState(i, shader.ConstantBuffers[i]);
                }
            }
        }

        public void SetPixelShader(PixelShader shader) {
            PixelShaderStage.DesiredState.Shader.State = shader;
            if (shader != null) {
                for (var i = 0; i < shader.ConstantBuffers.Count; i++) {
                    PixelShaderStage.DesiredState.ConstantBuffer.SetState(i, shader.ConstantBuffers[i]);
                }
            }
        }

        public void ApplyResources(ParameterManager paramManager) {
            InputAssemblerStage.ApplyDesiredState(DevContext, paramManager);
            VertexShaderStage.ApplyDesiredState(DevContext, paramManager);
            PixelShaderStage.ApplyDesiredState(DevContext, paramManager);
        }

        public void ClearResources() {
//            InputAssemblerStage.ClearDesiredState();
            VertexShaderStage.ClearDesiredState();
            PixelShaderStage.ClearDesiredState();
        }

        public void Draw(PrimitiveTopology primType, InputLayout inputLayout, int vertexCount, int startVertex=0) {
            InputAssemblerStage.ClearDesiredState();
            InputAssemblerStage.DesiredState.PrimitiveTopology.State = primType;
            InputAssemblerStage.DesiredState.InputLayout.State = inputLayout;
            InputAssemblerStage.ApplyDesiredState(DevContext,Parameters);
            DevContext.Draw(vertexCount, startVertex);
        }

        public void DrawIndexed(PrimitiveTopology primType, InputLayout inputLayout, int indexCount, int startIndex=0, int startVertex=0) {
            InputAssemblerStage.ClearDesiredState();
            InputAssemblerStage.DesiredState.PrimitiveTopology.State = primType;
            InputAssemblerStage.DesiredState.InputLayout.State = inputLayout;
            InputAssemblerStage.ApplyDesiredState(DevContext, Parameters);
            DevContext.DrawIndexed(indexCount,startIndex, startVertex);
        }
    }
}
