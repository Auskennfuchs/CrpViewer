using System;
using SharpDX;

namespace CrpViewer.Renderer {
    class Vector3Parameter : BaseConstantBufferParameter<Vector3> {


        public Vector3Parameter() : base(sizeof(float)*3){
        }

        public Vector3Parameter(Vector3 vec) : base(sizeof(float)*3, vec) {
        }
        
        public override ConstantBufferParameterType GetParamType() {
            return ConstantBufferParameterType.VECTOR3;
        }

        protected override Array GetValArray() {
            return val.ToArray();
        }
    }
}
