using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Config_Data.Configuration
{
    public class ConfigWriter
    {

        private string file_name = "RtuCfg.txt";
        private List<ConfigItem> DO_REG;
        private List<ConfigItem> DI_REG;
        private List<ConfigItem> IN_REG;
        private List<ConfigItem> HR_INT;
        private List<ConfigItem> Data;

        public ConfigWriter()
        {
            this.DO_REG = new List<ConfigItem>();
            this.DI_REG = new List<ConfigItem>();
            this.IN_REG = new List<ConfigItem>();
            this.HR_INT = new List<ConfigItem>();
            this.Data = new List<ConfigItem>();
            SetUpData();
        }

        public bool GenerateConfigFile()
        {
            using (StreamWriter writer = File.CreateText(file_name))
            {
                writer.WriteLine(GenerateContentForConfigFile());
            }
            return true;
        }
        private void SetUpData()
        {
            Data = DataModelRepository.Instance.Points.Values.ToList();
            this.DO_REG = Data.Where(x => x.RegistarType == PointType.DIGITAL_OUTPUT).ToList();
            this.DI_REG = Data.Where(x => x.RegistarType == PointType.DIGITAL_INPUT).ToList();
            this.IN_REG = Data.Where(x => x.RegistarType == PointType.ANALOG_INPUT).ToList();
            this.HR_INT = Data.Where(x => x.RegistarType == PointType.ANALOG_OUTPUT).ToList();


        }
        private string GenerateContentForConfigFile()
        {

            StringBuilder content = new StringBuilder();
            content.AppendLine($"STA \t {DataModelRepository.Instance.UnitAddress}");
            content.AppendLine($"TCP \t {DataModelRepository.Instance.TcpPort}");
            content.AppendLine();
            foreach (var item in DO_REG)
            {
                content.AppendLine($"DO_REG \t 1 \t {item.Address} \t 0 \t {item.MinValue} \t {item.MaxValue} \t {item.CurrentValue} \t DO \t @{item.Name}");

            }
            foreach (var item in DI_REG)
            {
                content.AppendLine($"DI_REG \t 1 \t {item.Address} \t 0 \t {item.MinValue} \t {item.MaxValue} \t {item.CurrentValue} \t DI \t @{item.Name}");
            }
            foreach (var item in IN_REG)
            {
                content.AppendLine($"IN_REG \t 1 \t {item.Address} \t 0 \t {item.MinValue} \t {item.MaxValue} \t {item.CurrentValue} \t AI \t @{item.Name}");
            }
            foreach (var item in HR_INT)
            {
                content.AppendLine($"HR_INT \t 1 \t {item.Address} \t 0 \t {item.MinValue} \t {item.MaxValue} \t {item.CurrentValue} \t AO \t @{item.Name}");
            }
            return content.ToString();
        }





    }
}
