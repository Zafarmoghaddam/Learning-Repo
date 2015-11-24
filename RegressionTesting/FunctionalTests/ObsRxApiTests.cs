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
    //Add Test for MYKONOS_setupObsRxAgc
    [TestFixture]
    [Category("ApiFunctional")]
    public class ObsRxApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public const byte GetCmd = 0x0C;
        public const byte OrxModeObjId = 0x61;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static Int16[] ObsRxFirCoeffs = new Int16[48] {  
                       4,     2,   -18,   -36,     -4,   86, 
                     120,   -30,  -276,  -270,    192,  692,
                     442,  -666, -1444,  -488,   1782,  2710,
                      86,  -4412, -5352,  1526, 14058, 24064, 
                   24064,  14058,  1526, -5352, -4412,    86, 
                    2710,   1782,  -488, -1444,  -666,   442,
                     692,    192,  -270,  -276,   -30,   120,
                      86,     -4,   -36,   -18,     2,     4};
        public static byte [,] ObsRxGainIndex = new byte[20, 4] {
                            {0,	 0,	0,	0}, {7,	 0,	0,	0}, 
                            {13, 0,	1,	1}, {18, 0,	3,	1}, 
                            {23, 0,	3,	1}, {28, 0,	0,	0},
                            {32, 0,	0,	0}, {35, 0,	2,	1}, 
                            {38, 0,	4,	1}, {41, 0,	2,	1}, 
                            {0,	0,	0,	0}, {7,	 0,	0,	0},
                            {13, 0,	1,	1}, {18,0,	3,	1}, 
                            {23, 0,	3,	1}, {28, 0,	0,	0}, 
                            {32,0,	0,	0}, {35, 0,	2,	1},
                            {38, 0,	4,	1}, {41,0,	2,	1} };
                         
                                
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to ObsRx Api Functional Tests
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// From Locally Stored Default Profile  @"..\..\..\..\mykonos_resources\"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: Rx 100MHz, IQrate 122.88MHz, Dec5
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
        public void ObsRxApiTestInit()
        {
            //Set Test Parameters
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
            Console.WriteLine("ObsRxApiFunctional Test Setup: Complete" );
        }
      

        ///<summary>
        /// API Test Name: 
        ///     CheckObsRxFirFilter
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
        public static void CheckObsRxFirFilter([Values(Mykonos.FIR.OBSRX_A_FIR, Mykonos.FIR.OBSRX_B_FIR)]Mykonos.FIR ObsRxFirMode)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte firMSB = 0;
            byte firLSB = 0;
            string ObsrxFirFile = settings.resourcePath + @"\DigitalFilters\" + "RxApp6_BW40_ADC1228p8_OR76p8.ftr";
            Link.Mykonos.programFir(ObsRxFirMode, ObsrxFirFile);

            //Select PFIR
            if (ObsRxFirMode == Mykonos.FIR.OBSRX_A_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xD0));
            }
            else if (ObsRxFirMode == Mykonos.FIR.OBSRX_B_FIR)
            {
                spiData1 = Link.spiRead(0xDFF);
                Link.spiWrite(0xDFF, (byte)(spiData1 & 0xE0));
            }
            else
                throw new ArgumentException();


            //Readback Filter data
            //Read Configured RX Gain
            //Read Read Number of Taps
            //Cross reference Coeffecients
            spiData1 = Link.spiRead(0x412); Console.WriteLine("ObsRx Filter Gain: " + spiData1.ToString("X"));
            if (ObsRxFirMode == Mykonos.FIR.OBSRX_A_FIR)
            {
                Assert.AreEqual(0x1, (spiData1 & 0x3), "Myk: Rx A Filter Gain: not as expected");
            }
            if (ObsRxFirMode == Mykonos.FIR.OBSRX_B_FIR)
            {
                Assert.AreEqual(0x1, ((spiData1 & 0x60) >> 5), "Myk: Rx B Filter Gain: not as expected");
            }

            spiData1 = Link.spiRead(0x410); Console.WriteLine("Myk: ObsRx Filter NumOf Taps: " + (spiData1).ToString("X"));
            if (ObsRxFirMode == Mykonos.FIR.OBSRX_A_FIR)
            {
                Assert.AreEqual(0x1, ((spiData1 & 0x6) >> 1), "Myk: ObsRx B Filter NumOf Taps not as expected");
            }
            if (ObsRxFirMode == Mykonos.FIR.OBSRX_B_FIR)
            {
                Assert.AreEqual(0x1, ((spiData1 & 0x18) >> 3), "Myk: ObsRx A Filter NumOf Taps not as expected");

            }

            Int16[] coefs = new Int16[48];
            for (int i = 0; i < 48; i++)
            {
                Link.spiWrite(0xE01, (byte)(i * 2));
                firLSB = Link.spiRead(0xE00);
                Link.spiWrite(0xE01, (byte)(i * 2 + 1));
                firMSB = Link.spiRead(0xE00);

                coefs[i] = (Int16)(((UInt16)(firMSB) << 8) | (UInt16)(firLSB));
                Console.WriteLine("Coef[" + i + "]: " + coefs[i]);
                Assert.AreEqual(ObsRxFirCoeffs[i], coefs[i], "Myk: ObsRx FilterCoeff" + i.ToString("d") + "not as expected");
            }
            Link.Disconnect();
        }

        ///<summary>
         /// API Test Name: 
         ///     CheckRxGainTable
         /// API Under-Test: 
         ///     MYKONOS_programRxGainTable
         /// API Test Description: 
        ///     Call MYKONOS_programRxGainTable to set ObsRx
         ///     orxGainTable_debug.csv 
         ///       Call radioOn()
         ///       Call setObsRxPathSource
        ///     See ObsRxGainIndex
         /// API Test Pass Criteria: 
         ///     Readback Gain Table data and check the values 
         ///     are as expected.
         ///</summary>
        [Test]
        public static void CheckObsRxChAGainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.programRxGainTable(settings.resourcePath + @"GainTables\" + "orxGainTable_debug.csv", Mykonos.RXGAIN_TABLE.ORX_GT);
            Link.Mykonos.radioOn();
            Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO);

            byte indexAddr = 0;
            byte spiData = 0;
            byte index = 0;
            byte feGain = 0;
            byte extControl = 0;
            byte digGain = 0;
            byte enableAtten = 0;
           
            //Select Gain Table from which to read.
            //Set Gain Read/Write mode to read.
            Link.spiWrite(0x516, 0x4);
            Link.spiWrite(0x517, 0xA);

            for (byte i = 255; i > (255-20); i--)
            {
                //Get Gain Table Index (WordAddress)
                //Read Currently Selected Gain Index
                //Read Currently Select Gain Index Rx Front Gain
                //Read Currently Selected External Control
                //Read Digital Gain and Attenaution status
                Link.spiWrite(0x500, i);
                indexAddr = Link.spiRead(0x500);
                Assert.AreEqual(i, indexAddr, "Myk: Gain Index" + i.ToString("d") + "not as expected");

                index = Link.spiRead(0x512);
                feGain = Link.spiRead(0x513);
                extControl = Link.spiRead(0x514);
                spiData = Link.spiRead(0x515);
                digGain = (byte)(spiData & 0x7F);
                enableAtten = (byte)(spiData >> 7);

                Console.WriteLine(i + "," + indexAddr + "," + index + ", " + feGain + ", " + extControl + ", " + digGain + ", " + enableAtten);

                //Assert.AreEqual(i, index, "Myk: Gain Index" + i.ToString("d") + "not as expected");
                //Assert.AreEqual(RxGainIndex[255-i, 0], feGain, "Myk: FE Gain" + i.ToString("d") + "not as expected");
                //Assert.AreEqual(RxGainIndex[255-i, 1], extControl, "Myk:extControl" + i.ToString("d") + "not as expected");
                //Assert.AreEqual(RxGainIndex[255-i, 2], digGain, "Myk:digGain" + i.ToString("d") + "not as expected");
                //Assert.AreEqual(RxGainIndex[255-i, 3], digGain, "Myk:enableAtten" + i.ToString("d") + "not as expected");
            }

            //Disable Readback of Rx1 Table
            Link.spiWrite(0x516, 0x08); 

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckSetObsRxGainModeManual
        /// API Under-Test: 
        ///     MYKONOS_setObsRxGainControlMode
        /// API Test Description: 
        ///     Call MYKONOS_setObsRxGainControlMode to set 
        ///     obsRx data path Gain mode to Manual Mode
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that Manual Gain Mode 
        ///     is for ObsRx
        ///</summary>
        [Test]
        public static void CheckSetObsRxGainModeManual()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.MGC);

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x4447);
            Console.WriteLine("SPI Addr: 0x447:" + spiData1.ToString("X"));
            Assert.AreEqual(0x0, (spiData1 & 0xF), "Myk: Manual Mode not Set");

            Link.Disconnect();
        }
        /// API Test Name: 
        ///     CheckSetObsRxGainModeAGC
        /// API Under-Test:ObsRxGainControlMode
        /// API Test Description: 
        ///     Call MYKONOS_setObsRxGainControlMode to set 
        ///     ObsRx to Data Path to AGC Mode
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that AGC Mode 
        ///     is set on ObsRx datapath.
        ///</summary>
        [Test]
        public static void CheckSetObsRxGainModeAGC()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.AGC);

            byte spiData1 = 0x0;
            //Check Gain Mode Status via SPI Read
            spiData1 = Link.spiRead(0x448);
            Console.WriteLine("SPI Addr: 0x448:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: AGC Mode not Set");

            Link.Disconnect();
        }

        /// API Test Name: 
        ///     CheckSetObsRxGainManualGain
        /// API Under-Test: 
        ///     MYKONOS_setObsRxManualGain
        ///     MYKONOS_getObsRx1Gain
        /// API Test Description: 
        ///     Call MYKONOS_setObsRxGainControlMode to set 
        ///     Obs Rx data path Gain mode to Manual Mode
        ///     Call MYKONOS_setObsRxManualGain to Gain Index 0x2D
        ///     Call MYKONOS_getObsRxGain
        ///     See Notes below for details
        /// API Test Pass Criteria: 
        ///     Check Via SPI Readback that 
        ///     Manual Mode for Rx1 is Enabled
        ///     SPI Manual Gain Control Mode is Enabled
        ///     That select Gain index is 0x2D
        ///     Check that MYKONOS_getObsRxGain return 
        ///     Gain Index value of 0x2D 
        ///</summary>
        [Test]
        public static void CheckSetObsGainManualGain()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.MGC);
            Link.Mykonos.setRx1ManualGain(245);

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
            Assert.AreEqual(0xF5, (spiData1 & 0xFF), "Myk: Rx1 Gain Index Not as Expected");


            Link.Disconnect();
        }
        
        /// <summary>
        /// API Test Name: 
        ///     CheckenableObsRxGainCtrSyncPulse
        /// API Under-Test: 
        ///     MYKONOS_enableRxGainCtrSyncPulse
        /// API Test Description: 
        ///     Call MYKONOS_enableObsRxGainCtrSyncPulse
        /// API Test Pass Criteria: 
        ///   Readback Via SPI GainCtrlSynPulse Enable Status - Disabled/Enabled
        ///   
        ///</summary>
        [Test, Sequential]
        public static void CheckenablObseRxGainCtrSyncPulse([Values(0x1,0x0)]Byte enable )
        {
           
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.EnableRxGainCtrSyncPulse(enable);
            byte spiData1 = 0x0;
            spiData1 = Link.spiRead(0x48E);
            if(enable == 0x0)
                Assert.AreEqual(0x0, (spiData1 >> 7), "Myk: Disable GainCtrSync Pulse Failed");
            else
                Assert.AreEqual(0x1, (spiData1 >> 7), "Myk: Enable GainCtrSync Pulse Failed");

            Link.Disconnect();
        }

        /// <summary>
        /// API Test Name: 
        ///     CheckObsRxPathSource
        /// API Under-Test: 
        ///     MYKONOS_setObsRxPathSource
        /// API Test Description: 
        ///     Enable Radio On
        ///     Call MYKONOS_setObsRxPathSource
        /// API Test Pass Criteria: 
        ///   Query ARM ORX_MODE using Get Command. Pass if it is set correctly.
        ///   
        ///</summary>
        [Test, Sequential]
        public static void CheckObsRxPathSource([Values(Mykonos.OBSRXCHANNEL.OBS_RXOFF,
                                                        Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO,   
                                                        Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO, 
                                                        Mykonos.OBSRXCHANNEL.OBS_INTERNALCALS,    
                                                        Mykonos.OBSRXCHANNEL.OBS_RX1_SNIFFERLO,   
                                                        Mykonos.OBSRXCHANNEL.OBS_RX2_SNIFFERLO,  
                                                        Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A, 
                                                        Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B,
                                                        Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C)] Mykonos.OBSRXCHANNEL ObsRxChannel)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.radioOn();
            Link.Mykonos.setObsRxPathSource(ObsRxChannel);

            //Check which ORX_MODE is Enabled 

            byte[] armData = new byte[] { 0 };
            Mykonos.OBSRXCHANNEL channel = Mykonos.OBSRXCHANNEL.OBS_RXOFF;
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { OrxModeObjId }, 1);
            Link.Mykonos.readArmMem(0x20000000, 1, 1, ref armData);
            Link.Mykonos.getObsRxPathSource(ref channel);
            //armData[0] is D2-D0 is the ObsRx Path

            Console.WriteLine("Arm OrxMode: " + armData[0].ToString("X"));
            Assert.AreEqual((Byte)ObsRxChannel, armData[0], "OrxMode does not match desired ObsRxChannel");
            Assert.AreEqual(ObsRxChannel, channel, "ObsRX channel does not match");

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetObsRxFramerDataSource
        /// API Under-Test: 
        ///     MYKONOS_setObsRxFramerDataSource
        /// API Test Description: 
        ///  Use MYKONOS_setObsRxFramerDataSource to setup dataSource 
        ///  Parameter. Check the SPI registers for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the in range configuration Values match the 
        ///  SPI Register Readbacks.
        ///</summary>
        ///
        [Test]
        [Category("JESD")]
        public static void CheckSetObsRxFramerDataSource([Values(0x0, 0x1)]byte dataSource)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;
            Link.Mykonos.setObsRxFramerDataSource(dataSource);

            spiData1 = Link.spiRead(0xDCA); Console.WriteLine("0xDCA: " + spiData1.ToString("X"));
            Assert.AreEqual(dataSource, ((spiData1 & 0x10) >> 4), "Loopback not set up as expected");

            Debug.WriteLine(Link.Mykonos.checkPllsLockStatus().ToString("X"));
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckDefaultObsRxPath
        /// API Under-Test: 
        ///     MYKONOS_setDefaultObsRxPath
        /// API Test Description: 
        ///     Call MYKONOS_setDefaultObsRxPath to set the default path
        ///     for each possible value
        ///       Call radioOn()
        ///       Call check that the returned path source is the same as the sent one
        ///       Call radioOff
        ///       Repeat for another path source
        /// API Test Pass Criteria: 
        ///     The default path is the same as the returned path source
        ///</summary>
        [Test]
        public static void CheckDefaultObsRxPath()
        {


            Mykonos.OBSRXCHANNEL retchannel = Mykonos.OBSRXCHANNEL.OBS_RXOFF;
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);



            foreach (Mykonos.OBSRXCHANNEL channel in Enum.GetValues(typeof(Mykonos.OBSRXCHANNEL)))
            {
                Link.Mykonos.setDefaultObsRxPath(channel);
                Link.Mykonos.radioOn();
                System.Threading.Thread.Sleep(5000);
                Link.Mykonos.getObsRxPathSource(ref retchannel);
                //Console.WriteLine("sent channel: " + channel);
                //Console.WriteLine("read channel: " + retchannel);
                if ((channel != Mykonos.OBSRXCHANNEL.OBS_SNIFFER) && (channel != Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B) && (channel != Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C))
                    Assert.AreEqual(retchannel, channel, "Returned channel is not the default selected channel");
                else
                    Assert.AreEqual(retchannel, Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A, "Returned channel is not the default selected channel");
                Link.Mykonos.radioOff();


            }
            Link.Disconnect();


        }

        ///<summary>
        /// API Test Name: 
        ///     CheckSetupObsRxAGC
        /// API Under-Test: 
        ///     CMDMYKONOS_initObsRxAgcStruct
        ///     CMDMYKONOS_initObsRxPeakAgcStruct
        ///     CMDMYKONOS_initObsRxPwrAgcStruct
        ///     MYKONOS_setupObxRxAgc
        /// API Test Description: 
        ///  Use CMDMYKONOS_initObsRxAgcStruct, CMDMYKONOS_initObsRxPeakAgcStruct,
        ///  and CMDMYKONOS_initObsRxPwrAgcStruct to write parameter values to the 
        ///  AGC structure. Call MYKONOS_setupObxRxAgc to write the values in the struct
        ///  to the corresponding registers.
        /////  Check the SPI registers for correct configuration
        ///  Check init function readbacks for correct configuration
        /// API Test Pass Criteria: 
        ///  Verify the spi readback and the function readback values 
        ///  match the set values
        ///</summary>
        [Test]
        [Category("JESD")]
        public static void CheckSetupObsRxAGC()
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
            byte agcObsRxMaxGainIndex = 250;
            byte agcObsRxMinGainIndex = 205;
            byte agcObsRxSelect = 1;
            byte agcPeakThresholdMode = 1; // Change for power only mode
            byte agcLowThsPreventGainIncrease = 1; // Change for power only mode
            UInt32 agcGainUpdateCounter = 30721;
            byte agcSlowLoopSettlingDelay = 1;
            byte agcPeakWaitTime = 5;
            byte pmdMeasDuration = 0x07;
            byte pmdMeasConfig = 0x3;
            byte agcResetOnRxEnable = 0;
            byte agcEnableSyncPulseForGainCounter = 1;

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
            Link.Mykonos.init_obsRxAgcStructure(1, ref agcRx1MaxGainIndex,
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

            Link.Mykonos.init_obsRxPwrAgcStructure(1, ref pmdUpperHighThresh,
                                                   ref pmdUpperLowThresh,
                                                   ref pmdLowerHighThresh,
                                                   ref pmdLowerLowThresh,
                                                   ref pmdUpperHighGainStepAttack,
                                                   ref pmdUpperLowGainStepAttack,
                                                   ref pmdLowerHighGainStepRecovery,
                                                   ref pmdLowerLowGainStepRecovery, ref pmdMeasDuration,
                                                   ref pmdMeasConfig);

            Link.Mykonos.init_obsRxPeakAgcStructure(1, ref apdHighThresh,
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
            Link.Mykonos.setupObsRxAgc();


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
            byte read_apdFastAttack = 01;
            byte read_hb2FastAttack = 0;
            byte read_hb2OverloadDetectEnable = 0;
            byte read_hb2OverloadDurationCnt = 0;
            byte read_hb2OverloadThreshCnt = 00;

            //Read the values
            Link.Mykonos.init_obsRxAgcStructure(0, ref read_agcRx1MaxGainIndex,
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

            Link.Mykonos.init_obsRxPwrAgcStructure(0, ref read_pmdUpperHighThresh,
                                                   ref read_pmdUpperLowThresh,
                                                   ref read_pmdLowerHighThresh,
                                                   ref read_pmdLowerLowThresh,
                                                   ref read_pmdUpperHighGainStepAttack,
                                                   ref read_pmdUpperLowGainStepAttack,
                                                   ref read_pmdLowerHighGainStepRecovery,
                                                   ref read_pmdLowerLowGainStepRecovery, ref read_pmdMeasDuration,
                                                   ref read_pmdMeasConfig);

            Link.Mykonos.init_obsRxPeakAgcStructure(0, ref read_apdHighThresh,
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
            /*
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
            */
            spiData = Link.spiRead(0x44A); Console.WriteLine("SPI Addr: 0x44A:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxMaxGainIndex, spiData + 128, "Register readback for agcObsRxMaxGainIndex incorrect");
            Assert.AreEqual(agcObsRxMaxGainIndex, read_agcObsRxMaxGainIndex, "Function readback for agcObsRxMaxGainIndex incorrect");

            spiData = Link.spiRead(0x44B); Console.WriteLine("SPI Addr: 0x44B:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxMinGainIndex, spiData + 128, "Register readback for agcObsRxMinGainIndex incorrect");
            Assert.AreEqual(agcObsRxMinGainIndex, read_agcObsRxMinGainIndex, "Function readback for agcObsRxMinGainIndex incorrect");

            spiData = Link.spiRead(0x460); Console.WriteLine("SPI Addr: 0x460:" + spiData.ToString("X"));
            Assert.AreEqual(agcObsRxSelect, spiData, "Register readback for agcObsRxSelect incorrect");
            Assert.AreEqual(agcObsRxSelect, read_agcObsRxSelect, "Function readback for agcObsRxSelect incorrect");

            spiData = Link.spiRead(0x49A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(agcPeakThresholdMode, (spiData >> 5) & 0x01, "Register readback for agcPeakThresholdMode incorrect");
            Assert.AreEqual(agcPeakThresholdMode, read_agcPeakThresholdMode, "Function readback for agcPeakThresholdMode incorrect");

            spiData = Link.spiRead(0x490); Console.WriteLine("SPI Addr: 0x490:" + spiData.ToString("X"));
            Assert.AreEqual(agcLowThsPreventGainIncrease, (spiData >> 7) & 0x01, "Register readback for agcLowThsPreventGainIncrease incorrect");
            Assert.AreEqual(agcLowThsPreventGainIncrease, read_agcLowThsPreventGainIncrease, "Function readback for agcLowThsPreventGainIncrease incorrect");

            spiData = Link.spiRead(0x49B); Console.WriteLine("SPI Addr: 0x49B:" + spiData.ToString("X"));
            Assert.AreEqual(agcGainUpdateCounter & 0xFF, spiData, "Register readback for agcGainUpdateCounter[7:0] incorrect");
            Assert.AreEqual(agcGainUpdateCounter, read_agcGainUpdateCounter, "Function readback for agcGainUpdateCounter incorrect");

            spiData = Link.spiRead(0x49C); Console.WriteLine("SPI Addr: 0x49C:" + spiData.ToString("X"));
            Assert.AreEqual((agcGainUpdateCounter >> 8) & 0xFF, spiData, "Register readback for agcGainUpdateCounter[15:8] incorrect");

            spiData = Link.spiRead(0x49D); Console.WriteLine("SPI Addr: 0x49D:" + spiData.ToString("X"));
            Assert.AreEqual((agcGainUpdateCounter >> 16) & 0x3F, spiData & 0x3F, "Register readback for agcGainUpdateCounter[21:16] incorrect");

            spiData = Link.spiRead(0x49E); Console.WriteLine("SPI Addr: 0x49E:" + spiData.ToString("X"));
            Assert.AreEqual(agcSlowLoopSettlingDelay, spiData & 0x7F, "Register readback for agcSlowLoopSettlingDelay incorrect");
            Assert.AreEqual(agcSlowLoopSettlingDelay, read_agcSlowLoopSettlingDelay, "Function readback for agcSlowLoopSettlingDelay incorrect");

            spiData = Link.spiRead(0x449); Console.WriteLine("SPI Addr: 0x449:" + spiData.ToString("X"));
            Assert.AreEqual(agcPeakWaitTime, spiData & 0x1F, "Register readback for agcPeakWaitTime incorrect");
            Assert.AreEqual(agcPeakWaitTime, read_agcPeakWaitTime, "Function readback for agcPeakWaitTime incorrect");

            spiData = Link.spiRead(0x49D); Console.WriteLine("SPI Addr: 0x49D:" + spiData.ToString("X"));
            Assert.AreEqual(agcResetOnRxEnable, (spiData >> 7) & 0x01, "Register readback for agcResetOnRxEnable incorrect");
            Assert.AreEqual(agcResetOnRxEnable, read_agcResetOnRxEnable, "Function readback for agcResetOnRxEnable incorrect");

            spiData = Link.spiRead(0x49E); Console.WriteLine("SPI Addr: 0x49E:" + spiData.ToString("X"));
            Assert.AreEqual(agcEnableSyncPulseForGainCounter, (spiData >> 7) & 0x01, "Register readback for agcEnableSyncPulseForGainCounter incorrect");
            Assert.AreEqual(agcEnableSyncPulseForGainCounter, read_agcEnableSyncPulseForGainCounter, "Function readback for agcEnableSyncPulseForGainCounter incorrect");

            spiData = Link.spiRead(0x49F); Console.WriteLine("SPI Addr: 0x49F:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperHighThresh, (spiData >> 4) & 0x0F, "Register readback for pmdUpperHighThresh incorrect");
            Assert.AreEqual(pmdUpperHighThresh, read_pmdUpperHighThresh, "Function readback for pmdUpperHighThresh incorrect");

            spiData = Link.spiRead(0x44E); Console.WriteLine("SPI Addr: 0x44E:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperLowThresh, (spiData >> 0) & 0x0F, "Register readback for pmdUpperLowThresh incorrect");
            Assert.AreEqual(pmdUpperLowThresh, read_pmdUpperLowThresh, "Function readback for pmdUpperLowThresh incorrect");

            spiData = Link.spiRead(0x490); Console.WriteLine("SPI Addr: 0x437:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerHighThresh, (spiData >> 0) & 0x7F, "Register readback for pmdLowerHighThresh incorrect");
            Assert.AreEqual(pmdLowerHighThresh, read_pmdLowerHighThresh, "Function readback for pmdLowerHighThresh incorrect");

            spiData = Link.spiRead(0x49F); Console.WriteLine("SPI Addr: 0x49F:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerLowThresh, (spiData >> 0) & 0x0F, "Register readback for pmdLowerLowThresh incorrect");
            Assert.AreEqual(pmdLowerLowThresh, read_pmdLowerLowThresh, "Function readback for pmdLowerLowThresh incorrect");

            spiData = Link.spiRead(0x499); Console.WriteLine("SPI Addr: 0x499:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperHighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for pmdUpperHighGainStepAttack incorrect");
            Assert.AreEqual(pmdUpperHighGainStepAttack, read_pmdUpperHighGainStepAttack, "Function readback for pmdUpperHighGainStepAttack incorrect");

            spiData = Link.spiRead(0x497); Console.WriteLine("SPI Addr: 0x497:" + spiData.ToString("X"));
            Assert.AreEqual(pmdUpperLowGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for pmdUpperLowGainStepAttack incorrect");
            Assert.AreEqual(pmdUpperLowGainStepAttack, read_pmdUpperLowGainStepAttack, "Function readback for pmdUpperLowGainStepAttack incorrect");

            spiData = Link.spiRead(0x498); Console.WriteLine("SPI Addr: 0x498:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerHighGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for pmdLowerHighGainStepRecovery incorrect");
            Assert.AreEqual(pmdLowerHighGainStepRecovery, read_pmdLowerHighGainStepRecovery, "Function readback for pmdLowerHighGainStepRecovery incorrect");

            spiData = Link.spiRead(0x49A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(pmdLowerLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for pmdLowerLowGainStepRecovery incorrect");
            Assert.AreEqual(pmdLowerLowGainStepRecovery, read_pmdLowerLowGainStepRecovery, "Function readback for pmdLowerLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x4D8); Console.WriteLine("SPI Addr: 0x4D8:" + spiData.ToString("X"));
            Assert.AreEqual(pmdMeasDuration, (spiData >> 0) & 0x0F, "Register readback for pmdMeasDuration incorrect");
            Assert.AreEqual(pmdMeasDuration, read_pmdMeasDuration, "Function readback for pmdMeasDuration incorrect");

            spiData = Link.spiRead(0x4D7); Console.WriteLine("SPI Addr: 0x4D7:" + spiData.ToString("X"));
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

            spiData = Link.spiRead(0x457); Console.WriteLine("SPI Addr: 0x457:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighThresh, (spiData >> 0) & 0x3F, "Register readback for apdHighThresh incorrect");
            Assert.AreEqual(apdHighThresh, read_apdHighThresh, "Function readback for apdHighThresh incorrect");

            spiData = Link.spiRead(0x458); Console.WriteLine("SPI Addr: 0x458:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowThresh, (spiData >> 0) & 0x3F, "Register readback for apdLowThresh incorrect");
            Assert.AreEqual(apdLowThresh, read_apdLowThresh, "Function readback for apdLowThresh incorrect");

            spiData = Link.spiRead(0x58B); Console.WriteLine("SPI Addr: 0x58B:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighThresh, (spiData >> 0) & 0xFF, "Register readback for hb2HighThresh incorrect");
            Assert.AreEqual(hb2HighThresh, read_hb2HighThresh, "Function readback for hb2HighThresh incorrect");

            spiData = Link.spiRead(0x58C); Console.WriteLine("SPI Addr: 0x58C:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowThresh, (spiData >> 0) & 0xFF, "Register readback for hb2LowThresh incorrect");
            Assert.AreEqual(hb2LowThresh, read_hb2LowThresh, "Function readback for hb2LowThresh incorrect");

            spiData = Link.spiRead(0x58D); Console.WriteLine("SPI Addr: 0x58D:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowThresh, (spiData >> 0) & 0xFF, "Register readback for hb2VeryLowThresh incorrect");
            Assert.AreEqual(hb2VeryLowThresh, read_hb2VeryLowThresh, "Function readback for hb2VeryLowThresh incorrect");

            spiData = Link.spiRead(0x491); Console.WriteLine("SPI Addr: 0x491:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for apdHighThreshExceededCnt incorrect");
            Assert.AreEqual(apdHighThreshExceededCnt, read_apdHighThreshExceededCnt, "Function readback for apdHighThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x492); Console.WriteLine("SPI Addr: 0x492:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for apdLowThreshExceededCnt incorrect");
            Assert.AreEqual(apdLowThreshExceededCnt, read_apdLowThreshExceededCnt, "Function readback for apdLowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x493); Console.WriteLine("SPI Addr: 0x493:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2HighThreshExceededCnt incorrect");
            Assert.AreEqual(hb2HighThreshExceededCnt, read_hb2HighThreshExceededCnt, "Function readback for hb2HighThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x494); Console.WriteLine("SPI Addr: 0x494:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2LowThreshExceededCnt incorrect");
            Assert.AreEqual(hb2LowThreshExceededCnt, read_hb2LowThreshExceededCnt, "Function readback for hb2LowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x495); Console.WriteLine("SPI Addr: 0x495:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowThreshExceededCnt, (spiData >> 0) & 0xFF, "Register readback for hb2VeryLowThreshExceededCnt incorrect");
            Assert.AreEqual(hb2VeryLowThreshExceededCnt, read_hb2VeryLowThreshExceededCnt, "Function readback for hb2VeryLowThreshExceededCnt incorrect");

            spiData = Link.spiRead(0x44F); Console.WriteLine("SPI Addr: 0x44F:" + spiData.ToString("X"));
            Assert.AreEqual(apdHighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for apdHighGainStepAttack incorrect");
            Assert.AreEqual(apdHighGainStepAttack, read_apdHighGainStepAttack, "Function readback for apdHighGainStepAttack incorrect");

            spiData = Link.spiRead(0x452); Console.WriteLine("SPI Addr: 0x452:" + spiData.ToString("X"));
            Assert.AreEqual(apdLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for apdLowGainStepRecovery incorrect");
            Assert.AreEqual(apdLowGainStepRecovery, read_apdLowGainStepRecovery, "Function readback for apdLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x450); Console.WriteLine("SPI Addr: 0x450:" + spiData.ToString("X"));
            Assert.AreEqual(hb2HighGainStepAttack, (spiData >> 0) & 0x1F, "Register readback for hb2HighGainStepAttack incorrect");
            Assert.AreEqual(hb2HighGainStepAttack, read_hb2HighGainStepAttack, "Function readback for hb2HighGainStepAttack incorrect");

            spiData = Link.spiRead(0x453); Console.WriteLine("SPI Addr: 0x453:" + spiData.ToString("X"));
            Assert.AreEqual(hb2LowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for hb2LowGainStepRecovery incorrect");
            Assert.AreEqual(hb2LowGainStepRecovery, read_hb2LowGainStepRecovery, "Function readback for hb2LowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x454); Console.WriteLine("SPI Addr: 0x454:" + spiData.ToString("X"));
            Assert.AreEqual(hb2VeryLowGainStepRecovery, (spiData >> 0) & 0x1F, "Register readback for hb2VeryLowGainStepRecovery incorrect");
            Assert.AreEqual(hb2VeryLowGainStepRecovery, read_hb2VeryLowGainStepRecovery, "Function readback for hb2VeryLowGainStepRecovery incorrect");

            spiData = Link.spiRead(0x49A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(apdFastAttack, (spiData >> 7) & 0x01, "Register readback for apdFastAttack incorrect");
            Assert.AreEqual(apdFastAttack, read_apdFastAttack, "Function readback for apdFastAttack incorrect");

            spiData = Link.spiRead(0x49A); Console.WriteLine("SPI Addr: 0x49A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2FastAttack, (spiData >> 6) & 0x01, "Register readback for hb2FastAttack incorrect");
            Assert.AreEqual(hb2FastAttack, read_hb2FastAttack, "Function readback for hb2FastAttack incorrect");

            spiData = Link.spiRead(0x58A); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadDetectEnable, (spiData >> 7) & 0x01, "Register readback for hb2OverloadDetectEnable incorrect");
            Assert.AreEqual(hb2OverloadDetectEnable, read_hb2OverloadDetectEnable, "Function readback for hb2OverloadDetectEnable incorrect");

            spiData = Link.spiRead(0x58A); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadDurationCnt, (spiData >> 4) & 0x07, "Register readback for hb2OverloadDurationCnt incorrect");
            Assert.AreEqual(hb2OverloadDurationCnt, read_hb2OverloadDurationCnt, "Function readback for hb2OverloadDurationCnt incorrect");

            spiData = Link.spiRead(0x58A); Console.WriteLine("SPI Addr: 0x58A:" + spiData.ToString("X"));
            Assert.AreEqual(hb2OverloadThreshCnt, (spiData >> 0) & 0x0F, "Register readback for hb2OverloadThreshCnt incorrect");
            Assert.AreEqual(hb2OverloadThreshCnt, read_hb2OverloadThreshCnt, "Function readback for hb2OverloadThreshCnt incorrect");
            Link.Disconnect();
        }
    }
}
