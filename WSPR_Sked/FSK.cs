using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;
using System.Threading.Channels;
using System.Threading;
using ZstdSharp.Unsafe;
using System.Xml.Linq;
using System.IO;
using Org.BouncyCastle.Ocsp;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using System.Windows.Forms;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using NAudio.Wave;

namespace FSK
{

    
    public class FSKMod    {
        public bool stopPlay = false;
        const int SampleRate = 12000; // 12 kHz sampling rate
        const double SymbolDuration = 0.683; // WSPR symbol duration in seconds
        static double[] Frequencies = { 1500.0, 1501.46, 1502.92, 1504.38 }; // 4-FSK Frequencie
        public double alpha = 0.1;
        public float volumeFactor = 1.0f;
       
       
        public FSKMod(int[] wsprSymbols, int offset, bool test, double newalpha,float volume)
        {
            alpha = newalpha;
            double baseFreq = 1400;
            volumeFactor = volume;
           
            double modFreq = 1.46413;
            if (test)
            {
                modFreq = 100; //use wider tone spacing for testing sound output (no TX)
            }
            for (int i =0; i <4;i++) //apply offset and fsk frequency
            {
                Frequencies[i] = baseFreq + offset + (modFreq * i);
            }
            stopPlay = false;
                                   

            // Gaussian filter parameters
            double bandwidthTimeProduct = 0.3; // Typical value for WSPR
            int samplesPerSymbol = (int)(SampleRate * SymbolDuration);
            double[] gaussianFilter = CreateGaussianFilter(samplesPerSymbol, bandwidthTimeProduct);

            // Generate the WSPR signal with Gaussian filtering
            double[] wsprSignal = GenerateWSPRSignal(wsprSymbols, gaussianFilter);

            string filePath = "wspr_signal.wav";
            try
            {
                // Save the signal to a WAV file
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                RunAudio(wsprSignal);
                
              
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            

        }

        private async void RunAudio(double[] signal)
        {
            await Task.Run(() =>
            {
                PlayWSPR(signal);
            });
        }
        private double[] CreateGaussianFilter(int length, double bandwidthTimeProduct)
        {
            double[] filter = new double[length];
            //double alpha = Math.Sqrt(Math.Log(2)) / (Math.PI * bandwidthTimeProduct);
            //double alpha = 0.1; //best to reduce slow scintillation of audio
            for (int i = 0; i < length; i++)
            {
                double d = 2.0;
                double t = (i - length / d) / (double)length;
                
                filter[i] = Math.Exp(-0.5 * Math.Pow(2 * Math.PI * alpha * t, 2));
            }
            return filter;
        }

        static double[] GenerateWSPRSignal(int[] symbols, double[] gaussianFilter)
        {
            int samplesPerSymbol = gaussianFilter.Length;
            double[] signal = new double[samplesPerSymbol * symbols.Length];

            for (int symbolIndex = 0; symbolIndex < symbols.Length; symbolIndex++)
            {
                double frequency = Frequencies[symbols[symbolIndex]];
                for (int i = 0; i < samplesPerSymbol; i++)
                {
                    int sampleIndex = symbolIndex * samplesPerSymbol + i;
                    double t = (double)sampleIndex / SampleRate;
                    double unfilteredSample = Math.Sin(2.0 * Math.PI * frequency * t);

                    // Apply Gaussian filter
                    signal[sampleIndex] += unfilteredSample * gaussianFilter[i % gaussianFilter.Length];
                }
            }

            return signal;
        }

        private async Task PlayWSPR(double[] signal)
        {
            int sampleRate = 44100; // Standard sample rate
            int durationInSeconds = 2; // Duration of the audio
            short amplitude = 32760; // Max amplitude for 16-bit audio
            double frequency = 440.0; // Frequency of the sine wave in Hz (A4)
            //amplitude = Convert.ToInt16(Math.Round(amplitude * volumeFactor)); //adjust volume between 0 to 1 x amplitude

            // Generate sine wave audio data
            byte[] wavData = RecordWSPR(signal,sampleRate, durationInSeconds, amplitude, frequency);

           
            // Load WAV data into a MemoryStream
            using (var memoryStream = new MemoryStream(wavData))
            using (var waveReader = new WaveFileReader(memoryStream))
            using (var waveOut = new WaveOutEvent())
            {
                waveOut.Volume = volumeFactor;
                // Initialize player and play
                waveOut.Init(waveReader);
                
                waveOut.Play();
                

                // Wait for playback to finish
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    //waveOut.Volume = volumeFactor;                   
                    if (stopPlay)
                    {                        
                        waveOut.Stop();
                    }
                    System.Threading.Thread.Sleep(100);
                }
                waveOut.Dispose();
                waveReader.Dispose();
                memoryStream.Dispose();
            }
            stopPlay = false;
           
        }
        private byte[] RecordWSPR(double[] signal, int sampleRate, int durationInSeconds, short amplitude, double frequency)
        {
            using (MemoryStream memoryStream = new MemoryStream())           
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                // WAV header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + signal.Length * 2); // File size
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1 size
                writer.Write((short)1); // Audio format (PCM)
                writer.Write((short)1); // Number of channels
                writer.Write(SampleRate); // Sample rate
                writer.Write(SampleRate * 2); // Byte rate
                writer.Write((short)2); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(signal.Length * 2); // Data chunk size
               

               
                //amp = Convert.ToInt16(Math.Round(amplitude * volumeFactor)); //adjust volume between 0 to 1 x amplitude
                short amp = (short)(amplitude * 0.95); //don't make volume max. (32760 * 0.95)
                // Write signal data
                foreach (double sample in signal)
                {                                        
                    
                    short intSample = (short)(sample *amplitude); // Convert to 16-bit integer  
                    writer.Write(intSample);
                }

                return memoryStream.ToArray();



            }
            

        }


