﻿using System;
using SharpDX;

namespace CrpViewer.Renderer {
    class MatrixParameter : BaseConstantBufferParameter<Matrix> {

        public MatrixParameter() : base(sizeof(float)*4*4){
        }

        public MatrixParameter(Matrix mat) : base(sizeof(float)*4*4, mat) {
        }
        
        public override ConstantBufferParameterType GetParamType() {
            return ConstantBufferParameterType.MATRIX;
        }

        protected override Array GetValArray() {
            return val.ToArray();
        }
    }
}
