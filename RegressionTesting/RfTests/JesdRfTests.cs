#define WR_RES_TO_CONSOLE //Option to write out results to console
#define WR_RES_TO_PDF     //Option to Create PDF report
#define WR_RES_TO_TXT     //Option to write Data to Text File

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using niVisaWrapper;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;

using iTextSharp.text;
using iTextSharp.text.pdf;
using AdiCmdServerClient;
using DotNETRoutines;
using AdiMathLibrary;






namespace mykonosUnitTest
{
    

    /// <summary>
    /// RF Test Suite for Rx Inputs
    /// </summary>
    //[TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "3.5")]
    //[TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "3.5")]
    //[TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "3.5")]
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "2.5")]
    //[TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "2.5")]
    //[TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "2.5")]
    //[TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "0.7")]
    //[TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "0.7")]
    //[TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "0.7")]
    [NUnit.Framework.Category("RF")]
    public class JesdRfTests
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
        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string RxProfile;
        private string RxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\RxRfJesdTests\";
        public JesdRfTests()
        {
            this.RxProfile = settings.mykSettings.rxProfileName;
            this.RxProfileString = Helper.parseProfileName(RxProfile);
            ResPath = System.IO.Path.Combine(ResPath, RxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public JesdRfTests(string RxProfile, string freq)
        {
            this.RxProfile = RxProfile;
            this.RxProfileString = Helper.parseProfileName(RxProfile);
            ResPath = System.IO.Path.Combine(ResPath, RxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
            if (freq == "3.5")
            {
                settings.rxPllLoFreq_Hz = 3500000000;
            }
            else if (freq == "2.5")
            {
                settings.rxPllLoFreq_Hz = 2500000000;
            }
            else if (freq == "0.7")
            {
                settings.rxPllLoFreq_Hz = 700000000;
            }
            else
            {
                settings.rxPllLoFreq_Hz = 2500000000;
            }

        }

        /// <summary>
        /// Mykonos Test Setup Prior to RF Testing
        /// Setup Parameters:  
        /// From Locally Stored ARM Firmware     @"..\..\..\resources\Profiles";
        /// From Locally Stored Default Profile  @"..\..\..\resources\ArmBinaries"
        /// </summary>
        [SetUp]
        public void JesdRfTestsInit()
        {
            //Use Default Test Constructor
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
        }
   

         public static void TxLoopbackRxCapture([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel,
                                          [Values(-20)]int amp_dbm,
                                          [Values(10)]int OffSet,
                                           [Values(20000)]int IQExptVal)
         {

             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] rxprofileInfo = new double[3];
             rxprofileInfo[0] = settings.rxProfileData.IqRate_kHz;
             rxprofileInfo[1] = settings.rxPllLoFreq_Hz;
             rxprofileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double[] txprofileInfo = new double[3];
             txprofileInfo[0] = settings.txProfileData.IqRate_kHz;
             txprofileInfo[1] = settings.txPllLoFreq_Hz;
             txprofileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;

             
             double samplingFreq_Hz = rxprofileInfo[0] * 1000;
             double profileBW_MHz = rxprofileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = rxprofileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

         

             //Define DataCapture Parameters
             const int NUM_SAMPLES = 8192;
             short[] rxDataArray = new short[NUM_SAMPLES * 2];
             double[,] timeDomainData = new double[NUM_SAMPLES / 2, 3];
             double[,] DomainData = new double[(2 * OffSet), 2];


             //Enable Mykonos Rx Datapath
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.radioOn();
             Link.Disconnect();

             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, txprofileInfo, OffSet * 100000, amp_dbm);

             System.Threading.Thread.Sleep(500);
             rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);

             //Frequency Domain Data Processing
             AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
             double samplingFreq_MHz = samplingFreq_Hz / 1000000;
             byte sampleBitWidth = 16;
             double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

             //Define the 2D array to store frequency bins corresponding to fft data
             double[,] fftFreqAmp = new double[data.Length, 2];
             double binSize = (samplingFreq_MHz / NUM_SAMPLES);
             double minFreq = samplingFreq_MHz / 2 * (-1);
             for (int i = 0; i < data.Length; i++)
             {
                 fftFreqAmp[i, 0] = minFreq + (binSize * 2 * i);
                 fftFreqAmp[i, 1] = data[i];
             }

             //Time Domain Data Processing
             int numSamplesDiv2 = (int)NUM_SAMPLES / 2;
             for (int i = 0; i < numSamplesDiv2; i++)
             {
                 timeDomainData[i, 0] = i;
                 timeDomainData[i, 1] = rxDataArray[2 * i];
                 timeDomainData[i, 2] = rxDataArray[2 * i + 1];
             }


             var IMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => timeDomainData[i, 1]).Min();
             var IMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => timeDomainData[i, 1]).Max();
             var QMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => timeDomainData[i, 2]).Min();
             var QMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => timeDomainData[i, 2]).Max();
             Console.WriteLine("I Max, Min:" + IMax.ToString() + "," + IMin.ToString());
             Console.WriteLine("Q Max, Min:" + QMax.ToString() + "," + QMin.ToString());
             NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Less(IMin, (IQExptVal * (-1)));
             NUnit.Framework.Assert.Less(IMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Greater(IMax, IQExptVal);

             NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Less(QMin, (IQExptVal * (-1)));
             NUnit.Framework.Assert.Less(QMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Greater(QMax, IQExptVal);


#if WR_RES_TO_PDF

             string path = JesdRfTests.ResPath + "Rx_JESD_FFT_TimeDomain_Plots";
             if (channel == Mykonos.RXCHANNEL.RX1)
                 path = path + "RX1";
             else
                 path = path + "RX2";

             string[] timeLabels = new string[] { "Time Domain Response of " + channel.ToString(), "Sample Number", "ADC Codes", "I data", "Q data" };
             string[] fftLabels = new string[] { "Frequency Domain Response of " + channel.ToString(), "Frequency (MHz)", "Amplitude (dBFS)", "FFT DATA" };                                                                                        // Should be >=4 long. 

             var doc1 = new Document();
             iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
             container[0] = Helper.MakeChartObject(timeDomainData, timeLabels, path);
             container[1] = Helper.MakeChartObject(fftFreqAmp, fftLabels, path);
             string[] pcbInfo;
             pcbInfo = Helper.PcbInfo();

             Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

             // open result pdf
             System.Diagnostics.Process.Start(path + ".pdf");
#endif

         }

         public static void TxLoopbackRx1Rx2Capture(int amp_dbm1,
                                                        int OffSet1,
                                                        int IQExptVal1,
                                                        int amp_dbm2,
                                                        int OffSet2,
                                                        int IQExptVal2)
         {

             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] rxprofileInfo = new double[3];
             rxprofileInfo[0] = settings.rxProfileData.IqRate_kHz;
             rxprofileInfo[1] = settings.rxPllLoFreq_Hz;
             rxprofileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double[] txprofileInfo = new double[3];
             txprofileInfo[0] = settings.txProfileData.IqRate_kHz;
             txprofileInfo[1] = settings.txPllLoFreq_Hz;
             txprofileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;


             double samplingFreq_Hz = rxprofileInfo[0] * 1000;
             double profileBW_MHz = rxprofileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = rxprofileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);



             //Define DataCapture Parameters
             const int NUM_SAMPLES = 8192;
             short[] rx1DataArray = new short[NUM_SAMPLES * 2];
             double[,] rx1timeDomainData = new double[NUM_SAMPLES / 2, 3];

             short[] rx2DataArray = new short[NUM_SAMPLES * 2];
             double[,] rx2timeDomainData = new double[NUM_SAMPLES / 2, 3];

            

             //Enable Mykonos Rx and Tx Datapaths
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
#if false    //Debug To check Test works- messup ADC Sampler Crossbar  
             byte dataRb;
             dataRb = Link.spiRead(0x082);
             Console.Write(dataRb.ToString("X"));
             Link.spiWrite(0x082, 0xF0);
             dataRb = Link.spiRead(0x082);
             Console.Write(dataRb.ToString("X"));
