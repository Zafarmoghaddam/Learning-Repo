using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using AdiCmdServerClient;
using NUnit.Framework;
using DotNETRoutines;
using DotNETRoutines.WindowsAPI;
using AdiMathLibrary;


namespace mykonosUnitTest
{
    static class Helper
    {

        public static double GetRandomNum(double min, double max)
        {
            Random random = new Random();
            System.Threading.Thread.Sleep(10);
            return random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// MakeChartObject(...) uses a 2D double array and creates a chart object based on the inputs. 
        /// 
        /// </summary>
        /// <param name="xyDouble"></param> The function requires the "X" variable to be in the first column, and then the Y1 variable in the next col,
        /// Y2 in the next col, Y3 and so forth. 
        /// <param name="chartLabels"></param> Chartlabels are used to provide [Title, XAxisLabel, YAxisLabel, SeriesName1, SeriesName2...]. The number of series labels
        /// must be equal to the number of columns in (xyDouble - 1)
        /// <param name="path"></param> Path should be provided to set the destination output for a TEXT File. This method alone does NOT create the pdf, only the images
        /// to be put into the PDF.
        /// <returns></returns>
        public static iTextSharp.text.Image MakeChartObject(double[,] xyDouble, string[] chartLabels, string path)
        {
            // create the chart
            var chart = new Chart();
            chart.Height = 300;
            chart.Width = 500;
            chart.Titles.Add(chartLabels[0]);
            var chartArea = new ChartArea();
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.Black;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;
            chartArea.AxisX.MinorGrid.Enabled = true;                               //Major grids enabled by default...
            chartArea.AxisY.MinorGrid.Enabled = true;
            chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisX.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8);
            chartArea.AxisX.Title = chartLabels[1];
            chartArea.AxisY.Title = chartLabels[2];
            chart.ChartAreas.Add(chartArea);

            string[] xy_string = new string[xyDouble.GetLength(0)];
            // Add the datapoints
            int width = xyDouble.GetLength(0);   //width = number of rows
            int height = xyDouble.GetLength(1);  //Height = number of columns
            for (int i = 0; i < (height - 1); i++)  //Plot all coordinates for one trace. i => column, j=> row
            {
                var series = new Series();
                series.Name = chartLabels[i + 3];
                series.ChartType = SeriesChartType.FastLine;
                chart.Series.Add(series);

                for (int j = 0; j < (width); j++)
                {
                    chart.Series[chartLabels[i + 3]].Points.AddXY(xyDouble[j, 0], xyDouble[j, i + 1]);

                    if (i == 0)
                        xy_string[j] = System.Convert.ToString(xyDouble[j, 0]) + " " + System.Convert.ToString(xyDouble[j, i + 1]);
                    else
                        xy_string[j] = xy_string[j] + " " + System.Convert.ToString(xyDouble[j, i + 1]);
                }
                // Create a new legend called "Legend2".
                chart.Legends.Add(new Legend(chartLabels[i + 3]));
                chart.Legends[chartLabels[i + 3]].Docking = 0;           //0 docks the legend to the top of the chart

            }

            //chart.ChartAreas["ChartArea1"].RecalculateAxesScale();
            chart.ChartAreas["ChartArea1"].RecalculateAxesScale();
            chart.ChartAreas["ChartArea1"].AxisX.Minimum = xyDouble[0, 0]; // X axis min
            chart.ChartAreas["ChartArea1"].AxisX.Maximum = xyDouble[width - 1, 0]; // X axis max
            //chart.ChartAreas["ChartArea1"].AxisY.Minimum = -25;// MinFundAmp - 1; // Y axis min
            //chart.ChartAreas["ChartArea1"].AxisY.Maximum = -15;//MaxFundAmp +1; // Y axis max
            System.IO.File.WriteAllLines(path + ".txt", xy_string);

            // write output file
            var chartImage = new MemoryStream();
            chart.SaveImage(chartImage, ChartImageFormat.Png);
            iTextSharp.text.Image Chart_Image = iTextSharp.text.Image.GetInstance(chartImage.GetBuffer());      //Consider returning this Chart_Image class

            return Chart_Image;
        }
        /// <summary>
        /// This is an overload function for MakeChartObject that allows the user to declare custom X and Y axis limits
        /// Allows for easier reading of printed out functions, particularly for data that may appear strange due to outliers
        /// 
        /// </summary>
        /// <param name="xyDouble"></param> The function requires the "X" variable to be in the first column, and then the Y1 variable in the next col,
        /// Y2 in the next col, Y3 and so forth. 
        /// <param name="chartLabels"></param> Chartlabels are used to provide [Title, XAxisLabel, YAxisLabel, SeriesName1, SeriesName2...]. The number of series labels
        /// must be equal to the number of columns in (xyDouble - 1)
        /// <param name="path"></param> Path should be provided to set the destination output for a TEXT File. This method alone does NOT create the pdf, only the images
        /// to be put into the PDF.
        /// <returns></returns>
        /// <param name="chartAxes"></param> There must be FOUR doubles in the array input chartAxes of format [XMIN, XMAX, YMIN, YMAX]
        /// <returns></returns>
        public static iTextSharp.text.Image MakeChartObject(double[,] xyDouble, string[] chartLabels, string path, double[] chartAxes)
        {
            // create the chart
            var chart = new Chart();
            chart.Height = 300;
            chart.Width = 500;
            chart.Titles.Add(chartLabels[0]);
            var chartArea = new ChartArea();
            chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.Black;
            chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.Black;
            chartArea.AxisX.MinorGrid.Enabled = true;                               //Major grids enabled by default...
            chartArea.AxisY.MinorGrid.Enabled = true;
            chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisX.MinorGrid.LineDashStyle = ChartDashStyle.Dash;
            chartArea.AxisX.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new System.Drawing.Font("Arial", 8);
            chartArea.AxisY.LabelStyle.Font = new System.Drawing.Font("Arial", 8);
            chartArea.AxisX.Title = chartLabels[1];
            chartArea.AxisY.Title = chartLabels[2];
            chart.ChartAreas.Add(chartArea);

            string[] xy_string = new string[xyDouble.GetLength(0)];
            // Add the datapoints
            int width = xyDouble.GetLength(0);   //width = number of rows
            int height = xyDouble.GetLength(1);  //Height = number of columns
            for (int i = 0; i < (height - 1); i++)  //Plot all coordinates for one trace. i => column, j=> row
            {
                var series = new Series();
                series.Name = chartLabels[i + 3];
                series.ChartType = SeriesChartType.FastLine;
                chart.Series.Add(series);

                for (int j = 0; j < (width); j++)
                {
                    chart.Series[chartLabels[i + 3]].Points.AddXY(xyDouble[j, 0], xyDouble[j, i + 1]);

                    if (i == 0)
                        xy_string[j] = System.Convert.ToString(xyDouble[j, 0]) + " " + System.Convert.ToString(xyDouble[j, i + 1]);
                    else
                        xy_string[j] = xy_string[j] + " " + System.Convert.ToString(xyDouble[j, i + 1]);
                }
                // Create a new legend called "Legend2".
                chart.Legends.Add(new Legend(chartLabels[i + 3]));
                chart.Legends[chartLabels[i + 3]].Docking = 0;           //0 docks the legend to the top of the chart

            }

            chart.ChartAreas["ChartArea1"].RecalculateAxesScale();
            chart.ChartAreas["ChartArea1"].AxisX.Minimum = chartAxes[0]; // X axis min
            chart.ChartAreas["ChartArea1"].AxisX.Maximum = chartAxes[1]; // X axis max
            chart.ChartAreas["ChartArea1"].AxisY.Minimum = chartAxes[2]; // Y axis min
            chart.ChartAreas["ChartArea1"].AxisY.Maximum = chartAxes[3]; // Y axis max

            System.IO.File.WriteAllLines(path + ".txt", xy_string);

            // write output file
            var chartImage = new MemoryStream();
            chart.SaveImage(chartImage, ChartImageFormat.Png);
            iTextSharp.text.Image Chart_Image = iTextSharp.text.Image.GetInstance(chartImage.GetBuffer());

            return Chart_Image;
        }



