namespace TelegramEventBot.Models
{
    public class EventUserModel
    {
        public int Id { get; set; }
        public long TelegramId { get; set; }
        public int Age { get; set; }
        public bool IsPaid { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? TicketId { get; set; }
        public bool IsAdmin { get; set; }
        public void SaveFormatPhoneNumber(string phoneNumber)
        {
            string formattedPhone = phoneNumber.StartsWith('+') ? phoneNumber[1..] : phoneNumber;

            if (formattedPhone.StartsWith("38"))
            {
                string countryCode = formattedPhone[..2];
                string operatorCode = formattedPhone.Substring(2, 3);
                string firstPart = formattedPhone.Substring(5, 3);
                string secondPart = formattedPhone.Substring(8, 2);
                string thirdPart = formattedPhone.Substring(10, 2);

                PhoneNumber = $"+{countryCode} ({operatorCode}) {firstPart} {secondPart} {thirdPart}";
            }

            PhoneNumber = phoneNumber;
        }
    }
}
