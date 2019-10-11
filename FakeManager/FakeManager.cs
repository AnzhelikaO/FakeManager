#region Using
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
#endregion
namespace FakeManager
{
    [ApiVersion(2, 1)]
    public class FakeManager : TerrariaPlugin
    {
        #region Data

        public override string Name => "FakeManager";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "Anzhelika & ASgo";
        public override string Description => "Plugin for creating zones with fake tiles and signs.";

        public static FakeCollection Common { get; } = new FakeCollection();
        //public static FakeCollection[] Personal = new FakeCollection[Main.maxPlayers];
        internal static int[] AllPlayers;
        internal static Func<RemoteClient, byte[], int, int, bool> NetSendBytes;

        #endregion

        #region Constructor

        public FakeManager(Main game)
            : base(game)
        {
            Order = -1001;
        }

        #endregion
        #region Initialize

        public override void Initialize()
        {
            NetSendBytes = (Func<RemoteClient, byte[], int, int, bool>)Delegate.CreateDelegate(
                typeof(Func<RemoteClient, byte[], int, int, bool>),
                ServerApi.Hooks,
                ServerApi.Hooks.GetType().GetMethod("InvokeNetSendBytes", BindingFlags.NonPublic | BindingFlags.Instance));

            AllPlayers = new int[Main.maxPlayers];
            for (int i = 0; i < Main.maxPlayers; i++)
                AllPlayers[i] = i;

            ServerApi.Hooks.NetSendData.Register(this, OnSendData, 1000000);
        }

        #endregion
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetSendData.Deregister(this, OnSendData);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region OnSendData

        private void OnSendData(SendDataEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.MsgId)
            {
                case PacketTypes.TileSendSection:
                    args.Handled = true;
                    if (args.text?._text?.Length > 0)
                        SendSectionPacket.Send(args.text._text.Select(c => (int)c), args.ignoreClient,
                            args.number, (int)args.number2, (short)args.number3, (short)args.number4);
                    else
                        SendSectionPacket.Send(args.remoteClient, args.ignoreClient,
                            args.number, (int)args.number2, (short)args.number3, (short)args.number4);
                    break;
                case PacketTypes.TileSendSquare:
                    args.Handled = true;
                    if (args.text?._text?.Length > 0)
                        SendTileSquarePacket.Send(args.text._text.Select(c => (int)c), args.ignoreClient,
                            args.number, (int)args.number2, (int)args.number3, args.number5);
                    else
                        SendTileSquarePacket.Send(args.remoteClient, args.ignoreClient,
                            args.number, (int)args.number2, (int)args.number3, args.number5);
                    break;
            }
        }

        #endregion

        #region GetAppliedTiles

        public static ITile[,] GetAppliedTiles(int X, int Y, int Width, int Height)
        {
            ITile[,] tiles = new ITile[Width, Height];
            int X2 = (X + Width), Y2 = (Y + Height);
            for (int i = X; i < X2; i++)
                for (int j = Y; j < Y2; j++)
                    tiles[i - X, j - Y] = Main.tile[i, j];

            for (int i = 0; i < Common.Order.Count; i++)
            {
                FakeTileRectangle fake = Common.Data[Common.Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplyTiles(tiles, X, Y);
            }
            /*
            for (int i = 0; i < Personal[Who].Order.Count; i++)
            {
                FakeTileRectangle fake = Personal[Who].Data[Personal[Who].Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplyTiles(tiles, X, Y);
            }
            */
            return tiles;
        }

        #endregion
        #region GetAppliedSigns

        public static Dictionary<int, Sign> GetAppliedSigns(int X, int Y, int Width, int Height)
        {
            Dictionary<int, Sign> signs = new Dictionary<int, Sign>();
            int X2 = (X + Width), Y2 = (Y + Height);
            for (int i = 0; i < Main.sign.Length; i++)
            {
                Sign sign = Main.sign[i];
                if ((sign != null) && (sign.x >= X) && (sign.x < X2)
                        && (sign.y >= Y) && (sign.y < Y2))
                    signs.Add(i, sign);
            }

            for (int i = 0; i < Common.Order.Count; i++)
            {
                FakeTileRectangle fake = Common.Data[Common.Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplySigns(signs, X, Y, Width, Height);
            }
            /*
            for (int i = 0; i < Personal[Who].Order.Count; i++)
            {
                FakeTileRectangle fake = Personal[Who].Data[Personal[Who].Order[i]];
                if (fake.Enabled && fake.IsIntersecting(X, Y, Width, Height))
                    fake.ApplySigns(signs, X, Y, Width, Height);
            }
            */
            return signs;
        }

        #endregion
    }
}