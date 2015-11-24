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

using iTextSharp.text;
using iTextSharp.text.pdf;
using AdiCmdServerClient;
using DotNETRoutines;
using AdiMathLibrary;

namespace mykonosUnitTest
{
    //Use ParamStructs to pass input parameters
    public struct SwpParamStruct
    {
        public double freqMin, freqMax, amplitude;
        public int numSteps;

        public SwpParamStruct(double min, double max, double amp, int steps)
        {
            freqMin = min;
            freqMax = max;
            amplitude = amp;
            numSteps = steps;
        }
    }


    public class MeasurementEquipment
    {
        public string ESGAddress = "TCPIP0::123.255.255.25::inst0::INSTR";
        public string PXAAddress = "TCPIP0::123.255.255.55::inst0::INSTR";

    }
    [TestFixture]
    [NUnit.Framework.Category("Equipment")]
    public class MeasurementEquipmentTests
    {
        public const string ESGAddress = "TCPIP0::123.255.255.25::inst0::INSTR";
        public const string PXAAddress = "TCPIP0::123.255.255.55::inst0::INSTR";

        /// <summary>
        /// CheckTestObject was written by Rick Matsick as a part of testing 
        /// the visaWrapper class
        /// Checks Connection to Test Equipment as Ni Visa Test Object
        /// Note: test.close causes issue for ESG Device
        /// </summary>

        [Test, Sequential]
        public void CheckTestObject([Values(ESGAddress, PXAAddress)]string EquipAddress)
        {

            Console.WriteLine("--TEST STARTED--");
            VisaCommands testObj = new VisaCommands(EquipAddress);

            Console.WriteLine(testObj.TerminationCharacter);
            Console.WriteLine(testObj.TermCharEnable);
            Console.WriteLine(testObj.Query("*IDN?\n"));

            //testObj.Close();

            Console.WriteLine("--TEST FINISHED--");
        }

        /// <summary>
        /// TestESG() was written by Rick Matsick as a part of testing the visaWrapper clss
        /// </summary>

        [Test]
        public static void TestESG()
        {

            SG_AgilentESG esg = new SG_AgilentESG(ESGAddress);
            Console.WriteLine(ESGAddress);
            Console.WriteLine(esg.Identify());

            //set init values
            esg.SetFrequency(2600.1234567);
            esg.SetAmplitude(-50.2);
            esg.SetRfOutput(false);

            for (int i = 0; i < 100; i++)
            {
                esg.SetFrequency(2600.0 + (0.1 * i));
                System.Threading.Thread.Sleep(200);
                Console.WriteLine(esg.GetFrequency());
                System.Threading.Thread.Sleep(200);
            }

           
        }

        /// <summary>
        /// TestPXA was written by Rick Matsick as a part of testing the visaWrapper clss
        /// 
        /// </summary>
        [Test]
        public static void TestPXA()
        {
            double pkAmp = 0;
            double pkFreq = 0;
            double[] data = new double[1001];
            

            Console.WriteLine("--PXA TEST STARTED--");


            SA_AgilentPXA pxa = new SA_AgilentPXA(PXAAddress);
            Console.WriteLine(PXAAddress);
            Console.WriteLine(pxa.Identify());

            pxa.SetFreq(2600);
            pxa.SetCenterFreq(10);
            pxa.SetFreqSpan(100);
            pxa.SetRefLevel(10);
            pxa.SetAtten(20);
            pxa.SetRBW_VBW(100, 1);
            pxa.SetCenterSpan(10, 100, 0);

            pxa.MeasPeakPower(ref pkAmp, ref pkFreq);
            Console.WriteLine("Peak Amplitude = " + pkAmp);
            Console.WriteLine("Peak Frequency = " + pkFreq);

            pxa.SetMarker(1, 0);
            Console.WriteLine(pxa.GetMarker(1));

            pxa.SetDetector(SA_AgilentPXA.DetectorTypes.NORM);

            pxa.GetPlotData(out data);

            Console.WriteLine("X    Y");
            Console.WriteLine(" ");

            for (int i = 0; i < data.Length; i++)
            {
              Console.WriteLine("{0}  {1}", i, data[i]);
            }

           pxa.Close();

            Console.WriteLine("--PXA TEST FINISHED--");
        }

