namespace CrpViewer.Events {
    public interface IParentEventListener {
        bool HandleEvent(IEvent ev);
    }
}
