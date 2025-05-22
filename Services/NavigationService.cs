using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace LanguageLearningApp.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route, IDictionary<string, object> parameters = null);
        Task GoBackAsync();
    }

    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string route, IDictionary<string, object> parameters = null)
        {
            if (parameters != null)
            {
                await Shell.Current.GoToAsync(route, parameters);
            }
            else
            {
                await Shell.Current.GoToAsync(route);
            }
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}