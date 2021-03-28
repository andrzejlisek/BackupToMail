using System;
namespace BackupToMail
{
    public class RandomSequence
    {
        public virtual byte[] GenSeq(long SeqStart, long SeqLen)
        {
            return null;
        }

        public ulong[] Stats = null;

        bool StatsEnabled = false;

        public void StatsReset(bool StatsEnabled_)
        {
            StatsEnabled = StatsEnabled_;
            if (Stats == null)
            {
                Stats = new ulong[256];
            }
            for (int i = 0; i < 256; i++)
            {
                Stats[i] = 0;
            }
        }

        public static long DummyFileSize = 0;
        public static string ErrorMsg = "";
        public static RandomSequence CreateRS(string Params, int RandomCacheStep)
        {
            string[] DummyFileParamsS = Params.Split(',');
            int[] DummyFileParamsI = new int[DummyFileParamsS.Length - 1];
            for (int i = 0; i < (DummyFileParamsS.Length - 1); i++)
            {
                try
                {
                    if ((i == 0) || (DummyFileParamsI[0] != 2))
                    {
                        DummyFileParamsI[i] = int.Parse(DummyFileParamsS[i + 1]);
                    }
                    else
                    {
                        DummyFileParamsI[i] = 0;
                        string StrHex = "";
                        for (int ii = 0; ii < DummyFileParamsS[i + 1].Length; ii++)
                        {
                            string C = DummyFileParamsS[i + 1][ii].ToString().ToUpperInvariant();
                            if ("0123456789ABCDEF".Contains(C))
                            {
                                StrHex = StrHex + C;
                            }
                            else
                            {
                                ErrorMsg = "Dummy file definition error - the sequence " + DummyFileParamsS[i + 1] + " contains invalid characters";
                                return null;
                            }
                        }
                        if ((StrHex.Length % 2) == 1)
                        {
                            DummyFileParamsS[i + 1] = "0" + StrHex;
                            ErrorMsg = "Dummy file definition error - length of the sequence " + DummyFileParamsS[i + 1] + " must be multiply of 2";
                            return null;
                        }
                        DummyFileParamsS[i + 1] = StrHex;
                    }
                }
                catch
                {
                    ErrorMsg = "Dummy file definition error - cannot convert " + DummyFileParamsS[i] + " to integer";
                    return null;
                }
            }

            try
            {
                RandomSequence.DummyFileSize = long.Parse(DummyFileParamsS[0]);
            }
            catch
            {
                ErrorMsg = "Dummy file size error - cannot convert " + DummyFileParamsS[0] + " to integer";
                return null;
            }

            RandomSequence _ = null;
            if (DummyFileParamsI[0] == 0)
            {
                if (DummyFileParamsI.Length != 6)
                {
                    ErrorMsg = "Linear congruential generator requires exactly five parameters";
                    return null;
                }
                _ = new RandomSequenceLCG(RandomCacheStep);
                ErrorMsg = ((RandomSequenceLCG)_).Init(DummyFileParamsI[1], DummyFileParamsI[2], DummyFileParamsI[3], DummyFileParamsI[4], DummyFileParamsI[5]);
            }
            if (DummyFileParamsI[0] == 1)
            {
                if (DummyFileParamsI.Length < 6)
                {
                    ErrorMsg = "Fibonacci generator requires at least five parameters";
                    return null;
                }
                _ = new RandomSequenceFib(RandomCacheStep);
                int[] DummyFileParamsVec = new int[DummyFileParamsI.Length - 5];
                for (int i = 0; i < DummyFileParamsVec.Length; i++)
                {
                    DummyFileParamsVec[i] = DummyFileParamsI[i + 5];
                }
                ErrorMsg = ((RandomSequenceFib)_).Init(DummyFileParamsI[1], DummyFileParamsI[2], DummyFileParamsI[3], DummyFileParamsI[4], DummyFileParamsVec);
            }
            if (DummyFileParamsI[0] == 2)
            {
                if (DummyFileParamsI.Length != 3)
                {
                    ErrorMsg = "Digest generator requires exactly two parameters";
                    return null;
                }
                _ = new RandomSequenceDigest(RandomCacheStep);
                ErrorMsg = ((RandomSequenceDigest)_).Init(DummyFileParamsI[1], DummyFileParamsS[2], DummyFileParamsS[3]);
            }
            if (DummyFileParamsI[0] == 3)
            {
                if ((DummyFileParamsI.Length != 1) && (DummyFileParamsI.Length != 2))
                {
                    ErrorMsg = ".NET internal generator requires no parameters or exactly one parameter";
                    return null;
                }
                _ = new RandomSequenceDotNet(RandomCacheStep);
                if (DummyFileParamsI.Length == 1)
                {
                    ErrorMsg = ((RandomSequenceDotNet)_).Init(0, true);
                }
                if (DummyFileParamsI.Length == 2)
                {
                    ErrorMsg = ((RandomSequenceDotNet)_).Init(DummyFileParamsI[1], false);
                }
            }
            if (_ == null)
            {
                ErrorMsg = "Supported generator types: 0, 1, 2, 3";
                return null;
            }
            if (ErrorMsg != "")
            {
                return null;
            }

            return _;
        }

        protected void AddToStats(byte[] Raw)
        {
            if (StatsEnabled)
            {
                int SeqLen = Raw.Length;
                for (int i = 0; i < SeqLen; i++)
                {
                    Stats[Raw[i]]++;
                }
            }
        }
    }
}
