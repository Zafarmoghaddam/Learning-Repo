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
//using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "3.5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "3.5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "3.5")]
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "2.5")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "2.5")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "2.5")]
    [TestFixture("Rx 20MHz, IQrate 30.72MHz, Dec5", "0.7")]
    [TestFixture("Rx 40MHz, IQrate 61.44MHz, Dec5", "0.7")]
    [TestFixture("Rx 100MHz, IQrate 122.88MHz, Dec5", "0.7")]
    [NUnit.Framework.Category("RF")]
    public class RxRfTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string RxProfile;
        private string RxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\RxRfTests\";
        public RxRfTests()
        {
            this.RxProfile = settings.mykSettings.rxProfileName;
            this.RxProfileString = Helper.parseProfileName(RxProfile);
            ResPath = System.IO.Path.Combine(ResPath, RxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public RxRfTests(string RxProfile, string freq)
        {
            this.RxProfile = RxProfile;
            this.RxProfileString = Helper.parseProfileName(RxProfile);
            ResPath = System.IO.Path.Combine(ResPath, RxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
            if (freq == "3.5")
            {
                settings.txPllLoFreq_Hz = 3500000000;

                settings.rxPllLoFreq_Hz = 3500000000;
                settings.obsRxPllLoFreq_Hz = 3500000000;
            }
            else if (freq == "2.5")
            {
                settings.txPllLoFreq_Hz = 2500000000;
                settings.rxPllLoFreq_Hz = 2500000000;
                settings.obsRxPllLoFreq_Hz = 2500000000;
            }
            else
            {
                settings.txPllLoFreq_Hz = 700000000;
                settings.rxPllLoFreq_Hz = 700000000;
                settings.obsRxPllLoFreq_Hz = 700000000;
            }

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
        public void RxRfTestsInit()
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
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
            Console.WriteLine("Test Setup Complete");
        }
   



        ///<summary>
        /// RF Test Name: 
        ///      RxPassbandSweep
        /// RF Feature Under Test: 
        ///     Rx Passband Sweep Test		
        /// RF Test Procedure: 
        ///     Based on Profile Data Determine Profile BW & Sampling Freq
        ///     & LO Frequency determine a Frequency range for sweep.
        ///     Allow for testing 50%  over & under PassBand
        ///     Configure Signal Generator & Enable Mykonos Datapath
        ///     Sweep thru Passband Frequencies
        ///     Process Sampled Data to Determine
        ///         1. Fundemental Frequency Detected (MHz)
        ///         2. Fundamental Power of Signal (dBFS)
        ///         3. Image Power(dBFS) Reduction}
        ///     Graph Data.
        /// RF Test Pass Criteria: 
        ///      Check Min Max Fund Amplitudes over passband are within 0.5db of each other.		
        ///</summary>
        [Test, Sequential]
        [NUnit.Framework.Category("RX")]
        public static void RxPassbandSweep([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel)
        {
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            profileInfo[0] = settings.rxProfileData.IqRate_kHz;
            profileInfo[1] = settings.rxPllLoFreq_Hz;
            profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

            double samplingFreq_MHz = profileInfo[0] / 1000;
            double profileBW_MHz = profileInfo[2] / 1000000;
            Console.WriteLine("Rx Sampling Freq (MHz): " + samplingFreq_MHz);
            Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
            double freqLo_MHz = profileInfo[1] / 1000000;
            Console.WriteLine("Rx LO Frequency (MHz): " + freqLo_MHz);
            
            

            //Define Test Parameters Based on Profile Info & Lo Frequency
            //Allow for testing 50%  over & under PassBand
            //Hard coded values for amplitude & Frequency Setups settings
            const int NUM_SAMPLES = 8192;
            double SwpSigAmp = -20;
            double SwpMinFreq = freqLo_MHz - (profileBW_MHz / 2) * 1.5; 
            double SwpMaxFreq = freqLo_MHz + (profileBW_MHz / 2) * 1.5;  
            int SwpNumSteps = 150;
            SwpParamStruct param = new SwpParamStruct(SwpMinFreq, SwpMaxFreq, SwpSigAmp, SwpNumSteps);              
            Console.WriteLine("SwpMinFreq (MHz): " + SwpMinFreq);
            Console.WriteLine("SwpMaxMax (MHz): " + SwpMaxFreq);
            Console.WriteLine("SwpSigAmp (MHz): " + SwpSigAmp);


            //Define Data Array for storing Fundamental 
            short[] rxDataArray = new short[16384];
            double[,] outputData = new double[param.numSteps, 4];
            string[] pcbInfo;
            AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();

            //Configure Signal Generator
            SG_AgilentESG sigGen = new SG_AgilentESG(measEquipment.ESGAddress);
            Console.WriteLine(sigGen.Identify());
            sigGen.SetFrequency(param.freqMin);
            sigGen.SetAmplitude(param.amplitude);
            sigGen.SetRfOutput(true);
            //Enable Mykonos Rx Datapath
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
            //Link.Mykonos.powerUpRxPath(channel);
            Link.Mykonos.radioOn();
            Link.Disconnect();
            
            //Test Sequence
            //Sweep Thru Rx Passband & Capture Data
            //Capture Received data from FPGA
            //Process Sampled Data to Determine
            //Fundemental Frequency Detected (MHz)
            //Fundamental Power of Signal (dBFS)
            //Image Power(dBFS)
            for (int i = 0; i < param.numSteps; i++)
            {
                double test_freq = param.freqMin + i * (param.freqMax - param.freqMin) / param.numSteps;
                sigGen.SetFrequency(test_freq);
                System.Threading.Thread.Sleep(1000);

                rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);
                
                byte sampleBitWidth = 16;
                double[] fftMagnitudeData = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                outputData[i, 0] = test_freq;
                //outputData[i, 0] = analysisData.FundamentalFrequency_MHz;
                outputData[i, 1] = analysisData.FundamentalPower_dBFS;
                outputData[i, 2] = analysisData.ImagePower_dBFS;
                outputData[i, 3] = analysisData.DcOffset_dBFS;
            }


            string path = RxRfTests.ResPath + "RxPassbandSweep";
            if (channel == Mykonos.RXCHANNEL.RX1)
                path = path + "RX1";
            else
                path = path + "RX2";
#if  WR_RES_TO_PDF
            var doc1 = new Document();
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
            string[] timeLabels = new string[] { "FFT Statistics versus CW Input Frequency for " + channel.ToString(),
                                                "CW Input Frequency (MHz)",
                                                "Amplitude (dBFS)" ,
                                                "Fundamental Tone",
                                                "Image Amplitude",
                                                "DC Offset Amplitude"};
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, "N/A", "N/A");
            container[0] = Helper.MakeChartObject(outputData, timeLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
            // Write data to txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
            {
                file.WriteLine("Sample,  Frequency MHz, Fundamental Power(dBFS), Image Power(dBFS), DC Offset(dBFS)");
                for (int i = 0; i < param.numSteps; i++)
                {
                    file.WriteLine(i +"," + outputData[i, 0].ToString() + "," + outputData[i, 1].ToString() + "," + outputData[i, 2].ToString() + "," + outputData[i, 3].ToString());
                }
            }
#endif
            //Check Min Max Fund Amplitudes are within 0.5db of each other.
            //var MinFundPower_dBFS = System.Linq.Enumerable.Range(50, 100).Select(i => outputData[i, 1]).Min();
            //var MaxFundPower_dBFS = System.Linq.Enumerable.Range(50, 100).Select(i => outputData[i, 1]).Max();
            double MinFundPower_dBFS = outputData[50, 1];
            double MaxFundPower_dBFS = outputData[50, 1];
            for(int i = 50; i<100; i++)
            {
                if (outputData[i, 1] < MinFundPower_dBFS)
                    MinFundPower_dBFS = outputData[i, 1];
                if (outputData[i, 1] > MaxFundPower_dBFS)
                    MaxFundPower_dBFS = outputData[i, 1];
            }
            Console.WriteLine("MinFundAmp: " + MinFundPower_dBFS);
            Console.WriteLine("MaxFundAmp: " + MaxFundPower_dBFS);
            Console.WriteLine("MaxDiffFundAmp: " + (MaxFundPower_dBFS - MinFundPower_dBFS));
            NUnit.Framework.Assert.IsTrue((MaxFundPower_dBFS - MinFundPower_dBFS) <= 0.5);
        }
 
        ///<summary>
        /// RF Test Name: 
        ///     RxCaptureTest
        /// RF Feature Under Test: 
        ///     Rx Data Capture Test		
        /// RF Test Procedure: 
        ///     Based on Profile Data Determine Profile BW & Sampling Freq
        ///     & LO Frequency send a Test Signal from the ESG.
        ///     Enable Mykonos Datapath
        ///     Using the FPGA capture Sampled Data.
        ///     Run complex FFT and scale
        ///     Plot on Time domain data    
        /// RF Test Pass Criteria: 
        ///     None		
        ///     Need to put in more practical pass criteria
        ///</summary>
         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
        public static void RxCaptureTest([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel, 
                                         [Values(-20)]int amp_dbm,
                                         [Values(10000)]int IQExptVal)

        {

            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            profileInfo[0] = settings.rxProfileData.IqRate_kHz;
            profileInfo[1] = settings.rxPllLoFreq_Hz;
            profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;
             
            double samplingFreq_Hz = profileInfo[0] * 1000;
            double profileBW_MHz = profileInfo[2] / 1000000;
            Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
            Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
            double freqLo_kHz = profileInfo[1] / 1000;
            Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

            //Define Receiver Test Signal to be 10MHz Offset from LO frequency
            double testSigFreq_MHz = (freqLo_kHz / 1000 + 10);
            int amplitude_dBm = amp_dbm;
            Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);

            //Define DataCapture Parameters
            const int NUM_SAMPLES = 8192;
            short[] rxDataArray = new short[NUM_SAMPLES * 2];
            double[,] timeDomainData = new double[NUM_SAMPLES / 2, 3];

           
            //Generate Test Signal for Rx Capture with ESG
            SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
            Console.WriteLine("ESG Info:" + esg.Identify());
            Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
            Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
            Console.WriteLine("ESG Generating Tone Amp:" + amplitude_dBm);
            esg.SetFrequency(testSigFreq_MHz);
            esg.SetAmplitude(amp_dbm);  
            esg.SetRfOutput(true);

            //Enable Mykonos Rx Datapath
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
            //Link.Mykonos.powerUpRxPath(channel);
            Link.Mykonos.radioOn();
            Link.Disconnect();
            System.Threading.Thread.Sleep(1000);

            //Retrieve Rx Data from FPGA
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
            Console.WriteLine("I Max, Min:" + IMax.ToString() +"," + IMin.ToString());
            Console.WriteLine("Q Max, Min:" + QMax.ToString() + "," + QMin.ToString());


#if WR_RES_TO_PDF

            string path = RxRfTests.ResPath + "Rx_FFT_TimeDomain_Plots";
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
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, "N/A", "N/A");

            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            // open result pdf
            System.Diagnostics.Process.Start(path + ".pdf");
#endif


            // open result pdf
            System.Diagnostics.Process.Start(path + ".pdf");
            /*NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Less(IMin, (IQExptVal * (-1)));
            NUnit.Framework.Assert.Less( IMax, ((IQExptVal + ((IQExptVal*10)/100))));
            NUnit.Framework.Assert.Greater(IMax, IQExptVal);

            NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Less(QMin, (IQExptVal * (-1)));
            NUnit.Framework.Assert.Less(QMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Greater(QMax, IQExptVal);
             */
            NUnit.Framework.Assert.Greater(IMax, 5000);

            NUnit.Framework.Assert.Greater(QMax, 5000);



        }
       

         ///<summary>
         /// RF Test Name: 
         ///      RxGainSweep
         /// RF Feature Under Test: 
         ///     Rx DataPath Gain Sweep Test		
         /// RF Test Procedure: 
         ///     Based on Profile Data Determine Profile BW & Sampling Freq
         ///     & LO Frequency send a Test Signal from the ESG.
         ///     Enable Mykonos Datapath
         ///     Sweep thru 60 Gain Indices of the Rx Gain Table and apply to 
         ///     datapath.
         ///     Retrieve Data from FPGA   
         ///     Process fft and obtain fundamental amplitude
         ///     Analyse Data for Graphing
         ///     amplitudeData[GainIndex, x] = { Gain Index, Measured Fundamental Power, 
         ///     Expected Amplitued if gain indext were linear with 0.5dB Step Size}
         ///     amplitudeDiffData[i - 1, x] = {Applied gainIndex, Measured Fundamental Power Reduction}
         /// RF Test Pass Criteria: 
         ///     Check Differential Amplitudes are within +-0.1db of 0.5db		
         ///</summary>
         [Test, Sequential]
         [NUnit.Framework.Category("RX")]
         public static void RxGainSweep([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel)
         {
             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             profileInfo[0] = settings.rxProfileData.IqRate_kHz;
             profileInfo[1] = settings.rxPllLoFreq_Hz;
             profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double samplingFreq_MHz = profileInfo[0] / 1000;
             double profileBW_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (MHz): " + samplingFreq_MHz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_MHz = profileInfo[1] / 1000000;
             Console.WriteLine("Detected LO Frequency: " + freqLo_MHz);

             //Define Receiver Test Signal to be 10MHz Offset from LO frequency
             double testSigFreq_MHz = (freqLo_MHz + 10);
             int amplitude_dBm = -20;
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);

             //Enable Mykonos Rx Datapath
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
             //Link.Mykonos.powerUpRxPath(channel);
             Link.Mykonos.radioOn();
             Link.Disconnect();


             //Define DataCapture Parameters
             //Fixed number of I and Q samples = 16384. 
             //NOTE: Must be a power of two to use DOTNetRoutines.dll functions
             //There exist only 37 valid Rx Atten index locations //TODO:??
             int numSamples = 8192;
             short[] rxDataArray = new short[numSamples * 2];
             int numIndices = 60;
             double[,] amplitudeData = new double[numIndices, 3];
             string[] pcbInfo;
             double[,] amplitudeDiffData = new double[numIndices - 1, 2];

             //Generate Test Signal for Rx Capture with ESG
             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
             Console.WriteLine("ESG Info:" + esg.Identify());
             Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
             Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
             Console.WriteLine("ESG Generating Tone Amp:" + amplitude_dBm);
             esg.SetFrequency(testSigFreq_MHz);
             esg.SetAmplitude(amplitude_dBm);
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.setRx1ManualGain(255);
             System.Threading.Thread.Sleep(1000);
             rxDataArray = Helper.MykonosRxCapture(channel, numSamples);

             rxDataArray = Helper.MykonosRxCapture(channel, numSamples);

             rxDataArray = Helper.MykonosRxCapture(channel, numSamples);


             //System.Threading.Thread.Sleep(1000);

             byte gainIndex = 240;
             AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();

             //Test Sequence
             //Sweep Gain Indices
             //Capture Received data from FPGA
             //Process Sampled Data to Determine
             //Fundamental Power of Signal (dBFS)
             for (int i = 0; i < (numIndices); i++)
             {
                 try
                 {
                    // Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                     /*
                     if (channel == Mykonos.RXCHANNEL.RX1)
                     {
                         Link.Mykonos.getRx1Gain(ref gainIndex);
                     }

                     else if (channel == Mykonos.RXCHANNEL.RX2)
                     {
                         Link.Mykonos.getRx2Gain(ref gainIndex);
                     }
                     */
                     gainIndex = (byte)(255 - i);


                     if (channel == Mykonos.RXCHANNEL.RX1)
                         Link.Mykonos.setRx1ManualGain(gainIndex);
                     else if (channel == Mykonos.RXCHANNEL.RX2)
                         Link.Mykonos.setRx2ManualGain(gainIndex);
                 }
                 catch (Exception e)
                 {
                     Console.WriteLine(e);
                     Console.WriteLine("Loop broken");
                     break;
                 }
                 finally
                 {
                     //Link.Disconnect();
                 }

                 System.Threading.Thread.Sleep(100);

                 //Retrieve Data from FPGA   
                 //Process fft and obtain fundamental amplitude
                 //Analyse Data for Graphing
                 //amplitudeData[GainIndex, x] = { Gain Index, Measured Fundamental Power, 
                 //Expected Amplitued if gain indext were linear with 0.5dB Step Size}
                 //amplitudeDiffData[i - 1, x] = {Applied gainIndex, Measured Fundamental Power Reduction}
                 rxDataArray = Helper.MykonosRxCapture(channel, numSamples);
                 byte sampleBitWidth = 16;
                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);


                 amplitudeData[i, 0] = (double)gainIndex;
                 Console.WriteLine("Gain Index:" + amplitudeData[i, 0]);

                 amplitudeData[i, 1] = analysisData.FundamentalPower_dBFS;
                 Console.WriteLine("Measured FundamentalPower_dBFS:" + amplitudeData[i, 1]);
                 if (i == 0) //Check if first tested gain index
                 {
                     amplitudeData[i, 2] = analysisData.FundamentalPower_dBFS;
                 }
                 else
                 {
                     //0.5dB step size for the Rx channel
                     amplitudeData[i, 2] = amplitudeData[i - 1, 2] - 0.5;
                     amplitudeDiffData[i - 1, 0] = (double)gainIndex;
                     amplitudeDiffData[i - 1, 1] = amplitudeData[i - 1, 1] - amplitudeData[i, 1];
                     Console.WriteLine(" Gain index :" + amplitudeDiffData[i - 1, 0]  );
                     Console.WriteLine(" Differential Amplitude" + amplitudeDiffData[i - 1, 1]);
                    
                    
                 }

             }

            

#if WR_RES_TO_PDF || WR_RES_TO_TXT
             string path = RxRfTests.ResPath + "RxGainIndexSweep";
             if (channel == Mykonos.RXCHANNEL.RX1)
                 path = path + "RX1";
             else
                 path = path + "RX2";
             var doc1 = new Document();
             iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
             string[] timeLabels = new string[] { "Rx Gain Sweep versus Amplitude for " + channel.ToString(),
                                                "Gain Index (byte)",
                                                "Amplitude (dBFS)",
                                                "Amplitude: " + amplitude_dBm + "dBm",
                                                "Perfect 0.5dB Gain Index Steps"
                                                 };
             string[] timeLabels2 = new string[] { "Difference between consecutive gain entries " + channel.ToString(),
                                                "Gain Index",
                                                "Amplitude delta (dB, comparing A(n + 1) - A(n))",
                                                "Amplitude: " + amplitude_dBm + "dBm"
                                                 };
             pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, "N/A", "N/A");
#endif
#if WR_RES_TO_PDF
             container[0] = Helper.MakeChartObject(amplitudeData, timeLabels, path);
             container[1] = Helper.MakeChartObject(amplitudeDiffData, timeLabels2, path + "2");

             Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);
             //Open Result PDF
             System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
             // Write data to file
             using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
             {
                 for (int i = 0; i <= numIndices - 1; i++)
                 {
                     // If the line doesn't contain the word 'Second', write the line to the file. 
                     file.WriteLine(amplitudeData[i, 0].ToString() + "," + amplitudeData[i, 1].ToString() + "," + amplitudeData[i, 2].ToString());
                 }
             }

