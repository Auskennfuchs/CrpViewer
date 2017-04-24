namespace CrpViewer.Renderer {
    public enum ConstantBufferParameterType {
        MATRIX,
        VECTOR3,
        VECTOR4,
        NUM_ELEM
    }

    public interface IConstantBufferParameter {
        int GetSize();
        byte[] GetBytes();
        void SetValue(object obj);
        object GetValue();
        ConstantBufferParameterType GetType();
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
