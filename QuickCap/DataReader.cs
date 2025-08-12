using System.Text.Json;

namespace QuickCap
{
    public static class DataReader
    {

        public static List<CaptionInput> ReadData(string fileName)
        {
            var file = File.ReadAllText(fileName);

            try
            {
                List<CaptionInput> data = JsonSerializer.Deserialize<List<CaptionInput>>(file);
                WriteMessage("Read and parsed data.sjon");
                return data;
            }
            catch (Exception ex)
            {
                WriteWarning("Failed to read data file, verify JSON");
                return null;
            }
            
        }

        public static CaptionsService ReadConfig(string fileName)
        {
            var file = File.ReadAllText(fileName);

            try
            {
                CaptionsService data = JsonSerializer.Deserialize<CaptionsService>(file);
                WriteMessage("Read and parsed config.json");
                return data;
            }
            catch (Exception ex)
            {
                WriteWarning("Failed to read config file, verify JSON");
                return null;
            }
        }

        public static void WriteMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
