using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments;
//using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.Dsp;
//using NationalInstruments.Analysis.Math;

namespace AdiMathLibrary
{
    public static class AdiMath
    {
     /// <summary>
     /// Runs a complex FFT on the interleaved time domain IQ samples and return the FFT's magnitude in dBFS
     /// </summary>
     /// <param name="timeDataInterleavedIq">Signed 16bit array interleaved with I and Q data.  First sample is I, second sample Q.</param>
     /// <param name="sampleRate_MHz">The sampling frequency, used to scale the analysis data.</param>
     /// <param name="runAnalysis">If true, will return a FftAnalysis structure with information about the FFT</param>
     /// <param name="sampleBitWidth"> Bit width of the converter sample (example: 16bit sample)</param>
     /// <param name="analysisData">FftAnalysis struture that is returned when runAnalysis = true</param>
     /// <returns></returns>
        public static double[] complexfftAndScale(short[] timeDataInterleavedIq, double sampleRate_MHz, byte sampleBitWidth,  bool runAnalysis, out FftAnalysis analysisData)
        {
            int numSamples = timeDataInterleavedIq.Length / 2;
            double[] fftReal;
            double[] fftImag;
            double[] fftMagnitude_dB = new double[numSamples];
            analysisData = new FftAnalysis();
            double AdcFullScaleCode = System.Math.Pow(2, (sampleBitWidth - 1)); //2^(16-1) for 16bit converter ...assumming two's complement

            //ScaledWindowType window = ScaledWindowType.Hanning;
            ScaledWindow window = ScaledWindow.CreateHanningWindow();
            

            ComplexDouble[] complexSignal = new ComplexDouble[numSamples];
            int j = 0;

            //determine scaling value to normalize the time domain data.
            ComplexDouble scaleValue = new ComplexDouble(AdcFullScaleCode * (numSamples) * Math.Sqrt(window.EquivalentNoiseBandwidth), 0);

            //deinterleave IQ data array into complex number data type and normalize data.
            for (int i = 0; i < complexSignal.Length; i++)
            {
                complexSignal[i] = new ComplexDouble(timeDataInterleavedIq[j], timeDataInterleavedIq[j + 1]);
                complexSignal[i] = complexSignal[i].Divide(scaleValue);
                j = j + 2;
            }

            //Apply the Window (Default hanning)
            window.Apply(complexSignal);

            ComplexDouble[] fftData = Transforms.Fft(complexSignal, true);
            NationalInstruments.ComplexDouble.DecomposeArray(fftData, out fftReal, out fftImag);

            if (runAnalysis == true)
            {
                analysisData = analyzeFft(fftReal, fftImag, sampleRate_MHz);
            }
            else
            {
                analysisData = new FftAnalysis();
            }

            for (int i = 0; i < numSamples; i++)
            {
                fftMagnitude_dB[i] = 10.0 * System.Math.Log10(fftReal[i] * fftReal[i] + fftImag[i] * fftImag[i]);
            }

            return fftMagnitude_dB;
        }

        /// <summary>
        /// Structure to return the FFT analysis results
        /// </summary>
        public struct FftAnalysis
        {
            public double FundamentalPeak_dBFS;
            public double FundamentalPower_dBFS;
            public double FundamentalFrequency_MHz;
            public double ImagePower_dBFS;
            public double ImagePower_dBm;
            public double DcOffset_dBFS;
        }

