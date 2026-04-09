namespace PL
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Reflection;

    public static class BrowserBehavior
    {
        // שינוי: במקום Url, אנחנו מגדירים תכונה ל-Html
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new PropertyMetadata(null, OnHtmlChanged));

        public static string GetHtml(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(HtmlProperty);
        }

        public static void SetHtml(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(HtmlProperty, value);
        }

        // כאשר מחרוזת ה-HTML משתנה (כשבוחרים הזמנה חדשה)
        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var browser = d as WebBrowser;
            if (browser != null)
            {
                // השתקת שגיאות סקריפט (חשוב מאוד ל-Leaflet!)
                try
                {
                    var axIWebBrowser2 = browser.GetType().InvokeMember("ActiveXInstance",
                        BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                        null, browser, new object[] { });

                    if (axIWebBrowser2 != null)
                    {
                        axIWebBrowser2.GetType().InvokeMember("Silent",
                            BindingFlags.SetProperty, null, axIWebBrowser2, new object[] { true });
                    }
                }
                catch { }

                var html = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(html))
                {
                    // שינוי: טעינת המחרוזת ישירות לדפדפן
                    try
                    {
                        browser.NavigateToString(html);
                    }
                    catch { }
                }
            }
        }
    }
}