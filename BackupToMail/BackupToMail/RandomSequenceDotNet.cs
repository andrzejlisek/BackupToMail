using System;
using System.Security.Cryptography;

namespace BackupToMail
{
    public class RandomSequenceDotNet : RandomSequence
    {
        public RandomSequenceDotNet(int CacheStep_)
        {
            DummyStep = (1L << CacheStep_);
        }

        long DummyStep;
        byte[] DummyBuf;
        byte[] DummySeq;
        Random Random_;
        RNGCryptoServiceProvider RNGCryptoServiceProvider_;
        int Seed;
        long CachePos;
        bool CryptoSecure;

        public string Init(int Seed_, bool CryptoSecure_)
        {
            CryptoSecure = CryptoSecure_;
            CachePos = 0;
            Seed = Seed_;
            if (CryptoSecure)
            {
                RNGCryptoServiceProvider_ = new RNGCryptoServiceProvider();
            }
            else
            {
                Random_ = new Random(Seed);
            }
            DummyBuf = new byte[DummyStep];
            return "";
        }

        public override byte[] GenSeq(long SeqStart, long SeqLen)
        {
            byte[] Raw = new byte[SeqLen];

            if (CryptoSecure)
            {
                RNGCryptoServiceProvider_.GetBytes(Raw);
            }
            else
            {
                if (SeqStart < CachePos)
                {
                    Random_ = new Random(Seed);
                    CachePos = 0;
                }

                int TestLosowanie = 0;
                while (CachePos < SeqStart)
                {
                    if ((SeqStart - CachePos) < DummyStep)
                    {
                        DummySeq = new byte[SeqStart - CachePos];
                        Random_.NextBytes(DummySeq);
                        TestLosowanie += DummySeq.Length;
                        CachePos = SeqStart;
                    }
                    else
                    {
                        Random_.NextBytes(DummyBuf);
                        TestLosowanie += DummyBuf.Length;
                        CachePos += DummyStep;
                    }
                }

                Random_.NextBytes(Raw);
                TestLosowanie += Raw.Length;
                CachePos += SeqLen;
            }

            AddToStats(Raw);
            return Raw;
        }
    }
}
