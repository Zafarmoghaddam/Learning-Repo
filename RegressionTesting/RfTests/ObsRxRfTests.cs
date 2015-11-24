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
    /// RF Test Suite for Observer Rx Inputs
    /// </summary>
    [TestFixture("ORX 200MHz, IQrate 245.75MHz, Dec5", "0.7")]
    [TestFixture("ORX 100MHz, IQrate 122.88MHz, Dec5", "0.7")]
    [TestFixture("ORX 200MHz, IQrate 245.75MHz, Dec5", "2.5")]
    [TestFixture("ORX 100MHz, IQrate 122.88MHz, Dec5", "2.5")]
    [TestFixture("ORX 200MHz, IQrate 245.75MHz, Dec5", "3.5")]
    [TestFixture("ORX 100MHz, IQrate 122.88MHz, Dec5", "3.5")]
    [NUnit.Framework.Category("RF")]
    public class ObsRxRfTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string ObsRxProfile;
        private string ObsRxProfileString;
        private string RxProfile;
        private string RxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\ObsRxRfTests\";
        public ObsRxRfTests()
        {
            this.ObsRxProfile = settings.mykSettings.orxProfileName;
            this.ObsRxProfileString = Helper.parseProfileName(ObsRxProfile);
            ResPath = System.IO.Path.Combine(ResPath, ObsRxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public ObsRxRfTests(string ObsRxProfile, string freq)
        {
            this.ObsRxProfile = ObsRxProfile;
            this.ObsRxProfileString = Helper.parseProfileName(ObsRxProfile);
            this.RxProfile = "Rx 40MHz, IQrate 61.44MHz, Dec5";//cannot program with rx 100 MHz profile
            this.RxProfileString = Helper.parseProfileName("Rx 40MHz, IQrate 61.44MHz, Dec5");
            ResPath = System.IO.Path.Combine(ResPath, ObsRxProfileString);
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
        public void ObsRxRfTestsInit()
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
            //settings.mykSettings.orxProfileName = ObsRxProfile;
            //settings.mykSettings.rxProfileName = RxProfile;
            Console.WriteLine("obsrxprofile: " + settings.mykSettings.orxProfileName);
            Console.WriteLine("rxprofile: " + settings.mykSettings.rxProfileName);
            Console.WriteLine("txprofile: " + settings.mykSettings.txProfileName);

            //Call Test Setup 
            TestSetup.TestSetupInit(settings);
        }


        /// <summary>
        /// This code performs a CW frequency sweep from the ESG and measures the fundamental amplitude, image amplitude, 
        /// and DC offset magnitude on observation channels. . 
        /// This test is only for the observation systerm receivers (ORx1,2 and SNRXA, B, C)
        /// 
        /// Bug: Sniffer passband not working well with AdiMath.cs class. Need to investigate
        /// </summary>
        [Test, Sequential]
        [NUnit.Framework.Category("Orx")]
        public static void ORxPassbandSweep([Values(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO, Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO)]Mykonos.OBSRXCHANNEL channel)
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
                System.Threading.Thread.Sleep(100);

                rxDataArray = Helper.MykonosOrxCapture(channel, NUM_SAMPLES);

                byte sampleBitWidth = 16;
                double[] fftMagnitudeData = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                outputData[i, 0] = test_freq;
                //outputData[i, 0] = analysisData.FundamentalFrequency_MHz;
                outputData[i, 1] = analysisData.FundamentalPower_dBFS;
                outputData[i, 2] = analysisData.ImagePower_dBFS;
                outputData[i, 3] = analysisData.DcOffset_dBFS;
            }


            string path = RxRfTests.ResPath + "RxPassbandSweep";
            if (channel == Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO)
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
                    file.WriteLine(i + "," + outputData[i, 0].ToString() + "," + outputData[i, 1].ToString() + "," + outputData[i, 2].ToString() + "," + outputData[i, 3].ToString());
                }
            }
