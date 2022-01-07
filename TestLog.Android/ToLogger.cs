//using Android.Util;
//using Java.Util.Logging;

//namespace TestLog.Droid
//{
//    public static partial class ToLogger
//    {
//        public static string LogTag { get; private set; } = "ToLoggerXamarin";

//        public static Logger CustomLogger { get; set; } = null;
//        public static Level LogLevel { get; private set; }

//        public static void SetLogLevel(Level level)
//        {
//            LogLevel = level;

//            if (CustomLogger != null)
//                CustomLogger.Level = level;
//        }

//        static ToLogger()
//        {
//        }

//        public static void Verbose(string tag, string message)
//        {
//            if (CustomLogger != null)
//                CustomLogger.Log(LogLevel, tag, message);
//            else
//                Log.Verbose(tag, message);
//        }

//        public static void Debug(string tag, string message)
//        {
//            if (CustomLogger != null)
//                CustomLogger.Log(LogLevel, tag, message);
//            else
//                Log.Debug(tag, message);
//        }

//        public static void Info(string tag, string message)
//        {
//            if (CustomLogger != null)
//                CustomLogger.Log(LogLevel, tag, message);
//            else
//                Log.Info(tag, message);
//        }

//        public static void Warn(string tag, string message)
//        {
//            if (CustomLogger != null)
//                CustomLogger.Log(LogLevel, tag, message);
//            else
//                Log.Warn(tag, message);
//        }

//        public static void Error(string tag, string message)
//        {
//            if (CustomLogger != null)
//                CustomLogger.Log(LogLevel, tag, message);
//            else
//                Log.Error(tag, message);
//        }
//    }
//}
