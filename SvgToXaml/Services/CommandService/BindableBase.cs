using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace SvgToXaml.CommandServices
{
    public class BindableBase : ObservableObject
    {
        private Dictionary<string, object> ?_propertyBag;

        private Dictionary<string, object> PropertyBag => _propertyBag ?? (_propertyBag = new Dictionary<string, object>());

       
        protected T? GetValue<T>([CallerMemberName] string? propertyName = null!)
        {
            GuardPropertyName(propertyName);
            return GetPropertyCore<T>(propertyName);
        }
        protected T? GetValue<T>(Expression<Func<T>> expression)
        {            
            return GetProperty<T>(expression);
        }         
     
        protected bool SetValue<T>(T value, [CallerMemberName] string? propertyName = null!)
        {
            return SetValue(value, (Action)null!, propertyName);
        }

        protected bool SetValue<T>(T value, Action changedCallback, [CallerMemberName] string? propertyName = null!)
        {
            return SetPropertyCore(propertyName, value, changedCallback);
        }

        protected bool SetValue<T>(T value, Action<T> changedCallback, [CallerMemberName] string? propertyName = null!)
        {
            return SetPropertyCore(propertyName, value, changedCallback);
        }
        protected bool SetValue<T>(ref T storage, T value, [CallerMemberName] string ?propertyName = null)
        {
            return SetValue(ref storage, value, null, propertyName);
        }
        protected bool SetValue<T>(ref T storage, T value, Action ?changedCallback, [CallerMemberName] string ?propertyName = null)
        {
            GuardPropertyName(propertyName);
            return SetPropertyCore(ref storage, value, propertyName, changedCallback);
        }
        protected bool SetValue<T>(Expression<Func<T>> expression, T value, Action<T>? changedCallback)
        {
            return SetProperty<T>(expression, value, changedCallback);
        }
        protected bool SetValue<T>(Expression<Func<T>> expression, T value)
        {          
            return SetProperty<T>(expression, value);
        }
        protected bool SetValue<T>(Expression<Func<T>> expression, T value, Action? changedCallback)
        {           
            return SetProperty(expression, value, changedCallback);
        }        
        private bool SetProperty<T>(Expression<Func<T>> expression, T value, Action<T>? changedCallback)
        {
            string? propertyName = GetPropertyName(expression);
            return SetPropertyCore(propertyName, value, changedCallback);
        }
        private bool SetProperty<T>(Expression<Func<T>> expression, T value)
        {
            return SetProperty(expression, value, (Action)null!);
        }
        private bool SetProperty<T>(Expression<Func<T>> expression, T value, Action? changedCallback)
        {
            string? propertyName = GetPropertyName(expression);
            return SetPropertyCore(propertyName, value, changedCallback);
        }
        private bool SetPropertyCore<T>(string? propertyName, T value, Action? changedCallback)
        {
            T oldValue;
            bool flag = SetPropertyCore(propertyName, value, out oldValue);
            if (flag)
            {
                changedCallback?.Invoke();
            }

            return flag;
        }

        private bool SetPropertyCore<T>(string? propertyName, T value, Action<T>? changedCallback)
        {
            T oldValue;
            bool flag = SetPropertyCore(propertyName, value, out oldValue);
            if (flag)
            {
                changedCallback?.Invoke(oldValue);
            }

            return flag;
        }
        private bool SetPropertyCore<T>(string ?propertyName, T value, out T oldValue)
        {          
            oldValue = default(T)!;
            if (PropertyBag.TryGetValue(propertyName!, out var value2))
            {
                oldValue = (T)value2;
            }

            if (CompareValues(oldValue, value))
            {
                return false;
            }

            lock (PropertyBag)
            {
                PropertyBag[propertyName!] = value!;
            }

            OnPropertyChanged(propertyName);
            return true;
        }
        private bool SetPropertyCore<T>(ref T storage, T value, string? propertyName, Action? changedCallback)
        {           
            if (CompareValues(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            changedCallback?.Invoke();
            return true;
        }
        private T? GetProperty<T>(Expression<Func<T>> expression)
        {
            string? propertyName = GetPropertyName(expression);
            return GetPropertyCore<T>(propertyName);
        }
        private T? GetPropertyCore<T>(string? propertyName)
        {
            if (PropertyBag.TryGetValue(propertyName!, out var value))
            {
                return (T)value;
            }

            return default(T);
        }

        private static bool CompareValues<T>(T storage, T value)
        {
            return EqualityComparer<T>.Default.Equals(storage, value);
        }
        private static string? GetPropertyName<T>(Expression<Func<T>>? expression)
        {
            return GetPropertyNameFast(expression);
        }

        private static string? GetPropertyNameFast(LambdaExpression? expression)
        {
            if (expression!.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException("MemberExpression is expected in expression.Body", "expression");
            }

            MemberInfo member = memberExpression.Member;
            if (member.MemberType == MemberTypes.Field && member.Name != null && member.Name.StartsWith("$VB$Local_"))
            {
                return member.Name.Substring("$VB$Local_".Length);
            }

            return member.Name;
        }
        private static void GuardPropertyName(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException("propertyName");
            }
        }
    }
}
