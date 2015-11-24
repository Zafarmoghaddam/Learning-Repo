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
    /// RF Test Suite for Sniffer Input
    /// </summary>
    [TestFixture("SRx 20MHz, IQrate 30.72MHz, Dec5")]
    [NUnit.Framework.Category("RF")]
    public class SnRxRfTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string SnRxProfile;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\SnRxRfTests\";
        public SnRxRfTests()
        {
            this.SnRxProfile = settings.mykSettings.srxProfileName;
        }

        public SnRxRfTests(string SnRxProfile)
        {
            this.SnRxProfile = SnRxProfile;
        }

        /// <summary>
        /// Mykonos Test Setup Prior to RF Testing
        /// Setup Parameters:  
        /// From Locally Stored ARM Firmware     @"..\..\..\resources\Profiles";
        /// From Locally Stored Default Profile  @"..\..\..\resources\ArmBinaries"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: ARx Profile: As Per Test Fixture Parameter
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
        public void SnRxRfTestsInit()
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
            settings.mykSettings.orxProfileName =SnRxProfile;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
        }






        /// <summary>
        /// ORxGainSweep Inputs: Mykonos.OBSRXCHANNEL Enum. Uses the practical Observation ports, ORX1,2 and SNRXA,B,C.
        /// Generates a pdf output showing the fundamental amplitude versus gain index
        /// The argument to the gain index function is normalized to 255, such that max gain condition = 255
        /// ORx gain has values from 255 to 237. 
        /// SNRx gain index has values from 255 to 198
        /// 
        /// BUG: Due to the lack of the DC offset calibration on the SNRX path, the fft analysis starts to return the 
        /// magnitude of the DC offset instead of the applied tone since it is a max search algorithm. This is bad,
        /// however when the DC offset correction is applied to SNRX, the problem should be remedied.
        /// 
        /// Workaround: Prevent the max search from returning values that are near the DC indicies. Not yet implemented
        /// TODO: Understand how Sn and Or Gain tables should be tested.
        /// </summary>

        [Test, Sequential]
        [NUnit.Framework.Category("Snf")]
        public static void SnRxGainSweep([Values(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A, Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B, Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C)]Mykonos.SNIFFER_CHANNEL channel)
        {

            //Initialize param structure with Hardcoded Values
            //Check why Sniffer test checks OrxProfile
            double[] profileInfo = Helper.SetOrxProfileInfo(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A);
            double samplingFreq_MHz = profileInfo[0] / 1000;
            double freqLo_MHz = profileInfo[1] / 1000000;
            double profileBW_MHz = profileInfo[2] / 1000000;
            double testFreq = 2510;
            double amplitude_dBm = -20;
            

            int numIndices = 58;
            double[,] amplitudeData = new double[numIndices, 3]; //58
            double[,] amplitudeDiffData = new double[numIndices - 1, 2];
            short[] rxDataArray = new short[16384];
            Console.WriteLine("Detected LO Frequency: " + freqLo_MHz);
            Console.WriteLine("Profile BW: " + profileBW_MHz);

            testFreq = freqLo_MHz + 5;
            amplitude_dBm = -35;
           
            ///ESG Setup
            SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
            Console.WriteLine(measEquipment.ESGAddress);
            Console.WriteLine(esg.Identify());
            esg.SetFrequency(testFreq);
            esg.SetAmplitude(amplitude_dBm);
            esg.SetRfOutput(true);
            
            // ----- Test Execution ----- //
            byte gainIndex = 255;
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();

            for (int i = 0; i < (numIndices + 1); i++)
            {
                try
                {
                    Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                    //Link.spiWrite(0x516, 0x84);
                    gainIndex = (byte)(255 - i);


                    //Console.WriteLine("Register 0x512: " + Link.spiRead(0x512));
                    //Console.WriteLine("Register 0x513: " + Link.spiRead(0x513));


                    //Console.WriteLine("Initial gain index = " + Link.Mykonos.getObsRxGain());
                    Link.Mykonos.setObsRxManualGain(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A, gainIndex);
                    //if (gainIndex <= 237)
                    //{
                    //    Link.spiWrite(0x516, 0xC0);
                    //    Link.spiWrite(0x508, 0x10);
                    //}
                    Console.WriteLine("Set gain index = " + gainIndex);
                    Console.WriteLine("Register 0x508: " + Link.spiRead(0x508).ToString("X"));
                    Console.WriteLine("Register 0x514: " + Link.spiRead(0x514).ToString("X"));
                    Console.WriteLine("Register 0x516: " + Link.spiRead(0x516).ToString("X"));
                    System.Threading.Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
                finally
                {
                    Link.Disconnect();
                }


                rxDataArray = Helper.MykonosOrxCapture(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A, 8192);
                byte sampleBitWidth = 16;
                double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                amplitudeData[i, 0] = (double)gainIndex;
                amplitudeData[i, 1] = analysisData.FundamentalPower_dBFS;
                Console.WriteLine("Fundamental Amplitude: " + analysisData.FundamentalPower_dBFS.ToString());
                if (i == 0)
                {
                    amplitudeData[i, 2] = analysisData.FundamentalPower_dBFS;
                }
                else
                {
                    amplitudeData[i, 2] = amplitudeData[i - 1, 2] - 1;
                    amplitudeDiffData[i - 1, 0] = (double)gainIndex;
                    amplitudeDiffData[i - 1, 1] = amplitudeData[i - 1, 1] - amplitudeData[i, 1];
                }

            }
#if false //Graphing Error - To be fixed
            string path = SnRxRfTest.ResPath + "SnRxGainSweep";
            if (channel == Mykonos.SNIFFER_CHANNEL.SNIFFER_A)
                path = path + "SnA";
            else if(channel == Mykonos.SNIFFER_CHANNEL.SNIFFER_B)
                path = path + "SnB";
            else
                path = path + "SnC";
            var doc1 = new Document();
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
            string[] timeLabels = new string[] { "Rx Gain Sweep versus Amplitude for " + channel.ToString(),
                                                "Gain Index (byte)",
                                                "Amplitude (dBFS)",
                                                "Amplitude: " + amplitude_dBm + "dBm",
                                                "Perfect 1dB Gain Index Steps"
                                                 };
            string[] timeLabels2 = new string[] { "Difference between consecutive gain entries " + channel.ToString(),
                                                "Gain Index",
                                                "Amplitude delta (dB, comparing A(n + 1) - A(n))",
                                                "Amplitude: " + amplitude_dBm + "dBm"
                                                 };
            string[] pcbInfo = Helper.PcbInfo();

            container[0] = Helper.MakeChartObject(amplitudeData, timeLabels, path);
            container[1] = Helper.MakeChartObject(amplitudeDiffData, timeLabels2, path + "2");

            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);
            //Console.WriteLine(pcbInfo);
            //Open Result PDF
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
        }



    }
}
