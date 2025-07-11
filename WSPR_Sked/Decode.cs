using System;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Wave;
using System.Numerics;

namespace Decoder
{
    public class Decode
    {


        public Decode()
        {   
       
            string filePath = "C:\\WSPR\\WAV\\WSPR_2.wav"; // Replace with actual file path
        double[] samples = ReadWavFile(filePath);
        Complex[] fftResult = PerformFFT(samples);

        Console.WriteLine($"FFT Results: {fftResult.Length} frequency bins.");
        }


        

        static double[] ReadWavFile(string path)
        {
            using (var reader = new AudioFileReader(path))
            {
                float[] floatSamples = new float[reader.Length];
                int sampleCount = reader.Read(floatSamples, 0, floatSamples.Length);
                double[] samples = new double[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                    samples[i] = floatSamples[i]; // Convert to double

                return samples;
            }
        }

        static Complex[] PerformFFT(double[] signal)
        {
            Complex[] spectrum = new Complex[signal.Length];
            for (int i = 0; i < signal.Length; i++)
                spectrum[i] = new Complex(signal[i], 0);

            Fourier.Forward(spectrum);
            return spectrum;
        }
    }

}

