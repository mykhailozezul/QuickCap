using QuickCap;

string dataFileName = "data.json";
string configFileName = "config.json";

var data = DataReader.ReadData(dataFileName);
var config = DataReader.ReadConfig(configFileName);

if (data != null && config != null)
{
    config.SetInputs(data);
    config.ProcessInputs();
}

Console.ReadLine();