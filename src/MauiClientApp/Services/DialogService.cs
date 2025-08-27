using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Amanati.ge.Services
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string accept);
        Task<bool> ShowDialogAsync(string title, string message, string accept, string cancel);

    }


    public class DialogService : IDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string accept)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, accept);
        }

        //public async Task ShowAPopupAsync(string title, string message, string accept)
        //{
        //    await Application.Current.MainPage.SH .ShowPopup.DisplayAlert(title, message, accept);
        //}

        public async Task<bool> ShowDialogAsync(string title, string message, string accept, string cancel)
        {
            return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }
    }
}