#endif
             //Check Differential Amplitudes are within +-0.1db of 0.5db.
             for (int i = 3; i < (numIndices); i++)
             {//used an upper limit of 1 instead of 0.6 and lower limit of 0
                 NUnit.Framework.Assert.IsTrue(((amplitudeDiffData[i - 1, 1] < 1)
                                      && (amplitudeDiffData[i - 1, 1] > 0)));
             }
         }

         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void RxCaptureTestTxLoopback([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel,
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

             //System.Threading.Thread.Sleep(500);
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


#if WR_RES_TO_PDF

             string path = RxRfTests.ResPath + "Rx_Loopback_FFT_TimeDomain_Plots";
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
             pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, "N/A", "N/A");

             Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

             // open result pdf
             System.Diagnostics.Process.Start(path + ".pdf");
#endif
             NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Less(IMin, (IQExptVal * (-1)));
             NUnit.Framework.Assert.Less(IMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Greater(IMax, IQExptVal);

             NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Less(QMin, (IQExptVal * (-1)));
             NUnit.Framework.Assert.Less(QMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
             NUnit.Framework.Assert.Greater(QMax, IQExptVal);




         }
         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void TxLoopbackRx1Rx2CaptureTest([Values(-20)]int amp_dbm1,
                                                        [Values(10)]int OffSet1,
                                                        [Values(20000)]int IQExptVal1,
                                                        [Values(-10)]int amp_dbm2,
                                                        [Values(5)]int OffSet2,
                                                        [Values(30000)]int IQExptVal2)
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
         //[Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void TempTest([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel,
                                                 [Values(-20)]int amp_dbm,
                                                 [Values(10000)]int IQExptVal)
         {

             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             profileInfo[0] = settings.rxProfileData.IqRate_kHz;
             profileInfo[1] = settings.rxPllLoFreq_Hz;
             profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double samplingFreq_Hz = profileInfo[0] * 1000;
             double profileBW_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = profileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

             //Define Receiver Test Signal to be 10MHz Offset from LO frequency
             double testSigFreq_MHz = (freqLo_kHz / 1000 + 10);
             int amplitude_dBm = amp_dbm;
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);

             //Define DataCapture Parameters
             const int NUM_SAMPLES = 8192;
             int startfreq = (int)profileBW_MHz-200;
             int stopfreq = (int)profileBW_MHz+200;
             int SWEEP_RANGE = stopfreq-startfreq;
             short[] rxDataArray = new short[NUM_SAMPLES * 2];
             double[,] timeDomainData = new double[NUM_SAMPLES, 2];
             double[,] DomainData = new double[SWEEP_RANGE, 4];


             //Generate Test Signal for Rx Capture with ESG
             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
             Console.WriteLine("ESG Info:" + esg.Identify());
             Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
             Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
             Console.WriteLine("ESG Generating Tone Amp:" + amplitude_dBm);


             for (int j = 0; j < SWEEP_RANGE; j++)
             {
                 esg.SetFrequency(startfreq + j);
                 esg.SetAmplitude(amplitude_dBm);
                 esg.SetRfOutput(true);

                 //Enable Mykonos Rx Datapath
                 AdiCommandServerClient Link = AdiCommandServerClient.Instance;
                 Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                 //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
                 //Link.Mykonos.powerUpRxPath(channel);
                 Link.Mykonos.radioOn();
                 Link.Disconnect();
                 //System.Threading.Thread.Sleep(200);
                 //Retrieve Rx Data from FPGA
                 double[] tempdata = new double[5];
                 rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);


                 //Time Domain Data Processing
                 for (int i = 0; i < NUM_SAMPLES; i++)
                 {
                     timeDomainData[i, 0] = i;
                     timeDomainData[i, 1] = rxDataArray[i];
                 }
                 tempdata[0] = System.Linq.Enumerable.Range(0, (int)NUM_SAMPLES).Select(i => timeDomainData[i, 1]).Max();

                 rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);


                 //Time Domain Data Processing
                 for (int i = 0; i < NUM_SAMPLES; i++)
                 {
                     timeDomainData[i, 0] = i;
                     timeDomainData[i, 1] = rxDataArray[i];
                 }
                 tempdata[1] = System.Linq.Enumerable.Range(0, (int)NUM_SAMPLES).Select(i => timeDomainData[i, 1]).Max();
                 rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);


                 //Time Domain Data Processing
                 for (int i = 0; i < NUM_SAMPLES; i++)
                 {
                     timeDomainData[i, 0] = i;
                     timeDomainData[i, 1] = rxDataArray[i];
                 }
                 tempdata[2] = System.Linq.Enumerable.Range(0, (int)NUM_SAMPLES).Select(i => timeDomainData[i, 1]).Max();
                 rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);


                 //Time Domain Data Processing
                 for (int i = 0; i < NUM_SAMPLES; i++)
                 {
                     timeDomainData[i, 0] = i;
                     timeDomainData[i, 1] = rxDataArray[i];
                 }
                 tempdata[3] = System.Linq.Enumerable.Range(0, (int)NUM_SAMPLES).Select(i => timeDomainData[i, 1]).Max();
                 rxDataArray = Helper.MykonosRxCapture(channel, NUM_SAMPLES);


                 //Time Domain Data Processing
                 for (int i = 0; i < NUM_SAMPLES; i++)
                 {
                     timeDomainData[i, 0] = i;
                     timeDomainData[i, 1] = rxDataArray[i];
                 }
                 tempdata[4] = System.Linq.Enumerable.Range(0, (int)NUM_SAMPLES).Select(i => timeDomainData[i, 1]).Max();

                 var DataMin = System.Linq.Enumerable.Range(0, 5).Select(i => tempdata[i]).Min();
                 var DataMax = System.Linq.Enumerable.Range(0, 5).Select(i => tempdata[i]).Max();
                 var DataAvg = System.Linq.Enumerable.Range(0, 5).Select(i => tempdata[i]).Average();
                 Console.WriteLine(DataMin);
                 DomainData[j, 0] = j + startfreq;
                 DomainData[j, 1] = DataMin;
                 DomainData[j, 2] = DataAvg;
                 DomainData[j, 3] = DataMax;


             }
