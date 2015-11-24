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
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5")]
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5")]
    [Category("ApiFunctional")]
    public class JESDAlphaTests
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
        public static string resFilePath = @"..\..\..\TestResults\JESDTests\JESDSingleCaseResult.txt";
        public static string resFilePathAlpha = @"..\..\..\TestResults\JESDTests\JESDAlphaTestResult.txt";

        private string RxProfile;
        private string TxProfile;
        private string OrxProfile;

        public JESDAlphaTests(string RxProfile, string TxProfile, string OrxProfile)
        {
            this.RxProfile = RxProfile;
            this.TxProfile = TxProfile;
            this.OrxProfile = OrxProfile;
        }
        /// <summary>
        /// Initializes Mykonos settings and JESD204B settings
        /// JESD204B is configured with a set of input parameters
        /// </summary>

        public void JESDTestInit(byte deframerM, byte deframerK, byte desLanesEnabled, 
            byte framerM, byte framerK, byte serializerLanesEnabled, byte obsRxframerM, 
            byte obsRxframerK, byte obsRxserializerLanesEnabled)
        {
            //Set Test Parameters  
            //            "Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 75/200MHz, IQrate 245.76MHz, Dec5", "ORX 200MHz, IQrate 245.75MHz, Dec5"
            //("Rx 100MHz, IQrate 122.88MHz, Dec5", "Tx 20/100MHz, IQrate  122.88MHz, Dec5", "ORX 100MHz, IQrate 122.88MHz, Dec5"
            settings.mykSettings.DeviceClock_kHz = 122880;
            settings.mykSettings.rxChannel = (Mykonos.RXCHANNEL) 3;
//            settings.mykSettings.rxProfileName = "Rx 20MHz, IQrate 30.72MHz, Dec5";
//            settings.mykSettings.txProfileName = "Tx 20/100MHz, IQrate  122.88MHz, Dec5";
//            settings.mykSettings.orxProfileName = "ORX 100MHz, IQrate 122.88MHz, Dec5";
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

            settings.mykSettings.rxProfileName = RxProfile;
            settings.mykSettings.txProfileName = TxProfile;
            settings.mykSettings.orxProfileName = OrxProfile;
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
        /// <summary>
        /// TestJESDSingleCase function tests Mykonos with one JESD parameter combinations 
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// This test is used if any JESD parameter combination failed in TestJESDParam
        /// </summary>
        [Test]
        [Category("JESD")]
        public void TestJESDSingleCase()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.resetDevice();

            //Configure Rx & Orx DataPath Settings
            // 2 Rx Channels enabled equates to 4 Converters Represented JESD M = 4
            //Use Lanes 0 & 1 for Rx1&Rx2 Channels
            // 1 Observer Channel from ORX1 Input equates to 2 Converters JESD M = 2
            //Use Lanes 3 & 4 for ORx Channel
            //Use K = 32 
            //Calculate F
            Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            byte framerM = 4;
            byte serializerLanesEnabled = 0x03;
            byte framerK = 32;
            byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));


            byte Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
            byte obsRxframerM = 2;
            byte obsRxserializerLanesEnabled = 0x0;
            byte obsRxframerK = 32;
            byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte deframerM = 4;
            byte desLanesEnabled = 0x0F;
            byte deframerK = 32;
            byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));

            //Update Test settings structure with JESD Test JESD Settings
            JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
            //Call Test Initialisation with Minimum Initialisation for JESD Links
            TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

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

            //Enable Full JESD Links in the System
            mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
            mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
            mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
        }



    
        /// <summary>
        /// TestJESDParam function tests Mykonos with all valid JESD data path Configurations
        /// but with only a subset of the K Parameter Settings K= 16 and K =32
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// Each group is a different combination of Rx channel, Tx channel, Rx lane, Tx lane and ORx lane
        /// </summary>
        [Test, Combinatorial]
        [Category("JESD")]
        public void TestJESDAlfaParam([Values(JESDAlphaTests.MYK_DATAPATH_MODE.RX2TX2OBS1, MYK_DATAPATH_MODE.RX1TX2OBS1,
                       MYK_DATAPATH_MODE.RX2TX1OBS1, MYK_DATAPATH_MODE.RX1TX1OBS1)]MYK_DATAPATH_MODE DataPath,
                      [Values(MYK_JESD_LANE_CFG.RL1OBSL0TL2, MYK_JESD_LANE_CFG.RL1OBSL0TL4,
                              MYK_JESD_LANE_CFG.RL1OBSL2TL2, MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL0TL2, MYK_JESD_LANE_CFG.RL2OBSL0TL4,
                              MYK_JESD_LANE_CFG.RL2OBSL2TL2, MYK_JESD_LANE_CFG.RL2OBSL2TL4 )]MYK_JESD_LANE_CFG LaneCfg,
                     [Values(16, 32)]byte framerK, 
                     [Values(16, 32)]byte obsRxframerK, 
                     [Values(20, 32)]byte deframerK)
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
                            desLanesEnabled = 0x0F;
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


                byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));
                byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));
                byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

               DateTime timeStamp = DateTime.Now;

               string text = "Tx M: " + deframerM + ", Tx K: " + deframerK + 
                           ", Tx lanes: " + string.Format("{0:X}", desLanesEnabled) + ", Tx F: " + deframerF +
                           ", Rx M: " + framerM + ", Rx K: " + framerK + 
                           ", Rx lanes: " + string.Format("{0:X}", serializerLanesEnabled) + ", Rx F: " + framerF +
                           ", ORx M: " + obsRxframerM + ", ORx K: " + obsRxframerK + 
                           ", ORx lanes: " + string.Format("{0:X}", obsRxserializerLanesEnabled) + 
                           ", ORx F: " + obsRxframerF + "  " + timeStamp.ToString();

               using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePathAlpha, true))
                {
                    file.WriteLine(text);
                }

                //Initialize(rxChannels, txChannels, Obsrxchannels, deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
                Console.WriteLine("Rx Profile: " + settings.mykSettings.rxProfileName);
                Console.WriteLine("Tx Profile: " + settings.mykSettings.txProfileName);
                Console.WriteLine("ORx Profile: " + settings.mykSettings.orxProfileName);
                TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);

                mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePathAlpha);
                mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePathAlpha);
                mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePathAlpha);

            }
        }



        /// <summary>
        /// TestJESDDeframerCrossbar tests Mykonos with static JESD parameter combinations.
        /// Varies the Tx Deframer crossbar setting
        /// Reports Rx framer, ORx framer and Tx deframer status
        /// </summary>
        [Test, Combinatorial]
        [Category("JESD")]
        public void TestJESDDeframerCrossbar([Values(0xE4, 0x1B, 0xB1, 0x4E)] Byte deframerCrossbar,
            [Values(0xC, 0x3, 0xF)]Byte desLanesEnabled)
        {
            Console.WriteLine(settings.txProfileData.IqRate_kHz);
            //Invalid test conditions, iqrate too high. Temp workaround
            if ((desLanesEnabled != 0xF) && (TxProfile.Contains("245.76")))
                Assert.Pass();
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            Link.Mykonos.resetDevice();

            //Configure Rx & Orx DataPath Settings
            // 2 Rx Channels enabled equates to 4 Converters Represented JESD M = 4
            //Use Lanes 0 & 1 for Rx1&Rx2 Channels
            // 1 Observer Channel from ORX1 Input equates to 2 Converters JESD M = 2
            //Use Lanes 3 & 4 for ORx Channel
            //Use K = 32 
            //Calculate F
            Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
            byte framerM = 4;
            byte serializerLanesEnabled = 0x03;
            byte framerK = 32;
            byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serializerLanesEnabled));


            byte Obsrxchannels = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
            byte obsRxframerM = 2;
            byte obsRxserializerLanesEnabled = 0xC;
            byte obsRxframerK = 32;
            byte obsRxframerF = (byte)(2 * (int)obsRxframerM / ConvertMaskToCount(obsRxserializerLanesEnabled));

            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte deframerM = 4;
           // byte desLanesEnabled = 0x04;
            byte deframerK = 32;
            byte deframerF = (byte)(2 * (int)deframerM / ConvertMaskToCount(desLanesEnabled));

            //Update Test settings structure with JESD Test JESD Settings
            JESDTestInit(deframerM, deframerK, desLanesEnabled, framerM, framerK, serializerLanesEnabled, obsRxframerM, obsRxframerK, obsRxserializerLanesEnabled);
            Console.WriteLine("desLanesEnabled: " + desLanesEnabled);
            Console.WriteLine("desLanesEnabled: " + settings.txProfileData.IqRate_kHz);
            //Vary the Deframer Crossbar Setting
            settings.mykTxDeFrmrCfg.deserializerLaneCrossbar = deframerCrossbar;

            //Call Test Initialisation with Minimum Initialisation for JESD Links
            TestSetup.JESDTestSetupInit(settings, rxChannels, txChannels, Obsrxchannels);
            //Enable Full JESD Links in the System
            mykonosUnitTest.Helper.EnableRxFramerLink_JESD(resFilePath);
            mykonosUnitTest.Helper.EnableORxFramerLink_JESD(resFilePath);
            mykonosUnitTest.Helper.EnableTxLink_JESD(resFilePath);
            Console.Write(deframerCrossbar.ToString("X"));
            byte temp = 0;
            byte deframerCrossbarNew = 0x00;
            byte deframerCrossbarRb = 0xFF;
            if (desLanesEnabled == 0xC)
            {
                temp = (byte)(deframerCrossbar >> 4);
                deframerCrossbarNew = temp;
                Console.Write(deframerCrossbarNew.ToString("X"));
            }
            else if (desLanesEnabled == 0x3)
            {
                deframerCrossbarNew = (byte) (deframerCrossbar & 0xF);
                Console.Write(deframerCrossbarNew.ToString("X"));
            }
            else
            {
                deframerCrossbarNew = deframerCrossbar;
                Console.Write(deframerCrossbarNew.ToString("X"));
            }
            if ((deframerM == 4) && (ConvertMaskToCount(desLanesEnabled) == 2))
            {
                temp = (byte)((deframerCrossbarNew & 0xC0) | ((deframerCrossbarNew & 0xC) << 2)
                                    | ((deframerCrossbarNew & 0x30) >> 2) | (deframerCrossbarNew & 0x3));
                deframerCrossbarNew = temp;
                Console.Write(deframerCrossbarNew.ToString("X"));
            }



            deframerCrossbarRb = Link.spiRead(0x83);
            Console.Write(deframerCrossbarRb.ToString("X"));
            Assert.AreEqual((deframerCrossbarNew ), deframerCrossbarRb );
        }

    }
}