            /*
        private void PlayWSPR(double[] signal)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                // WAV header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + signal.Length * 2); // File size
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1 size
                writer.Write((short)1); // Audio format (PCM)
                writer.Write((short)1); // Number of channels
                writer.Write(SampleRate); // Sample rate
                writer.Write(SampleRate * 2); // Byte rate
                writer.Write((short)2); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(signal.Length * 2); // Data chunk size

                // Write signal data
                foreach (double sample in signal)
                {
                    short intSample = (short)(sample * 32767); // Convert to 16-bit integer
                    writer.Write(intSample);
                }
                Play the sine wave
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset stream position
                using (SoundPlayer player = new SoundPlayer(memoryStream))
                {                  
                    player.Play();

                }
               

                }


            }*/



            //-------------Alternative method to play WSPR audio----------------:

            static void SaveToWav(string filePath, double[] signal)
        {
            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // WAV header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + signal.Length * 2); // File size
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1 size
                writer.Write((short)1); // Audio format (PCM)
                writer.Write((short)1); // Number of channels
                writer.Write(SampleRate); // Sample rate
                writer.Write(SampleRate * 2); // Byte rate
                writer.Write((short)2); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(signal.Length * 2); // Data chunk size

                // Write signal data
                foreach (double sample in signal)
                {
                    short intSample = (short)(sample * 32767); // Convert to 16-bit integer
                    writer.Write(intSample);
                }
            }
        }
       
        static void PlayWavFileAsync(string filePath)
        {
            var audioFile = new AudioFileReader(filePath);             
            var outputDevice = new WaveOutEvent();

            // Handle playback stopped event
            outputDevice.PlaybackStopped += (s, e) =>
            {
              
                audioFile.Dispose();
                outputDevice.Dispose();
            };

            // Initialize and start playback
            outputDevice.Init(audioFile);
            outputDevice.Play();
            
        }
    }
}
        

            