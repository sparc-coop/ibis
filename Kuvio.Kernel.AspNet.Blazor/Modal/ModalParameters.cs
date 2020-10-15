using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.AspNet.Blazor.Modal
{
    public class ModalParameters : IModalParameters
    {
        private Dictionary<string, object> _parameters;

        public ModalParameters()
        {
            _parameters = new Dictionary<string, object>();
        }

        public void Add(string parameterName, object value)
        {
            _parameters[parameterName] = value;
        }

        public T Get<T>(string parameterName)
        {
            if (!_parameters.ContainsKey(parameterName))
            {
                throw new KeyNotFoundException($"{parameterName} does not exist in modal parameters");
            }

            return (T)_parameters[parameterName];
        }

        public T TryGet<T>(string parameterName)
        {
            if (!_parameters.ContainsKey(parameterName))
            {
                return default(T);
            }

            return (T)_parameters[parameterName];
        }
    }
}
