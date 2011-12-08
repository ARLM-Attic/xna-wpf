using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace XNAPF.Tools
{
    public delegate bool Predicate();

    public abstract class CCommand : ICommand
    {
        #region Properties

        /// <summary>
        /// Predictate use to specify if the action could be execute
        /// </summary>
        /// 
        public Predicate DefaultValidator { get; set; }

        #endregion

        #region Event

        /// <summary>
        /// Event that notify if the status of the action execution had changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion

        #region Method

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        public virtual void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }

    /// <summary>
    /// helper class use to create command from delegate
    /// </summary>
    /// <typeparam name="T">T is type of the parameter send to the action</typeparam>
    public class CActionCommand<T> : CCommand
    // where T : class
    {

        #region Property

        /// <summary>
        /// Action Execute by the command
        /// </summary>
        public Action<T> Action { get; set; }


        /// <summary>
        /// Predictate use to specify if the action could be execute
        /// </summary>
        public Predicate<T> Validator { get; set; }

        #endregion

        #region Method

        /// <summary>
        /// Method that use the validator Property to notify if the action could be execute
        /// </summary>
        /// <param name="parameter">Parameter send by the view</param>
        /// <returns>bool that notify if the action could be execute</returns>
        public override bool CanExecute(object parameter)
        {
            if (Validator != null && parameter is T)
                return Validator((T) parameter);
            if (DefaultValidator != null)
                return DefaultValidator();
            return true;
        }

        /// <summary>
        /// Methode that execute the action 
        /// </summary>
        /// <param name="parameter">Parameter use to execute the action</param>
        public override void Execute(object parameter)
        {
            if (Action != null)
                Action((T) parameter);
        }

        #endregion
    }

    /// <summary>
    /// helper class use to create command from delegate
    /// </summary>
    public class CActionCommand : CCommand
    {

        #region Property

        /// <summary>
        /// Action Execute by the command
        /// </summary>
        public Action Action { get; set; }

        #endregion

        #region Method

        /// <summary>
        /// Method that use the validator Property to notify if the action could be execute
        /// </summary>
        /// <param name="parameter">useless</param>
        /// <returns>true</returns>
        public override bool CanExecute(object parameter)
        {
            if (DefaultValidator != null)
                return DefaultValidator();
            return true;
        }


        /// <summary>
        /// Methode that execute the action 
        /// </summary>
        /// <param name="parameter">useless</param>
        public override void Execute(object parameter)
        {
            if (Action != null)
                Action();
        }

        #endregion
    }
}
