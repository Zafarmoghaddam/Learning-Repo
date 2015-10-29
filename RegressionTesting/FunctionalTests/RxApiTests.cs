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
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5")]
    [TestFixture]
    [Category("ApiFunctional")]
    public class RxApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static Int16[] RxFirCoeffs = new Int16[48] {  
                       4,     2,   -18,   -36,     -4,   86, 
                     120,   -30,  -276,  -270,    192,  692,
                     442,  -666, -1444,  -488,   1782,  2710,
                      86,  -4412, -5352,  1526, 14058, 24064, 
                   24064,  14058,  1526, -5352, -4412,    86, 
                    2710,   1782,  -488, -1444,  -666,   442,
                     692,    192,  -270,  -276,   -30,   120,
                      86,     -4,   -36,   -18,     2,     4};
        public static byte[,] RxGainIndex = new byte[72, 4] {
            {0,	 0,	0,	0 }, { 3, 0,  1, 1}, {6,  0,  2, 1},
            {9,	 0,	3,	1},  {12, 0,  3, 1}, {15, 0,  2, 1},
            {18, 0,	1,	1},  {20, 0,  3, 1}, {23, 0,  1, 1},
            {25, 0,	2,	1},  {27, 0,  3, 1}, {29, 0,  4, 1},
            {31, 0,	3,	1},  {33, 0,  2, 1}, {35, 0,  1, 1},
            {36, 0,	5,	1},  {38, 0,  2, 1}, {39, 0,  6, 1},
            {41, 0,	1,	1},  {42, 0,  4, 1}, {43, 0,  6, 1},
            {44, 0,	7,	1},  {45, 0,  8, 1}, {46, 0,  9, 1},
            {47, 0,	9,	1},  {48, 0,  8, 1}, {49, 0,  7, 1},
            {50, 0,	5,	1},  {51, 0,  3, 1}, {51, 0, 13, 1},
            {52, 0,	10,	1},  {53, 0,  5, 1}, {53, 0, 15, 1},
            {54, 0,	9,  1},  {55, 0,  1, 1}, {55, 0, 11, 1},
            {56, 0,	1,	1},  {56, 0, 11, 1}, {56, 0, 21, 1},
            {57, 0,	9,	1},  {57, 0, 19, 1}, {58, 0,  2, 1},
            {58, 0,	12,	1},  {58, 0, 22, 1}, {59, 0,  1, 1},
            {59, 0,	11,	1},  {59, 0, 21, 1}, {59, 0, 31, 1},
            {60, 0,	3,	1},  {60, 0, 13, 1}, {60, 0, 23, 1},
            {60, 0,	33,	1},  {60, 0, 43, 1}, {61, 0,  4, 1},
            {61, 0,	14,	1},  {61, 0, 24, 1}, {61, 0, 34, 1},
            {61, 0,	44,	1},  {61, 0, 54, 1}, {61, 0, 64, 1},
            {62, 0,	4,	1},  {62, 0, 14, 1}, {62, 0, 24, 1},
            {62, 0,	34,	1},  {62, 0, 44, 1}, {62, 0, 54, 1},
            {62, 0,	64,	1},  {62, 0, 74, 1}, {62,	0,	84,	1},
            {62, 0,	94,	1},  {62, 0, 104, 1}, {62,	0,	114, 1}
        };
        private string RxProfile;
        public RxApiFunctionalTests(string RxProfile)
        {
            this.RxProfile = RxProfile;
        }
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to QA Api Functional Tests
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// From Locally Stored Default Profile  @"..\..\..\..\mykonos_resources\"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: Rx Profile: As Per Test Fixture Parameter
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
        public void RxApiTestInit()
        {
            //Set Calibration Mask
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
            Console.WriteLine("TxApiFunctional Test Setup: Complete");
        }


        ///<summary>
        /// API Test Name: 
        ///     CheckRxFirFilter
        /// API Under-Test: 
        ///     MYKONOS_programFir	
        /// API Test Description: 
        ///     Call MYKONOS_programFir to set RxFilter to 
        ///     from  RxApp6_BW40_ADC1228p8_OR76p8.ftr 
        ///     See Notes below for details
        /// API Test Pass Criteria: 
        ///     Check Rx Filter Registers are updated 
        ///     as Expected with the correct Gain,
        ///     Taps and coefficient values.
        /// Notes:
        /// RXFIR_GAIN =-6dB
        /// RXFIR_TAPS = 48
        /// RXFIR_COEFFS : 4,     2,   -18,   -36,     -4,   86, 
        ///              120,   -30,  -276,  -270,    192,  692,
        ///              442,  -666, -1444,  -488,   1782,  2710,
        ///               86,  -4412, -5352,  1526, 14058, 24064, 
        ///            24064,  14058,  1526, -5352, -4412,    86, 
        ///             2710,   1782,  -488, -1444,  -666,   442,
        ///              692,    192,  -270,  -276,   -30,   120,
        ///               86,     -4,   -36,   -18,     2,     4
        ///</summary>
        [Test, Sequential]
        public static void CheckRxFirFilter([Values(Mykonos.FIR.RX1_FIR, Mykonos.FIR.RX2_FIR)]Mykonos.FIR RxFirMode)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte firMSB = 0;
            byte firLSB = 0;
            string rxFirFile = settings.resourcePath + @"\DigitalFilters\" + "RxApp6_BW40_ADC1228p8_OR76p8.ftr";
            Link.Mykonos.programFir(RxFirMode, rxFirFile);

            //Select PFIR to RX1
            if (RxFirMode == Mykonos.FIR.RX1_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xC4));
            }
            else if (RxFirMode == Mykonos.FIR.RX2_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xC8));
            }
            else
                throw new ArgumentException();


            //Readback Filter data

            //Read Configured RX Gain
            //Read Read Number of Taps
            //Cross reference Coeffecients


            spiData1 = Link.spiRead(0x411); Console.WriteLine("Rx1 Filter Gain: " + (spiData1 & 0x03).ToString("X"));
            Assert.AreEqual(0x1, (spiData1 & 0x3), "Myk: Rx1 Filter Gain: not as expected");

            spiData1 = Link.spiRead(0x410); Console.WriteLine("Myk: Rx1 Filter NumOf Taps: " + (spiData1).ToString("X"));
            Assert.AreEqual(0x1, ((spiData1 & 0x60) >> 5), "Myk: Rx1 Filter NumOf Taps not as expected");


            Int16[] coefs = new Int16[48];
            for (int i = 0; i < 48; i++)
            {
                Link.spiWrite(0xE01, (byte)(i * 2));
                firLSB = Link.spiRead(0xE00);
                Link.spiWrite(0xE01, (byte)(i * 2 + 1));
                firMSB = Link.spiRead(0xE00);

                coefs[i] = (Int16)(((UInt16)(firMSB) << 8) | (UInt16)(firLSB));
                Console.WriteLine("Coef[" + i + "]: " + coefs[i]);
                Assert.AreEqual(RxFirCoeffs[i], coefs[i], "Myk: Rx1 FilterCoeff" + i.ToString("d") + "not as expected");
            }
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckRxGainTable
        /// API Under-Test: 
        ///     MYKONOS_programRxGainTable
        /// API Test Description: 
        ///     Call MYKONOS_programFir to set RX1 
        ///     MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv 
        ///    Ref RxGainIndex for details.
        /// API Test Pass Criteria: 
        ///     Readback Gain Table data and check the values 
        ///     are as expected.
        /// Notes
        ///   MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv
        ///   has 72 Indices
        ///</summary>
        [Test]
        public static void CheckRxCh1GainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.programRxGainTable(settings.resourcePath + @"GainTables\" + "MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv", Mykonos.RXGAIN_TABLE.RX1_GT);

            //Turn Radio On in order to read back Gain Table
            Link.Mykonos.radioOn();

            byte indexAddr = 0;
            byte spiData = 0;
            byte index = 0;
            byte feGain = 0;
            byte extControl = 0;
            byte digGain = 0;
            byte enableAtten = 0;

            //Select Gain Table from which to read.
            //Set Gain Read/Write mode to read.
            Link.spiWrite(0x516, 0xC);



            for (byte i = 255; i > (255 - 72); i--)
            {
                //Get Gain Table Index (WordAddress)
                //Read Currently Selected Gain Index
                //Read Currently Select Gain Index Rx Front Gain
                //Read Currently Selected External Control
                //Read Digital Gain and Attenaution status
                Link.spiWrite(0x500, i);
                indexAddr = Link.spiRead(0x500);
                Assert.AreEqual(i, indexAddr, "Myk: Gain Index" + i.ToString("d") + "not as expected");


                index = Link.spiRead(0x50A);
                feGain = Link.spiRead(0x50B);
                extControl = Link.spiRead(0x50C);
                spiData = Link.spiRead(0x50D);
                digGain = (byte)(spiData & 0x7F);
                enableAtten = (byte)(spiData >> 7);


                Console.WriteLine(i + "," + indexAddr + "," + index + ", " + feGain + ", " + extControl + ", " + digGain + ", " + enableAtten);

                Assert.AreEqual(i, index, "Myk: Gain Index" + i.ToString("d") + "not as expected");
                Assert.AreEqual(RxGainIndex[255 - i, 0], feGain, "Myk: FE Gain" + i.ToString("d") + "not as expected");
                Assert.AreEqual(RxGainIndex[255 - i, 1], extControl, "Myk:extControl" + i.ToString("d") + "not as expected");
                Assert.AreEqual(RxGainIndex[255 - i, 2], digGain, "Myk:digGain" + i.ToString("d") + "not as expected");
                Assert.AreEqual(RxGainIndex[255 - i, 3], enableAtten, "Myk:enableAtten" + i.ToString("d") + "not as expected");
            }

            //Disable Readback of Rx1 Table
            Link.spiWrite(0x516, 0x08);
            Link.Mykonos.radioOff();
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckSetRxGainModeManual
        /// API Under-Test: 
        ///     MYKONOS_setRxGainControlMode
        /// API Test Description: 
        ///     Call MYKONOS_setRxGainControlMode to set 
        ///     RX1 & RX2 data path Gain mode to Manual Mode
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that Manual Gain Mode 
        ///     is set on both RX1 and RX2 datapaths.
        ///</summary>
        [Test]
        public static void CheckSetRxGainModeManual()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.MGC);

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x42E);
            Console.WriteLine("SPI Addr: 0x42E:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0xF), "Myk: Manual Mode not Set");

            Link.Disconnect();
        }
        /// API Test Name: 
        ///     CheckSetRxGainModeAGC
        /// API Under-Test: 
        ///     MYKONOS_setRxGainControlMode
        /// API Test Description: 
        ///     Call MYKONOS_setRxGainControlMode to set 
        ///     RX1 & RX2 data path Gain mode to AGC Mode
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that AGC Mode 
        ///     is set on both RX1 and RX2 datapaths.
        ///</summary>
        [Test]
        public static void CheckSetRxGainModeAGC()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.AGC);

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x42E);
            Console.WriteLine("SPI Addr: 0x42E:" + spiData1.ToString("X"));
            Assert.AreEqual(0xA, (spiData1 & 0xF), "Myk: AGC Mode not Set");

            Link.Disconnect();
        }

        /// API Test Name: 
        ///     CheckSetRx1GainManualGain
        /// API Under-Test: 
        ///     MYKONOS_setRx1ManualGain
        ///     MYKONOS_getRx1Gain
        /// API Test Description: 
        ///     Call MYKONOS_setRxGainControlMode to set 
        ///     RX1 & RX2 data path Gain mode to Manual Mode
        ///     Call MYKONOS_setRx1ManualGain to Gain Index 0xBA
        ///     Call MYKONOS_getRx1Gain
        ///     See Notes below for details
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that 
        ///     Manual Mode for Rx1 is Enabled
        ///     SPI Manual Gain Control Mode is Enabled
        ///     That select Gain index is 0xBA
        ///     Check that MYKONOS_getRx1Gain return 
        ///     Gain Index value of 0xBA 
        ///</summary>
        [Test]
        public static void CheckSetRx1GainManualGain()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.programRxGainTable(settings.resourcePath + @"GainTables\" + "MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv", Mykonos.RXGAIN_TABLE.RX1_GT);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.MGC);
            Link.Mykonos.setRx1ManualGain(186);
            Link.Mykonos.radioOn();

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x42E);
            Console.WriteLine("SPI Addr: 0x42E:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0xF), "Myk: Manual Mode not Set");
            //Check Gain SPI Mode Control via SPI Read
            spiData1 = Link.spiRead(0x433);
            Console.WriteLine("SPI Addr: 0x433:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0x1), "Myk: Gain SPI Mode Control not Set");

            //Check Gain SPI Mode Control via SPI Read
            spiData1 = Link.spiRead(0x435);
            Console.WriteLine("SPI Addr: 0x435:" + spiData1.ToString("X"));
            Assert.AreEqual(0xBA, (spiData1 & 0xFF), "Myk: Rx1 Gain Index Not as Expected");

            //API Gain Readback
            byte rx1Index = 0x0;
            Link.Mykonos.getRx1Gain(ref rx1Index);
            Assert.AreEqual(0xBA, rx1Index, "Myk:Rx1 Gain Index From Readback Not as Expected");


            //Check current Rx Ch1 Gain Index Data
            byte OgainCh1 = 0;
            OgainCh1 = (byte)Link.spiRead(0x4B0);
            Assert.AreEqual(0xBA, OgainCh1, "Myk:Rx1 Gain CH1 Index From Readback Not as Expected");

            Link.Mykonos.setRx1ManualGain(196);
            Link.Mykonos.radioOff();
            Link.Disconnect();
        }
        /// <summary>
        /// API Test Name: 
        ///     CheckSetRx2GainManualGain
        /// API Under-Test: 
        ///     MYKONOS_setRx2ManualGain
        ///     MYKONOS_getRx2Gain
        /// API Test Description: 
        ///     Call MYKONOS_setRxGainControlMode to set 
        ///     RX1 & RX2 data path Gain mode to Manual Mode
        ///     Call MYKONOS_setRx2ManualGain to Gain Index 0xC8
        ///     Call MYKONOS_getRx2Gain
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that 
        ///     Manual Mode for Rx1 is Enabled
        ///     SPI Manual Gain Control Mode is Enabled
        ///     That select Gain index is 0xC8
        ///     Check that MYKONOS_getRx1Gain return 
        ///     Gain Index value of 0xC8
        ///</summary>
        [Test]
        public static void CheckSetRx2GainManualGain()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.programRxGainTable(settings.resourcePath + @"GainTables\" + "MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv", Mykonos.RXGAIN_TABLE.RX2_GT);
            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.MGC);
            Link.Mykonos.setRx2ManualGain(200);
            Link.Mykonos.radioOn();

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x42E);
            Console.WriteLine("SPI Addr: 0x42E:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0xF), "Myk: Manual Mode not Set");
            //Check Gain SPI Mode Control via SPI Read
            spiData1 = Link.spiRead(0x433);
            Console.WriteLine("SPI Addr: 0x433:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0x1), "Myk: Gain SPI Mode Control not Set");

            //Check Gain SPI Mode Control via SPI Read
            spiData1 = Link.spiRead(0x436);
            Console.WriteLine("SPI Addr: 0x436:" + spiData1.ToString("X"));
            Assert.AreEqual(0xC8, (spiData1 & 0xFF), "Myk: Rx2 Gain Index Not as Expected");

            //API Gain Readback
            byte rx2Index = 0x0;
            Link.Mykonos.getRx2Gain(ref rx2Index);
            Assert.AreEqual(0xC8, rx2Index, "Myk:Rx2 Gain Index From Readback Not as Expected");

            //Check current Gain Index Data

            byte OgainCh2 = 0;
            OgainCh2 = (byte)Link.spiRead(0x4B3);
            Assert.AreEqual(0xC8, OgainCh2, "Myk:Rx2 Gain CH2 Index From Readback Not as Expected");

            Link.Mykonos.radioOff();

            Link.Disconnect();
        }
        /// <summary>
        /// API Test Name: 
        ///     CheckenableRxGainCtrSyncPulse
        /// API Under-Test: 
        ///     MYKONOS_enableRxGainCtrSyncPulse
        /// API Test Description: 
        ///     Call MYKONOS_enableRxGainCtrSyncPulse
        /// API Test Pass Criteria: 
        ///   Readback Via SPI GainCtrlSynPulse Enable Status
        ///</summary>
        [Test, Sequential]
        public static void CheckenableRxGainCtrSyncPulse([Values(0x1, 0x0)]Byte enable)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.EnableRxGainCtrSyncPulse(enable);
            byte spiData1 = 0x0;
            spiData1 = Link.spiRead(0x48E);
            Console.WriteLine(spiData1);
            if (enable == 0x0)
                Assert.AreEqual(0x0, (spiData1 >> 7), "Myk: Disable GainCtrSync Pulse Failed");
            else
                Assert.AreEqual(0x1, (spiData1 >> 7), "Myk: Enable GainCtrSync Pulse Failed");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetRxFramerDataSource
        /// API Under-Test: 
        ///     MYKONOS_setRxFramerDataSource
        /// API Test Description: 
        ///  Use MYKONOS_setRxFramerDataSource to setup dataSource 
        ///  Parameter. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        ///</summary>
        ///
        [Test]
        [Category("JESD")]
        public static void CheckSetRxFramerDataSource([Values(0x0, 0x1)]byte dataSource)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;
            Link.Mykonos.setRxFramerDataSource(dataSource);

            spiData1 = Link.spiRead(0x063); Console.WriteLine("0x063: " + spiData1.ToString("X"));
            Assert.AreEqual(dataSource, ((spiData1 & 0x10) >> 4), "Loopback not set up as expected");

            Debug.WriteLine(Link.Mykonos.checkPllsLockStatus().ToString("X"));
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetRxFramerDataSource
        /// API Under-Test: 
        ///     MYKONOS_setRxFramerDataSource
        /// API Test Description: 
        ///  Use MYKONOS_setRxFramerDataSource to setup dataSource 
        ///  Parameter. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        ///</summary>
        ///
       // [Test]
        [Category("JESD")]
        public static void CheckRFDCSetHigh()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;

            spiData1 = Link.spiRead(0x635); Console.WriteLine("0x635: " + spiData1.ToString("X"));
            Assert.AreEqual(3, ((spiData1 & 0xE0) >> 5), "RFDC not set high");
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetupRxAGC
        /// API Under-Test: 
        ///     CMDMYKONOS_initRxAgcStruct
        ///     CMDMYKONOS_initRxPeakAgcStruct
        ///     CMDMYKONOS_initRxPwrAgcStruct
        ///     MYKONOS_setupRxAgc
        /// API Test Description: 
        ///  Use CMDMYKONOS_initRxAgcStruct, CMDMYKONOS_initRxPeakAgcStruct,
        ///  and CMDMYKONOS_initRxPwrAgcStruct to write parameter values to the 
        ///  AGC structure. Call MYKONOS_setupRxAgc to write the values in the struct
        ///  to the corresponding registers.
        ///  Check the SPI registers for correct configuration
        ///  Check init function readbacks for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the spi readback and the function readback values 
        ///  match the set values
        ///</summary>
        ///
        [Test]
        [Category("JESD")]
        public static void CheckSetupRxAGC()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            // Copied default values from Kevin's code
            // All the values can go into the three structures
            byte spiData = 0x0;
            // mykonosAgcCfg_t
            byte agcRx1MaxGainIndex = 250;
            byte agcRx1MinGainIndex = 198;
            byte agcRx2MaxGainIndex = 250;
            byte agcRx2MinGainIndex = 198;
            byte agcObsRxMaxGainIndex = 0;
            byte agcObsRxMinGainIndex = 0;
            byte agcObsRxSelect = 0;
            byte agcPeakThresholdMode = 1; // Change for power only mode
            byte agcLowThsPreventGainIncrease =1; // Change for power only mode
            UInt32 agcGainUpdateCounter = 30721;
            byte agcSlowLoopSettlingDelay = 1;
            byte agcPeakWaitTime = 5;
            byte pmdMeasDuration = 0x07;
            byte pmdMeasConfig = 0x3;
            byte agcResetOnRxEnable = 0;
            byte agcEnableSyncPulseForGainCounter =1;

            // mykonosPowerMeasAgcCfg_t
            byte pmdUpperHighThresh = 0x02; // Triggered at approx -2dBFS
            byte pmdUpperLowThresh = 0x04;
            byte pmdLowerHighThresh = 0x0C;
            byte pmdLowerLowThresh = 0x05;
            byte pmdUpperHighGainStepAttack = 0x05;
            byte pmdUpperLowGainStepAttack = 0x01;
            byte pmdLowerHighGainStepRecovery = 0x01;
            byte pmdLowerLowGainStepRecovery = 0x05;

            // mykonosPeakDetAgcCfg_t
            byte apdHighThresh = 0x1E; //Triggered at approx -3dBFS
            byte apdLowThresh = 0x17; //Triggered at approx -5.5dBFS
            byte hb2HighThresh = 0xB6; // Triggered at approx -2.18dBFS
            byte hb2LowThresh = 0x81; // Triggered at approx -5.5dBFS
            byte hb2VeryLowThresh = 0x41; // Triggered at approx -9dBFS
            byte apdHighThreshExceededCnt = 0x0B;
            byte apdLowThreshExceededCnt = 0x04;
            byte hb2HighThreshExceededCnt = 0x0B;
            byte hb2LowThreshExceededCnt = 0x04;
            byte hb2VeryLowThreshExceededCnt = 0x04;
            byte apdHighGainStepAttack = 0x01;
            byte apdLowGainStepRecovery = 0x01;
            byte hb2HighGainStepAttack = 0x01;
            byte hb2LowGainStepRecovery = 0x01;
            byte hb2VeryLowGainStepRecovery = 0x01;
            byte apdFastAttack = 0;
            byte hb2FastAttack = 0;
            byte hb2OverloadDetectEnable = 0;
            byte hb2OverloadDurationCnt = 5;
            byte hb2OverloadThreshCnt = 0x9;

            // Write some values into the structure 
            Link.Mykonos.init_rxAgcStructure(1, ref agcRx1MaxGainIndex,
                                                ref agcRx1MinGainIndex,
                                                ref agcRx2MaxGainIndex,
                                                ref agcRx2MinGainIndex,
                                                ref agcObsRxMaxGainIndex,
                                                ref agcObsRxMinGainIndex,
                                                ref agcObsRxSelect,
                                                ref agcPeakThresholdMode,
                                                ref agcLowThsPreventGainIncrease,
                                                ref agcGainUpdateCounter,
                                                ref agcSlowLoopSettlingDelay,
                                                ref agcPeakWaitTime,
                                                ref agcResetOnRxEnable,
                                                ref agcEnableSyncPulseForGainCounter);

            Link.Mykonos.init_rxPwrAgcStructure(1, ref pmdUpperHighThresh,
                                                   ref pmdUpperLowThresh,
                                                   ref pmdLowerHighThresh,
                                                   ref pmdLowerLowThresh,
                                                   ref pmdUpperHighGainStepAttack,
                                                   ref pmdUpperLowGainStepAttack,
                                                   ref pmdLowerHighGainStepRecovery,
                                                   ref pmdLowerLowGainStepRecovery, ref pmdMeasDuration,
                                                   ref pmdMeasConfig);

            Link.Mykonos.init_rxPeakAgcStructure(1, ref apdHighThresh,
                                                    ref apdLowThresh,
                                                    ref hb2HighThresh,
                                                    ref hb2LowThresh,
                                                    ref hb2VeryLowThresh,
                                                    ref apdHighThreshExceededCnt,
                                                    ref apdLowThreshExceededCnt,
                                                    ref hb2HighThreshExceededCnt,
                                                    ref hb2LowThreshExceededCnt,
                                                    ref hb2VeryLowThreshExceededCnt,
                                                    ref apdHighGainStepAttack,
                                                    ref apdLowGainStepRecovery,
                                                    ref hb2HighGainStepAttack,
                                                    ref hb2LowGainStepRecovery,
                                                    ref hb2VeryLowGainStepRecovery,
                                                    ref apdFastAttack,
                                                    ref hb2FastAttack,
                                                    ref hb2OverloadDetectEnable,
                                                    ref hb2OverloadDurationCnt,
                                                    ref hb2OverloadThreshCnt);
            Link.Mykonos.setupRxAgc();


            byte read_agcRx1MaxGainIndex = 0;
            byte read_agcRx1MinGainIndex = 0;
            byte read_agcRx2MaxGainIndex = 0;
            byte read_agcRx2MinGainIndex = 0;
            byte read_agcObsRxMaxGainIndex = 0;
            byte read_agcObsRxMinGainIndex = 0;
            byte read_agcObsRxSelect = 0;
            byte read_agcPeakThresholdMode = 0; // Change for power only mode
            byte read_agcLowThsPreventGainIncrease = 0; // Change for power only mode
            UInt32 read_agcGainUpdateCounter = 0;
            byte read_agcSlowLoopSettlingDelay = 0;
            byte read_agcPeakWaitTime = 0;
            byte read_pmdMeasDuration = 0x0;
            byte read_pmdMeasConfig = 0x0;
            byte read_agcResetOnRxEnable = 0;
            byte read_agcEnableSyncPulseForGainCounter = 0;

            // mykonosPowerMeasAgcCfg_t
            byte read_pmdUpperHighThresh = 0x0; // Triggered at approx -2dBFS
            byte read_pmdUpperLowThresh = 0x0;
            byte read_pmdLowerHighThresh = 0x0;
            byte read_pmdLowerLowThresh = 0x0;
            byte read_pmdUpperHighGainStepAttack = 0x0;
            byte read_pmdUpperLowGainStepAttack = 0x00;
            byte read_pmdLowerHighGainStepRecovery = 0x00;
            byte read_pmdLowerLowGainStepRecovery = 0x0;

            // mykonosPeakDetAgcCfg_t
            byte read_apdHighThresh = 0x0; //Triggered at approx -3dBFS
            byte read_apdLowThresh = 0x0; //Triggered at approx -5.5dBFS
            byte read_hb2HighThresh = 0x0; // Triggered at approx -2.18dBFS
            byte read_hb2LowThresh = 0x0; // Triggered at approx -5.5dBFS
            byte read_hb2VeryLowThresh = 0x0; // Triggered at approx -9dBFS
            byte read_apdHighThreshExceededCnt = 0x0;
            byte read_apdLowThreshExceededCnt = 0x0;
            byte read_hb2HighThreshExceededCnt = 0x0;
            byte read_hb2LowThreshExceededCnt = 0x3;
            byte read_hb2VeryLowThreshExceededCnt = 0x0;
            byte read_apdHighGainStepAttack = 0x00;
            byte read_apdLowGainStepRecovery = 0x00;
            byte read_hb2HighGainStepAttack = 0x00;
            byte read_hb2LowGainStepRecovery = 0x00;
            byte read_hb2VeryLowGainStepRecovery = 0x00;
            byte read_apdFastAttack =01;
            byte read_hb2FastAttack = 0;
            byte read_hb2OverloadDetectEnable = 0;
            byte read_hb2OverloadDurationCnt = 0;
            byte read_hb2OverloadThreshCnt = 00;

            //Read the values
            Link.Mykonos.init_rxAgcStructure(0, ref read_agcRx1MaxGainIndex,
                                    ref read_agcRx1MinGainIndex,
                                    ref read_agcRx2MaxGainIndex,
                                    ref read_agcRx2MinGainIndex,
                                    ref read_agcObsRxMaxGainIndex,
                                    ref read_agcObsRxMinGainIndex,
                                    ref read_agcObsRxSelect,
                                    ref read_agcPeakThresholdMode,
                                    ref read_agcLowThsPreventGainIncrease,
                                    ref read_agcGainUpdateCounter,
                                    ref read_agcSlowLoopSettlingDelay,
                                    ref read_agcPeakWaitTime,
                                    ref read_agcResetOnRxEnable,
                                    ref read_agcEnableSyncPulseForGainCounter);

            Link.Mykonos.init_rxPwrAgcStructure(0, ref read_pmdUpperHighThresh,
                                                   ref read_pmdUpperLowThresh,
                                                   ref read_pmdLowerHighThresh,
                                                   ref read_pmdLowerLowThresh,
                                                   ref read_pmdUpperHighGainStepAttack,
                                                   ref read_pmdUpperLowGainStepAttack,
                                                   ref read_pmdLowerHighGainStepRecovery,
                                                   ref read_pmdLowerLowGainStepRecovery, ref read_pmdMeasDuration,
                                                   ref read_pmdMeasConfig);

            Link.Mykonos.init_rxPeakAgcStructure(0, ref read_apdHighThresh,
                                                    ref read_apdLowThresh,
                                                    ref read_hb2HighThresh,
                                                    ref read_hb2LowThresh,
                                                    ref read_hb2VeryLowThresh,
                                                    ref read_apdHighThreshExceededCnt,
                                                    ref read_apdLowThreshExceededCnt,
                                                    ref read_hb2HighThreshExceededCnt,
                                                    ref read_hb2LowThreshExceededCnt,
                                                    ref read_hb2VeryLowThreshExceededCnt,
                                                    ref read_apdHighGainStepAttack,
                                                    ref read_apdLowGainStepRecovery,
                                                    ref read_hb2HighGainStepAttack,
                                                    ref read_hb2LowGainStepRecovery,
                                                    ref read_hb2VeryLowGainStepRecovery,
                                                    ref read_apdFastAttack,
                                                    ref read_hb2FastAttack,
                                                    ref read_hb2OverloadDetectEnable,
                                                    ref read_hb2OverloadDurationCnt,
                                                    ref read_hb2OverloadThreshCnt);
            //Compare set values to function return values and register readback
            spiData = Link.spiRead(0x431); Console.WriteLine("SPI Addr: 0x431:" + spiData.ToString("X"));
            Assert.AreEqual(agcRx1MaxGainIndex, spiData, "Register readback for agcRx1MaxGainIndex incorrect");
            Assert.AreEqual(agcRx1MaxGainIndex, read_agcRx1MaxGainIndex, "Function readback for agcRx1MaxGainIndex incorrect");

            spiData = Link.spiRead(0x432); Console.WriteLine("SPI Addr: 0x432:" + spiData.ToString("X"));
            Assert.AreEqual(agcRx1MinGainIndex, spiData, "Register readback for agcRx1MinGainIndex incorrect");
            Assert.AreEqual(agcRx1MinGainIndex, read_agcRx1MinGainIndex, "Function readback for agcRx1MinGainIndex incorrect");

            spiData = Link.spiRead(0x446); Console.WriteLine("SPI Addr: 0x445:" + spiData.ToString("X"));
            Assert.AreEqual(agcRx2MaxGainIndex, spiData, "Register readback for agcRx1MaxGainIndex incorrect");
            Assert.AreEqual(agcRx2MaxGainIndex, read_agcRx2MaxGainIndex, "Function readback for agcRx2MaxGainIndex incorrect");

            spiData = Link.spiRead(0x447); Console.WriteLine("SPI Addr: 0x446:" + spiData.ToString("X"));
            Assert.AreEqual(agcRx2MinGainIndex, spiData, "Register readback for agcRx2MinGainIndex incorrect");
            Assert.AreEqual(agcRx2MinGainIndex, read_agcRx2MinGainIndex, "Function readback for agcRx2MinGainIndex incorrect");
            /*
            spiData = Link.spiRead(0x449); Console.WriteLine("SPI Addr: 0x449:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxMaxGainIndex, spiData, "Register readback for agcObsRxMaxGainIndex incorrect");
            Assert.AreEqual(agcObsRxMaxGainIndex, read_agcObsRxMaxGainIndex, "Function readback for agcObsRxMaxGainIndex incorrect");

            spiData = Link.spiRead(0x44A); Console.WriteLine("SPI Addr: 0x44A:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxMinGainIndex, spiData, "Register readback for agcObsRxMinGainIndex incorrect");
            Assert.AreEqual(agcObsRxMinGainIndex, read_agcObsRxMinGainIndex, "Function readback for agcObsRxMinGainIndex incorrect");
            

            spiData = Link.spiRead(0x460); Console.WriteLine("SPI Addr: 0x460:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxSelect, spiData, "Register readback for agcObsRxSelect incorrect");
            Assert.AreEqual(agcObsRxSelect, read_agcObsRxSelect, "Function readback for agcObsRxSelect incorrect");

            spiData = Link.spiRead(0x460); Console.WriteLine("SPI Addr: 0x460:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxSelect, spiData, "Register readback for agcObsRxSelect incorrect");
            Assert.AreEqual(agcObsRxSelect, read_agcObsRxSelect, "Function readback for agcObsRxSelect incorrect");*/

            spiData = Link.spiRead(0x48A); Console.WriteLine("SPI Addr: 0x48A:" + spiData.ToString("X"));
            Assert.AreEqual(agcPeakThresholdMode, (spiData >> 5) & 0x01, "Register readback for agcPeakThresholdMode incorrect");
            Assert.AreEqual(agcPeakThresholdMode, read_agcPeakThresholdMode, "Function readback for agcPeakThresholdMode incorrect");

            spiData = Link.spiRead(0x480); Console.WriteLine("SPI Addr: 0x480:" + spiData.ToString("X"));
            Assert.AreEqual(agcLowThsPreventGainIncrease, (spiData >> 7) & 0x01, "Register readback for agcLowThsPreventGainIncrease incorrect");
            Assert.AreEqual(agcLowThsPreventGainIncrease, read_agcLowThsPreventGainIncrease, "Function readback for agcLowThsPreventGainIncrease incorrect");

            spiData = Link.spiRead(0x48B); Console.WriteLine("SPI Addr: 0x48B:" + spiData.ToString("X"));
            Assert.AreEqual(agcGainUpdateCounter & 0xFF, spiData, "Register readback for agcGainUpdateCounter[7:0] incorrect");
            Assert.AreEqual(agcGainUpdateCounter, read_agcGainUpdateCounter, "Function readback for agcGainUpdateCounter incorrect");

            spiData = Link.spiRead(0x48C); Console.WriteLine("SPI Addr: 0x48C:" + spiData.ToString("X"));
            Assert.AreEqual((agcGainUpdateCounter >> 8) & 0xFF, spiData, "Register readback for agcGainUpdateCounter[15:8] incorrect");

            spiData = Link.spiRead(0x48D); Console.WriteLine("SPI Addr: 0x48D:" + spiData.ToString("X"));
            Assert.AreEqual((agcGainUpdateCounter >> 16) & 0x3F, spiData & 0x3F, "Register readback for agcGainUpdateCounter[21:16] incorrect");

            spiData = Link.spiRead(0x48E); Console.WriteLine("SPI Addr: 0x48E:" + spiData.ToString("X"));
            Assert.AreEqual(agcSlowLoopSettlingDelay, spiData & 0x7F, "Register readback for agcSlowLoopSettlingDelay incorrect");
            Assert.AreEqual(agcSlowLoopSettlingDelay, read_agcSlowLoopSettlingDelay, "Function readback for agcSlowLoopSettlingDelay incorrect");

            spiData = Link.spiRead(0x42F); Console.WriteLine("SPI Addr: 0x42F:" + spiData.ToString("X"));
            Assert.AreEqual(agcPeakWaitTime, spiData & 0x1F, "Register readback for agcPeakWaitTime incorrect");
            Assert.AreEqual(agcPeakWaitTime, read_agcPeakWaitTime, "Function readback for agcPeakWaitTime incorrect");

            spiData = Link.spiRead(0x48D); Console.WriteLine("SPI Addr: 0x48D:" + spiData.ToString("X"));
            Assert.AreEqual(agcResetOnRxEnable, (spiData >> 7) & 0x01, "Register readback for agcResetOnRxEnable incorrect");
            Assert.AreEqual(agcResetOnRxEnable, read_agcResetOnRxEnable, "Function readback for agcResetOnRxEnable incorrect");

            spiData = Link.spiRead(0x48E); Console.WriteLine("SPI Addr: 0x48E:" + spiData.ToString("X"));
            Assert.AreEqual(agcEnableSyncPulseForGainCounter, (spiData >> 7) & 0x01, "Register readback for agcEnableSyncPulseForGainCounter incorrect");
            Assert.AreEqual(agcEnableSyncPulseForGainCounter, read_agcEnableSyncPulseForGainCounter, "Function readback for agcEnableSyncPulseForGainCounter incorrect");

            spiData = Link.spiRead(0x48F); Console.WriteLine("SPI Addr: 0x480:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperHighThresh, (spiData >> 4) & 0x0F, "Register readback for pmdUpperHighThresh incorrect");
            Assert.AreEqual(pmdUpperHighThresh, read_pmdUpperHighThresh, "Function readback for pmdUpperHighThresh incorrect");

            spiData = Link.spiRead(0x437); Console.WriteLine("SPI Addr: 0x437:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperLowThresh, (spiData >> 0) & 0x0F, "Register readback for pmdUpperLowThresh incorrect");
            Assert.AreEqual(pmdUpperLowThresh, read_pmdUpperLowThresh, "Function readback for pmdUpperLowThresh incorrect");

            spiData = Link.spiRead(0x480); Console.WriteLine("SPI Addr: 0x437:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerHighThresh, (spiData >> 0) & 0x7F, "Register readback for pmdLowerHighThresh incorrect");
            Assert.AreEqual(pmdLowerHighThresh, read_pmdLowerHighThresh, "Function readback for pmdLowerHighThresh incorrect");

            spiData = Link.spiRead(0x48F); Console.WriteLine("SPI Addr: 0x48F:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerLowThresh, (spiData >> 0) & 0x0F, "Register readback for pmdLowerLowThresh incorrect");
            Assert.AreEqual(pmdLowerLowThresh, read_pmdLowerLowThresh, "Function readback for pmdLowerLowThresh incorrect");

            spiData = Link.spiRead(0x489); Console.WriteLine("SPI Addr: 0x489:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperHighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for pmdUpperHighGainStepAttack incorrect");
            Assert.AreEqual(pmdUpperHighGainStepAttack, read_pmdUpperHighGainStepAttack, "Function readback for pmdUpperHighGainStepAttack incorrect");

            spiData = Link.spiRead(0x487); Console.WriteLine("SPI Addr: 0x487:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperLowGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for pmdUpperLowGainStepAttack incorrect");
            Assert.AreEqual(pmdUpperLowGainStepAttack, read_pmdUpperLowGainStepAttack, "Function readback for pmdUpperLowGainStepAttack incorrect");

            spiData = Link.spiRead(0x488); Console.WriteLine("SPI Addr: 0x488:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerHighGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for pmdLowerHighGainStepRecovery incorrect");
            Assert.AreEqual(pmdLowerHighGainStepRecovery, read_pmdLowerHighGainStepRecovery, "Function readback for pmdLowerHighGainStepRecovery incorrect");

            spiData = Link.spiRead(0x48A); Console.WriteLine("SPI Addr: 0x48A:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for pmdLowerLowGainStepRecovery incorrect");
            Assert.AreEqual(pmdLowerLowGainStepRecovery, read_pmdLowerLowGainStepRecovery, "Function readback for pmdLowerLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x4C8); Console.WriteLine("SPI Addr: 0x4C8:" + spiData.ToString("X"));
            Assert.AreEqual(pmdMeasDuration, (spiData >> 0) & 0x0F, "Register readback for pmdMeasDuration incorrect");
            Assert.AreEqual(pmdMeasDuration, read_pmdMeasDuration, "Function readback for pmdMeasDuration incorrect");

            spiData = Link.spiRead(0x4C7); Console.WriteLine("SPI Addr: 0x4C7:" + spiData.ToString("X"));
            if (spiData == 0)
            { }
            else if (spiData == 0x03)
                spiData = 1;
            else if (spiData == 0x05)
                spiData = 2;
            else if (spiData == 0x11)
                spiData = 3;
            else
                Assert.Fail("Invalid readback for pmdMeasConfig");

            Assert.AreEqual(pmdMeasConfig, (spiData >> 0) & 0x0F, "Register readback for pmdMeasConfig incorrect");
            Assert.AreEqual(pmdMeasConfig, read_pmdMeasConfig, "Function readback for pmdMeasConfig incorrect");

            spiData = Link.spiRead(0x442); Console.WriteLine("SPI Addr: 0x4C8:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighThresh, (spiData >> 0) & 0x3F, "Register readback for apdHighThresh incorrect");
            Assert.AreEqual(apdHighThresh, read_apdHighThresh, "Function readback for apdHighThresh incorrect");

            spiData = Link.spiRead(0x443); Console.WriteLine("SPI Addr: 0x443:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowThresh, (spiData >> 0) & 0x3F, "Register readback for apdLowThresh incorrect");
            Assert.AreEqual(apdLowThresh, read_apdLowThresh, "Function readback for apdLowThresh incorrect");

            spiData = Link.spiRead(0x584); Console.WriteLine("SPI Addr: 0x584:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighThresh, (spiData >> 0) & 0xFF, "Register readback for hb2HighThresh incorrect");
            Assert.AreEqual(hb2HighThresh, read_hb2HighThresh, "Function readback for hb2HighThresh incorrect");

            spiData = Link.spiRead(0x585); Console.WriteLine("SPI Addr: 0x585:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowThresh, (spiData >> 0) & 0xFF, "Register readback for hb2LowThresh incorrect");
            Assert.AreEqual(hb2LowThresh, read_hb2LowThresh, "Function readback for hb2LowThresh incorrect");

            spiData = Link.spiRead(0x586); Console.WriteLine("SPI Addr: 0x586:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowThresh, (spiData >> 0) & 0xFF, "Register readback for hb2VeryLowThresh incorrect");
            Assert.AreEqual(hb2VeryLowThresh, read_hb2VeryLowThresh, "Function readback for hb2VeryLowThresh incorrect");

            spiData = Link.spiRead(0x481); Console.WriteLine("SPI Addr: 0x481:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for apdHighThreshExceededCnt incorrect");
            Assert.AreEqual(apdHighThreshExceededCnt, read_apdHighThreshExceededCnt, "Function readback for apdHighThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x482); Console.WriteLine("SPI Addr: 0x482:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for apdLowThreshExceededCnt incorrect");
            Assert.AreEqual(apdLowThreshExceededCnt, read_apdLowThreshExceededCnt, "Function readback for apdLowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x483); Console.WriteLine("SPI Addr: 0x483:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2HighThreshExceededCnt incorrect");
            Assert.AreEqual(hb2HighThreshExceededCnt, read_hb2HighThreshExceededCnt, "Function readback for hb2HighThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x484); Console.WriteLine("SPI Addr: 0x484:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2LowThreshExceededCnt incorrect");
            Assert.AreEqual(hb2LowThreshExceededCnt, read_hb2LowThreshExceededCnt, "Function readback for hb2LowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x485); Console.WriteLine("SPI Addr: 0x485:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2VeryLowThreshExceededCnt incorrect");
            Assert.AreEqual(hb2VeryLowThreshExceededCnt, read_hb2VeryLowThreshExceededCnt, "Function readback for hb2VeryLowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x438); Console.WriteLine("SPI Addr: 0x438:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for apdHighGainStepAttack incorrect");
            Assert.AreEqual(apdHighGainStepAttack, read_apdHighGainStepAttack, "Function readback for apdHighGainStepAttack incorrect");

            spiData = Link.spiRead(0x43B); Console.WriteLine("SPI Addr: 0x43B:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for apdLowGainStepRecovery incorrect");
            Assert.AreEqual(apdLowGainStepRecovery, read_apdLowGainStepRecovery, "Function readback for apdLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x439); Console.WriteLine("SPI Addr: 0x439:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for hb2HighGainStepAttack incorrect");
            Assert.AreEqual(hb2HighGainStepAttack, read_hb2HighGainStepAttack, "Function readback for hb2HighGainStepAttack incorrect");

            spiData = Link.spiRead(0x43C); Console.WriteLine("SPI Addr: 0x43C:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for hb2LowGainStepRecovery incorrect");
            Assert.AreEqual(hb2LowGainStepRecovery, read_hb2LowGainStepRecovery, "Function readback for hb2LowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x43D); Console.WriteLine("SPI Addr: 0x43D:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for hb2VeryLowGainStepRecovery incorrect");
            Assert.AreEqual(hb2VeryLowGainStepRecovery, read_hb2VeryLowGainStepRecovery, "Function readback for hb2VeryLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x48A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(apdFastAttack, (spiData >> 7) & 0x01, "Register readback for apdFastAttack incorrect");
            Assert.AreEqual(apdFastAttack, read_apdFastAttack, "Function readback for apdFastAttack incorrect");

            spiData = Link.spiRead(0x48A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2FastAttack, (spiData >> 6) & 0x01, "Register readback for hb2FastAttack incorrect");
            Assert.AreEqual(hb2FastAttack, read_hb2FastAttack, "Function readback for hb2FastAttack incorrect");

            spiData = Link.spiRead(0x583); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadDetectEnable, (spiData >> 7) & 0x01, "Register readback for hb2OverloadDetectEnable incorrect");
            Assert.AreEqual(hb2OverloadDetectEnable, read_hb2OverloadDetectEnable, "Function readback for hb2OverloadDetectEnable incorrect");

            spiData = Link.spiRead(0x583); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadDurationCnt, (spiData >> 4) & 0x07, "Register readback for hb2OverloadDurationCnt incorrect");
            Assert.AreEqual(hb2OverloadDurationCnt, read_hb2OverloadDurationCnt, "Function readback for hb2OverloadDurationCnt incorrect");

            spiData = Link.spiRead(0x583); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadThreshCnt, (spiData >> 0) & 0x0F, "Register readback for hb2OverloadThreshCnt incorrect");
            Assert.AreEqual(hb2OverloadThreshCnt, read_hb2OverloadThreshCnt, "Function readback for hb2OverloadThreshCnt incorrect");
            Link.Disconnect();
        }
    }
}