using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;

namespace CrpViewer {
    static class Program {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main() {
            var form = new MainWindow();
            RenderLoop.Run(form, form.MainLoop);
            form.CleanUp();
            form.Close();
        }
    }
}
