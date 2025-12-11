namespace OBase.Pazaryeri.Business.Helper
{
    public static class DateTimeHelper
    {
        /// <summary>
        /// Nullable DateTime'i UTC DateTime olarak döndürür. Eğer null ise null döner.
        /// </summary>
        public static DateTime? ToUtcDateTime(this DateTime? localTime)
        {
            if (localTime == null)
                return null;

            DateTime dt = localTime.Value;
            if (dt.Kind == DateTimeKind.Unspecified)
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);

            return dt.ToUniversalTime();
        }
    }
}
