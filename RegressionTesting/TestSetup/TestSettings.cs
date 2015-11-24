using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdiCmdServerClient;

namespace mykonosUnitTest
{
    public class MykonosJesdFramerConfig
    {
        public byte bankId;
        public byte deviceId;
        public byte laneId;
        public byte M;
        public byte K;
        public byte scramble;
        public byte externalSysref;
        public byte serializerLanesEnabled;
        public byte serializerLaneCrossbar;
        public byte serializerAmplitude;
        public byte preEmphasis;
        public byte invertLanePolarity;
        public byte lmfcOffset;
        public byte newSysrefOnRelink;
        public byte enableAutoChanXbar;
        public byte obsRxSyncbSelect;
        public byte overSample;

        public MykonosJesdFramerConfig()
        {
            bankId = 0;
            deviceId = 0;
            laneId = 0;
            M = 4;
            K = 32;
            scramble = 1;
            externalSysref = 1;
            serializerLanesEnabled = 0x03;
            serializerLaneCrossbar = 0xE4;
            serializerAmplitude = 22;
            preEmphasis = 4;
            invertLanePolarity = 0;
            lmfcOffset = 0;
            obsRxSyncbSelect = 0;
            newSysrefOnRelink = 0;
            overSample = 0;
            enableAutoChanXbar = 0;
        }
    }
    public class MykonosJesdDeFramerConfig
    {
        public byte deviceID;
        public byte laneID;
        public byte bankID;
        public byte M;
        public byte K;
        public byte scramble;
        public byte externalSysref;
        public byte deserializerLanesEnabled;
        public byte deserializerLaneCrossbar;
        public byte eqSetting;
        public byte invertLanePolarity;
        public byte enableAutoChanXbar;
        public byte lmfcOffset;
        public byte newSysrefOnRelink;

        public MykonosJesdDeFramerConfig()
        {
            deviceID = 0;
            laneID = 0;
            bankID = 0;
            M = 4;
            K = 32;
            scramble = 1;
            externalSysref = 1;
            deserializerLanesEnabled = 0x0F;
            deserializerLaneCrossbar = 0xE4;
            eqSetting = 1;
            invertLanePolarity = 0;
            enableAutoChanXbar = 0;
            lmfcOffset = 0;
            newSysrefOnRelink = 0;
        }
    }
    public class MykonosConfig
    {
        public UInt32 DeviceClock_kHz = 0;
        public Mykonos.RXCHANNEL rxChannel = Mykonos.RXCHANNEL.RX1_RX2; //0=off, 1=Rx1, 2=Rx2, 3=Rx1+Rx2
        public Mykonos.TXCHANNEL txChannel = Mykonos.TXCHANNEL.TX1_TX2 ; //0=off, 1=Tx1, 2=Tx2, 3=Tx1+Tx2
        public Mykonos.OBSRXCHANNEL_ENABLE orxChannel = Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1; //0=off, 1=Tx1, 2=Tx2, 3=Tx1+Tx2
        public string rxProfileName = "None";
        public string txProfileName = "None";
        public string orxProfileName = "None";
        public string srxProfileName = "None";
        public string ArmFirmwareName = "None";
        
    }
    public class MykonosDpdConfig
    {
        public byte enableDpd = 1;
        public byte enableClgc = 0;
        public byte enableVswr = 0;
        public UInt16 dpdDamping = 13;
        public UInt16 dpdSamples = 2048;
        public UInt16 dpdOutlierThreshold = 1638;
        public Int16 dpdAdditionalDelayOffset = 0;
        public UInt16 dpdPathDelayPnSeqLevel = 255;
        public Int16 clgcDesiredGain = -20 * 100;
        public UInt16 clgcTxAttenLimit = 0;
        public UInt16 clgcControlRatio = 45;
    }
    public class TestSetupConfig
    {
        public static String ipAddr = "192.168.1.10";
        public static Int32 port = 55555;
        public string resourcePath = @"..\..\..\Resources\";
        public string resultsPath = @"..\..\..\TestResults\";
        public string testBoard = "3.5";
        public MykonosConfig mykSettings = new MykonosConfig();
        public MykonosProfileData rxProfileData = new MykonosProfileData();
        public MykonosProfileData txProfileData = new MykonosProfileData();
        public MykonosProfileData obsRxProfileData = new MykonosProfileData();
        public MykonosProfileData srxProfileData = new MykonosProfileData();
        public MykonosJesdFramerConfig mykRxFrmrCfg = new MykonosJesdFramerConfig();
        public MykonosJesdFramerConfig mykObsRxFrmrCfg = new MykonosJesdFramerConfig();
        public MykonosJesdDeFramerConfig mykTxDeFrmrCfg = new MykonosJesdDeFramerConfig();
        public MykDpdConfig mykDpdCfg = new MykDpdConfig();
        public MykClgcConfig mykClgcCfg = new MykClgcConfig();
        public ulong rxPllLoFreq_Hz = 0;
        public byte rxPllUseExtLo = 0;
        public byte rxUseRealIfData = 0;
        public ulong txPllLoFreq_Hz = 0;
        public byte txPllUseExtLo = 0;
        public byte txAttenStepSize = 0; /*TXATTEN_0P05_DB*/
        public ushort tx1Atten = 0;
        public ushort tx2Atten = 0;
        public ulong obsRxPllLoFreq_Hz = 0;
        public UInt32 calMask = 0;
        public UInt32 trackCalMask = 0;

