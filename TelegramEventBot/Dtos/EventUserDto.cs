using TelegramEventBot.Models;

namespace TelegramEventBot.Dtos
{
    public class EventUserDto
    {
        public string Id { get; set; }
        public string TelegramId { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Username { get; set; }
        public string IsPaidMessage { get; set; }
        public string IsValidatedMessage { get; set; }

        public EventUserDto(EventUserModel? eventUserModel) 
        {
            if (eventUserModel == null)
            {
                Id = TelegramId = Name = Age = Username = IsPaidMessage = IsValidatedMessage = "Невідомо";
            }
            else
            {
                Id = eventUserModel.Id.ToString();
                TelegramId = eventUserModel.TelegramId.ToString();
                Name = eventUserModel.Name!;
                Age = eventUserModel.Age.ToString();
                Username = eventUserModel.Username! == "@" ? "Відсутній" : eventUserModel.Username!;
                IsPaidMessage = eventUserModel.IsPaid ? "💸 Сплачено" : "🙅‍♂ Не сплачено";
                IsValidatedMessage = eventUserModel.TicketId!.EndsWith("[VALIDATED]") ? "‼️ Вже на заході" : "Потрібно підтвердити";
            }
        }
    }
}
