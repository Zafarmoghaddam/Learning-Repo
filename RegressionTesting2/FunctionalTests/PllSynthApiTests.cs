using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;


using NUnit.Framework;

using AdiCmdServerClient;

namespace mykonosUnitTest
{

    //TODO: For this Test Fixture
    //For Set TxPLLFrequency Add check on Fractional N Min
    //Add check for initDigitalClocks
    //Add check for checkPllsLockStatus
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5")]
    [Category("ApiFunctional")]
    public class PllSynApiFunctionalTests
    {
        private string RxProfile;
        private string TxProfile;
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public static TestSetupConfig settings = new TestSetupConfig();
        public PllSynApiFunctionalTests(string RxProfile, string TxProfile)
        {
            this.RxProfile = RxProfile;
            this.TxProfile = TxProfile;
        }
        public PllSynApiFunctionalTests(string RxProfile)
        {
            this.RxProfile = RxProfile;
            this.TxProfile = "Tx 75/200MHz, IQrate 245.76MHz, Dec5";
        }
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to QA Api Functional Tests
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// From Locally Stored Default Profile  @"..\..\..\..\mykonos_resources\"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: As Per Test Fixture Parameter
        ///     Tx Profile: Tx 75/200MHz, IQrate 245.76MHz, Dec5
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
        public void PllSynApiTestInit()
        {
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
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
            Console.WriteLine("TxApiFunctional Test Setup: Complete" );
        }
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo60250Khz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 60250Khz		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo60250Khz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 60250000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");

            //Check Tx VCO Divider, 
            //For 60Mhz Expecting 0x6
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("SPI Addr: 0x2F2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x06, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");

            //Check Tx Synth SDM Integer 
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x07D
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x7D, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");

            //Check Tx Synth SDM FRAC
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x42A680
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x80, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0xA6, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x42, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(60250000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo150Mhz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 150MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo150Mhz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 150000000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");

            //Check Tx VCO Divider, 
            //For 700Mhz Expecting 0x5
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("SPI Addr: 0x2F2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x05, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");

            //Check Tx Synth SDM Integer 
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x09C
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x9C, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");

            //Check Tx Synth SDM FRAC
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x1FFE00
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0xFE, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x1F, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(150000000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo200Mhz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 200MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo200Mhz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 200000000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");

            //Check Tx VCO Divider, 
            //For 700Mhz Expecting 0x4
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("SPI Addr: 0x2F2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x04, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");

            //Check Tx Synth SDM Integer 
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x068
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x68, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");

            //Check Tx Synth SDM FRAC
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x155400
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0x54, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x15, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(200000000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }
        
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo700Mhz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 700MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo700Mhz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);
         
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 700000000);
            System.Threading.Thread.Sleep(1000);
           
            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            
            //Check Tx VCO Divider, 
            //For 700Mhz Expecting 0x3
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("SPI Addr: 0x2F2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");
            
            //Check Tx Synth SDM Integer 
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x0B6
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0xB6, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");
            
            //Check Tx Synth SDM FRAC
            //For 700Mhz ref Clock 61.44Mhz Expecting 0x255300
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0x53, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x25, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(700000000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }
         
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo902200Khz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 902.2MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo902200Khz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 902200000);
            System.Threading.Thread.Sleep(1000);
            
            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
         
