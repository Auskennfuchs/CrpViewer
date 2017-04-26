using System.Collections.Generic;

namespace CrpViewer.Events {
    abstract class EventListener {

        private EventManager eventManager;

        public EventManager EventManager {
            get {
                return eventManager;
            }
            set {
                SetEventManager(value);
            }
        }

        private List<EventType> registeredEvents = new List<EventType>();

        public EventListener() {
            EventManager = null;
        }

        public void RegisterEvent(EventType type) {
            if(!registeredEvents.Contains(type)) {
                registeredEvents.Add(type);
            }

            if(EventManager!=null) {
                EventManager.AddEventListener(type, this);
            }
        }

        public void UnregisterEvent(EventType type) {
            if(registeredEvents.Contains(type)) {
                registeredEvents.Remove(type);
            }

            if(EventManager!=null) {
                EventManager.RemoveEventListener(type, this);
            }
        }

        public abstract bool HandleEvent(IEvent ev);

        private void SetEventManager(EventManager em) {
            if(eventManager!=null) {
                foreach (EventType et in registeredEvents) {
                    eventManager.RemoveEventListener(et,this);
                 }
            }
            eventManager = em;
            foreach (EventType et in registeredEvents) {
                eventManager.AddEventListener(et, this);
            }
        }
    }
}
