using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramEventBot.AppDb;
using TelegramEventBot.Dtos;
using TelegramEventBot.Enums;
using TelegramEventBot.Models;

namespace TelegramEventBot.BotStatics
{
    public static class BotStaticHelper
    {
        public static Stage CheckStage(EventUserModel? eventUser)
        {
            return eventUser switch
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

        public static RegexEnum IsMatchRegularExpressionAsync(Update update, EventUserModel? eventUser)
        {
            var result = CheckStage(eventUser);

            if (update.Message == null || update.Message.Text == null || update.Message.Contact != null)
            {
                return result switch
                {
                    Stage.TicketStage => RegexEnum.Ticket,
                    Stage.DoneStage => RegexEnum.Done,
                    Stage.ContactStage => RegexEnum.PhoneNum,
                    Stage.PaymentStage when update.CallbackQuery?.Data != null => RegexEnum.Payment,
                    _ => RegexEnum.Null
                };
            }

            return update.Message!.Text switch
            {
                _ when Regex.IsMatch(update.Message!.Text!, "^[А-Яа-яЇїІіЄєҐґA-Za-z'-]+\\s[А-Яа-яЇїІіЄєҐґA-Za-z'-]+$") && result == Stage.NameStage => RegexEnum.NameAndSurname,
                _ when Regex.IsMatch(update.Message!.Text!, "^\\d+$") && result == Stage.AgeStage => RegexEnum.Age,
                _ when Regex.IsMatch(update.Message!.Text!, "^\\/start\\s+([1-9]\\d*)$") && result == Stage.AdminStage => RegexEnum.Admin,
                _ => RegexEnum.Null
            };
        }

        public static async Task<string> GenerateQRAndSendItAsync(Update update, TelegramBotClient botClient, EventUserModel? user)
        {
            using var httpClient = new HttpClient();

            var byteArray = await httpClient.GetByteArrayAsync($"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=https://t.me/event_uniia_bot?start={user!.Id}");

            using var memoryStream = new MemoryStream(byteArray);

            var result = new InputFileStream(memoryStream);

            var sentMessage = await botClient.SendPhoto(chatId: update.CallbackQuery!.Message!.Chat.Id, photo: result);

            return sentMessage.Photo![^1].FileId;
        }
        public static bool IsAdmin(EventUserModel? user)
        {
            if (user == null || !user.IsAdmin) 
                return false;

            return true;
        }
    }
}
