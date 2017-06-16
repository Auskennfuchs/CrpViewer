using System;
using SharpDX;

namespace CrpViewer.Renderer {
    class MatrixParameter : ConstantBufferParameter {

        public MatrixParameter(int offset=0) : base(RenderParameterType.MATRIX, sizeof(float)*4*4, offset) {
        }

        public MatrixParameter(Matrix mat, int offset=0) : base(RenderParameterType.MATRIX, sizeof(float)*4*4, offset, mat) {
        }
        
        protected override Array GetValArray() {
            return ((Matrix)Value).ToArray();
        }
    }
}
