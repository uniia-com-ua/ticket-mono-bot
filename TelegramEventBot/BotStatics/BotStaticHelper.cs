using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramEventBot.AppDb;
using TelegramEventBot.Enums;

namespace TelegramEventBot.BotStatics
{
    public static class BotStaticHelper
    {
        public static async Task<Stage> CheckStageAsync(Update update, DbRequest dbRequest)
        {
            var user = await dbRequest.GetUserDataAsync(update);

            return user switch
            {
                null => Stage.NullStage,
                { IsAdmin: true } => Stage.AdminStage,
                { Name: null or "" } => Stage.NameStage,
                { Age: 0 } => Stage.AgeStage,
                { PhoneNumber: null or "" } => Stage.ContactStage,
                { IsPaid: false } => Stage.PaymentStage,
                { TicketId: null or "" } => Stage.TicketStage,
                _ => Stage.DoneStage,
            };
        }

        public static async Task<RegexEnum> IsMatchRegularExpressionAsync(Update update, DbRequest dbRequest)
        {
            var result = await CheckStageAsync(update, dbRequest);

            if (result == Stage.DoneStage)
            {
                return RegexEnum.Done;
            }
            if (result == Stage.PaymentStage && update.Message == null && update.CallbackQuery!.Data != null)
            {
                return RegexEnum.Payment;
            }
            if (result == Stage.ContactStage && update.Message!.Contact != null)
            {
                return RegexEnum.PhoneNum;
            }

            return update.Message!.Text switch
            {
                _ when Regex.IsMatch(update.Message!.Text!, "^[А-Яа-яЇїІіЄєҐґA-Za-z'-]+\\s[А-Яа-яЇїІіЄєҐґA-Za-z'-]+$") && result == Stage.NameStage => RegexEnum.NameAndSurname,
                _ when Regex.IsMatch(update.Message!.Text!, "^\\d+$") && result == Stage.AgeStage => RegexEnum.Age,
                _ when Regex.IsMatch(update.Message!.Text!, "^\\/start\\s+([1-9]\\d*)$") && result == Stage.AdminStage => RegexEnum.Admin,
                _ when result == Stage.TicketStage => RegexEnum.Ticket,
                _ when result == Stage.DoneStage => RegexEnum.Done,
                _ => RegexEnum.Null
            };
        }

        public static async Task<string> GenerateQRAndSendItAsync(Update update, DbRequest dbRequest, TelegramBotClient botClient)
        {
            var user = await dbRequest.GetUserDataAsync(update);

            using var httpClient = new HttpClient();

            var byteArray = await httpClient.GetByteArrayAsync($"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=https://t.me/event_uniia_bot?start={user!.Id}");

            using var memoryStream = new MemoryStream(byteArray);

            var result = new InputFileStream(memoryStream);

            var sentMessage = await botClient.SendPhoto(chatId: update.CallbackQuery!.Message!.Chat.Id, photo: result);

            return sentMessage.Photo![sentMessage.Photo.Length - 1].FileId;
        }
    }
}
