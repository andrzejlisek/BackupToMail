using System;
using System.Collections.Generic;

namespace BackupToMail
{
    public class RandomSequenceFib : RandomSequence
    {
        public RandomSequenceFib(int CacheStep_)
        {
            CacheStepB = CacheStep_;
            CacheStepV = (1L << CacheStepB) - 1L;
        }

        int CacheStepB;
        long CacheStepV;
        List<int[]> CacheVals = new List<int[]>();
        long CachePos;

        int CalcMod;
        int CalcP;
        int CalcQ;
        int CalcBits;

        int BufL;
        int BufL2;
        int BufP;
        int[] Buf;
        int[] CacheItem;


        public string Init(int CalcBits_, int CalcP_, int CalcQ_, int CalcMod_, int[] CalcNums)
        {
            CalcBits = CalcBits_;
            CalcP = CalcP_;
            CalcQ = CalcQ_;
            CalcMod = CalcMod_;

            if ((CalcBits != 1) && (CalcBits != 2) && (CalcBits != 4) && (CalcBits != 8))
            {
                return "Incorrect number of bits (" + CalcBits.ToString() + ")";
            }

            if ((CalcP < 1) || (CalcQ < 1) || (CalcMod < 2))
            {
                return "Incorrect constant values (A=" + CalcP.ToString() + ", B=" + CalcQ.ToString() + ", M=" + CalcMod.ToString() + ")";
            }


            BufL = Math.Max(CalcP, CalcQ);
            BufL2 = BufL + BufL;
            if (CalcNums.Length != BufL)
            {
                return "Incorrect initial vector size";
            }
            CacheItem = new int[BufL];
            for (int i = 0; i < BufL; i++)
            {
                CacheItem[i] = CalcNums[i];
            }


            BufP = BufL;
            Buf = new int[BufL2];

            CacheVals.Clear();
            CacheVals.Add(CacheItem);

            return "";
        }





        int GenVal()
        {
            if ((CachePos & CacheStepV) == 0)
            {
                if (CacheVals.Count == (CachePos >> CacheStepB))
                {
                    CacheItem = new int[BufL];
                    for (int i = 0; i < BufL; i++)
                    {
                        CacheItem[i] = Buf[BufP - BufL + i];
                    }
                    CacheVals.Add(CacheItem);
                }
            }

            int Val = Buf[BufP];

            int Temp = (Buf[BufP - CalcP] + Buf[BufP - CalcQ]) % CalcMod;
            Buf[BufP] = Temp;
            Buf[BufP - BufL] = Temp;

            BufP++;
            if (BufP == BufL2)
            {
                BufP = BufL;
            }

            CachePos++;

            return Val;
        }

        public override byte[] GenSeq(long SeqStart, long SeqLen)
        {
            byte[] Raw = new byte[SeqLen];
            switch (CalcBits)
            {
                case 1: SeqStart = SeqStart << 3; break;
                case 2: SeqStart = SeqStart << 2; break;
                case 4: SeqStart = SeqStart << 1; break;
            }

            CachePos = SeqStart >> CacheStepB;
            if (CachePos > (CacheVals.Count - 1))
            {
                CachePos = (CacheVals.Count - 1);
            }

            CacheItem = CacheVals[(int)CachePos];
            CachePos = CachePos << CacheStepB;
            BufP = BufL;
            for (int I = 0; I < BufL; I++)
            {
                Buf[I] = CacheItem[I];
                Buf[I + BufL] = CacheItem[I];
            }

            long FlushNum = SeqStart - CachePos;
            while (FlushNum > 0)
            {
                GenVal();
                FlushNum--;
            }

            int V1;
            int V2;
            int V3;
            int V4;
            int V5;
            int V6;
            int V7;
            int V8;
            switch (CalcBits)
            {
                case 1:
                    for (int i = 0; i < SeqLen; i++)
                    {
                        V1 = ((GenVal() & 1) << 7);
                        V2 = ((GenVal() & 1) << 6);
                        V3 = ((GenVal() & 1) << 5);
                        V4 = ((GenVal() & 1) << 4);
                        V5 = ((GenVal() & 1) << 3);
                        V6 = ((GenVal() & 1) << 2);
                        V7 = ((GenVal() & 1) << 1);
                        V8 = ((GenVal() & 1));
                        Raw[i] = (byte)(V1 + V2 + V3 + V4 + V5 + V6 + V7 + V8);
                    }
                    break;
                case 2:
                    for (int i = 0; i < SeqLen; i++)
                    {
                        V1 = ((GenVal() & 3) << 6);
                        V2 = ((GenVal() & 3) << 4);
                        V3 = ((GenVal() & 3) << 2);
                        V4 = ((GenVal() & 3));
                        Raw[i] = (byte)(V1 + V2 + V3 + V4);
                    }
                    break;
                case 4:
                    for (int i = 0; i < SeqLen; i++)
                    {
                        V1 = ((GenVal() & 15) << 4);
                        V2 = ((GenVal() & 15));
                        Raw[i] = (byte)(V1 + V2);
                    }
                    break;
                case 8:
                    for (int i = 0; i < SeqLen; i++)
                    {
                        V1 = ((GenVal() & 255));
                        Raw[i] = (byte)(V1);
                    }
                    break;
            }

            AddToStats(Raw);
            return Raw;
        }
    }
}
