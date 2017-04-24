using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace CrpViewer.Renderer {
    public class Renderer : IDisposable{
        public Device Device {
            get; private set;
        }

        public static Renderer Instance {
            get; private set;
        }

        public DeviceContext DevContext {
            get; private set;
        }

        public ParameterManager Parameters {
            get; private set;
        }

        public Renderer() {
            Instance = this;
            DeviceCreationFlags dcf = DeviceCreationFlags.None;
#if DEBUG
            dcf |= DeviceCreationFlags.Debug;
#endif
            Device = new Device(SharpDX.Direct3D.DriverType.Hardware, dcf);

            DevContext = Device.ImmediateContext;

            Parameters = new ParameterManager();
        }

        public void Dispose() {
            Instance = null;
            if (Device != null) {
                Device.Dispose();
            }
        }
    }
}
