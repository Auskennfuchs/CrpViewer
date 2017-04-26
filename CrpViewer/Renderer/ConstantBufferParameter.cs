namespace CrpViewer.Renderer {
    public enum ConstantBufferParameterType {
        MATRIX,
        VECTOR3,
        VECTOR4,
        NUM_ELEM
    }

    public interface IConstantBufferParameter {
        int GetSize();
        object GetValue();
        void SetValue(object obj);
        byte[] GetBytes();
        ConstantBufferParameterType GetParamType();
    }

    public abstract class BaseConstantBufferParameter<T> : IConstantBufferParameter where T: struct{

        protected int size;

        protected T val;

        public T Value {
            get {
                return val;
            }
            set {
                val = value;
                UpdateBuffer();
            }
        }

        private byte[] buffer;


        public BaseConstantBufferParameter(int size) {
            this.size = size;
            buffer = new byte[size];
        }

        public BaseConstantBufferParameter(int size, T val) {
            this.size = size;        
            buffer = new byte[size];
            this.val = val;
        }    

        public int GetSize() {
            return size;
        }
        public byte[] GetBytes() {
            return buffer;
        }
        public void SetValue(object obj) {
            Value = (T)obj;
        }
        public object GetValue() {
            return val;
        }
        public abstract ConstantBufferParameterType GetParamType();

        private void UpdateBuffer() {
            System.Buffer.BlockCopy(GetValArray(), 0, buffer, 0, size);
        }

        protected abstract System.Array GetValArray();
    }

    public class ConstantBufferParameter {
        public int Size {
            get {
                return param.GetSize();
            }
        }

        public object Value {
            get {
                return param.GetValue();
            }
            set {
                param.SetValue(value);
            }
        }

        private IConstantBufferParameter param;
        public IConstantBufferParameter Param {
            get { return param; }
        }

        public string Name {
            get;
        }

        public int Offset {
            get;
        }

        public ConstantBufferParameter(string name, int offset, IConstantBufferParameter param) {
            Name = name;
            Offset = offset;
            this.param = param;
        }
    }
}
