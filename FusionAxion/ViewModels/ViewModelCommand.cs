using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FusionAxion.ViewModels
{
    public class ViewModelCommand : ICommand
    {
        // Fields
        private readonly Action<object> executeAction;
        private readonly Predicate<object> canExecuteAction;

        // Constructors
        public ViewModelCommand(Action<object> executeAction)
        {
            this.executeAction = executeAction;
            canExecuteAction = null;
        }
        public ViewModelCommand(Action<object> executeAction, Predicate<object> canExecuteAction)
        {
            this.executeAction = executeAction;
            this.canExecuteAction = canExecuteAction;
        }

        // Events
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value;  }
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteAction == null || canExecuteAction(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}
