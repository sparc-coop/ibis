using System;
using System.Collections.Generic;
using System.Text;

namespace Kuvio.Kernel.AspNet.Blazor.Modal
{
    public interface IModalService
    {
        event Func<IModalParameters, IModalParameters> OnClose;
        //event Action<IModalParameters> OnClose;

        void Show(string title, Type contentType);

        void Show(string title, Type contentType, IModalParameters parameters);

        void Close();
        IModalParameters Close(IModalParameters returnValues);
    }

    public interface IModalParameters
    {
        void Add(string parameterName, object value);
        T Get<T>(string parameterName);
        T TryGet<T>(string parameterName);
    }
}
