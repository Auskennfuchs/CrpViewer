using System;

namespace CrpViewer.Renderer {

    public abstract class ConstantBufferParameter : RenderParameter {

        public byte[] Bytebuffer {
            get; private set;
        }

        public int Size {
            get; private set;
        }

        public int Offset {
            get; private set;
        }

        public ConstantBufferParameter(RenderParameterType type, int size, int offset, object value=null) : base(type,value) {
            Size = size;
            Offset = offset;
            Bytebuffer = new byte[size];
        }

        public void UpdateBuffer() {
            System.Buffer.BlockCopy(GetValArray(), 0, Bytebuffer, 0, Size);
        }

        protected abstract Array GetValArray();
    }
}
