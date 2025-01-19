﻿using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Telegram.Bot.Types;
using TelegramEventBot.Dtos;
using TelegramEventBot.Enums;
using TelegramEventBot.Models;

namespace TelegramEventBot.AppDb
{
    public class DbRequest
    {
        private readonly AppDbContext _db;
        private readonly int _needToPay;
        private readonly string _accountSecret;
        private readonly string _xToken;
        private readonly int _maxTickets;

        public DbRequest(AppDbContext db, int needToPay, string accountSecret, string xToken, int maxTickets)
        {
            _db = db;
            _needToPay=needToPay;
            _accountSecret=accountSecret;
            _xToken=xToken;
            _maxTickets=maxTickets;
        }

        public async Task MakeBlankUserAsync(Update update)
        {
            var checkedUser = await _db.EventUsers.FirstOrDefaultAsync(u => u.TelegramId == update.Message!.From!.Id);

            if(checkedUser == null)
            {
                var IsUserAdmin = update.Message!.From!.Id == 636936065;

                EventUserModel user = new()
                {
                    Username = "@" + update.Message!.Chat.Username,
                    TelegramId = update.Message!.From!.Id,
                    IsAdmin = IsUserAdmin,
                };

                await _db.EventUsers.AddAsync(user);

                await _db.SaveChangesAsync();
            }
        }
        public async Task<bool> CheckRemainingTicketsAsync()
        {
            var usedTickets = await _db.EventUsers.Where(u => u.IsPaid).CountAsync();

            if (_maxTickets - usedTickets == 0)
            {
                return false;
            }
            return true;
        }

        public async Task<UpdateUserParamStatus> UpdateUserParamAsync(Update update, RegexEnum regexEnum)
        {
            var checkedUser = await GetUserDataAsync(update);

            if (checkedUser == null)
            {
                return UpdateUserParamStatus.NotFound;
            }

            if (regexEnum == RegexEnum.NameAndSurname)
            {
                checkedUser.Name = update.Message!.Text;

                _db.EventUsers.Update(checkedUser);

                await _db.SaveChangesAsync();
            }
            else if (regexEnum == RegexEnum.Age)
            {
                checkedUser.Age = int.Parse(update.Message!.Text!);

                _db.EventUsers.Update(checkedUser);

                await _db.SaveChangesAsync();
            }
            else if (regexEnum == RegexEnum.PhoneNum)
            {
                checkedUser.SaveFormatPhoneNumber(update.Message!.Contact!.PhoneNumber);

                _db.EventUsers.Update(checkedUser);

                await _db.SaveChangesAsync();
            }
            else if (regexEnum == RegexEnum.Payment)
            {
                using var httpClient = new HttpClient();

                var from = DateTimeOffset.Now.AddDays(-28).ToUnixTimeSeconds();

                httpClient.DefaultRequestHeaders.Add("X-Token", _xToken);
                var result = await httpClient.GetStringAsync($"https://api.monobank.ua/personal/statement/{_accountSecret}/{from}/");

                var transactions = JsonSerializer.Deserialize<List<Transaction>>(result);

                if (transactions == null)
                {
                    return UpdateUserParamStatus.NotPaid;
                }

                var searchedId = checkedUser.TelegramId.ToString();

                var amountSum = transactions.Where(x => x.Comment!.Contains(searchedId)).Select(t => t.Amount).Sum();

                if (amountSum < _needToPay)
                {
                    return UpdateUserParamStatus.NotPaid;
                }

                checkedUser.IsPaid = true;

                _db.EventUsers.Update(checkedUser);

                await _db.SaveChangesAsync();

                return UpdateUserParamStatus.PaymentSuccessful;
            }
            return UpdateUserParamStatus.OK;
        }

        public async Task<EventUserModel?> GetUserDataAsync(Update update)
        {
            if (update.Message != null)
            {
                var user = await _db.EventUsers.FirstOrDefaultAsync(u => u.TelegramId == update.Message!.From!.Id);
                return user;
            }
            else if (update.CallbackQuery != null)
            {
                var user = await _db.EventUsers.FirstOrDefaultAsync(u => u.TelegramId == update.CallbackQuery!.From!.Id);
                return user;
            }
            return null;
        }

        public async Task<string> GetUserTicketIdAsync(Update update)
        {
            var user = await _db.EventUsers.FirstOrDefaultAsync(u => u.TelegramId == update.Message!.From!.Id);

            return user!.TicketId!;
        }
        public async Task<(bool, EventUserDto)> IsTicketValid(Update update)
        {
            var userIdStr = update.Message!.Text!.Split(" ");

            if (userIdStr[1] == null)
            {
                return (false, new EventUserDto(null));
            }

            int userId = int.Parse(userIdStr[1]);

            var user = await _db.EventUsers.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && !string.IsNullOrEmpty(user.TicketId) && !user.TicketId.EndsWith("[VALIDATED]")) 
            {
                var eventUserDto = new EventUserDto(user);

                user.TicketId += "[VALIDATED]";

                _db.Update(user);

                await _db.SaveChangesAsync();

                return (true, eventUserDto);
            }

            return (false, new EventUserDto(user));
        }

        public async Task<bool> MakeUserAdminAsync(Update update)
        {
            var currentUser = await GetUserDataAsync(update);

            if (currentUser != null && !currentUser.IsAdmin)
            {
                return false;
            }

            var userIdStr = update.Message!.Text!.Split(" ");

            if (userIdStr[1] == null)
            {
                return false;
            }

            var user = await _db.EventUsers.FirstOrDefaultAsync(u => u.Username == userIdStr[1]);

            if (user == null)
            {
                int.TryParse(userIdStr[1], out var userId);

                user = await _db.EventUsers.FirstOrDefaultAsync(u => u.Id == userId);
            }

            if (user != null)
            {
                user.IsAdmin = true;

                _db.EventUsers.Update(user);

                await _db.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task SaveTicketIdForUserAsync(Update update, string ticketId)
        {
            var user = await GetUserDataAsync(update);

            user!.TicketId = ticketId;

            _db.EventUsers.Update(user);

            await _db.SaveChangesAsync();
        }
    }
}
