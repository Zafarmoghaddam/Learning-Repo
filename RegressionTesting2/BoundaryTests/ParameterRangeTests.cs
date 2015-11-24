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

    public class RangeMykonosSettings
    {
        public UInt32 DeviceClock_kHz = 245760;
        public byte rxChannel = 3; //0=off, 1=Rx1, 2=Rx2, 3=Rx1+Rx2
        public byte txChannel = 3; //0=off, 1=Tx1, 2=Tx2, 3=Tx1+Tx2
        public string rxProfileName = "Rx 100MHz, IQrate 122.88MHz, Dec5";
        public string txProfileName = "Tx 75/200MHz, IQrate 245.76MHz, Dec5";
        public string orxProfileName = "ORX 200MHz, IQrate 245.75MHz, Dec5";
        public string srxProfileName = "SRx 20MHz, IQrate 30.72MHz, Dec5";


    }

    public static class RangeTestSettings
    {
        public static String ipAddr = "192.168.1.10";
        public static Int32 port = 55555;
        
        public static RangeMykonosSettings RangemykSettings = new RangeMykonosSettings();

    }

    [TestFixture]
    [Category("Boundary")]
    public class ParameterRangeTests
    {
        public static String resFilePath = @"..\..\..\TestResults\ParameterRangeTests\InvalidParameterTestsResult.txt";
        /// <summary>
        /// Tests MYKONOS_writeArmMem API function
        /// Valid address range: Program Memory (0x01000000 - 0x01017FFF), Data Memory (0x20000000 - 0x2000FFFF)
        /// </summary>
        [Test]
        public static void writeArmMem()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            Link.hw.ReceiveTimeout = 0;

            //UInt32 address = 0x00FFFFFF;
            UInt32 address;
            UInt32 numBytes = 1;
            byte[] dataBytes = { 0x0 };
            //Invalid, address < min
            string text;

            for (address = 0x00FFFFFF; address > 0x00FFFFF0; address--)
            {
                text = "Run writeArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.writeArmMem(address, numBytes, dataBytes); });

            }

            for (address = 0x01018000; address < 0x101800F; address++)
            {
                text = "Run writeArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                //Invalid, address > max

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.writeArmMem(address, numBytes, dataBytes); });
            }

            for (address = 0x20010000; address < 0x2001000F; address++)
            {
                text = "Run writeArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                //Invalid, address > max
                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.writeArmMem(address, numBytes, dataBytes); });
            }
        }

        /// <summary>
        /// Tests MYKONOS_readArmMem API function
        /// Valid address range: Program Memory (0x01000000 - 0x01017FFF), Data Memory (0x20000000 - 0x2000FFFF)
        /// </summary>
        [Test]
        public static void readArmMem()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            Link.hw.ReceiveTimeout = 0;

            //UInt32 address = 0x00FFFFFF;
            UInt32 address;
            Int32 numBytes = 1;
            byte[] dataBytes = { 0x0 };
            string text;
            //Invalid, address < min
            for (address = 0x00FFFFFF; address > 0x00FFFFF0; address--)
            {
                text = "Run readArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.readArmMem(address, numBytes, 1, ref dataBytes); });
            }

            for (address = 0x01018000; address < 0x101800F; address++)
            {
                text = "Run readArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                //Invalid, address > max
                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.readArmMem(address, numBytes, 1, ref dataBytes); });
            }

            for (address = 0x20010000; address < 0x2001000F; address++)
            {
                text = "Run readArmMemInvalid, address = " + address + ", numBytes = " + numBytes;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                //Invalid, address > max
                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.readArmMem(address, numBytes, 1, ref dataBytes); });
            }
        }

        /// <summary>
        /// Tests MYKONOS_sendArmCommand API function
        /// Valid opcode range: 0-30, Only even opcodes
        /// Valid extendedDataNumBytes range: 0-7
        /// </summary>
        [Test]
        public static void sendArmCommand()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            //ARMCommand(0x0A, 0x69)
            byte[] dataBytes = { 0x69 };
            string text;
            for (byte opcode = 27; opcode < 36; opcode+=2)
            {
                text = "Run sendArmCommandInvalid, opcode = " + opcode;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }
                //Invalid opcode
                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.sendArmCommand(opcode, dataBytes, 1); });
            }


            text = "Run sendArmCommandInvalid, Numbytes = 8";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            //Invalid NumByte
            byte[] dataBytes2 = { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

            Exception ex2 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.sendArmCommand(0xA, dataBytes2, 8); });
            //To Do: Numdata != DataByte
        }

        /// <summary>
        /// Tests MYKONOS_readArmCmdStatByte API function
        /// Valid opcode range: 0-30, Only even opcodes
        /// </summary>
        [Test]
        public static void readArmCmdStatByte()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            byte statusByte = 0;
            byte opcode;
            //Invalid opcode

            for (opcode = 27; opcode < 36; opcode+=2)
            {
                string text = "Run readArmCmdStatByteInvalid, opdocde = " + opcode;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.readArmCmdStatByte(opcode, ref statusByte); });
            }
        }

        /// <summary>
        /// Tests MYKONOS_waitArmCmdStatus API function
        /// Valid opcode range: 0-30, Only even opcodes
        /// </summary>
        [Test]
        public static void waitArmCmdStatus()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            byte statusByte = 0;
            UInt32 timeOut_ms = 20000;
            //Invalid opcode

            for (byte opcode = 27; opcode < 36; opcode+=2)
            {
                string text = "Run waitArmCmdStatusInvalid, opdocde = " + opcode;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.waitArmCmdStatus(opcode, timeOut_ms, ref statusByte); });
            }
        }

        /// <summary>
        /// Tests MYKONOS_readDeframerPrbsCounters API function
        /// Valid lane select range: 0-3
        /// </summary>
        [Test]
        public static void ReadDeserializerPrbsCounters()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            byte laneSelect;
            for (laneSelect = 4; laneSelect < 8; laneSelect++)
            {
                string text = "Run ReadDeserializerPrbsCountersInvalid, laneSelect = " + laneSelect;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.readDeframerPrbsCounters(laneSelect); });
            }
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setRfPllFrequency API function
        /// Valid RFPLL frequency range: 46MHz to 6100Mhz
        /// Valid lane select range: 0-3
        /// </summary>
        [Test]
        public static void SetTxPllFrequency()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);
            Link.setSpiChannel(1);
            //Link.spiWrite(0x2B0, 0x4);

            //Link.spiWrite(0x233, 0x1B);
            string text;
            UInt64 rfPllFrequency_Hz;
            for (rfPllFrequency_Hz = 6100000001; rfPllFrequency_Hz < 6100000010; rfPllFrequency_Hz++)
            {
                text = "Run SetTxPllFrequencyInvalid, rfPllFrequency_Hz = " + rfPllFrequency_Hz;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, rfPllFrequency_Hz); });
                //Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, rfPllFrequency_Hz);
            }

            for (rfPllFrequency_Hz = 45999999; rfPllFrequency_Hz > 45999990; rfPllFrequency_Hz--)
            {
                text = "Run SetTxPllFrequencyInvalid, rfPllFrequency_Hz = " + rfPllFrequency_Hz;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.RX_PLL, rfPllFrequency_Hz); });
            }

            rfPllFrequency_Hz = 250000000;
            int pll = 4;
            text = "Run SetTxPllFrequencyInvalid, rfPllFrequency_Hz = " + rfPllFrequency_Hz + ", pll value: " + pll;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex2 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setRfPllFrequency((Mykonos.PLLNAME)pll, rfPllFrequency_Hz); });

            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setTx1Attenuation API function
        /// Valid attenuation range(MHz): 0 to 41950
        /// </summary>
        [Test]
        public static void SetTx1Atten()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);
            Link.hw.ReceiveTimeout = 0;

            double attenuation_dB;
            for (attenuation_dB = 41.96; attenuation_dB < 42; attenuation_dB += 0.01)
            {
                string text = "Run SetTx1AttenInvalid, attenuation_dB = " + attenuation_dB;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setTx1Attenuation(attenuation_dB); });
            }
            Link.hw.ReceiveTimeout = 5000;
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setTx2Attenuation API function
        /// Valid attenuation range(MHz): 0 to 41950
        /// </summary>
        [Test]
        public static void SetTx2Atten()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);
            Link.hw.ReceiveTimeout = 0;
            double attenuation_dB;
            for (attenuation_dB = 41.96; attenuation_dB < 42; attenuation_dB += 0.01)
            {
                string text = "Run SetTx2AttenInvalid, attenuation_dB = " + attenuation_dB;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                {
                    file.WriteLine(text);
                }

                Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setTx2Attenuation(attenuation_dB); });
            }
            Link.hw.ReceiveTimeout = 5000;
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_enableRxFramerPrbs API function
        /// Valid polyorder range: 0 to 2
        /// Valid enable bit: 0 or 1
        /// </summary>
        [Test]
        public static void enableRxFramerPrbs()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int order = 3;
            byte enable = 1;
            string text = "Run enableRxFramerPrbsInvalid, PRBS order value = " + order + ", enable = " + enable;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.enableRxFramerPrbs((Mykonos.MYK_PRBS_ORDER)order, enable); });

            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_enableObsRxFramerPrbs API function
        /// Valid polyorder range: 0 to 2
        /// Valid enable bit: 0 or 1
        /// </summary>
        [Test]
        public static void enableObsRxFramerPrbs()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int order = 3;
            byte enable = 1;
            string text = "Run enableObsRxFramerPrbsInvalid, PRBS order value = " + order + ", enable = " + enable;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.enableObsRxFramerPrbs((Mykonos.MYK_PRBS_ORDER)order, enable); });

            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_enableDeframerPrbsChecker API function
        /// Valid polyorder range: 0 to 2
        /// Valid enable bit: 0 or 1
        /// Valid lane: 4bit mask, bit per (0-F) 
        /// </summary>
        [Test]
        public static void enableDeframerPrbsChecker()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int order = 3;
            byte enable = 1;
            byte lane = 0xF;
            string text = "Run enableDeframerPrbsCheckerInvalid, PRBS order value = " + order + ", enable = " + enable + ", lane" + lane;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.enableDeframerPrbsChecker(lane, (Mykonos.MYK_PRBS_ORDER)order, enable); });

            order = 2;
            enable = 2;
            text = "Run enableDeframerPrbsCheckerInvalid, PRBS order value = " + order + ", enable = " + enable + ", lane" + lane;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            Exception ex2 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.enableDeframerPrbsChecker(lane, (Mykonos.MYK_PRBS_ORDER)order, enable); });

            enable = 0;
            lane = 0x10;
            text = "Run enableDeframerPrbsCheckerInvalid, PRBS order value = " + order + ", enable = " + enable + ", lane" + lane;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex3 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.enableDeframerPrbsChecker(lane, Mykonos.MYK_PRBS_ORDER.PRBS7, enable); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setObsRxPathSource API function
        /// enum OBSRXCHANNEL
        /// {
        ///    OBS_RXOFF = 0,
        ///    OBS_RX1_TXLO = 1,
        ///    OBS_RX2_TXLO = 2,
        ///    OBS_INTERNALCALS = 3,
        ///    OBS_SNIFFER = 4,
        ///    OBS_RX1_SNIFFERLO = 5,
        ///    OBS_RX2_SNIFFERLO = 6,
        ///    OBS_SNIFFER_A = 0x14,  // Valid in ARM Command Mode, not pin mode
        ///    OBS_SNIFFER_B = 0x24,  // Valid in ARM Command Mode, not pin mode
        ///    OBS_SNIFFER_C = 0x34   // Valid in ARM Command Mode, not pin mode
        /// };
        /// </summary>
        [Test]
        public static void setObsRxPathSource()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int channel = 7;

            string text = "Run setObsRxPathSourceInvalid, channel value = " + channel;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setObsRxPathSource((Mykonos.OBSRXCHANNEL)channel); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_programRxGainTable API function
        /// public enum RXGAIN_TABLE
        /// {
        ///     RX1_GT = 1,
        ///     RX2_GT = 2,
        ///     RX1_RX2_GT = 3,
        ///     ORX_GT = 4,
        ///     SNRX_GT = 5
        /// };
        /// </summary>
        [Test]
        public static void programRxGainTable()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int table = 7;
            string text = "Run programRxGainTableInvalid, table value = " + table;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.programRxGainTable(@"..\..\..\Resources\GainTables\orxGainTable_debug.csv", (Mykonos.RXGAIN_TABLE)table); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_loadarmfrombinary API function
        /// test.bin is created by changing a randon test.txt file to test.bin
        /// </summary>
        [Test]
        public static void LoadArm()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            string text = "Run LoadArmInvalid.";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            String fileName = @"..\..\..\Resources\ArmBinaries\test.bin";

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.loadArm(fileName); });
            Link.Disconnect();

        }

        /// <summary>
        /// Tests MYKONOS_setRxGainControlMode API function
        /// enum GAINMODE { MGC = 0, AGC = 2, HYBRID = 3 };
        /// </summary>
        [Test]
        public static void setRxGainControlMode()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int mode = 4;

            string text = "Run setRxGainControlModeInvalid, channel value = " + mode;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setRxGainControlMode((Mykonos.GAINMODE)mode); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setObsRxGainControlMode API function
        /// enum GAINMODE { MGC = 0, AGC = 2, HYBRID = 3 };
        /// </summary>
        [Test]
        public static void setObsRxGainControlMode()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int mode = 4;

            string text = "Run setRxGainControlModeInvalid, channel value = " + mode;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setObsRxGainControlMode((Mykonos.GAINMODE)mode); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_waitForEvent API function
        /// Valid range of enum WAIT_EVENT: 0-18
        /// </summary>
        [Test]
        public static void waitForEvent()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int num = 20;

            string text = "Run waitForEventInvalid, channel value = " + num;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.waitForEvent((Mykonos.WAIT_EVENT)num, 5000); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_setObsRxManualGain API function
        /// enum OBSRXCHANNEL
        /// {
        ///    OBS_RXOFF = 0,
        ///    OBS_RX1_TXLO = 1,
        ///    OBS_RX2_TXLO = 2,
        ///    OBS_INTERNALCALS = 3,
        ///    OBS_SNIFFER = 4,
        ///    OBS_RX1_SNIFFERLO = 5,
        ///    OBS_RX2_SNIFFERLO = 6,
        ///    OBS_SNIFFER_A = 0x14,  // Valid in ARM Command Mode, not pin mode
        ///    OBS_SNIFFER_B = 0x24,  // Valid in ARM Command Mode, not pin mode
        ///    OBS_SNIFFER_C = 0x34   // Valid in ARM Command Mode, not pin mode
        /// };
        /// </summary>
        [Test]
        public static void setObsRxManualGain()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            int mode = 7;
            byte gainIndex = 1;

            string text = "Run setObsRxManualGainInvalid, channel value = " + mode;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.setObsRxManualGain((Mykonos.OBSRXCHANNEL)mode, gainIndex); });
            Link.Disconnect();
        }

        /// <summary>
        /// Tests MYKONOS_ProgramFir API function
        /// Valid range of #taps:
        /// Rx filters can have 24, 48, 72, or 96 taps.  
        /// Tx filters can have 16, 32, 48, 64, 80, 112, or 128 taps.
        /// </summary> 
        [Test]
        public static void ProgramFir()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            string text = "Run ProgramFirInvalid.";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }

            String fileName = @"..\..\..\Resources\DigitalFilters\Tx_invalid.ftr";

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.programFir(Mykonos.FIR.TX1_FIR, fileName); });
//            Link.Mykonos.programFir(Mykonos.FIR.TX1_FIR, fileName);

            fileName = @"..\..\..\Resources\DigitalFilters\Rx_invalid.ftr";
            Exception ex2 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.programFir(Mykonos.FIR.RX1_FIR, fileName); });

            Link.Disconnect();

        }
        /// <summary>
        /// Tests MYKONOS_getClgcStatus(..) & MYKONOS_getDpdStatus(..)
        /// Tx1 and Tx2 are only valid parameters for TxChannel
        /// This test verifies exception is thrown when Tx1Tx2 is set as parameter
        /// </summary> 
        [Test]
        public static void CheckDpdStatus()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(RangeTestSettings.ipAddr, RangeTestSettings.port);

            string text = "Run CheckDpdStatusInvalid.";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            MykClgcStatus tx1ClgcStatus;
            MykDpdStatus tx1DpdStatus;

            Exception ex = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.getDpdStatus(Mykonos.TXCHANNEL.TX1_TX2, out tx1DpdStatus); });

            Exception ex2 = Assert.Throws<Exception>(
                    delegate { Link.Mykonos.getClgcStatus(Mykonos.TXCHANNEL.TX1_TX2, out tx1ClgcStatus); });

            Link.Disconnect();

        }

        /// <summary>
        /// Tests MYKONOS_setupRxAgc(..)
        /// All the parameters have valid ranges based on bit size and other parameters
        /// Test all possible parameters (some can't be tested) and verify that the error
        /// and the error message correspond to the correct parameter
        /// </summary> 
        [Test]
        public static void CheckSetupRxAgc()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            ParameterRangeTests helperclass = new ParameterRangeTests();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            // Copied default values from Kevin's code
            // All the values can go into the three structures
            byte spiData = 0x0;

            //(device->rx->rxAgcCtrl->agcRx1MaxGainIndex > device->rx->rxGainCtrl->rx1MaxGainIndex) 
            //Can't trigger condition since rx1MaxGainIndex = 255

            //(device->rx->rxAgcCtrl->agcRx1MaxGainIndex < device->rx->rxAgcCtrl->agcRx1MinGainIndex)
            helperclass.SetupRxAgcHelper(Link, agcRx1MaxGainIndex: 190);
            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcRx1MaxGainIndex out of range in MYKONOS_setupRxAgc() \n", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->agcRx1MinGainIndex < device->rx->rxGainCtrl->rx1MinGainIndex)
            helperclass.SetupRxAgcHelper(Link, agcRx1MinGainIndex: 180);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcRx1MinGainIndex out of range in MYKONOS_setupRxAgc() \n", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->agcRx1MaxGainIndex < device->rx->rxAgcCtrl->agcRx1MinGainIndex)
            //Can't trigger condition due to a previous condition   

            //(device->rx->rxAgcCtrl->agcRx2MaxGainIndex > device->rx->rxGainCtrl->rx2MaxGainIndex)
            //Can't triger since rx2MaxGainIndex is 255

            //(device->rx->rxAgcCtrl->agcRx2MaxGainIndex < device->rx->rxAgcCtrl->agcRx2MinGainIndex)
            helperclass.SetupRxAgcHelper(Link, agcRx2MaxGainIndex: 190);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcRx2MaxGainIndex out of range in MYKONOS_setupRxAgc() \n", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcRx2MinGainIndex < device->rx->rxGainCtrl->rx2MinGainIndex)
            helperclass.SetupRxAgcHelper(Link, agcRx2MinGainIndex: 190);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcRx2MinGainIndex out of range in MYKONOS_setupRxAgc() \n", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcRx2MaxGainIndex < device->rx->rxAgcCtrl->agcRx2MinGainIndex)
            helperclass.SetupRxAgcHelper(Link, agcRx2MinGainIndex: 254);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcRx2MaxGainIndex out of range in MYKONOS_setupRxAgc() \n", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcGainUpdateCounter > 0x3FFFFF) 
            helperclass.SetupRxAgcHelper(Link, agcGainUpdateCounter: 0x4FFFFF);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcGainUpdateTime_us out of range in MYKONOS_setupRxAgc()\n", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcGainUpdateCounter < 1)
            helperclass.SetupRxAgcHelper(Link, agcGainUpdateCounter: 0);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcGainUpdateTime_us out of range in MYKONOS_setupRxAgc()\n", ex.Message, "Error message incorrect");

            //device->rx->rxAgcCtrl->agcPeakWaitTime > 0x1F
            helperclass.SetupRxAgcHelper(Link, agcPeakWaitTime: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcPeakWaitTime out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //device->rx->rxAgcCtrl->agcPeakWaitTime < 0x02
            helperclass.SetupRxAgcHelper(Link, agcPeakWaitTime: 0x01);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcPeakWaitTime out of range in MYKONOS_setupRxAgc()\n", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->agcSlowLoopSettlingDelay > 0x7F)
            helperclass.SetupRxAgcHelper(Link, agcSlowLoopSettlingDelay: 0x8F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcSlowLoopSettlingDelay out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //((1 << (3 + device->rx->rxAgcCtrl->powerAgc->pmdMeasDuration)) >= (device->rx->rxAgcCtrl->agcGainUpdateCounter))
            helperclass.SetupRxAgcHelper(Link, agcGainUpdateCounter: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdMeasDuration out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->powerAgc->pmdMeasConfig > 0x3)
            helperclass.SetupRxAgcHelper(Link, pmdMeasConfig: 0x4);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdMeasConfig out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcLowThsPreventGainIncrease > 1)
            helperclass.SetupRxAgcHelper(Link, agcLowThsPreventGainIncrease: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcLowThsPreventGainIncrease out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->agcPeakThresholdMode > 1)
            helperclass.SetupRxAgcHelper(Link, agcPeakThresholdMode: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcPeakThresholdMode out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->agcResetOnRxEnable > 1)
            helperclass.SetupRxAgcHelper(Link, agcResetOnRxEnable: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcResetOnRxEnable out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->agcEnableSyncPulseForGainCounter > 1)
            helperclass.SetupRxAgcHelper(Link, agcEnableSyncPulseForGainCounter: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->agcEnableSyncPulseForGainCounter out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->powerAgc->pmdLowerHighThresh <= device->rx->rxAgcCtrl->powerAgc->pmdUpperLowThresh) 
            helperclass.SetupRxAgcHelper(Link, pmdLowerHighThresh: 3);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdLowerHighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // device->rx->rxAgcCtrl->powerAgc->pmdLowerHighThresh > 0x7F
            helperclass.SetupRxAgcHelper(Link, pmdLowerHighThresh: 0x8F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdLowerHighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdUpperLowThresh > 0x7F)
            //Can't trigger since a previous condition requires pmdLowerHighThresh > pmdUpperLowThresh and pmdLowerHighThresh < 0x7F

            // (device->rx->rxAgcCtrl->powerAgc->pmdLowerLowThresh > 0xF)
            helperclass.SetupRxAgcHelper(Link, pmdLowerLowThresh: 0x10);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdLowerLowThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdUpperHighThresh > 0xF)
            helperclass.SetupRxAgcHelper(Link, pmdUpperHighThresh: 0x10);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdUpperHighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdUpperHighGainStepAttack > 0x1F)
            helperclass.SetupRxAgcHelper(Link, pmdUpperHighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdUpperHighGainStepAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdLowerLowGainStepRecovery > 0x1F)
            helperclass.SetupRxAgcHelper(Link, pmdLowerLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdLowerLowGainStepRecovery out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdUpperLowGainStepAttack > 0x1F)
            helperclass.SetupRxAgcHelper(Link, pmdUpperLowGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdUpperLowGainStepAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->powerAgc->pmdLowerHighGainStepRecovery > 0x1F)
            helperclass.SetupRxAgcHelper(Link, pmdLowerHighGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->powerAgc->pmdLowerHighGainStepRecovery out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");
            //wrong error maybe

            // device->rx->rxAgcCtrl->peakAgc->apdFastAttack > 0x1
            helperclass.SetupRxAgcHelper(Link, apdFastAttack: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->apdFastAttack or hb2FastAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // device->rx->rxAgcCtrl->peakAgc->hb2FastAttack > 0x1
            helperclass.SetupRxAgcHelper(Link, hb2FastAttack: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->apdFastAttack or hb2FastAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->peakAgc->apdHighThresh > 0x3F)
            helperclass.SetupRxAgcHelper(Link, apdHighThresh: 0x4F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->apdHighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->peakAgc->apdHighThresh <= device->rx->rxAgcCtrl->peakAgc->apdLowThresh)
            helperclass.SetupRxAgcHelper(Link, apdHighThresh: 0x16);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->apdHighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            // (device->rx->rxAgcCtrl->peakAgc->apdLowThresh > 0x3F) 
            //Can't triger since apdHighThresh > apdLowThresh and apdHighThresh > 0x3F


            //(device->rx->rxAgcCtrl->peakAgc->apdHighThresh < device->rx->rxAgcCtrl->peakAgc->apdLowThresh)
            //Can't trigger this condition because of a previous condition

            // (device->rx->rxAgcCtrl->peakAgc->hb2HighThresh > 0xFF) 
            //Can't trigger this condition since hb2HighThresh is a byte

            // (device->rx->rxAgcCtrl->peakAgc->hb2HighThresh < device->rx->rxAgcCtrl->peakAgc->hb2LowThresh)
            helperclass.SetupRxAgcHelper(Link, hb2HighThresh: 0x80);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2HighThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->peakAgc->hb2LowThresh > 0xFF) 
            //Can't trigger since hb2LowThresh is a byte

            //(device->rx->rxAgcCtrl->peakAgc->hb2LowThresh > device->rx->rxAgcCtrl->peakAgc->hb2HighThresh)
            //Can't trigger since its the same as a previous condition

            //(device->rx->rxAgcCtrl->peakAgc->hb2VeryLowThresh > 0xFF)
            //Can't trigger since hb2VeryLowThresh is a byte

            //(device->rx->rxAgcCtrl->peakAgc->hb2VeryLowThresh > device->rx->rxAgcCtrl->peakAgc->hb2LowThresh)
            helperclass.SetupRxAgcHelper(Link, hb2VeryLowThresh: 0x82);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2VeryLowThresh out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");

            //(device->rx->rxAgcCtrl->peakAgc->apdHighGainStepAttack > 0x1F)
            helperclass.SetupRxAgcHelper(Link, apdHighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->apdHighGainStepAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->apdLowGainStepRecovery > 0x1F)
            helperclass.SetupRxAgcHelper(Link, apdLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->apdLowGainStepRecovery out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->hb2HighGainStepAttack > 0x1F)
            helperclass.SetupRxAgcHelper(Link, hb2HighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2HighGainStepAttack out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->hb2LowGainStepRecovery > 0x1F)
            helperclass.SetupRxAgcHelper(Link, hb2LowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2LowGainStepRecovery out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->hb2VeryLowGainStepRecovery > 0x1F)
            helperclass.SetupRxAgcHelper(Link, hb2VeryLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2VeryLowGainStepRecovery out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->hb2OverloadDetectEnable > 0x1)
            helperclass.SetupRxAgcHelper(Link, hb2OverloadDetectEnable: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2OverloadEnable out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");
            //not error message atm

            //(device->rx->rxAgcCtrl->peakAgc->hb2OverloadDurationCnt > 0x7)
            helperclass.SetupRxAgcHelper(Link, hb2OverloadDurationCnt: 0x8);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2OverloadDurationCnt out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");


            //(device->rx->rxAgcCtrl->peakAgc->hb2OverloadThreshCnt > 0xF)
            helperclass.SetupRxAgcHelper(Link, hb2OverloadThreshCnt: 0x1F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupRxAgc(); ; });
            StringAssert.Contains("device->rx->rxAgcCtrl->peakAgc->hb2OverloadThreshCnt out of range in MYKONOS_setupRxAgc()", ex.Message, "Error message incorrect");

            Console.WriteLine(ex.Message);
        }


        //Helper function
        void SetupRxAgcHelper(AdiCommandServerClient Link, byte agcRx1MaxGainIndex = 250,
            byte agcRx1MinGainIndex = 198,
            byte agcRx2MaxGainIndex = 250,
            byte agcRx2MinGainIndex = 198,
            byte agcObsRxMaxGainIndex = 0,
            byte agcObsRxMinGainIndex = 0,
            byte agcObsRxSelect = 0,
            byte agcPeakThresholdMode = 1,// Change for power only mode
            byte agcLowThsPreventGainIncrease = 1, // Change for power only mode
            UInt32 agcGainUpdateCounter = 30721,
            byte agcSlowLoopSettlingDelay = 1,
            byte agcPeakWaitTime = 5,
            byte pmdMeasDuration = 0x07,
            byte pmdMeasConfig = 0x3,
            byte agcResetOnRxEnable = 0,
            byte agcEnableSyncPulseForGainCounter = 1,
            // mykonosPowerMeasAgcCfg_t
            byte pmdUpperHighThresh = 0x02, // Triggered at approx -2dBFS
            byte pmdUpperLowThresh = 0x04,
            byte pmdLowerHighThresh = 0x0C,
            byte pmdLowerLowThresh = 0x05,
            byte pmdUpperHighGainStepAttack = 0x05,
            byte pmdUpperLowGainStepAttack = 0x01,
            byte pmdLowerHighGainStepRecovery = 0x01,
            byte pmdLowerLowGainStepRecovery = 0x05,
            // mykonosPeakDetAgcCfg_t
            byte apdHighThresh = 0x1E, //Triggered at approx -3dBFS
            byte apdLowThresh = 0x17, //Triggered at approx -5.5dBFS
            byte hb2HighThresh = 0xB6, // Triggered at approx -2.18dBFS
            byte hb2LowThresh = 0x81, // Triggered at approx -5.5dBFS
            byte hb2VeryLowThresh = 0x41, // Triggered at approx -9dBFS
            byte apdHighThreshExceededCnt = 0x0B,
            byte apdLowThreshExceededCnt = 0x04,
            byte hb2HighThreshExceededCnt = 0x0B,
            byte hb2LowThreshExceededCnt = 0x04,
            byte hb2VeryLowThreshExceededCnt = 0x04,
            byte apdHighGainStepAttack = 0x01,
            byte apdLowGainStepRecovery = 0x01,
            byte hb2HighGainStepAttack = 0x01,
            byte hb2LowGainStepRecovery = 0x01,
            byte hb2VeryLowGainStepRecovery = 0x01,
            byte apdFastAttack = 0,
            byte hb2FastAttack = 0,
            byte hb2OverloadDetectEnable = 0,
            byte hb2OverloadDurationCnt = 5,
            byte hb2OverloadThreshCnt = 0x9)
        {
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


        }


        /// <summary>
        /// Tests MYKONOS_setupObsRxAgc(..)
        /// All the parameters have valid ranges based on bit size and other parameters
        /// Test all possible parameters (some can't be tested) and verify that the error
        /// and the error message correspond to the correct parameter
        /// </summary> 
        [Test]
        public static void CheckSetupObsRxAgc()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            ParameterRangeTests helperclass = new ParameterRangeTests();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            // Copied default values from Kevin's code
            // All the values can go into the three structures
            byte spiData = 0x0;

            //(device->obsRx->orxAgcCtrl->agcObsRxMaxGainIndex > device->obsRx->snifferGainCtrl->maxGainIndex)
            //Can't trigger condition since maxGainIndex = 255

            //(device->obsRx->orxAgcCtrl->agcObsRxMaxGainIndex < device->obsRx->orxAgcCtrl->agcObsRxMinGainIndex)
            helperclass.SetupObsRxAgcHelper(Link, agcObsRxMaxGainIndex: 210);
            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcObsRxMaxGainIndex out of range in  MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->agcObsRxMinGainIndex < device->obsRx->snifferGainCtrl->minGainIndex)
            helperclass.SetupObsRxAgcHelper(Link, agcObsRxMinGainIndex: 200);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcObsRxMinGainIndex out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->agcObsRxMaxGainIndex < device->obsRx->orxAgcCtrl->agcObsRxMinGainIndex)
            //Can't trigger condition since it is the same as a previous condition

            //(device->obsRx->orxAgcCtrl->agcObsRxSelect > 1)
            helperclass.SetupObsRxAgcHelper(Link, agcObsRxSelect: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcObsRxSelect out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->agcGainUpdateCounter > 0x3FFFFF) 
            helperclass.SetupObsRxAgcHelper(Link, agcGainUpdateCounter: 0x4FFFFF);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcGainUpdateTime_us out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->agcGainUpdateCounter < 1)
            helperclass.SetupObsRxAgcHelper(Link, agcGainUpdateCounter: 0);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcGainUpdateTime_us out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //device->obsRx->orxAgcCtrl->agcPeakWaitTime > 0x1F
            helperclass.SetupObsRxAgcHelper(Link, agcPeakWaitTime: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcPeakWaitTime out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //device->obsRx->orxAgcCtrl->agcPeakWaitTime < 0x02
            helperclass.SetupObsRxAgcHelper(Link, agcPeakWaitTime: 0x01);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcPeakWaitTime out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->agcSlowLoopSettlingDelay > 0x7F)
            helperclass.SetupObsRxAgcHelper(Link, agcSlowLoopSettlingDelay: 0x8F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcSlowLoopSettlingDelay out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(1 << (3 + device->obsRx->orxAgcCtrl->powerAgc->pmdMeasDuration)) >= (device->obsRx->orxAgcCtrl->agcGainUpdateCounter)
            helperclass.SetupObsRxAgcHelper(Link, agcGainUpdateCounter: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdMeasDuration out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdMeasConfig > 0x3)
            helperclass.SetupObsRxAgcHelper(Link, pmdMeasConfig: 0x4);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdMeasConfig out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->agcLowThsPreventGainIncrease > 1)
            helperclass.SetupObsRxAgcHelper(Link, agcLowThsPreventGainIncrease: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcLowThsPreventGainIncrease out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->agcPeakThresholdMode > 1)
            helperclass.SetupObsRxAgcHelper(Link, agcPeakThresholdMode: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcPeakThresholdMode out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->agcResetOnRxEnable > 1)
            helperclass.SetupObsRxAgcHelper(Link, agcResetOnRxEnable: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcResetOnRxEnable out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->agcEnableSyncPulseForGainCounter > 1)
            helperclass.SetupObsRxAgcHelper(Link, agcEnableSyncPulseForGainCounter: 2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->agcEnableSyncPulseForGainCounter out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighThresh <= device->obsRx->orxAgcCtrl->powerAgc->pmdUpperLowThresh)
            helperclass.SetupObsRxAgcHelper(Link, pmdLowerHighThresh: 3);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            // device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighThresh > 0x7F
            helperclass.SetupObsRxAgcHelper(Link, pmdLowerHighThresh: 0x8F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            // (device->obsRx->orxAgcCtrl->powerAgc->pmdUpperLowThresh > 0x7F)
            //Can't trigger since a previous condition requires pmdLowerHighThresh > pmdUpperLowThresh and pmdLowerHighThresh < 0x7F

            // (device->obsRx->orxAgcCtrl->powerAgc->pmdLowerLowThresh > 0xF)
            helperclass.SetupObsRxAgcHelper(Link, pmdLowerLowThresh: 0x10);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdLowerLowThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdUpperHighThresh > 0xF)
            helperclass.SetupObsRxAgcHelper(Link, pmdUpperHighThresh: 0x10);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdUpperHighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdUpperHighGainStepAttack > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, pmdUpperHighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdUpperHighGainStepAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdLowerLowGainStepRecovery > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, pmdLowerLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdLowerLowGainStepRecovery out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->powerAgc->pmdUpperLowGainStepAttack > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, pmdUpperLowGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdUpperLowGainStepAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");
            //wrong error maybe

            //(device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighGainStepRecovery > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, pmdLowerHighGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->powerAgc->pmdLowerHighGainStepRecovery out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //device->obsRx->orxAgcCtrl->peakAgc->apdFastAttack > 0x1 
            helperclass.SetupObsRxAgcHelper(Link, apdFastAttack: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->apdFastAttack or hb2FastAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //device->obsRx->orxAgcCtrl->peakAgc->hb2FastAttack > 0x1
            helperclass.SetupObsRxAgcHelper(Link, hb2FastAttack: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->apdFastAttack or hb2FastAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->apdHighThresh > 0x3F)
            helperclass.SetupObsRxAgcHelper(Link, apdHighThresh: 0x4F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->apdHighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->apdHighThresh <= device->obsRx->orxAgcCtrl->peakAgc->apdLowThresh)
            helperclass.SetupObsRxAgcHelper(Link, apdHighThresh: 0x16);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->apdHighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            // (device->obsRx->orxAgcCtrl->peakAgc->apdLowThresh > 0x3F)
            //Can't triger since apdHighThresh > apdLowThresh and apdHighThresh > 0x3F


            //(device->obsRx->orxAgcCtrl->peakAgc->apdHighThresh < device->obsRx->orxAgcCtrl->peakAgc->apdLowThresh)
            //Can't trigger this condition because of a previous condition

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2HighThresh > 0xFF)
            //Can't trigger this condition since hb2HighThresh is a byte

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2HighThresh < device->obsRx->orxAgcCtrl->peakAgc->hb2LowThresh)
            helperclass.SetupObsRxAgcHelper(Link, hb2HighThresh: 0x80);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2HighThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2LowThresh > 0xFF) 
            //Can't trigger since hb2LowThresh is a byte

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2LowThresh > device->obsRx->orxAgcCtrl->peakAgc->hb2HighThresh)
            //Can't trigger since its the same as a previous condition

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2VeryLowThresh > 0xFF) 
            //Can't trigger since hb2VeryLowThresh is a byte

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2VeryLowThresh > device->obsRx->orxAgcCtrl->peakAgc->hb2LowThresh)
            helperclass.SetupObsRxAgcHelper(Link, hb2VeryLowThresh: 0x82);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2VeryLowThresh out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->peakAgc->apdHighGainStepAttack > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, apdHighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->apdHighGainStepAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->apdLowGainStepRecovery > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, apdLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->apdLowGainStepRecovery out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->hb2HighGainStepAttack > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, hb2HighGainStepAttack: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2HighGainStepAttack out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->hb2LowGainStepRecovery > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, hb2LowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2LowGainStepRecovery out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->hb2VeryLowGainStepRecovery > 0x1F)
            helperclass.SetupObsRxAgcHelper(Link, hb2VeryLowGainStepRecovery: 0x2F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2VeryLowGainStepRecovery out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");


            //(device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadDetectEnable > 0x1)
            helperclass.SetupObsRxAgcHelper(Link, hb2OverloadDetectEnable: 0x2);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadEnable out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadDurationCnt > 0x7)
            helperclass.SetupObsRxAgcHelper(Link, hb2OverloadDurationCnt: 0x8);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadDurationCnt out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            //(device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadThreshCnt > 0xF)
            helperclass.SetupObsRxAgcHelper(Link, hb2OverloadThreshCnt: 0x1F);
            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupObsRxAgc(); ; });
            StringAssert.Contains("device->obsRx->orxAgcCtrl->peakAgc->hb2OverloadThresholdCnt out of range in MYKONOS_setupObsRxAgc()", ex.Message, "Error message incorrect");

            Console.WriteLine(ex.Message);
        }

        //Helper Function
        void SetupObsRxAgcHelper(AdiCommandServerClient Link, byte agcRx1MaxGainIndex = 250,
                                byte agcRx1MinGainIndex = 198,
                                byte agcRx2MaxGainIndex = 250,
                                byte agcRx2MinGainIndex = 198,
                                byte agcObsRxMaxGainIndex = 250,
                                byte agcObsRxMinGainIndex = 220,
                                byte agcObsRxSelect = 1,
                                byte agcPeakThresholdMode = 1,// Change for power only mode
                                byte agcLowThsPreventGainIncrease = 1, // Change for power only mode
                                UInt32 agcGainUpdateCounter = 30721,
                                byte agcSlowLoopSettlingDelay = 1,
                                byte agcPeakWaitTime = 5,
                                byte pmdMeasDuration = 0x07,
                                byte pmdMeasConfig = 0x3,
                                byte agcResetOnRxEnable = 0,
                                byte agcEnableSyncPulseForGainCounter = 1,
                                        // mykonosPowerMeasAgcCfg_t
                                byte pmdUpperHighThresh = 0x02, // Triggered at approx -2dBFS
                                byte pmdUpperLowThresh = 0x04,
                                byte pmdLowerHighThresh = 0x0C,
                                byte pmdLowerLowThresh = 0x05,
                                byte pmdUpperHighGainStepAttack = 0x05,
                                byte pmdUpperLowGainStepAttack = 0x01,
                                byte pmdLowerHighGainStepRecovery = 0x01,
                                byte pmdLowerLowGainStepRecovery = 0x05,
                                        // mykonosPeakDetAgcCfg_t
                                byte apdHighThresh = 0x1E, //Triggered at approx -3dBFS
                                byte apdLowThresh = 0x17, //Triggered at approx -5.5dBFS
                                byte hb2HighThresh = 0xB6, // Triggered at approx -2.18dBFS
                                byte hb2LowThresh = 0x81, // Triggered at approx -5.5dBFS
                                byte hb2VeryLowThresh = 0x41, // Triggered at approx -9dBFS
                                byte apdHighThreshExceededCnt = 0x0B,
                                byte apdLowThreshExceededCnt = 0x04,
                                byte hb2HighThreshExceededCnt = 0x0B,
                                byte hb2LowThreshExceededCnt = 0x04,
                                byte hb2VeryLowThreshExceededCnt = 0x04,
                                byte apdHighGainStepAttack = 0x01,
                                byte apdLowGainStepRecovery = 0x01,
                                byte hb2HighGainStepAttack = 0x01,
                                byte hb2LowGainStepRecovery = 0x01,
                                byte hb2VeryLowGainStepRecovery = 0x01,
                                byte apdFastAttack = 1,
                                byte hb2FastAttack = 1,
                                byte hb2OverloadDetectEnable = 1,
                                byte hb2OverloadDurationCnt = 5,
                                byte hb2OverloadThreshCnt = 0x9)
        {
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


        }


        [Test]
        public static void SetGPIOMonitorOut()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;
            Link.Mykonos.setupGpio(0x00FF, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE);


            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setGpioMonitorOut(0x43, 0xFF); ; });
            StringAssert.Contains("MYKONOS_setGpioMonitorOut()The index specified is incorrect, index available are from 0x01 to 0x42", ex.Message, "Error message incorrect");

            Link.Disconnect();
        }

        [Test]
        public static void setRx1GainCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int incStep = 7;
            int decStep = 7;
            uint gainInc = 0x400;
            uint gainDec = 0x800;
            int enCh = 1;
            Exception ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx1GainCtrlPin((byte)0x08, (byte)decStep, gainInc, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx1GainCtrlPin() An invalid step size has been passed, valid step sizes for increment/decrement is 0-7", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx1GainCtrlPin((byte)incStep, (byte)0x08, gainInc, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx1GainCtrlPin() An invalid step size has been passed, valid step sizes for increment/decrement is 0-7", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx1GainCtrlPin((byte)incStep, (byte)decStep, 0x200, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx1GainCtrlPin() An invalid increment pin has been passed,", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setRx1GainCtrlPin((byte)incStep, (byte)decStep, gainInc, 0x400, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx1GainCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");

            Link.Disconnect();
        }


        [Test]
        public static void SetRx2GainCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int incStep = 7;
            int decStep = 7;
            uint gainInc = 0x2000;
            uint gainDec = 0x4000;
            int enCh = 1;
            Exception ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx2GainCtrlPin((byte)0x08, (byte)decStep, gainInc, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx2GainCtrlPin() An invalid step size has been passed, valid step sizes for increment/decrement is 0-7", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx2GainCtrlPin((byte)incStep, (byte)0x08, gainInc, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx2GainCtrlPin() An invalid step size has been passed, valid step sizes for increment/decrement is 0-7", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                 delegate { Link.Mykonos.setRx2GainCtrlPin((byte)incStep, (byte)decStep, 0x200, gainDec, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx2GainCtrlPin() An invalid increment pin has been passed", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setRx2GainCtrlPin((byte)incStep, (byte)decStep, gainInc, 0x400, (byte)enCh); ; });
            StringAssert.Contains("MYKONOS_setRx2GainCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");

            Link.Disconnect();
        }


        [Test]
        [Category("GPIO")]
        public static void setTx1AttenCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int stepSize = 0x1f;
            byte readstepSize = 0;
            uint tx1AttenIncPin = 0x1000;
            uint readtx1AttenIncPin = 0;
            uint tx1AttenDecPin = 0x2000;
            uint readtx1AttenDecPin = 0;
            int useTx1ForTx2 = 1;
            byte readuseTx1ForTx2 = 0;
            int enable = 1;
            byte readenable = 0;
            int tpcMode = 0;
            int tpcMaskTx2 = 0;



            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setTx1AttenCtrlPin((byte)0x2F, tx1AttenIncPin, tx1AttenDecPin, (byte)enable, (byte)useTx1ForTx2); ; });
            StringAssert.Contains("MYKONOS_setTx1AttenCtrlPin() An invalid step size has been passed, valid step sizes for att are 0x00-0x1F", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
              delegate { Link.Mykonos.setTx1AttenCtrlPin((byte)stepSize, 0x100, tx1AttenDecPin, (byte)enable, (byte)useTx1ForTx2); ; });
            StringAssert.Contains("MYKONOS_setTx1AttenCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
              delegate { Link.Mykonos.setTx1AttenCtrlPin((byte)stepSize, tx1AttenIncPin, 0x200, (byte)enable, (byte)useTx1ForTx2); ; });
            StringAssert.Contains("MYKONOS_setTx1AttenCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");



            Link.Disconnect();
        }


        [Test]
        [Category("GPIO")]
        public static void setTx2AttenCtrlPin()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            int spiData2 = 0x0;
            int stepSize = 0x1f;
            byte readstepSize = 0;
            uint tx1AttenIncPin = 0x1000;
            uint readtx1AttenIncPin = 0;
            uint tx1AttenDecPin = 0x2000;
            uint readtx1AttenDecPin = 0;
            int useTx1ForTx2 = 1;
            byte readuseTx1ForTx2 = 0;
            int enable = 1;
            byte readenable = 0;
            int tpcMode = 0;
            int tpcMaskTx2 = 0;




            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setTx2AttenCtrlPin((byte)0x2F, tx1AttenIncPin, tx1AttenDecPin, (byte)enable); ; });
            StringAssert.Contains("MYKONOS_setTx2AttenCtrlPin() An invalid step size has been passed, valid step sizes for att are 0x00-0x1F", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
              delegate { Link.Mykonos.setTx2AttenCtrlPin((byte)stepSize, 0x100, tx1AttenDecPin, (byte)enable); ; });
            StringAssert.Contains("MYKONOS_setTx2AttenCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setTx2AttenCtrlPin((byte)stepSize, tx1AttenIncPin, 0x100, (byte)enable); ; });
            StringAssert.Contains("MYKONOS_setTx2AttenCtrlPin() An invalid decrement pin has been passed", ex.Message, "Error message incorrect");



            Link.Disconnect();
        }

        [Test]
        public static void ConfigGpInterrupt()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 readStatus = 0;



            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.configGpInterrupt(0x2FF); ; });
            StringAssert.Contains("General Purpose Interrupt source mask parameter is invalid", ex.Message, "Error message incorrect");


            Link.Disconnect();
        }


        [Test]
        public static void setupAuxAdcs()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 readStatus = 0;



            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setupAuxAdc(8, 1); ; });
            StringAssert.Contains("adcDecimation value out of range in MYKONOS_setupAuxAdcs()", ex.Message, "Error message incorrect");


            Link.Disconnect();
        }



        [Test]
        public static void setAuxAdcChannel()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 readStatus = 0;



            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.setAuxAdcChannel(14); ; });
            StringAssert.Contains("auxAdcChannel value out of range in MYKONOS_setAuxAdcChannel()", ex.Message, "Error message incorrect");



            Link.Disconnect();
        }


        public static void writeAuxDac()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            UInt16 readStatus = 0;



            Exception ex = Assert.Throws<Exception>(
                delegate { Link.Mykonos.writeAuxDac(1, 1024); ; });
            StringAssert.Contains("auxDacCode value out of range in MYKONOS_writeAuxDac()", ex.Message, "Error message incorrect");

            ex = Assert.Throws<Exception>(
   delegate { Link.Mykonos.writeAuxDac(10, 1000); ; });
            StringAssert.Contains("auxDacIndex value out of range in MYKONOS_writeAuxDac()", ex.Message, "Error message incorrect");

            Link.Disconnect();
        }

        //[Test]
        public static void setupGpio()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;
            Link.Mykonos.setGpioPinLevel(0xFFFFFFFF);


            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    Link.Mykonos.setupGpio(0x00FF, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE, Mykonos.GPIO_MODE.GPIO_MONITOR_MODE);
                    ;
                });
            StringAssert.Contains("The GPIO mode enum is not a valid value in MYKONOS_setupGpio", ex.Message, "Error message incorrect");

        }

        [Test]
        public static void setGpioOe()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;


            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    Link.Mykonos.setGpioOe(0x8FFFF, 0xFFFFFFFF);
                    ;
                });
            StringAssert.Contains("MYKONOS_setGpioOe() had invalid parameter gpioOutEn (valid range 0 - 0x07FFFF)", ex.Message, "Error message incorrect");

        }

        [Test]
        public static void setGpioSourceCtrl()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;


            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    Link.Mykonos.setGpioSourceCtrl(0xFFFFFF);
                    ;
                });
            StringAssert.Contains("MYKONOS_setGpioSourceCtrl() An invalid source control parameter has been passed", ex.Message, "Error message incorrect");

        }


        //[Test]
        public static void setupGpio3v3()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;
            Link.Mykonos.setGpio3v3SourceCtrl(0x0FFF);


            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    Link.Mykonos.setupGpio3v3(0x00FF, Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE, Mykonos.GPIO3V3_MODE.GPIO3V3_LEVELTRANSLATE_MODE);
                    ;
                });
            StringAssert.Contains("Invalid GPIO3v3 source control mode", ex.Message, "Error message incorrect");

        }


        [Test]
        public static void setGpio3v3SourceCtrl()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            byte spiData1 = 0x0;
            byte spiData2 = 0x0;
            byte monitorIndex;
            int monitorMask;



            Exception ex = Assert.Throws<Exception>(
                delegate
                {
                    Link.Mykonos.setGpio3v3SourceCtrl(0xFFFF);
                    ;
                });
            StringAssert.Contains("gpio3v3 members have invalid value for the GPIO3v3 source control mode.", ex.Message, "Error message incorrect");
            //No error message atm
        }

    }
}
