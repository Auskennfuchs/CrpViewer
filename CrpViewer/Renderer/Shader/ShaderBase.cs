using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using CBuffer = SharpDX.D3DCompiler.ConstantBuffer;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CrpViewer.Renderer.Shader {
    public abstract class ShaderBase : IDisposable {

        private static int ShaderIdCounter = 1;
        
        protected List<ConstantBuffer> constantBuffers = new List<ConstantBuffer>();

        public ShaderSignature InputSignature {
            get; protected set;
        }

        public int ID {
            get; private set;
        }

        public ShaderBase() {
            ID = ShaderIdCounter++;
        }

        public void Dispose() {
            if(InputSignature!=null) {
                InputSignature.Dispose();
            }
            if(constantBuffers!=null) {
                foreach(ConstantBuffer cb in constantBuffers) {
                    cb.Dispose();
                }
            }
        }

        public abstract void Apply(DeviceContext context, ParameterManager paramManager);

        public void SetParameterMatrix(string name,Matrix m) {
            foreach(ConstantBuffer cb in constantBuffers) {
                cb.SetParameterMatrix(name, m);
            }
        }

        protected void ReflectBytecode(ShaderBytecode bytecode) {
            using (var reflection = new ShaderReflection(bytecode)) {
                for (int cBufferIndex = 0; cBufferIndex < reflection.Description.ConstantBuffers; cBufferIndex++) {
                    CBuffer cb = reflection.GetConstantBuffer(cBufferIndex);
                    Buffer buf = new Buffer(Renderer.Instance.Device, cb.Description.Size, ResourceUsage.Dynamic, BindFlags.ConstantBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, sizeof(float));
                    ConstantBuffer constantBuffer = new ConstantBuffer(buf);
                    for (int i = 0; i < cb.Description.VariableCount; i++) {
                        var refVar = cb.GetVariable(i);
                        var type = refVar.GetVariableType();
                        switch (type.Description.Type) {
                            case ShaderVariableType.Float:
                                if (type.Description.RowCount == 4 && type.Description.ColumnCount == 4) {
                                    var matParam = new MatrixParameter();
                                    if (matParam.GetSize() != refVar.Description.Size) {
                                        throw CrpRendererException.Create("Error ConstantBufferParamtersize");
                                    }
                                    constantBuffer.AddParameter(refVar.Description.Name, refVar.Description.StartOffset, matParam);
                                }
                                if (type.Description.RowCount == 1 && type.Description.ColumnCount == 3) {
                                    var vec3Param = new Vector3Parameter();
                                    if (vec3Param.GetSize() != refVar.Description.Size) {
                                        throw CrpRendererException.Create("Error ConstantBufferParamtersize");
                                    }
                                    constantBuffer.AddParameter(refVar.Description.Name, refVar.Description.StartOffset, vec3Param);
                                }
                                break;
                        }
                    }
                    constantBuffers.Add(constantBuffer);
                }
            }
        }
    }
}
