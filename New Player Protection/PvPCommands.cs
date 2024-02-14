using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Views;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using NLog;
using System.Xml;
using System.Xml.Linq;
using System;
using VRage.Network;
using Sandbox.Game.Multiplayer;
using Torch.API.Managers;
using Torch.Session;
using VRage;

namespace NewPlayerProtection
{
    [Category("protection")]
    public class PvPCommands : CommandModule
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Command("time", "Shows how long you have left")]
        [Permission(MyPromoteLevel.None)]
        public void Time()
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            var tsData = new NewPlayerProtection.Plugin();
            tsData.idTimeMap.TryGetValue(Context.Player.SteamUserId.ToString(),out var joinTime);
            var currentTime = DateTime.UtcNow.ToUnixTimestamp();
            long.TryParse(joinTime + 604800, out long timeTillEnd);
            dateTime = dateTime.AddSeconds(timeTillEnd);
            TimeSpan timeLeft = dateTime - DateTime.Now; // At time of posting, it is 18/12/2019
            string printDate = dateTime.ToShortDateString() + " " + dateTime.ToShortTimeString();
            Log.Info("This is a Test ");
            if (timeLeft.Days > 0)
            {
                Context.Respond("Your protection ends in" + $"{timeLeft.Days} days");
            }
            else if (timeLeft.Days < 0 & timeLeft.Hours >= 1)
            {
                Context.Respond("Your protection Ends in" + $"{timeLeft.Hours} hours");
            }
            else if (timeLeft.Days <= 0 & timeLeft.Hours <= 0 & timeLeft.Minutes >= 1)
            {
                Context.Respond("Your protection Ends in" + $"{timeLeft.Minutes} minutes");
            }
            else if (timeLeft.Days <= 0 & timeLeft.Hours <= 0 & timeLeft.Minutes <= 0 & timeLeft.Seconds >= 1)
            {
                Context.Respond("Your protection Ends in" + $"{timeLeft.Minutes} minutes");
            }
            else
            {
                Context.Respond("Your protection has already ended");
            }
        }
        [Command("disable", "Disables PvP protection")]
        [Permission(MyPromoteLevel.None)]
        public void Disable(string? cmdArgs = null)
        {
            //ADD CONFIRMATION
            if (cmdArgs == "confirm")
            {
                Context.Respond("Your New Player Protection has been disabled");

                var filename = "config/NPPTime.xml";
                var currentDirectory = Directory.GetCurrentDirectory();
                var NPPTimeFile = Path.Combine(currentDirectory, filename);

                XElement NPPTime = XElement.Load(NPPTimeFile);

                IEnumerable<XElement> xmlData = from item in NPPTime.Descendants("player")
                                                where item.Attribute("ID").Value == Context.Player.SteamUserId.ToString()
                                                select item;
                foreach (var data in xmlData)
                {
                    data.SetElementValue("Timestamp","0");
                    var tsData = new NewPlayerProtection.Plugin();
                    tsData.idTimeMap.Remove(Context.Player.SteamUserId.ToString());
                    tsData.idTimeMap.Add(Context.Player.SteamUserId.ToString(), "0");

                }
                NPPTime.Save(NPPTimeFile);
            }
            else {
                Context.Respond("To disable the use of your New Player protection SafeZone early please type !protection disable confirm.");
                Context.Respond("WARNING: This is permanent and cannot be undone");
            }
        }

    }
}