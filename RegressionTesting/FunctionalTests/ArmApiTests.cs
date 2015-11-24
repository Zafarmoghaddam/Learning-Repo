using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Remoting.Metadata.W3cXsd2001;


using NUnit.Framework;

using AdiCmdServerClient;

namespace mykonosUnitTest
{

    //TODO: For this Test Fixture
    //Add check fo runInitCals and abortInitCals
    [TestFixture]
    [Category("ApiFunctional")]
    public class ArmApiFunctionalTests
    {
        public const byte MykonosSpi = 0x1;
        public const byte AD9528Spi = 0x2;
        public const byte WriteConfigCmd = 0x06;
        public const byte ReadConfigCmd = 0x08;
        public const byte GetCmd = 0x0C;
        public const byte IntCalsDoneObjId = 0x43;
        public const byte CalSchedObjId = 0x83;
        public static TestSetupConfig settings = new TestSetupConfig();
        /// <summary>
        /// ApiFunctionalTests Configure Test Setup Prior to QA Api Functional Tests
        /// Setup Parameters:  Refer to Test Settings
        /// From Locally Stored ARM Firmware     @"..\..\..\..\mykonos_resources\";
        /// Default test settings from TestSettings.cs
        /// </summary>
        [SetUp] 
        public void ArmApiTestInit()
        {
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
                            //(UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                            (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            
            //Call Test Setup 
            TestSetup.ArmTestSetupInit(settings);
            
            
            //Radio Status Via Direct Spi Read
            byte spiData1 = 0x0;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x1, (spiData1 & 0x3), 
                "ArmApiFunctional Test Setup: Radio State not READY");
            Console.WriteLine("ArmApiFunctional Test Setup: Complete" );
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckRunInitCals()
        /// API Under-Test: 
        ///     MYKONOS_runInitCals(..)
        ///     MYKONOS_waitInitCals(..)
        ///     MYKONOS_abortInitCals(..)
        ///     MYKONOS_getRadioState(..)
        /// API Test Description: 
        ///     Check that ARM is in Ready State
        ///     Run Init Calibration Process
        /// API Test Pass Criteria: 
        ///     Init Calibration Runs without Errors.
        ///     Check Arm State is now in INIT State
        ///     Request Init Cals done from Arm and 
        ///     Cross check with Init Cal Mask
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///0x00	TXBBF	Complete
        ///0x01	ADC_TUNER	Complete
        ///0x02	TIA	Complete
        ///0x03	DC_OFFSET	Complete
        ///0x04	Tx Atten Delay	In Development
        ///0x05	Rx Gain Delay	Unknown (no progress?)
        ///0x06	Flash Cal	Complete
        ///0x07	Path Delay	Complete
        ///0x08	TxLO Leakage Internal Init	Complete
        ///0x09	TxLO Leakage External Init	Complete
        ///0x0a	TxQEC Init	Complete
        ///0x0b	LBRx LO Delay	Complete
        ///0x0c	LBRx TCAL	Complete
        ///0x0d	Rx LO Delay	Complete
        ///0x0e	Rx TCAL	Complete
        ///0x0f	DPD Init	Complete (undergoing release)
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckRunInitCalsFromReady()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;
            Link.setSpiChannel(MykonosSpi);
            byte spiData1 = 0x0;
            Console.WriteLine("Starting CheckRunInitCalsFromReady Test");

            //Test Setup Check ARM in READY STATE   
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x1, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in READY STATE");

            //Run Init Cals
            UInt32 testCalMask = 0x0;
            testCalMask = settings.calMask;
            Console.WriteLine("Test Setup: Cals" + testCalMask.ToString("X"));
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(10000);
            //Check ARM in INIT STATE
            //Via SPI Readback
            //Via API
            UInt32 radioState = 0;
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: 1 Radio State is Not in INIT STATE");
            
            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Assert.AreEqual(0x2, (radioState & 0x3), "Myk: 2 Radio State not INIT");

            //Check which Init calibrations were 
            //Completed Sucessfully during last RUN_INIT

