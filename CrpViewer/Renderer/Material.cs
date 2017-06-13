using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace CrpViewer.Renderer {
    class Material {
        public Dictionary<string, ShaderResourceView> SRV {
            get; private set;
        }

        public Material() {
            SRV = new Dictionary<string, ShaderResourceView>();
        }

        public void SetRenderParameters(Renderer renderer) {
        }
    }
}
