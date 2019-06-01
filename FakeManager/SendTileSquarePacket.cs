﻿#region Using
using System.IO;
using Terraria;
using Terraria.Net.Sockets;
#endregion
namespace FakeManager
{
    class SendTileSquarePacket
    {
        #region Send

        public static void Send(int PlayerIndex, int IgnoreIndex,
            int Size, int X, int Y, int Number5 = 0)
        {
            if (PlayerIndex == -1)
            {
                for (int i = 0; i < 256; i++)
                    Send(i, IgnoreIndex, Size, X, Y, Number5);
                return;
            }

            RemoteClient client = Netplay.Clients[PlayerIndex];
            if ((PlayerIndex == IgnoreIndex) || (client?.IsActive != true))
                return;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.BaseStream.Position = 2L;
                bw.Write((byte)PacketTypes.TileSendSquare);
                WriteTiles(bw, PlayerIndex, Size, X, Y, Number5);
                long position = bw.BaseStream.Position;
                bw.BaseStream.Position = 0L;
                bw.Write((short)position);
                bw.BaseStream.Position = position;
                byte[] data = ms.ToArray();
                client.Socket.AsyncSend(data, 0, data.Length,
                    new SocketSendCallback(client.ServerWriteCallBack), null);
            }
        }

        #endregion
        #region WriteTiles

        private static void WriteTiles(BinaryWriter BinaryWriter,
            int PlayerIndex, int Size, int X, int Y, int Number5 = 0)
        {
            if (Size < 0)
            {
                Size = 0;
            }
            if (X < Size)
            {
                X = Size;
            }
            if (X >= Main.maxTilesX + Size)
            {
                X = Main.maxTilesX - Size - 1;
            }
            if (Y < Size)
            {
                Y = Size;
            }
            if (Y >= Main.maxTilesY + Size)
            {
                Y = Main.maxTilesY - Size - 1;
            }
            if (Number5 == 0)
            {
                BinaryWriter.Write((ushort)(Size & 32767));
            }
            else
            {
                BinaryWriter.Write((ushort)((Size & 32767) | 32768));
                BinaryWriter.Write((byte)Number5);
            }
            BinaryWriter.Write((short)X);
            BinaryWriter.Write((short)Y);
            OTAPI.Tile.ITile[,] applied = FakeManager.GetApplied(PlayerIndex, X, Y, Size, Size);
            for (int num8 = X; num8 < X + Size; num8++)
            {
                for (int num9 = Y; num9 < Y + Size; num9++)
                {
                    BitsByte bb11 = 0;
                    BitsByte bb12 = 0;
                    byte value = 0;
                    byte value2 = 0;
                    OTAPI.Tile.ITile tile = applied[num8 - X, num9 - Y];
                    bb11[0] = tile.active();
                    bb11[2] = (tile.wall > 0);
                    bb11[3] = (tile.liquid > 0 && Main.netMode == 2);
                    bb11[4] = tile.wire();
                    bb11[5] = tile.halfBrick();
                    bb11[6] = tile.actuator();
                    bb11[7] = tile.inActive();
                    bb12[0] = tile.wire2();
                    bb12[1] = tile.wire3();
                    if (tile.active())
                    {
                        bb12[2] = true;
                        value = tile.color();
                    }
                    if (tile.wall > 0)
                    {
                        bb12[3] = true;
                        value2 = tile.wallColor();
                    }
                    bb12 += (byte)(tile.slope() << 4);
                    bb12[7] = tile.wire4();
                    BinaryWriter.Write(bb11);
                    BinaryWriter.Write(bb12);
                    if (tile.active())
                    {
                        BinaryWriter.Write(value);
                    }
                    if (tile.wall > 0)
                    {
                        BinaryWriter.Write(value2);
                    }
                    if (tile.active())
                    {
                        BinaryWriter.Write(tile.type);
                        if (Main.tileFrameImportant[(int)tile.type])
                        {
                            BinaryWriter.Write(tile.frameX);
                            BinaryWriter.Write(tile.frameY);
                        }
                    }
                    if (tile.wall > 0)
                    {
                        BinaryWriter.Write(tile.wall);
                    }
                    if (tile.liquid > 0 && Main.netMode == 2)
                    {
                        BinaryWriter.Write(tile.liquid);
                        BinaryWriter.Write(tile.liquidType());
                    }
                }
            }
        }

        #endregion
    }
}