#endif
            //Check Min Max Fund Amplitudes are within 0.5db of each other.
            //var MinFundPower_dBFS = System.Linq.Enumerable.Range(50, 100).Select(i => outputData[i, 1]).Min();
            //var MaxFundPower_dBFS = System.Linq.Enumerable.Range(50, 100).Select(i => outputData[i, 1]).Max();
            double MinFundPower_dBFS = outputData[50, 1];
            double MaxFundPower_dBFS = outputData[50, 1];
            for (int i = 50; i < 100; i++)
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

        
    


        

        /// <summary>
        /// This is a single capture test of the observation system ports - ORx1, ORx2, SnRXA, SnRXB,SnRXC.. It returns the time domain data and
        /// complex fft of the IQ data in a pdf printout. Please initialize and enable ORx lanes prior to test execution
        /// 
        /// Very helpful for trying to determine if the received data makes sense.
        /// </summary>
         [Test]
         [NUnit.Framework.Category("Orx")]
        public static void OrxCaptureTest([Values(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO, Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO)]Mykonos.OBSRXCHANNEL channel, [Values(-20)]int amp_dbm,
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
            Link.Mykonos.setObsRxPathSource(channel);

            Link.Disconnect();
            System.Threading.Thread.Sleep(1000);

            //Retrieve Rx Data from FPGA
            rxDataArray = Helper.MykonosOrxCapture(channel, NUM_SAMPLES);

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

            string path = RxRfTests.ResPath + "Rx_FFT_TimeDomain_Plots";
            if (channel == Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO)
                path = path + "ORX1";
            else
                path = path + "ORX2";

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
             /*
            NUnit.Framework.Assert.Greater(IMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Less(IMin, (IQExptVal * (-1)));
            NUnit.Framework.Assert.Less(IMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Greater(IMax, IQExptVal);

            NUnit.Framework.Assert.Greater(QMin, ((-1) * (IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Less(QMin, (IQExptVal * (-1)));
            NUnit.Framework.Assert.Less(QMax, ((IQExptVal + ((IQExptVal * 10) / 100))));
            NUnit.Framework.Assert.Greater(QMax, IQExptVal);
             */
            NUnit.Framework.Assert.Greater(IMax, 5000);
            NUnit.Framework.Assert.Greater(QMax, 5000);


        }



     
      

        /// <summary>
        /// ORxGainSweep 
        /// Inputs: Mykonos.OBSRXCHANNEL Enum. Uses the practical Observation ports, ORX1,2 and SNRXA,B,C.
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
        /// </summary>

        [Test,Sequential]
        [NUnit.Framework.Category("Orx")]
         public static void ORxGainSweep([Values(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO, Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO)]Mykonos.OBSRXCHANNEL channel)
        {

            //Initialize param structure with Hardcoded Values
            double[] profileInfo = Helper.SetOrxProfileInfo(channel);  
            double samplingFreq_MHz = profileInfo[0] / 1000;
            double freqLo_MHz = profileInfo[1] / 1000000;
            double profileBW_MHz = profileInfo[2] / 1000000;
            double testFreq = 2510;
            double amplitude_dBm = -20;
                   
            string[] pcbInfo;
            
            //TODO: Hard coded, may want to read the gain tables instead if custom ones are loaded
            //Number of known indicies in the gain index table. 
            //Should a customer provide a custom gain table, we should determine the number of valid gain indicies
            // and this array size should correspond to that number. 
            int numIndices = 19; 
            double[,] amplitudeData = new double[numIndices, 3]; 
            double[,] amplitudeDiffData = new double[numIndices - 1, 2];
            short[] rxDataArray = new short[16384];
            Console.WriteLine("Detected LO Frequency: " + freqLo_MHz);
            Console.WriteLine("Profile BW: " + profileBW_MHz);
            
            switch (channel)
            {
                case Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO:
                case Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO:
                    testFreq = freqLo_MHz + 10;
                    break;
                case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A:
                case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B:
                case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C:
                    testFreq = freqLo_MHz + 5;
                    amplitude_dBm = -30;
                    break;
            }
            
            //ESG Configuration
            SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
            Console.WriteLine(measEquipment.ESGAddress);
            Console.WriteLine(esg.Identify());
            esg.SetFrequency(testFreq);
            esg.SetAmplitude(amplitude_dBm);
            esg.SetRfOutput(true);

            //Test Sequence
            byte gainIndex = 255;
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
            
            for (int i = 0; i < (numIndices + 1); i++)
            {
                try
                {
                    Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                    Link.spiWrite(0x4F1, 0x80);
                    gainIndex = (byte)(255 - i);
                    Console.WriteLine("Initial gain index = " + Link.Mykonos.getObsRxGain());
                    Link.Mykonos.setObsRxManualGain(channel, gainIndex);    //There is currently an error in the API.
                    Console.WriteLine("Set gain index = " + gainIndex);
                    System.Threading.Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    //Console.WriteLine("Invalid Gain index reached" + i);    //Need to figure out a better way to exit the loop when invalid gain reached
                    Console.WriteLine(e);
                    break;
                }
                finally
                {
                    Link.Disconnect();
                }


                rxDataArray = Helper.MykonosOrxCapture(channel, 8192); //Grab data from the FPGA                
                byte sampleBitWidth = 16;
                double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                amplitudeData[i, 0] = (double)gainIndex;
                amplitudeData[i, 1] = analysisData.FundamentalPower_dBFS;
                if (i == 0)
                {
                    amplitudeData[i, 2] = analysisData.FundamentalPower_dBFS;
                }
                else
                {
                    amplitudeData[i, 2] = amplitudeData[i - 1, 2] - 1;
                    amplitudeDiffData[i - 1, 0] = (double)gainIndex;
                    amplitudeDiffData[i - 1, 1] = amplitudeData[i - 1, 1] - amplitudeData[i, 1];
                    Console.WriteLine(" Gain index :" + amplitudeDiffData[i - 1, 0]);
                    Console.WriteLine(" Differential Amplitude" + amplitudeDiffData[i - 1, 1]);
                }
                
            }
            
           
#if WR_RES_TO_PDF
            string path = ObsRxRfTests.ResPath + "ORxGainSweep";  
            if (channel == Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO)
                path = path + "OBS_RX1_TXLO";
            else
                path = path + "OBS_RX2_TXLO";
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
            pcbInfo = Helper.PcbInfo();

            container[0] = Helper.MakeChartObject(amplitudeData, timeLabels, path);
            container[1] = Helper.MakeChartObject(amplitudeDiffData, timeLabels2, path + "2");

            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);
            //Open Result PDF
            System.Diagnostics.Process.Start(path + ".pdf");
            for (int i = 1; i < (numIndices); i++)
            {
                NUnit.Framework.Assert.IsTrue(((amplitudeDiffData[i - 1, 1] < 1)
                                     && (amplitudeDiffData[i - 1, 1] > 0)));
            }
