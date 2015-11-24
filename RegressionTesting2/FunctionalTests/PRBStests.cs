using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

//Uses NUnit plugin for Visual Studio for testing
using NUnit.Framework;

using AdiCmdServerClient;

namespace mykonosUnitTest
{

    [TestFixture]
    public static class PRBSTests
    {
        public static TestSetupConfig settings = new TestSetupConfig();

        ///<summary>
        /// API Test Name: 
        ///     EnableDeframerPrbsChecker
        /// API Under-Test: 
        ///     Mykonos_enableDeframerPrbsChecker	
        /// API Test Description: 
        ///     Call API Mykonos_enableDeframerPrbsChecker to set 
        ///     To enable 3 types of PRBS Test Modes on the Deframer
        ///     PRBS7 = 0, PRBS15 = 1, PRBS31 = 2
        ///     Reads SPI register value to check if PRBS setting is correct
        ///     0x0A5[7:4] PRBS20 Lane Selection
        ///     0x0A5[3] PRBS20 Clear
        ///     0x0A5[2:1] PRBS20 Type Selection
        ///     0x0A5[0] Deframer PRBS20 Enable
        /// API Test Pass Criteria: 
        ///     Check Deframer PRBS Configuration Registers are updated. 
        ///     As Expected to enable the desired PRBS Test Mode.
        /// 
        ///</summary>
        [Test]
        public static void EnableDeframerPrbsChecker([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER prbsorder)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //reset the device
            Link.Mykonos.resetDevice();
            System.Threading.Thread.Sleep(100);

            //setting SPI channel for 4-wire mode
            Link.setSpiChannel(1);
            Link.spiWrite(0x000, 0x18);

            Link.Mykonos.enableDeframerPrbsChecker(0xF, prbsorder, 1);
            byte PRBSspi = Link.spiRead(0xA5);
            if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS7)
            {
                Assert.AreEqual(PRBSspi, 0xF1, "PRBS7 mismatch");
            }
            else if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS15)
            {
                Assert.AreEqual(PRBSspi, 0xF3, "PRBS15 mismatch");
            }
            else
            {
                Assert.AreEqual(PRBSspi, 0xF5, "PRBS31 mismatch");
            }


