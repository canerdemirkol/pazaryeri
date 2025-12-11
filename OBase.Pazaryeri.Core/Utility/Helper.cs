namespace OBase.Pazaryeri.Core.Utility
{
    public static class Helper
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return $"{char.ToUpper(input[0])}{input[1..]}";
        }

        public static string TrCharToEn(this string text)
        {
            if (text is null) return text;

            char[] turkishChars = { 'ı', 'ğ', 'İ', 'Ğ', 'ç', 'Ç', 'ş', 'Ş', 'ö', 'Ö', 'ü', 'Ü' };
            char[] englishChars = { 'i', 'g', 'I', 'G', 'c', 'C', 's', 'S', 'o', 'O', 'u', 'U' };

            // Match chars
            for (int i = 0; i < turkishChars.Length; i++)
                text = text.Replace(turkishChars[i], englishChars[i]);

            return text;
        }
      public static string ValidateCallRequest(string qpOrderNumber, string pickerPhoneNumber)
		{
			string message = string.Empty;
			if (string.IsNullOrEmpty(qpOrderNumber))
			{
				message += "'Sipariş numarası boş olamaz.'";
			}
			if (pickerPhoneNumber.Trim().Length != 10)
			{
				message += "'Lütfen telefon numarasını 10 hane olacak şekilde giriniz.'";
			}
			return message;
		}
		public static string ConvertCallResponseMessageToTurkish(string message = "")
		{
			message = message switch
			{
				string a when a.Contains("seller.reached.max.call.count") => "Satıcı maksimum arama sayısına ulaşıldı.",
				string b when b.Contains("seller.id.not.match.packagePicked") => "İstekte gönderilen sellerId ile package içerisindeki seller aynı değil.",
				string c when c.Contains("instant.order.customer.phone.not.found") => "Paketteki teslimat adresi (shipment address) içerisindeki telefon numarası boş.",
				string d when d.Contains("verimor.bridge.call.error") => "Gsm arama hatası/picker’ın telefonuna ulaşılamıyor.",
				string e when e.Contains("seller.authorization.error") => " Token bilgisi yok.",
				string f when f.Contains("order.number.not.found") => " Sipariş mevcut değil/iptal edilmiş.",
				string g when g.Contains("seller.authorization.error") => " Token bilgisi hatalı.",
				string h when h.Contains("instant.status.not.suitable.for.call") => "Siparişin statüsü müşteriyi aramak için uygun değil.",
				_ => message
			};
			return message;
		}

		public static int ToInt(this string Txt, int DefaultVal = 0)
		{
			return int.TryParse(Txt, out int res) ? res : DefaultVal;
		}

		public static bool EnumChecker<T>(object val) where T : Enum
		{
			return Enum.IsDefined(typeof(T), val);
        }
    }
}
