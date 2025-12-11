using Newtonsoft.Json;

namespace OBase.Pazaryeri.DataAccess.Utilities
{
    public static class ExceptionServiceHelper
    {
        public static void ThrowExMessages<T>(this Exception ex, string entity, T obj = null) where T : class
        {
            string errMSG = "DB HATASI:";
            errMSG += string.Format("Tablo:\"{0}\" Message:\"{1}\"", entity, ex.ToString());

            if (ex.InnerException != null)
            {
                errMSG += string.Format("\r\nInner Exception: {0}", ex.InnerException.Message);
            }
            if (obj != null)
            {
                errMSG += string.Format("\r\nFailed Data: {0}",JsonConvert.SerializeObject(obj));
            }

            throw new Exception(errMSG);
        }
        public static void ThrowExMessages(this Exception ex, string entity)
        {
            ex.ThrowExMessages<string>(entity);
        }
    }
}