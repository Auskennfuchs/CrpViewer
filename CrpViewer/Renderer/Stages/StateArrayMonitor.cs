using System;

namespace CrpViewer.Renderer.Stages {
    public class TStateArrayMonitor<T> where T : class {

        public bool NeedUpdate {
            get; private set;
        }

        private T initialState;
        public int NumSlots {
            get; private set;
        }

        public int StartSlot {
            get; private set;
        }
        public int EndSlot {
            get; private set;
        }

        public int Range {
            get {
                return Math.Max(0, EndSlot - StartSlot + 1);
            }
        }

        public T[] States {
            get; private set;
        }

        private T[] changedStates;

        public T[] ChangedStates {
            get {
                if(Range==0) {
                    return null;
                }
                if (NeedUpdate) {
                    for (var i = 0; i < Range; i++) {
                        changedStates[i] = States[StartSlot + i];
                    }
                }
                return changedStates;
            }
        }

        public TStateArrayMonitor<T> Sister {
            get; set;
        }

        public TStateArrayMonitor(int numSlots, T initialState) {
            States = new T[numSlots];
            changedStates = new T[numSlots];
            this.initialState = initialState;
            NumSlots = numSlots;
            ResetTracking();
        }

        public void SetState(int slot, T state) {
            States[slot] = state;
            bool needUpdate = !SameSister(slot);
            if (needUpdate) {
                NeedUpdate = true;
                if (slot < StartSlot) {
                    StartSlot = slot;
                }
                if (slot > EndSlot) {
                    EndSlot = slot;
                }
            }
        }

        private bool SameSister(int slot) {
            if (Sister != null) {
                return States[slot] == Sister.States[slot];
            }
            return false;
        }

        public void ResetTracking() {
            NeedUpdate = false;
            StartSlot = 0;
            EndSlot = -1;
        }

        public void InitializeState() {
            for (int i = 0; i < NumSlots; i++) {
                States[i] = initialState;
                changedStates[i] = initialState;
            }
        }
    }
}