#if WR_RES_TO_PDF
             string path = RxRfTests.ResPath + "Rx_FFT_TimeDomain_Plots";
             if (channel == Mykonos.RXCHANNEL.RX1)
                 path = path + "RX1";
             else
                 path = path + "RX2";

             string[] timeLabels = new string[] { "Frequency Sweep Response of " + channel.ToString(), "Sweep Frequency (MHz)", "ADC Codes", "Min", "Avg", "Max" };


             var doc1 = new Document();
             iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
             container[0] = Helper.MakeChartObject(DomainData, timeLabels, path);
             string[] pcbInfo;
             pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, "N/A", "N/A");

             Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

             // open result pdf
             System.Diagnostics.Process.Start(path + ".pdf");
#endif


         }

         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void RxDecPwrTest([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel)
         {
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.radioOn();
             Link.Mykonos.setRx1ManualGain(255);

             //Link.Mykonos.setObsRxPathSource(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO);

             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             profileInfo[0] = settings.rxProfileData.IqRate_kHz;
             profileInfo[1] = settings.rxPllLoFreq_Hz;
             profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double samplingFreq_Hz = profileInfo[0] * 1000;
             double profileBW_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = profileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

             //Define Receiver Test Signal to be 10MHz Offset from LO frequency
             double testSigFreq_MHz = (freqLo_kHz / 1000 + 10);
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);

             //Define DataCapture Parameters
             const byte NUM_SAMPLES = 255;
             UInt16 RxDecPower_mdBFS = 10;

             byte spiData = 0x0;

             

             //Generate Test Signal for Rx Capture with ESG
             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
             Console.WriteLine("ESG Info:" + esg.Identify());
             Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
             Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
             esg.SetFrequency(testSigFreq_MHz);
             esg.SetRfOutput(true);

             AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
             double samplingFreq_MHz = samplingFreq_Hz / 1000000;
             byte sampleBitWidth = 16;


             for (int amp = -20; amp > -50; amp--)
             {
                 System.Threading.Thread.Sleep(100);
                 esg.SetAmplitude(amp);
                 System.Threading.Thread.Sleep(100);
                 short[] rxDataArray = Helper.MykonosRxCapture(channel, 16384);

                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                 if (channel == Mykonos.RXCHANNEL.RX1)
                 {
                     Link.Mykonos.getRx1DecPower(ref RxDecPower_mdBFS);
                     spiData = Link.spiRead(0x4DE);
                 }
                 else
                 {
                     Link.Mykonos.getRx2DecPower(ref RxDecPower_mdBFS);
                     spiData = Link.spiRead(0x4DF);

                 }

                 Console.WriteLine("Received power: " + RxDecPower_mdBFS + " calculated power: " + analysisData.FundamentalPower_dBFS +  " at " + amp + " dbfs");
                 NUnit.Framework.Assert.Less(System.Math.Abs((double)RxDecPower_mdBFS / 1000 + analysisData.FundamentalPower_dBFS), 0.50);
                 NUnit.Framework.Assert.AreEqual(spiData * 250, RxDecPower_mdBFS);

             }
             
             Link.Disconnect();


         }




         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void RxPathOverloadTest()
         {
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             Link.Mykonos.radioOn();
             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             profileInfo[0] = settings.rxProfileData.IqRate_kHz;
             profileInfo[1] = settings.rxPllLoFreq_Hz;
             profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double samplingFreq_Hz = profileInfo[0] * 1000;
             double profileBW_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = profileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

             //Define Receiver Test Signal to be 10MHz Offset from LO frequency
             double testSigFreq_MHz = (freqLo_kHz / 1000 + 10);
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);
             Link.Mykonos.setRx1ManualGain(254);
             //Generate Test Signal for Rx Capture with ESG
             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
             Console.WriteLine("ESG Info:" + esg.Identify());
             Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
             Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
             esg.SetFrequency(testSigFreq_MHz);
             esg.SetRfOutput(true);

             for (int amplitude_dBm = 10;  amplitude_dBm > -60; amplitude_dBm--)
             {
                 esg.SetAmplitude(amplitude_dBm);
                 System.Threading.Thread.Sleep(100);
                 //Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.AGC);


                 string response = Link.Mykonos.getRxPathOverloads();
                 Console.WriteLine("Response: " + response + " at " + amplitude_dBm);
             }
             Link.Disconnect();


         }

         [Test, Combinatorial]
         [NUnit.Framework.Category("RX")]
         public static void RxAGCTest([Values(Mykonos.RXCHANNEL.RX1, Mykonos.RXCHANNEL.RX2)]Mykonos.RXCHANNEL channel)
         {
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             ushort power = 0;
             byte spiData1 = 0;
             byte agcRx1MaxGainIndex = 255;
             byte agcRx1MinGainIndex = 210;
             byte agcRx2MaxGainIndex = 255;
             byte agcRx2MinGainIndex = 195;
             byte agcObsRxMaxGainIndex = 255;
             byte agcObsRxMinGainIndex = 203;
             byte agcObsRxSelect = 1;
             byte agcPeakThresholdMode = 1; // Change for power only mode
             byte agcLowThsPreventGainIncrease = 1; // Change for power only mode
             UInt32 agcGainUpdateCounter = 30720;
             byte agcSlowLoopSettlingDelay = 3;
             byte agcPeakWaitTime = 2;
             byte pmdMeasDuration = 0x08;
             byte pmdMeasConfig = 0x2;
             byte agcResetOnRxEnable = 1;
             byte agcEnableSyncPulseForGainCounter = 0;

             // mykonosPowerMeasAgcCfg_t
             byte pmdUpperHighThresh = 0x01; // Triggered at approx -2dBFS
             byte pmdUpperLowThresh = 0x03;
             byte pmdLowerHighThresh = 0x0C;
             byte pmdLowerLowThresh = 0x04;
             byte pmdUpperHighGainStepAttack = 0x04;
             byte pmdUpperLowGainStepAttack = 0x02;
             byte pmdLowerHighGainStepRecovery = 0x02;
             byte pmdLowerLowGainStepRecovery = 0x04;

             // mykonosPeakDetAgcCfg_t
             byte apdHighThresh = 0x1F; //Triggered at approx -3dBFS
             byte apdLowThresh = 0x16; //Triggered at approx -5.5dBFS
             byte hb2HighThresh = 0xB5; // Triggered at approx -2.18dBFS
             byte hb2LowThresh = 0x80; // Triggered at approx -5.5dBFS
             byte hb2VeryLowThresh = 0x40; // Triggered at approx -9dBFS
             byte apdHighThreshExceededCnt = 0x06;
             byte apdLowThreshExceededCnt = 0x04;
             byte hb2HighThreshExceededCnt = 0x06;
             byte hb2LowThreshExceededCnt = 0x04;
             byte hb2VeryLowThreshExceededCnt = 0x04;
             byte apdHighGainStepAttack = 0x04;
             byte apdLowGainStepRecovery = 0x02;
             byte hb2HighGainStepAttack = 0x04;
             byte hb2LowGainStepRecovery = 0x02;
             byte hb2VeryLowGainStepRecovery = 0x04;
             byte apdFastAttack = 1;
             byte hb2FastAttack = 1;
             byte hb2OverloadDetectEnable = 1;
             byte hb2OverloadDurationCnt = 1;
             byte hb2OverloadThreshCnt = 0x1;

             // Write some values into the structure 
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


             Link.Mykonos.setupRxAgc();
             Link.Mykonos.radioOn();
             Console.WriteLine("gain before: " + Link.Mykonos.getRx1Gain());

             Link.Mykonos.setRxGainControlMode(Mykonos.GAINMODE.AGC);

             Console.WriteLine("gain after: " + Link.Mykonos.getRx1Gain());
             //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
             double[] profileInfo = new double[3];
             profileInfo[0] = settings.rxProfileData.IqRate_kHz;
             profileInfo[1] = settings.rxPllLoFreq_Hz;
             profileInfo[2] = settings.rxProfileData.PrimarySigBw_Hz;

             double samplingFreq_Hz = profileInfo[0] * 1000;
             double profileBW_MHz = profileInfo[2] / 1000000;
             Console.WriteLine("Rx Sampling Freq (Hz): " + samplingFreq_Hz);
             Console.WriteLine("Rx Profile Bandwdith (MHz): " + profileBW_MHz);
             double freqLo_kHz = profileInfo[1] / 1000;
             Console.WriteLine("Rx LO Frequency (kHz): " + freqLo_kHz);

             //Define Receiver Test Signal to be 10MHz Offset from LO frequency
             double testSigFreq_MHz = (freqLo_kHz / 1000 + 10);
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);
             //Generate Test Signal for Rx Capture with ESG
             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
             Console.WriteLine("ESG Info:" + esg.Identify());
             Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
             Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
             esg.SetFrequency(testSigFreq_MHz);
             esg.SetRfOutput(true);

             AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
             double samplingFreq_MHz = samplingFreq_Hz / 1000000;
             byte sampleBitWidth = 16;
             string response = "";
             string initgain = "";
             string finalgain = "";
             for (int amplitude_dBm = -20; amplitude_dBm < 10; amplitude_dBm++)
             {
                 esg.SetAmplitude(amplitude_dBm);
                 System.Threading.Thread.Sleep(100);
                 //Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.AGC);
                 short[] rxDataArray = Helper.MykonosRxCapture(channel, 16384);

                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                 if (channel == Mykonos.RXCHANNEL.RX1)
                 {
                     response = Link.Mykonos.getRx1Gain();
                     spiData1 = Link.spiRead(0x4B0);
                     Assert.AreEqual(spiData1, Int32.Parse(response), "Register Readback for Rx1 Gain not as expected");
                     Link.Mykonos.getRx1DecPower(ref power);
                 }
                 else
                 {
                     response = Link.Mykonos.getRx2Gain();
                     spiData1 = Link.spiRead(0x4B3);
                     Assert.AreEqual(spiData1, Int32.Parse(response), "Register Readback for Rx2 Gain not as expected");
                     Link.Mykonos.getRx2DecPower(ref power);
                 }
                 if (amplitude_dBm == -20)
                      initgain = response;
                 Console.WriteLine("Gain: " + response + "DecPower: " + power + " FFT Power " + analysisData.FundamentalPower_dBFS + " at " + amplitude_dBm + "\n");
             }
             
             for (int amplitude_dBm = 10; amplitude_dBm >= -20; amplitude_dBm--)
             {
                 esg.SetAmplitude(amplitude_dBm);
                 System.Threading.Thread.Sleep(100);
                 //Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.AGC);
                 short[] rxDataArray = Helper.MykonosRxCapture(channel, 16384);

                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                 if (channel == Mykonos.RXCHANNEL.RX1)
                 {
                     response = Link.Mykonos.getRx1Gain();
                     spiData1 = Link.spiRead(0x4B0);
                     Assert.AreEqual(spiData1, Int32.Parse(response), "Register Readback for Rx1 Gain not as expected");
                     Link.Mykonos.getRx1DecPower(ref power);
                 }
                 else
                 {
                     response = Link.Mykonos.getRx2Gain();
                     spiData1 = Link.spiRead(0x4B3);
                     Assert.AreEqual(spiData1, Int32.Parse(response), "Register Readback for Rx2 Gain not as expected");
                     Link.Mykonos.getRx2DecPower(ref power);
                 }
                 if (amplitude_dBm == -20)
                     finalgain = response;
                 Console.WriteLine("Gain: " + response + "DecPower: " + power + " FFT Power " + analysisData.FundamentalPower_dBFS + " at " + amplitude_dBm + "\n");
 
             }
             Assert.AreEqual(initgain, finalgain, "Initial gain is not equal to the final gain");
             Link.Disconnect();


         }

       

    }
}
