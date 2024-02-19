using NLog;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Drawing.Text;
using System.IO;
using System.Windows.Controls;
using Torch;
using Torch.API;
using Torch.API.Plugins;
using Torch.Commands;
using Torch.Views;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using System;
using Sandbox.Engine.Multiplayer;
using VRage.Dedicated.RemoteAPI;
using Torch.API.Managers;
using Sandbox.Game.World;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Multiplayer;
using Torch.Session;
using Torch.API.Session;
using Newtonsoft.Json;
using VRage.Input;
using System.Xml.Linq;
using System.Xml;
using Sandbox.Game.GameSystems;
using SpaceEngineers.Game.ModAPI;
using System.Windows.Documents;
using VRageMath;
using VRage;

namespace NewPlayerProtection;
public class Plugin : TorchPluginBase, IWpfPlugin
{
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private Persistent<Config> _config = null!;
    private TorchSessionManager? _sessionManager;
    public Dictionary<string, string> idTimeMap = new Dictionary<string, string>();
    public override void Init(ITorchBase torch)
    {
        base.Init(torch);
        _config = Persistent<Config>.Load(Path.Combine(StoragePath, "NewPlayerProtection.cfg"));

        _sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
        if (_sessionManager != null)
            _sessionManager.SessionStateChanged += SessionChanged;
        else
            Log.Warn("No session manager.");
        //Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerBase>().PlayerJoined += playerloggedin;
        InitTimeDB();
    }

    private void InitTimeDB()
    {

        var filename = "config/NPPTime.xml";
        var currentDirectory = Directory.GetCurrentDirectory();
        var NPPTimeFile = Path.Combine(currentDirectory, filename);

        XElement NPPTime = XElement.Load(NPPTimeFile);

        var IDs = from item in NPPTime.Descendants("player")
                  select ((string)item.Attribute("ID"));
        foreach (var data in IDs)
        {
            //idTimeMap.Add(data[0, 1], data);

        }

        IEnumerable<XElement> address =
            from el in NPPTime.Elements("player")
            select el;
        foreach (XElement el in address)
        {
            var ID = el.Attribute("ID").ToString().Replace("ID=\"", "").Replace("\"", "");
            var Timestamp = el.Element("Timestamp").ToString().Replace("<Timestamp>", "").Replace("</Timestamp>", "");
            //Log.Info(ID + " / " + Timestamp);
            if (idTimeMap.ContainsKey(ID) == false)
            {
                idTimeMap.Add(ID, Timestamp);
            }
        }
    }

    private void SessionChanged(ITorchSession session, TorchSessionState state)
    {
        var mpMan = Torch.CurrentSession.Managers.GetManager<IMultiplayerManagerServer>();

        switch (state)
        {
            case TorchSessionState.Loading:
                break;

            case TorchSessionState.Loaded:
                mpMan.PlayerJoined += playerloggedin;
                //mpMan.PlayerJoined += AccModule.CheckIp;

                //mpMan.PlayerLeft += ResetMotdOnce;

                break;


            case TorchSessionState.Unloading:
                break;
        }
    }
    private void checkSZ()
    {

    }
    public UserControl GetControl() => new PropertyGrid
    {
        Margin = new(3),
        DataContext = _config.Data
    };
    int lastcheck = 0;
    List<MyCubeGrid> gridList = new List<MyCubeGrid>();

    public IMySafeZoneBlock? SZBlock { get; private set; }

    public override void Update()
    {

        if ((int)MySandboxGame.Static.SimulationFrameCounter >= lastcheck + 600)
        {
            lastcheck = (int)MySandboxGame.Static.SimulationFrameCounter;
            //Log.Info("Check");
            var players = MySession.Static.Players.GetOnlinePlayers();
            //Gets all grids in the World
            foreach (var entity in MyEntities.GetEntities())
            {
                MyCubeGrid? grid = entity as MyCubeGrid;
                if (grid == null || grid.Projector != null)
                    continue;
                //Gets Id of all Grid Majority Owners (Including NPC)
                for (int i = 0; i < grid.BigOwners.Count; i++)
                {
                    var owner = grid.BigOwners[i];
                    //checks if owner is not NPC
                    if (!MySession.Static.Players.IdentityIsNpc(owner))
                    {
                        //Player owned grid found
                        foreach (var identity in MySession.Static.Players.GetAllIdentities())
                        {
                            if (identity.IdentityId == owner)
                            {
                                //Log.Info("Grid is owned by: " + identity.DisplayName);
                                foreach (MyCubeBlock block in grid.GetFatBlocks())
                                {
                                    SZBlock = block as IMySafeZoneBlock;
                                    if (SZBlock != null)
                                    {
                                        //Log.Info("SafeZone found");
                                        var ownerSteamID = MySession.Static.Players.TryGetSteamId(owner);
                                        idTimeMap.TryGetValue(ownerSteamID.ToString(), out string ownerTS);
                                        Log.Info(ownerTS);
                                        long.TryParse(ownerTS, out long ownerTSNumber);
                                        long mathedTS = ownerTSNumber + 604800;
                                        if ((long)mathedTS >= DateTime.UtcNow.ToUnixTimestamp())
                                        {
                                            Log.Info("Player too old Deleting safezone owned by: " + identity.DisplayName);
                                            SZBlock.CustomName = "Test";
                                            MySlimBlock SZSlim = (MySlimBlock)SZBlock.SlimBlock;
                                            //grid.RemoveBlock(SZSlim);
                                            SZBlock.CubeGrid.RemoveBlock(SZSlim);
                                            break;

                                        }
                                    }
                                }

                            }
                        }

                    }
                }
                // All Grids searched
            }
        }
    }

    private void playerloggedin(IPlayer player)
    {
        var PID = Sync.Players.TryGetPlayerIdentity(player.SteamId);

        if (PID == null)
        {
            //Log.Info("New Player joined");
            //var PlayerInit = Sync.Players.InitNewPlayer;
            var STEAMID = player.SteamId;
            var filename = "config/NPPTime.xml";
            var currentDirectory = Directory.GetCurrentDirectory();
            var NPPTimeFile = Path.Combine(currentDirectory, filename);
            long Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            if (!File.Exists(filename))
            {
                using (XmlWriter writer = XmlWriter.Create("config/NPPTime.xml"))
                {
                    writer.WriteStartElement("NPP");
                    writer.WriteStartElement("player");
                    writer.WriteAttributeString("ID", STEAMID.ToString());
                    writer.WriteElementString("Timestamp", Timestamp.ToString());
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.Flush();
                    if (idTimeMap.ContainsKey(STEAMID.ToString()) == false)
                    {
                        idTimeMap.Add(STEAMID.ToString(), Timestamp.ToString());
                    }
                }
            }
            else
            {
                {
                    XDocument xDocument = XDocument.Load(NPPTimeFile);
                    XElement root = xDocument.Element("NPP");
                    IEnumerable<XElement> rows = root.Descendants("player");
                    XElement firstRow = rows.First();
                    firstRow.AddBeforeSelf(
                       new XElement("player",
                       new XAttribute("ID", STEAMID.ToString()),
                       new XElement("Timestamp", Timestamp.ToString())));
                    xDocument.Save(NPPTimeFile);
                    if (idTimeMap.ContainsKey(STEAMID.ToString()) == false)
                    {
                        idTimeMap.Add(STEAMID.ToString(), Timestamp.ToString());
                    }
                }

            }
        }
    }
}