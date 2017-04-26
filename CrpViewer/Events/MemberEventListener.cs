namespace CrpViewer.Events {
    class MemberEventListener : EventListener{

        IParentEventListener parent;

        public MemberEventListener(IParentEventListener parent) {
            this.parent = parent;
        }

        public override bool HandleEvent(IEvent ev) {
            if(parent!=null) {
                return parent.HandleEvent(ev);
            }
            return false;
        }
    }
}
