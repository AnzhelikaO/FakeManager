#region Using
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using Terraria;
#endregion
namespace FakeManager
{
    public class FakeCollection
    {
        #region Data

        public Dictionary<object, FakeTileRectangle> Data = new Dictionary<object, FakeTileRectangle>();
        // The more is index in Order, the higher in hierarchy fake is.
        public bool IsPersonal = false;
        // Anzh: Why did we have order as a public list?
        // We should probaly completely remake it, since it's not only for UI now.
        internal List<object> Order = new List<object>();
        // Anzh: Should we still use locker, or maybe use concurrent shit?
        private object Locker = new object();

        #endregion
        public FakeCollection(bool IsPersonal = false) =>
            this.IsPersonal = IsPersonal;

        #region Add

        public FakeTileRectangle Add(object Key, int X, int Y,
            int Width, int Height, ITileCollection Tile = null)
        {
            lock (Locker)
            {
                if (Data.ContainsKey(Key))
                    throw new ArgumentException($"Key '{Key}' is already in use.");
                FakeTileRectangle fake = new FakeTileRectangle(this, X, Y, Width, Height, Tile);
                Data.Add(Key, fake);
                Order.Add(Key);
                return fake;
            }
        }

        #endregion
        #region Remove

        public bool Remove(object Key, bool Cleanup = true)
        {
            lock (Locker)
            {
                if (!Data.ContainsKey(Key))
                    return false;
                FakeTileRectangle o = Data[Key];
                Data.Remove(Key);
                Order.Remove(Key);
                int x = o.X, y = o.Y;
                int w = (o.X + o.Width - 1), h = (o.Y + o.Height - 1);
                int sx1 = Netplay.GetSectionX(x), sy1 = Netplay.GetSectionY(y);
                int sx2 = Netplay.GetSectionX(w), sy2 = Netplay.GetSectionY(h);
                o.Tile.Dispose();
                if (Cleanup)
                    GC.Collect();
                NetMessage.SendData((int)PacketTypes.TileSendSection,
                    -1, -1, null, x, y, w, h);
                NetMessage.SendData((int)PacketTypes.TileFrameSection,
                    -1, -1, null, sx1, sy1, sx2, sy2);
                return true;
            }
        }

        #endregion
        #region Resize

        public FakeTileRectangle Resize(object Key, int Width, int Height, FakeTileProvider Tile = null)
        {
            if (!Data.ContainsKey(Key))
                throw new KeyNotFoundException(Key.ToString());
            lock (Locker)
            {
                int x = Data[Key].X, y = Data[Key].Y;
                Remove(Key);
                return Add(Key, x, y, Width, Height, Tile);
            }
        }

        #endregion
        #region Clear

        public void Clear()
        {
            lock (Locker)
            {
                List<object> keys = new List<object>(Data.Keys);
                foreach (object key in keys)
                    Remove(key, false);
                GC.Collect();
            }
        }

        #endregion

        #region SetTop

        public void SetTop(object key)
        {
            lock (Locker)
            {
                if (!Order.Remove(key))
                    throw new KeyNotFoundException(key.ToString());
                Order.Add(key);
            }
        }

        #endregion
    }
}