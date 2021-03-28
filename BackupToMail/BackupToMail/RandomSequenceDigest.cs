using System;
using System.Collections.Generic;

namespace BackupToMail
{
    public class RandomSequenceDigest : RandomSequence
    {
        public RandomSequenceDigest(int CacheStep_)
        {
            CacheStepB = CacheStep_;
            CacheStepV = (1L << CacheStepB) - 1L;
        }

        int CacheStepB;
        long CacheStepV;
        List<byte[]> CacheVals = new List<byte[]>();
        byte[] CacheItem;
        long CachePos;
        int PadTOffset = 0;
        const int DigestLength = 16;
        int DigestStateLength = 0;
        int PadL_Length = 0;
        int PadT_Length = 0;
        byte[] PadL;
        byte[] PadT;
        byte[] DigestState;

        public string Init(int CalcBits_, string HexPadL, string HexPadT)
        {
            PadL = new byte[HexPadL.Length / 2];
            PadL_Length = PadL.Length;
            for (int i = 0; i < PadL.Length; i++)
            {
                PadL[i] = (byte)MailSegment.HexToInt(HexPadL.Substring(i * 2, 2));
            }
            PadT = new byte[HexPadT.Length / 2];
            PadT_Length = PadT.Length;
            for (int i = 0; i < PadT.Length; i++)
            {
                PadT[i] = (byte)MailSegment.HexToInt(HexPadT.Substring(i * 2, 2));
            }
            PadTOffset = PadL_Length + DigestLength;

            CacheVals.Clear();
            DigestState = new byte[PadL_Length + PadT_Length];
            for (int i = 0; i < PadL.Length; i++)
            {
                DigestState[i] = PadL[i];
            }
            for (int i = 0; i < PadT_Length; i++)
            {
                DigestState[i + PadL_Length] = PadT[i];
            }
            CacheVals.Add(DigestState);
            DigestStateLength = PadL_Length + DigestLength + PadT_Length;
            return "";
        }

        public override byte[] GenSeq(long SeqStart, long SeqLen)
        {
            byte[] Raw = new byte[SeqLen];

            long SeqStartBlock = SeqStart / DigestLength;
            int SeqStartOffset = (int)(SeqStart - (SeqStartBlock * DigestLength));


            CachePos = SeqStartBlock >> CacheStepB;
            if (CachePos > (CacheVals.Count - 1))
            {
                CachePos = (CacheVals.Count - 1);
            }
            DigestState = CacheVals[(int)CachePos];
            CachePos = CachePos << CacheStepB;

            long FlushNum = SeqStartBlock - CachePos;
            while (FlushNum > 0)
            {
                GenValSeq();
                FlushNum--;
            }

            byte[] ValSeq = null;

            int SeqEnd = 0;
            if (SeqStartOffset > 0)
            {
                ValSeq = GenValSeq();
                SeqEnd = DigestLength - SeqStartOffset;
                if (((long)SeqEnd) > SeqLen)
                {
                    SeqEnd = (int)SeqLen;
                }
                for (int i = 0; i < SeqEnd; i++)
                {
                    Raw[i] = ValSeq[i + SeqStartOffset];
                }
            }

            int NumOfBlocks = ((int)SeqLen - SeqEnd) / DigestLength;

            if (NumOfBlocks > 0)
            {
                for (int i = 0; i < NumOfBlocks; i++)
                {
                    ValSeq = GenValSeq();
                    for (int ii = 0; ii < DigestLength; ii++)
                    {
                        Raw[SeqEnd] = ValSeq[ii];
                        SeqEnd++;
                    }
                }
            }

            int TrailingBlockSize = (int)SeqLen - SeqEnd;

            if (TrailingBlockSize > 0)
            {
                ValSeq = GenValSeq();
                for (int i = 0; i < TrailingBlockSize; i++)
                {
                    Raw[i + SeqEnd] = ValSeq[i];
                }
            }

            AddToStats(Raw);
            return Raw;
        }

        byte[] GenValSeq()
        {
            if ((CachePos & CacheStepV) == 0)
            {
                if (CacheVals.Count == (CachePos >> CacheStepB))
                {
                    CacheItem = new byte[DigestStateLength];
                    for (int i = 0; i < DigestStateLength; i++)
                    {
                        CacheItem[i] = DigestState[i];
                    }
                    CacheVals.Add(CacheItem);
                }
            }

            byte[] Seq = MailSegment.DigestBin_(DigestState);
            DigestState = new byte[DigestStateLength];
            for (int i = 0; i < PadL_Length; i++)
            {
                DigestState[i] = PadL[i];
            }
            for (int i = 0; i < DigestLength; i++)
            {
                DigestState[i + PadL_Length] = Seq[i];
            }
            for (int i = 0; i < PadT_Length; i++)
            {
                DigestState[i + PadTOffset] = PadT[i];
            }
            CachePos++;
            return Seq;
        }
    }
}