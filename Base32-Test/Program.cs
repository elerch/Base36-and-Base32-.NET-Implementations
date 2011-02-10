using System;
using System.Diagnostics;

namespace Base32Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Base36Tests();
            Base32Tests();
        }

        private static void Base32Tests()
        {
            long ticks = DateTime.Now.Ticks;
            long baseTicks = ticks - 1125899906842624;  // This is 11 digits of Base32
            // 100-0000-0000

            Debug.WriteLine(baseTicks);
            Debug.WriteLine(new DateTime(baseTicks));  // Beginning of epoch (11 digit Base32 number)
            Base32 x = "100-0000-00000*U$=";
            Debug.Assert(x.NumericValue == 36028797018963968);
            Debug.WriteLine(new DateTime(baseTicks + x.NumericValue));  // End of 11 digit space (approx 400 yrs?)
            Debug.WriteLine(Base32.ToBase32(ticks - baseTicks));
            Debug.WriteLine(Base32.ToBase32(ticks - baseTicks).Length);

            // Write out the number for some random date in the future
            Debug.WriteLine(Base32.ToBase32(new DateTime(2345, 1, 1).Ticks - baseTicks));

        }
        private static void Base36Tests()
        {
            long ticks = DateTime.Now.Ticks;
            long baseTicks = ticks - 3656158440062976;
            // 10000000000
            // 01234567890
            Debug.WriteLine(baseTicks);
            Debug.WriteLine(new DateTime(baseTicks));  // Beginning of epoch (11 digit Base36 number)
            Base36 x = "100000000000";
            Debug.WriteLine(new DateTime(baseTicks + x.NumericValue));  // End of 11 digit space (approx 400 yrs)
            Debug.WriteLine(Base36.NumberToBase36(ticks - baseTicks));
            Debug.WriteLine(Base36.NumberToBase36(ticks - baseTicks).Length);

            // Write out the number for some random date in the future
            Debug.WriteLine(Base36.NumberToBase36(new DateTime(2345, 1, 1).Ticks - baseTicks));

        }
    }
}
