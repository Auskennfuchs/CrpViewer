using SharpDX;

namespace CrpViewer {
    class Transform {
        public bool Changed {
            get;
            private set;
        }

        private Vector3 scale;
        public Vector3 Scale {
            get {
                return scale;
            }
            set {
                scale = value;
                Changed = true;
            }
        }

        private Vector3 position;
        public Vector3 Position {
            get {
                return position;
            }
            set {
                position = value;
                Changed = true;
            }
        }
        public Vector3 GetTransformedPosition() {
            if (Parent != null) {
                Vector4 res = Vector3.Transform(position, Parent.WorldMatrix);
                return new Vector3(res.X, res.Y, res.Z);
            }
            return position;
        }

        private Matrix worldMatrix;
        public Matrix WorldMatrix {
            get {
                CalcMatrices();
                if (Parent != null && (Changed || Parent.Changed)) {
                    worldMatrix = LocalMatrix * Parent.WorldMatrix;
                }
                return worldMatrix;
            }
            private set {
                worldMatrix = value;
            }
        }

        public Matrix LocalMatrix {
            get;
            private set;
        }

        public Transform Parent {
            get; set;
        }

        public Quaternion Rotation {
            get;
            private set;
        }
        public void AddRotation(Vector3 axis, float angle) {
            Rotation *= Quaternion.RotationAxis(axis, angle);
            Rotation.Normalize();
            Changed = true;
        }
        public void SetRotation(Vector3 axis, float angle) {
            Rotation = Quaternion.RotationAxis(axis, angle);
            Rotation.Normalize();
            Changed = true;
        }
        public void SetRotationYawPitchRoll(float yaw, float pitch, float roll) {
            Rotation = Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
        }
        public Quaternion GetTransformedRotation() {
            Quaternion parentRot = Quaternion.Identity;
            if (Parent != null) {
                parentRot = Parent.GetTransformedRotation();
            }
            return Quaternion.Multiply(Rotation, parentRot);
        }

        public Transform() {
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
            WorldMatrix = LocalMatrix = Matrix.Identity;
        }

        public void Update() {
            CalcMatrices();
            Changed = false;
        }

        private void CalcMatrices() {
            if (Changed) {
                LocalMatrix = Matrix.Scaling(Scale) * Matrix.RotationQuaternion(Rotation) * Matrix.Translation(Position);
                if (Parent == null) {
                    WorldMatrix = LocalMatrix;
                }
            }
        }

        public Vector3 GetRightVector() {
            return GetRotationVector(Vector3.Right);
        }
        public Vector3 GetLeftVector() {
            return GetRotationVector(Vector3.Left);
        }
        public Vector3 GetForwardVector() {
            return GetRotationVector(Vector3.ForwardLH);
        }
        public Vector3 GetBackwardVector() {
            return GetRotationVector(Vector3.BackwardLH);
        }
        public Vector3 GetUpVector() {
            return GetRotationVector(Vector3.Up);
        }
        public Vector3 GetDownVector() {
            return GetRotationVector(Vector3.Down);
        }

        private Vector3 GetRotationVector(Vector3 vecin) {
            return Vector3.Transform(vecin, Quaternion.Conjugate(Rotation));
        }
    }
}