        /// <summary>
        /// The AddAllChartsToPdf(...) function takes an array of iTextSharp chart images and writes them to a pdf. 
        /// headerStrings is an array of strings that are printed cell by cell to individual lines in the pdf
        /// There is a concern with image quality in the pdf generation that needs to be resolved. 
        /// 
        /// Please consult the iTextSharp documentation online for more options about how to create the pdfs. 
        /// </summary>
        /// <param name="chartImage"></param> Generate from CreateChartObject function
        /// <param name="path"></param> The path the pdf will be printed to
        /// <param name="headerStrings"></param> An array of strings that are put in the beginning of the pdf

        public static void AddAllChartsToPdf(iTextSharp.text.Image[] chartImage, string path, string[] headerStrings)
        {
            var doc1 = new Document();
            PdfWriter.GetInstance(doc1, new FileStream(path, FileMode.Append, FileAccess.Write));

            doc1.Open();
            for (int j = 0; j < headerStrings.Length; j++)
            {
                doc1.Add(new Paragraph(headerStrings[j]));
            }
            for (int i = 0; i < chartImage.Length; i++)
            {
                //Chart_Image[i].ScalePercent(75f);
                doc1.Add(chartImage[i]);
            }
            doc1.Close();
        }

        /// <summary>
        /// GenerateTxTone generates a CW tone at the desired offsetFreq from the LO. 
        /// The input offsetFreq must be expressed in Hz. 
        /// Amplitude variations have not been tested extensively, 
        /// not has every possible input for the offsetFreq. It does not respond well to 0 frequency. 
        /// The function will not generate ANY arbitrary signal. 
        /// It will generate tones at increments of the binSize
        /// </summary>
        /// <param name="settings"></param> 
        /// <param name="offsetFreq"></param> Unit is Hz
        /// <param name="amplitude"></param> Use 0dBm for most purposes
        public static void GenerateTxTone(Mykonos.TXCHANNEL channel, double[] profileInfo, double offsetFreq, double amplitude)
        {
            double txIqDataRate_kHz = profileInfo[0];
            // double freqTxLo_MHz = profileInfo[1] / 1000000; 
            // double profileBandwidth_MHz = profileInfo[2] / 1000000;


            const int NUM_SAMPLES = 32768;
            double binSize_kHz = txIqDataRate_kHz / NUM_SAMPLES; //This is the resolution of the generated tones
            double targetBin = System.Math.Floor(offsetFreq / 1000 / binSize_kHz);
            double actualOffsetFreq_MHz = targetBin * binSize_kHz * 1000;

            double[] TxComplexData;
            //int CIFRTxDataToneSize = 16384 * 2; //Default
            //int CIFRTxBufferSize = 16384 * 2; //Default

            DotNETRoutines.SigGen.GeneratorParameters Gen;
            Gen = new DotNETRoutines.SigGen.GeneratorParameters(actualOffsetFreq_MHz, amplitude, 0.0);
            double absOffsetFreq = System.Math.Abs(offsetFreq);
            Gen.freq = actualOffsetFreq_MHz;

            TxComplexData = DotNETRoutines.SigGen.GetSineWave(Gen, NUM_SAMPLES, txIqDataRate_kHz * 1000, amplitude, true, 0);

            // De-interleave the output of the sine wave generator i.e. split into i and q data
            double[] Tx1iData = new double[NUM_SAMPLES];
            double[] Tx1qData = new double[NUM_SAMPLES];

            for (int i = 0; i < TxComplexData.Length; i += 2)
            {
                Tx1iData[i / 2] = TxComplexData[i];
                Tx1qData[i / 2] = TxComplexData[i + 1];
            }

            short[] dataTx1i_short = new short[Tx1iData.Length];
            for (int i = 0; i < Tx1iData.Length; i++)
            {
                dataTx1i_short[i] = (short)(Tx1iData[i] * System.Math.Pow(2, 15) - 1);
                if (Tx1iData[i] >= 1)
                    dataTx1i_short[i] = (short)(System.Math.Pow(2, 15) - 1);
                if (Tx1iData[i] <= -1)
                    dataTx1i_short[i] = (short)(-System.Math.Pow(2, 15) + 1);

            }

            short[] dataTx1q_short = new short[Tx1qData.Length];
            for (int i = 0; i < Tx1iData.Length; i++)
            {
                dataTx1q_short[i] = (short)(Tx1qData[i] * System.Math.Pow(2, 15) - 1);
                if (Tx1qData[i] >= 1)
                    dataTx1q_short[i] = (short)(System.Math.Pow(2, 15) - 1);
                if (Tx1qData[i] <= -1)
                    dataTx1q_short[i] = (short)(-System.Math.Pow(2, 15) + 1);
            }

            Int16[] txData = new Int16[NUM_SAMPLES * 2];

            for (int i = 0; i < (NUM_SAMPLES); i++)
            {
                txData[2 * i] = dataTx1i_short[i];
                txData[2 * i + 1] = dataTx1q_short[i];
            }

            //Send data from fpga to the transmitter
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
            try
            {
                Link.FpgaMykonos.stopTxData();
                Link.fpgaWrite(0x200, 0);
                Link.fpgaWrite(0x14, 0x18); //reset Tx data movers
                Link.fpgaWrite(0x14, 0x00); //reset Tx data movers
                Link.fpgaWrite(0x400, 0x00200); //Enable all Rx and Tx lanes in FPGA
                Link.FpgaMykonos.enableTxDataPaths(FpgaMykonos.TX_DATAPATH.DISABLE);

                //Console.WriteLine("FPGA 0x220: " + Link.fpgaRead(0x220));

                if (channel == Mykonos.TXCHANNEL.TX1)
                {
                    Link.writeRam(Enums.FPGA_CHANNEL.TX1, 0, txData);
                    Link.FpgaMykonos.setTxTransmitSamples(FpgaMykonos.TXBUFFER.TX1_DM, NUM_SAMPLES * 2);
                }
                else if (channel == Mykonos.TXCHANNEL.TX2)
                {
                    Link.writeRam(Enums.FPGA_CHANNEL.TX2, 0, txData);
                    Link.FpgaMykonos.setTxTransmitSamples(FpgaMykonos.TXBUFFER.TX2_DM, NUM_SAMPLES * 2); //Newly added
                }
                else if (channel == Mykonos.TXCHANNEL.TX1_TX2)
                {
                    Link.writeRam(Enums.FPGA_CHANNEL.TX1, 0, txData);
                    Link.writeRam(Enums.FPGA_CHANNEL.TX2, 0, txData);
                    Link.FpgaMykonos.setTxTransmitSamples(FpgaMykonos.TXBUFFER.TX1_DM, NUM_SAMPLES * 2);
                    Link.FpgaMykonos.setTxTransmitSamples(FpgaMykonos.TXBUFFER.TX2_DM, NUM_SAMPLES * 2); //Newly added
                }
                Link.FpgaMykonos.setTxTransmitMode(1);
                Link.FpgaMykonos.enableTxDataPaths(FpgaMykonos.TX_DATAPATH.TX1_TX2);
                Link.FpgaMykonos.startTxData();


#if false
                short[] fpgaData = Link.readRam(Enums.FPGA_CHANNEL.TX1, 0, NUM_SAMPLES * 2);
                path = @"..\..\..\..\TestOutputs\TESTFFTs2";
                Helper.GenerateFft(fpgaData, txIqDataRate_kHz * 1000, path);
#endif
#if false
                for (int i = 0; i < tx1Data.Length; i++)
                {
                    if (tx1Data[i] != fpgaData[i])
                    {
                        Console.WriteLine("Tx1 Data: " + tx1Data[i]);
                        Console.WriteLine("Readback Tx1 Data: " + fpgaData[i]);
                    }
                }
#endif //Print TxData FPGA Data mis-match for debug
            }
            catch
            {
                Console.WriteLine("FPGA Tone Generator Error");
                throw;
            }
            finally
            {
                Link.Disconnect();
            }


        }