            Link.Disconnect();
        }


        ///<summary>
        /// API Test Name: 
        ///     EnableRxFramerPrbs
        /// API Under-Test: 
        ///     Mykonos_enableRxFramerPrbs
        /// API Test Description: 
        ///     Call API Mykonos_enableRxFramerPrbs to set 
        ///     To enable 3 types of PRBS Test Modes on the framer
        ///     PRBS7 = 0, PRBS15 = 1, PRBS31 = 2
        ///     Reads SPI register value to check if PRBS setting is correct
        ///     0x072[4] Framer PRBS20 Error Injection
        ///     0x072[3] Framer PRBS20 Hold
        ///     0x072[2:1] Framer PRBS20 Selection
        ///     0x072[0] Framer PRBS20 Enable
        /// API Test Pass Criteria: 
        ///     Check Deframer PRBS Configuration Registers are updated. 
        ///     As Expected to enable the desired PRBS Test Mode.
        /// 
        ///</summary>
        [Test]
        public static void EnableRxFramerPrbs([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER prbsorder)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //reset the device
            Link.Mykonos.resetDevice();
            System.Threading.Thread.Sleep(100);

            //setting SPI channel for 4-wire mode
            Link.setSpiChannel(1);
            Link.spiWrite(0x000, 0x18);

            Link.Mykonos.enableRxFramerPrbs(prbsorder, 1);
            byte PRBSspi = Link.spiRead(0x72);

            if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS7)
            {
                Assert.AreEqual(PRBSspi, 0x01, "PRBS7 mismatch");
            }
            else if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS15)
            {
                Assert.AreEqual(PRBSspi, 0x03, "PRBS15 mismatch");
            }
            else
            {
                Assert.AreEqual(PRBSspi, 0x05, "PRBS31 mismatch");
            }

            Link.Disconnect();
        }

        ///<summary>
        /// API Test Name: 
        ///     EnableObsRxFramerPrbs
        /// API Under-Test: 
        ///      Mykonos_enableObsRxFramerPrbs
        /// API Test Description: 
        ///     Call API Mykonos_enableRxFramerPrbs to set 
        ///     To enable 3 types of PRBS Test Modes on the framer
        ///     PRBS7 = 0, PRBS15 = 1, PRBS31 = 2
        ///     Reads SPI register value to check if PRBS setting is correct
        ///     0xDD9[4] sniffer prbs20 error injection
        ///     0xDD9[3] sniffer prbs20 hold
        ///     0xDD9[2:1] sniffer prbs20 selection
        ///     0xDD9[0] sniffer framer prbs20 enable
        ///     PRBS20 Type Selection:
        ///     PRBS7 = 0, PRBS15 = 1, PRBS31 = 2
        /// API Test Pass Criteria: 
        ///     Check framer PRBS Configuration Registers are updated. 
        ///     As Expected to enable the desired PRBS Test Mode.
        /// 
        ///</summary>
        [Test]
        public static void EnableObsRxFramerPrbs([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER prbsorder)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            //reset the device
            Link.Mykonos.resetDevice();
            System.Threading.Thread.Sleep(100);

            //setting SPI channel for 4-wire mode
            Link.setSpiChannel(1);
            Link.spiWrite(0x000, 0x18);

            Link.Mykonos.enableObsRxFramerPrbs(prbsorder, 1);
            byte PRBSspi = Link.spiRead(0xDD9);

            if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS7)
            {
                Assert.AreEqual(PRBSspi, 0x01, "PRBS7 mismatch");
            }
            else if (prbsorder == Mykonos.MYK_PRBS_ORDER.PRBS15)
            {
                Assert.AreEqual(PRBSspi, 0x03, "PRBS15 mismatch");
            }
            else
            {
                Assert.AreEqual(PRBSspi, 0x05, "PRBS31 mismatch");
            }

            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     RxFramerInjectPrbsErrorCheck
        /// API Under-Test: 
        ///      Mykonos_enableRxFramerPrbs
        ///      MYKONOS_rxInjectPrbsError
        /// API Test Description: 
        ///     1)Initialise System and JESD Links
        ///     2)Enable PRBS Test Mode ON Mykonos and FPGA 
        ///     3) Clear FPGA PRBS counters then check error count 
        ///        is 0 following some delay
        ///     4) Call MYKONOS_rxInjectPrbsError and check FPGA
        ///     error counters again. 
        /// API Test Pass Criteria: 
        ///    No errors should be detected after enabling PRBS
        ///    Error should be detected after enabling InjectPrbsError
        /// Notes:
        /// x41C[31:16] When 43C0_0404.4 is 1’b1:
        ///             Lane 1 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             RX Sync error report counter  – clear with 43C0_0404.5
        /// x41C[15:0]  When 43C0_0404.4 is 1’b1:
        ///             Lane 0 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             ORX Sync error report counter  – clear with 43C0_0404.5
        /// x420[31:16] When 43C0_0404.4 is 1’b1:
        ///             Lane 3 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             0 
        /// x420[15:0]  When 43C0_0404.4 is 1’b1:
        ///             Lane 2 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             TX Sync error report counter – clear with 43C0_0404.5
        ///</summary>
        [Test]
        public static void RxFramerInjectPrbsErrorCheck([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER PrbsOrder)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;

            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 fpgaData = 0;
            byte mykData = 0;
            UInt32 FpgaLane0ErrCnt = 0;
            UInt32 FpgaLane1ErrCnt = 0;
            mykData = Link.spiRead(0x78); Console.WriteLine("SPI Reg x78 = " + mykData.ToString("X"));
            
            //Initialise System and JESD Links
            TestSetup.PrbsRxTestSetupInit(settings);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaData = Link.fpgaRead(0x410); 
            Console.WriteLine("FPGA Reg x410: REFCLK Frequency Detect = 0x" + fpgaData.ToString("X") + " = " + fpgaData.ToString());
            fpgaData = Link.fpgaRead(0x10); 
            Console.WriteLine("FPGA Version x10:" + fpgaData.ToString("X"));

            //Enable PRBS Test Mode ON Mykonos and FPGA 
            //Clear FPGA PRBS counters
            //Then Check error count is 0 following some delay
            Link.Mykonos.enableRxFramerPrbs(PrbsOrder, 1);
            fpgaData = Link.fpgaRead(0x404);
            switch(PrbsOrder)
            {
                case Mykonos.MYK_PRBS_ORDER.PRBS7:
                    fpgaData  |= 0x1;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS15:
                    fpgaData |= 0x2;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS31:
                    fpgaData |= 0x4;
                    break;
                default:
                    Assert.Fail("Invalid PRBS Order");
                    break;
            }
            //GTX PRBS Check Counter + GTX PRBS Config
            //Disable PRBS Error Counter Reset
            //Read Counters for Lane 0 & 1
            //Check Reset Was Successful
            //Let PRBS Checker Run
            //Check there are no errors
            Link.fpgaWrite(0x404, fpgaData | 0x18);
            fpgaData = Link.fpgaRead(0x404);
            Link.fpgaWrite(0x404, (fpgaData & 0xFFFFFFF7));  
            fpgaData = Link.fpgaRead(0x404);
            Console.WriteLine("FPGA Reg 0x404 = " + fpgaData.ToString("X"));

            fpgaData = Link.fpgaRead(0x41C);
            Console.WriteLine("FPGA Reg 0x41C = " + fpgaData.ToString("X"));
            Assert.AreEqual(fpgaData, 0x0); 

            System.Threading.Thread.Sleep(5000);
            fpgaData = Link.fpgaRead(0x41C);
            Console.WriteLine("FPGA Reg 0x41C = " + fpgaData.ToString("X"));
            FpgaLane0ErrCnt =  (fpgaData & 0xFFFF);
            FpgaLane1ErrCnt =  (fpgaData >> 16);
            Console.WriteLine("FPGA Lane 0 Error = " + FpgaLane0ErrCnt.ToString("X"));
            Console.WriteLine("FPGA Lane 1 Error = " + FpgaLane1ErrCnt.ToString("X"));
            Assert.Less( FpgaLane0ErrCnt, 0x1);
            Assert.Less( FpgaLane1ErrCnt, 0x1);

            //Reset Counters
            Link.fpgaWrite(0x404, (fpgaData | 0x8));  //Reset PRBS Error Counter
            fpgaData = Link.fpgaRead(0x404);
            Link.fpgaWrite(0x404, (fpgaData & 0xFFFFFFF7));  //Reset PRBS Error Counter
            fpgaData = Link.fpgaRead(0x404);
            Console.WriteLine("FPGA Reg 0x404 = " + fpgaData.ToString("X"));

            //Start Injecting Prbs Errors
            //Then Check FPGA error count
            Link.Mykonos.rxInjectPrbsError();
            System.Threading.Thread.Sleep(1000);
            Link.Mykonos.rxInjectPrbsError();
            System.Threading.Thread.Sleep(1000);
            Link.Mykonos.rxInjectPrbsError();
            System.Threading.Thread.Sleep(1000);
            fpgaData = Link.fpgaRead(0x41C);
            Console.WriteLine("FPGA Reg 0x41C = " + fpgaData.ToString("X"));
            FpgaLane0ErrCnt = (fpgaData & 0xFFFF);
            FpgaLane1ErrCnt = (fpgaData >> 16);
            Console.WriteLine("FPGA Lane 0 Error = " + FpgaLane0ErrCnt.ToString("X"));
            Console.WriteLine("FPGA Lane 1 Error = " + FpgaLane1ErrCnt.ToString("X"));
            Assert.LessOrEqual(FpgaLane0ErrCnt + FpgaLane1ErrCnt, 0x3);
            Assert.GreaterOrEqual(FpgaLane0ErrCnt +FpgaLane1ErrCnt, 0x1);

            Link.Disconnect(); 
        }

        ///<summary>
        /// API Test Name: 
        ///     ObsFramerInjectPrbsErrorCheck
        /// API Under-Test: 
        ///      Mykonos_enableORxFramerPrbs
        ///      MYKONOS_obsInjectPrbsError
        /// API Test Description: 
        ///     1)Initialise System and JESD Links
        ///     2)Enable PRBS Test Mode ON Mykonos and FPGA 
        ///     3) Clear FPGA PRBS counters then check error count 
        ///        is 0 following some delay
        ///     4) Call MYKONOS_rxInjectPrbsError and check FPGA
        ///     error counters again. 
        /// API Test Pass Criteria: 
        ///    No errors should be detected after enabling PRBS
        ///    Error should be detected after enabling InjectPrbsError
        /// Notes:
        /// x41C[31:16] When 43C0_0404.4 is 1’b1:
        ///             Lane 1 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             RX Sync error report counter  – clear with 43C0_0404.5
        /// x41C[15:0]  When 43C0_0404.4 is 1’b1:
        ///             Lane 0 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             ORX Sync error report counter  – clear with 43C0_0404.5
        /// x420[31:16] When 43C0_0404.4 is 1’b1:
        ///             Lane 3 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             0 
        /// x420[15:0]  When 43C0_0404.4 is 1’b1:
        ///             Lane 2 PRBS Error Counter – clear with 43C0_0404.3
        ///             When 43C0_0404.4 is 1’b0:
        ///             TX Sync error report counter – clear with 43C0_0404.5
        ///</summary>
        [Test]
        public static void ObsFramerInjectPrbsErrorCheck([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER PrbsOrder)
        {
           AdiCommandServerClient Link = AdiCommandServerClient.Instance;

            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 fpgaData = 0;
            byte mykData = 0;
            UInt32 FpgaLane2ErrCnt = 0;
            UInt32 FpgaLane3ErrCnt = 0;
            mykData = Link.spiRead(0x78); Console.WriteLine("SPI Reg x78 = " + mykData.ToString("X"));

            //Initialise System and JESD Links
            TestSetup.PrbsORxTestSetupInit(settings);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaData = Link.fpgaRead(0x410); Console.WriteLine("FPGA Reg x410: REFCLK Frequency Detect = 0x" + fpgaData.ToString("X") + " = " + fpgaData.ToString());
            fpgaData = Link.fpgaRead(0x10); Console.WriteLine("FPGA Version x10:" + fpgaData.ToString("X"));

            //Enable PRBS Test Mode ON Mykonos and FPGA 
            //Clear counters
            //Then Check error count is 0 following some delay
            Link.Mykonos.enableRxFramerPrbs(PrbsOrder, 1);
            fpgaData = Link.fpgaRead(0x404);
            switch(PrbsOrder)
            {
                case Mykonos.MYK_PRBS_ORDER.PRBS7:
                    fpgaData  |= 0x1;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS15:
                    fpgaData |= 0x2;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS31:
                    fpgaData |= 0x4;
                    break;
                default:
                    Assert.Fail("Invalid PRBS Order");
                    break;
            }
            //GTX PRBS Check Counter + GTX PRBS Config
            Link.fpgaWrite(0x404, fpgaData | 0x18);
            //Disable PRBS Error Counter Reset
            fpgaData = Link.fpgaRead(0x404);
            Link.fpgaWrite(0x404, (fpgaData & 0xFFFFFFF7));
            fpgaData = Link.fpgaRead(0x404);
            Console.WriteLine("FPGA Reg 0x404 = " + fpgaData.ToString("X"));
            //Read Counters for Lane 0 & 1
            fpgaData = Link.fpgaRead(0x420);
            Console.WriteLine("FPGA Reg 0x420 = " + fpgaData.ToString("X"));
            //Check Reset Was Successful
            Assert.AreEqual(fpgaData, 0x0);
            //Let PRBS Checker Run
            System.Threading.Thread.Sleep(5000);
            fpgaData = Link.fpgaRead(0x420);
            FpgaLane2ErrCnt = fpgaData & 0xFFFF;
            FpgaLane3ErrCnt = fpgaData >> 16;
            Console.WriteLine("FPGA Lane 2 Error = " + FpgaLane2ErrCnt.ToString("X"));
            Console.WriteLine("FPGA Lane 3 Error = " + FpgaLane3ErrCnt.ToString("X"));
            Assert.Less(FpgaLane2ErrCnt, 0x1);
            Assert.Less(FpgaLane3ErrCnt, 0x1);



            //Start Injecting Prbs Errors
            //Then Check FPGA error count
            Link.Mykonos.rxInjectPrbsError();
            System.Threading.Thread.Sleep(1000);
            fpgaData = Link.fpgaRead(0x420);
            FpgaLane2ErrCnt = fpgaData & 0xFFFF;
            FpgaLane3ErrCnt = fpgaData >> 16;
            Console.WriteLine("FPGA Lane 2 Error = " + FpgaLane2ErrCnt.ToString("X"));
            Console.WriteLine("FPGA Lane 3 Error = " + FpgaLane3ErrCnt.ToString("X"));
            Assert.LessOrEqual(FpgaLane2ErrCnt + FpgaLane3ErrCnt, 0xA);
            Assert.GreaterOrEqual(FpgaLane2ErrCnt + FpgaLane3ErrCnt, 0x1);
            Link.Disconnect();
        }
        ///<summary>
        /// API Test Name: 
        ///     TxDeFramerRunPrbsErrorCheck
        /// API Under-Test: 
        ///      Mykonos_enableDeFramerPrbsChecker
        ///      MYKONOS_readDeframerPrbsErrors
        ///      Mykonos_clearDeframerPrbsErrors
        /// API Test Description: 
        ///     1)Initialise System and JESD Links
        ///     2)Enable PRBS Test Mode ON Mykonos and FPGA 
        ///     3) Clear FPGA PRBS counters then check error count 
        ///        is 0 following some delay
        ///     4) Call MYKONOS_rxInjectPrbsError and check FPGA
        ///     error counters again. 
        ///     3) Clear FPGA PRBS counters then check error count 
        ///        is 0 no delay
        ///     4) Disable PRBS Checker
        /// API Test Pass Criteria: 
        ///    No errors should be detected after enabling PRBS
        ///    Error should be detected after enabling InjectPrbsError
        ///    Injected errors should be cleared after calling
        ///    Mykonos_clearDeframerPrbsErrors
        ///    
        /// Notes:
        ///</summary>
        [Test]
        public static void TxDeFramerRunPrbsErrorCheck([Values(Mykonos.MYK_PRBS_ORDER.PRBS7, Mykonos.MYK_PRBS_ORDER.PRBS15, Mykonos.MYK_PRBS_ORDER.PRBS31)]Mykonos.MYK_PRBS_ORDER PrbsOrder)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;

            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            UInt32 fpgaData = 0;
            byte mykData = 0;
            mykData = Link.spiRead(0x78); Console.WriteLine("SPI Reg x78 = " + mykData.ToString("X"));

            //Initialise System and JESD Links
            TestSetup.PrbsRxTestSetupInit(settings);
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            fpgaData = Link.fpgaRead(0x410);
            Console.WriteLine("FPGA Reg x410: REFCLK Frequency Detect = 0x" + fpgaData.ToString("X") + " = " + fpgaData.ToString());
            fpgaData = Link.fpgaRead(0x10);
            Console.WriteLine("FPGA Version x10:" + fpgaData.ToString("X"));

            //Enable PRBS Generator on FPGA 
            //GTX PRBS Config FPGA 0x404[10:8]
            //Then Enable PRBS Error Checker on Tx Deframer
            fpgaData = Link.fpgaRead(0x404);
            switch (PrbsOrder)
            {
                case Mykonos.MYK_PRBS_ORDER.PRBS7:
                    fpgaData |= 0x100 ;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS15:
                    fpgaData |= 0x200;
                    break;
                case Mykonos.MYK_PRBS_ORDER.PRBS31:
                    fpgaData |= 0x400;
                    break;
                default:
                    Assert.Fail("Invalid PRBS Order");
                    break;
            }
            Link.fpgaWrite(0x404, fpgaData);
            Link.Mykonos.enableDeframerPrbsChecker(0xF, PrbsOrder, 1);
            Link.Mykonos.clearDeframerPrbsCounters();
            //Let PRBS Run
            uint [] LaneError = {0xFF,0xFF, 0xFF,0xFF};
            System.Threading.Thread.Sleep(5000);
            for(byte i=0; i<4; i++)
            {
                Link.Mykonos.ReadDeframerPrbsCounters(i, ref LaneError[i]);
                Assert.AreEqual(0x0, LaneError[i]);
                LaneError[i] = 0x0;
            }

            //Enable Error Injection of FPGA PRBS Generator
            fpgaData = Link.fpgaRead(0x404);
            Link.fpgaWrite(0x404, (fpgaData | 0x800));
            //Let PRBS Run
            System.Threading.Thread.Sleep(5000);
            for (byte i = 0; i < 4; i++)
            {
                Link.Mykonos.ReadDeframerPrbsCounters(i, ref LaneError[i]);
                Assert.GreaterOrEqual(LaneError[i], 0x1);
            }
    
            //Check ClearDeframerPrbsCounters Clears Injected Errors
            Link.Mykonos.clearDeframerPrbsCounters();
            for (byte i = 0; i < 4; i++)
            {
                Link.Mykonos.ReadDeframerPrbsCounters(i, ref LaneError[i]);
                Assert.AreEqual(0x0, LaneError[i]);
                LaneError[i] = 0x0;
            }
            Link.Mykonos.enableDeframerPrbsChecker(0xF, PrbsOrder, 0);
            Link.Disconnect();
        }
    }
}
