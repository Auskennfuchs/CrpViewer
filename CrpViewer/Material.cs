using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrpViewer {
    enum TEXTURE_TYPE {
        DIFFUSE,
        NORMAL,
        NUM_TEXTYPE
    }

    class Material {
        Dictionary<TEXTURE_TYPE, Texture> textures = new Dictionary<TEXTURE_TYPE, Texture>();

        public void AddTexture(TEXTURE_TYPE type, Texture tex) {
            textures.Add(type, tex);
        }

        public Texture GetTexture(TEXTURE_TYPE type) {
            if(textures.ContainsKey(type)) {
                return textures[type];
            }
            return null;
        }
    }
}
