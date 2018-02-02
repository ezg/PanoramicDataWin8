using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.view.common;

namespace PanoramicDataWin8.controller.data.progressive
{
    public static class IDEAGateway
    {
        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public static async Task<string> Request(object data, string endpoint)
        {
            while (true)
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var handler = new HttpClientHandler();
                    var httpClient = new HttpClient();

                    if (!string.IsNullOrEmpty(MainViewController.Instance.MainModel.Username))
                    {
                        var byteArray = Encoding.ASCII.GetBytes(MainViewController.Instance.MainModel.Username + ":" + MainViewController.Instance.MainModel.Password);
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage httpResponseMessage = null;
                    if (data != null)
                    {
                        httpResponseMessage = await httpClient.PostAsync(
                            MainViewController.Instance.MainModel.Hostname +
                            MainViewController.Instance.MainModel.APIPath + "/" + endpoint, new StringContent(
                                data.ToString(),
                                Encoding.UTF8,
                                "application/json"));
                    }
                    else
                    {
                        httpResponseMessage = await httpClient.GetAsync(
                            MainViewController.Instance.MainModel.Hostname +
                            MainViewController.Instance.MainModel.APIPath + "/" + endpoint);
                    }
                    sw.Restart();
                    var stringContent = await httpResponseMessage.Content.ReadAsStringAsync();
                    if (stringContent.ToUpper().Contains("LOGIN FORM"))
                    {
                        throw new WebException();
                    }
                    sw.Restart();

                    return stringContent;
                }
                catch (Exception e)
                {
                    var darpa = MainViewController.Instance.MainModel.IsDarpaSubmissionMode;
                    var dialog = new GatewayErrorDialog
                    {
                        Ip = MainViewController.Instance.MainModel.Hostname,
                        Content = darpa ? 
                            "Enter the connection URLs that was provided by the evaluator\nand your D3M login."
                            : e.Message,
                        StackTrace = (MainViewController.Instance.MainModel.Hostname + "/" + endpoint) + "\n" + e.InnerException + "\n" + e.StackTrace
                    };
                    dialog.Title = darpa ? "Please enter connection URL" : "Connection Problems";

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Secondary)
                    {
                        CoreApplication.Exit();
                    }
                    else
                    {
                        string newIp = dialog.Ip;
                        if (newIp.EndsWith("/"))
                        {
                            newIp = newIp.TrimEnd('/');
                        }
                        if (!newIp.StartsWith("http://") &&
                            !newIp.StartsWith("https://"))
                        {
                            newIp = "http://" + newIp;
                        }
                        MainViewController.Instance.MainModel.Hostname = newIp;
                        MainViewController.Instance.MainModel.Username = dialog.Username;
                        MainViewController.Instance.MainModel.Password = dialog.Password;
                    }
                }
        }
    }
}