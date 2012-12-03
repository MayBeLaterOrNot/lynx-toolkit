namespace PropertyCG.Tests
{
    using System;
    using System.ComponentModel;

    [Flags]
    public enum PropertyChangedFlags { None = 0, AffectsRender = 1, AffectsResults = 2 }

    public class PropertyChangedEventArgsEx : PropertyChangedEventArgs
    {
        public PropertyChangedEventArgsEx(string propertyName, object oldValue, object newValue, PropertyChangedFlags flags)
            : base(propertyName)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.Flags = flags;
        }

        public object OldValue { get; set; }
        public object NewValue { get; set; }
        public PropertyChangedFlags Flags { get; private set; }
    }
}