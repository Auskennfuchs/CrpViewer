using System;
using System.Drawing;

namespace CrpViewer.Events {

    public class SResizeEvent : EventArgs {
        public Size Size = Size.Empty;
    }

    public class EventResize : IEvent {

        public SResizeEvent Data {
            get;
        }

        public EventResize(SResizeEvent ev) {
            Data = ev;
        }

        public EventType GetEventType() {
            return EventType.RESIZE;
        }

        public string GetName() {
            return "resize";
        }
    }
}
