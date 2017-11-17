using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SkyWest.Common.WPF
{
    public abstract class SubscribableState : INotifyPropertyChanged, ICloneable
    {
        private ViewModelNode VM;
        private bool _isLoading;
        private Dictionary<string, Action<object>> EventHandlers;
        private Dictionary<string, ICommand> _commands;

        public SubscribableState(ViewModelNode _VM)
        {
            VM = _VM;
            EventHandlers = new Dictionary<string, Action<object>>();
            _commands = new Dictionary<string, ICommand>();
            _isLoading = false;
        }

        public void AddHandler(string property, Action<object> handler)
        {
            EventHandlers.Add(property, handler);
        }

        public void AddCommand(string commandName, Action action)
        {
            Commands.Add(commandName, new RelayCommand(action));
        }

        [Order]
        public virtual bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading == value)
                    return;
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        [Order]
        public Dictionary<string, ICommand> Commands
        {
            get { return _commands; }
            set
            {
                if (_commands == value)
                    return;
                _commands = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            if (IsLoading)
                return;

            //update props in children
            foreach (ViewModelNode child in VM.GetChildren())
            {
                if (child.Props.ContainsKey(propertyName))
                {
                    if (GetType().GetProperty(propertyName) != null)
                        child.Props[propertyName] = GetType().GetProperty(propertyName).GetValue(this);
                }
            }

            //notify VM
            if (EventHandlers.ContainsKey(propertyName))
            {
                EventHandlers[propertyName](propertyName);
            }

            //broadcast
            if (GetType().GetProperty(propertyName) != null)
                VM.GetRoot().Broadcast(propertyName, GetType().GetProperty(propertyName).GetValue(this));
        }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }
}
