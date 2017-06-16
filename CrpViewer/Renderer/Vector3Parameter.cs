using System;
using SharpDX;

namespace CrpViewer.Renderer {
    class Vector3Parameter : ConstantBufferParameter {


        public Vector3Parameter(int offset=0) : base(RenderParameterType.VECTOR3,sizeof(float)*3, offset){
        }

        public Vector3Parameter(Vector3 vec, int offset=0) : base(RenderParameterType.VECTOR3, sizeof(float)*3, offset, vec) {
        }
        
        protected override Array GetValArray() {
            return ((Vector3)Value).ToArray();
        }
    }
}
