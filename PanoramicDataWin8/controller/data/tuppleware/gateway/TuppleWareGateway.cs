﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.input;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.model.data;
using PanoramicDataWin8.model.data.tuppleware;

namespace PanoramicDataWin8.controller.data.tuppleware.gateway
{

    public class TuppleWareGateway
    {
        public static async Task<string> Request(string endPoint, JObject data)
        {
            try
            {
                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine(data.ToString());
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var httpResponseMessage = await httpClient.PostAsync(endPoint, new StringContent(
                    data.ToString(),
                    Encoding.UTF8,
                    "application/json"));
                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine("TuppleWare Roundtrip Time: " + sw.ElapsedMilliseconds);
                }
                sw.Restart();

                var stringContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (MainViewController.Instance.MainModel.Verbose)
                {
                    Debug.WriteLine("TuppleWare Read Content Time: " + sw.ElapsedMilliseconds);
                }
                sw.Restart();

                return stringContent;
            }
            catch (Exception)
            {
                ContentDialog noWifiDialog = new ContentDialog()
                {
                    Title = "No wifi connection",
                    Content = "Check connection and try again",
                    PrimaryButtonText = "Ok"
                };

                ContentDialogResult result = await noWifiDialog.ShowAsync();
                
                throw;
            }
            return "";
        }
    }
}
