using System;
using SharpDX;

namespace CrpViewer.Renderer {
    class MatrixParameter : IConstantBufferParameter {

        private Matrix mat;

        public Matrix Value {
            get {
                return mat;
            }
            set {
                mat = value;
                UpdateBuffer();
            }
        }

        private static int SIZE = sizeof(float) * 4 * 4;

        private byte[] buffer = new byte[SIZE];

        public MatrixParameter() {
        }

        public MatrixParameter(Matrix mat) {
            this.mat = mat;
        }

        public byte[] GetBytes() {
            return buffer;
        }

        public int GetSize() {
            return SIZE;
        }

        public void SetValue(object obj) {
            Value = (Matrix)obj;
        }

        private void UpdateBuffer() {
            Buffer.BlockCopy(mat.ToArray(), 0, buffer, 0, SIZE);
        }

        public object GetValue() {
            return Value;
        }

        ConstantBufferParameterType IConstantBufferParameter.GetType() {
            return ConstantBufferParameterType.MATRIX;
        }
    }
}
