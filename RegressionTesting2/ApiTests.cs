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
    public class ApiFunctionalTests
    {
       
        [SetUp] 
        public void ApiTestInit()
        {
            //Set Test Parameters
            TestSetupConfig settings = new TestSetupConfig();
            settings.mykSettings.DeviceClock_kHz = 245760;
            settings.mykSettings.rxChannel = 3;
            settings.mykSettings.rxProfileName = "Rx 100MHz, IQrate 122.88MHz, Dec5";
            settings.mykSettings.txProfileName = "Tx 75/200MHz, IQrate 245.76MHz, Dec5";
            settings.mykSettings.orxProfileName = "ORX 200MHz, IQrate 245.75MHz, Dec5";
            settings.mykSettings.srxProfileName = "SRx 20MHz, IQrate 30.72MHz, Dec5";
            settings.mykSettings.ArmFirmwareName = "Mykonos_M3.bin";
            byte deframerM = 4;
            byte desLanesEnabled = 0x0F;

            settings.mykTxDeFrmrCfg.deviceID = 0;
            settings.mykTxDeFrmrCfg.laneID = 0;
            settings.mykTxDeFrmrCfg.bankID = 0;
            settings.mykTxDeFrmrCfg.M = deframerM;
            settings.mykTxDeFrmrCfg.K = 32;
            settings.mykTxDeFrmrCfg.scramble = 1;
            settings.mykTxDeFrmrCfg.externalSysref = 1;
            settings.mykTxDeFrmrCfg.deserializerLanesEnabled = desLanesEnabled;
            settings.mykTxDeFrmrCfg.deserializerLaneCrossbar = 0xE4;
            settings.mykTxDeFrmrCfg.eqSetting = 1;
            settings.mykTxDeFrmrCfg.invertLanePolarity = 0;
            settings.mykTxDeFrmrCfg.enableAutoChanXbar = 0;
            settings.mykTxDeFrmrCfg.lmfcOffset = 0;
            settings.mykTxDeFrmrCfg.newSysrefOnRelink = 0;

            byte framerM = 4;
            byte serializerLanesEnabled = 0x03;

            settings.mykRxFrmrCfg.bankId = 0;
            settings.mykRxFrmrCfg.deviceId = 0;
            settings.mykRxFrmrCfg.laneId = 0;
            settings.mykRxFrmrCfg.M = framerM;
            settings.mykRxFrmrCfg.K = 32;
            settings.mykRxFrmrCfg.scramble = 1;
            settings.mykRxFrmrCfg.externalSysref = 1;
            settings.mykRxFrmrCfg.serializerLanesEnabled = serializerLanesEnabled;
            settings.mykRxFrmrCfg.serializerLaneCrossbar = 0xE4;
            settings.mykRxFrmrCfg.serializerAmplitude = 22;
            settings.mykRxFrmrCfg.preEmphasis = 4;
            settings.mykRxFrmrCfg.invertLanePolarity = 0;
            settings.mykRxFrmrCfg.lmfcOffset = 0;
            settings.mykRxFrmrCfg.obsRxSyncbSelect = 0;
            settings.mykRxFrmrCfg.newSysrefOnRelink = 0;
            settings.mykRxFrmrCfg.overSample = 0;
            settings.mykRxFrmrCfg.enableAutoChanXbar = 0;

            byte obsRxframerM = 2;
            byte obsRxserializerLanesEnabled = 0xC;


            settings.mykObsRxFrmrCfg.bankId = 1;
            settings.mykObsRxFrmrCfg.deviceId = 0;
            settings.mykObsRxFrmrCfg.laneId = 0;
            settings.mykObsRxFrmrCfg.M = obsRxframerM;
            settings.mykObsRxFrmrCfg.K = 32;
            settings.mykObsRxFrmrCfg.scramble = 1;
            settings.mykObsRxFrmrCfg.externalSysref = 1;
            settings.mykObsRxFrmrCfg.serializerLanesEnabled = obsRxserializerLanesEnabled;
            settings.mykObsRxFrmrCfg.serializerLaneCrossbar = 0xE4;
            settings.mykObsRxFrmrCfg.serializerAmplitude = 22;
            settings.mykObsRxFrmrCfg.preEmphasis = 4;
            settings.mykObsRxFrmrCfg.invertLanePolarity = 0;
            settings.mykObsRxFrmrCfg.lmfcOffset = 0;
            settings.mykObsRxFrmrCfg.newSysrefOnRelink = 0;
            settings.mykObsRxFrmrCfg.enableAutoChanXbar = 0;
            settings.mykObsRxFrmrCfg.obsRxSyncbSelect = 1;
            settings.mykObsRxFrmrCfg.overSample = 1;
            settings.rxPllLoFreq = 2400000000;
            settings.txPllLoFreq = 2500000000;
            settings.obsRxPllLoFreq = 2600000000;


            //Start Calibration
            UInt32 calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
                            (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
                            (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
                            (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
                            (UInt32)(Mykonos.CALMASK.TX_ATTENUATION_DELAY) |
                            (UInt32)(Mykonos.CALMASK.RX_GAIN_DELAY) |
                            (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
                            (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                            (UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
                            (UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
                            (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
                            (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                            (UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
                            (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
        }

        [Test]
        public static void SetTxPllFrequency()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.setSpiChannel(1);
            //Link.spiWrite(0x2B0, 0x4);
            //Link.spiWrite(0x233, 0x1B);
            Link.Mykonos.setRfPllFrequency(Mykonos.PLLNAME.TX_PLL, 700000000);


            System.Threading.Thread.Sleep(1000);
            //Link.spiWrite(0x2B0, 0x05);

            byte pllStatus = Link.Mykonos.checkPllsLockStatus();
            Console.WriteLine("PLL Status: 0x" + pllStatus.ToString("X"));

            UInt16[] spiAddr = new UInt16[35];
            byte[] spiData = null;

            for (int j = 0; j < 35; j++)
            {
                spiAddr[j] = (UInt16)(0x2B0 + j);
            }

            spiData = Link.spiRead(spiAddr);

            for (int j = 0; j < 35; j++)
            {
                Console.WriteLine("SPI Addr: " + spiAddr[j].ToString("X") + ": " + spiData[j].ToString("X"));
            }

            byte spiData1 = Link.spiRead(0x382); Console.WriteLine("SPI Addr: 0x382:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x230); Console.WriteLine("SPI Addr: 0x230:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x231); Console.WriteLine("SPI Addr: 0x231:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x232); Console.WriteLine("SPI Addr: 0x232:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x233); Console.WriteLine("SPI Addr: 0x233:" + spiData1.ToString("X"));

            spiData1 = Link.spiRead(0x157); Console.WriteLine("SPI Addr: 0x157:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x257); Console.WriteLine("SPI Addr: 0x257:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x2C7); Console.WriteLine("SPI Addr: 0x2C7:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x357); Console.WriteLine("SPI Addr: 0x357:" + spiData1.ToString("X"));
            spiData1 = Link.spiRead(0x17F); Console.WriteLine("SPI Addr: 0x17F:" + spiData1.ToString("X"));


            Link.Disconnect();
        }

        [Test]
        public static void SweepTxPllFrequency(TestSetupConfig settings)
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

                pllStatus = Link.Mykonos.checkPllsLockStatus();
                //Console.WriteLine("Frequency: " + freq[i]/4 + ", PLL Status: 0x" + pllStatus.ToString("X"));


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

 
        [Test]
        public static void SetArmDebugMode()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Console.Write(Link.identify());
            Console.Write(Link.version());



            Link.fpgaWrite(0x100, 1); //enable Test mode 0xA for ARM Debug

            byte fpgaReadData = Link.spiRead(0xD00); Console.WriteLine("xD00 = " + fpgaReadData.ToString("X"));
            fpgaReadData = Link.spiRead(0xD08); Console.WriteLine("xD08 = " + fpgaReadData.ToString("X"));
            fpgaReadData = Link.spiRead(0xD09); Console.WriteLine("xD09 = " + fpgaReadData.ToString("X"));
            fpgaReadData = Link.spiRead(0xD14); Console.WriteLine("xD14 = " + fpgaReadData.ToString("X"));


            Link.Disconnect();
        }

        [Test]
        public static void TestMykonosSerializerPrbs()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.spiWrite(0xDC0, 0x3); //hold obsRx framer in reset
            Link.spiWrite(0xDC7, 0x00); //Disable lane FIFOs in ObsRx framer
            Link.spiWrite(0xDC1, 0x00); //disable ObsRx framer clocks

            Link.fpgaWrite(0x400, 0x400); //Enable all Rx and Tx lanes in FPGA
            Link.fpgaWrite(0x40C, 0xE4E4E4);
            Link.spiWrite(0x78, 0x00); //Lane polarity
            Link.spiWrite(0x72, 1); //enable prbs 20
            Link.spiWrite(0xB1, 0x90); //enable all serializer lanes
            //Link.spiWrite(0xB3, 0x20); //slow down lane rate by /2
            //Link.spiWrite(0x11A, 0x00); //divide serializer some

            Link.fpgaWrite(0x404, 0x08); //Clear Error counters
            Link.fpgaWrite(0x404, 0x01); //enable PRBS7 checker

            Link.fpgaWrite(0x404, 0x11); //Enable PRBS7 error counters

            UInt32 fpgaReg = 0;
            byte spiReg = 0;
            spiReg = Link.spiRead(0x059); Console.WriteLine("SPI Reg x059 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x05A); Console.WriteLine("SPI Reg x05A = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x060); Console.WriteLine("SPI Reg x060 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x061); Console.WriteLine("SPI Reg x061 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x062); Console.WriteLine("SPI Reg x062 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x059); Console.WriteLine("SPI Reg x059 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x110); Console.WriteLine("SPI Reg x110 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B3); Console.WriteLine("SPI Reg x0B3 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x11A); Console.WriteLine("SPI Reg x11A = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B0); Console.WriteLine("SPI Reg x0B0 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B1); Console.WriteLine("SPI Reg x0B1 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B2); Console.WriteLine("SPI Reg x0B2 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B3); Console.WriteLine("SPI Reg x0B3 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B4); Console.WriteLine("SPI Reg x0B4 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x0B5); Console.WriteLine("SPI Reg x0B5 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x224); Console.WriteLine("RCAL result = " + spiReg.ToString("X"));

            fpgaReg = Link.fpgaRead(0x404); Console.WriteLine("FPGA Reg x404 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x41C); Console.WriteLine("FPGA Reg x41C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x420); Console.WriteLine("FPGA Reg x420 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x14); Console.WriteLine("FPGA Reg x14 = " + fpgaReg.ToString("X"));

            Link.Disconnect();
        }

        [Test]
        public static void EnableTxNCO()
        {
            /* not working yet */
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            string firPath = @"C:\svn\software\projects\mykonos\Filters\"; //TODO: replace with dynamic
            string txFirFile = firPath + "TxApp49_307p2_BW250_PriSigBW100.ftr";

            //Link.Mykonos.programFir(Mykonos.FIR.TX1TX2_FIR, txFirFile);

            //Link.Mykonos.setEnsmPinMode(0);
            //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
            //Link.Mykonos.powerUpTxPath(Mykonos.TXCHANNEL.TX1_TX2);
            //Move to Radio On ARM state
            Link.Mykonos.radioOn();

            byte spiReg = Link.spiRead(0x1B0); Console.WriteLine("SPI Reg x1B0 = " + spiReg.ToString("X"));
            spiReg = Link.spiRead(0x1B3); Console.WriteLine("SPI Reg x1B3 = " + spiReg.ToString("X"));


            Link.spiWrite(0xC40, 0x80); //enable Tx NCO
            Link.spiWrite(0x9CB, 0x5); //freq
            Link.spiWrite(0x9CC, 0x5); //freq
            Link.spiWrite(0x9CD, 0x5); //freq
            Link.spiWrite(0x9CE, 0x5); //freq

            Link.Disconnect();
        }

        [Test]
        public static void SetOrxGain()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setObsRxManualGain(Mykonos.OBSRXCHANNEL.OBS_RX1, 240);

            Link.Disconnect();
        }

        [Test]
        public static void LoadOrxGainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\orxGainTable_debug.csv", Mykonos.RXGAIN_TABLE.ORX_GT);

            Link.Disconnect();
        }

        [Test]
        public static void LoadRxGainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\MykonosRxGainTable36dB_steps0p5dB_using_measured_FEGain.csv", Mykonos.RXGAIN_TABLE.RX1_RX2_GT);
            //Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\MykonosRxGainTable30dB_steps0p5dB.csv", Mykonos.RXGAIN_TABLE.RX1_RX2_GT);
            //Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\RxTable_SweepDigAttenWord.csv", Mykonos.RXGAIN_TABLE.RX1_RX2_GT);
            //Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\RxTable_SweepFEGainWord.csv", Mykonos.RXGAIN_TABLE.RX1_RX2_GT); 
            Link.Disconnect();
        }

        [Test]
        public static void LoadSnifferGainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.programRxGainTable(@"..\..\..\..\mykonos_resources\SnifferRxGainTable_default.csv", Mykonos.RXGAIN_TABLE.SNRX_GT);

            Link.Disconnect();
        }

        [Test]
        public static void ReadFirFilter()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiReg1 = 0;
            byte spiReg2 = 0;
            Link.spiWrite(0x00A, 0x84); //Enable readback of RX1 FIR
            spiReg1 = Link.spiRead(0x00A); Console.WriteLine("Scratch Reg Test: " + spiReg1.ToString("X"));
            Link.spiWrite(0xDFF, 0x84); //Enable readback of RX1 FIR
            spiReg1 = Link.spiRead(0xDFF); Console.WriteLine("Filter Select: " + spiReg1.ToString("X"));

            spiReg1 = Link.spiRead(0x410); Console.WriteLine("Rx #taps: " + (spiReg1).ToString("X"));
            spiReg1 = Link.spiRead(0x411); Console.WriteLine("Rx Filter Gain: " + (spiReg1 & 0x03).ToString("X"));
            spiReg1 = Link.spiRead(0x412); Console.WriteLine("DPD Rx Filter Gain: " + (spiReg1 & 0x03).ToString("X"));
            Console.WriteLine("Sniffer Rx Filter Gain: " + ((spiReg1 >> 5) & 0x03).ToString("X"));

            Int16[] coefs = new Int16[128];

            for (int i = 0; i < 128; i++)
            {
                Link.spiWrite(0xE01, (byte)(i * 2));
                spiReg1 = Link.spiRead(0xE00);
                Link.spiWrite(0xE01, (byte)(i * 2 + 1));
                spiReg2 = Link.spiRead(0xE00);

                coefs[i] = (Int16)((UInt16)(spiReg1) | ((UInt16)(spiReg2) << 8));
                Console.WriteLine("Coef[" + i + "]: " + coefs[i]);

            }



            Link.Disconnect();
        }
 
        [Test]
        public static void ReadRxGainTable()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiReg1 = 0;
            byte spiReg2 = 0;
            byte index = 0;
            byte feGain = 0;
            byte extControl = 0;
            byte digGain = 0;
            byte enableAtten = 0;
            Link.setSpiChannel(1);

            Link.spiWrite(0x00A, 0x84); 
            spiReg1 = Link.spiRead(0x00A); Console.WriteLine("Scratch Reg Test: " + spiReg1.ToString("X"));
            Assert.AreEqual(0x84, spiReg1, "SPI not working");

            Link.spiWrite(0x516, 0x0C); //Enable Rx1 gain table for readback
            
            Int16[] coefs = new Int16[128];

            for (byte i = 255; i > 128; i--)
            {
                Link.spiWrite(0x500, i);
                index = Link.spiRead(0x50A);
                
                feGain = Link.spiRead(0x50B);
                
                extControl = Link.spiRead(0x50C);

                spiReg2 = Link.spiRead(0x50D);
                digGain = (byte)(spiReg2 & 0x7F);
                enableAtten = (byte)(spiReg2 >> 7);

                Console.WriteLine(index + ", " + feGain + ", " + extControl + ", " + digGain + ", " + enableAtten);

            }

            Link.spiWrite(0x516, 0x08); //Enable Rx1 gain table for readback

            Link.Disconnect();
        }
        
        [Test]
        public static void SetObsRxInputToSnifferC()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_8, 0);
            Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C);

            Link.Disconnect();

        }

        [Test]
        public static void SetObsRxInputToORx1()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;
            
            Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_0, 0);
            Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_RX1);
            
            Link.hw.ReceiveTimeout = 5000;
            Link.Disconnect();

        }

        [Test]
        public static void SetObsRxInputToORx2()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_0, 0);
            Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_RX2);

            Link.Disconnect();

        }
        [Test]
        public static void SetObsRxInputToInternalCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_0, 0);
            Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_INTERNALCALS);
        
            Link.Disconnect();

        }
        [Test]
        public static void SetTx1Atten()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;
            Link.Mykonos.setTx1Attenuation(41.95);
            Link.hw.ReceiveTimeout = 5000;
            Link.Disconnect();

        }
        [Test]
        public static void SetRadioOn()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.radioOn();

            Link.Disconnect();
        }
        [Test]
        public static void SetRadioOff()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.radioOff();

            Link.Disconnect();
        }
        [Test]
        public static void GetRadioState()
        {
            UInt32 radioState = 0;
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.getRadioState(ref radioState);
            Console.WriteLine("MYKONOS RadioState = " + radioState.ToString("X"));
            Link.Disconnect();
        }



      

        [Test, Sequential]
        public static void SetObsRxGainMode([Values(Mykonos.GAINMODE.MGC, Mykonos.GAINMODE.AGC, Mykonos.GAINMODE.HYBRID, 5)] Mykonos.GAINMODE mode)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            //InitializationTests.TestFullInit();
            //Link.hw.Connect(TestSettings.ipAddr, TestSettings.port);

            Link.Mykonos.setObsRxGainControlMode(mode);

            Link.hw.ReceiveTimeout = 5000;

            Link.Disconnect();
        }

        [Test]
        public static void TestMCS()
        {
            
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte mcsStatus = 0;
            Link.setSpiChannel(1);
            Link.spiWrite(0x120, 0x00);

            Link.Mykonos.enableMultichipSync(1, ref mcsStatus);

            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);
            Link.Ad9528.requestSysref(true);

            Link.Mykonos.enableMultichipSync(0, ref mcsStatus);
            Link.Disconnect();


        }

        [Test]
        public static void SetupAuxDacs()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            
            /*
            Link.setSpiChannel(1);

            //config[7] = 1 = add comp cap 400fF
            //config[6] = AuxDAC step size (1= 12bit resolution, 0 = 11bit resolution)
            //config[5:4] = VREF = 1 + ([5:4] * 0.5)
            Link.spiWrite(0xBA0, 0x00); //AuxDAC0 config[7:4], AuxDAC0[11:8]  (xBA0 - xBB3)
            Link.spiWrite(0xBA1, 0x00); //AuxDAC0 [7:0]

            Link.spiWrite(0xB73, 0x3F); //enable manual mode for All AuxDACS [5:0]
            Link.spiWrite(0xB74, 0x0F); //enable manual mode for All AuxDACS [9:6]
            Link.spiWrite(0xB75, 0x3E); //clear power down bit for AuxDAC0 on GPIO 3.3[9]
            Link.spiWrite(0xB76, 0x0F);

            Debug.WriteLine("0xB73: " + Link.spiRead(0xB73).ToString("X"));
            Debug.WriteLine("0xB74: " + Link.spiRead(0xB74).ToString("X"));
            Debug.WriteLine("0xB75: " + Link.spiRead(0xB75).ToString("X"));
            Debug.WriteLine("0xB76: " + Link.spiRead(0xB76).ToString("X"));

            

            //Link.Mykonos.radioOn(); //autotoggle mode will follow the SPI bits as well if ENSM are in SPI mode.
            //Link.spiWrite(0x1b0, 0x89);
            */

            Link.hw.ReceiveTimeout = 0;

            UInt16 auxDacEnableMask = 0x0201;
            UInt16[] auxDacCode = new UInt16[] {0,10,20,30,40,50,60,70,80,512};
            Byte[] auxDacSlope = new Byte[] {0,1,0,1,0,1,1,1,0,0};
            Byte[] auxDacVref = new Byte[] {0,1,2,3,0,1,2,3,0,3};

            Link.Mykonos.init_auxDacStructure(1, ref auxDacEnableMask, ref auxDacCode, ref auxDacSlope, ref auxDacVref);

            Link.Mykonos.setupAuxDacs();
            

            Link.hw.ReceiveTimeout = 5000;

            Debug.WriteLine("0xB73: " + Link.spiRead(0xB73).ToString("X"));
            Debug.WriteLine("0xB74: " + Link.spiRead(0xB74).ToString("X"));
            Debug.WriteLine("0xB75: " + Link.spiRead(0xB75).ToString("X"));
            Debug.WriteLine("0xB76: " + Link.spiRead(0xB76).ToString("X"));
            Link.Disconnect();
        }

        [Test]
        public static void writeAuxDac()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.writeAuxDac(0, 512);

            Link.hw.ReceiveTimeout = 5000;

            Debug.WriteLine("0xB73: " + Link.spiRead(0xB73).ToString("X"));
            Debug.WriteLine("0xB74: " + Link.spiRead(0xB74).ToString("X"));
            Debug.WriteLine("0xB75: " + Link.spiRead(0xB75).ToString("X"));
            Debug.WriteLine("0xB76: " + Link.spiRead(0xB76).ToString("X"));
            Link.Disconnect();
        }

        [Test]
        public static void TestAuxAdc()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setupAuxAdc(7, 1);
            Link.Mykonos.setAuxAdcChannel(1);
            System.Threading.Thread.Sleep(500);
            UInt16 auxAdcCode = Link.Mykonos.readAuxAdc();
            Debug.WriteLine("AuxADC Code: " + auxAdcCode);


            Link.hw.ReceiveTimeout = 5000;

            Debug.WriteLine("0xBC0: " + Link.spiRead(0xBC0).ToString("X"));
            Debug.WriteLine("0xBC1: " + Link.spiRead(0xBC1).ToString("X"));
            Debug.WriteLine("0xBC2: " + Link.spiRead(0xBC2).ToString("X"));
            Debug.WriteLine("0xBC3: " + Link.spiRead(0xBC3).ToString("X"));
            Debug.WriteLine("0xBC4: " + Link.spiRead(0xBC4).ToString("X"));
            Debug.WriteLine("0xBC5: " + Link.spiRead(0xBC5).ToString("X"));
            Debug.WriteLine("0xBC6: " + Link.spiRead(0xBC6).ToString("X"));
            Link.Disconnect();
        }
        [Test]
        public static void ReadAuxAdc()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.setAuxAdcChannel(3);
            
            UInt16 auxAdcCode = Link.Mykonos.readAuxAdc();
            Debug.WriteLine("AuxADC Code: " + auxAdcCode);


            Link.hw.ReceiveTimeout = 5000;

            Debug.WriteLine("0xBC0: " + Link.spiRead(0xBC0).ToString("X"));
            Debug.WriteLine("0xBC1: " + Link.spiRead(0xBC1).ToString("X"));
            Debug.WriteLine("0xBC2: " + Link.spiRead(0xBC2).ToString("X"));
            Debug.WriteLine("0xBC3: " + Link.spiRead(0xBC3).ToString("X"));
            Debug.WriteLine("0xBC4: " + Link.spiRead(0xBC4).ToString("X"));
            Debug.WriteLine("0xBC5: " + Link.spiRead(0xBC5).ToString("X"));
            Debug.WriteLine("0xBC6: " + Link.spiRead(0xBC6).ToString("X"));
            Link.Disconnect();
        }


        [Test]
        public static void EnableTrackingCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;
            
            Link.Mykonos.enableTrackingCals((UInt32)(0x3FF)); //track all cals
            Link.hw.ReceiveTimeout = 5000;

            Link.Disconnect();
        }
        [Test]
        public static void readEnabledTrackingCals()
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.hw.ReceiveTimeout = 0;

            Link.Mykonos.radioOff();
            
            UInt32 calmask = 0x03FF;
            
            Link.Mykonos.enableTrackingCals(calmask);
            Link.Mykonos.radioOn();

            Link.Mykonos.sendArmCommand(0x08, new byte[] { 0x83, 0, 0, 2 }, 4);
            byte[] armData = new byte[] {0,0,0,0};
            Link.Mykonos.readArmMem(0x20000000, 4, 1, ref armData);

            //armData[1] is the calmask [15:8], and armData[0] is calmask[7:0]
            Debug.WriteLine("Enabled tracking calmask: " + armData[3].ToString("X") + armData[2].ToString("X") + armData[1].ToString("X") + armData[0].ToString("X"));

            Assert.AreEqual(calmask, (UInt32)( ((UInt32)(armData[3]) << 24) | ((UInt32)(armData[2]) << 16) | ((UInt32)(armData[1]) << 8) | ((UInt32)(armData[0])) ), "Tracking calmask did not match the mask written to ARM memory");
            
            Link.hw.ReceiveTimeout = 5000;

            Link.Disconnect();
        }

   

    }
}
