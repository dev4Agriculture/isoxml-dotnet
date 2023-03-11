using de.dev4Agriculture.ISOXML.DDI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev4Agriculture.ISO11783.ISOXML.Test
{
    [TestClass]
    public class DDIConstantGeneratorTests
    {
        #region TestData

        //DD Entity: 27 Actual Mass Per Mass Application Rate
        public const string Name_27 = "ActualMassPerMassApplicationRate";
        public const string Unit_27 = "mg/kg";
        public const float Resolution_27 = 1;
        public const string Definition_27 = "Actual Application Rate specified as mass per mass";

        //DD Entity: 86 Volume Per Time Yield
        public const string Name_86 = "VolumePerTimeYield";
        public const string Unit_86 = "ml/s";
        public const float Resolution_86 = 1;
        public const string Definition_86 = "Yield as volume per time";

        //DD Entity: 84 Mass Per Area Yield
        public const string Name_84 = "MassPerAreaYield";
        public const string Unit_84 = "mg/m_2";
        public const float Resolution_84 = 1;
        public const string Definition_84 = "Yield as mass per area, not corrected for the reference moisture percentage DDI 184.";

        #endregion

        [TestMethod]
        public void CanGenerateDDI()
        {
            var isoxml = ISOXML.Load("./testdata/TimeLogs/TotalsTests");
            // Check is DDI id
            Assert.IsTrue((int)DDIList.ActualMassPerMassApplicationRate == 27);
            Assert.IsTrue((int)DDIList.VolumePerTimeYield == 86);
            Assert.IsTrue((int)DDIList.MassPerAreaYield == 84);


            //Testing DDI name
            Assert.AreEqual(Name_27, DDIInfo.DDICollection[DDIList.ActualMassPerMassApplicationRate].Name);
            Assert.AreEqual(Name_86, DDIInfo.DDICollection[DDIList.VolumePerTimeYield].Name);
            Assert.AreEqual(Name_84, DDIInfo.DDICollection[DDIList.MassPerAreaYield].Name);

            //Testing DDI unit
            Assert.AreEqual(Unit_27, DDIInfo.DDICollection[DDIList.ActualMassPerMassApplicationRate].Unit);
            Assert.AreEqual(Unit_86, DDIInfo.DDICollection[DDIList.VolumePerTimeYield].Unit);
            Assert.AreEqual(Unit_84, DDIInfo.DDICollection[DDIList.MassPerAreaYield].Unit);

            //Testing DDI resolution
            Assert.AreEqual(Resolution_27, DDIInfo.DDICollection[DDIList.ActualMassPerMassApplicationRate].Resolution);
            Assert.AreEqual(Resolution_86, DDIInfo.DDICollection[DDIList.VolumePerTimeYield].Resolution);
            Assert.AreEqual(Resolution_84, DDIInfo.DDICollection[DDIList.MassPerAreaYield].Resolution);

            //Testing DDI definition
            Assert.AreEqual(Definition_27, DDIInfo.DDICollection[DDIList.ActualMassPerMassApplicationRate].Description);
            Assert.AreEqual(Definition_86, DDIInfo.DDICollection[DDIList.VolumePerTimeYield].Description);
            Assert.AreEqual(Definition_84, DDIInfo.DDICollection[DDIList.MassPerAreaYield].Description);


        }
    }
}

