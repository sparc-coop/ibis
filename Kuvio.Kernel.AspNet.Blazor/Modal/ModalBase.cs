using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components.Web;

namespace Kuvio.Kernel.AspNet.Blazor.Modal
{
    public class ModalBase : ComponentBase, IDisposable
    {
        [Inject] private IModalService ModalService { get; set; }

        protected bool IsVisible { get; set; }
        protected string Title { get; set; }
        protected RenderFragment Content { get; set; }
        protected IModalParameters Parameters { get; set; }
        protected IModalParameters ReturnValues { get; set; }

        protected override void OnInitialized()
        {
            ((ModalService)ModalService).OnShow += ShowModal;
            ModalService.OnClose += CloseModal;
            //ModalService.OnClose2 += CloseModal;
        }

        public void ShowModal(string title, RenderFragment content, IModalParameters parameters)
        {
            Title = title;
            Content = content;
            Parameters = parameters;

            IsVisible = true;
            StateHasChanged();
        }

        public IModalParameters CloseModal(IModalParameters returnValues)
        {
            IsVisible = false;
            Title = "";
            Content = null;
            Parameters = null;
            ReturnValues = null;
            //ReturnValues = returnValues;

            StateHasChanged();

            return returnValues;
        }

        public void Dispose()
        {
            IsVisible = false;
            Title = "";
            Content = null;
            Parameters = null;
            ReturnValues = null;

            ((ModalService)ModalService).OnShow -= ShowModal;
            ModalService.OnClose -= CloseModal;
            //ModalService.OnClose2 -= CloseModal;
        }
    }
}