namespace CrpViewer.Events {
    class EventMouseUp : EventMouse {

        public EventMouseUp(SMouseEvent ev) : base(ev) {
        }

        public override EventType GetEventType() {
            return EventType.MOUSE_UP;
        }

        public override string GetName() {
            return "mouse_up";
        }
    }
}