        /// <summary>
        /// This function returns some basic FFT analysis results such as Fundamental Frequency, power, image power, and DC offset
        /// </summary>
        /// <param name="fftReal">The voltage domain (not Log10) real fft data from the complex FFT routine.</param>
        /// <param name="fftImag">The voltage domain (not Log10) imaginary fft data from the complex FFT routine.</param>
        /// <param name="sampleRate_MHz">The sampling frequency of the data converter in MHz</param>
        /// <returns></returns>
        public static FftAnalysis analyzeFft(double[] fftReal, double[] fftImag, double sampleRate_MHz)
        {
            int numBins = fftReal.Length;

            double fundamentalMag = 0;
            double fundamentalIndex = 0;
            double fftMag = 0;

            double[] returnData = new double[3];
            FftAnalysis analysisData = new FftAnalysis();

            for (int fftBin = 0; fftBin < numBins; fftBin++)
            {
                //Find bin with largest amplitude (fundamental)
                fftMag = fftReal[fftBin] * fftReal[fftBin] + fftImag[fftBin] * fftImag[fftBin];
                if ((fftBin <= (numBins / 2 - 2)) || (fftBin >= (numBins / 2 + 2)))
                {
                if (fftMag > fundamentalMag)
                {
                    fundamentalMag = fftMag;
                    fundamentalIndex = fftBin;
                    }
                }
            }

            double startF = (-1 * sampleRate_MHz / 2);
            double deltaF = (sampleRate_MHz / numBins);
            analysisData.FundamentalPeak_dBFS = 10 * System.Math.Log10(fundamentalMag);
            analysisData.FundamentalFrequency_MHz = (startF + (fundamentalIndex * deltaF));

            //sum Fundamental power over 5 bins
            int binsToSum = 5;
            int startBin = (int)(fundamentalIndex - System.Math.Floor((double)(binsToSum / 2)));
            int stopBin = (int)(fundamentalIndex + System.Math.Floor((double)(binsToSum / 2)));
            if (startBin < 0)
            {
                startBin = 0;
            }
            if (stopBin >= fftReal.Length)
            {
                stopBin = fftReal.Length - 1;
            }

            double sum = 0;
            for (int fftBin = startBin; fftBin <= stopBin; fftBin++)
            {
                fftMag = fftReal[fftBin] * fftReal[fftBin] + fftImag[fftBin] * fftImag[fftBin];
                sum += fftMag;
            }
            analysisData.FundamentalPower_dBFS = 10 * System.Math.Log10(sum);

            //Calculate image power over 5 bins
            int imageIndex = (int)(((analysisData.FundamentalFrequency_MHz * -1) - startF) / deltaF);

            binsToSum = 5;
            startBin = (int)(imageIndex - System.Math.Floor((double)(binsToSum / 2)));
            stopBin = (int)(imageIndex + System.Math.Floor((double)(binsToSum / 2)));
            if (startBin < 0)
            {
                startBin = 0;
            }
            if (stopBin >= fftReal.Length)
            {
                stopBin = fftReal.Length - 1;
            }

            sum = 0;
            for (int fftBin = startBin; fftBin <= stopBin; fftBin++)
            {
                fftMag = fftReal[fftBin] * fftReal[fftBin] + fftImag[fftBin] * fftImag[fftBin];
                sum += fftMag;
            }
            analysisData.ImagePower_dBFS = 10 * System.Math.Log10(sum);
            analysisData.ImagePower_dBm = analysisData.FundamentalPower_dBFS - analysisData.ImagePower_dBFS;


            //Calculate DC Offset power over 5 bins
            int DCOffsetIndex = numBins / 2;

            binsToSum = 5;
            startBin = (int)(DCOffsetIndex - System.Math.Floor((double)(binsToSum / 2)));
            stopBin = (int)(DCOffsetIndex + System.Math.Floor((double)(binsToSum / 2)));
            if (startBin < 0)
            {
                startBin = 0;
            }
            if (stopBin >= fftReal.Length)
            {
                stopBin = fftReal.Length - 1;
            }

            sum = 0;
            for (int fftBin = startBin; fftBin <= stopBin; fftBin++)
            {
                fftMag = fftReal[fftBin] * fftReal[fftBin] + fftImag[fftBin] * fftImag[fftBin];
                sum += fftMag;
            }
            analysisData.DcOffset_dBFS = 10 * System.Math.Log10(sum);

            return analysisData;

        }
    }
}
