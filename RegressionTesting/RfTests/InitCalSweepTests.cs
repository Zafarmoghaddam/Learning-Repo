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
    [TestFixture("Tx 20/100MHz, IQrate  122.88MHz, Dec5")]
    [TestFixture("Tx 75/200MHz, IQrate 245.76MHz, Dec5")]
    [NUnit.Framework.Category("RF")]
    public class InitCalSweepTests
    {

        static MeasurementEquipment measEquipment = new MeasurementEquipment();
        private string TxProfile;
        private string TxProfileString;
        public static TestSetupConfig settings = new TestSetupConfig();
        public static string ResPath = @"..\..\..\TestResults\LOLeakageTests\";
        public InitCalSweepTests()
        {
            this.TxProfile = settings.mykSettings.txProfileName;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
        }

        public InitCalSweepTests(string TxProfile)
        {
            this.TxProfile = TxProfile;
            this.TxProfileString = Helper.parseProfileName(TxProfile);
            ResPath = System.IO.Path.Combine(ResPath, TxProfileString);
            System.IO.Directory.CreateDirectory(ResPath);
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
        //[SetUp]
        public void LOLeakageTestsInit()
        {
            //Use Default Test Constructor
            //Start Calibration
            System.Diagnostics.Debug.WriteLine("Hello");
            UInt32 calMask = (UInt32)(Mykonos.CALMASK.TX_BB_FILTER) |
               (UInt32)(Mykonos.CALMASK.ADC_TUNER) |
               (UInt32)(Mykonos.CALMASK.TIA_3DB_CORNER) |
               (UInt32)(Mykonos.CALMASK.DC_OFFSET) |
               (UInt32)(Mykonos.CALMASK.TX_ATTENUATION_DELAY) |
               (UInt32)(Mykonos.CALMASK.RX_GAIN_DELAY) |
               (UInt32)(Mykonos.CALMASK.FLASH_CAL) |
               (UInt32)(Mykonos.CALMASK.PATH_DELAY) |
                //(UInt32)(Mykonos.CALMASK.TX_LO_LEAKAGE_INTERNAL) |
               (UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
               (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
               (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                //(UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
               (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            settings.mykSettings.txProfileName = TxProfile;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);

        }

        public void QECTestsInit()
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
               //(UInt32)(Mykonos.CALMASK.TX_QEC_INIT) |
               (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_LO_DELAY) |
               (UInt32)(Mykonos.CALMASK.LOOPBACK_RX_RX_QEC_INIT) |
                //(UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
               (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            settings.mykSettings.txProfileName = TxProfile;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);

        }
        public void AllCalsInit()
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
                //(UInt32)(Mykonos.CALMASK.RX_LO_DELAY) |
               (UInt32)(Mykonos.CALMASK.RX_QEC_INIT);
            settings.calMask = calMask;
            settings.mykSettings.txProfileName = TxProfile;
            //Call Test Setup 
            TestSetup.TestSetupInit(settings);

        }



        ///<summary>
        /// RF Test Name: 
        ///      LOLeakageTest
        /// RF Feature Under Test: 
        ///     The LOLeakageTest is the test to verify 
        ///      the LO Leakage over the passband. 		
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
        ///      ??		
        ///</summary>
        [Test, Sequential]
        [NUnit.Framework.Category("TX")]
        public static void LOLeakageSweep([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
        {
            InitCalSweepTests obj = new InitCalSweepTests();
            obj.LOLeakageTestsInit();
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            double backoff = -15;
            int atten = 0;
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
            double SwpMinFreq = freqTxLo_MHz - (2 * (profileBandwidth_MHz / 2));
            double SwpMaxFreq = freqTxLo_MHz + (2 * (profileBandwidth_MHz / 2));
            double SwpSigAmp = -40;
            int SwpNumSteps = 200;
            SwpParamStruct param = new SwpParamStruct(SwpMinFreq, SwpMaxFreq, SwpSigAmp, SwpNumSteps);
            double SwpStepSz = (param.freqMax - param.freqMin) / param.numSteps;
            Console.WriteLine("SwpMinFreq (MHz): " + SwpMinFreq);
            Console.WriteLine("SwpMaxMax (MHz): " + SwpMaxFreq);
            Console.WriteLine("SwpSigAmp (MHz): " + SwpSigAmp);

            //Define Data Array for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
            //double[,] fundAmpXY = new double[param.numSteps, 2];
            double[,] loLeakageXY = new double[param.numSteps + 1, 3];
            double[,] loLeakageXYcal = new double[param.numSteps + 1, 2];
            double[,] loLeakageXYdif = new double[param.numSteps + 1, 2];

            //double[,] imageAmpXY = new double[param.numSteps, 2];

            //Connect to Signal Generator 
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
                              "LO Leakage (dBm)");
#endif
            //Test Sequence
            //Sweep Thru Passband
            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double loLeakage;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;

                if (offsetFreq_Hz == 0)
                {

                    loLeakageXY[i, 0] = offsetFreq_Mhz;
                    loLeakageXY[i, 1] = loLeakageXY[i - 1, 1];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA

                pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
                pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                loLeakage = Double.Parse(pxa.GetMarker(2), style);


                loLeakageXY[i, 0] = offsetFreq_Mhz;
                loLeakageXY[i, 1] = loLeakage;


#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + loLeakage + ",   ");

#endif

            }


            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOff();
            Link.Disconnect();

            obj.AllCalsInit();

            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);
            Link.Mykonos.radioOn();
            Link.Disconnect();

            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double loLeakage;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;

                if (offsetFreq_Hz == 0)
                {

                    //loLeakageXY[i, 0] = offsetFreq_Mhz;
                    loLeakageXY[i, 2] = loLeakageXY[i - 1, 2];
                    //loLeakageXY[i, 3] = loLeakageXY[i, 1] - loLeakageXY[i, 2];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA

                pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
                pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                loLeakage = Double.Parse(pxa.GetMarker(2), style);


                //loLeakageXY[i, 0] = offsetFreq_Mhz;
                loLeakageXY[i, 2] = loLeakage;
                //loLeakageXY[i, 3] = loLeakageXY[i, 1] - loLeakageXY[i, 2];

#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + loLeakage + ",   ");

#endif
            }

#if WR_RES_TO_PDF
            //Graph Data and Save in PDF Form
            var doc1 = new Document();
            string path = TxRfTests.ResPath + "LOLeakageSweep";
            TxRfTests instance = new TxRfTests();
            string[] pcbInfo;
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
            string[] loLeakageLabels = new string[] { "LO Leakage Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "LO Leakage Versus Offset Tone Frequency without Calibrations",  "LO Leakage Versus Offset Tone Frequency with Calibrations",  "LO Leakage Versus Offset Tone Frequency Difference" };
            string[] loLeakageLabelscal = new string[] { "LO Leakage Versus Offset Tone Frequency with Calibrations",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            string[] loLeakageLabelsdif = new string[] { "LO Leakage Versus Offset Tone Frequency Difference",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, backoff.ToString(), atten.ToString());
            // pcbInfo = Helper.PcbInfo();
            //container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
            container[0] = Helper.MakeChartObject(loLeakageXY, loLeakageLabels, path);
            //container[1] = Helper.MakeChartObject(loLeakageXYcal, loLeakageLabelscal, path);
            //container[2] = Helper.MakeChartObject(loLeakageXYdif, loLeakageLabelsdif, path);

            //container[2] = Helper.MakeChartObject(imageAmpXY, imageAmpLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF            
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
            // Write data to txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
            {
                file.WriteLine("Sample,  Frequency MHz, LOL(dBFS) w/o cal, LOL(dBFS) w/ cal");
                for (int i = 0; i <= param.numSteps; i++)
                {
                    file.WriteLine(i + "," + loLeakageXY[i, 0].ToString() + "," + loLeakageXY[i, 1].ToString() + "," + loLeakageXY[i, 2].ToString());
                }
            }
#endif

            //Check Min Max Fund Amplitudes are within 0.5db of each other.
            /*
            var MinFundAmp = System.Linq.Enumerable.Range(0, param.numSteps).Select(i => fundAmpXY[i, 1]).Min();
            var MaxFundAmp = System.Linq.Enumerable.Range(0, param.numSteps).Select(i => fundAmpXY[i, 1]).Max();
            Console.WriteLine("MinFundAmp: " + MinFundAmp);
            Console.WriteLine("MaxFundAmp: " + MaxFundAmp);
            Console.WriteLine("MaxDiffFundAmp: " + (MaxFundAmp - MinFundAmp));
            NUnit.Framework.Assert.IsTrue((MaxFundAmp - MinFundAmp) < 0.5);
            */
        }
        [Test, Sequential]
        [NUnit.Framework.Category("TX")]
        public static void QECInitSweep([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
        {
            InitCalSweepTests obj = new InitCalSweepTests();
            obj.QECTestsInit();
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            double backoff = -15;
            int atten = 0;
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
            double SwpMinFreq = freqTxLo_MHz - (2 * (profileBandwidth_MHz / 2));
            double SwpMaxFreq = freqTxLo_MHz + (2 * (profileBandwidth_MHz / 2));
            double SwpSigAmp = -40;
            int SwpNumSteps = 200;
            SwpParamStruct param = new SwpParamStruct(SwpMinFreq, SwpMaxFreq, SwpSigAmp, SwpNumSteps);
            double SwpStepSz = (param.freqMax - param.freqMin) / param.numSteps;
            Console.WriteLine("SwpMinFreq (MHz): " + SwpMinFreq);
            Console.WriteLine("SwpMaxMax (MHz): " + SwpMaxFreq);
            Console.WriteLine("SwpSigAmp (MHz): " + SwpSigAmp);

            //Define Data Array for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
            //double[,] fundAmpXY = new double[param.numSteps, 2];
            double[,] loLeakageXY = new double[param.numSteps + 1, 3];
            double[,] loLeakageXYcal = new double[param.numSteps + 1, 2];
            double[,] loLeakageXYdif = new double[param.numSteps + 1, 2];

            //double[,] imageAmpXY = new double[param.numSteps, 2];

            //Connect to Signal Generator 
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
                              "Image Amplitude (dBm)");
#endif
            //Test Sequence
            //Sweep Thru Passband
            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double loLeakage;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;

                if (offsetFreq_Hz == 0)
                {

                    loLeakageXY[i, 0] = offsetFreq_Mhz;
                    loLeakageXY[i, 1] = loLeakageXY[i - 1, 1];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA

                pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
                pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                loLeakage = Double.Parse(pxa.GetMarker(3), style);


                loLeakageXY[i, 0] = offsetFreq_Mhz;
                loLeakageXY[i, 1] = loLeakage;


#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + loLeakage + ",   ");

#endif

            }


            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOff();
            Link.Disconnect();


            obj.AllCalsInit();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);
            Link.Mykonos.radioOn();
            Link.Disconnect();

            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double loLeakage;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;

                if (offsetFreq_Hz == 0)
                {

                    //loLeakageXY[i, 0] = offsetFreq_Mhz;
                    loLeakageXY[i, 2] = loLeakageXY[i - 1, 2];
                    //loLeakageXY[i, 3] = loLeakageXY[i, 1] - loLeakageXY[i, 2];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA

                pxa.SetMarker(3, freqTxLo_MHz - (offsetFreq_Hz / 1000000)); //Image marker
                pxa.SetMarker(1, testFreq_MHz); //Fundamental amplitue marker
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                loLeakage = Double.Parse(pxa.GetMarker(3), style);


                //loLeakageXY[i, 0] = offsetFreq_Mhz;
                loLeakageXY[i, 2] = loLeakage;
                //loLeakageXY[i, 3] = loLeakageXY[i, 1] - loLeakageXY[i, 2];

#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + loLeakage + ",   ");

#endif
            }

#if WR_RES_TO_PDF
            //Graph Data and Save in PDF Form
            var doc1 = new Document();
            string path = TxRfTests.ResPath + "QECInitSweep";
            TxRfTests instance = new TxRfTests();
            string[] pcbInfo;
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
            string[] loLeakageLabels = new string[] { "Image Amplitude Versus Offset Tone Frequency",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Image Amplitude Versus Offset Tone Frequency without Calibrations",  "Image Amplitude Versus Offset Tone Frequency with Calibrations",  "LO Leakage Versus Offset Tone Frequency Difference" };
            string[] loLeakageLabelscal = new string[] { "Image Amplitude Versus Offset Tone Frequency with Calibrations",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            string[] loLeakageLabelsdif = new string[] { "Image Amplitude Versus Offset Tone Frequency Difference",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, backoff.ToString(), atten.ToString());
            // pcbInfo = Helper.PcbInfo();
            //container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
            container[0] = Helper.MakeChartObject(loLeakageXY, loLeakageLabels, path);
            //container[1] = Helper.MakeChartObject(loLeakageXYcal, loLeakageLabelscal, path);
            //container[2] = Helper.MakeChartObject(loLeakageXYdif, loLeakageLabelsdif, path);

            //container[2] = Helper.MakeChartObject(imageAmpXY, imageAmpLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF            
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
            // Write data to txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
            {
                file.WriteLine("Sample,  Frequency MHz, LOL(dBFS) w/o cal, LOL(dBFS) w/ cal");
                for (int i = 0; i <= param.numSteps; i++)
                {
                    file.WriteLine(i + "," + loLeakageXY[i, 0].ToString() + "," + loLeakageXY[i, 1].ToString() + "," + loLeakageXY[i, 2].ToString());
                }
            }
#endif


        }

        [Test, Sequential]
        [NUnit.Framework.Category("TX")]
        public static void QECHarmonicSweep([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
        {
            InitCalSweepTests obj = new InitCalSweepTests();
            obj.QECTestsInit();
            //Retrieve Profile Information, samplingFreq_Hz,  ProfileBW, LO Frequency Information
            double[] profileInfo = new double[3];
            double backoff = -15;
            int atten = 0;
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
            double SwpMinFreq = freqTxLo_MHz - (2 * (profileBandwidth_MHz / 2));
            double SwpMaxFreq = freqTxLo_MHz + (2 * (profileBandwidth_MHz / 2));
            double SwpSigAmp = -40;
            int SwpNumSteps = 200;
            SwpParamStruct param = new SwpParamStruct(SwpMinFreq, SwpMaxFreq, SwpSigAmp, SwpNumSteps);
            double SwpStepSz = (param.freqMax - param.freqMin) / param.numSteps;
            Console.WriteLine("SwpMinFreq (MHz): " + SwpMinFreq);
            Console.WriteLine("SwpMaxMax (MHz): " + SwpMaxFreq);
            Console.WriteLine("SwpSigAmp (MHz): " + SwpSigAmp);

            //Define Data Array for storing Fundamental Amplitue, LoLeakage and ImageAmplitude
            //double[,] fundAmpXY = new double[param.numSteps, 2];
            double[,] harmonicAmpnoCal = new double[param.numSteps + 1, 4];
            double[,] harmonicAmpCal = new double[param.numSteps + 1, 4];

            double[,] loLeakageXYcal = new double[param.numSteps + 1, 2];
            double[,] loLeakageXYdif = new double[param.numSteps + 1, 2];

            //double[,] imageAmpXY = new double[param.numSteps, 2];

            //Connect to Signal Generator 
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
                              "Image Amplitude (dBm)");
#endif
            //Test Sequence
            //Sweep Thru Passband
            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double harmonicAmp;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;

                if (offsetFreq_Hz == 0)
                {

                    harmonicAmpnoCal[i, 0] = offsetFreq_Mhz;
                    harmonicAmpnoCal[i, 1] = harmonicAmpnoCal[i - 1, 1];
                    harmonicAmpnoCal[i, 2] = harmonicAmpnoCal[i - 1, 2];
                    harmonicAmpnoCal[i, 3] = harmonicAmpnoCal[i - 1, 3];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);
                //Take Measurements from PXA

                pxa.SetMarker(1, freqTxLo_MHz + (offsetFreq_Hz / 1000000)); 
                pxa.SetMarker(2, freqTxLo_MHz + 2 * (offsetFreq_Hz / 1000000));
                pxa.SetMarker(3, freqTxLo_MHz + 3 * (offsetFreq_Hz / 1000000));
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                harmonicAmpnoCal[i, 0] = offsetFreq_Mhz;
                harmonicAmp = Double.Parse(pxa.GetMarker(1), style);
                harmonicAmpnoCal[i, 1] = harmonicAmp;
                harmonicAmp = Double.Parse(pxa.GetMarker(2), style);
                harmonicAmpnoCal[i, 2] = harmonicAmp;
                harmonicAmp = Double.Parse(pxa.GetMarker(3), style);
                harmonicAmpnoCal[i, 3] = harmonicAmp;

#if WR_RES_TO_CONSOLE //Optional printout for text based readout in test output window
                Console.WriteLine(offsetFreq_Mhz + ":   " + harmonicAmp + ",   ");

#endif

            }


            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.radioOff();
            Link.Disconnect();


            obj.AllCalsInit();
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            Link.Mykonos.setTx1Attenuation(0);
            Link.Mykonos.radioOn();
            Link.Disconnect();

            for (int i = 0; (i <= param.numSteps); i++)
            {
                //Calculate Test Tone And Generate
                double offsetFreq_Mhz = param.freqMin + (i * SwpStepSz) - (freqTxLo_MHz);
                double testFreq_MHz = freqTxLo_MHz + offsetFreq_Mhz;
                double offsetFreq_Hz = offsetFreq_Mhz * 1000000;
                double harmonicAmp;

                if (offsetFreq_Hz == 0)
                {

                    harmonicAmpCal[i, 0] = offsetFreq_Mhz;
                    harmonicAmpCal[i, 1] = harmonicAmpCal[i - 1, 1];
                    harmonicAmpCal[i, 2] = harmonicAmpCal[i - 1, 2];
                    harmonicAmpCal[i, 3] = harmonicAmpCal[i - 1, 3];
                    //loLeakageXY[i, 3] = loLeakageXY[i, 1] - loLeakageXY[i, 2];

                    continue;
                }
                Helper.GenerateTxTone(Mykonos.TXCHANNEL.TX1_TX2, profileInfo, offsetFreq_Hz, backoff);

                pxa.SetMarker(1, freqTxLo_MHz + (offsetFreq_Hz / 1000000));
                pxa.SetMarker(2, freqTxLo_MHz + 2 * (offsetFreq_Hz / 1000000));
                pxa.SetMarker(3, freqTxLo_MHz + 3 * (offsetFreq_Hz / 1000000));
                //pxa.HoldAverage();
                System.Threading.Thread.Sleep(500);
                harmonicAmpCal[i, 0] = offsetFreq_Mhz;
                harmonicAmp = Double.Parse(pxa.GetMarker(1), style);
                harmonicAmpCal[i, 1] = harmonicAmp;
                harmonicAmp = Double.Parse(pxa.GetMarker(2), style);
                harmonicAmpCal[i, 2] = harmonicAmp;
                harmonicAmp = Double.Parse(pxa.GetMarker(3), style);
                harmonicAmpCal[i, 3] = harmonicAmp;

    }

#if WR_RES_TO_PDF
            //Graph Data and Save in PDF Form
            var doc1 = new Document();
            string path = TxRfTests.ResPath + "QECInitSweep";
            TxRfTests instance = new TxRfTests();
            string[] pcbInfo;
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
            string[] loLeakageLabels = new string[] { "Harmonic Amplitude Versus Offset Tone Frequency without Calibrations",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "1st Harmonic",  "2nd Harmonic",  "3rd Harmonic" };
            string[] loLeakageLabelscal = new string[] { "Harmonic Amplitude Versus Offset Tone Frequency with Calibrations",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "1st Harmonic",  "2nd Harmonic",  "3rd Harmonic" };
            string[] loLeakageLabelsdif = new string[] { "Image Amplitude Versus Offset Tone Frequency Difference",
                                                "Offset Tone Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace Amplitude" };
            pcbInfo = Helper.PcbInfo((settings.txPllLoFreq_Hz / 1000000000.0).ToString(), (settings.rxPllLoFreq_Hz / 1000000000.0).ToString(), settings.mykSettings.txProfileName, settings.mykSettings.rxProfileName, backoff.ToString(), atten.ToString());
            // pcbInfo = Helper.PcbInfo();
            //container[0] = Helper.MakeChartObject(fundAmpXY, fundAmpLabels, path);
            container[0] = Helper.MakeChartObject(harmonicAmpnoCal, loLeakageLabels, path);
            container[1] = Helper.MakeChartObject(harmonicAmpCal, loLeakageLabelscal, path);
            //container[1] = Helper.MakeChartObject(loLeakageXYcal, loLeakageLabelscal, path);
            //container[2] = Helper.MakeChartObject(loLeakageXYdif, loLeakageLabelsdif, path);

            //container[2] = Helper.MakeChartObject(imageAmpXY, imageAmpLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            //Open Result PDF            
            System.Diagnostics.Process.Start(path + ".pdf");
#endif
#if WR_RES_TO_TXT
            // Write data to txt file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path + ".txt"))
            {
                file.WriteLine("Sample,  Frequency MHz, LOL(dBFS) w/o cal, LOL(dBFS) w/ cal");
                for (int i = 0; i <= param.numSteps; i++)
                {
                    file.WriteLine(i + "," + harmonicAmpnoCal[i, 0].ToString() + "," + harmonicAmpnoCal[i, 1].ToString() + "," + harmonicAmpnoCal[i, 2].ToString());
                }
            }
#endif


        }


    }
}
