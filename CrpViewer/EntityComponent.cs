namespace CrpViewer {
    class EntityComponent {
        private Entity parent;
        public Entity Parent {
            get { return parent; }
            set {
                if (parent != null) {
                    parent.RemoveComponent(this);
                }
                parent = value;
                if (parent != null) {
                    Transform = parent.Transform;
                }
            }
        }

        public Transform Transform {
            get;
            protected set;
        }

        public virtual void Update(float elapsed) { }

        public virtual void Render(Renderer.Renderer renderer) { }
    }
}