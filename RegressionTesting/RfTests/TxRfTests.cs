#define WR_RES_TO_CONSOLE //Option to write out results to console
#define WR_RES_TO_PDF     //Option to Create PDF report
#define WR_RES_TO_TXT

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
    /// Test Suite of Tx RF tests
    /// </summary>
    [TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "3.5")]
    [TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "3.5")]
    [TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "2.5")]
    [TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "2.5")]
    [TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5", "0.7")]
    [TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5", "0.7")]
    [NUnit.Framework.Category("RF")]
    public class TxRfTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string TxProfile;
        private string TxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\TxRfTests\";
        public TxRfTests()
        {
            this.TxProfile = settings.mykSettings.txProfileName;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public TxRfTests(string TxProfile, string freq)
        {
            this.TxProfile = TxProfile;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            if (freq == "3.5")
            {
                settings.txPllLoFreq_Hz = 3500000000;
            }
            else if (freq == "2.5")
            {
                settings.txPllLoFreq_Hz = 2500000000;
            }
            else
            {
                settings.txPllLoFreq_Hz = 700000000;
            }
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        /// <summary>
        /// Mykonos Test Setup Prior to RF Testing
        /// Setup Parameters:  
        /// From Locally Stored ARM Firmware     @"..\..\..\resources\Profiles";
        /// From Locally Stored Default Profile  @"..\..\..\resources\ArmBinaries"
        ///     Device Clock: 245.760MHz
        ///     Rx Profile: Default settings see TestSettings.cs
        ///     Tx Profile: As Per Test Fixture
        ///     Rx Data Paths: RX1 and Rx2 Enabled
        ///     Tx Data Paths: TX1 and TX2 Enabled
        ///     ObsRx Data Paths: ObsRx1 Enabled
        ///     Sniffer Data Path: Disabled
        ///     JESD204B Configurations: Default settings see TestSettings.cs
        /// </summary>
        [SetUp]
        public void TxRfTestsInit()
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
            settings.mykSettings.txProfileName = TxProfile;
            TestSetup.TestSetupInit(settings);
 
        }

        [Test, Combinatorial]
        public static void TxTransmitTestTone([Values(20)]double offsetFreq_Mhz)
        {
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];

            profileInfo[0] = settings.txProfileData.IqRate_kHz;
            profileInfo[1] = settings.txPllLoFreq_Hz;
            profileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;

            double txIqDataRate_kHz = profileInfo[0];
            double profileBandwidth_MHz = profileInfo[2] / 1000000;
            Console.WriteLine("IQ Data Rate (kHz): " + txIqDataRate_kHz);
            Console.WriteLine("Profile Bandwdith (MHz): " + profileBandwidth_MHz);
            double freqTxLo_MHz = profileInfo[1] / 1000000;
            Console.WriteLine("Tx LO Frequency (MHz): " + freqTxLo_MHz);

            //Calculate Test Tone And Generate
            Console.WriteLine("Tone Offset (Mhz): " + offsetFreq_Mhz);
            double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
            double offsetFreq_Hz = offsetFreq_Mhz * 1000000;
            Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, -15);

            //Config and Enable Mykonos with Test Specific Settings
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);
            Link.Mykonos.radioOn();
            Link.Disconnect();


            //Connect to Signal Analyser
            //The span is fixed to 50MHz
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
            pxa.SetCenterSpan(freqTxLo_MHz, 50, 0);
            pxa.SetAtten(20);
            pxa.SetRefLevel(10);
            pxa.SetMarker(1, freqTxLo_MHz + offsetFreq_Mhz); //Fundemental Amplitued Marker
            double fundAmp_dB = Double.Parse(pxa.GetMarker(1), style);
            NUnit.Framework.Assert.Greater(fundAmp_dB, 5);

        }
        ///<summary>
        /// RF Test Name: 
        ///      TxPassbandSweep
        /// RF Feature Under Test: 
        ///     The TxPassbandSweep is the test to verify 
        ///      the gain flatness over the passband. 		
        /// RF Test Procedure: 
        ///     Based on Profile Data Determine Profile BW & Sampling Freq
        ///     & LO Frequency determine a Frequency range for sweep.
        ///     Allow for testing 10%  over & under PassBand
        ///     Configure FPGA Tone Generator & Enable Mykonos Datapath
        ///     Sweep thru Passband Frequencies
        ///     From PXA Analayser record
        ///         1. Fundamental amplitude (dBm)
        ///         2. LO leakage (dBm)
        ///         3. Image Power(dBm)
        ///     Graph Data.
        /// RF Test Pass Criteria: 
        ///      Check Min Max Fund Amplitudes are within 0.5db of each other.		
        ///</summary>
         [Test, Sequential]
         [NUnit.Framework.Category("TX")]
        public static void TxPassbandSweep([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)

        {
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            int atten = 0;
            int backoff = 0;
            profileInfo[0] = settings.txProfileData.IqRate_kHz;
            profileInfo[1] = settings.txPllLoFreq_Hz;
            profileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;
            
            double txIqDataRate_kHz = profileInfo[0]; 
            double profileBandwidth_MHz = profileInfo[2] / 1000000; 
            Console.WriteLine("IQ Data Rate (kHz): " + txIqDataRate_kHz);
            Console.WriteLine("Profile Bandwdith (MHz): " + profileBandwidth_MHz);
            double freqTxLo_MHz = profileInfo[1] / 1000000; 
            Console.WriteLine("Tx LO Frequency (MHz): " + freqTxLo_MHz);
            
            //Define Test Parameters Based on Profile Info & Lo Frequency
            double SwpMinFreq = freqTxLo_MHz - (1.1 * (profileBandwidth_MHz / 2));
            double SwpMaxFreq = freqTxLo_MHz + (1.1 * (profileBandwidth_MHz / 2));
            double SwpSigAmp = -40 ;
            int SwpNumSteps = 50;
            SwpParamStruct param = new SwpParamStruct(SwpMinFreq, SwpMaxFreq, SwpSigAmp, SwpNumSteps);
            double SwpStepSz = (param.freqMax - param.freqMin) / param.numSteps;
            Console.WriteLine("SwpMinFreq (MHz): " + SwpMinFreq);
            Console.WriteLine("SwpMaxMax (MHz): " + SwpMaxFreq);
            Console.WriteLine("SwpSigAmp (MHz): " + SwpSigAmp);
            
            //Define Data Array for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
            double[,] fundAmpXY = new double[param.numSteps + 1, 2];
            double[,] loLeakageXY = new double[param.numSteps + 1, 2];
            double[,] imageAmpXY = new double[param.numSteps + 1, 2];

            //Connect to Signal Analyser
            //The span is fixed to 100MHz
            //Set Marker 2 as LO leakage marker
            //Note: this may need to be set depending on profile.
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
            pxa.SetCenterSpan(freqTxLo_MHz, 200, 0);
            pxa.SetAtten(20);
            pxa.SetRefLevel(10);
            pxa.SetMarker(2, freqTxLo_MHz); 
            
            //Config and Enable Mykonos with Test Specific Settings
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);
            Link.Mykonos.radioOn();
            Link.Disconnect();

#if WR_RES_TO_CONSOLE 
            Console.WriteLine(" OffSet Frequency input (MHz):" +  
                              "Fundamental Amplitude (dBm), " +
                              "LO Leakage (dBm)," + 
                              "Image Amplitude (dBm)," );
#endif
            //Test Sequence
            //Sweep Thru Passband
            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double fundAmp, loLeakage, imageAmp;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;
                
                if (offsetFreq_Hz == 0)
                {
                    fundAmpXY[i, 0] = offsetFreq_Mhz;
                    fundAmpXY[i, 1] = fundAmpXY[i-1, 1];

                    loLeakageXY[i, 0] = offsetFreq_Mhz;
                    loLeakageXY[i, 1] = loLeakageXY[i-1, 1];

                    imageAmpXY[i, 0] = offsetFreq_Mhz;
                    imageAmpXY[i, 1] = imageAmpXY[i-1, 1];
                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA
                
                pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
                pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                fundAmp = Double.Parse(pxa.GetMarker(1), style);  
                loLeakage = Double.Parse(pxa.GetMarker(2), style);
                imageAmp = Double.Parse(pxa.GetMarker(3), style);

                fundAmpXY[i, 0] = offsetFreq_Mhz;
                fundAmpXY[i, 1] = fundAmp;

                loLeakageXY[i, 0] = offsetFreq_Mhz;
                loLeakageXY[i, 1] = loLeakage;

                imageAmpXY[i, 0] = offsetFreq_Mhz;
                imageAmpXY[i, 1] = imageAmp;

               
                

#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + fundAmp + ",   " + loLeakage + ",   " + imageAmp + ",   " + (fundAmp - imageAmp) + ",   ");
                
#endif
            }
           
#if WR_RES_TO_PDF
            //Graph Data and Save in PDF Form
            var doc1 = new Document();
            string path = TxRfTests.ResPath + "TxPassbandSweep";
            TxRfTests instance = new TxRfTests();    
            string[] pcbInfo;
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[3];
            string[] fundAmpLabels = new string[] { "Fundamental Amplitude Versus Offset Tone Frequency (from LO)",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            string[] loLeakageLabels = new string[] { "LO Leakage Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            string[] imageAmpLabels = new string[] { "Image Amplitude Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, backoff.ToString(), atten.ToString());

           // pcbInfo = Helper.PcbInfo();
            container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
            container[1] = Helper.MakeChartObject(loLeakageXY, loLeakageLabels, path);
            container[2] = Helper.MakeChartObject(imageAmpXY, imageAmpLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF            
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
            // Write data to txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
            {
                file.WriteLine("Sample,  Frequency MHz, Fundamental Power(dBm),  LOL(dBFS), Image Power(dBFS),");
                for (int i = 0; i < param.numSteps; i++)
                {
                    file.WriteLine(i + "," + fundAmpXY[i, 0].ToString() + "," + fundAmpXY[i, 1].ToString() + "," + loLeakageXY[i, 1].ToString() + "," + imageAmpXY[i, 1].ToString());
                }
            }
#endif

            //Check Min Max Fund Amplitudes are within 0.5db of each other.
            var MinFundAmp = System.Linq.Enumerable.Range(0, param.numSteps).Select(i => fundAmpXY[i, 1]).Min();
            var MaxFundAmp = System.Linq.Enumerable.Range(0, param.numSteps).Select(i => fundAmpXY[i, 1]).Max();
            Console.WriteLine("MinFundAmp: " + MinFundAmp);
            Console.WriteLine("MaxFundAmp: " + MaxFundAmp);
            Console.WriteLine("MaxDiffFundAmp: " + (MaxFundAmp - MinFundAmp));
            NUnit.Framework.Assert.IsTrue((MaxFundAmp - MinFundAmp) < 1.0);

        }
   


         ///<summary>
         /// RF Test Name: 
         ///      TxAttenuationSweep
         /// RF Feature Under Test: 
         ///     The TxAttenuation sweep attempts to test all valid 
         ///     attenuation settings for the Tx channel. 		
         /// RF Test Procedure: 
         ///     Based on Profile Data Determine Profile BW & Sampling Freq
         ///     & LO Frequency determine Test Signal Frequency.
         ///     Configure FPGA Tone Generator,the generated tone frequency (10Mhz Offset) & 
         ///     amplitude is held constant. 
         ///     Enable Mykonos Datapath
         ///     Sweep thru Valid Tx Datapath Attenuation settings
         ///     From PXA Analayser record
         ///         1. Fundamental amplitude (dBm)
         ///     Graph and output Data to Text file.
         /// RF Test Pass Criteria: 
         ///      	None
         ///</summary>
         [Test, Sequential]
         [NUnit.Framework.Category("TX")]
        public static void TxAttenuationSweep([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
        {
            int atten = 0;
            int backoff = 0;
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            profileInfo[0] = settings.txProfileData.IqRate_kHz;
            profileInfo[1] = settings.txPllLoFreq_Hz;
            profileInfo[2] = settings.txProfileData.PrimarySigBw_Hz;
            
            double txIqDataRate_kHz = profileInfo[0]; 
            double profileBandwidth_MHz = profileInfo[2] / 1000000; 
            Console.WriteLine("IQ Data Rate (kHz): " + txIqDataRate_kHz);
            Console.WriteLine("Profile Bandwdith (MHz): " + profileBandwidth_MHz);
            double freqTxLo_MHz = profileInfo[1] / 1000000;
            Console.WriteLine("Tx LO Frequency (MHz): " + freqTxLo_MHz);

            //Configure Tone Generator
            //10MHz offset tone generation
            double offsetFreq_Hz = 10000000;
            double[,] fundAmpXY = new double[42, 2];
            Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);

            //Connect to Signal Analyser
            //The span is fixed to 50MHz
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
            pxa.SetCenterSpan(freqTxLo_MHz, 50, 0);
            pxa.SetAtten(20);
            pxa.SetRefLevel(10);
            pxa.SetMarker(1, freqTxLo_MHz + 10); //Fundemental Amplitued Marker
            pxa.SetMarker(2, freqTxLo_MHz); //LO Leakage Marker
            pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image Amplitude marker

            //Config and Enable Mykonos with Test Specific Settings
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOn();

            //Test Sequence
            //Iterate Thru Tx Attenuation Settings 
            //Measure Amplitude of the fundamental
            //TODO: 0 to 41950
            int step = 0;
            double fundAmp_dB = 0;
            double minAttnVal_dB = 0;
            double maxAttnVal_dB = 41.95;
            for (double attenVal_dB = minAttnVal_dB; (attenVal_dB < maxAttnVal_dB); attenVal_dB += 1 ) 
            {
                try
                {
                    if(channel == Mykonos.TXCHANNEL.TX1)
                        Link.Mykonos.setTx1Attenuation(attenVal_dB);
                    if (channel == Mykonos.TXCHANNEL.TX2)
                        Link.Mykonos.setTx2Attenuation(attenVal_dB);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }
                               
                pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);

                fundAmp_dB = Double.Parse(pxa.GetMarker(1), style);
                fundAmpXY[step, 1] = fundAmp_dB;
                fundAmpXY[step, 0] = attenVal_dB;
                step++;
#if WR_RES_TO_CONSOLE
                Console.WriteLine("Fundamental Amplitude, Attenuation Value " + attenVal_dB + ": " + fundAmp_dB);
#endif

            }
            Link.Disconnect();
#if WR_RES_TO_PDF
            //Graph Data and Save in PDF Form
            var doc1 = new Document();
            string path = TxRfTests.ResPath + "TxAttenuationSweep";
            string[] pcbInfo;
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
            string[] fundAmpLabels = new string[] { "Fundamental Amplitude Versus Offset Tone Frequency (from LO)",
                                                "setTxAtten Argument",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };

            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, backoff.ToString(), atten.ToString());

            container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
            TxRfTests instance = new TxRfTests();
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
        }
         ///<summary>
         /// RF Test Name: 
         ///     TxPassbandSweepTxNCO
         /// RF Feature Under Test: 
         ///     MYKONOS_enableTxNco() 
         ///     This function enables/disables the Tx NCO test tone 
         ///     and forces the TxAtten to max analog output power 
         ///     with 6dB digital backoff to protect the digital filter from clipping
         /// RF Test Procedure: 
         ///    Currently calls TxNco to generate 2 tones from -60Mhz to +60Mhz in 1 meg steps
         /// RF Test Pass Criteria: 
         ///      Range is (-IQrate/2 to IQrate/2)
         ///      Base sweep based on profile IQ rate.
         /// Notes:
         ///      Not Tested.
         ///      Not Complete
         ///</summary>
         [Test, Sequential]
         [TestCategory("Tx Tests")]
         public static void TxPassbandSweepTxNCO([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel, [Values(2500, 2500)]double deltaF_kHz)
         {
             AdiCommandServerClient Link = AdiCommandServerClient.Instance;
             Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
             SA_AgilentPXA pxa = new SA_AgilentPXA(measEquipment.PXAAddress);
             NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;

             double spanMhz;
             int numSteps = 0;

             double txIqDataRate_MHz = 0;
             double freqTxLo_MHz = 0;
             double profileBandwidth_MHz = 0;
             double toneOffsetFreq_Mhz = 0;
             double offsetFreq_Hz = 0;
             double testRfFreq_MHz = 0;


             try
             {
                 //Enable the Transmitter Path
                 Link.Mykonos.radioOn();
                 //Profile Information
                 txIqDataRate_MHz = (double)(settings.txProfileData.IqRate_kHz) / 1000;
                 freqTxLo_MHz = (double)(settings.txPllLoFreq_Hz) / 1000000;
                 profileBandwidth_MHz = (double)settings.txProfileData.PrimarySigBw_Hz;

                 numSteps = (int)(txIqDataRate_MHz / (deltaF_kHz / 1000.0));
                 spanMhz = txIqDataRate_MHz;
                 //Define Data Array for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
                 double[,] fundAmpXY = new double[numSteps, 2];
                 double[,] loLeakageXY = new double[numSteps, 2];
                 double[,] imageAmpXY = new double[numSteps, 2];

                 pxa.SysPreset();
                 pxa.SetRBW_VBW(30, 30);
                 pxa.SetCenterSpan(freqTxLo_MHz, 200, 0); 
                 pxa.SetAtten(20);
                 pxa.SetMarker(2, freqTxLo_MHz); //LO leakage marker

                 double peakAmp_dBm = 0;
                 double peakFreq_Mhz = 0;
                 double deltaF = txIqDataRate_MHz / numSteps;

                 double fundAmp = 0;
                 double loLeakage = 0;
                 double imageAmp = 0;
                 Console.Write(numSteps + 1);
                 for (int i = 0; (i < numSteps); i++)
                 {
                     toneOffsetFreq_Mhz = (-0.5 * txIqDataRate_MHz) + ((deltaF_kHz / 1000.0) * i);

                     offsetFreq_Hz = toneOffsetFreq_Mhz * 1000000;
                     testRfFreq_MHz = freqTxLo_MHz + offsetFreq_Hz / 1000000;

                     Link.Mykonos.enableTxNco(1, (int)(toneOffsetFreq_Mhz * 1000), (int)(toneOffsetFreq_Mhz * 1000));
                    
                     System.Threading.Thread.Sleep(500);
                     //pxa.HoldAverage(10);
                     pxa.MeasPeakPower(ref peakAmp_dBm, ref peakFreq_Mhz);

                     //Set PXA Markers  
                     /* for this to work, NCO freq set must be rastered to its actual frequency in the chip */
                     //pxa.SetMarker(3, freqTxLo_MHz - (freqTxLo_MHz - peakFreq_Mhz)); //Image marker
                     //pxa.SetMarker(1, testRfFreq_MHz); //Fundamental amplitue marker

                     //fundAmp = Double.Parse(pxa.GetMarker(1), style);
                     //loLeakage = Double.Parse(pxa.GetMarker(2), style);
                     //imageAmp = Double.Parse(pxa.GetMarker(3), style);

                     //TODO: Marker 3 does not always sit on the image, sometimes its off a bin or so and measures incorrectly. 
                     pxa.SetMarker(3, (freqTxLo_MHz - ((peakFreq_Mhz / 1000000.0) - freqTxLo_MHz))); //Image marker

                     fundAmp = peakAmp_dBm;
                     loLeakage = Double.Parse(pxa.GetMarker(2), style);
                     imageAmp = Double.Parse(pxa.GetMarker(3), style);

                     fundAmpXY[i, 0] = toneOffsetFreq_Mhz;
                     fundAmpXY[i, 1] = fundAmp;

                     loLeakageXY[i, 0] = toneOffsetFreq_Mhz;
                     loLeakageXY[i, 1] = loLeakage;

                     imageAmpXY[i, 0] = toneOffsetFreq_Mhz;
                     imageAmpXY[i, 1] = imageAmp;
                     if ((toneOffsetFreq_Mhz > -1) && (toneOffsetFreq_Mhz < 1))
                     {
                         Console.WriteLine("Bypass:" + toneOffsetFreq_Mhz);
                         fundAmpXY[i, 0] = toneOffsetFreq_Mhz;
                         fundAmpXY[i, 1] = fundAmpXY[i - 1, 1];

                         loLeakageXY[i, 0] = toneOffsetFreq_Mhz;
                         loLeakageXY[i, 1] = loLeakageXY[i - 1, 1];

                         imageAmpXY[i, 0] = toneOffsetFreq_Mhz;
                         imageAmpXY[i, 1] = imageAmpXY[i - 1, 1];
                         continue;
                     }
#if true //Optional printout for text based readout in test output window
                     Console.WriteLine("Fundamental Amplitude (dBm)" + i + ": "+ toneOffsetFreq_Mhz + ": " + fundAmp);
                     Console.WriteLine("LO Leakage (dBm)" + toneOffsetFreq_Mhz + ": " + loLeakage);
                     Console.WriteLine("Image Amplitude (dBm)" + toneOffsetFreq_Mhz + ": " + imageAmp);
                     Console.WriteLine("Image Amplitude (dBc)" + toneOffsetFreq_Mhz + ": " + (fundAmp - imageAmp));
#endif



                 }
                 Link.Disconnect();
                 //Graph Data and Save in PDF Form
                 var doc1 = new Document();
                 string path = TxRfTests.ResPath + "TxPassbandSweepTxNco";
                 string[] pcbInfo;
                 iTextSharp.text.Image[] container = new iTextSharp.text.Image[3];
                 string[] fundAmpLabels = new string[] { "Fundamental Amplitude Versus Offset Tone Frequency (from LO)",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
                 string[] loLeakageLabels = new string[] { "LO Leakage Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
                 string[] imageAmpLabels = new string[] { "Image Amplitude Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
                
                 pcbInfo = Helper.PcbInfo();
                 Console.Write(fundAmpXY[48, 0]);
                 container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
                 container[1] = Helper.MakeChartObject(loLeakageXY, loLeakageLabels, path);
                 container[2] = Helper.MakeChartObject(imageAmpXY, imageAmpLabels, path);
                 Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

                 //Open Result PDF            
                 System.Diagnostics.Process.Start(path + ".pdf");

                 //Check Min Max Fund Amplitudes are within +/-0.5db of each other.
                 var MinFundAmp = System.Linq.Enumerable.Range(15, 30).Select(i => fundAmpXY[i, 1]).Min();
                 var MaxFundAmp = System.Linq.Enumerable.Range(15, 30).Select(i => fundAmpXY[i, 1]).Max();
                 Console.WriteLine("MinFundAmp: " + MinFundAmp);
                 Console.WriteLine("MaxFundAmp: " + MaxFundAmp);
                 Console.WriteLine("MaxDiffFundAmp: " + (MaxFundAmp - MinFundAmp));
                 NUnit.Framework.Assert.IsTrue((MaxFundAmp - MinFundAmp) < 1.0);
             }
             catch (Exception e)
             {
                 Console.WriteLine(e);
                 throw;
             }

           


         }

    }
}
