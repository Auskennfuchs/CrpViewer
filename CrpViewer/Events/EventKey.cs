using System.Windows.Forms;

namespace CrpViewer.Events {

    class SKeyEvent {
        public Keys keyCode = Keys.None;
        public bool shift = false;
        public bool alt = false;
        public bool control = false;
    }

    abstract class EventKey : IEvent{
        public SKeyEvent Data {
            get;
        }

        public EventKey(SKeyEvent ev) {
            Data = ev;
        }

        public abstract string GetName();
        public abstract EventType GetEventType();
    }
}
