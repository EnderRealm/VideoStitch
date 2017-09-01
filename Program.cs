using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace VideoStitch
{
    class Program
    {
        static string ffmpeg = @"d:\Video";
        static string inputDirectory = @"d:\Video";
        static string outputDirectory = @"d:\Video\output";

        static int Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLine.Parser();
          
            if (parser.ParseArguments(args, options))
            {
                inputDirectory = options.InputDirectory;
                outputDirectory = options.OutputDirectory;
                ffmpeg = MakeFilename(options.FfmpegDirectory, "ffmpeg.exe");
            }

            // get top-level directories which represent cameras
            string[] cameras = Directory.GetDirectories(inputDirectory);

            Console.Out.WriteLine("Starting VideoStitch...");

            // if there are no top-level directors, there are no cameras to process
            if (cameras.Length < 1)
            {
                WriteErrorMessage("Unable to find any video camera directories");
                return -1;
            }

            Console.Out.WriteLine("Found " + cameras.Length + " cameras.");
            
            foreach (string camera in cameras)
            {
                Console.Out.WriteLine("Processing video for camera [" + camera + "]");

                // get year directories
                string[] years = Directory.GetDirectories(camera);
                Console.Out.WriteLine("Found " + years.Length + " years");

                foreach (string year in years)
                {
                    // get month directories
                    string[] months = Directory.GetDirectories(year);
                    Console.Out.WriteLine("Found " + months.Length + " months");

                    foreach (string month in months)
                    {
                        // get day directories
                        string[] days = Directory.GetDirectories(month);
                        Console.Out.WriteLine("Found " + days.Length + " days");

                        foreach (string day in days)
                        {
                            // get individual clips
                            string[] clips = Directory.GetFiles(day);
                            Console.Out.WriteLine("Found " + clips.Length + " clips");

                            List<string> stitches = new List<string>();
                            DateTime lastCreateTime = DateTime.MaxValue;

                            foreach (string clip in clips) {
                                // make sure the file is an mp4 file
                                if (Path.GetExtension(clip).Equals(".mp4"))
                                {
                                    DateTime createTime = File.GetLastWriteTime(clip);
                                    TimeSpan interval = createTime - lastCreateTime;


                                    // if there is a greater then 5 second gap, its a new video
                                    if (interval.TotalSeconds > 3)
                                    {
                                        Console.Out.WriteLine("Stitching video with " + stitches.Count + " clips");
                                        lastCreateTime = DateTime.MaxValue;

                                        string configFile = WriteConfigFile(day, stitches.ToArray());
                                        ProcessConfigFile(day, configFile);
                                        DeleteConfigFile(day, configFile);

                                        stitches.Clear();
                                    }
                                    else
                                    {
                                        lastCreateTime = createTime;
                                        stitches.Add(clip);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.Out.WriteLine("VideoStitch completed.");
            return 1;
        }

        static void WriteErrorMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.WriteLine(msg);
            Console.ResetColor();
        }

        static string WriteConfigFile(string directory, string[] clips)
        {
            DateTime now = DateTime.Now;
            string name = now.ToFileTimeUtc().ToString();
            string fullPath = MakeFilename(directory, name, "txt");

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(fullPath))
            {
                foreach (string clip in clips)
                {
                    file.WriteLine("file '" + clip + "'");
                }
            }

            return name;
        }

        static void DeleteConfigFile(string fullPath, string file)
        {
            File.Delete(MakeFilename(fullPath, file, "txt"));
        }

        static void ProcessConfigFile(string fullPath, string baseFileName)
        {
            string paramString = "-f concat -safe 0 -i " + MakeFilename(fullPath, baseFileName, "txt") + " -c copy " + MakeFilename(outputDirectory, baseFileName, "mp4");
            var processInfo = new ProcessStartInfo(ffmpeg, paramString);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = processInfo;
            process.Start();
            process.WaitForExit();          
        }

        static string MakeFilename(string path, string name, string extension)
        {
            return Path.ChangeExtension(Path.Combine(path, name), "." + extension);
        }

        static string MakeFilename(string path, string name)
        {
            return Path.Combine(path, name);
        }
    }
}
