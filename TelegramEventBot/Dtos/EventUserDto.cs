using TelegramEventBot.Models;

namespace TelegramEventBot.Dtos
{
    public class EventUserDto
    {
        public string Id { get; set; } = "Невідомо";
        public string TelegramId { get; set; } = "Невідомо";
        public string Name { get; set; } = "Невідомо";
        public string Age { get; set; } = "Невідомо";
        public string Username { get; set; } = "Невідомо";
        public string IsPaidMessage { get; set; } = "Невідомо";
        public string IsValidatedMessage { get; set; } = "Невідомо";
        public string PassedBy { get; set; } = "Невідомо";

        public EventUserDto(EventUserModel? eventUserModel) 
        {
            if (eventUserModel != null)
            {
                Id = eventUserModel.Id.ToString();
                TelegramId = eventUserModel.TelegramId.ToString();
                Name = eventUserModel.Name!;
                Age = eventUserModel.Age.ToString();
                Username = eventUserModel.Username! == "@" ? "Відсутній" : eventUserModel.Username!;
                IsPaidMessage = eventUserModel.IsPaid ? "💸 Сплачено" : "🙅‍♂ Не сплачено";

                if (!string.IsNullOrEmpty(eventUserModel.TicketId))
                {
                    IsValidatedMessage = eventUserModel.TicketId.EndsWith("VALIDATED]") ? "‼️ Вже на заході" : "Потрібно підтвердити";
                    var startIndex = eventUserModel.TicketId.IndexOf('[') + 1;
                    var endIndex = eventUserModel.TicketId.IndexOf('|');
                    PassedBy = eventUserModel.TicketId.Substring(startIndex, endIndex - startIndex);
                }
            }
        }
    }
}
