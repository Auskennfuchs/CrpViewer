using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrpViewer.Renderer {

    public enum RenderParameterType {
        MATRIX,
        VECTOR3,
        VECTOR4,
        SRV,
        NUM_ELEM
    }

    public abstract class RenderParameter {

        public object Value {
            get; set;
        }

        public RenderParameterType Type {
            get;
        }

        protected RenderParameter(RenderParameterType type, object value) {
            Value = value;
            Type = type;
        }
    }
}
