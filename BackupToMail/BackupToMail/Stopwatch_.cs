/*
 * Created by SharpDevelop.
 * User: XXX
 * Date: 2020-05-31
 * Time: 16:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace BackupToMail
{
    /// <summary>
    /// Time measurement object similar to always running stopwatch
    /// </summary>
    public class Stopwatch_
    {
        // Environment.TickCount, DateTime.UtcNow or Stopwatch
        
        DateTime ResetTime;
        
        public void Reset()
        {
            ResetTime = DateTime.UtcNow;
        }
        
        public long Elapsed()
        {
            return (long)((DateTime.UtcNow - ResetTime).TotalMilliseconds);
        }

        public Stopwatch_()
        {
            Reset();
        }

        public bool ProgressTriggeringValue(ref long TriggeringValue)
        {
            long Elapsed_ = Elapsed();
            if (Elapsed_ > TriggeringValue)
            {
                long Delta = Elapsed_ - TriggeringValue;
                Delta = Delta + 1000L;
                Delta = Delta / 1000L;
                Delta = Delta * 1000L;
                TriggeringValue += Delta;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
