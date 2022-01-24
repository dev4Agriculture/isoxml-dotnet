using System;

namespace Dev4ag.ISO11783 {

    // Extends RegularExpressionAttribute to byte[] by converting them to hex strings
    public class RegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        public RegularExpressionAttribute(string pattern) : base(pattern)
        {
        }

        public override bool IsValid(object value)
        {
            if (value.GetType().Name == "Byte[]") {
                var hexStr = BitConverter.ToString((byte[])value).Replace("-","");
                return base.IsValid(hexStr);
            } else {
                return base.IsValid(value);
            }
        }
    }
}