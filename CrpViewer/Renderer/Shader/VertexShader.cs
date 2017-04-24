﻿using System;
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
                using (var bytecode = ShaderBytecode.CompileFromFile(file, entryfunction, "vs_5_0", ShaderFlags.PackMatrixRowMajor, EffectFlags.None)) {
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