using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Hooks;
using TShockAPI;

namespace ExplosiveRod
{

    [APIVersion(1, 11)]
    public class ExplosiveRod: TerrariaPlugin
    {
        public static Dictionary<int, byte> cannonPlayers = new Dictionary<int, byte>();
        public override string Name
        {
            get { return "Explosive rod"; }
        }
        public override string Author
        {
            get { return "by InanZen"; }
        }
        public override string Description
        {
            get { return "Shoots Explosives from ice rod"; }
        }
        public override Version Version
        {
            get { return new Version("1.0"); }
        }
        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            GetDataHandlers.TileEdit += TileEdit;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                GetDataHandlers.TileEdit -= TileEdit;
            }
            base.Dispose(disposing);
        }
        public ExplosiveRod(Main game)
            : base(game)
        {
            Order = 5;
        }
        public void OnInitialize()
        {

            Commands.ChatCommands.Add(new Command("explosiverod", cannonCommand, "cannon"));
        }
        void TileEdit(Object sender, TShockAPI.GetDataHandlers.TileEditEventArgs args)
        {
            if (args.Type == 127 && cannonPlayers.Count > 0 && cannonPlayers.Keys.Contains(args.Player.UserID))
            {
                byte cannonType = cannonPlayers[args.Player.UserID];
                if (cannonType == 1)
                {
                    generateBoulder(args.X, args.Y);
                    updateTile(args.X, args.Y);
                }
                else if (cannonType == 2)
                {
                    generateExplosives(args.X, args.Y);
                    Main.tile[args.X + 1, args.Y].active = true;
                    Main.tile[args.X + 1, args.Y].wire = true;
                    updateTile(args.X, args.Y);
                    args.Handled = true;
                    WorldGen.TripWire(args.X + 1, args.Y);
                    Main.tile[args.X, args.Y].wire = false;
                    Main.tile[args.X+1, args.Y].wire = false;
                    updateTile(args.X, args.Y);
                }
            }
        }
        public static void updateTile(int x, int y)
        {
            x = Netplay.GetSectionX(x);
            y = Netplay.GetSectionY(y);
            foreach (Terraria.ServerSock theSock in Netplay.serverSock)
            {
                theSock.tileSection[x, y] = false;
            }
        }
        public void generateExplosives(int x, int y)
        {
            Main.tile[x, y].active = true;
            Main.tile[x, y].type = 141;
            Main.tile[x, y].frameX = 0;
            Main.tile[x, y].frameY = 18;
            Main.tile[x, y].wire = true;
        }
        public void generateBoulder(int x, int y)
        {
            Main.tile[x, y].active = true;
            Main.tile[x, y].type = 138;
            Main.tile[x, y].frameX = 0;
            Main.tile[x, y].frameY = 18;

            Main.tile[x + 1, y].active = true;
            Main.tile[x + 1, y].type = 138;
            Main.tile[x + 1, y].frameX = 18;
            Main.tile[x + 1, y].frameY = 18;

            Main.tile[x, y - 1].active = true;
            Main.tile[x, y - 1].type = 138;
            Main.tile[x, y - 1].frameX = 0;
            Main.tile[x, y - 1].frameY = 0;

            Main.tile[x + 1, y - 1].active = true;
            Main.tile[x + 1, y - 1].type = 138;
            Main.tile[x + 1, y - 1].frameX = 18;
            Main.tile[x + 1, y - 1].frameY = 0;
        }
        public void cannonCommand(CommandArgs args)
        {
            string cmd = "";
            if (args.Parameters.Count > 0)
            {
                cmd = args.Parameters[0].ToLower();
            }
            switch (cmd)
            {
                case "boulder":
                    {
                        if (cannonPlayers.Keys.Contains(args.Player.UserID))
                        {
                            cannonPlayers.Remove(args.Player.UserID);
                            args.Player.SendMessage("Boulder rod disabled", Color.Violet);
                        }
                        else
                        {
                            cannonPlayers.Add(args.Player.UserID, 1);
                            args.Player.SendMessage("Boulder rod enabled", Color.Violet);
                        }
                        break;
                    }
                case "explosives":
                    {
                        if (cannonPlayers.Keys.Contains(args.Player.UserID))
                        {
                            cannonPlayers.Remove(args.Player.UserID);
                            args.Player.SendMessage("Explosive rod disabled", Color.Violet);
                        }
                        else
                        {
                            cannonPlayers.Add(args.Player.UserID, 2);
                            args.Player.SendMessage("Explosive rod enabled", Color.Violet);
                        }
                        break;
                    }
                default:
                    {
                        args.Player.SendMessage("Available commands:", Color.Violet);
                        args.Player.SendMessage("/cannon explosives - toggles explosive cannon", Color.Violet);
                        args.Player.SendMessage("/cannon boulder - toggles boulder cannon", Color.Violet);
                        break;
                    }
            }
        }
    }
}
