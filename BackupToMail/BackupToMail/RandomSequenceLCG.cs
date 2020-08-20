using System;
using System.Collections.Generic;

namespace BackupToMail
{
    public class RandomSequenceLCG : RandomSequence
    {
        public RandomSequenceLCG(int CacheStep_)
        {
            CacheStepB = CacheStep_;
            CacheStepV = (1L << CacheStepB) - 1L;
        }

        int CacheStepB;
        long CacheStepV;
        List<int> CacheVals = new List<int>();
        long CachePos;

        int LCG_State;
        int LCG_A;
        int LCG_B;
        int LCG_M;
        int CalcBits;

        public string Init(int CalcBits_, int LCG_A_, int LCG_B_, int LCG_M_, int LCG_State_)
        {
            CalcBits = CalcBits_;
            LCG_State = LCG_State_;
            LCG_A = LCG_A_;
            LCG_B = LCG_B_;
            LCG_M = LCG_M_;

            if ((CalcBits != 1) && (CalcBits != 2) && (CalcBits != 4) && (CalcBits != 8))
            {
                return "Incorrect number of bits";
            }

            if ((LCG_A < 0) || (LCG_B < 0) || (LCG_M < 2))
            {
                return "Incorrect constant values (A=" + LCG_A.ToString() + ", B=" + LCG_B.ToString() + ", M=" + LCG_M.ToString() + ")";
            }

            CacheVals.Clear();
            CacheVals.Add(LCG_State);

            return "";
        }

        int GenVal()
        {
            if ((CachePos & CacheStepV) == 0)
            {
                if (CacheVals.Count == (CachePos >> CacheStepB))
                {
                    CacheVals.Add(LCG_State);
                }
            }

            LCG_State = (LCG_A * LCG_State + LCG_B) % LCG_M;

            CachePos++;

            return LCG_State;
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

            LCG_State = CacheVals[(int)CachePos];
            CachePos = CachePos << CacheStepB;

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
            return Raw;
        }
    }
}
