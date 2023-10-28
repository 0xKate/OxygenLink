using System;

namespace OxygenLink
{
    public class Extensions
    {
        public class EventArgs<T> : EventArgs
        {
            public T Object;
            public String Message;
            public EventArgs(T obj, string message = null)
            {
                this.Object = obj;
                this.Message = message;
            }
        }
        public class Event<T>
        {
            public event EventHandler<EventArgs<T>> OnEvent;
            public void Raise(T obj, string message = null)
            {
                OnEventRaised(new EventArgs<T>(obj, message));
            }
            protected virtual void OnEventRaised(EventArgs<T> e)
            {
                OnEvent?.Invoke(this, e);
            }
        }
    }
}
