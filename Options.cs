using System.Text;
using CommandLine;

namespace VideoStitch
{
    class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input directory for mp4 clip files.")]
        public string InputDirectory { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output directory for mp4 files.")]
        public string OutputDirectory { get; set; }

        [Option('f', "ffmpeg", Required = true, HelpText = "Path to ffmpeg.exe.")]
        public string FfmpegDirectory { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("VideoStitch 1.0");
            usage.AppendLine("Automatically stitch together video segments created by UniFi Video NVR.");
            usage.AppendLine();
            usage.AppendLine(@"VideoStitch -i c:\input -o c:\output -ff c:\ffmpeg\bin");

            return usage.ToString();
        }
    }
}
