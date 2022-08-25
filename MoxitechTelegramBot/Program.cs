/*
 *  authors: Moxitech
 *      Telegram bot, powered with .NET 6.0 && C#
 *      Additional: MySql for user tables
 * 
 * **/
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ReceiverOptions = Telegram.Bot.Polling.ReceiverOptions;
using System.Data.OleDb;
using System.Data;

Dictionary<string, string> configs = new Dictionary<string, string>();
/**
 * token => tokenofbot
 * (optional)key => true //For tb keyboards
 * (optional)bdconstr => localdb... //BD connection string
 */
Console.WriteLine("\t\tHello in Moxitech Telegram Bots!");
pictureLogo();
Console.WriteLine("Setups...");
try
{   string tempStr;
    FileStream fs = new FileStream(path: "botconf.txt", FileMode.Open);
    StreamReader sr = new StreamReader(fs);
    #region loader
    tempStr = sr.ReadLine();
    if (tempStr.Substring(0,5) == "token")
    {
        tempStr = tempStr.Substring(9);
        configs.Add("token", tempStr);
        tempStr = "";
    }
    else if (tempStr.Substring(0,7) == "bdconstr"){
        tempStr = tempStr.Substring(11);
        configs.Add("dbconstr", tempStr);
        tempStr = "";
    }
    #endregion
    fs.Close();
    sr.Close();
    Console.Write('\n'+ "Токен бота (сокращено): " + configs["token"].ToString().Substring(0,4));
    Console.Write("...");
    Console.Write(configs["token"].ToString().Substring(configs["token"].Length - 5) + '\n');
}
catch (FileNotFoundException)
{
    Console.WriteLine("Файл конфигурации не найден!\nНовый файл botconf был создан в корневой папке с программой");
    FileStream fs = new FileStream(path: "botconf.txt", FileMode.Create);
    fs.Close();
}
catch (FieldAccessException)
{
    Console.BackgroundColor = ConsoleColor.Red;
    Console.WriteLine("Конфиг файл был запрещен//защищен для чтения, попробуйте изменить политику безопасности");
    Console.BackgroundColor = ConsoleColor.Black;
}

var bot = new TelegramBotClient(token: configs["token"]);
var cts = new CancellationTokenSource();
#region Установка Консоли после инициализации
Console.Title = "Moxitech Telegram Bot";
Console.BackgroundColor = ConsoleColor.Cyan;
Console.ForegroundColor = ConsoleColor.Black;
#endregion
Task stopAllBtn = new Task(() => ReadKeys(cts));
stopAllBtn.Start();

var receiverOption = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};
bot.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOption,
    cancellationToken: cts.Token
);

var me = await bot.GetMeAsync();
Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;
    var chatId = message.Chat.Id;
    var username = message.Chat.Username;
    
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}, Username {username}.");
    if (checkRoleFromDB(username) == 2)
    {
        Console.WriteLine(2);
        Message sentMessage = await botClient.SendTextMessageAsync(
         chatId: chatId,
         text: "Привет, администратор",
         cancellationToken: cancellationToken);
    }
    else
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "You said:\n" + messageText,
            cancellationToken: cancellationToken);
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };
    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

static Int16 checkRoleFromDB(string username)
{
    /*
     * Проверка Наличия пользователя в БД (Microsoft Access)
     * **/
    OleDbConnection oleDbConnection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\User\\Documents\\TGdb.accdb;Persist Security Info=True");
    string query = "SELECT UserName, role FROM userNameTable WHERE (role = 2)";
    //OleDbCommand command = new OleDbCommand(query, oleDbConnection);
    OleDbDataAdapter da = new OleDbDataAdapter(query, oleDbConnection);
    DataSet dataSet = new DataSet();
    oleDbConnection.OpenAsync();
    da.Fill(dataSet, "uname");

    foreach (DataTable table in dataSet.Tables)
    {
        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn column in table.Columns)
            {
                if (row[column].ToString() == username)
                {
                    oleDbConnection.CloseAsync();
                    return 2;
                }
                else
                {
                    continue;
                }
            }
        }
    }
    oleDbConnection.CloseAsync();
    return 0;
}

static void ReadKeys(CancellationTokenSource cts)
{
    /*  author: Moxitech 
     *  cts: Cancellation Token для остановки работы бота
     *  Обработчик события нажатия кнопки ВЫХОД на клавиатуре 
     * **/
    
    ConsoleKeyInfo key = new ConsoleKeyInfo();

    while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
    {

        key = Console.ReadKey(true);

        switch (key.Key)
        {
            case ConsoleKey.Escape:
                Console.WriteLine("ESC pressed");
                cts.Cancel();
                break;

            default:
                if (Console.CapsLock && Console.NumberLock)
                {
                    Console.WriteLine(key.KeyChar);
                }
                break;
        }
    } 
}

#region Лого в консоль Beta
void pictureLogo()
{
    Console.WriteLine("0001100000000011000");
    Console.WriteLine("0011011000001101100");
    Console.WriteLine("0011001100011001100");
    Console.WriteLine("0011000010100001100");
    Console.WriteLine("0011000001000001100");
    Console.WriteLine("0011000000000001100");
    Console.WriteLine("0011000000000001100");
    Console.WriteLine("0011000000000001100");
}
#endregion
//OK and work
//TODO : Messages
//TODO : Keyboard