        /// <summary>
        /// Generates part of the headerString that can be appended to the pdf document
        /// It acquires information regarding fpga version, commmand server version and board id. 
        /// Returns a string array
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns> Returns a string array of pcb information
        public static string[] PcbInfo(string txLO = "", string rxLO = "", string txprofile = "", string rxprofile = "", string backoff = "", string atten = "")
        {
            zc706TcpipClient hw = zc706TcpipClient.Instance;

            string[] outputStringArray = null;

            try
            {
                hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                hw.Write("*IDN?");
                String readString = hw.Read();
                String timeStamp = GetTimestamp();
                string pcbInfo = hw.Query("GetPcbInfo").ToString();
                string versionInfo = hw.Query("Version").ToString();
                string tempString = string.Concat(pcbInfo, "\n" + readString);
                if (txLO != "")
                {
                    tempString = string.Concat(tempString, "\nTx LO Frequency: " + txLO + "MHz\n");
                    tempString = string.Concat(tempString, "\nRx LO Frequency: " + rxLO + "MHz\n");
                    tempString = string.Concat(tempString, "\nTX Profile: " + txprofile.Replace(',', '/'));
                    tempString = string.Concat(tempString, "\nRX Profile: " + rxprofile.Replace(',', '/'));
                    tempString = string.Concat(tempString, "\nTX Backoff: " + backoff + "dBFS");
                    tempString = string.Concat(tempString, "\nRX Attenuation: " + atten + "dB\n");
                }
                tempString = string.Concat(tempString, timeStamp);
                Console.WriteLine(tempString);
                //Console.ReadLine();
                char[] delimiterChars = { ';', ',' };
                tempString = tempString.Replace("Devices", "Devices Inc.").Replace("ADI Transceiver", "");
                outputStringArray = tempString.Split(delimiterChars);
                outputStringArray = outputStringArray.Where(w => w != outputStringArray[2]).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                hw.Disconnect();
            }

            return outputStringArray;
        }
        /// <summary>
        /// Simple little function that obtains a timestamp. It is used in Helper.PcbInfo
        /// </summary>
        /// <returns></returns> Returns a single string
        public static String GetTimestamp()
        {
            /* This function returns the date and time of the function call. 
             * 
             */

            DateTime time = DateTime.Now;
            return time.ToString();
        }

