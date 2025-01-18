using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramEventBot.AppDb;

namespace TelegramEventBot.BotStatics
{
    public static class BotMessages
    {
        public static async Task SendStartMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Привіт! \r\n\r\nЯ бот квартирнику «Нуль два нуль два», і я буду твоїм особистим помічником! ✨\r\n\r\n" +
                            $"Хочу тобі трішки розказати за мету нашого заходу! " +
                            $"Перш за все, ми хочемо створити місце, щоб волонтери, активісти та всі, кому не байдуже, змогли відпочити, поспілкуватися і наповнитися енергією та натхненням. " +
                            $"Послухати музику українських виконавців, купити прикрасу ручної робота, та випити запашного чаю🎤🎸\r\n\r\nАле найголовніше - це зібрати кошти для майстерні FPV дронів «Серафим». \r\n\r\n" +
                            $"Тримай посилання на їх інстаграм: https://www.instagram.com/seraphym_drone?igsh=ZWxzNDh5bDJlM3lj\r\n\r\n" +
                            $"Завдяки квартирнику, ти зможеш подарувати нашим захисникам такі необхідні для них дрони! Хіба не чудово!  \r\n\r\n" +
                            $"Тепер трішки розкажи про себе!");
        }
        public static async Task SendNameRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Напиши своє ім’я та прізвище \r\n");
        }
        public static async Task SendOopsMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Схоже дані введенні невірно 🤔");
        }
        public static async Task SendContactMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: "Щоб надати номер телефону, надішли свій контакт кнопкою нижче 👇",
                            replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("Поділитися контактом")
                            {
                                RequestContact = true
                            })
                            {
                                ResizeKeyboard = true,
                                OneTimeKeyboard = true
                            });
        }
        public static async Task SendAgeMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Введи будь ласка свій вік");
        }
        public static async Task SendPaymentMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Приємно познайомитись!☺️👋\r\n\r\n" +
                            $"А тепер варто зробити подарунок військовим, і оплатити квиток на квартирник! \r\n\r\n" +
                            $"Ціна: донат від 500 грн \r\n\r\n" +
                            $"Перейди за цим посиланням:" +
                            $"https://send.monobank.ua/jar/bdHEqjRyu?t=Квиток%20№{update.Message!.From!.Id}" +
                            $"\r\n\r\nПісля проведення оплати натисни на кнопку нижче" +
                            $"\r\n\r\n❗️Увага! Не передавай це посилання у такому вигляді іншим та не змінюй коментар при поповненні, якщо хочеш отримати квиток, дякуємо!",
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Підтвердити оплату", "accept_payment")
                                }
                            }));
        }
        public static async Task SendNotPaidMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.CallbackQuery!.From.Id,
                            text: $"Упс, оплата не прийшла, зачекай хвилинку, якщо не зміниться статус, спробуй ще раз!");
        }
        public static async Task SendNoTicketsAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Квитків більше не залишилось, вибачте(");
        }
        public static async Task<string> SendTicketMessageAsync(Update update, TelegramBotClient botClient, DbRequest _dbRequest)
        {
            _=await botClient.SendMessage(
                            chatId: update.CallbackQuery!.Message!.Chat.Id,
                            text: $"Ваш Квиток !!! В майбутньому, ви зможете його отримувати за командою /ticket",
                            replyMarkup: new ReplyKeyboardMarkup(new KeyboardButton("/ticket"))
                            {
                                ResizeKeyboard = true,
                            });

            var fileId = await BotStaticHelper.GenerateQRAndSendItAsync(update, _dbRequest!, botClient);

            return fileId;
        }
        public static async Task SendTicketAcceptedAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Квиток валідний");
        }
        public static async Task SendTicketNotAcceptedAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"На жаль, квиток не валідний");
        }
        public static async Task SendSuccessfulMakingAdminAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Користувач став адміністратором!");
        }
        public static async Task SendNotSuccessfulMakingAdminAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: $"Користувач не був знайдений або некоректний введення команди");
        }
        public static async Task SendTicketAsync(Update update, TelegramBotClient botClient, string fileId)
        {
            await botClient.SendPhoto(
                            chatId: update.Message!.Chat.Id,
                            photo: new InputFileId(fileId));
        }
        public static async Task SendThanksMessageAsync(Update update, TelegramBotClient botClient)
        {
            await botClient.SendMessage(
                            chatId: update.Message!.Chat.Id,
                            text: "Твою присутність відмічено! 💙",
                            replyMarkup: new ReplyKeyboardRemove
                            {
                                Selective = false
                            });

            await botClient.SendSticker(
                            chatId: update.Message.Chat.Id,
                            sticker: new InputFileId("CAACAgIAAxkBAUbjnGcBlfjmcrhqRTKN6u0bHHlCHB-6AAJAAQACVp29CmzpW0AsSdYlNgQ"));
        }
    }
}
