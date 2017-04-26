namespace CrpViewer.Events {
    class EventFrameStart : IEvent {
        public EventType GetEventType() {
            return EventType.FRAME_START;
        }

        public string GetName() {
            return "framestart";
        }
    }
}
