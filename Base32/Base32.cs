using System;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Class representing a Base32 number.  Based on Douglas Crockford"s Base32: http://www.crockford.com/wrmg/base32.html
/// </summary>
public struct Base32
{
    /// <summary>
    /// Base32 containing the maximum supported value for this type
    /// </summary>
    public static readonly Base32 MaxValue = new Base32(long.MaxValue);

    /// <summary>
    /// Base32 containing the minimum supported value for this type
    /// </summary>
    public static readonly Base32 MinValue = new Base32(long.MinValue + 1);

    private static readonly char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    private long numericValue;

    private static readonly IDictionary<char, byte> charDict;

    static Base32()
    {
        charDict = new Dictionary<char,byte>();
        for (byte i = 0; i < chars.Length; i++)
            charDict.Add(chars[i], i);
    }

    /// <summary>
    /// Instantiate a Base32 number from a long value
    /// </summary>
    /// <param name="NumericValue">The long value to give to the Base32 number</param>
    public Base32(long NumericValue)
    {
        numericValue = 0; //required by the struct.
        this.NumericValue = NumericValue;
    }


    /// <summary>
    /// Instantiate a Base32 number from a Base32 string
    /// </summary>
    /// <param name="Value">The value to give to the Base32 number</param>
    public Base32(string Value)
    {
        numericValue = 0; //required by the struct.
        this.Value = Value;
    }


    /// <summary>
    /// Get or set the value of the type using a base-10 long integer
    /// </summary>
    public long NumericValue
    {
        get
        {
            return numericValue;
        }
        set
        {
            //Make sure value is between allowed ranges
            if (value <= long.MinValue || value > long.MaxValue) {
                throw new ArgumentOutOfRangeException(value.ToString());
            }

            numericValue = value;
        }
    }


    /// <summary>
    /// Get or set the value of the type using a Base32 string
    /// </summary>
    public string Value
    {
        get
        {
            return Base32.ToBase32(numericValue);
        }
        set
        {
            try {
                numericValue = Base32.ToNumber(value);
            }
            catch {
                //Catch potential errors
                throw new ArgumentOutOfRangeException(value.ToString());
            }
        }
    }


    /// <summary>
    /// Static method to convert a Base32 string to a long integer (base-10)
    /// </summary>
    /// <param name="base32Value">The number to convert from</param>
    /// <returns>The long integer</returns>
    public static long ToNumber(string base32Value)
    {
        // Make sure we have passed something
        if (base32Value == "") {
            throw new ArgumentOutOfRangeException(base32Value);
        }

        // Make sure the number is in upper case:
        StringBuilder sb = new StringBuilder(base32Value.ToUpper());

        // Account for negative values:
        bool isNegative = false;
        if (base32Value[0] == '-') {
            sb.Remove(0, 1);
            isNegative = true;
        }

        // Remove any other "-" and *~$=U
        foreach (var chr in new string[] { "-", "*", "~", "$", "=", "U" })
            sb.Replace(chr, "");

        // Convert confusing characters to standard format
        sb.Replace('O', '0'); // Capital O to zero
        sb.Replace('I', '1'); // Capital I to one
        sb.Replace('L', '1'); // Capital L to one

        // Done modifying the input, let's write it back
        base32Value = sb.ToString();

        //Loop through our string and calculate its value
        try {
            //Keep a running total of the value
            long returnValue = Base32DigitToNumber(base32Value[base32Value.Length - 1]);

            //Loop through the character in the string (right to left) and add
            //up increasing powers as we go.
            for (int i = 1; i < base32Value.Length; i++) {
                returnValue += ((long)Math.Pow(32, i) * Base32DigitToNumber(base32Value[base32Value.Length - (i + 1)]));
            }

            //Do negative correction if required:
            return returnValue * (isNegative ? -1 : 1);
        }
        catch {
            //If something goes wrong, this is not a valid number
            throw new ArgumentOutOfRangeException(base32Value);
        }
    }


    /// <summary>
    /// Public static method to convert a long integer (base-10) to a Base32 number
    /// </summary>
    /// <param name="NumericValue">The base-10 long integer</param>
    /// <returns>A Base32 representation</returns>
    public static string ToBase32(long NumericValue)
    {
        try {
            //Handle negative values:
            if (NumericValue < 0) {
                return string.Concat("-", PositiveNumberToBase32(Math.Abs(NumericValue)));
            }
            else {
                return PositiveNumberToBase32(NumericValue);
            }
        }
        catch {
            throw new ArgumentOutOfRangeException(NumericValue.ToString());
        }
    }


    private static string PositiveNumberToBase32(long NumericValue)
    {
        //This is a clever recursively called function that builds
        //the base-32 string representation of the long base-10 value
        if (NumericValue < 32) {
            //The get out clause; fires when we reach a number less than 
            //32 - this means we can add the last digit.
            return NumberToBase32Digit((byte)NumericValue).ToString();
        }
        else {
            //Add digits from left to right in powers of 32 
            //(recursive)
            return string.Concat(PositiveNumberToBase32(NumericValue / 32), NumberToBase32Digit((byte)(NumericValue % 32)).ToString());
        }
    }


    private static byte Base32DigitToNumber(char base32Digit)
    {
        return charDict[base32Digit];
    }