        public TestSetupConfig()
        {
            mykSettings.DeviceClock_kHz = 245760;
            mykSettings.rxChannel = (Mykonos.RXCHANNEL) 3;
            mykSettings.rxProfileName = "Rx 100MHz, IQrate 122.88MHz, Dec5";
            mykSettings.txProfileName = "Tx 75/200MHz, IQrate 245.76MHz, Dec5";
            mykSettings.orxProfileName = "ORX 200MHz, IQrate 245.75MHz, Dec5";
            mykSettings.srxProfileName = "SRx 20MHz, IQrate 30.72MHz, Dec5";
            mykSettings.ArmFirmwareName = "Mykonos_M3.bin";
            
           

            mykTxDeFrmrCfg.deviceID = 0;
            mykTxDeFrmrCfg.laneID = 0;
            mykTxDeFrmrCfg.bankID = 0;
            mykTxDeFrmrCfg.M = 4;
            mykTxDeFrmrCfg.K = 32;
            mykTxDeFrmrCfg.scramble = 1;
            mykTxDeFrmrCfg.externalSysref = 1;
            mykTxDeFrmrCfg.deserializerLanesEnabled = 0x0F;
            mykTxDeFrmrCfg.deserializerLaneCrossbar = 0xE4;
            mykTxDeFrmrCfg.eqSetting = 1;
            mykTxDeFrmrCfg.invertLanePolarity = 0;
            mykTxDeFrmrCfg.enableAutoChanXbar = 0;
            mykTxDeFrmrCfg.lmfcOffset = 0;
            mykTxDeFrmrCfg.newSysrefOnRelink = 0;

            mykRxFrmrCfg.bankId = 0;
            mykRxFrmrCfg.deviceId = 0;
            mykRxFrmrCfg.laneId = 0;
            mykRxFrmrCfg.M = 4;
            mykRxFrmrCfg.K = 32;
            mykRxFrmrCfg.scramble = 1;
            mykRxFrmrCfg.externalSysref = 1;
            mykRxFrmrCfg.serializerLanesEnabled = 0x03;
            mykRxFrmrCfg.serializerLaneCrossbar = 0xE4;
            mykRxFrmrCfg.serializerAmplitude = 22;
            mykRxFrmrCfg.preEmphasis = 4;
            mykRxFrmrCfg.invertLanePolarity = 0;
            mykRxFrmrCfg.lmfcOffset = 0;
            mykRxFrmrCfg.obsRxSyncbSelect = 0;
            mykRxFrmrCfg.newSysrefOnRelink = 0;
            mykRxFrmrCfg.overSample = 0;
            mykRxFrmrCfg.enableAutoChanXbar = 0;


            mykObsRxFrmrCfg.bankId = 1;
            mykObsRxFrmrCfg.deviceId = 0;
            mykObsRxFrmrCfg.laneId = 0;
            mykObsRxFrmrCfg.M = 2;
            mykObsRxFrmrCfg.K = 32;
            mykObsRxFrmrCfg.scramble = 1;
            mykObsRxFrmrCfg.externalSysref = 1;
            mykObsRxFrmrCfg.serializerLanesEnabled = 0xC;
            mykObsRxFrmrCfg.serializerLaneCrossbar = 0xE4;
            mykObsRxFrmrCfg.serializerAmplitude = 22;
            mykObsRxFrmrCfg.preEmphasis = 4;
            mykObsRxFrmrCfg.invertLanePolarity = 0;
            mykObsRxFrmrCfg.lmfcOffset = 0;
            mykObsRxFrmrCfg.newSysrefOnRelink = 0;
            mykObsRxFrmrCfg.enableAutoChanXbar = 0;
            mykObsRxFrmrCfg.obsRxSyncbSelect = 1;
            mykObsRxFrmrCfg.overSample = 1;
            
            rxPllLoFreq_Hz = 2500000000;
            txPllLoFreq_Hz = 2500000000;
            obsRxPllLoFreq_Hz = 2600000000;
            calMask =      (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
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



            mykDpdCfg.AdditionalDelayOffset = 0;
            mykDpdCfg.Damping = 13;
            mykDpdCfg.HighSampleHistory = 1;
            mykDpdCfg.ModelVersion = 3;
            mykDpdCfg.NumWeights= 1;
            mykDpdCfg.OutlierThreshold = 1638;
            mykDpdCfg.PathDelayPnSeqLevel = 255;
            mykDpdCfg.Samples = 2048;
            //mykDpdCfg.WeightsImag = ?
           // mykDpdCfg.WeightsReal = ?

            mykClgcCfg.Tx1AttenLimit = 0;
            mykClgcCfg.Tx1ControlRatio = 45;
            mykClgcCfg.Tx1DesiredGain = -2000;
            mykClgcCfg.Tx2AttenLimit = 0;
            mykClgcCfg.Tx2ControlRatio = 45;
            mykClgcCfg.Tx2DesiredGain = -2000;
           
        }
    }

    
    
    
}
