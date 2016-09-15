using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace PanoramicDataWin8.model.data
{
    public class ErrorHandler
    {
        public static void HandleError(string msg)
        {
            XmlDocument x = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            XmlNodeList toastTextElements = x.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(x.CreateTextNode(msg));
            ToastNotification toast = new ToastNotification(x);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