#endif
        }


        /// <summary>
        /// Test for checking the decimated power measurements on a amplitude sweep and reading back the spi registers
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="amp_dbm"></param>
        [Test, Combinatorial]
        [NUnit.Framework.Category("Orx")]
        public static void ObsRxDecPwrTest([Values(Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO, Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO)]Mykonos.OBSRXCHANNEL channel,
                                            [Values(-20)]int amp_dbm)
        {


            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOn();
            Link.Mykonos.setObsRxPathSource(channel);

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
            const byte NUM_SAMPLES = 255;
            ushort obsRxDecPower_mdBFS = 10;
            byte spiData = 0x0;


            //Generate Test Signal for Rx Capture with ESG
            SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);
            Console.WriteLine("ESG Info:" + esg.Identify());
            Console.WriteLine("ESG Address :" + measEquipment.ESGAddress);
            Console.WriteLine("ESG Generating Tone Freq:" + testSigFreq_MHz);
            Console.WriteLine("ESG Generating Tone Amp:" + amplitude_dBm);
            esg.SetFrequency(testSigFreq_MHz);

            esg.SetRfOutput(true);
            esg.SetAmplitude(amplitude_dBm);
            System.Threading.Thread.Sleep(1000);


            AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
            double samplingFreq_MHz = samplingFreq_Hz / 1000000;
            byte sampleBitWidth = 16;


            for (int amp = -20; amp > -40; amp--)
            {
                System.Threading.Thread.Sleep(100);
                esg.SetAmplitude(amp);
                System.Threading.Thread.Sleep(100);
                short[] rxDataArray = Helper.MykonosOrxCapture(channel, 16384);

                double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                Link.Mykonos.getObsRxDecPower(ref obsRxDecPower_mdBFS);
                spiData = Link.spiRead(0x4E6);


                Console.WriteLine("Received power: " + obsRxDecPower_mdBFS + " calculated power: " + analysisData.FundamentalPower_dBFS + " at " + amp + " dbfs");
                NUnit.Framework.Assert.Less(System.Math.Abs((double)obsRxDecPower_mdBFS / 1000 + analysisData.FundamentalPower_dBFS), 0.50);
                NUnit.Framework.Assert.AreEqual(spiData * 250, obsRxDecPower_mdBFS);
            }


            Link.Disconnect();


        }

        /// <summary>
        /// Basic test for verifying the functionality of the sniffer AGC. Gain and power
        /// measurements are taken during a sweep between -20 and 10 db. Register readback
        /// is also used to ensure correct measurements.
        /// </summary>
        /// <param name="channel"></param>
        [Test, Combinatorial]
        [NUnit.Framework.Category("Orx")]
        public static void ObsRxAGCTest([Values(Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A)]Mykonos.OBSRXCHANNEL channel)
        {


            
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             byte spiData1 = 0;
             ushort power = 0;
             byte agcRx1MaxGainIndex = 255;
             byte agcRx1MinGainIndex = 195;
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


             Link.Mykonos.setupObsRxAgc();

             Link.Mykonos.radioOn();
             Link.Mykonos.setObsRxPathSource(channel);
             Console.WriteLine("gain before: " + Link.Mykonos.getObsRxGain());
             Console.WriteLine(Link.spiRead(0x448));
             //Link.spiWrite(0x448, 0x2);
             Link.Mykonos.setObsRxGainControlMode(Mykonos.GAINMODE.AGC);

             Console.WriteLine("gain after: " + Link.Mykonos.getObsRxGain());
             //Assert.Pass();


             SG_AgilentESG esg = new SG_AgilentESG(measEquipment.ESGAddress);

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
             double testSigFreq_MHz = (freqLo_kHz / 1000 + 15);
             Console.WriteLine("Rx Test Signal Freq (MHz): " + testSigFreq_MHz);
             //Generate Test Signal for Rx Capture with ESG
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

            //Sweep from -20 to 10db
             for (int amplitude_dBm = -20; amplitude_dBm < 10; amplitude_dBm++)
             {
                 esg.SetAmplitude(amplitude_dBm);
                 System.Threading.Thread.Sleep(100);
                 Link.Mykonos.getObsRxDecPower(ref power);

                 short[] rxDataArray = Helper.MykonosOrxCapture(channel, 16384);
                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                 response = Link.Mykonos.getObsRxGain();
                 spiData1 = Link.spiRead(0x4B6);
                 Assert.AreEqual(spiData1 + 128, Int32.Parse(response), "Register Readback for Sniffer Gain not as expected");
                

                 if (amplitude_dBm == -20)
                     initgain = response;
                 Console.WriteLine("Gain: " + response + "DecPower: " + power + " FFT Power " + analysisData.FundamentalPower_dBFS + " at " + amplitude_dBm + "\n");
             }

             //Sweep from 10 to -20db

             for (int amplitude_dBm = 10; amplitude_dBm >= -20; amplitude_dBm--)
             {
                 esg.SetAmplitude(amplitude_dBm);
                 System.Threading.Thread.Sleep(100);
                 Link.Mykonos.getObsRxDecPower(ref power);

                 short[] rxDataArray = Helper.MykonosOrxCapture(channel, 16384);
                 double[] data = AdiMath.complexfftAndScale(rxDataArray, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

                 response = Link.Mykonos.getObsRxGain();
                 spiData1 = Link.spiRead(0x4B6);
                 Assert.AreEqual(spiData1 + 128, Int32.Parse(response), "Register Readback for Sniffer Gain not as expected");


                 if (amplitude_dBm == -20)
                     finalgain = response;
                 Console.WriteLine("Gain: " + response + "DecPower: " + power + " FFT Power " + analysisData.FundamentalPower_dBFS + " at " + amplitude_dBm + "\n");
 
             }
             Assert.AreEqual(initgain, finalgain, "Initial gain is not equal to the final gain");
             Link.Disconnect();
              
        }

    }
}
