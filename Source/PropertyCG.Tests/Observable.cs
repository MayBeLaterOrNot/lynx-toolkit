namespace PropertyCG.Tests
{
    using System;
    using System.ComponentModel;

    public class Observable : INotifyPropertyChanged
    {
        protected bool SetValue<T>(ref T property, T value, string propertyName, PropertyChangedFlags flags = PropertyChangedFlags.None)
        {
            if (Object.Equals(property, value))
            {
                return false;
            }
            var oldValue = value;
            property = value;

            this.OnPropertyChanged(new PropertyChangedEventArgsEx(propertyName, oldValue, value, flags));

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgsEx args)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected bool SetReference<T>(ref T property, T value, string propertyName, PropertyChangedFlags flags = PropertyChangedFlags.None)
        {
            if (Object.Equals(property, value))
            {
                return false;
            }
            var oldValue = value;
            property = value;

            this.OnPropertyChanged(new PropertyChangedEventArgsEx(propertyName, oldValue, value, flags));

            return true;
        }

        protected void ResolveReference<T>(ref Reference<T> reference)
        {
        }
    }
}