        /// <summary>
        /// The CableTest provides a means to measure the cable loss (dB) versus frequency exhibited between a ESG signal source and 
        /// PXA signal analyzer through a cable.
        /// This was also my means of testing the pdf printing methods and visaWrapper classes. 
        /// This data set could be subtracted out of other data sets to obtain more accurate data, however, that is a minor detail. Will likely
        /// not be needed until a 'golden platform' is established
        /// </summary>

        [Test]
        public static void CableTest()
        {
            //TEST PARAMETER DEFINITIONS
            int[] amplitudes = new int[1] { -20 }; //Hard coded values for amplitude settings
            SwpParamStruct param = new SwpParamStruct(2000, 4000, amplitudes[0], 12); //Hard coded values, Initialize param structure

            string[] seriesName = new string[amplitudes.Length];
            double[] data = new double[param.numSteps];
            double[] freqAxis = new double[param.numSteps];
            double[,] freqAmplitudeData = new double[param.numSteps, amplitudes.Length + 1]; //Always a 2D array              


            //MEASUREMENT EQUIPMENT SETUP
            NumberStyles style = NumberStyles.AllowExponent | NumberStyles.Number;
            ///ESG Setup
            SG_AgilentESG esg = new SG_AgilentESG(ESGAddress);
            Console.WriteLine(esg.Identify());

            //PXA Setup  
            Console.WriteLine("--PXA TEST STARTED--");
            SA_AgilentPXA pxa = new SA_AgilentPXA(PXAAddress);
            Console.WriteLine("--CABLE TEST STARTED--");


            for (int j = 0; j < amplitudes.Length; j++)
            {
                param.amplitude = amplitudes[j];
                string seriesLabel = "Input Amplitude " + amplitudes[j] + "dBm";
                seriesName[j] = seriesLabel;

                //set init values on ESG
                esg.SetFrequency(param.freqMin);
                esg.SetAmplitude(param.amplitude);
                esg.SetRfOutput(true);

                for (int i = 0; i < param.numSteps; i++)
                {
                    double test_freq = param.freqMin + i * (param.freqMax - param.freqMin) / param.numSteps;
                    esg.SetFrequency(test_freq);
                    pxa.SetCenterSpan(test_freq, 100, 0);               //MHz
                    pxa.SetAtten(10);
                    pxa.SetRefLevel(-10);
                    pxa.SetMarker(1, test_freq);                        //Sets marker to frequency of interest
                    pxa.HoldAverage();
                    System.Threading.Thread.Sleep(500);
                    data[i] = Double.Parse(pxa.GetMarker(1), style);            //Rewrite this... it's verbose
                    freqAxis[i] = test_freq;

                    freqAmplitudeData[i, 0] = test_freq;
                    freqAmplitudeData[i, j + 1] = Double.Parse(pxa.GetMarker(1), style);
                }
            }

            // PRINT OUTPUT FILES
            string path = @"..\..\..\TestResults\CableTest";           //Sends output data to the TestOutputs folder
            //Output Data to a text file

            //Output Charts to a pdf file
            var doc1 = new Document();

            string[] chartLabels = new string[] { "Received ESG Amplitude on PXA (Cable Test)",
                                                "ESG Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "ESG Amplitude (dBm): " + amplitudes[0] };
            string[] headerStrings = new string[] { "Cable Test Output Graph",
                                                    "Mykonos API Regression Testing Development",
                                                    Helper.GetTimestamp()};

            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];
            container[0] = Helper.MakeChartObject(freqAmplitudeData, chartLabels, path + ".pdf");
            Helper.AddAllChartsToPdf(container, path + ".pdf", headerStrings);
            //Open Result pdf
            System.Diagnostics.Process.Start(path + ".pdf");
        }


        /// <summary>
        /// FakeCableTest() is a test bench I used to verify the functionality of the graphing functions
        /// It uses a random number generator in order to create some arbitrary values to plot
        /// </summary>

