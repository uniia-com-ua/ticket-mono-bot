using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TelegramEventBot.BotStatics.BotMessageFactory;

namespace TelegramEventBot.BotExceptions
{
    public static class ExceptionBotFactory
    {
        public static bool IsKeyExist(ConcurrentDictionary<string, AsyncFunctionDelegate> functions, string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            if (key.Contains(" "))
            {
                var arr = key.Split(" ");
                var firstPart = arr[0];

                return functions.GetValueOrDefault(firstPart) != null;
            }

            return functions.GetValueOrDefault(key) != null;
        }
    }
}