            byte[] armData = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { IntCalsDoneObjId }, 1);
            Link.Mykonos.readArmMem(0x20000000, 8, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0] since last init
            //armData[2] is the calmask [15:8], and armData[3] is calmask[7:0] since power up
            Console.WriteLine("Init Cals Requested: " + testCalMask.ToString("X"));
            Console.WriteLine("Init Cals Done Last Power Up: " + armData[2].ToString("X") + armData[3].ToString("X"));
            Console.WriteLine("Init Cals Done Last Init: " + armData[1].ToString("X") + armData[0].ToString("X"));
            Assert.AreEqual(testCalMask, ((UInt32)(((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])))),
                 "Init Cals did not match the mask written to ARM memory");

            Link.Disconnect();
        }
      
        ///<summary>
        /// API Test Name: 
        ///     CheckSetRadioOnFromInit()
        /// API Under-Test: 
        ///     
        ///     MYKONOS_radioOn	
        ///     MYKONOS_getRadioState
        /// API Test Description: 
        ///     Call API MYKONOS_runInitCals
        ///     Check device is in INIT State
        ///     Call API MYKONOS_radioOn
        ///     Call API MYKONOS_getRadioState
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     RADIO_ON state
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     RADIO_ON state
        ///     Cross Check with SPI Rd-back
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckSetRadioOnFromInit()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 radioState = 0;
            byte spiData1 = 0x0;

            //Test Setup Run Init Cals
            TestSetup.MykonosInitCals(settings);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");

            //Call API under test
            Link.Mykonos.radioOn();
            
            //Check Pass Criteria
            //Radio Status Via Direct Spi Read
            //Radio Status Via API
            
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, (spiData1 & 0x03), "Myk:  1 Radio Satus not RADIO_ON as expected");

            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Assert.AreEqual(0x3, (radioState & 0x3), "Myk: 2 Radio State not RADIO_ON as expected");
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckSetRadioOnFromRadioOn
        /// API Under-Test: 
        ///     MYKONOS_radioOn	
        ///     MYKONOS_getRadioState
        /// API Test Description: 
        ///     Call API MYKONOS_runInitCals
        ///     Call API MYKONOS_radioOn and
        ///     verify in RADIO_ON Sate
        ///     Call API MYKONOS_radioOn
        ///     Call API MYKONOS_getRadioState
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     RADIO_ON state
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     RADIO_ON state
        ///     Cross Check with SPI Rd-back
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckSetRadioOnFromRadioOn()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 radioState = 0;
            byte spiData1 = 0x0;

            //Test Setup Run Init Cals
            TestSetup.MykonosInitCals(settings);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");
            Link.Mykonos.radioOn();
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, (spiData1 & 0x03), "Myk: Test Setup Failed  Radio Satus not RADIO_ON as expected");

            //Call API under test
            Link.Mykonos.radioOn();

            //Check Pass Criteria
            //Radio Status Via Direct Spi Read
            //Radio Status Via API

            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, (spiData1 & 0x03), "Myk:  1 Radio Satus not RADIO_ON as expected");

            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Assert.AreEqual(0x3, (radioState & 0x3), "Myk: 2 Radio State not RADIO_ON as expected");
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckSetRadioOffFromRadioOn()
        /// API Under-Test: 
        ///     MYKONOS_radioOff
        ///     MYKONOS_getRadioState
        /// API Test Description: 
        ///     Call API MYKONOS_runInitCals
        ///     Check that device is INIT state
        ///     Call API MYKONOS_radioOff
        ///     Call API MYKONOS_getRadioState
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     INIT state
        ///     Check return
        ///     Cross Check with SPI Rd-back
        /// NOTE: 
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckSetRadioOffFromInit()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 radioState = 0;
            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Radio State is Not in INIT STATE");

            //Call API under test
            Link.Mykonos.radioOff();

            //Check Pass Criteria
            //Radio Status Via Direct Spi Read
            //Radio Status Via API
            spiData1 = Link.spiRead(0xD40); 
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x02, (spiData1 & 0x03), "Myk:  2 Radio Satus not INIT as Expected");

            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Assert.AreNotEqual(0x3, (radioState & 0x3), "Myk: 3 Radio State is Still Radio ON");
            Assert.AreEqual(0x2, (radioState & 0x3), "Myk: 4 Radio State not INIT");
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckSetRadioOffFromRadioOn()
        /// API Under-Test: 
        ///     MYKONOS_radioOff
        ///     MYKONOS_getRadioState
        /// API Test Description: 
        ///     Call MYKONOS_runInitCals
        ///     Call API MYKONOS_radioOn
        ///     Check that Status is Now RADIO_ON
        ///     Call API MYKONOS_radioOff
        ///     Call API MYKONOS_getRadioState
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     INIT state
        ///     Check return
        ///     Cross Check with SPI Rd-back
        /// NOTE: 
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckSetRadioOffFromRadioOn()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 radioState = 0;
            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Init Cals not completed");
            Link.Mykonos.radioOn();
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x03, (spiData1 & 0x03), "Myk: Test Setup Failed  Radio Satus not RADIO_ON as expected");
            //Call API under test
            Link.Mykonos.radioOff();

            //Check Pass Criteria
            //Radio Status Via Direct Spi Read
            //Radio Status Via API
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreNotEqual(0x3, (spiData1 & 0x3), "Myk: 1 Radio State is Still Radio ON");
            Assert.AreEqual(0x02, (spiData1 & 0x03), "Myk:  2 Radio Satus not INIT as Expected");

            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Assert.AreNotEqual(0x3, (radioState & 0x3), "Myk: 3 Radio State is Still Radio ON");
            Assert.AreEqual(0x2, (radioState & 0x3), "Myk: 4 Radio State not INIT");
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckEnaAllTrackCalsFromInit
        /// API Under-Test: 
        ///     MYKONOS_enableTrackingCals
        /// API Test Description: 
        ///     Call MYKONOS_runInitCals
        ///     Check for Init State
        ///     Call MYKONOS_enableTrackingCals to enable 
        ///     ALL cals
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates INIT
        ///     Cross Check Tracking Cals Results from
        ///     ARM Command Readback
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckEnaAllTrackCalsFromInit()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;
            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            //And check INIT
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Init Cals not completed");

 
            //Call API Under Test
            Link.Mykonos.enableTrackingCals((UInt32)(0x3FF));
            Link.hw.ReceiveTimeout = 5000;

            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(0x08, new byte[] { 0x83, 0, 0, 2 }, 4);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Debug.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") +
                armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(0x3FF, (UInt32)(((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16)
                | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0]))),
                "Tracking calmask did not match the mask written to ARM memory");

            Link.Disconnect();
        }



        ///<summary>
        /// API Test Name: 
        ///     CheckInitTrackingCals
        /// API Under-Test: 
        ///     MYKONOS_getEnabledTrackingCals
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Call MYKONOS_getEnabledTrackingCals to enable 
        ///     RX1 and RX2 QEC cals
        ///     Sweep all possible combinations
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getEnabledTrackingCals is
        ///     the same as the calmask sent.
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckInitTrackingCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;
            byte spiData1 = 0x0;
            UInt32 calmask = 0;
            //Test Setup:
            //Run Init Cals
            //And check INIT
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Init Cals not completed");


            //Call API Under Test
            for (int i = 0; i <= 0x3ff; i++)
            {
                Link.Mykonos.enableTrackingCals((UInt32)(i));
                Link.hw.ReceiveTimeout = 5000;
                Link.Mykonos.getEnabledTrackingCals(ref calmask);
                //Console.WriteLine("Enabled tracking calmask: " + calmask);

                Assert.AreEqual(i, calmask, "Tracking calmask did not match the mask returned from the function");
            }
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     CheckEnRxQecTrackingCalsFromRadioInit
        /// API Under-Test: 
        ///     MYKONOS_enableTrackingCals
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Call MYKONOS_enableTrackingCals to enable 
        ///     RX1 and RX2 QEC cals
        /// API Test Pass Criteria: 
        ///     Check that return Value from 
        ///     MYKONOS_getRadioState indicates
        ///     Cross Check Tracking Cals Results from
        ///     ARM Command Readback
        /// NOTE: 
        /// Radio State definition
        /// 1:0 | State[1:0], 
        ///     0=POWERUP, 1=READY, 2=INIT, 3=RADIO ON 
        /// 3:2 | unused 
        /// 4 | TDD_nFDD
        ///     1= TDD, 0=FDD 
        /// 7:5 | unused
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckEnRxQecTrackingCalsFromRadioInit()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;

            settings.trackCalMask = (UInt32)(Mykonos.TRACKING_CALMASK.RX1_QEC) 
                                    | (UInt32)(Mykonos.TRACKING_CALMASK.ORX1_QEC);

            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), 
                        "Myk: Test Setup Failed  Init Cals not completed");

            //Call API Under Test
            //track RX QEC
            Link.Mykonos.enableTrackingCals(settings.trackCalMask);
            Link.hw.ReceiveTimeout = 5000;
            byte[] armData = new byte[] { 0, 0, 0, 0 };
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { CalSchedObjId, 0, 0, 2 }, 4);
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
            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckGetPendingTrackingCals
        /// API Under-Test: 
        ///     MYKONOS_getPendingTrackingCals
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Call MYKONOS_enableTrackingCals to enable RX QEC and TX QEC (calmask 195)
        ///     Call RadioON
        ///     Call MYKONOS_getPendingTrackingCals to check the pending calibrations
        ///     Verify that RX QEC and TX QEC cals are pending (calmask 20485)
        /// API Test Pass Criteria: 
        ///     Check that RX QEC and TX QEC are pending
        /// Notes
        ///     Did not test with other tracking calibrations - they may not be working.
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckGetPendingTrackingCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_INTERNALCALS);
            UInt32 calmask = 0;
            byte spiData1 = 0x0;
            Mykonos.OBSRXCHANNEL channel = Mykonos.OBSRXCHANNEL.OBS_RXOFF;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            Link.Mykonos.getPendingTrackingCals(ref calmask);
            Console.WriteLine("calmask: " + calmask);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3),
                        "Myk: Test Setup Failed  Init Cals not completed");
            //Enable RxQEC and TxQEC tracking cals

            Link.Mykonos.enableTrackingCals((UInt32)(195));
            Link.Mykonos.radioOn();
            Link.Mykonos.getPendingTrackingCals(ref calmask);

            //Check that those tracking cals are pending
            Assert.AreEqual(calmask, 20485, "incorrect calibrations are pending");
            Console.WriteLine("calmask: " + calmask);

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckWriteArmConfig  
        /// API Under-Test: 
        ///     MYKONOS_writeArmConfig
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Call MYKONOS_writeArmConfig to write byte array
        ///     Manually read the Arm Config for objectids
        ///     0x0F, 0x24, 0x81
        /// API Test Pass Criteria: 
        ///     Check that the byte array sent is the 
        ///     same as the byte array read
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckWriteArmConfig()
        {
            byte statusByte = 0;
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3),
                        "Myk: Test Setup Failed  Init Cals not completed");
            //Call API Under Test
            //Writearmconfig for object ids
            //0x0F, 0x24, 0x81

            byte[] readarmData = new byte[] { };


            Link.Mykonos.writeArmConfig(0x0f, 0, new byte[] { 0x01, 0x01}, 2);
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { 0x0f, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            byte[] armData = new byte[]{};
            Link.Mykonos.readArmMem(0x20000000, 2, 1, ref armData);
            SoapHexBinary shb = new SoapHexBinary(armData);
            Assert.AreEqual(new byte[] { 0x01, 0x01 }, armData, "Read Arm Data does not match Write Arm Data");

            Link.Mykonos.writeArmConfig(0x24, 0, new byte[] { 1, 2, 3, 4 }, 4);
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { 0x24, 0, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);
            shb = new SoapHexBinary(armData);
            Assert.AreEqual(new byte[] { 1, 2, 3, 4 }, armData, "Read Arm Data does not match Write Arm Data");

            Link.Mykonos.writeArmConfig(0x81, 0, new byte[] { 1, 1 }, 2);
            Link.Mykonos.sendArmCommand(ReadConfigCmd, new byte[] { 0x81, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(ReadConfigCmd, 1000, ref statusByte);
            Link.Mykonos.readArmMem(0x20000000, 2, 1, ref armData);
            shb = new SoapHexBinary(armData);
            Assert.AreEqual(new byte[] { 1, 1, }, armData, "Read Arm Data does not match Write Arm Data");


            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     CheckReadArmConfig
        /// API Under-Test: 
        ///     MYKONOS_readArmConfig
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Manually write the data and config the chip
        ///     Call MYKONOS_readArmConfig to read the arm config
        ///     for object ids
        ///     0x0F, 0x24, 0x81
        /// API Test Pass Criteria: 
        ///     Check that the byte array sent is the 
        ///     same as the byte array read
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckReadArmConfig()
        {
            byte statusByte = 0;
            byte[] readarmData = new byte[] { };

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiData1 = 0x0;

            //Test Setup:
            //Run Init Cals
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3), "Myk: Test Setup Failed  Init Cals not completed");
            //Call API Under Test
            //Readarmconfig for object ids
            //0x0F, 0x24, 0x81

            Link.Mykonos.writeArmMem(0x20000000, 2, new byte[] { 1, 1 });
            Link.Mykonos.sendArmCommand(WriteConfigCmd, new byte[] { 0x0f, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(WriteConfigCmd, 1000, ref statusByte);

            Link.hw.ReceiveTimeout = 5000;
            Link.Mykonos.readArmConfig(0x0f, 0, ref readarmData, 2);
            Assert.AreEqual(readarmData, new byte[] { 1, 1 }, "Read Arm Data does not match Write Arm Data");


            Link.Mykonos.writeArmMem(0x20000000, 4, new byte[] { 1, 2, 3, 4 });
            Link.Mykonos.sendArmCommand(WriteConfigCmd, new byte[] { 0x24, 0, 0, 4 }, 4);
            Link.Mykonos.waitArmCmdStatus(WriteConfigCmd, 1000, ref statusByte);

            Link.hw.ReceiveTimeout = 5000;
            Link.Mykonos.readArmConfig(0x24, 0, ref readarmData, 4);
            Assert.AreEqual(readarmData, new byte[] { 1, 2, 3, 4 }, "Read Arm Data does not match Write Arm Data");

            Link.Mykonos.writeArmMem(0x20000000, 2, new byte[] { 1, 1 });
            Link.Mykonos.sendArmCommand(WriteConfigCmd, new byte[] { 0x81, 0, 0, 2 }, 4);
            Link.Mykonos.waitArmCmdStatus(WriteConfigCmd, 1000, ref statusByte);

            Link.hw.ReceiveTimeout = 5000;
            Link.Mykonos.readArmConfig(0x81, 0, ref readarmData, 2);
            Assert.AreEqual(readarmData, new byte[] { 1, 1 }, "Read Arm Data does not match Write Arm Data");
            Link.Disconnect();
        }


        ///<summary>
        /// API Test Name: 
        ///     CheckGetInitCalStatus
        /// API Under-Test: 
        ///     MYKONOS_getInitCalStatus
        /// API Test Description: 
        ///     Call API MYKONOS_radioOn
        ///     Check for Init State
        ///     Call MYKONOS_runInitCals (called in MykonosInitCals)
        ///     Call MYKONOS_getInitCalStatus to readback the status of all the 
        ///     cals. Reference with ARM mem readback and MYKONOS_getInitCalStatus
        ///     Enable one more calibration and call MYKONOS_runInitCals
        ///     Reference with ARM mem readback and MYKONOS_getInitCalStatus
        /// API Test Pass Criteria: 
        ///     Check that the MYKONOS_getInitCalStatus returns the same values as ARM mem readback
        ///     and that the return calibrations match the set ones
        /// Notes
        ///     Did not test with all init calibrations - they may not be working.
        ///</summary>
        [Test]
        [Category("ARM")]
        public static void CheckGetInitCalStatus()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_INTERNALCALS);
            UInt32 calmask = 0;
            byte spiData1 = 0x0;
            MykInitCalStatus initcalstat = new MykInitCalStatus();
            //Test Setup:
            //Run Init Cals without TX_LO_LEAKAGE_INTERNAL
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
                //(UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            TestSetup.MykonosInitCals(settings);
            System.Threading.Thread.Sleep(5000);
            Link.Mykonos.getPendingTrackingCals(ref calmask);
            Console.WriteLine("calmask: " + calmask);
            spiData1 = Link.spiRead(0xD40);
            Console.WriteLine("SPI Addr: 0xD40:" + spiData1.ToString("X"));
            Assert.AreEqual(0x2, (spiData1 & 0x3),
                        "Myk: Test Setup Failed  Init Cals not completed");


            //Readback the init cal status
            Link.Mykonos.getInitCalStatus(out initcalstat);
            Assert.AreEqual(initcalstat.CalsDoneLastRun, settings.calMask, "Cals done last run not as expected");
            Assert.AreEqual(initcalstat.CalsDoneLifeTime, settings.calMask, "Cals done lifetime not as expected");

            byte[] armData = new byte[14];
            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { IntCalsDoneObjId }, 1);
            Link.Mykonos.readArmMem(0x20000000, 14, 1, ref armData);


            Assert.AreEqual(initcalstat.CalsDoneLifeTime, ((UInt32)(((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16) | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])))), "Init done last run readback not as expected");
            Assert.AreEqual(initcalstat.CalsDoneLastRun, ((UInt32)(((UInt32)(armData[7]) << 24) | ((UInt32)(armData[6]) << 16) | ((UInt32)(armData[5]) << 8) | ((UInt32)(armData[4])))), "Init done lifetime readback not as expected");
            Assert.AreEqual(initcalstat.CalsMinimum, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Min cal readback not as expected");
            Assert.AreEqual(initcalstat.InitErrCal, (UInt32)(armData[12]), "Init Error cal readback not as expected");
            Assert.AreEqual(initcalstat.InitErrCode, (UInt32)(armData[13]), "Init Error code readback not as expected");

            //Run init cals with TX_LO_LEAKAGE_INTERNAL
            Link.Mykonos.runInitCals((UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL));


            System.Threading.Thread.Sleep(5000);

            //Readback the init cals. Last run should only have TX_LO_LEAKAGE_INTERNAL
            Link.Mykonos.getInitCalStatus(out initcalstat);
            Assert.AreEqual(initcalstat.CalsDoneLastRun, (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL), "Cals done last run not as expected");
            Assert.AreEqual(initcalstat.CalsDoneLifeTime, settings.calMask | (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL), "Cals done lifetime not as expected");

            Link.Mykonos.sendArmCommand(GetCmd, new byte[] { IntCalsDoneObjId }, 1);
            Link.Mykonos.readArmMem(0x20000000, 14, 1, ref armData);


            Assert.AreEqual(initcalstat.CalsDoneLifeTime, ((UInt32)(((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16) | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])))), "Init done last run readback not as expected");
            Assert.AreEqual(initcalstat.CalsDoneLastRun, ((UInt32)(((UInt32)(armData[7]) << 24) | ((UInt32)(armData[6]) << 16) | ((UInt32)(armData[5]) << 8) | ((UInt32)(armData[4])))), "Init done lifetime readback not as expected");
            Assert.AreEqual(initcalstat.CalsMinimum, ((UInt32)(((UInt32)(armData[11]) << 24) | ((UInt32)(armData[10]) << 16) | ((UInt32)(armData[9]) << 8) | ((UInt32)(armData[8])))), "Min cal readback not as expected");
            Assert.AreEqual(initcalstat.InitErrCal, (UInt32)(armData[12]), "Init Error cal readback not as expected");
            Assert.AreEqual(initcalstat.InitErrCode, (UInt32)(armData[13]), "Init Error code readback not as expected");
            Link.Disconnect();
        }


    }
}
