using System;
using System.Diagnostics;


using NUnit.Framework;
using AdiCmdServerClient;

namespace mykonosUnitTest
{
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [Category("ApiFunctional")]
    public class QABasicSmokeTests
    {
        private string RxProfile;
        private string TxProfile;
        private string OrxProfile;
        public static TestSetupConfig settings = new TestSetupConfig();
        public QABasicSmokeTests(string RxProfile, string TxProfile, string OrxProfile)
        {
            this.RxProfile = RxProfile;
            this.TxProfile = TxProfile;
            this.OrxProfile = OrxProfile;
        }
        public QABasicSmokeTests(string RxProfile)
        {
            this.RxProfile = RxProfile;
            this.TxProfile = "Tx 75/200MHz, IQrate 245.76MHz, Dec5";
            this.RxProfile = "ORX 200MHz, IQrate 245.75MHz, Dec5";
        }

        /// <summary>
        /// TestSetupSmokeTest Configure Test Setup Prior to QA Testing And Smoke Test
        /// Setup Parameters:  
        /// From Locally Stored ARM Firmware     @"..\..\..\resources\Profiles";
        /// From Locally Stored Default Profile  @"..\..\..\resources\ArmBinaries"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: ARx Profile: As Per Test Fixture Parameter
        ///     Tx Profile: As Per Test Fixture Parameter
        ///     Rx Data Paths: RX1 and Rx2 Enabled
        ///     Tx Data Paths: TX1 and TX2 Enabled
        ///     ObsRx Data Paths: ObsRx1 Enabled
        ///     Sniffer Data Path: Disabled
        ///     JESD204B Deframer Settings:
        ///         deviceID = 0;
        ///         laneID = 0;
        ///         bankID = 0;
        ///         M = 4;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         deserializerLanesEnabled = 0x0F;
        ///         deserializerLaneCrossbar = 0xE4;
        ///         eqSetting = 1;
        ///         invertLanePolarity = 0;
        ///         enableAutoChanXbar = 0;
        ///         lmfcOffset = 0;
        ///         newSysrefOnRelink = 0;
        ///     JESD204B Framer Settings:
        ///         bankId = 0;
        ///         deviceId = 0;
        ///         laneId = 0;
        ///         M = 4;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         serializerLanesEnabled = 0x3;
        ///         serializerLaneCrossbar = 0xE4;
        ///         serializerAmplitude = 22;
        ///         preEmphasis = 4;
        ///         invertLanePolarity = 0;
        ///         lmfcOffset = 0;
        ///         obsRxSyncbSelect = 0;
        ///         newSysrefOnRelink = 0;
        ///         overSample = 0;
        ///         enableAutoChanXbar = 0;
        ///     JESD204B ObsRx Framer Settings:
        ///         bankId = 1;
        ///         deviceId = 0;
        ///         laneId = 0;
        ///         M = 2;
        ///         K = 32;
        ///         scramble = 1;
        ///         externalSysref = 1;
        ///         serializerLanesEnabled = 0xC;
        ///         serializerLaneCrossbar = 0xE4;
        ///         serializerAmplitude = 22;
        ///         preEmphasis = 4;
        ///         invertLanePolarity = 0;
        ///         lmfcOffset = 0;
        ///         obsRxSyncbSelect = 1;
        ///         newSysrefOnRelink = 0;
        ///         overSample = 1;
        ///         enableAutoChanXbar = 0;
        /// </summary>
        [SetUp]
        public void SmokeTestInit()
        {

           //Use Default Test Constructor
            UInt32 calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
                            (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
                            (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
                            (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
                            (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
                            (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                            (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
                            (UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
                            (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
                            (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                            (UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                            (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
           settings.calMask = calMask;
           settings.mykSettings.rxProfileName = RxProfile;
           settings.mykSettings.txProfileName = TxProfile;
           settings.mykSettings.orxProfileName = OrxProfile;

           //Call Test Setup 
           TestSetup.TestSetupInit(settings);
        }
        [Test]
        public static void AD9528ClockStatusCheck()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);




            byte pllStatus = Link.Ad9528.readPllStatus();
            Assert.AreEqual(0x27, (pllStatus & 0x27), "AD9528 PLL not locked or does REFCLK not detected.");

            byte spiData = Link.spiRead(0x403); Console.WriteLine("AD9528: 0x403: " + spiData.ToString("X"));

            Console.WriteLine("AD9528: 9528 PLL Status: " + pllStatus.ToString("X"));

            Link.Disconnect();
        }

        //[Test]
        public static void MykonosCheckReset()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Check that ENSM status is no longer in Alert State following Reset
            byte spiData1 = 0x0;
            spiData1 = Link.spiRead(0x1B3);
            Console.Write("0x1B3: " + spiData1.ToString("x"));

            Link.Mykonos.resetDevice();

            System.Threading.Thread.Sleep(5000);

            spiData1 = Link.spiRead(0x1B3);
            Console.Write("0x1B3: " + spiData1.ToString("x"));
            Assert.AreEqual(0xFF, spiData1);
            Link.Disconnect();
        }

        [Test]
        public static void MykonosInitStatusCheck()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.setSpiChannel(TestSetup.MykonosSpi);
            Console.WriteLine("SPIRead x119: " + Link.spiRead(0x119).ToString("X"));
            Console.WriteLine("SPIRead x157: " + Link.spiRead(0x157).ToString("X"));
            Console.WriteLine("SPIRead x1B3: " + Link.spiRead(0x1B3).ToString("X"));
            UInt32 pllStatus = Link.spiRead(0x157);
            Assert.AreEqual(0x01, pllStatus & 0x01, "CLK PLL not locked");


            UInt32 ensmStatus = Link.spiRead(0x1B3);
            Assert.AreEqual(0x05, ensmStatus & 0x7, "Not in Alert state.");

            Link.Disconnect();
        }


        [Test]
        public static void MykonosInitArmCheck()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            bool isArmGood = Link.Mykonos.verifyArmChecksum();
            Assert.AreEqual(true, isArmGood, "ARM checksum did not match");
           
            //Check ARM Version API
            byte[] ver = new byte[4];
            UInt32 fullVersion = 0;
            Link.Mykonos.readArmMem(0x01000128, 4, 1, ref ver);
            fullVersion = ((UInt32)(ver[0]) | ((UInt32)(ver[1]) << 8) | ((UInt32)(ver[2]) << 16) | ((UInt32)(ver[3]) << 24));


            string version = Link.Mykonos.getArmVersion();
            Console.WriteLine("ARM Version: " + version);

            byte armMajor = 0;
            byte armMinor = 0;
            byte armRc = 0;
            Link.Mykonos.getArmVersion(ref armMajor, ref armMinor, ref armRc);
            Console.WriteLine("ARM Version: " + armMajor + "." + armMinor + "." + armRc);
            Assert.AreEqual(fullVersion % 100, armRc, "ARM RC version readback not as expected");
            Assert.AreEqual((fullVersion / 100) % 100, armMinor, "ARM minor version readback not as expected");
            Assert.AreEqual(fullVersion / 10000, armMajor, "ARM major version readback not as expected");
            Assert.AreEqual(armMajor + "." + armMinor + "." + armRc, version, "ARM version readback not as expected");

            //Debug - make sure we loaded ARM profile into ARM mem
            byte[] armMemData = new byte[96];
            Link.Mykonos.readArmMem(0x20000000, 96, 1, ref armMemData);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(settings.resultsPath + @"QABasicSmokeTest\ArmProfile.txt"))
            {
                foreach (byte element in armMemData)
                {
                    file.WriteLine(element.ToString("X"));
                }
            }

            Link.Disconnect();
        }

       
        [Test]
        public static void MykonosCheckInitCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(TestSetup.MykonosSpi);


            Console.WriteLine("InitArmCal(0x" + settings.calMask.ToString("X") + ")");
            Console.WriteLine("SPIRead x119: " + Link.spiRead(0x119).ToString("X"));
            Console.WriteLine("SPIReadxD34: " + Link.spiRead(0xD34).ToString("X"));

            Link.Disconnect();
        }
      

        [Test]
        public static void MykonosCheckRxFramerLink()
        {
            //TODO: Possible convert these to SPI Writes
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(TestSetup.MykonosSpi);

            byte status = Link.Mykonos.readRxFramerStatus();
            Console.WriteLine("Framer Status: " + status.ToString("X"));
            Assert.AreEqual(0x20, (status & 0x20), "SYSREF not received by Mykonos Rx Framer IP");

            status = Link.Mykonos.readOrxFramerStatus();
            Console.WriteLine("ObsRx Framer Status: " + status.ToString("X"));
            Assert.AreEqual(0x20, (status & 0x20), "SYSREF not received by Mykonos ObsRx Framer IP");


            UInt32 syncStatus = Link.FpgaMykonos.readSyncbStatus();
            Console.WriteLine("SYNC Status: " + syncStatus.ToString("X"));
            Assert.AreEqual(0x02, (syncStatus & 0x2), "RXSYNBC not asserted, Rx JESD204 Link down.");

            Link.Disconnect();
        }
        [Test]
        public static void MykonosCheckTxDeFramerLink()
        {
            //TODO: Possible convert these to SPI Writes
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(TestSetup.MykonosSpi);

            Console.WriteLine("Mykonos Deframer Status: " + Link.Mykonos.readDeframerStatus().ToString("X"));
            byte spiReg = 0;
            spiReg = Link.spiRead(0x1B0);
            Console.WriteLine("SPI Reg x1B0 = " + spiReg.ToString("X"));

            
            Link.Disconnect();
        }
        [Test]
        public static void MykonosCheckFPGAFramerLink()
        {
            //TODO: Possible convert these to SPI Writes
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);


            UInt32 fpgaReg = 0;

            //Check Tx DeFramer -FPGA JESD Link
            fpgaReg = Link.fpgaRead(0x418);
            Console.WriteLine("FPGA Reg x418 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x428);
            Console.WriteLine("FPGA Reg x428[4 TxSYNCb] = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x42C);
            Console.WriteLine("FPGA Reg x42C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x430);
            Console.WriteLine("FPGA Reg x430 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x434);
            Console.WriteLine("FPGA Reg x434 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x438);
            Console.WriteLine("FPGA Reg x438 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x43C);
            Console.WriteLine("FPGA Reg x43C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x428); 
            Debug.WriteLine("FPGA Reg x428[4 TxSYNCb] = " + fpgaReg.ToString("X"));
            
            Assert.AreEqual(0x01, ((fpgaReg >> 4) & 1), "TxSYNCb low");


            Link.Disconnect();
        }
        [Test] 
        public static void MykonosReadTIA3dbCorner()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(TestSetup.MykonosSpi);

            //Just for debug -read TIA 3dB corner
            byte spiReg = 0;
            double rx1TiaCap = 480; //fF
            double rx2TiaCap = 480; //fF
            spiReg = Link.spiRead(0x224); Debug.WriteLine("0xRCAL code = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x693); Debug.WriteLine("0x693 = " + spiReg.ToString("X"));
            rx1TiaCap += ((spiReg & 0x3F) * 10);
            spiReg = Link.spiRead(0x694); Debug.WriteLine("0x694 = " + spiReg.ToString("X"));
            rx1TiaCap += ((spiReg & 0x1F) * 320);
            Console.WriteLine("Rx1 TIA 3dB corner = " + (1 / (2 * System.Math.PI * 1100 * rx1TiaCap * System.Math.Pow(10, -9))) + " MHz");

            spiReg = Link.spiRead(0x695); Debug.WriteLine("0x695 = " + spiReg.ToString("X"));
            rx2TiaCap += ((spiReg & 0x3F) * 10);
            spiReg = Link.spiRead(0x696); Debug.WriteLine("0x696 = " + spiReg.ToString("X"));
            rx2TiaCap += ((spiReg & 0x1F) * 320);
            Console.WriteLine("Rx1 TIA 3dB corner = " + (1 / (2 * System.Math.PI * 1100 * rx2TiaCap * System.Math.Pow(10, -9))) + " MHz");

            Link.Disconnect();
        }

        [Test]
        public static void MykonosCheckGetMykonosErrorMessage()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            string errorMsg = "";
            uint MAX_ERRORCODE = 277;

            errorMsg = Link.Mykonos.getMykonosErrorMessage(0);
            Console.WriteLine(errorMsg);
            Assert.AreEqual("\n", errorMsg, "Error Message not as expected");

            for (uint i = 1; i < MAX_ERRORCODE; i++)
            {
                errorMsg = Link.Mykonos.getMykonosErrorMessage(i);
                Console.WriteLine(errorMsg);
                Assert.AreNotEqual("", errorMsg, "Error Message not as expected. Error code: " + i);
            }
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckDeviceRev
        /// API Under-Test: 
        ///     MYKONOS_getDeviceRev
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Manually write the data and config the chip
        ///     Call MYKONOS_getDeviceRev and see if the 
        ///     value is 0x00-0x03
        /// API Test Pass Criteria: 
        ///     read back value is 
        ///     0x00-0x03
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void MykonosCheckDeviceRev()
        {
           
            byte[] readarmData = new byte[] { };

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;

            byte devRev = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Init Cals not completed");
            //Check Device Revision value
            Link.Mykonos.getDeviceRev(ref devRev);
            Assert.GreaterOrEqual(devRev, 0, "Device Revision is less than 1");
            Assert.Less(devRev, 4, "Device Revision is more than 3");
            Console.WriteLine(devRev);
            spiData1 = Link.spiRead(0x004);
            //Check if the function returns the same value as the register
            Assert.AreEqual(spiData1, devRev, "Function return value not the same as register value");

            Link.Disconnect();
        }
    }
}
