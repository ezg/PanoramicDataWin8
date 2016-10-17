using Windows.UI.Notifications;

namespace PanoramicDataWin8.model.data.attribute
{
    public class ErrorHandler
    {
        public static void HandleError(string msg)
        {
            var x = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var toastTextElements = x.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(x.CreateTextNode(msg));
            var toast = new ToastNotification(x);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}