            //Check Tx VCO Divider, 
            //For 902.2MHz Expecting 0x2
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("Myk: Tx VCO Divider Val:" + spiData1.ToString("X"));
            Assert.AreEqual(0x02, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");
            //Check Tx Synth SDM Integer
            //For 902.2MHz, ref Clock 61.44Mhz 0x075
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x75, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");
            //Check Tx Synth SDM FRAC
            //For 902.2MHz, 0x3CA6E0
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0xE0, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0xA6, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3C, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(902200000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo2000Mhz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 2000MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo2000Mhz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 2000000000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");

            //Check Tx VCO Divider, 
            //For 902.2MHz Expecting 0x1
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("Myk: Tx VCO Divider Val:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");
            //Check Tx Synth SDM Integer
            //For 902.2MHz, ref Clock 61.44Mhz 0x082
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x82, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");
            //Check Tx Synth SDM FRAC
            //For 902.2MHz, 0x1AA900
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0xA9, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x1A, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(2000000000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     SetTxPllFrequencyTo45000Mhz
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL to 4500MHZ		
        /// API Test Pass Criteria: 
        ///     Check Tx VCO Divider is as expected
        ///     Check Tx Synth SDM Integer is as expected
        ///     Check Tx Synth SDM FRAC is as expected
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.			
        ///</summary>
        [Test]
        public static void SetTxPllFrequencyTo4500MHz()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 4500000000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check Tx PLL Lock
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");

            //Check Tx VCO Divider, 
            //For 902.2MHz Expecting 0x0
            spiData1 = Link.spiRead(0x2F2); Console.WriteLine("Myk: Tx VCO Divider Val:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x0F, "Myk: Tx VCO Divider not as expected");
            //Check Tx Synth SDM Integer
            //For 902.2MHz, ref Clock 61.44Mhz 0x092
            spiData1 = Link.spiRead(0x2B1); Console.WriteLine("SPI Addr: 0x2B1:" + spiData1.ToString("X"));
            Assert.AreEqual(0x92, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B2); Console.WriteLine("SPI Addr: 0x2B2:" + spiData1.ToString("X"));
            Assert.AreEqual(0x00, spiData1 & 0x07, "Myk: Tx Synth SDM Int not as expected");
            //Check Tx Synth SDM FRAC
            //For 902.2MHz, 0x3DFC20
            spiData1 = Link.spiRead(0x2B3); Console.WriteLine("SPI Addr: 0x2B3:" + spiData1.ToString("X"));
            Assert.AreEqual(0x20, spiData1 & 0xFF, "Myk: Tx Synth SDM LBS Int not as expected");
            spiData1 = Link.spiRead(0x2B4); Console.WriteLine("SPI Addr: 0x2B4:" + spiData1.ToString("X"));
            Assert.AreEqual(0xFC, spiData1 & 0xFF, "Myk: Tx Synth SDM Int not as expected");
            spiData1 = Link.spiRead(0x2B5); Console.WriteLine("SPI Addr: 0x2B5:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3D, spiData1 & 0x7F, "Myk: Tx Synth SDM Int MSB not as expected");

            //Check All PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.TX_PLL, ref readBack);
            Assert.AreEqual(4500000000, readBack, "Tx PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     SweepTxPllFrequency
        /// API Under-Test: 
        ///     MYKONOS_setRfPllFrequency	
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency to set TX_PLL 
        ///     from  1.5GHz to   3.15GHz            
        /// API Test Pass Criteria: 
        ///     Check Tx PLL is Locked. 
        ///     Check Clock Synth is locked. 
        ///     Check Rx Clock is still Locked.	
        /// 
        ///</summary>
        [Test]
        public static void SweepTxPllFrequency()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(1);
            Link.Mykonos.init_clocks(settings.mykSettings.DeviceClock_kHz, 9830400, Mykonos.VCODIV.VCODIV_1, 4);

            double[] freq = { 1.2605E+10, 1.2245E+10, 1.1906E+10, 1.1588E+10, 1.1288E+10, 1.1007E+10, 1.0742E+10, 1.0492E+10, 1.0258E+10, 1.0036E+10, 9.8278E+09, 9.6311E+09, 9.4453E+09, 9.2698E+09, 9.1036E+09, 8.9463E+09, 8.7970E+09, 8.6553E+09, 8.5206E+09, 8.3923E+09, 8.2699E+09, 8.1531E+09, 8.0414E+09, 7.9344E+09, 7.8318E+09, 7.7332E+09, 7.6384E+09, 7.5471E+09, 7.4590E+09, 7.3740E+09, 7.2919E+09, 7.2124E+09, 7.1355E+09, 7.0610E+09, 6.9887E+09, 6.9186E+09, 6.8506E+09, 6.7846E+09, 6.7205E+09, 6.6582E+09, 6.5978E+09, 6.5392E+09, 6.4823E+09, 6.4270E+09, 6.3734E+09, 6.3214E+09, 6.2709E+09, 6.2220E+09, 6.1745E+09, 6.1284E+09, 6.0836E+09, 6.0401E+09, 5.9977E+09 };

            byte pllStatus = 0;

            for (int i = 0; i <= 52; i++)
            {
                Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, (UInt64)(freq[i] / 4));
                System.Threading.Thread.Sleep(100);
                byte spiData1 = 0x0;
                //Check PLL Lock Status
                spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
                Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
                spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
                Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
                spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
                Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
                spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
                Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
                spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
                Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");
                Console.WriteLine("Frequency: " + freq[i]/4 + ", PLL Status: 0x" + pllStatus.ToString("X"));

                //TODO: Check if these need to be Added to Pass Criteria
                byte vcoOutLevel = (byte)(Link.spiRead(0x2BA) & 0x0F);
                byte vcoVar = (byte)(Link.spiRead(0x2B9) & 0xF);
                byte vcoBiasRef = (byte)(Link.spiRead(0x2C2) & 0x7);
                byte vcoBiasTcf = (byte)((Link.spiRead(0x2C2) >> 3) & 0x3);
                byte vcoCalOffset = (byte)((Link.spiRead(0x2B8) >> 3) & 0xF);
                byte vcoVarRef = (byte)(Link.spiRead(0x2D1) & 0xF);
                byte iCp = (byte)(Link.spiRead(0x2BB) & 0x3F);
                byte lfC2 = (byte)((Link.spiRead(0x2BE) >> 4) & 0xF);
                byte lfC1 = (byte)(Link.spiRead(0x2BE) & 0xF);
                byte lfR1 = (byte)((Link.spiRead(0x2BF) >> 4) & 0xF);
                byte lfC3 = (byte)(Link.spiRead(0x2BF) & 0xF);
                byte lfR3 = (byte)(Link.spiRead(0x2C0) & 0xF);

                Console.WriteLine(freq[i] + "\t" + vcoOutLevel + "\t" + vcoVar + "\t" + vcoBiasRef + "\t" + vcoBiasTcf + "\t" + vcoCalOffset + "\t" + vcoVarRef + "\t" + iCp + "\t" + lfC2 + "\t" + lfC1 + "\t" + lfR1 + "\t" + lfC3 + "\t" + lfR3);
            }

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     getPllFrequency
        /// API Under-Test: 
        ///     MYKONOS_getRfPllFrequency		
        /// API Test Description: 
        ///     Call API MYKONOS_setRfPllFrequency then call 
        ///     MYKONOS_getRfPllFrequency to read back
        /// API Test Pass Criteria: 
        ///     Readback matches the value being set		
        ///</summary>
        [Test]
        public static void getPllFrequency([Values(Mykonos.PLLNAME.RX_PLL, Mykonos.PLLNAME.SNIFFER_PLL)]Mykonos.PLLNAME pllName)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(MykonosSpi);

            Link.Mykonos.setRfPllFrequency(pllName, 3500000000);
            System.Threading.Thread.Sleep(1000);

            byte spiData1 = 0x0;
            //Check PLL Lock Status
            spiData1 = Link.spiRead(0x157); Console.WriteLine("Myk: Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: CLK PLL not locked");
            spiData1 = Link.spiRead(0x257); Console.WriteLine("Myk: Rx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Rx CLK Syn not locked");
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("Myk: Tx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Tx CLK Syn not locked");
            spiData1 = Link.spiRead(0x357); Console.WriteLine("Myk: SnRx Clk Syn Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: SnRx CLK Syn not locked");
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("Myk: Cal PLL Stat:" + spiData1.ToString("X"));
            Assert.AreEqual(0x01, spiData1 & 0x01, "Myk: Cal PLL  not locked");

            ulong readBack = 0;
            Link.Mykonos.getRfPllFrequency(Mykonos.PLLNAME.CLK_PLL, ref readBack);
            Console.Write("Test" + settings.rxProfileData.ClkPllFrequency_kHz);
            Assert.AreEqual(settings.rxProfileData.ClkPllFrequency_kHz, (readBack/1000), "CLKPLL Frequency Readback Not as Expected");
            Console.WriteLine("Clk Pll Frequency: " + readBack);

            Link.Mykonos.getRfPllFrequency(pllName, ref readBack);
            Assert.AreEqual(3500000000, readBack, "PLL Frequency Readback Not as Expected");

            Link.Disconnect();
        }

    }
}
