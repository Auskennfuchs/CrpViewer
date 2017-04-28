using System.Collections.Generic;

namespace CrpViewer {

    class Model {

        public List<Material> Materials {
            get; set;
        }

        public List<Texture> Textures {
            get; set;
        }

        public StaticMesh Mesh {
            get; set;
        }

        public Model() {

        }

    }
}
