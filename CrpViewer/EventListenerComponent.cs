using CrpViewer.Events;
using EManager = CrpViewer.Events.EventManager;

namespace CrpViewer {
    abstract class EventListenerComponent : EntityComponent {
        private class EventListenerChild : EventListener {

            private EventListenerComponent parent;

            public EventListenerChild(EventListenerComponent p) {
                parent = p;
            }

            public override bool HandleEvent(IEvent ev) {
                return parent.HandleEvent(ev);
            }
        }

        private EventListenerChild evListener;

        public EventListenerComponent(EManager em) {
            evListener = new EventListenerChild(this);
            evListener.EventManager = em;
        }

        public abstract bool HandleEvent(IEvent ev);

        public void RegisterEvent(EventType et) {
            evListener.RegisterEvent(et);
        }
    }
}
