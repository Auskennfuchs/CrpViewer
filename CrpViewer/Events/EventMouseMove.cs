namespace CrpViewer.Events {
    class EventMouseMove : EventMouse {

        public EventMouseMove(SMouseEvent ev) : base(ev) {
        }

        public override EventType GetEventType() {
            return EventType.MOUSE_MOVE;
        }

        public override string GetName() {
            return "mouse_move";
        }
    }
}
