﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

using VertexShaderStage = CrpViewer.Renderer.Stages.VertexShaderStage;
using PixelShaderStage = CrpViewer.Renderer.Stages.PixelShaderStage;
using VertexShader = CrpViewer.Renderer.Shader.VertexShader;
using PixelShader = CrpViewer.Renderer.Shader.PixelShader;

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
            VertexShaderStage.ApplyDesiredState(DevContext, paramManager);
            PixelShaderStage.ApplyDesiredState(DevContext, paramManager);
//            InputAssemblerStage.ApplyDesiredState(DevContext, paramManager);
        }

        public void ClearResources() {
            VertexShaderStage.ClearDesiredState();
            PixelShaderStage.ClearDesiredState();
//            InputAssemblerStage.ClearDesiredState();
        }
    }
}