        /// <summary>
        /// Generates an array of doubles with Tx specific profile information
        /// Format of output [iqRate, TxPLL Frequency, Primary signal bandwidth]
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns> Returns a double array, [txiqRate_kHz, txPllLoFrequency_Hz, primarySigBw_Hz]
        public static double[] SetTxProfileInfo()
        {
            const byte WRITE_SETTINGS = 1;
            const byte READ_SETTINGS = 0;
            double[] output = new double[3];

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;

            try
            {
                Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);



                Mykonos.TXATTENSTEPSIZE txStepSize = Mykonos.TXATTENSTEPSIZE.TXATTEN_0P05_DB;
                Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
                byte txPllUseExternalLo = 0;
                UInt64 txPllLoFrequency_Hz = 5650000000;
                UInt16 tx1Atten_mdB = 10000;
                UInt16 tx2Atten_mdB = 10000;
                Link.Mykonos.init_txsettings(READ_SETTINGS,
                                                ref txStepSize,
                                                ref txChannels,
                                                ref txPllUseExternalLo,
                                                ref txPllLoFrequency_Hz,
                                                ref tx1Atten_mdB,
                                                ref tx2Atten_mdB
                                             );

                Mykonos.DACDIV txDACDiv = Mykonos.DACDIV.DACDIV_2p5;

                byte txFirInterpolation = 1;
                byte thb1Interpolation = 2;
                byte thb2Interpolation = 1;
                byte txInputHbInterpolation = 1;
                UInt32 txiqRate_kHz = 245760;
                UInt32 primarySigBw_Hz = 75000000;
                UInt32 txrfBandwidth_Hz = 200000000;
                UInt32 txDac3dBCorner_kHz = 189477;
                UInt32 txBbf3dBCorner_kHz = 100000;

                Link.Mykonos.init_txProfiles(WRITE_SETTINGS,
                                                ref txDACDiv,
                                                ref txFirInterpolation,
                                                ref thb1Interpolation,
                                                ref thb2Interpolation,
                                                ref txInputHbInterpolation,
                                                ref txiqRate_kHz,
                                                ref primarySigBw_Hz,
                                                ref txrfBandwidth_Hz,
                                                ref txDac3dBCorner_kHz,
                                                ref txBbf3dBCorner_kHz
                                              );
                output[0] = txiqRate_kHz;
                output[1] = txPllLoFrequency_Hz;
                output[2] = primarySigBw_Hz;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Link.Disconnect();
            }


            return output;
        }

        /// <summary>
        /// Generates an array of important ORx profile information 
        /// Format of the output is [iqRate, Tx LO Frequency, signal bandwidth]
        /// 
        /// Bug: Cannot detect which LOGEN is routed to the channel of interest. This 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="channel"></param> Specify a specific Mykonos ObsRxChannel. Do not use loopback rx or no rx. 
        /// <returns></returns> Returns a double array in the format, [iqRate, Tx LO Frequency, signal bandwidth]
        public static double[] SetOrxProfileInfo(Mykonos.OBSRXCHANNEL channel)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;


            const byte WRITE_SETTINGS = 1;
            const byte READ_SETTINGS = 0;

            byte realIfData = 0;
            byte rxDec5Decimation = 5;
            UInt32 iqRate_kHz = 122880;
            Mykonos.TXATTENSTEPSIZE txStepSize = Mykonos.TXATTENSTEPSIZE.TXATTEN_0P05_DB;
            Mykonos.TXCHANNEL txChannels = Mykonos.TXCHANNEL.TX1_TX2;
            byte txPllUseExternalLo = 0;
            UInt64 txPllLoFrequency_Hz = 5650000000;
            UInt16 tx1Atten_mdB = 10000;
            UInt16 tx2Atten_mdB = 10000;
            byte obsRxChannel = (byte)Mykonos.OBSRXCHANNEL_ENABLE.MYK_ORX1;
            Mykonos.OBSRX_LO_SOURCE obsRxLoSource = Mykonos.OBSRX_LO_SOURCE.OBSLO_SNIFFER_PLL;
            Mykonos.OBSRXCHANNEL default_obsrx2txlo = Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO;
            Mykonos.OBSRXCHANNEL default_obsrxsnc = Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C;
            UInt64 snifferPllLoFrequency_Hz = 5700000000;
            byte obsRxadcDiv = 1;
            byte obsRxFirDecimation = 1;
            byte obsRxrhb1Decimation = 1;
            UInt32 obsRxiqRate_kHz = 245760;
            UInt32 obsRxrfBandwidth_Hz = 200000000;
            UInt32 obsRxBbf3dBCorner_kHz = 100000;
            byte snifferAdcDiv = 1;
            byte snifferRxFirDecimation = 4;
            byte snifferRxDec5Decimation = 5;
            byte snifferRhb1Decimation = 2;
            UInt32 snifferIqRate_kHz = 30720;
            UInt32 snifferRfBandwidth_Hz = 20000000;

