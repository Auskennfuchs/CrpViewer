using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace CrpViewer {
    class Texture : IDisposable {
        public string Checksum {
            get; private set;
        }    

        public ShaderResourceView SRV {
            get; private set;
        }

        private Texture2D texture;

        public Texture(string checkSum, Texture2D tex) {
            Checksum = checkSum;
            texture = tex;
            SRV = new ShaderResourceView(texture.Device, texture);
        }

        public void Dispose() {
            if (SRV != null) {
                SRV.Dispose();
            }
            if(texture!=null) {
                texture.Dispose();
            }
        }
    }
}
