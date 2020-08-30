using System;
using System.Collections.Generic;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;
using VkNet.Model;
using Energy.Plugin;
using Energy;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace TestBot
{
    class Program
    {
        private static VkApi vkApi = new VkApi();
        private static PluginManager Manager = new PluginManager();
        static void Main(string[] args)
        {         
            vkApi.Authorize(new ApiAuthParams()
            {
                ApplicationId = 7340430,//id приложения
                Settings = Settings.All,
                AccessToken = "49a3f0eee351cb3f1935ab293e9f208547aec28853ba4748c54d54cf3c1df7df939164f1bb1647b3bab4d",//ключ доступа группы
                UserId = 177103273,// id группы
            });
            Manager.Init();
            while (true)
            {
                var PoolServer = vkApi.Groups.GetLongPollServer(177103273);
                var PoolHistory = vkApi.Groups.GetBotsLongPollHistory(
                new BotsLongPollHistoryParams() { Server = PoolServer.Server, Ts = PoolServer.Ts, Key = PoolServer.Key, Wait = 25 });
                if (PoolHistory.Updates == null) continue;
                foreach (var a in PoolHistory.Updates)
                {
                    if (a.Type == GroupUpdateType.MessageNew)
                    {                  
                        ExecutePlugin(a.MessageNew.Message, a.MessageNew.Message.PeerId.Value);
                        PluginManager.Call("OnPrintConsole", new object[] { a.MessageNew.Message });
                        Console.WriteLine($"Сообщение \"{a.MessageNew.Message.Text}\" от {GetUserInfo(a.MessageNew.Message.PeerId.Value,a.MessageNew.Message.FromId)}");
                    }
                }
            }
        }
        public static string GetUserInfo(long PeerId, long? UserId)
        {
            try
            {
                foreach (var item in vkApi.Messages.GetConversationMembers(PeerId, new List<string>() { "LastName", "FirstName", "Id" }).Profiles)
                {

                    if (UserId == item.Id)
                    {
                        return string.Format($"{item.FirstName} {item.LastName} [id {item.Id}]");
                    }
                }
            }
            catch
            {
                Console.WriteLine($"error GetUserInfo {UserId.Value}");
            }
            return string.Empty;
        }
        public static void ExecutePlugin(Message message, long peedId)
        {
            foreach (var commandslist in PluginManager._pluginCommands)
            {
                foreach (var command in commandslist.Value)
                {
                    if (message.Text.Contains(command) && ((message.Text.Contains("бот") || message.Text.Contains("Бот"))))
                    {
                        PluginManager.GetPluginCommand(command).api = vkApi;
                        PluginManager.Call("OnExecute", command, new object[] { message, peedId });
                    }
                }

            }
        }
    }
}
