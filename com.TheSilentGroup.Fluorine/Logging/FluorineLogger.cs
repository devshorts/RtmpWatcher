using System;
using log4net;

namespace com.TheSilentGroup.Fluorine.Logging
{
    internal enum LogType
    {
        Fatal,
        Error,
        Warn,
        Info,
        Debug
    }

    public static class FluorineLogger
    {
        /// <summary>
        /// Returns true if an external event already handled the log
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static bool Log(object sender, string message, Exception ex)
        {
            if(LogEvent != null)
            {
                var args = new FluorineLogEventArgs(message, ex);
                LogEvent(sender, args);
                if(!args.Handled)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public static event FluorineLogEvent LogEvent;
    }

    public delegate void FluorineLogEvent(object sender, FluorineLogEventArgs args);

    public class FluorineLogEventArgs : EventArgs
    {
        private string _message;
        private Exception _ex;

        public bool Handled { get; set; }

        internal FluorineLogEventArgs(string message, Exception ex)
        {
            _message = message;
            _ex = ex;
        }

        public string Message
        {
            get { return _message; }
        }

        public Exception Exception
        {
            get { return _ex; }
        }
    }
}