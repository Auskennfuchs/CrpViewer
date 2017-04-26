namespace CrpViewer.Events {

    class EventKeyDown : EventKey {

        public EventKeyDown(SKeyEvent ev):base(ev) {
        }

        public override EventType GetEventType() {
            return EventType.KEYDOWN;
        }

        public override string GetName() {
            return "keyboard_keydown";
        }

    }
}
