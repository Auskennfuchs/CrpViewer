namespace CrpViewer.Events {
    class EventMouseDown : EventMouse {

        public EventMouseDown(SMouseEvent ev):base(ev) {
        }

        public override EventType GetEventType() {
            return EventType.MOUSE_DOWN;
        }

        public override string GetName() {
            return "mouse_down";
        }
    }
}