            double[] output = new double[3];
            try
            {
                Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

                switch (channel)
                {
                    case Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO:
                    case Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO:
                        Link.Mykonos.init_obsrxsettings(READ_SETTINGS,
                                                        ref obsRxChannel,
                                                        ref obsRxLoSource,
                                                        ref snifferPllLoFrequency_Hz,
                                                        ref realIfData,
                                                        ref default_obsrx2txlo
                                                        );
                        Link.Mykonos.init_obsProfiles(WRITE_SETTINGS,               //For some reason, you must use WRITE_SETTINGS instead of READ_SETTINGS to read
                                                        ref obsRxadcDiv,
                                                        ref obsRxFirDecimation,
                                                        ref rxDec5Decimation,
                                                        ref obsRxrhb1Decimation,
                                                        ref obsRxiqRate_kHz,
                                                        ref obsRxrfBandwidth_Hz,
                                                        ref obsRxBbf3dBCorner_kHz
                                                        );
                        Link.Mykonos.init_txsettings(READ_SETTINGS,
                                                        ref txStepSize,
                                                        ref txChannels,
                                                        ref txPllUseExternalLo,
                                                        ref txPllLoFrequency_Hz,
                                                        ref tx1Atten_mdB,
                                                        ref tx2Atten_mdB
                                                    );
                        Console.WriteLine("IQRate: " + obsRxiqRate_kHz);
                        //iqDataRate_kHz = obsRxiqRate_kHz;
                        //LOFreq_Hz = txPllLoFrequency_Hz;
                        //portBandwidth_Hz = obsRxrfBandwidth_Hz;
                        output[0] = obsRxiqRate_kHz;
                        output[1] = txPllLoFrequency_Hz;
                        output[2] = obsRxrfBandwidth_Hz;
                        break;
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A:
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B:
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C:
                        Link.Mykonos.init_obsrxsettings(READ_SETTINGS,
                                                        ref obsRxChannel,
                                                        ref obsRxLoSource,
                                                        ref snifferPllLoFrequency_Hz,
                                                        ref realIfData,
                                                        ref default_obsrxsnc);
                        Link.Mykonos.init_snifferProfiles(WRITE_SETTINGS,
                                                            ref snifferAdcDiv,
                                                            ref snifferRxFirDecimation,
                                                            ref snifferRxDec5Decimation,
                                                            ref snifferRhb1Decimation,
                                                            ref snifferIqRate_kHz,
                                                            ref snifferRfBandwidth_Hz,
                                                            ref obsRxBbf3dBCorner_kHz
                                                            );
                        Link.Mykonos.init_obsProfiles(WRITE_SETTINGS,
                                                        ref obsRxadcDiv,
                                                        ref obsRxFirDecimation,
                                                        ref rxDec5Decimation,
                                                        ref obsRxrhb1Decimation,
                                                        ref iqRate_kHz,
                                                        ref obsRxrfBandwidth_Hz,
                                                        ref obsRxBbf3dBCorner_kHz
                                                        );
                        Link.Mykonos.init_txsettings(READ_SETTINGS,
                                                        ref txStepSize,
                                                        ref txChannels,
                                                        ref txPllUseExternalLo,
                                                        ref txPllLoFrequency_Hz,
                                                        ref tx1Atten_mdB,
                                                        ref tx2Atten_mdB
                                                    );
                        output[0] = snifferIqRate_kHz;
                        output[1] = snifferPllLoFrequency_Hz;
                        output[2] = snifferRfBandwidth_Hz;
                        break;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Link.Disconnect();

            return output;

        }
        /// <summary>
        /// Obtains important Rx profile information
        /// Format of the output is [iqRate, Rx LO Frequency, signal bandwidth]
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns> Returns a double array in the format [iqRate_kHz, RxLOFrequency_Hz, signalBandwidth_Hz]
        public static double[] SetRxProfileInfo()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            double[] output = new double[3];
            const byte WRITE_SETTINGS = 1;
            const byte READ_SETTINGS = 0;

            try
            {
                Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

                Mykonos.RXCHANNEL rxChannel = Mykonos.RXCHANNEL.RX1_RX2;
                byte rxPllExternalLo = 1;
                UInt64 rxPllFrequency_Hz = 2400000000;
                byte realIfData = 0;
                Link.Mykonos.init_rxsettings(READ_SETTINGS,
                                            ref rxChannel,
                                            ref rxPllExternalLo,
                                            ref rxPllFrequency_Hz,
                                            ref realIfData);

                byte adcDiv = 1;
                byte rxFirDecimation = 2;
                byte rxDec5Decimation = 5;
                byte enDec5Hr = 1;
                byte rhb1Decimation = 1;
                UInt32 iqRate_kHz = 122880;
                UInt32 rfBandwidth_Hz = 100000000;
                UInt32 rxBbf3dBCorner_kHz = 100000;

                Link.Mykonos.init_rxProfiles(WRITE_SETTINGS,
                                                ref adcDiv,
                                                ref rxFirDecimation,
                                                ref rxDec5Decimation,
                                                ref enDec5Hr,
                                                ref rhb1Decimation,
                                                ref iqRate_kHz,
                                                ref rfBandwidth_Hz,
                                                ref rxBbf3dBCorner_kHz
                                             );

                output[0] = iqRate_kHz;
                output[1] = rxPllFrequency_Hz;
                output[2] = rfBandwidth_Hz;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Link.Disconnect();
            }

            return output;
        }

