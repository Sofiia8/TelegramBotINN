using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotINNConsole
{
    internal class TelegramMessageClient
    {
        private ITelegramBotClient _bot;
        private IConfiguration _config;
        internal TelegramMessageClient(ITelegramBotClient bot, IConfiguration config)
        {
            _bot = bot;
            _config = config;
        }
        internal async Task StartLongPolling()
        {
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message },
            };

            _bot.StartReceiving(handleUpdateAsync, handleErrorAsync, receiverOptions, cancellationToken);

            Console.WriteLine("Bot is running with long polling. Press any key to exit.");
            Console.ReadKey();

            cts.Cancel();
        }
        private async Task handleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if (message?.Type == MessageType.Text)
                    await handleTextMessageAsync(message);
            }
        }
        private async Task handleTextMessageAsync(Message message)
        {
            var text = message?.Text?.ToLower();
            if (text == null || text.Length == 0)
                return;
            string answer = "";
            switch (text)
            {
                case "/start":
                    answer = "You are welcome on board!\n";
                    break;
                case "/help":
                    answer = "Input /start - to start a dialog,\n" +
                            "Input /help - to show a list of avaliable commands,\n" +
                            "Input /hello - to show my name and surname, email and date of start,\n" +
                            "Input /inn - to get and show info about legal entities by their INN,\n" +
                            "Input /full - to get and show full info about legal entity by its INN.";
                    break;
                case "/hello":
                    answer = "My name is Bezukhova Sofiia,\nemail - sofi_92@mail.ru,\ndate of start - 06.11.2023";
                    break;
                case "/inn":
                    answer = "Please enter one or several INN(s) to get info about legal entities.\n" +
                             "Use format: <<INN_1>,<INN_2>,...,<INN_n>>,\nwhere <INN_i> - sequence of numbers like 1234567.";
                    break;
                case "/full":
                    answer = "Please enter one INN to get full info about legal entity.\n" +
                             "Use format: <#INN>,\nwhere INN - sequence of numbers like 1234567.";
                    break;
                default:
                    if (CheckRequest.CheckFormatMainInfo(text))
                    {
                        RequestTaxService requestTax = new RequestTaxService(_config);
                        answer = await requestTax.GetInfoByINNAsync(text);
                    }
                    else if (CheckRequest.CheckFormatFullInfo(text, out string output))
                    {
                        RequestTaxService requestTax = new RequestTaxService(_config);
                        answer = await requestTax.GetFullInfoByINNAsync(output);
                    }
                    else
                    {
                        answer = "Not correct request";
                    }
                    break;

            }
            if (answer.Length > GlobalConstants.chunkSize)
            {
                await splitTextToChunks(answer, message);
                return;
            }
            await _bot.SendTextMessageAsync(message.Chat, answer);
        }
        private async Task splitTextToChunks(string bigString, Message message)
        {
            int chunkSize = GlobalConstants.chunkSize;
            int totalChunks = (int)Math.Ceiling((double)bigString.Length / chunkSize);
            string chunk = "";

            for (int i = 0; i < totalChunks; i++)
            {
                int start = i * chunkSize;
                int length = Math.Min(chunkSize, bigString.Length - start);
                chunk = bigString.Substring(start, length);
                await _bot.SendTextMessageAsync(message.Chat, chunk);
            }
        }
        private static async Task handleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Debug.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}