        [Test]
        public static void FakeCableTest()
        {
            //Parameter Initialization
            SwpParamStruct param = new SwpParamStruct(300, 3000, -40, 25);
            int numIterations = 2;
            double[] data = new double[param.numSteps];                       //Define data array
            double[] freqAxis = new double[param.numSteps];                  //Define the frequency array
            double[,] xyDouble = new double[param.numSteps, numIterations + 1];

            //FakeCableTest Code Body
            for (int j = 0; j < numIterations; j++)
            {
                for (int i = 0; i < param.numSteps; i++)
                {
                    double test_freq = param.freqMin + i * (param.freqMax - param.freqMin) / param.numSteps;

                    xyDouble[i, 0] = test_freq;
                    xyDouble[i, j + 1] = Helper.GetRandomNum(-50, -30);
                }
            }

            //Output Charts to a pdf file
            string path = @"..\..\..\TestResults\FakeCableTestOutput";           //Sends output data to the TestOutputs folder
            var doc1 = new Document();
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[1];

            string[] chartLabels = new string[] { "Random Number Generator",
                                                "Arbitrary Frequency (MHz)",
                                                "Amplitude (dBm)",
                                                "Trace 1",
                                                "Trace 2"};
            container[0] = Helper.MakeChartObject(xyDouble, chartLabels, path);
            Helper.AddAllChartsToPdf(container, path + ".pdf", chartLabels);

            ////Open Result PDF
            System.Diagnostics.Process.Start(path + ".pdf");

        }
        /// <summary>
        /// TestTxToneGenerator() only calls a function to generate a Tx tone. There has been some inconsistencies, and this test method 
        /// provides an easy means to determine if a given tone generation works properly. 
        /// 
        /// Bugs: For spurs increasingly further away from the LO tone, there are extra spurs near all frequency points of interest
        /// </summary>
        [Test,Sequential]
        public static void TestTxToneGenerator([Values(Mykonos.TXCHANNEL.TX1, Mykonos.TXCHANNEL.TX2)]Mykonos.TXCHANNEL channel)
        {
            UInt64 txPllLoFrequency_Hz = 5650000000 / 1000000; ;
            UInt32 txiqRate_kHz = 245760;
            UInt32 primarySigBw_Hz = 75000000 / 1000000; ;

            double[] profileInfo = new double[3];
            profileInfo[0] = txPllLoFrequency_Hz;
            profileInfo[1] = txiqRate_kHz;
            profileInfo[2] = primarySigBw_Hz;
            Helper.GenerateTxTone(channel, profileInfo, 12000000, 0);
        }
#if false //This Equipment is not available in SQA Lab.
        /// <summary>
        /// TestPxiSlot3 was written by Rick Matsick as a part of testing the visaWrapper clss
        /// This function must be used with a National Instruments PXI rack with appropriate slots configured
        /// </summary>
        [Test]
        [TestCategory("Measurement Equipment Tests")]
        public static void TestPxiSlot3()
        {
            Console.WriteLine("--PXI SLOT 3 TEST STARTED--");

            string sAddr = "TCPIP0::192.168.1.110::6341::SOCKET";
            SG_NatInstPXI pxi = new SG_NatInstPXI(sAddr);
            Console.WriteLine(sAddr);

            Console.WriteLine(pxi.Identify());

            //set init values
            pxi.SetFreqSlot3(2600.1234567);
            pxi.SetAmpSlot3(-50.2);
            pxi.SetRfOutSlot3(false);
            pxi.Close();

            Console.WriteLine("--PXI SLOT 3 TEST FINISHED--");
        }


        /// <summary>
        /// TestPxiSlot16 was written by Rick Matsick as a part of testing the visaWrapper clss
        /// This function must be used with a National Instruments PXI rack with appropriate slots configured
        /// </summary>
        [Test]
        [TestCategory("Measurement Equipment Tests")]
        public static void TestPxiSlot16()
        {
            Console.WriteLine("--PXI SLOT 16 TEST STARTED--");

            string sAddr = "TCPIP0::192.168.1.110::6341::SOCKET";
            SG_NatInstPXI pxi = new SG_NatInstPXI(sAddr);
            Console.WriteLine(sAddr);
            Console.WriteLine(pxi.Identify());

            //set init values
            pxi.SetFreqSlot16(2600.1234567);
            pxi.SetAmpSlot16(-50.2);
            pxi.SetRfOutSlot16(false);

            for (int i = 0; i < 100; i++)
            {
                pxi.SetFreqSlot16(2600.0 + (0.1 * i));
                System.Threading.Thread.Sleep(200);
            }

            Console.WriteLine(pxi.GetFreqSlot16());
            Console.WriteLine(pxi.GetAmpSlot16());
            Console.WriteLine(pxi.GetRfOutSlot16());

            pxi.Close();

            Console.WriteLine("--PXI SLOT 16 TEST FINISHED--");
        }
#endif
    }
}
