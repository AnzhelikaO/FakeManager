#region Using
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Terraria;
using Terraria.Net.Sockets;
#endregion
namespace FakeManager
{
    class SendSectionPacket
    {
        #region Send

        public static void Send(int PlayerIndex, int IgnoreIndex,
            int X, int Y, short Width, short Height)
        {
            if (PlayerIndex == -1)
            {
                for (int i = 0; i < 256; i++)
                    Send(i, IgnoreIndex, X, Y, Width, Height);
                return;
            }

            RemoteClient client = Netplay.Clients[PlayerIndex];
            if ((PlayerIndex == IgnoreIndex) || (client?.IsActive != true))
                return;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.BaseStream.Position = 2L;
                bw.Write((byte)PacketTypes.TileSendSection);
                byte[] array = new byte[260000];
                int count = CompressTileBlock(PlayerIndex, X, Y, Width, Height, array, 0);
                bw.Write(array, 0, count);
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
        #region CompressTileBlock

        private static int CompressTileBlock(int PlayerIndex,
            int XStart, int YStart, short Width, short Height,
            byte[] Buffer, int BufferStart)
        {
            if (XStart + Width > Main.maxTilesX)
                Width = (short)(Main.maxTilesX - XStart);
            if (YStart + Height > Main.maxTilesY)
                Height = (short)(Main.maxTilesY - YStart);
            int result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(XStart);
                    binaryWriter.Write(YStart);
                    binaryWriter.Write(Width);
                    binaryWriter.Write(Height);
                    CompressTileBlock_Inner(PlayerIndex, binaryWriter, XStart, YStart, (int)Width, (int)Height);
                    int num = Buffer.Length;
                    if ((long)BufferStart + memoryStream.Length > (long)num)
                    {
                        result = (int)((long)(num - BufferStart) + memoryStream.Length);
                    }
                    else
                    {
                        memoryStream.Position = 0L;
                        using (MemoryStream memoryStream2 = new MemoryStream())
                        {
                            using (DeflateStream deflateStream = new DeflateStream(memoryStream2, CompressionMode.Compress, true))
                            {
                                memoryStream.CopyTo(deflateStream);
                                deflateStream.Flush();
                                deflateStream.Close();
                                deflateStream.Dispose();
                            }
                            if (memoryStream.Length <= memoryStream2.Length)
                            {
                                memoryStream.Position = 0L;
                                Buffer[BufferStart] = 0;
                                BufferStart++;
                                memoryStream.Read(Buffer, BufferStart, (int)memoryStream.Length);
                                result = (int)memoryStream.Length + 1;
                            }
                            else
                            {
                                memoryStream2.Position = 0L;
                                Buffer[BufferStart] = 1;
                                BufferStart++;
                                memoryStream2.Read(Buffer, BufferStart, (int)memoryStream2.Length);
                                result = (int)memoryStream2.Length + 1;
                            }
                        }
                    }
                }
            }
            return result;
        }

        #endregion
        #region CompressTileBlock_Inner

