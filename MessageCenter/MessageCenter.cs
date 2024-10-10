using Koboldcs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koboldcs.MessageCenter
{
    public class ScrollToBottomMessage
    {
        public bool Animate { get; set; }

        public ScrollToBottomMessage(bool animate)
        {
            Animate = animate;
        }
    }

    public class RequestLogitModifierEdit
    {
        public LogitBiasEntry LogitBiasEntry { get; set; }


        public RequestLogitModifierEdit(LogitBiasEntry lbe)
        {
            LogitBiasEntry = lbe;
        }
    }

    public class DisplayAlertMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Button1 { get; set; }
        public string Button2 { get; set; }


        public DisplayAlertMessage(string title, string message, string btn1 = "", string btn2 = "")
        {
            Title = title;
            Message = message;
            Button1 = btn1;
            Button2 = btn2;
        }
    }

    public class AlertResponseMessage
    {
        public bool IsConfirmed { get; set; } 
    }
}
