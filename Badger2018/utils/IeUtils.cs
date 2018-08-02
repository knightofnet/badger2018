using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace Badger2018.utils
{
    class IeUtils
    {
        private static bool webReady;

        public static void SetField(WebBrowser wb, string formname, string fieldname, string fieldvalue)
        {
            HtmlElement f = wb.Document.Forms[formname].All[fieldname];
            f.SetAttribute("value", fieldvalue);
        }

        public static void SetRadio(WebBrowser wb, string formname, string fieldname, bool isChecked)
        {
            HtmlElement f = wb.Document.Forms[formname].All[fieldname];
            f.SetAttribute("checked", isChecked ? "True" : "False");
        }

        public static void SubmitForm(WebBrowser wb, string formname)
        {
            HtmlElement f = wb.Document.Forms[formname];
            f.InvokeMember("submit");
        }

        public static void ClickButtonAndWait(WebBrowser wb, string buttonname, int timeOut)
        {
            HtmlElement f = wb.Document.All[buttonname];
            webReady = false;
            f.InvokeMember("click");
            DateTime endTime = DateTime.Now.AddSeconds(timeOut);
            bool finished = false;
            while (!finished)
            {
                if (webReady)
                    finished = true;
                Application.DoEvents();
                //if (aborted)
                //    throw new EUserAborted();
                Thread.Sleep(50);
                if ((timeOut != 0) && (DateTime.Now > endTime))
                {
                    finished = true;
                }
            }
        }

        public static void ClickButtonAndWait(WebBrowser wb, string buttonname)
        {
            ClickButtonAndWait(wb, buttonname, 0);
        }

        public static void Navigate(System.Windows.Controls.WebBrowser wb, string url, int timeOut)
        {
            webReady = false;
            wb.Navigate(url);
            DateTime endTime = DateTime.Now.AddSeconds(timeOut);
            bool finished = false;
            while (!finished)
            {
                if (webReady)
                    finished = true;
                Application.DoEvents();
                //if (aborted)
                //   throw new EUserAborted();
                Thread.Sleep(50);
                if ((timeOut != 0) && (DateTime.Now > endTime))
                {
                    finished = true;
                }
            }
        }

        private static void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webReady = true;
        }

    }
}