        private static void CompressTileBlock_Inner(int PlayerIndex,
            BinaryWriter BinaryWriter, int XStart, int YStart, int Width, int Height)
        {
            short[] array = new short[1000];
            short[] array3 = new short[1000];
            short num = 0;
            short num3 = 0;
            short num4 = 0;
            int num5 = 0;
            int num6 = 0;
            byte b = 0;
            byte[] array4 = new byte[13];
            OTAPI.Tile.ITile tile = null;

            OTAPI.Tile.ITile[,] tiles =
                FakeManager.GetAppliedTiles(PlayerIndex, XStart, YStart, Width, Height);
            for (int i = YStart; i < YStart + Height; i++)
            {
                for (int j = XStart; j < XStart + Width; j++)
                {
                    OTAPI.Tile.ITile tile2 = tiles[j - XStart, i - YStart];
                    if (tile2.isTheSameAs(tile))
                    {
                        num4 += 1;
                    }
                    else
                    {
                        if (tile != null)
                        {
                            if (num4 > 0)
                            {
                                array4[num5] = (byte)(num4 & 255);
                                num5++;
                                if (num4 > 255)
                                {
                                    b |= 128;
                                    array4[num5] = (byte)(((int)num4 & 65280) >> 8);
                                    num5++;
                                }
                                else
                                {
                                    b |= 64;
                                }
                            }
                            array4[num6] = b;
                            BinaryWriter.Write(array4, num6, num5 - num6);
                            num4 = 0;
                        }
                        num5 = 3;
                        byte b3;
                        byte b2 = b = (b3 = 0);
                        if (tile2.active())
                        {
                            b |= 2;
                            array4[num5] = (byte)tile2.type;
                            num5++;
                            if (tile2.type > 255)
                            {
                                array4[num5] = (byte)(tile2.type >> 8);
                                num5++;
                                b |= 32;
                            }
                            if (Terraria.ID.TileID.Sets.BasicChest[(int)tile2.type] && tile2.frameX % 36 == 0 && tile2.frameY % 36 == 0)
                            {
                                short num7 = (short)Chest.FindChest(j, i);
                                if (num7 != -1)
                                {
                                    array[(int)num] = num7;
                                    num += 1;
                                }
                            }
                            if (tile2.type == 88 && tile2.frameX % 54 == 0 && tile2.frameY % 36 == 0)
                            {
                                short num8 = (short)Chest.FindChest(j, i);
                                if (num8 != -1)
                                {
                                    array[(int)num] = num8;
                                    num += 1;
                                }
                            }
                            if (tile2.type == 378 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
                            {
                                int num15 = Terraria.GameContent.Tile_Entities.TETrainingDummy.Find(j, i);
                                if (num15 != -1)
                                {
                                    short[] array8 = array3;
                                    short num16 = num3;
                                    num3 = (short)(num16 + 1);
                                    array8[(int)num16] = (short)num15;
                                }
                            }
                            if (tile2.type == 395 && tile2.frameX % 36 == 0 && tile2.frameY == 0)
                            {
                                int num17 = Terraria.GameContent.Tile_Entities.TEItemFrame.Find(j, i);
                                if (num17 != -1)
                                {
                                    short[] array9 = array3;
                                    short num18 = num3;
                                    num3 = (short)(num18 + 1);
                                    array9[(int)num18] = (short)num17;
                                }
                            }
                            if (Main.tileFrameImportant[(int)tile2.type])
                            {
                                array4[num5] = (byte)(tile2.frameX & 255);
                                num5++;
                                array4[num5] = (byte)(((int)tile2.frameX & 65280) >> 8);
                                num5++;
                                array4[num5] = (byte)(tile2.frameY & 255);
                                num5++;
                                array4[num5] = (byte)(((int)tile2.frameY & 65280) >> 8);
                                num5++;
                            }
                            if (tile2.color() != 0)
                            {
                                b3 |= 8;
                                array4[num5] = tile2.color();
                                num5++;
                            }
                        }
                        if (tile2.wall != 0)
                        {
                            b |= 4;
                            array4[num5] = tile2.wall;
                            num5++;
                            if (tile2.wallColor() != 0)
                            {
                                b3 |= 16;
                                array4[num5] = tile2.wallColor();
                                num5++;
                            }
                        }
                        if (tile2.liquid != 0)
                        {
                            if (tile2.lava())
                            {
                                b |= 16;
                            }
                            else if (tile2.honey())
                            {
                                b |= 24;
                            }
                            else
                            {
                                b |= 8;
                            }
                            array4[num5] = tile2.liquid;
                            num5++;
                        }
                        if (tile2.wire())
                        {
                            b2 |= 2;
                        }
                        if (tile2.wire2())
                        {
                            b2 |= 4;
                        }
                        if (tile2.wire3())
                        {
                            b2 |= 8;
                        }
                        int num19;
                        if (tile2.halfBrick())
                        {
                            num19 = 16;
                        }
                        else if (tile2.slope() != 0)
                        {
                            num19 = (int)(tile2.slope() + 1) << 4;
                        }
                        else
                        {
                            num19 = 0;
                        }
                        b2 |= (byte)num19;
                        if (tile2.actuator())
                        {
                            b3 |= 2;
                        }
                        if (tile2.inActive())
                        {
                            b3 |= 4;
                        }
                        if (tile2.wire4())
                        {
                            b3 |= 32;
                        }
                        num6 = 2;
                        if (b3 != 0)
                        {
                            b2 |= 1;
                            array4[num6] = b3;
                            num6--;
                        }
                        if (b2 != 0)
                        {
                            b |= 1;
                            array4[num6] = b2;
                            num6--;
                        }
                        tile = tile2;
                    }
                }
            }
            if (num4 > 0)
            {
                array4[num5] = (byte)(num4 & 255);
                num5++;
                if (num4 > 255)
                {
                    b |= 128;
                    array4[num5] = (byte)(((int)num4 & 65280) >> 8);
                    num5++;
                }
                else
                {
                    b |= 64;
                }
            }
            array4[num6] = b;
            BinaryWriter.Write(array4, num6, num5 - num6);
            BinaryWriter.Write(num);
            for (int k = 0; k < (int)num; k++)
            {
                Chest chest = Main.chest[(int)array[k]];
                BinaryWriter.Write(array[k]);
                BinaryWriter.Write((short)chest.x);
                BinaryWriter.Write((short)chest.y);
                BinaryWriter.Write(chest.name);
            }

            Dictionary<int, Sign> signs =
                FakeManager.GetAppliedSigns(PlayerIndex, XStart, YStart, Width, Height);
            BinaryWriter.Write((short)signs.Count);
            foreach (KeyValuePair<int, Sign> pair in signs)
            {
                Sign sign = pair.Value;
                BinaryWriter.Write((short)pair.Key);
                BinaryWriter.Write((short)sign.x);
                BinaryWriter.Write((short)sign.y);
                BinaryWriter.Write(sign.text);
            }

            BinaryWriter.Write(num3);
            for (int m = 0; m < (int)num3; m++)
            {
                Terraria.DataStructures.TileEntity.Write(BinaryWriter, Terraria.DataStructures.TileEntity.ByID[(int)array3[m]], false);
            }
        }

        #endregion
    }
}