        /// <summary>
        /// Obtains a number of samples from the fpga based on the Rx channel of interest
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="channel"></param> Rx Channel. Use Mykonos.RXCHANNEL enum
        /// <param name="numSamples"></param> Number of samples. Recommended to be 16384. 
        /// <returns></returns>
        public static short[] MykonosRxCapture(Mykonos.RXCHANNEL channel, int numSamples)
        {
            //const int numSamples = 16384;  //CONSTANT
            short[] rxDataArray = new short[numSamples];


            Enums.FPGA_CHANNEL fpgaChannel = 0;
            FpgaMykonos.RXCAPTURE dataMover = 0;

            if (channel == Mykonos.RXCHANNEL.RX1)
            {
                fpgaChannel = Enums.FPGA_CHANNEL.RX1;
                dataMover = FpgaMykonos.RXCAPTURE.RX1_DM;
            }
            else if (channel == Mykonos.RXCHANNEL.RX2)
            {
                fpgaChannel = Enums.FPGA_CHANNEL.RX2;
                dataMover = FpgaMykonos.RXCAPTURE.RX2_DM;
            }

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            try
            {
                Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                //Link.Mykonos.setEnsmState(Mykonos.ENSM_STATE.TX_RX);
                //Link.fpgaWrite(0x200, 0x0); //disable any active Data movers.
                //Link.Mykonos.powerUpRxPath(channel);
                //Link.Mykonos.radioOn();
                Link.FpgaMykonos.setRxCaptureSamples(dataMover, (uint)numSamples); //numSamples must be casted as a uint type
                Link.FpgaMykonos.setRxTrigger(FpgaMykonos.RXTRIGGER.IMMEDIATE);
                Link.FpgaMykonos.enableRxDataPaths(FpgaMykonos.RX_DATAPATH.RX1_RX2_OBSRX);
                //Console.WriteLine("SPI Read for 0x1B3: " + Link.spiRead(0x1B3).ToString("X"));
                Link.FpgaMykonos.CaptureRxData();
                Link.FpgaMykonos.waitRxCapture(1000);

                UInt32 fpgaReg = 0;
                fpgaReg = Link.fpgaRead(0x0200);
                Console.WriteLine("FPGA REG 0x0200:" + fpgaReg.ToString("X"));
                NUnit.Framework.Assert.AreEqual(0x00, fpgaReg & 0x01, "Rx Capture bit did not self clear"); //verify Rx capture bit self cleared
                rxDataArray = Link.readRam(fpgaChannel, 0, numSamples);


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //Link.Disconnect();
            }

            return rxDataArray;
        }
        /// <summary>
        /// Obtains a number of samples of the ORx channel from the FPGA
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="channel"></param> Use a Mykonos.OBSRXCHANNEL enum
        /// <param name="numSamples"></param> The number of samples. Recommended as 16384. 
        /// <returns></returns>
        public static short[] MykonosOrxCapture(Mykonos.OBSRXCHANNEL channel, int numSamples)
        {
            //const int numSamples = 16384;
            short[] rxDataArray = new short[numSamples];

            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            try
            {
                Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);
                Link.Mykonos.radioOn();


                switch (channel)
                {
                    case Mykonos.OBSRXCHANNEL.OBS_RX1_TXLO:
                    case Mykonos.OBSRXCHANNEL.OBS_RX2_TXLO:
                        Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_0, 0);
                        break;
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_A:
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_B:
                    case Mykonos.OBSRXCHANNEL.OBS_SNIFFER_C:
                        Link.FpgaMykonos.setupJesd204bObsRxOversampler(FpgaMykonos.FPGA_SAMPLE_DECIMATION.DECIMATE_BY_8, 0);
                        break;
                }

                Link.fpgaWrite(0x200, 0x0); //disable any active Data movers.
                Link.FpgaMykonos.enableRxDataPaths(FpgaMykonos.RX_DATAPATH.RX1_RX2_OBSRX);
                Link.Mykonos.setObsRxPathSource(channel);
                Link.FpgaMykonos.setRxCaptureSamples(FpgaMykonos.RXCAPTURE.OBSRX_DM, (uint)numSamples);
                Link.FpgaMykonos.setRxTrigger(FpgaMykonos.RXTRIGGER.IMMEDIATE);

                Link.FpgaMykonos.CaptureRxData();
                Link.FpgaMykonos.waitRxCapture(1000);

