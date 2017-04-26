using SharpDX;

namespace CrpViewer {
    class Camera : Entity {
        protected Matrix projMatrix;

        public Matrix ProjectionMatrix {
            get { return projMatrix; }
        }

        public Camera() {
            projMatrix = Matrix.Identity;
        }

        public void SetProjection(float zNear, float zFar, float aspect, float fov) {
            projMatrix = Matrix.PerspectiveFovLH(fov, aspect, zNear, zFar);
        }

        public Matrix ViewMatrix {
            get {
                return Matrix.Translation(Transform.GetTransformedPosition() * -1.0f) * Matrix.RotationQuaternion(-Transform.GetTransformedRotation());
            }
        }
    }
}
