using Serilog;

namespace OBase.Pazaryeri.Business.LogHelper
{
    public class Logger
    {
        public static void Debug(string messageTemplate, string fileName = null, params object[] propertyValues)
        {
            Log.ForContext("LogFolderName", fileName, true).Debug(messageTemplate, propertyValues);
        }
        public static void Information(string messageTemplate, string fileName = null, params object[] propertyValues)
        {
            Log.ForContext("LogFolderName", fileName, true).Information(messageTemplate, propertyValues);
        }
        public static void Warning(string messageTemplate, string fileName = null, params object[] propertyValues)
        {
            Log.ForContext("LogFolderName", fileName, true).Warning(messageTemplate, propertyValues);
        }
        public static void Error(string messageTemplate, string fileName = null, params object[] propertyValues)
        {
            Log.ForContext("LogFolderName", fileName, true).Error(messageTemplate, propertyValues);
        }
    }
}
