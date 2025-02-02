using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramEventBot.AppDb;
using TelegramEventBot.BotExceptions;
using TelegramEventBot.Enums;
using TelegramEventBot.Models;

namespace TelegramEventBot.BotStatics
{
    public static class BotMessageFactory
    {
        public delegate Task AsyncFunctionDelegate(Update update, TelegramBotClient botClient);

        private static DbRequest? _dbRequest;

        private static EventUserModel? _user;

        private static readonly ConcurrentDictionary<string, AsyncFunctionDelegate> _functions = new()
        {
            ["/start"] = SendStartMessageAsync,
            ["/ticket"] = SendTicketAsync,
            ["/makeUserAdmin"] = SendMakeUserAdminConfirmAsync,
            ["/cnt"] = SendCountOfPersonsMessageAsync,
            ["/regUserTicket"] = SendRegUserTicketMessageAsync,
            ["/removeUserAdmin"] = SendRemoveFromAdminMessageAsync,
        };

        private static readonly ConcurrentDictionary<Stage, AsyncFunctionDelegate> _stageActions = new()
        {
            [Stage.NameStage] = SendNameRequestMessageAsync,
            [Stage.AgeStage] = SendAgeRequestMessageAsync,
            [Stage.ContactStage] = SendContactRequestMessageAsync,
            [Stage.PaymentStage] = SendPaymentRequestMessageAsync,
            [Stage.TicketStage] = SendTicketWithMessageAsync,
            [Stage.DoneStage] = SendTicketAsync,
            [Stage.AdminStage] = SendCheckTicketMessageAsync,
            [Stage.NullStage] = SendOopsRequestMessageAsync,
        };

        public static async Task AcceptCommandAsync(Update update, TelegramBotClient botClient, AppDbContext appDbContext, string xToken, string accountSecret, int needToPay, int maxTickets)
        {
            _dbRequest = new(appDbContext, needToPay, accountSecret, xToken, maxTickets);

            _user = await _dbRequest.GetUserDataAsync(update);

            var regexVal = BotStaticHelper.IsMatchRegularExpressionAsync(update, _user);

            if (regexVal != RegexEnum.Null && regexVal != RegexEnum.Done && regexVal != RegexEnum.Ticket)
            {
                var status = await _dbRequest!.UpdateUserParamAsync(update, regexVal, _user);

                if (status == UpdateUserParamStatus.NotPaid)
                {
                    await SendNotPaidMessageAsync(update, botClient);
                }
                else
                {
                    await SendAdditionalInfoMessageAsync(update, botClient);
                }
                return;
            }
            else if (regexVal == RegexEnum.Ticket)
            {
                await SendTicketWithMessageAsync(update, botClient);

                return;
            }

            var isExist = ExceptionBotFactory.IsKeyExist(_functions, update.Message!.Text!);

            if (isExist)
            {
                if (update.Message.Text!.Contains(' '))
                {
                    var arr = update.Message.Text!.Split(" ");
                    var firstPart = arr[0];

                    var func = _functions[firstPart];

                    await func.Invoke(update, botClient);
                }
                else
                {
                    var func = _functions[update.Message.Text!];

                    await func.Invoke(update, botClient);
                }
            }
            else
            {
                await SendOopsRequestMessageAsync(update , botClient);
            }
        }

        private static async Task SendStartMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendStartMessageAsync(update, botClient);
            _user = await _dbRequest!.MakeBlankUserAsync(update);
            await SendAdditionalInfoMessageAsync(update, botClient);
        }
        private static async Task SendAdditionalInfoMessageAsync(Update update, TelegramBotClient botClient)
        {
            var result = BotStaticHelper.CheckStage(_user);

            var func = _stageActions[result];

            await func.Invoke(update, botClient);
        }
        private static async Task SendNameRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendNameRequestMessageAsync(update, botClient);
        }
        private static async Task SendAgeRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendAgeMessageAsync(update, botClient);
        }
        private static async Task SendContactRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendContactMessageAsync(update, botClient);
        }
        private static async Task SendPaymentRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            if (await _dbRequest!.CheckRemainingTicketsAsync())
            {
                await BotMessages.SendPaymentMessageAsync(update, botClient);
            }
            else
            {
                await BotMessages.SendNoTicketsAsync(update, botClient);
            }
        }
        private static async Task SendNotPaidMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendNotPaidMessageAsync(update, botClient);
        }
        private static async Task SendTicketWithMessageAsync(Update update, TelegramBotClient botClient)
        {
            var ticketId = await BotMessages.SendTicketMessageAsync(update, botClient, _user);
            await _dbRequest!.SaveTicketIdForUserAsync(ticketId, _user);
        }
        private static async Task SendTicketAsync(Update update, TelegramBotClient botClient)
        {
            var ticket = await _dbRequest!.GetUserTicketIdAsync(update);
            await BotMessages.SendTicketAsync(update, botClient, ticket);
        }
        private static async Task SendMakeUserAdminConfirmAsync(Update update, TelegramBotClient botClient)
        {
            var isSuccessful = await _dbRequest!.MakeUserAdminAsync(update, _user);

            if (isSuccessful)
            {
                await BotMessages.SendSuccessfulMakingAdminAsync(update, botClient);
            }
            else
            {
                await BotMessages.SendNotSuccessfulMakingAdminAsync(update, botClient);
            }
        }
        private static async Task SendCheckTicketMessageAsync(Update update, TelegramBotClient botClient)
        {
            var isValid = await _dbRequest!.IsTicketValid(update, _user);

            if (isValid.Item1)
            {
                await BotMessages.SendTicketAcceptedAsync(update, botClient, isValid.Item2);
            }
            else
            {
                await BotMessages.SendTicketNotAcceptedAsync(update, botClient, isValid.Item2);
            }
        }
        private static async Task SendCountOfPersonsMessageAsync(Update update, TelegramBotClient botClient)
        {
            var isAdmin = BotStaticHelper.IsAdmin(_user);

            if(isAdmin)
            {
                var count = await _dbRequest!.FillCountModelAsync();
                await BotMessages.SendCountOfPersonsMessageAsync(update, botClient, count);
            }
            else
            {
                await SendOopsRequestMessageAsync(update, botClient);
            }
        }
        private static async Task SendRegUserTicketMessageAsync(Update update, TelegramBotClient botClient)
        {
            var isAdmin = BotStaticHelper.IsAdmin(_user);

            if (isAdmin)
            {
                var user = await _dbRequest!.DeleteTicketByUserIdAsync(update);
                var ticketId = await BotMessages.SendTicketMessageAsync(update, botClient, user);
                await _dbRequest!.SaveTicketIdForUserAsync(ticketId, user);
            }
            else
            {
                await SendOopsRequestMessageAsync(update, botClient);
            }
        }
        private static async Task SendRemoveFromAdminMessageAsync(Update update, TelegramBotClient botClient)
        {
            var isSuccessful = await _dbRequest!.RemoveUserAdminAsync(update, _user);

            if (isSuccessful)
            {
                await BotMessages.SendUserRemovedFromAdminsAsync(update, botClient);
            }
            else
            {
                await BotMessages.SendNotSuccessfulMakingAdminAsync(update, botClient);
            }
        }
        private static async Task SendOopsRequestMessageAsync(Update update, TelegramBotClient botClient)
        {
            await BotMessages.SendOopsMessageAsync(update, botClient);

            var result = BotStaticHelper.CheckStage(_user);

            if (result != Stage.NullStage)
            {
                await SendAdditionalInfoMessageAsync(update, botClient);
            }
        }
    }
}
