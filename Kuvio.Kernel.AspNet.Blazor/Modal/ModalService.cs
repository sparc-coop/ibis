using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Kuvio.Kernel.AspNet.Blazor.Modal
{
    public class ModalService : IModalService
    {
        public event Func<IModalParameters, IModalParameters> OnClose;

        //event Func<IModalParameters, IModalParameters> OnClose;
        //public event Action OnClose;
        //public event Action<IModalParameters> OnClose2;

        internal event Action<string, RenderFragment, IModalParameters> OnShow;

        //event Func<IModalParameters, IModalParameters> IModalService.OnClose
        //{
        //    add
        //    {
        //        OnClose?.Invoke(null);
        //    }

        //    remove
        //    {
        //        OnClose?.Invoke(null);
        //    }
        //}

        public void Show(string title, Type contentType)
        {
            Show(title, contentType, new ModalParameters());
        }

        public void Show(string title, Type contentType, IModalParameters parameters)
        {
            if (!typeof(ComponentBase).IsAssignableFrom(contentType))
            {
                throw new ArgumentException($"{contentType.FullName} must be a Blazor Component");
            }

            var content = new RenderFragment(x => { x.OpenComponent(1, contentType); x.CloseComponent(); });

            OnShow?.Invoke(title, content, parameters);
        }

        public void Close()
        {
            //OnClose?.Invoke(null);
            OnClose?.Invoke(new ModalParameters());
        }

        public IModalParameters Close(IModalParameters modalParameters)
        {
            //return OnClose?.Invoke(modalParameters);
            return OnClose?.Invoke(modalParameters);
        }
    }
}