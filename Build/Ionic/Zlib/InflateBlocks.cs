﻿using System;

namespace Ionic.Zlib
{
	// Token: 0x02000113 RID: 275
	internal sealed class InflateBlocks
	{
		// Token: 0x06000790 RID: 1936 RVA: 0x00041B44 File Offset: 0x0003FD44
		internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
		{
			this._codec = codec;
			this.hufts = new int[4320];
			this.window = new byte[w];
			this.end = w;
			this.checkfn = checkfn;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.Reset();
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00041BC4 File Offset: 0x0003FDC4
		internal uint Reset()
		{
			uint result = this.check;
			this.mode = InflateBlocks.InflateBlockMode.TYPE;
			this.bitk = 0;
			this.bitb = 0;
			this.writeAt = 0;
			this.readAt = 0;
			if (this.checkfn != null)
			{
				this._codec._Adler32 = (this.check = Adler.Adler32(0U, null, 0, 0));
			}
			return result;
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00041C24 File Offset: 0x0003FE24
		internal int Process(int r)
		{
			int num = this._codec.NextIn;
			int num2 = this._codec.AvailableBytesIn;
			int num3 = this.bitb;
			int i = this.bitk;
			int num4 = this.writeAt;
			int num5 = (num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4);
			int num6;
			for (;;)
			{
				switch (this.mode)
				{
				case InflateBlocks.InflateBlockMode.TYPE:
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_928;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num6 = (num3 & 7);
					this.last = (num6 & 1);
					switch ((uint)num6 >> 1)
					{
					case 0U:
						num3 >>= 3;
						i -= 3;
						num6 = (i & 7);
						num3 >>= num6;
						i -= num6;
						this.mode = InflateBlocks.InflateBlockMode.LENS;
						continue;
					case 1U:
					{
						int[] array = new int[1];
						int[] array2 = new int[1];
						int[][] array3 = new int[1][];
						int[][] array4 = new int[1][];
						InfTree.inflate_trees_fixed(array, array2, array3, array4, this._codec);
						this.codes.Init(array[0], array2[0], array3[0], 0, array4[0], 0);
						num3 >>= 3;
						i -= 3;
						this.mode = InflateBlocks.InflateBlockMode.CODES;
						continue;
					}
					case 2U:
						num3 >>= 3;
						i -= 3;
						this.mode = InflateBlocks.InflateBlockMode.TABLE;
						continue;
					case 3U:
						goto IL_8AD;
					default:
						continue;
					}
					break;
				case InflateBlocks.InflateBlockMode.LENS:
				{
					while (i < 32)
					{
						if (num2 == 0)
						{
							goto IL_9F1;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					if ((~num3 >> 16 & 65535) != (num3 & 65535))
					{
						goto Block_45;
					}
					this.left = (num3 & 65535);
					int num7 = 0;
					i = 0;
					num3 = num7;
					this.mode = ((this.left != 0) ? InflateBlocks.InflateBlockMode.STORED : ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE));
					continue;
				}
				case InflateBlocks.InflateBlockMode.STORED:
					if (num2 == 0)
					{
						goto IL_A47;
					}
					if (num5 == 0)
					{
						if (num4 == this.end && this.readAt != 0)
						{
							num4 = 0;
							num5 = ((0 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
						}
						if (num5 == 0)
						{
							this.writeAt = num4;
							r = this.Flush(r);
							num4 = this.writeAt;
							num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
							if (num4 == this.end && this.readAt != 0)
							{
								num4 = 0;
								num5 = ((0 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
							}
							if (num5 == 0)
							{
								goto IL_A9D;
							}
						}
					}
					r = 0;
					num6 = this.left;
					if (num6 > num2)
					{
						num6 = num2;
					}
					if (num6 > num5)
					{
						num6 = num5;
					}
					Array.Copy(this._codec.InputBuffer, num, this.window, num4, num6);
					num += num6;
					num2 -= num6;
					num4 += num6;
					num5 -= num6;
					if ((this.left -= num6) == 0)
					{
						this.mode = ((this.last != 0) ? InflateBlocks.InflateBlockMode.DRY : InflateBlocks.InflateBlockMode.TYPE);
						continue;
					}
					continue;
				case InflateBlocks.InflateBlockMode.TABLE:
					while (i < 14)
					{
						if (num2 == 0)
						{
							goto IL_B66;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num6 = (this.table = (num3 & 16383));
					if ((num6 & 31) <= 29 && (num6 >> 5 & 31) <= 29)
					{
						num6 = 258 + (num6 & 31) + (num6 >> 5 & 31);
						if (this.blens != null && this.blens.Length >= num6)
						{
							Array.Clear(this.blens, 0, num6);
						}
						else
						{
							this.blens = new int[num6];
						}
						num3 >>= 14;
						i -= 14;
						this.index = 0;
						this.mode = InflateBlocks.InflateBlockMode.BTREE;
						goto IL_40A;
					}
					goto IL_AF3;
				case InflateBlocks.InflateBlockMode.BTREE:
					goto IL_40A;
				case InflateBlocks.InflateBlockMode.DTREE:
					goto IL_2CC;
				case InflateBlocks.InflateBlockMode.CODES:
					goto IL_55;
				case InflateBlocks.InflateBlockMode.DRY:
					goto IL_E24;
				case InflateBlocks.InflateBlockMode.DONE:
					goto IL_ECA;
				case InflateBlocks.InflateBlockMode.BAD:
					goto IL_F23;
				}
				goto Block_51;
				for (;;)
				{
					IL_2CC:
					num6 = this.table;
					if (this.index >= 258 + (num6 & 31) + (num6 >> 5 & 31))
					{
						break;
					}
					num6 = this.bb[0];
					while (i < num6)
					{
						if (num2 == 0)
						{
							goto IL_D50;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					num6 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 1];
					int num8 = this.hufts[(this.tb[0] + (num3 & InternalInflateConstants.InflateMask[num6])) * 3 + 2];
					if (num8 < 16)
					{
						num3 >>= num6;
						i -= num6;
						this.blens[this.index++] = num8;
					}
					else
					{
						int num9 = (num8 == 18) ? 7 : (num8 - 14);
						int num10 = (num8 == 18) ? 11 : 3;
						while (i < num6 + num9)
						{
							if (num2 == 0)
							{
								goto IL_CFA;
							}
							r = 0;
							num2--;
							num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
							i += 8;
						}
						num3 >>= num6;
						i -= num6;
						num10 += (num3 & InternalInflateConstants.InflateMask[num9]);
						num3 >>= num9;
						i -= num9;
						num9 = this.index;
						num6 = this.table;
						if (num9 + num10 > 258 + (num6 & 31) + (num6 >> 5 & 31) || (num8 == 16 && num9 < 1))
						{
							goto IL_C80;
						}
						num8 = ((num8 == 16) ? this.blens[num9 - 1] : 0);
						do
						{
							this.blens[num9++] = num8;
						}
						while (--num10 != 0);
						this.index = num9;
					}
				}
				this.tb[0] = -1;
				int[] array5 = new int[]
				{
					9
				};
				int[] array6 = new int[]
				{
					6
				};
				int[] array7 = new int[1];
				int[] array8 = new int[1];
				num6 = this.table;
				num6 = this.inftree.inflate_trees_dynamic(257 + (num6 & 31), 1 + (num6 >> 5 & 31), this.blens, array5, array6, array7, array8, this.hufts, this._codec);
				if (num6 == 0)
				{
					this.codes.Init(array5[0], array6[0], this.hufts, array7[0], this.hufts, array8[0]);
					this.mode = InflateBlocks.InflateBlockMode.CODES;
					goto IL_55;
				}
				goto IL_DA6;
				IL_40A:
				while (this.index < 4 + (this.table >> 10))
				{
					while (i < 3)
					{
						if (num2 == 0)
						{
							goto IL_C2A;
						}
						r = 0;
						num2--;
						num3 |= (int)(this._codec.InputBuffer[num++] & byte.MaxValue) << i;
						i += 8;
					}
					this.blens[InflateBlocks.border[this.index++]] = (num3 & 7);
					num3 >>= 3;
					i -= 3;
				}
				while (this.index < 19)
				{
					this.blens[InflateBlocks.border[this.index++]] = 0;
				}
				this.bb[0] = 7;
				num6 = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this._codec);
				if (num6 == 0)
				{
					this.index = 0;
					this.mode = InflateBlocks.InflateBlockMode.DTREE;
					goto IL_2CC;
				}
				goto IL_BBC;
				IL_55:
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				r = this.codes.Process(this, r);
				if (r != 1)
				{
					break;
				}
				r = 0;
				num = this._codec.NextIn;
				num2 = this._codec.AvailableBytesIn;
				num3 = this.bitb;
				i = this.bitk;
				num4 = this.writeAt;
				num5 = ((num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4));
				if (this.last != 0)
				{
					goto IL_E1D;
				}
				this.mode = InflateBlocks.InflateBlockMode.TYPE;
			}
			return this.Flush(r);
			Block_45:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid stored block lengths";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-3);
			Block_51:
			r = -2;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-2);
			IL_8AD:
			num3 >>= 3;
			i -= 3;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid block type";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-3);
			IL_928:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_9F1:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_A47:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_A9D:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_AF3:
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "too many length or distance symbols";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-3);
			IL_B66:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_BBC:
			r = num6;
			if (r == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_C2A:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_C80:
			this.blens = null;
			this.mode = InflateBlocks.InflateBlockMode.BAD;
			this._codec.Message = "invalid bit length repeat";
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-3);
			IL_CFA:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_D50:
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_DA6:
			if (num6 == -3)
			{
				this.blens = null;
				this.mode = InflateBlocks.InflateBlockMode.BAD;
			}
			r = num6;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(r);
			IL_E1D:
			this.mode = InflateBlocks.InflateBlockMode.DRY;
			IL_E24:
			this.writeAt = num4;
			r = this.Flush(r);
			num4 = this.writeAt;
			int num11 = (num4 < this.readAt) ? (this.readAt - num4 - 1) : (this.end - num4);
			if (this.readAt != this.writeAt)
			{
				this.bitb = num3;
				this.bitk = i;
				this._codec.AvailableBytesIn = num2;
				this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
				this._codec.NextIn = num;
				this.writeAt = num4;
				return this.Flush(r);
			}
			this.mode = InflateBlocks.InflateBlockMode.DONE;
			IL_ECA:
			r = 1;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(1);
			IL_F23:
			r = -3;
			this.bitb = num3;
			this.bitk = i;
			this._codec.AvailableBytesIn = num2;
			this._codec.TotalBytesIn += (long)(num - this._codec.NextIn);
			this._codec.NextIn = num;
			this.writeAt = num4;
			return this.Flush(-3);
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x0000C873 File Offset: 0x0000AA73
		internal void Free()
		{
			this.Reset();
			this.window = null;
			this.hufts = null;
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x00042BB0 File Offset: 0x00040DB0
		internal void SetDictionary(byte[] d, int start, int n)
		{
			Array.Copy(d, start, this.window, 0, n);
			this.writeAt = n;
			this.readAt = n;
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x0000C88A File Offset: 0x0000AA8A
		internal int SyncPoint()
		{
			if (this.mode != InflateBlocks.InflateBlockMode.LENS)
			{
				return 0;
			}
			return 1;
		}

		// Token: 0x06000796 RID: 1942 RVA: 0x00042BDC File Offset: 0x00040DDC
		internal int Flush(int r)
		{
			for (int i = 0; i < 2; i++)
			{
				int num;
				if (i == 0)
				{
					num = ((this.readAt <= this.writeAt) ? this.writeAt : this.end) - this.readAt;
				}
				else
				{
					num = this.writeAt - this.readAt;
				}
				if (num == 0)
				{
					if (r == -5)
					{
						r = 0;
					}
					return r;
				}
				if (num > this._codec.AvailableBytesOut)
				{
					num = this._codec.AvailableBytesOut;
				}
				if (num != 0 && r == -5)
				{
					r = 0;
				}
				this._codec.AvailableBytesOut -= num;
				this._codec.TotalBytesOut += (long)num;
				if (this.checkfn != null)
				{
					this._codec._Adler32 = (this.check = Adler.Adler32(this.check, this.window, this.readAt, num));
				}
				Array.Copy(this.window, this.readAt, this._codec.OutputBuffer, this._codec.NextOut, num);
				this._codec.NextOut += num;
				this.readAt += num;
				if (this.readAt == this.end && i == 0)
				{
					this.readAt = 0;
					if (this.writeAt == this.end)
					{
						this.writeAt = 0;
					}
				}
				else
				{
					i++;
				}
			}
			return r;
		}

		// Token: 0x04000519 RID: 1305
		private const int MANY = 1440;

		// Token: 0x0400051A RID: 1306
		internal static readonly int[] border = new int[]
		{
			16,
			17,
			18,
			0,
			8,
			7,
			9,
			6,
			10,
			5,
			11,
			4,
			12,
			3,
			13,
			2,
			14,
			1,
			15
		};

		// Token: 0x0400051B RID: 1307
		private InflateBlocks.InflateBlockMode mode;

		// Token: 0x0400051C RID: 1308
		internal int left;

		// Token: 0x0400051D RID: 1309
		internal int table;

		// Token: 0x0400051E RID: 1310
		internal int index;

		// Token: 0x0400051F RID: 1311
		internal int[] blens;

		// Token: 0x04000520 RID: 1312
		internal int[] bb = new int[1];

		// Token: 0x04000521 RID: 1313
		internal int[] tb = new int[1];

		// Token: 0x04000522 RID: 1314
		internal InflateCodes codes = new InflateCodes();

		// Token: 0x04000523 RID: 1315
		internal int last;

		// Token: 0x04000524 RID: 1316
		internal ZlibCodec _codec;

		// Token: 0x04000525 RID: 1317
		internal int bitk;

		// Token: 0x04000526 RID: 1318
		internal int bitb;

		// Token: 0x04000527 RID: 1319
		internal int[] hufts;

		// Token: 0x04000528 RID: 1320
		internal byte[] window;

		// Token: 0x04000529 RID: 1321
		internal int end;

		// Token: 0x0400052A RID: 1322
		internal int readAt;

		// Token: 0x0400052B RID: 1323
		internal int writeAt;

		// Token: 0x0400052C RID: 1324
		internal object checkfn;

		// Token: 0x0400052D RID: 1325
		internal uint check;

		// Token: 0x0400052E RID: 1326
		internal InfTree inftree = new InfTree();

		// Token: 0x02000114 RID: 276
		private enum InflateBlockMode
		{
			// Token: 0x04000530 RID: 1328
			TYPE,
			// Token: 0x04000531 RID: 1329
			LENS,
			// Token: 0x04000532 RID: 1330
			STORED,
			// Token: 0x04000533 RID: 1331
			TABLE,
			// Token: 0x04000534 RID: 1332
			BTREE,
			// Token: 0x04000535 RID: 1333
			DTREE,
			// Token: 0x04000536 RID: 1334
			CODES,
			// Token: 0x04000537 RID: 1335
			DRY,
			// Token: 0x04000538 RID: 1336
			DONE,
			// Token: 0x04000539 RID: 1337
			BAD
		}
	}
}
