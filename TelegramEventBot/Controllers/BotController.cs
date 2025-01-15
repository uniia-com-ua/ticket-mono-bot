using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramEventBot.AppDb;
using TelegramEventBot.BotStatics;

namespace TelegramEventBot.Controllers
{
    [ApiController]
    [Route("/telegram-event-bot")]
    public class BotController : ControllerBase
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _botToken;
        private readonly ILogger<BotController> _logger;
        private readonly AppDbContext _db;
        private readonly int _needToPay;
        private readonly int _maxTickets;
        private readonly string _accountSecret;
        private readonly string _xToken;

        public BotController(ILogger<BotController> logger, IConfiguration configuration, AppDbContext db)
        {
            _botToken = configuration["TelegramBotToken"]!;
            _xToken = configuration["xToken"]!;
            _accountSecret = configuration["AccountSecret"]!;
            _needToPay = 30000;
            _maxTickets = 200;
            _botClient = new TelegramBotClient(_botToken);
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Update update)
        {
            try
            {
                await BotMessageFactory.AcceptCommandAsync(update, _botClient, _db, _xToken, _accountSecret, _needToPay, _maxTickets);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unpredictable exception");

                return Ok();
            }
        }
    }
}
