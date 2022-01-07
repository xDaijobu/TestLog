//using System;
//using System.ComponentModel;

//namespace TestLog
//{
//    [EditorBrowsable(EditorBrowsableState.Never)]
//    public static partial class ToLogger
//    {
//        public static string LogTag { get; private set; }

//        static ToLogger()
//        {
//        }

//        public static void Verbose(string tag, string message, Exception exception)
//        {
//            Verbose(tag, ConcatMessageException(message, exception));
//        }

//        public static void Debug(string tag, string message)
//        {
//        }

//        public static void Info(string tag, string message)
//        {
//        }

//        public static void Warn(string tag, string message)
//        {
//        }

//        public static void Error(string tag, string message)
//        {
//        }

//        private static string ConcatMessageException(string message, Exception exception)
//        {
//            return message + "\n" + exception;
//        }
//    }
//}