                UInt32 fpgaReg = 0;
                fpgaReg = Link.fpgaRead(0x0200); //Console.WriteLine("FPGA REG 0x0200:" + fpgaReg.ToString("X"));
                //NUnit.Framework.Assert.AreEqual(0x00, fpgaReg & 0x01, "Rx Capture bit did not self clear"); //verify Rx capture bit self cleared
                rxDataArray = Link.readRam(Enums.FPGA_CHANNEL.OBSRX, 0, numSamples);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                //Link.Disconnect();
            }

            return rxDataArray;
        }

        public static void GenerateFft(short[] iqData, double samplingFreq_Hz, string path)
        {
            // ----- Path and Variable Initialization ----- //
            short[] rxDataArray = new short[iqData.Length];
            double[] timeDomainWindowed = new double[iqData.Length];
            double[,] timeDomainData = new double[iqData.Length / 2, 3];
            double[] timeDomainDataShort = new double[iqData.Length / 2];
            double NUM_SAMPLES = iqData.Length;

            AdiMath.FftAnalysis analysisData = new AdiMath.FftAnalysis();
            double samplingFreq_MHz = samplingFreq_Hz / 1000000;
            byte sampleBitWidth = 16;
            double[] data = AdiMath.complexfftAndScale(iqData, samplingFreq_MHz, sampleBitWidth, true, out analysisData);

            double[,] fftFreqAmp = new double[data.Length, 2]; //Defines the 2D array to store frequency bins corresponding to fft data
            double binSize = (samplingFreq_MHz / NUM_SAMPLES);
            double minFreq = samplingFreq_MHz / 2 * (-1);
            for (int i = 0; i < data.Length; i++)
            {
                fftFreqAmp[i, 0] = minFreq + (binSize * i);
                fftFreqAmp[i, 1] = data[i];
            }

            //Time Domain Data Processing
            int numSamplesDiv2 = (int)NUM_SAMPLES / 2;
            for (int i = 0; i < numSamplesDiv2; i++)
            {
                timeDomainData[i, 0] = i;
                timeDomainData[i, 1] = iqData[2 * i];
                timeDomainData[i, 2] = iqData[2 * i + 1];

            }

            // ----- Data Analysis ----- //

            // output Charts to a pdf file //
            string[] timeLabels = new string[] { "Time Domain Response", "Sample Number", "ADC Codes", "I data", "Q data" };      //This array holds axis lablels. 
            string[] fftLabels = new string[] { "Frequency Domain Response", "Frequency (MHz)", "Amplitude (dBFS)", "FFT DATA" };                                                                                        // Should be >=4 long. 

            var doc1 = new Document();
            iTextSharp.text.Image[] container = new iTextSharp.text.Image[2];
            double[] chartAxes = new double[] { -125, 125, -120, 10 };
            container[0] = Helper.MakeChartObject(fftFreqAmp, fftLabels, path, chartAxes);
            container[1] = Helper.MakeChartObject(timeDomainData, timeLabels, path);
            string[] pcbInfo = new string[2];
            pcbInfo[0] = "text";
            pcbInfo[1] = "text";


            Helper.AddAllChartsToPdf(container, path + ".pdf", pcbInfo);

            // open result pdf
            System.Diagnostics.Process.Start(path + ".pdf");
        }

        public static double[,] RemoveZeros(double[,] xyDouble)
        {
            int width = xyDouble.GetLength(0);
            int length = xyDouble.GetLength(1);
            int lastMeaningfulIndex = 0;

            for (int i = 0; i < width; i++)
            {
                if (xyDouble[i, 0] == 0)
                {
                    lastMeaningfulIndex = i;
                    break;
                }
            }

            double[,] modifiedXyDouble = new double[lastMeaningfulIndex, length];

            for (int i = 0; i < lastMeaningfulIndex; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    modifiedXyDouble[i, j] = xyDouble[i, j];
                }
            }

            return modifiedXyDouble;

        }

        public static void EnableTxLink()
        {
            /* not working yet */
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            byte spiReg = 0;
            UInt32 fpgaReg = 0;
            spiReg = Link.spiRead(0x121); Console.WriteLine("MCS Status SPI Reg x121 = " + spiReg.ToString("X"));
            Link.fpgaWrite(0x400, 0x00400); //Enable all Rx and Tx lanes in FPGA
            Link.FpgaMykonos.resetFPGAIP(0x01);


            Link.spiWrite(0xC40, 0x00); //disable Tx NCO
            //Link.FpgaMykonos.setupJESD204bTxFramer(0x0, 0x0, 0x0, 4, 0xF, 32, true);
            //Link.FpgaMykonos.setupJESD204bSerializer(FpgaMykonos.DAUGHTER_CARD_NAME.AD9370EE01A, 0xF);

            fpgaReg = Link.fpgaRead(0x428); Console.WriteLine("FPGA Reg x428 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x42C); Console.WriteLine("FPGA Reg x42C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x430); Console.WriteLine("FPGA Reg x430 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x434); Console.WriteLine("FPGA Reg x434 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x438); Console.WriteLine("FPGA Reg x438 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x40C); Console.WriteLine("FPGA Reg x40C = " + fpgaReg.ToString("X"));


            //Enable SYSREF to framer
            Link.spiWrite(0x120, 0x11); //Enable MCS to JESD 
            Link.fpgaWrite(0x428, 0x308); //Disable FPGA Tx framer (308)
            Link.FpgaMykonos.resetFPGAIP(0x00); //Allow FPGA Plls to power up.
            //Link.fpgaWrite(0x40C, 0x8DEE99); //Set FPGA Tx Lane mux


            Link.spiWrite(0x7F, 0x30); //Disable SYSREF to Myk deframer
            Link.spiWrite(0x7A, 0x03); //reset lane fifos

            Link.fpgaWrite(0x428, 0x309); //Enable FPGA Tx Framer (309)

            System.Threading.Thread.Sleep(10);
            Link.spiWrite(0x7A, 0x00); //bring lane fifos and deframer out of reset
            Link.Mykonos.enableSysrefToDeframer(1);

            Link.FpgaMykonos.requestSysref();
            Link.Ad9528.requestSysref(true);



            spiReg = Link.spiRead(0x121); Console.WriteLine("SPI Reg x121 = " + spiReg.ToString("X"));
            Console.WriteLine("Mykonos Deframer Status: " + Link.Mykonos.readDeframerStatus().ToString("X"));

            spiReg = Link.spiRead(0x1B0); Console.WriteLine("SPI Reg x1B0 = " + spiReg.ToString("X"));

            fpgaReg = Link.fpgaRead(0x418); Console.WriteLine("FPGA Reg x418 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x428); Console.WriteLine("FPGA Reg x428[4 TxSYNCb] = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x42C); Console.WriteLine("FPGA Reg x42C = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x430); Console.WriteLine("FPGA Reg x430 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x434); Console.WriteLine("FPGA Reg x434 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x438); Console.WriteLine("FPGA Reg x438 = " + fpgaReg.ToString("X"));
            fpgaReg = Link.fpgaRead(0x43C); Console.WriteLine("FPGA Reg x43C = " + fpgaReg.ToString("X"));


            fpgaReg = Link.fpgaRead(0x428); Console.WriteLine("FPGA Reg x428[4 TxSYNCb] = " + fpgaReg.ToString("X"));
            Assert.AreEqual(0x01, ((fpgaReg >> 4) & 1), "TxSYNCb low");
            Link.Disconnect();
        }

        public static string parseProfileName(string profile)
        {
            string profileString;
            profileString = profile.Replace(" ", "");
            profileString = profileString.Replace(",", "");
            profileString = profileString.Replace(".", "");
            profileString = profileString.Replace("/", "");
            profileString += "\\";
            return profileString;
        }

        /// <summary>
        /// EnableRxFramerLink_JESD function checks Framer Status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// </summary>
        public static void EnableRxFramerLink_JESD(string resFilePath)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            string text = "";
            byte status = Link.Mykonos.readRxFramerStatus();
            if ((status & 0x20) != 0x20)
                text = "SYSREF not received by Mykonos Rx Framer IP";

            status = Link.Mykonos.readOrxFramerStatus();
            if ((status & 0x20) != 0x20)
                text = "SYSREF not received by Mykonos ObsRx Framer IP";

            UInt32 syncStatus = Link.FpgaMykonos.readSyncbStatus();

            if ((syncStatus & 0x2) != 0x02)
                text = "RXSYNBC not asserted, Rx JESD204 Link down.";
            else
                text = "Rx link up";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            //            Link.Disconnect();
        }

        /// <summary>
        /// EnableORxFramerLink_JESD function checks ORx Framer Status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// </summary>
        public static void EnableORxFramerLink_JESD(string resFilePath)
        {
            /* not working yet */
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            string text = "";
            byte status = Link.Mykonos.readRxFramerStatus();
            if ((status & 0x20) != 0x20)
                text = "SYSREF not received by Mykonos Rx Framer IP";

            status = Link.Mykonos.readOrxFramerStatus();
            if ((status & 0x20) != 0x20)
                text = "SYSREF not received by Mykonos ObsRx Framer IP";

            UInt32 syncStatus = Link.FpgaMykonos.readSyncbStatus();

            if ((syncStatus & 0x4) != 0x04)
                text = "ORXSYNBC not asserted, ORx JESD204 Link down.";
            else
                text = "ORx link up";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            //            Link.Disconnect();
        }

        /// <summary>
        /// EnableTxLink_JESD function checks Deframer Status
        /// Result is saved at test\RegressionTest_Sln\TestResults\JESDTests\JESDTestResult.txt
        /// </summary>
        public static void EnableTxLink_JESD(string resFilePath)
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            UInt32 fpgaReg = 0;

            fpgaReg = Link.fpgaRead(0x428);
            string text = "";
            if (((fpgaReg >> 4) & 1) != 0x01)
                text = "TxSYNCb low\n";
            else
                text = "Tx link up\n";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resFilePath, true))
            {
                file.WriteLine(text);
            }
            //            Link.Disconnect();
        }

        public static string readArmAdcProfilesStruct()
        {
            AdiCommandServerClient Link = AdiCommandServerClient.Instance;
            Link.hw.Connect(TestSetupConfig.ipAddr, TestSetupConfig.port);

            /* read ARM adc tuner structure */
            Link.Mykonos.sendArmCommand(0x08, new byte[] { 0x01, 0, 0, 133 }, 4);
            byte[] armData = new byte[133];
            Link.Mykonos.readArmMem(0x20000000, 133, 1, ref armData);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();



            for (int i = 0; i < 32; i = i + 2)
            {
                sb.Append((((UInt16)(armData[i]) | (UInt16)(armData[i + 1] << 8)).ToString()) + ", ");
            }
            //            Console.WriteLine(sb.ToString());

            sb = new System.Text.StringBuilder();
            for (int i = 32; i < 64; i = i + 2)
            {
                sb.Append((((UInt16)(armData[i]) | (UInt16)(armData[i + 1] << 8)).ToString()) + ", ");
            }
            //            Console.WriteLine(sb.ToString());

            sb = new System.Text.StringBuilder();
            for (int i = 64; i < 96; i = i + 2)
            {
                sb.Append((((UInt16)(armData[i]) | (UInt16)(armData[i + 1] << 8)).ToString()) + ", ");
            }
            //            Console.WriteLine(sb.ToString());

            sb = new System.Text.StringBuilder();
            for (int i = 96; i < 128; i = i + 2)
            {
                sb.Append((((UInt16)(armData[i]) | (UInt16)(armData[i + 1] << 8)).ToString()) + ", ");
            }
            //            Console.WriteLine(sb.ToString());

            //            Console.WriteLine(armData[128] + " " + armData[129] + " " + armData[130] + " " + armData[131] + " " + armData[132]);

            Link.Disconnect();

            return sb.ToString();
        }
    }
}
