using Telegram.Bot;
using TelegramBotINNConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

IConfiguration configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
ITelegramBotClient telegramClient = new TelegramBotClient(configuration["TelegramBotTokenKey"] ?? "");
var telegram = new TelegramMessageClient(telegramClient, configuration);
await telegram.StartLongPolling();
