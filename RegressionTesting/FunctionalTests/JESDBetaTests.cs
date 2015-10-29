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
    [Category("ApiFunctional")]
    public class JESDBetaTests
    {
        public enum MYK_DATAPATH_MODE
        {
            RX2TX2OBS1 = 0,
            RX1TX2OBS1 = 1,
            RX2TX1OBS1 = 3,
            RX1TX1OBS1 = 4,
            RX2TX2OBS_MON = 5,
        };
        public enum MYK_JESD_LANE_CFG
        {
            RL2OBSL2TL4 = 0,
            RL1OBSL2TL4 = 1,
            RL2OBSL2TL2 = 3,
            RL1OBSL2TL2 = 4,
            RL2OBSL0TL4 = 5,
            RL1OBSL0TL4 = 6,
            RL2OBSL0TL2 = 7,
            RL1OBSL0TL2 = 8,
        };
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string resFilePath = @"..\..\..\TestResults\JESDTests\JESDTestResult.txt";
 
        /// <summary>
        /// Initializes Mykonos settings and JESD204B settings
        /// JESD204B is configured with a set of input parameters
        /// </summary>

        public static void JESDTestInit(byte deframerM, byte deframerK, byte desLanesEnabled, 
            byte framerM, byte framerK, byte serializerLanesEnabled, byte obsRxframerM, 
            byte obsRxframerK, byte obsRxserializerLanesEnabled)
        {
            //Set Test Parameters  
            //            "Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5"
            //("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5"
            settings.mykSettings.DeviceClock_kHz = 122880;
            settings.mykSettings.rxChannel = (Mykonos.RXCHANNEL) 3;
            settings.mykSettings.rxProfileName = "Rx 20MHz, IQrate 30.72MHz, Dec5";
            settings.mykSettings.txProfileName = "Tx 20/100MHz, IQrate  122.88MHz, Dec5";
            settings.mykSettings.orxProfileName = "ORX 100MHz, IQrate 122.88MHz, Dec5";
            settings.mykSettings.srxProfileName = "SRx 20MHz, IQrate 30.72MHz, Dec5";
            settings.mykSettings.ArmFirmwareName = "Mykonos_M3.bin";

            settings.mykTxDeFrmrCfg.deviceID = 0;
            settings.mykTxDeFrmrCfg.laneID = 0;
            settings.mykTxDeFrmrCfg.bankID = 0;
            settings.mykTxDeFrmrCfg.M = deframerM;
            settings.mykTxDeFrmrCfg.K = deframerK;
            settings.mykTxDeFrmrCfg.scramble = 1;
            settings.mykTxDeFrmrCfg.externalSysref = 1;
            settings.mykTxDeFrmrCfg.deserializerLanesEnabled = desLanesEnabled;
            settings.mykTxDeFrmrCfg.deserializerLaneCrossbar = 0xE4;
            settings.mykTxDeFrmrCfg.eqSetting = 1;
            settings.mykTxDeFrmrCfg.invertLanePolarity = 0;
            settings.mykTxDeFrmrCfg.enableAutoChanXbar = 0;
            settings.mykTxDeFrmrCfg.lmfcOffset = 0;
            settings.mykTxDeFrmrCfg.newSysrefOnRelink = 0;

            settings.mykRxFrmrCfg.bankId = 0;
            settings.mykRxFrmrCfg.deviceId = 0;
            settings.mykRxFrmrCfg.laneId = 0;
            settings.mykRxFrmrCfg.M = framerM;
            settings.mykRxFrmrCfg.K = framerK;
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


            settings.mykObsRxFrmrCfg.bankId = 1;
            settings.mykObsRxFrmrCfg.deviceId = 0;
            settings.mykObsRxFrmrCfg.laneId = 0;
            settings.mykObsRxFrmrCfg.M = obsRxframerM;
            settings.mykObsRxFrmrCfg.K = obsRxframerK;
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
            settings.rxPllLoFreq_Hz = 2400000000;
            settings.txPllLoFreq_Hz = 2500000000;
            settings.obsRxPllLoFreq_Hz = 2600000000;
            settings.rxPllUseExtLo = 0;
            settings.rxUseRealIfData = 0;
            settings.txPllUseExtLo = 0;
            settings.tx1Atten = 10000;
            settings.tx1Atten = 10000;

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
        }

        public static int ConvertMaskToCount(int mask)
        {
            int count = 0;
            switch (mask)
            {
                case 0x0F:
                    count = 4;
                    break;
                case 0x03:
                case 0x0C:
                    count = 2;
                    break;
                case 0x01:
                    count = 1;
                    break;
                default:
                    count = 1;
                    break;
            }
            return count;
        }

        public static byte CalculateStartK(int f)
        {
            byte k = 0;
            switch (f)
            {
                case 4:
                    k = 5;
                    break;
                case 2:
                    k = 10;
                    break;
                case 1:
                    k = 20;
                    break;
                default:
                    k = 0;
                    break;
            }
            return k;
        }

        public static byte CalculateKIncrement(int f)
        {
            byte k = 0;
            switch (f)
            {
                case 4:
                    k = 1;
                    break;
                case 2:
                    k = 2;
                    break;
                case 1:
                    k = 4;
                    break;
                default:
                    k = 0;
                    break;
            }
            return k;
        }
        /// <summary>
        /// TestInvalidJESDParam function tests Mykonos with some invalid JESD parameter combinations 
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// </summary>
 //       [Test]
        [Category("JESD")]
        public static void TestInvalidJESDParam()
        {
            //const byte WRITE_SETTINGS = 1;
            //const byte READ_SETTINGS = 0;


            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.resetDevice();
            //Configure Rx & Orx DataPath Settings
            // 2 Rx Channels enabled equates to 4 Converters Represented JESD M = 4
            //Use Lanes 0 & 1 for Rx1&Rx2 Channels
            // 1 Observer Channel from ORX1 Input equates to 2 Converters JESD M = 2
            //Use Lanes 3 & 4 for ORx Channel
            Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            byte framerM = 4;
            byte serializerLanesEnabled = 0x03;

            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte deframerM = 4;
            byte desLanesEnabled = 0x0F;

            byte Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
            byte obsRxframerM = 2;
            byte obsRxserializerLanesEnabled = 0xC;

            //Invalid K Settings
            byte framerK;
            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + 
                                    ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + 
                                    ", Tx F: " + deframerF +
                                    ", Rx M: " + framerM + ", Rx K: " + framerK + 
                                    ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + 
                                    ", Rx F: " + framerF +
                                    ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + 
                                    ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + 
                                    ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }
                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, 
                            framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, 
                            obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            rxChannels = Mykonos.RXCHANNEL.RX1;

            deframerM = 4;
            desLanesEnabled = 0x0F;

            framerM = 2;
            serializerLanesEnabled = 0x01;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK +
                                   ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) +
                                   ", Tx F: " + deframerF +
                                   ", Rx M: " + framerM + ", Rx K: " + framerK +
                                   ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) +
                                   ", Rx F: " + framerF +
                                   ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK +
                                   ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) +
                                   ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, 
                            framerM, framerK, serializerLanesEnabled, obsRxframerM, 
                            obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            deframerM = 4;
            desLanesEnabled = 0x0F;

            framerM = 2;
            serializerLanesEnabled = 0x03;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 9; framerK <= 33; framerK += 12)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK +
                                  ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) +
                                  ", Tx F: " + deframerF +
                                  ", Rx M: " + framerM + ", Rx K: " + framerK +
                                  ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) +
                                  ", Rx F: " + framerF +
                                  ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK +
                                  ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) +
                                  ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            txChannels = Mykonos.TXCHANNEL.TX1;

            deframerM = 2;
            desLanesEnabled = 0x03;

            framerM = 4;
            serializerLanesEnabled = 0x03;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {
                       

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK +
                                   ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) +
                                   ", Tx F: " + deframerF +
                                   ", Rx M: " + framerM + ", Rx K: " + framerK +
                                   ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) +
                                   ", Rx F: " + framerF +
                                   ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK +
                                   ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) +
                                   ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            deframerM = 2;
            desLanesEnabled = 0x0F;

            framerM = 4;
            serializerLanesEnabled = 0x03;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 19; deframerK <= 33; deframerK += 7)
                    {
                        

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            rxChannels = Mykonos.RXCHANNEL.RX1;

            deframerM = 2;
            desLanesEnabled = 0x03;

            framerM = 2;
            serializerLanesEnabled = 0x01;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {
                       

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            deframerM = 2;
            desLanesEnabled = 0x0F;

            framerM = 2;
            serializerLanesEnabled = 0x01;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 4; framerK <= 33; framerK += 29)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 19; deframerK <= 33; deframerK += 7)
                    {
                      

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            deframerM = 2;
            desLanesEnabled = 0x03;

            framerM = 2;
            serializerLanesEnabled = 0x03;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 9; framerK <= 33; framerK += 12)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 9; deframerK <= 33; deframerK += 12)
                    {
                      
                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }

            deframerM = 2;
            desLanesEnabled = 0x0F;

            framerM = 2;
            serializerLanesEnabled = 0x03;

            obsRxframerM = 2;
            obsRxserializerLanesEnabled = 0xC;

            for (framerK = 9; framerK <= 33; framerK += 12)
            {
                byte obsRxframerK;
                for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                {
                    byte deframerK;
                    for (deframerK = 19; deframerK <= 33; deframerK += 7)
                    {
                        

                        byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                        byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                        byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                        DateTime timeStamp = DateTime.Now;

                        string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();


                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                        {
                            file.WriteLine(text);
                        }

                        //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                        TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                        mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                        mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                    }
                }
            }
        }


        /// <summary>
        /// TestInvalidJESDParam function tests Mykonos with some invalid JESD parameter combinations 
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// </summary>
        //[Test]
        [Category("JESD")]
        public static void TestJESDBetaInvalidParam(
                      [Values(MYK_DATAPATH_MODE.RX2TX2OBS1,
                              MYK_DATAPATH_MODE.RX2TX2OBS1,
                              MYK_DATAPATH_MODE.RX1TX2OBS1,
                              MYK_DATAPATH_MODE.RX1TX2OBS1,
                              MYK_DATAPATH_MODE.RX2TX1OBS1,
                              MYK_DATAPATH_MODE.RX2TX1OBS1,
                              MYK_DATAPATH_MODE.RX1TX1OBS1,
                              MYK_DATAPATH_MODE.RX1TX1OBS1,
                              MYK_DATAPATH_MODE.RX1TX1OBS1,
                              MYK_DATAPATH_MODE.RX1TX1OBS1)] MYK_DATAPATH_MODE DataPath,
                      [Values(MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL2,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL2,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL2,
                               MYK_JESD_LANE_CFG.RL2OBSL2TL4
                             )]MYK_JESD_LANE_CFG LaneCfg)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.resetDevice();
            byte invalidTest = 0x0;
            //Configure Rx & Orx DataPath Settings
            // 2 Rx Channels enabled equates to 4 Converters Represented JESD M = 4
            //Use Lanes 0 & 1 for Rx1&Rx2 Channels
            // 1 Observer Channel from ORX1 Input equates to 2 Converters JESD M = 2
            //Use Lanes 3 & 4 for ORx Channel
            Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            byte framerM = 4;
            byte serializerLanesEnabled = 0x03;

            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte deframerM = 4;
            byte desLanesEnabled = 0x0F;

            byte Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
            byte obsRxframerM = 2;
            byte obsRxserializerLanesEnabled = 0xC;
            byte framerKinit = 4;
            byte framerKinc = 29;
            byte deframerKinit = 9;
            byte deframerKinc = 12;
            switch (DataPath)
            {
                case MYK_DATAPATH_MODE.RX2TX2OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
                    framerM = 4;
                    txChannels = Mykonos.TXCHANNEL.TX1_TX2;
                    deframerM = 4;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        default:
                            invalidTest = 1;
                            break;
                    }
                    break;
                case MYK_DATAPATH_MODE.RX1TX2OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1;
                    framerM = 2;
                    txChannels = Mykonos.TXCHANNEL.TX1_TX2;
                    deframerM = 4;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            desLanesEnabled = 0x0F;
                            serializerLanesEnabled = 0x03;
                            obsRxframerM = 2;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 9;
                            framerKinc = 12;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        default:
                            invalidTest = 0x1;
                            break;
                    }
                    break;
                case MYK_DATAPATH_MODE.RX2TX1OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
                    framerM = 4;
                    txChannels = Mykonos.TXCHANNEL.TX1;
                    deframerM = 2;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                            desLanesEnabled = 0x03;
                            serializerLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            desLanesEnabled = 0x0F;
                            serializerLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 19;
                            deframerKinc = 7;
                            break;
                        default:
                            invalidTest = 0x1;
                            break;
                    }
                    break;
                case MYK_DATAPATH_MODE.RX1TX1OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1;
                    framerM = 2;
                    txChannels = Mykonos.TXCHANNEL.TX1;
                    deframerM = 2;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL1OBSL2TL2:
                            desLanesEnabled = 0x03;
                            serializerLanesEnabled = 0x01;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                            desLanesEnabled = 0x0F;
                            serializerLanesEnabled = 0x01;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 4;
                            framerKinc = 29;
                            deframerKinit = 19;
                            deframerKinc = 7;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                            desLanesEnabled = 0x03;
                            serializerLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 9;
                            framerKinc = 12;
                            deframerKinit = 9;
                            deframerKinc = 12;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            desLanesEnabled = 0x0F;
                            serializerLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            framerKinit = 9;
                            framerKinc = 12;
                            deframerKinit = 19;
                            deframerKinc = 7;
                            break;
                        default:
                            invalidTest = 0x1;
                            break;
                    }
                    break;
                default:
                    invalidTest = 0x1;
                    break;
            }
            if (invalidTest != 0x1)
            {
                byte framerK;
                for (framerK = framerKinit; framerK <= 33; framerK += framerKinc)
                {
                    byte obsRxframerK;
                    for (obsRxframerK = 9; obsRxframerK <= 33; obsRxframerK += 12)
                    {
                        byte deframerK;
                        for (deframerK = deframerKinit; deframerK <= 33; deframerK += deframerKinc)
                        {

                            byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                            byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                            byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                            DateTime timeStamp = DateTime.Now;

                            string text = "Tx M: " + deframerM + ", Tx K: " + deframerK +
                                        ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) +
                                        ", Tx F: " + deframerF +
                                        ", Rx M: " + framerM + ", Rx K: " + framerK +
                                        ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) +
                                        ", Rx F: " + framerF +
                                        ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK +
                                        ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) +
                                        ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                            {
                                file.WriteLine(text);
                            }
                            //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                            JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM,
                                framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK,
                                obsRxserializerLanesEnabled);
                            TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                            mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                            mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                            mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// TestJESDParam function tests Mykonos with all valid JESD parameter combinations 
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// Each group is a different combination of Rx channel, Tx channel, Rx lane, Tx lane and ORx lane
        /// </summary>
        [Test, Combinatorial]
        [Category("JESD")]
        public static void TestJESDBetaParam([Values(MYK_DATAPATH_MODE.RX2TX2OBS1, MYK_DATAPATH_MODE.RX1TX2OBS1,
                      MYK_DATAPATH_MODE.RX2TX1OBS1, MYK_DATAPATH_MODE.RX1TX1OBS1)] MYK_DATAPATH_MODE DataPath,
                      [Values(MYK_JESD_LANE_CFG.RL1OBSL0TL2,
                              MYK_JESD_LANE_CFG.RL1OBSL0TL4,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL2,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL0TL2,
                              MYK_JESD_LANE_CFG.RL2OBSL0TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL2,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL4
                                )]MYK_JESD_LANE_CFG LaneCfg)
        {

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.resetDevice();

            Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;

            byte deframerM = 0;
            byte desLanesEnabled = 0;

            byte framerM = 0;
            byte serializerLanesEnabled = 0;

            byte obsRxframerM = 0;
            byte obsRxserializerLanesEnabled = 0;
            byte invalidTest = 0;
            switch (DataPath)
            {
                case MYK_DATAPATH_MODE.RX2TX2OBS1:
                    //Data Path Configuration
                    rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
                    framerM = 4;

                    txChannels = Mykonos.TXCHANNEL.TX1_TX2;
                    deframerM = 4;

                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                     //Lane Config
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        default:
                            invalidTest = 1;
                            break;

                    }
                    break;

                case MYK_DATAPATH_MODE.RX1TX2OBS1:
                    //Data Path Configuration
                    rxChannels = Mykonos.RXCHANNEL.RX1;
                    framerM = 2;
                    txChannels = Mykonos.TXCHANNEL.TX1_TX2;
                    deframerM = 4;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    //Lane Config
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL1OBSL0TL4:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        default:
                            invalidTest = 1;
                            break;
                    }
                    break;
                case MYK_DATAPATH_MODE.RX2TX1OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
                    framerM = 4;
                    txChannels = Mykonos.TXCHANNEL.TX1;
                    deframerM = 2;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;

                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0x0;
                            break;

                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        default:
                            invalidTest = 1;
                            break;
                    }
                    break;

                case MYK_DATAPATH_MODE.RX1TX1OBS1:
                    rxChannels = Mykonos.RXCHANNEL.RX1;
                    framerM = 2;
                    txChannels = Mykonos.TXCHANNEL.TX1;
                    deframerM = 2;
                    Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
                    obsRxframerM = 2;
                    switch (LaneCfg)
                    {
                        case MYK_JESD_LANE_CFG.RL1OBSL2TL2:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL1OBSL0TL2:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0x0;
                            break;

                        case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL1OBSL0TL4:
                            serializerLanesEnabled = 0x01;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x03;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0xC;
                            break;
                        case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                            serializerLanesEnabled = 0x03;
                            desLanesEnabled = 0x0F;
                            obsRxserializerLanesEnabled = 0x0;
                            break;
                        default:
                            invalidTest = 1;
                            break;
                    }
                    break;
                default:
                    break;
            }
            if (invalidTest != 1)
            {
                byte deframerF = (byte)((2 * (int)deframerM) / (int)ConvertMaskToCount(desLanesEnabled));
                byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

                byte framerKmin = CalculateStartK(framerF);
                byte deframerKmin = CalculateStartK(deframerF);
                byte obsRxframerKmin = CalculateStartK(obsRxframerF);

                byte framerKincrement = CalculateKIncrement(framerF);
                byte deframerKincrement = CalculateKIncrement(deframerF);
                byte obsRxframerKincrement = CalculateKIncrement(obsRxframerF);

                for (byte framerK = framerKmin; framerK < 33; framerK += framerKincrement)
                {
                    for (byte obsRxframerK = obsRxframerKmin; obsRxframerK < 33; obsRxframerK += obsRxframerKincrement)
                    {
                        for (byte deframerK = deframerKmin; deframerK < 33; deframerK += deframerKincrement)
                        {
                            Console.Write(deframerF);
                            DateTime timeStamp = DateTime.Now;

                            string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                                            ", Rx M: " + framerM + ", Rx K: " + framerK + ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                                            ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
                            {
                                file.WriteLine(text);
                            }

                            //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                            JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                            TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                            mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
                            mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
                            mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
                        }
                    }
                }
            }
        }
    }
}