#endif
#if false    //Debug To check Test works- messup Lane Crossbar  
             byte dataRb;
             dataRb = Link.spiRead(0x083);
             Console.Write(dataRb.ToString("X"));
             Link.spiWrite(0x083, 0xE0);
             dataRb = Link.spiRead(0x083);
             Console.Write(dataRb.ToString("X"));
#endif
             Link.Mykonos.radioOn();
             Link.Disconnect();

             //Generate Loopback Tone
             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1, txprofileInfo, OffSet1 * 100000, amp_dbm1);
             Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX2, txprofileInfo, OffSet2 * 100000, amp_dbm2);


             //Capture Rx Data Tone
             System.Threading.Thread.Sleep(500);
             rx1DataArray = Helper.MykonosRxCapture(Mykonos.RXCHANNEL.RX1, NUM_SAMPLES);
             rx2DataArray = Helper.MykonosRxCapture(Mykonos.RXCHANNEL.RX2, NUM_SAMPLES);

             //Frequency Domain Data Processing for Rx1
             AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
             double samplingFreq_MHz = samplingFreq_Hz / 1000000;
             byte sampleBitWidth = 16;
             double[] data = AdiMath.complexfftAndScale(rx1DataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

             //Define the 2D array to store frequency bins corresponding to fft data
             double[,] fftFreqAmp = new double[data.Length, 2];
             double binSize = (samplingFreq_MHz / NUM_SAMPLES);
             double minFreq = samplingFreq_MHz / 2 * (-1);
             for (int i = 0; i < data.Length; i++)
             {
                 fftFreqAmp[i, 0] = minFreq + (binSize * 2 * i);
                 fftFreqAmp[i, 1] = data[i];
             }

             //Time Domain Data Processing
             int numSamplesDiv2 = (int)NUM_SAMPLES / 2;
             for (int i = 0; i < numSamplesDiv2; i++)
             {
                 rx1timeDomainData[i, 0] = i;
                 rx1timeDomainData[i, 1] = rx1DataArray[2 * i];
                 rx1timeDomainData[i, 2] = rx1DataArray[2 * i + 1];
             }

             //Frequency Domain Data Processing for Rx2
             data = AdiMath.complexfftAndScale(rx2DataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

             //Define the 2D array to store frequency bins corresponding to fft data
             fftFreqAmp = new double[data.Length, 2];
             binSize = (samplingFreq_MHz / NUM_SAMPLES);
             minFreq = samplingFreq_MHz / 2 * (-1);
             for (int i = 0; i < data.Length; i++)
             {
                 fftFreqAmp[i, 0] = minFreq + (binSize * 2 * i);
                 fftFreqAmp[i, 1] = data[i];
             }

             //Time Domain Data Processing
             numSamplesDiv2 = (int)NUM_SAMPLES / 2;
             for (int i = 0; i < numSamplesDiv2; i++)
             {
                 rx2timeDomainData[i, 0] = i;
                 rx2timeDomainData[i, 1] = rx2DataArray[2 * i];
                 rx2timeDomainData[i, 2] = rx2DataArray[2 * i + 1];
             }

             //Check IQ Data For Rx1
             var IMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx1timeDomainData[i, 1]).Min();
             var IMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx1timeDomainData[i, 1]).Max();
             var QMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx1timeDomainData[i, 2]).Min();
             var QMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx1timeDomainData[i, 2]).Max();
             Console.WriteLine("I Max, Min:" + IMax.ToString() + "," + IMin.ToString());
             Console.WriteLine("Q Max, Min:" + QMax.ToString() + "," + QMin.ToString());
             NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal1 + ((IQExptVal1 * 10) / 100))));
             NUnit.Framework.Assert.Less(IMin, (IQExptVal1 * (-1)));
             NUnit.Framework.Assert.Less(IMax, ((IQExptVal1 + ((IQExptVal1 * 10) / 100))));
             NUnit.Framework.Assert.Greater(IMax, IQExptVal1);

             NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal1 + ((IQExptVal1 * 10) / 100))));
             NUnit.Framework.Assert.Less(QMin, (IQExptVal1 * (-1)));
             NUnit.Framework.Assert.Less(QMax, ((IQExptVal1 + ((IQExptVal1 * 10) / 100))));
             NUnit.Framework.Assert.Greater(QMax, IQExptVal1);

             //Check IQ Data For Rx2
             IMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx2timeDomainData[i, 1]).Min();
             IMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx2timeDomainData[i, 1]).Max();
             QMin = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx2timeDomainData[i, 2]).Min();
             QMax = System.Linq.Enumerable.Range(0, numSamplesDiv2).Select(i => rx2timeDomainData[i, 2]).Max();
             Console.WriteLine("I Max, Min:" + IMax.ToString() + "," + IMin.ToString());
             Console.WriteLine("Q Max, Min:" + QMax.ToString() + "," + QMin.ToString());
             NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal2 + ((IQExptVal2 * 10) / 100))));
             NUnit.Framework.Assert.Less(IMin, (IQExptVal2 * (-1)));
             NUnit.Framework.Assert.Less(IMax, ((IQExptVal2 + ((IQExptVal2 * 10) / 100))));
             NUnit.Framework.Assert.Greater(IMax, IQExptVal2);

             NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal2 + ((IQExptVal2 * 10) / 100))));
             NUnit.Framework.Assert.Less(QMin, (IQExptVal2 * (-1)));
             NUnit.Framework.Assert.Less(QMax, ((IQExptVal2 + ((IQExptVal2 * 10) / 100))));
             NUnit.Framework.Assert.Greater(QMax, IQExptVal2);


