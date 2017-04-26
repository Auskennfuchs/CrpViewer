using System.Collections.Generic;

namespace CrpViewer.Events {
    class EventManager {

        private List<EventListener>[] eventListener = new List<EventListener>[(int)EventType.NUM_EVENTS];

        public EventManager() {
            for(int i=0;i<(int)EventType.NUM_EVENTS;i++) {
                eventListener[i] = new List<EventListener>();
            }
        }

        public void AddEventListener(EventType type, EventListener listener) {
            eventListener[(int)type].Add(listener);
        }

        public void RemoveEventListener(EventType type, EventListener listener) {
            if(eventListener[(int)type].Contains(listener)) {
                eventListener[(int)type].Remove(listener);
            }
        }

        public void ProcessEvent(IEvent ev) {
            foreach(EventListener listener in eventListener[(int)ev.GetEventType()]) {
                if (listener.HandleEvent(ev)) {
                    break;
                }
            }
        }
            
    }
}
