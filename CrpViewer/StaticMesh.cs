using System.Collections.Generic;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CrpViewer {
    class StaticMesh {
        public List<Buffer> Buffers {
            get; private set;
        }

        public Buffer IndexBuffer {
            get; private set;
        }

        public List<InputElement> Elements {
            get; private set;
        }

        public List<VertexBufferBinding> BufferBindings {
            get; private set;
        }

        public int NumIndices {
            get; set;
        }
        
        public StaticMesh() {
            Buffers = new List<Buffer>();
            Elements = new List<InputElement>();
            BufferBindings = new List<VertexBufferBinding>();
        }

        public void AddBuffer(Buffer buf, string inputName, Format format, int stride) {
            int index = 0;
            foreach(var e in Elements) {
                if(e.SemanticName.Equals(inputName)) {
                    index++;
                }
            }
            var el = new InputElement(inputName, index, format, Buffers.Count);
            Elements.Add(el);
            Buffers.Add(buf);

            var bufferBinding = new VertexBufferBinding(buf, stride, 0);
            BufferBindings.Add(bufferBinding);
        }

        public void SetIndexBuffer(Buffer indices, int numIndices) {
            IndexBuffer = indices;
            NumIndices = numIndices;
        }
    }
}
