using CrpViewer.Events;
using SharpDX;

namespace CrpViewer {
    class FreeLook : EventListenerComponent {

        private bool mousedown = false;
        private System.Drawing.Point mousepos, delta;

        public FreeLook(Events.EventManager em) : base(em) {
            RegisterEvent(EventType.MOUSE_DOWN);
            RegisterEvent(EventType.MOUSE_UP);
            RegisterEvent(EventType.MOUSE_MOVE);
        }

        public override bool HandleEvent(IEvent ev) {
            switch (ev.GetEventType()) {
                case EventType.MOUSE_DOWN: {
                        var mev = (EventMouse)ev;
                        if (mev.Data.button == System.Windows.Forms.MouseButtons.Left) {
                            mousedown = true;
                            mousepos = mev.Data.position;
                        }
                        break;
                    }
                case EventType.MOUSE_UP: {
                        var mev = (EventMouse)ev;
                        if (mev.Data.button == System.Windows.Forms.MouseButtons.Left) {
                            mousedown = false;
                        }
                        break;
                    }
                case EventType.MOUSE_MOVE: {
                        var mev = (EventMouse)ev;
                        if (mousedown) {
                            delta = new System.Drawing.Point(delta.X + mousepos.X - mev.Data.position.X,
                                              delta.Y + mousepos.Y - mev.Data.position.Y);
                            mousepos = mev.Data.position;
                        }
                        break;
                    }
            }
            return false;
        }

        public override void Update(float elapsed) {
            if (mousedown) {
                Transform.AddRotation(Vector3.Up, delta.X * elapsed);
                Transform.AddRotation(Transform.GetRightVector(), delta.Y * elapsed);
                delta = System.Drawing.Point.Empty;
            }
        }
    }
}