    private static char NumberToBase32Digit(byte numericValue)
    {
        // Converts a number to it's base-32 value.
        // Only works for numbers <= 31.
        if (numericValue > 31) 
            throw new ArgumentOutOfRangeException(numericValue.ToString());

        return chars[numericValue];
    }

    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator >(Base32 LHS, Base32 RHS)
    {
        return LHS.numericValue > RHS.numericValue;
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator <(Base32 LHS, Base32 RHS)
    {
        return LHS.numericValue < RHS.numericValue;
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator >=(Base32 LHS, Base32 RHS)
    {
        return LHS.numericValue >= RHS.numericValue;
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator <=(Base32 LHS, Base32 RHS)
    {
        return LHS.numericValue <= RHS.numericValue;
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator ==(Base32 LHS, Base32 RHS)
    {
        return LHS.numericValue == RHS.numericValue;
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static bool operator !=(Base32 LHS, Base32 RHS)
    {
        return !(LHS == RHS);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static Base32 operator +(Base32 LHS, Base32 RHS)
    {
        return new Base32(LHS.numericValue + RHS.numericValue);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static Base32 operator -(Base32 LHS, Base32 RHS)
    {
        return new Base32(LHS.numericValue - RHS.numericValue);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static Base32 operator ++(Base32 Value)
    {
        return new Base32(Value.numericValue++);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public static Base32 operator --(Base32 Value)
    {
        return new Base32(Value.numericValue--);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static Base32 operator *(Base32 LHS, Base32 RHS)
    {
        return new Base32(LHS.numericValue * RHS.numericValue);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static Base32 operator /(Base32 LHS, Base32 RHS)
    {
        return new Base32(LHS.numericValue / RHS.numericValue);
    }


    /// <summary>
    /// Operator overload
    /// </summary>
    /// <param name="LHS"></param>
    /// <param name="RHS"></param>
    /// <returns></returns>
    public static Base32 operator %(Base32 LHS, Base32 RHS)
    {
        return new Base32(LHS.numericValue % RHS.numericValue);
    }


    /// <summary>
    /// Converts type Base32 to a base-10 long
    /// </summary>
    /// <param name="Value">The Base32 object</param>
    /// <returns>The base-10 long integer</returns>
    public static implicit operator long(Base32 Value)
    {
        return Value.numericValue;
    }


    /// <summary>
    /// Converts type Base32 to a base-10 integer
    /// </summary>
    /// <param name="Value">The Base32 object</param>
    /// <returns>The base-10 integer</returns>
    public static implicit operator int(Base32 Value)
    {
        try {
            return (int)Value.numericValue;
        }
        catch {
            throw new OverflowException("Overflow: Value too large to return as an integer");
        }
    }


    /// <summary>
    /// Converts type Base32 to a base-10 short
    /// </summary>
    /// <param name="Value">The Base32 object</param>
    /// <returns>The base-10 short</returns>
    public static implicit operator short(Base32 Value)
    {
        try {
            return (short)Value.numericValue;
        }
        catch {
            throw new OverflowException("Overflow: Value too large to return as a short");
        }
    }


    /// <summary>
    /// Converts a long (base-10) to a Base32 type
    /// </summary>
    /// <param name="Value">The long to convert</param>
    /// <returns>The Base32 object</returns>
    public static implicit operator Base32(long Value)
    {
        return new Base32(Value);
    }


    /// <summary>
    /// Converts type Base32 to a string; must be explicit, since
    /// Base32 > string is dangerous!
    /// </summary>
    /// <param name="Value">The Base32 type</param>
    /// <returns>The string representation</returns>
    public static explicit operator string(Base32 Value)
    {
        return Value.Value;
    }


    /// <summary>
    /// Converts a string to a Base32
    /// </summary>
    /// <param name="Value">The string (must be a Base32 string)</param>
    /// <returns>A Base32 type</returns>
    public static implicit operator Base32(string Value)
    {
        return new Base32(Value);
    }

    /// <summary>
    /// Returns a string representation of the Base32 number
    /// </summary>
    /// <returns>A string representation</returns>
    public override string ToString()
    {
        return Base32.ToBase32(numericValue);
    }


    /// <summary>
    /// A unique value representing the value of the number
    /// </summary>
    /// <returns>The unique number</returns>
    public override int GetHashCode()
    {
        return numericValue.GetHashCode();
    }


    /// <summary>
    /// Determines if an object has the same value as the instance
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>True if the values are the same</returns>
    public override bool Equals(object obj)
    {
        if (!(obj is Base32)) {
            return false;
        }
        else {
            return this == (Base32)obj;
        }
    }

    /// <summary>
    /// Returns a string representation padding the leading edge with
    /// zeros if necessary to make up the number of characters
    /// </summary>
    /// <param name="MinimumDigits">The minimum number of digits that the string must contain</param>
    /// <returns>The padded string representation</returns>
    public string ToString(int MinimumDigits)
    {
        string base32Value = Base32.ToBase32(numericValue);

        if (base32Value.Length >= MinimumDigits) {
            return base32Value;
        }
        else {
            string padding = new string('0', (MinimumDigits - base32Value.Length));
            return string.Format("{0}{1}", padding, base32Value);
        }
    }

}
