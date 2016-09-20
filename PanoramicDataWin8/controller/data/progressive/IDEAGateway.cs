﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PanoramicDataWin8.controller.view;
using PanoramicDataWin8.view.common;

namespace PanoramicDataWin8.controller.data.progressive
{
    public class IDEAGateway
    {
        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
        };

        public static async Task<string> Request(object data, string endpoint)
        {
            while (true)
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
                    HttpResponseMessage httpResponseMessage = null;
                    if (data != null)
                    {
                        httpResponseMessage = await httpClient.PostAsync(MainViewController.Instance.MainModel.Ip + "/" + endpoint, new StringContent(
                            data.ToString(),
                            Encoding.UTF8,
                            "application/json"));
                    }
                    else
                    {
                        httpResponseMessage = await httpClient.GetAsync(MainViewController.Instance.MainModel.Ip + "/" + endpoint);
                    }
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
                catch (Exception e)
                {
                    GatewayErrorDialog dialog = new GatewayErrorDialog()
                    {
                        Ip = MainViewController.Instance.MainModel.Ip,
                        Content = e.Message
                    };

                    ContentDialogResult result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Secondary)
                    {
                        CoreApplication.Exit();
                    }
                    else
                    {
                        MainViewController.Instance.MainModel.Ip = dialog.Ip;
                    }
                }
            }
        }
    }
}
