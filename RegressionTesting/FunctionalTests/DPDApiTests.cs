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
    [TestFixture]
    [Category("DPDFunctional")]
    public class DpdApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public const byte ReadConfigCmd = 0x08;
        public const byte GetCmd = 0x0C;
        public const byte IntCalsDoneObjId = 0x43;
        public const byte CalSchedObjId = 0x83;
        public const byte DpdObjId = 0x24;
        public const byte DpdInitConfig = 0x0F;
        public const byte CalStatusId = 0x42;

        public static TestSetupConfig settings;
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to QA Api Functional Tests
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// Default Setup Parameters:  Refer to TestSettings.cs
        /// </summary>
        [SetUp] 
        public void DpdApiTestInit()
        {
            settings = new TestSetupConfig();
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            //Start Calibration
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
                            //(UInt32)(Mykonos.CALMASK.DPD_INIT) |
                            (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;

            //Call Test Setup 
            TestSetup.DpdTestSetupInit(settings);

            //Test Setup Ensure now in Init State
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");
            Link.hw.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckDpdConfig()
        /// API Under-Test: 
        ///     MYKONOS_configDpd
        /// API Test Description: 
        ///    MYKONOS_configDpd
        ///    Is called during initialisation
        ///    Read ARM back and compare DPD configuration
        /// API Test Pass Criteria: 
        ///     Compare arm configuration registers to DPD setting
        ///     in test setup 
        ///</summary>
        [Test]
        [Category("DPD")]
        public static void CheckDpdConfig()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte statusByte = 0;
            //read back DPD Config structure from ARM memory
            Debug.WriteLine("DPD Config Structure from ARM Mem");
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 0, 0, 20 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            byte[] armTrackingData = new byte[20];
            Link.Mykonos.readArmMem(0x20000000, 20, 1, ref armTrackingData);

            Debug.WriteLine("DPD Init Structure from ARM Mem");
            //Check against Configuration Settings
            Assert.AreEqual(settings.mykDpdCfg.Damping, ((UInt32)(armTrackingData[0]))
                                              & 0x0F, "DPD Damping Threshold Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.Samples, ((UInt32)(armTrackingData[3]) << 8) | ((UInt32)(armTrackingData[2]))
                                              , "DPD Samples Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.ModelVersion, ((UInt32)(armTrackingData[1]) & 0x03)
                                            , "ModelVersion Not Set as Desired");

            Assert.AreEqual(settings.mykDpdCfg.HighSampleHistory, ((UInt32)(armTrackingData[1]) >> 2)
                                             & 0x01, "HighSampleHistory Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.OutlierThreshold, ((UInt32)(armTrackingData[11]) << 8)
                                             | ((UInt32)(armTrackingData[10])), "OutlierThreshold Not Set as Desired");

            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdInitConfig, 2, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            armTrackingData = new byte[2];
            Link.Mykonos.readArmMem(0x20000000, 2, 1, ref armTrackingData);

            Assert.AreEqual(settings.mykDpdCfg.AdditionalDelayOffset, ((UInt32)(armTrackingData[1]) << 8)
                                             | ((UInt32)(armTrackingData[0])), "AdditionalDelayOffset Not Set as Desired");

            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdInitConfig, 10, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            armTrackingData = new byte[2];
            Link.Mykonos.readArmMem(0x20000000, 2, 1, ref armTrackingData);

            Assert.AreEqual(settings.mykDpdCfg.PathDelayPnSeqLevel, ((UInt32)(armTrackingData[1]) << 8)
                                             | ((UInt32)(armTrackingData[0])), "enableDpd Not Set as Desired");
              }

        ///<summary>
        /// API Test Name: 
        ///     CheckCLGCConfig()
        /// API Under-Test: 
        ///     MYKONOS_configClgc
        /// API Test Description: 
        ///    MYKONOS_configClgc
        ///    Is called during initialisation
        ///    Read ARM back and compare CLGC configuration
        /// API Test Pass Criteria: 
        ///     Compare arm configuration registers to CLGC setting
        ///     in test setup 
        ///</summary>
        [Test]
        [Category("DPD")]
        public static void CheckCLGCConfig()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte statusByte = 0;
            //read back CLGC Config structure from ARM memory
            Debug.WriteLine("CLGC Config Structure from ARM Mem");
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 20, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            byte[] armTrackingData = new byte[4];
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armTrackingData);

            Debug.WriteLine("CLGC Init Structure from ARM Mem");
            //Check against Configuration Settings
            Assert.AreEqual((ushort)settings.mykClgcCfg.Tx1DesiredGain, ((UInt16)(armTrackingData[0])
                                              & 0xFF) | (((UInt16)(armTrackingData[1]) & 0xFF) << 8), "CLGC Tx1DesiredGain Not Set as Desired");
            Assert.AreEqual((ushort)settings.mykClgcCfg.Tx2DesiredGain, ((UInt32)(armTrackingData[2])
                                            & 0xFF) | (((UInt32)(armTrackingData[3]) & 0xFF) << 8), "CLGC Tx2DesiredGain Not Set as Desired");

            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 36, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armTrackingData);

            Assert.AreEqual(settings.mykClgcCfg.Tx1AttenLimit, ((UInt32)(armTrackingData[0])
                                  & 0xFF) | (((UInt32)(armTrackingData[1]) & 0xFF) << 8), "CLGC Tx1AttenLimit Not Set as Desired");
            Assert.AreEqual(settings.mykClgcCfg.Tx2AttenLimit, ((UInt32)(armTrackingData[2])
                                            & 0xFF) | (((UInt32)(armTrackingData[3]) & 0xFF) << 8), "CLGC Tx2AttenLimit Not Set as Desired");
            
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 40, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armTrackingData);

            Assert.AreEqual(settings.mykClgcCfg.Tx1ControlRatio, ((UInt32)(armTrackingData[0])
                                  & 0xFF) | (((UInt32)(armTrackingData[1])  & 0xFF) << 8), "CLGC Tx1ControlRatio Not Set as Desired");
            Assert.AreEqual(settings.mykClgcCfg.Tx2ControlRatio, ((UInt32)(armTrackingData[2])
                                            & 0xFF) | (((UInt32)(armTrackingData[3])  & 0xFF) << 8), "CLGC Tx2ControlRatio Not Set as Desired");

            

        }
        ///<summary>
        /// API Test Name: 
        ///     CheckGetDpdClgcConfig()
        /// API Under-Test: 
        ///     MYKONOS_getDpdConfig
        ///     MYKONOS_getClgcConfig
        /// API Test Description: 
        ///    MYKONOS_configDpd and MYKONOS_configClgc
        ///    Is called during initialisation writting testSettings to ARM
        ///    Use MYKONOS_getDpdConfig and MYKONOS_getClgcConfig to Readback from Arm into Device Structure
        ///    Compare currect Device Structure to testSettings
        /// API Test Pass Criteria: 
        ///     Configuration read back from API should match Test Settings 
        /// NOTE: 
        ///</summary>
        [Test]
        [Category("DPD")]
        public static void CheckGetDpdClgcConfig()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            MykDpdConfig readdpdconfig = new MykDpdConfig();
            MykClgcConfig readclgcconfig = new MykClgcConfig();
            //read back DPD Config structure via API
            Debug.WriteLine("Read DPD Config Structure via API");
            Link.Mykonos.getDpdConfig(out readdpdconfig);
            Link.Mykonos.getClgcConfig(out readclgcconfig);
            MykonosDpdConfig DpdConfigRb = new MykonosDpdConfig();
            
            //Link.Mykonos.init_dpdConfigStruct(0, ref readdpdconfig);
            
            //Check against DPD Configuration Settings
            Assert.AreEqual(settings.mykDpdCfg.AdditionalDelayOffset, readdpdconfig.AdditionalDelayOffset, "AdditionalDelayOffset Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.Damping, readdpdconfig.Damping, "Damping Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.HighSampleHistory, readdpdconfig.HighSampleHistory, "HighSampleHistory Not Set as Desired");

            Assert.AreEqual(settings.mykDpdCfg.ModelVersion, readdpdconfig.ModelVersion, "ModelVersion Not Set as Desired");

            Assert.AreEqual(settings.mykDpdCfg.NumWeights, readdpdconfig.NumWeights, "NumWeights Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.OutlierThreshold, readdpdconfig.OutlierThreshold, "OutlierThreshold Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.PathDelayPnSeqLevel, readdpdconfig.PathDelayPnSeqLevel, "PathDelayPnSeqLevel Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.Samples, readdpdconfig.Samples, "Samples Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.WeightsImag, readdpdconfig.WeightsImag, "WeightsImag Not Set as Desired");
            Assert.AreEqual(settings.mykDpdCfg.WeightsReal, readdpdconfig.WeightsReal, "WeightsReal Not Set as Desired");
 


            //Check against CLGC configuration settings
            Assert.AreEqual(settings.mykClgcCfg.Tx1AttenLimit, readclgcconfig.Tx1AttenLimit, "Tx1AttenLimit Not Set as Desired");
            Assert.AreEqual(settings.mykClgcCfg.Tx2AttenLimit, readclgcconfig.Tx2AttenLimit, "Tx2AttenLimit Not Set as Desired");
            Assert.AreEqual(settings.mykClgcCfg.Tx1DesiredGain, readclgcconfig.Tx1DesiredGain, "Tx1DesiredGain Not Set as Desired");

            Assert.AreEqual(settings.mykClgcCfg.Tx2DesiredGain, readclgcconfig.Tx2DesiredGain, "Tx2DesiredGain Not Set as Desired");

            Assert.AreEqual(settings.mykClgcCfg.Tx1ControlRatio, readclgcconfig.Tx1ControlRatio, "Tx1ControlRatio Not Set as Desired");
            Assert.AreEqual(settings.mykClgcCfg.Tx2ControlRatio, readclgcconfig.Tx2ControlRatio, "Tx2ControlRatio Not Set as Desired");

            Link.hw.Disconnect();
        }
        
        ///<summary>
        /// API Test Name: 
        ///     CheckEnableDpdTrackingFromInit()
        /// API Under-Test: 
        ///     MYKONOS_enableTrackingCals(..)
        ///     enableDpdTracking is a helper function called from MYKONOS_enableTrackingCals
        /// API Test Description: 
        ///    Check device is in INIT State
        ///    Call API to enable Dpd Tracking 
        ///    Configure Tracking cals with Tracking Cal enabled
        ///    Call to enable Radio on Radio OFF with DPD doesn't cause Error
        /// API Test Pass Criteria: 
        ///     Read back configuration from ARM memory to ensure
        ///     DPD confguration has enabled/disabled Feature
        /// NOTE: 
        ///</summary>
        [Test, Combinatorial]
        [Category("DPD")]
        public static void CheckEnableDpdTrackingFromInit([Values(0x0, 0x1)] byte Tx1DpdTracking, [Values(0x0, 0x1)] byte Tx2DpdTracking)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte status = 0;
            UInt32 bitmask = 0;

            //Enable DPD Tracking Calibrations
            //settings.trackCalMask = (0x3FF);
            Console.WriteLine(settings.trackCalMask);
             settings.trackCalMask = settings.trackCalMask
                                     | ((UInt32)Tx1DpdTracking << 8)
                                    | ((UInt32)Tx2DpdTracking << 9);

             Console.WriteLine(settings.trackCalMask);
             Link.Mykonos.enableTrackingCals(settings.trackCalMask);

            //Test That DPD Tracking Cals were enabled in Cal Mask
            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { CalSchedObjId, 0, 0, 2 }, 4);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Console.WriteLine("Test tracking calmask:" + settings.trackCalMask.ToString("X"));
            Console.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") +
                armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));
            Link.Mykonos.getEnabledTrackingCals(ref bitmask);

            Assert.AreEqual(settings.trackCalMask, (UInt32)(((UInt32)(armData[3]) << 24)
                                              | ((UInt32)(armData[2]) << 16)
                                              | ((UInt32)(armData[1]) << 8)
                                              | ((UInt32)(armData[0]))),
                "Tracking calmask did not match the mask written to ARM memory");
            Assert.AreEqual(settings.trackCalMask, bitmask, "getEnabledTrackingCals readback not as expected");

            //Call API under test To enable DPD Tracking
            //Link.Mykonos.enableDpdTracking(DpdTracking);
            //DPD Tracking already enabled in MYKONOS_enableTrackingCals

            //Test That DPD Tracking Cals were enabled in DPD Config
            armData = new byte[] { 0, 0, 0, 0};
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 32, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);

            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is Expected to 0, and armData[0] is expected to be 1 for DPD Enable
            Console.WriteLine("DPD Config Readback: " +
                armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(Tx1DpdTracking, armData[0], "Tx1 DPD enable Not Set in DPD Config");
            Assert.AreEqual(Tx2DpdTracking, armData[2], "Tx2 DPD enable Not Set in DPD Config");


            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOn();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not Radio On State");
            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOff();
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x02, (spiData1 & 0x03), "Myk:  2 Radio Satus not INIT as Expected");
            Link.hw.Disconnect();

        }
        ///<summary>
        /// API Test Name: 
        ///     CheckEnableClgcTrackingFromInit()
        /// API Under-Test: 
        ///     MYKONOS_enableTrackingCals(..)
        ///     enableClgcTracking is a helper function called from MYKONOS_enableTrackingCals
        /// API Test Description: 
        ///    Check device is in INIT State
        ///    Call API to enable Dpd Tracking 
        ///    Configure Tracking cals with Tracking Cal enabled
        ///    Call to enable Radio on Radio OFF with DPD doesn't cause Error
        /// API Test Pass Criteria: 
        ///     Read back configuration from ARM memory to ensure
        ///     CLGC confguration has enabled/disabled Feature
        /// NOTE: 
        ///</summary>
        [Test, Combinatorial]
        [Category("DPD")]
        public static void CheckEnableClgcTrackingFromInit([Values(0x0, 0x1)] byte Tx1ClgcTracking, [Values(0x0, 0x1)] byte Tx2ClgcTracking)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            UInt32 bitmask = 0;
            //Test Setup Run Init Cals
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");

            //Enable DPD Tracking Calibrations
            //settings.trackCalMask = (0x3FF);
            settings.trackCalMask = settings.trackCalMask
                                    //| (UInt32)(Mykonos.TRACKING_CALMASK.TX1_DPD)
                                   //| (UInt32)(Mykonos.TRACKING_CALMASK.TX2_DPD)
                                   | ((UInt32)Tx1ClgcTracking << 10)
                                     | ((UInt32)Tx2ClgcTracking << 11); 
            Link.Mykonos.enableTrackingCals(settings.trackCalMask);

            //Test That DPD Tracking Cals were enabled in Cal Mask
            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { CalSchedObjId, 0, 0, 4 }, 4);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Console.WriteLine("Test tracking calmask:" + settings.trackCalMask.ToString("X"));
            Console.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") +
                armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));
            //For now, the CLGC bits will not show up in the calmask. However, they show up
            //in dpdconfig struct, which is checked below. DPD bitmasks are de-enabled because they are
            //not enabled for this test and CLGC enables them.

            Link.Mykonos.getEnabledTrackingCals(ref bitmask);
            Assert.AreEqual((settings.trackCalMask & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX1_CLGC)) & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX2_CLGC), ((UInt32)(((UInt32)(armData[3]) << 24)
                                              | ((UInt32)(armData[2]) << 16)
                                              | ((UInt32)(armData[1]) << 8)
                                              | ((UInt32)(armData[0])))) & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX1_DPD) & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX2_DPD),
                "Tracking calmask did not match the mask written to ARM memory");
            Assert.AreEqual(settings.trackCalMask, bitmask, "getEnabledTrackingCals readback not as expected");


            //Call API under test To enable DPD Tracking
            //Link.Mykonos.enableClgcTracking(VlgcTracking);
            //CLGC and DPD tracking already enabled in MYKONOS_enableTrackingCals

            //Test That DPD Tracking Cals were enabled in DPD Config
            armData = new byte[] { 0, 0 ,0 ,0};
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 24, 0, 4 }, 4);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is Expected to 0, and armData[0] is expected to be 1 for DPD Enable
            Console.WriteLine("DPD Config Readback: " +
                armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(Tx1ClgcTracking, armData[0], "DPD enable Not Set in DPD Config");
            Assert.AreEqual(Tx2ClgcTracking, armData[2], "DPD enable Not Set in DPD Config");
            //Assert.AreEqual(0x0, armData[1], "DPD enable Not Set in DPD Config");

            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOn();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not Radio On State");
            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOff();
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x02, (spiData1 & 0x03), "Myk:  2 Radio Satus not INIT as Expected");
            Link.hw.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckGetDpdCalStatus()
        /// API Under-Test: 
        ///     MYKONOS_GetDpdStatus
        /// API Test Description: 
        ///    Check device is in INIT State
        ///    Call API to enable Dpd Tracking 
        ///    Configure Tracking cals with DPD Tracking Cal enabled
        ///    Call MYKONOS_GetDpdCalStatus to retrieve DpdCalStatus
        ///  API Test Pass Criteria: 
        ///     Readbacks are as expected
        /// NOTE: 
        ///     Revisit this test once we have DPD setup for 
        ///     Automated testing.
        ///</summary>
        [Test]
        [Category("DPD")]
        public static void CheckGetDpdStatus()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte status = 0;
            //Enable DPD Tracking Calibrations
            settings.trackCalMask = (0x3FF);
            settings.trackCalMask = settings.trackCalMask
                                    | (UInt32)(Mykonos.TRACKING_CALMASK.TX1_DPD)
                                   | (UInt32)(Mykonos.TRACKING_CALMASK.TX2_DPD);
            Link.Mykonos.enableTrackingCals(settings.trackCalMask);

            //Test That DPD Tracking Cals were enabled in Cal Mask
            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { CalSchedObjId, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);

            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Console.WriteLine("Test tracking calmask:" + settings.trackCalMask.ToString("X"));
            Console.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") +
                armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(settings.trackCalMask, (UInt32)(((UInt32)(armData[3]) << 24)
                                              | ((UInt32)(armData[2]) << 16)
                                              | ((UInt32)(armData[1]) << 8)
                                              | ((UInt32)(armData[0]))),
                "Tracking calmask did not match the mask written to ARM memory");
            //Call API under test To enable DPD Tracking
            //Link.Mykonos.enableDpdTracking(0x1);
            //DPD tracking already enabled in MYKONOS_enableTrackingCals
            //Test That DPD Tracking Cals were enabled in DPD Config
            armData = new byte[] { 0, 0, 0, 0 };
            //Link.Mykonos.readArmConfig(DpdObjId, 32, ref armData, 4);
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 32, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is Expected to 0, and armData[0] is expected to be 1 for DPD Enable
            Console.WriteLine("DPD Config Readback: " +
                armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(0x1, armData[0], "DPD Tx1 enable Not Set in DPD Config");
            Assert.AreEqual(0x1, armData[2], "DPD Tx2 enable Not Set in DPD Config");

            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOn();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not Radio On State");


            MykDpdStatus tx1DpdStatus;
            Link.Mykonos.getDpdStatus(Mykonos.TXCHANNEL.TX1, out tx1DpdStatus);

            Debug.WriteLine("Tx1 Error Status :" + tx1DpdStatus.ErrorStatus);
            Debug.WriteLine("Tx1 Model Error Percent :" + tx1DpdStatus.ModelErrorPercent);
            Debug.WriteLine("Tx1 Ext Path Delay :" + tx1DpdStatus.ExtPathDelay);
            Debug.WriteLine("Tx1 Track Count :" + tx1DpdStatus.TrackCount);

            armData = new byte[28];
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { CalStatusId, DpdObjId, 0 }, 3);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);


            Link.Mykonos.readArmMem(0x20000000, 28, 1, ref armData);
            Assert.AreEqual(tx1DpdStatus.ErrorStatus, ( ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0]))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1DpdStatus.TrackCount, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1DpdStatus.ModelErrorPercent, ((UInt32)(((UInt32)(armData[15]) << 24) | ((UInt32)(armData[14]) << 16) | ((UInt32)(armData[13]) << 8) | ((UInt32)(armData[12])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1DpdStatus.ExtPathDelay, ((UInt32)(armData[26])*16) + ((UInt32)(armData[24])), "Error Status Readback not as expected");


            MykDpdStatus tx2DpdStatus;
            Link.Mykonos.getDpdStatus(Mykonos.TXCHANNEL.TX2, out tx2DpdStatus);

            Debug.WriteLine("Tx2 Error Status :" + tx2DpdStatus.ErrorStatus);
            Debug.WriteLine("Tx2 Model Error Percent :" + tx2DpdStatus.ModelErrorPercent);
            Debug.WriteLine("Tx2 Ext Path Delay :" + tx2DpdStatus.ExtPathDelay);
            Debug.WriteLine("Tx2 Track Count :" + tx2DpdStatus.TrackCount);

            armData = new byte[28];
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { CalStatusId, DpdObjId, 1 }, 3);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);


            Link.Mykonos.readArmMem(0x20000000, 28, 1, ref armData);
            Assert.AreEqual(tx2DpdStatus.ErrorStatus, (((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0]))), "Error Status Readback not as expected");
            Assert.AreEqual(tx2DpdStatus.TrackCount, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx2DpdStatus.ModelErrorPercent, ((UInt32)(((UInt32)(armData[15]) << 24) | ((UInt32)(armData[14]) << 16) | ((UInt32)(armData[13]) << 8) | ((UInt32)(armData[12])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx2DpdStatus.ExtPathDelay, ((UInt32)(armData[26]) * 16) + ((UInt32)(armData[24])), "Error Status Readback not as expected");


        }
        ///<summary>
        /// API Test Name: 
        ///     CheckGetClgcStatus()
        /// API Under-Test: 
        ///     MYKONOS_getClgcStatus(..)
        /// API Test Description: 
        ///    Check device is in INIT State
        ///    Call API to enable CLGC Tracking 
        ///    Configure Tracking cals with CLGC Tracking Cal enabled
        ///    Use API to readback CLGC status.
        /// API Test Pass Criteria: 
        ///     Readbacks are as expected
        /// NOTE: 
        ///     Revisit this test once we have DPD setup for 
        ///     Automated testing.
        ///</summary>
        [Test, Sequential]
        [Category("DPD")]
        public static void CheckGetClgcStatus()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte status = 0;
            //Test Setup Run Init Cals
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");

            //Enable DPD Tracking Calibrations
            settings.trackCalMask = (0x3FF);
            settings.trackCalMask = settings.trackCalMask
                                  //  | (UInt32)(Mykonos.TRACKING_CALMASK.TX1_DPD)
                                 //  | (UInt32)(Mykonos.TRACKING_CALMASK.TX2_DPD)
                                    | (UInt32)(Mykonos.TRACKING_CALMASK.TX1_CLGC)
                                     | (UInt32)(Mykonos.TRACKING_CALMASK.TX2_CLGC);
            Link.Mykonos.enableTrackingCals(settings.trackCalMask);

            //Test That DPD Tracking Cals were enabled in Cal Mask
            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { CalSchedObjId, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Console.WriteLine("Test tracking calmask:" + settings.trackCalMask.ToString("X"));
            Console.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") +
                armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual((settings.trackCalMask & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX1_CLGC)) & ~(UInt32)(Mykonos.TRACKING_CALMASK.TX2_CLGC) , (UInt32)(((UInt32)(armData[3]) << 24)
                                              | ((UInt32)(armData[2]) << 16)
                                              | ((UInt32)(armData[1]) << 8)
                                              | ((UInt32)(armData[0]))),
                "Tracking calmask did not match the mask written to ARM memory");
            //Call API under test To enable DPD Tracking
            //Link.Mykonos.enableClgcTracking(0x1);
            //CLGC and DPD tracking already enabled in MYKONOS_enableTrackingCals.

            //Test That DPD Tracking Cals were enabled in DPD Config
            armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { DpdObjId, 24, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);

            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is Expected to 0, and armData[0] is expected to be 1 for DPD Enable
            Console.WriteLine("DPD Config Readback: " +
                armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(0x1, armData[0], "CLGC Tx1 enable Not Set in DPD Config");
            Assert.AreEqual(0x1, armData[2], "CLGC Tx2 enable Not Set in DPD Config");

            //Make Sure Radio can be enabled with no error
            Link.Mykonos.radioOn();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x3, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not Radio On State");


            //Check CLGC status with ARM mem
            MykClgcStatus tx1ClgcStatus;
            Link.Mykonos.getClgcStatus(Mykonos.TXCHANNEL.TX1, out tx1ClgcStatus);

            Debug.WriteLine("Tx1 Error Status :" + tx1ClgcStatus.ErrorStatus);
            Debug.WriteLine("Tx1 Desired Gain :" + tx1ClgcStatus.DesiredGain);
            Debug.WriteLine("Tx1 Current Gain :" + tx1ClgcStatus.CurrentGain);
            Debug.WriteLine("Tx1 TxGain :" + tx1ClgcStatus.TxGain);
            Debug.WriteLine("Tx1 OrxRms :" + tx1ClgcStatus.OrxRms);
            Debug.WriteLine("Tx1 TxRms :" + tx1ClgcStatus.TxRms);

            armData = new byte[24];
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { CalStatusId, DpdObjId, 0}, 3);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);
            // Offset of 40
            uint offset = 40;
            Link.Mykonos.readArmMem(0x20000000 + offset, 24, 1, ref armData);
            Assert.AreEqual(tx1ClgcStatus.ErrorStatus, ((UInt32)(((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16) | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.DesiredGain, ((UInt32)(((UInt32)(armData[7]) << 24) | ((UInt32)(armData[6]) << 16) | ((UInt32)(armData[5]) << 8) | ((UInt32)(armData[4])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.CurrentGain, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.TxGain, ((UInt32)(((UInt32)(armData[15]) << 24) | ((UInt32)(armData[14]) << 16) | ((UInt32)(armData[13]) << 8) | ((UInt32)(armData[12])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.OrxRms, ((UInt32)(((UInt32)(armData[19]) << 24) | ((UInt32)(armData[18]) << 16) | ((UInt32)(armData[17]) << 8) | ((UInt32)(armData[16])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.TxRms, ((UInt32)(((UInt32)(armData[23]) << 24) | ((UInt32)(armData[22]) << 16) | ((UInt32)(armData[21]) << 8) | ((UInt32)(armData[20])))), "Error Status Readback not as expected");

            MykClgcStatus tx2ClgcStatus;
            Link.Mykonos.getClgcStatus(Mykonos.TXCHANNEL.TX2, out tx2ClgcStatus);

            Debug.WriteLine("Tx2 Error Status :" + tx2ClgcStatus.ErrorStatus);
            Debug.WriteLine("Tx2 Desired Gain :" + tx2ClgcStatus.DesiredGain);
            Debug.WriteLine("Tx2 Current Gain :" + tx2ClgcStatus.CurrentGain);
            Debug.WriteLine("Tx2 TxGain :" + tx2ClgcStatus.TxGain);
            Debug.WriteLine("Tx2 OrxRms :" + tx2ClgcStatus.OrxRms);
            Debug.WriteLine("Tx2 TxRms :" + tx2ClgcStatus.TxRms);

            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { CalStatusId, DpdObjId, 1 }, 3);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref status);
            // Offset of 40
            Link.Mykonos.readArmMem(0x20000000 + offset, 24, 1, ref armData);
            Assert.AreEqual(tx1ClgcStatus.ErrorStatus, ((UInt32)(((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16) | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.DesiredGain, ((UInt32)(((UInt32)(armData[7]) << 24) | ((UInt32)(armData[6]) << 16) | ((UInt32)(armData[5]) << 8) | ((UInt32)(armData[4])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.CurrentGain, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.TxGain, ((UInt32)(((UInt32)(armData[15]) << 24) | ((UInt32)(armData[14]) << 16) | ((UInt32)(armData[13]) << 8) | ((UInt32)(armData[12])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.OrxRms, ((UInt32)(((UInt32)(armData[19]) << 24) | ((UInt32)(armData[18]) << 16) | ((UInt32)(armData[17]) << 8) | ((UInt32)(armData[16])))), "Error Status Readback not as expected");
            Assert.AreEqual(tx1ClgcStatus.TxRms, ((UInt32)(((UInt32)(armData[23]) << 24) | ((UInt32)(armData[22]) << 16) | ((UInt32)(armData[21]) << 8) | ((UInt32)(armData[20])))), "Error Status Readback not as expected");

            Link.hw.Disconnect();

        }
    }
}
