namespace CrpViewer.Events {

    class EventKeyUp : EventKey {

        public EventKeyUp(SKeyEvent ev) : base(ev) {
        }

        public override EventType GetEventType() {
            return EventType.KEYUP;
        }

        public override string GetName() {
            return "keyboard_keyup";
        }

    }
}
