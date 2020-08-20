using System;
namespace BackupToMail
{
    public class RandomSequence
    {

        public virtual byte[] GenSeq(long SeqStart, long SeqLen)
        {
            return null;
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
                    DummyFileParamsI[i] = int.Parse(DummyFileParamsS[i + 1]);
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
            if (_ == null)
            {
                ErrorMsg = "Supported generator types: 0, 1";
                return null;
            }
            if (ErrorMsg != "")
            {
                return null;
            }

            return _;
        }
    }
}
