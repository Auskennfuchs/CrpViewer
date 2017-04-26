using System.Windows.Forms;
using System.Drawing;

namespace CrpViewer.Events {
    class SMouseEvent {
        public Point position = new Point(-1, -1);
        public MouseButtons button = MouseButtons.None;
    }

    abstract class EventMouse : IEvent{
        public SMouseEvent Data {
            get;
        }

        public EventMouse(SMouseEvent ev) {
            Data = ev;
        }

        public abstract string GetName();
        public abstract EventType GetEventType();
    }
}
