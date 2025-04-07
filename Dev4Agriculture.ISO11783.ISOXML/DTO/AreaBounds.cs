using System;
using System.Collections.Generic;
using System.Text;

namespace Dev4Agriculture.ISO11783.ISOXML.DTO
{
    public class AreaBounds
    {
        public decimal MinLat = decimal.MaxValue;
        public decimal MinLong = decimal.MaxValue;
        public decimal MaxLat = decimal.MinValue;
        public decimal MaxLong = decimal.MinValue;

        public void Update(decimal north, decimal east)
        {
            MinLat = MinLat < north ? MinLat : north;
            MinLong = MinLong < east ? MinLong : east;
            MaxLat = MaxLat > north ? MaxLat : north;
            MaxLong = MaxLong > east ? MaxLong : east;
        }
    }
}
