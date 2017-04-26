using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrpViewer {
    class Entity {
        private List<Entity> childs = new List<Entity>();
        private List<EntityComponent> components = new List<EntityComponent>();

        public Transform Transform {
            get; set;
        }

        public Entity() {
            Transform = new Transform();
        }

        public Entity AddChild(Entity c) {
            childs.Add(c);
            return this;
        }

        public Entity AddComponent(EntityComponent component) {
            component.Parent = this;
            components.Add(component);
            return this;
        }

        public Entity RemoveComponent(EntityComponent component) {
            if (components.Contains(component)) {
                components.Remove(component);
                component.Parent = null;
            }
            return this;
        }

        public void OnUpdate(float elapsed) {
            Update(elapsed);

            if (components.Count > 0) {
                components.ForEach((c) => {
                    c.Update(elapsed);
                });
            }
            if (childs.Count > 0) {
                childs.ForEach((c) => {
                    c.OnUpdate(elapsed);
                });
            }
        }

        public virtual void Update(float elapsed) { }

        public void OnRender(Renderer.Renderer renderer) {
            if (components.Count > 0) {
                components.ForEach((c) => {
                    c.Render(renderer);
                });
            }
            if (childs.Count > 0) {
                childs.ForEach((c) => {
                    c.OnRender(renderer);
                });
            }
        }

        public virtual void PreRender(Renderer.Renderer renderer) { }

//        public virtual void Render(RenderPipeline pipeline) { }
    }
}