#if false

             string path = JesdRfTests.ResPath + "Rx_JESD_FFT_TimeDomain_Plots";
             path = path + "RX2";

             string[] timeLabels = new string[] { "Time Domain Response of " + "RX2", "Sample Number", "ADC Codes", "I data", "Q data" };
             string[] fftLabels = new string[] { "Frequency Domain Response of " + "RX2", "Frequency (MHz)", "Amplitude (dBFS)", "FFT DATA" };                                                                                        // Should be >=4 long. 

             var doc1 = new Document();
             iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
             container[0] = Helper.MakeChartObject(rx2timeDomainData, timeLabels, path);
             container[1] = Helper.MakeChartObject(fftFreqAmp, fftLabels, path);
             string[] pcbInfo;
             pcbInfo = Helper.PcbInfo();

             Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

             // open result pdf
             System.Diagnostics.Process.Start(path + ".pdf");
#endif

         }


         public static void UpdateJesdConfig(byte deframerM, byte deframerK, byte desLaneEn,
                                byte framerM, byte framerK, byte serLaneEn, 
                                byte obsFramerM, byte obsRxframerK, byte obsRxserLaneEn)
         {

             settings.mykTxDeFrmrCfg.M = deframerM;
             settings.mykTxDeFrmrCfg.K = deframerK;
             settings.mykTxDeFrmrCfg.deserializerLanesEnabled = desLaneEn;

             settings.mykRxFrmrCfg.M = framerM;
             settings.mykRxFrmrCfg.K = framerK;
             settings.mykRxFrmrCfg.serializerLanesEnabled = serLaneEn;

             settings.mykObsRxFrmrCfg.M = obsFramerM;
             settings.mykObsRxFrmrCfg.K = obsRxframerK;
             settings.mykObsRxFrmrCfg.serializerLanesEnabled = obsRxserLaneEn;
             

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

         public static byte ConfigJesdParams(MYK_DATAPATH_MODE DataPath,
                                                 MYK_JESD_LANE_CFG LaneCfg,
                                                [Values(12, 32)]byte framerK,
                                    [Values(12, 32)]byte obsRxframerK,
                                    [Values(20, 32)]byte deframerK )
         {

             Mykonos.RXCHANNEL rxChannels = Mykonos.RXCHANNEL.RX1_RX2;
             Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
             byte Obsrxchannels = (byte) Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;

             byte deframerM = 0;
             byte desLaneEn = 0;

             byte framerM = 0;
             byte serLaneEn = 0;

             byte obsFramerM = 0;
             byte obsSerLaneEn = 0;

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
                     obsFramerM = 2;
                     //Lane Config
                     switch (LaneCfg)
                     {
                         case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                             serLaneEn = 0x03;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0x0;
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
                     obsFramerM = 2;
                     //Lane Config
                     switch (LaneCfg)
                     {
                         case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                             serLaneEn = 0x01;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL1OBSL0TL4:
                             serLaneEn = 0x01;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0x0;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0x0;
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
                     obsFramerM = 2;

                     switch (LaneCfg)
                     {
                         case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                             serLaneEn = 0x03;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                             serLaneEn = 0x03;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0x0;
                             break;

                         case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0x0;
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
                     obsFramerM = 2;
                     switch (LaneCfg)
                     {
                         case MYK_JESD_LANE_CFG.RL1OBSL2TL2:
                             serLaneEn = 0x01;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL1OBSL0TL2:
                             serLaneEn = 0x01;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0x0;
                             break;

                         case MYK_JESD_LANE_CFG.RL1OBSL2TL4:
                             serLaneEn = 0x01;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL1OBSL0TL4:
                             serLaneEn = 0x01;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0x0;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL2TL2:
                             serLaneEn = 0x03;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL2:
                             serLaneEn = 0x03;
                             desLaneEn = 0x03;
                             obsSerLaneEn= 0x0;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL2TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0xC;
                             break;
                         case MYK_JESD_LANE_CFG.RL2OBSL0TL4:
                             serLaneEn = 0x03;
                             desLaneEn = 0x0F;
                             obsSerLaneEn= 0x0;
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
                 byte deframerF = (byte)((2 * (int)deframerM) / (int)ConvertMaskToCount(desLaneEn));
                 byte framerF = (byte)(2 * (int)framerM / ConvertMaskToCount(serLaneEn));
                 byte obsFramerF = (byte)(2 * (int)obsFramerM / ConvertMaskToCount(obsSerLaneEn));

                 settings.mykSettings.rxChannel = rxChannels;
                 settings.mykSettings.txChannel = txChannels;
                 settings.mykSettings.orxChannel = (Mykonos.OBSRXCHANNEL_ENABLE) Obsrxchannels;
                 settings.mykTxDeFrmrCfg.M = deframerM;
                 settings.mykTxDeFrmrCfg.K = deframerK;
                 settings.mykTxDeFrmrCfg.deserializerLanesEnabled = desLaneEn;

                 settings.mykRxFrmrCfg.M = framerM;
                 settings.mykRxFrmrCfg.K = framerK;
                 settings.mykRxFrmrCfg.serializerLanesEnabled = serLaneEn;

                 settings.mykObsRxFrmrCfg.M = obsFramerM;
                 settings.mykObsRxFrmrCfg.K = obsRxframerK;
                 settings.mykObsRxFrmrCfg.serializerLanesEnabled = obsSerLaneEn;
                 
                 return 0;

             }
             return 1;
         }


         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void JesdDefConfigRx1Rx2CaptureTest([Values(-20)]int amp_dbm1,
                                                        [Values(10)]int OffSet1,
                                                        [Values(20000)]int IQExptVal1,
                                                        [Values(-10)]int amp_dbm2,
                                                        [Values(5)]int OffSet2,
                                                        [Values(30000)]int IQExptVal2)
         {

            //Configure Chip and Data Paths with default JESD Settings
            TestSetup.TestSetupInit(settings);
            TxLoopbackRx1Rx2Capture(amp_dbm1,OffSet1, IQExptVal1, amp_dbm2,OffSet2, IQExptVal2 );

         }

         [Test, Combinatorial]
         [NUnit.Framework.Category("JESD")]
         public static void JesdConfigRx1Rx2CaptureTest([Values(JESDAlphaTests.MYK_DATAPATH_MODE.RX2TX2OBS1)]MYK_DATAPATH_MODE DataPath,
                                    [Values(MYK_JESD_LANE_CFG.RL1OBSL2TL2, MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                                    MYK_JESD_LANE_CFG.RL2OBSL2TL2, MYK_JESD_LANE_CFG.RL2OBSL2TL4)]MYK_JESD_LANE_CFG LaneCfg,
                                    [Values(12, 32)]byte framerK,
                                    [Values(12, 32)]byte obsRxframerK,
                                    [Values(20, 32)]byte deframerK)
         {

             //Define Test Signal Parameters
             int amp_dbm1 = -20;
             int OffSet1 = 10;
             int IQExptVal1 = 20000;
             int amp_dbm2 = -10;
             int OffSet2 = 5;
             int IQExptVal2 = 30000;


             //Based on Test Parameters Determin Jesd Settings
              ConfigJesdParams(DataPath, LaneCfg, framerK, obsRxframerK, deframerK);

             //Configure Chip and Data Paths with Test Specific JESD Settings
             TestSetup.TestSetupInit(settings);
             //Enable System, Apply Signal, Analysis Captured Data
             TxLoopbackRx1Rx2Capture(amp_dbm1, OffSet1, IQExptVal1, amp_dbm2, OffSet2, IQExptVal2);

         }



         [Test, Combinatorial]
         [NUnit.Framework.Category("JESD")]
         public static void JesdConfigRx1CaptureTest([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel,[Values(JESDAlphaTests.MYK_DATAPATH_MODE.RX1TX1OBS1)]MYK_DATAPATH_MODE DataPath,
                                    [Values(MYK_JESD_LANE_CFG.RL1OBSL2TL2, MYK_JESD_LANE_CFG.RL1OBSL2TL4,
                                     MYK_JESD_LANE_CFG.RL2OBSL2TL2, MYK_JESD_LANE_CFG.RL2OBSL2TL4)]MYK_JESD_LANE_CFG LaneCfg,
                                    [Values(12, 32)]byte framerK,
                                    [Values(12, 32)]byte obsRxframerK,
                                    [Values(20, 32)]byte deframerK)
         {

             //Define Test Signal Parameters
             int amp_dbm1 = -20;
             int OffSet1 = 10;
             int IQExptVal1 = 20000;
             //Based on Test Parameters Determin Jesd Settings
             ConfigJesdParams(DataPath, LaneCfg, framerK, obsRxframerK, deframerK);
             //Configure Chip and Data Paths with Test Specific JESD Settings
             TestSetup.TestSetupInit(settings);
             //Enable System, Apply Signal, Analysis Captured Data
             TxLoopbackRxCapture(channel, amp_dbm1, OffSet1, IQExptVal1);

         }
     

    }
}
