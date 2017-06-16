using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CrpViewer.Renderer {
    public class ParameterManager {
        Dictionary<string, RenderParameter> parameters = new Dictionary<string, RenderParameter>();

        private static string WORLDMATRIX = "worldMatrix";
        private static string VIEWMATRIX = "viewMatrix";
        private static string PROJMATRIX = "projMatrix";
        private static string WORLDVIEWPROJMATRIX = "worldViewProjMatrix";
        private static string VIEWPROJMATRIX = "viewProjMatrix";
        private static string INV_VIEWPROJMATRIX = "invViewProjMatrix";

        public ParameterManager() {
            SetParameter(WORLDMATRIX, Matrix.Identity);
            SetParameter(VIEWMATRIX, Matrix.Identity);
            SetParameter(PROJMATRIX, Matrix.Identity);
            SetParameter(WORLDVIEWPROJMATRIX, Matrix.Identity);
        }

        public void SetParameter(string name, Matrix mat) {
            if (!SetParam(name, mat, RenderParameterType.MATRIX)) {
                parameters.Add(name, new MatrixParameter(mat));
            }
        }

        public RenderParameter GetParameter(string name) {
            if (parameters.ContainsKey(name)) {
                return parameters[name];
            }
            return null;
        }

        public Matrix GetMatrixParameter(string name) {
            return (Matrix)GetParam(name, RenderParameterType.MATRIX);
        }

        public void SetParameter(string name, Vector3 vec) {
            if (!SetParam(name, vec, RenderParameterType.VECTOR3)) {
                parameters.Add(name, new Vector3Parameter(vec));
            }
        }
        public Vector3 GetVector3Parameter(string name) {
            return (Vector3)GetParam(name, RenderParameterType.VECTOR3);
        }

        public void SetParameter(string name, Vector4 vec) {
            if (!SetParam(name, vec, RenderParameterType.VECTOR4)) {
                throw new NotImplementedException();
            }
        }
        public Vector4 GetVector4Parameter(string name) {
            return (Vector4)GetParam(name, RenderParameterType.VECTOR4);
        }

        public void SetWorldMatrix(Matrix world) {
            SetParameter(WORLDMATRIX, world);
            UpdateMatrices();
        }
        public void SetViewMatrix(Matrix view) {
            SetParameter(VIEWMATRIX, view);
            UpdateMatrices();
        }
        public void SetProjectionMatrix(Matrix projection) {
            SetParameter(PROJMATRIX, projection);
            UpdateMatrices();
        }

        private bool SetParam(string name, object obj, RenderParameterType type) {
            if (parameters.ContainsKey(name)) {
                var param = parameters[name];
                if (param.Type != type) {
                    throw CrpRendererException.Create("Wrong Parametertype expected " + type + " but was " + param.GetType());
                }
                param.Value = obj;
                return true;
            }
            return false;
        }

        private object GetParam(string name, RenderParameterType type) {
            if (parameters.ContainsKey(name)) {
                var param = parameters[name];
                if (param.Type != type) {
                    throw CrpRendererException.Create("Wrong Parametertype expected " + type + " but was " + param.GetType());
                }
                return param.Value;
            }
            return null;
        }

        private void UpdateMatrices() {
            Matrix world = (Matrix)GetParam(WORLDMATRIX, RenderParameterType.MATRIX);
            Matrix view = (Matrix)GetParam(VIEWMATRIX, RenderParameterType.MATRIX);
            Matrix proj = (Matrix)GetParam(PROJMATRIX, RenderParameterType.MATRIX);
            Matrix viewProj = view * proj;
            Matrix invViewProj = Matrix.Invert(viewProj);
            SetParameter(WORLDVIEWPROJMATRIX, world * view * proj);
            SetParameter(VIEWPROJMATRIX, viewProj);
            SetParameter(INV_VIEWPROJMATRIX, invViewProj);
        }
    }
}
