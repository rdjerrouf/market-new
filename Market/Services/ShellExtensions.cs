using Microsoft.Maui.Controls;
using System.Web;
namespace Market.Services
{
    public static class ShellExtensions
    {
        public static Task<string> GetQueryParameterAsync(this Shell shell, string parameterName)
        {
            try
            {
                var query = Shell.Current.CurrentState.Location.Query;
                if (string.IsNullOrEmpty(query))
                    return Task.FromResult(string.Empty);

                query = query.TrimStart('?');
                var parameters = query.Split('&');

                foreach (var param in parameters)
                {
                    var keyValue = param.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == parameterName)
                    {
                        return Task.FromResult(keyValue[1]);
                    }
                }

                return Task.FromResult(string.Empty);
            }
            catch
            {
                return Task.FromResult(string.Empty);
            }
        }
    }
}