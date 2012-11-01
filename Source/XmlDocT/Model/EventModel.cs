namespace XmlDocT
{
    using System.Reflection;

    public class EventModel : Content
    {
        public EventModel(EventInfo ei)
        {
            this.Info = ei;
        }

        public override string ToString()
        {
            return this.Info.Name;
        }
    }
}