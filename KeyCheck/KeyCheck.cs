using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KeyCheck
{
    class KeyCheck
    {
        private static readonly HttpClient client = new HttpClient();

        private const string URL = "https://nau-mag.com/pub/school";
        private const string URL_JAPAN = "https://savanto.me/pub/school";
        private const string OK = "ok";
        private const string FAIL = "fail";
        private const StatisticsProduct japan = StatisticsProduct.school_japan;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter product key:");
            var key = Console.ReadLine();
            List<KeyResponse> responses = new List<KeyResponse>();

            foreach (StatisticsProduct product in Enum.GetValues(typeof(StatisticsProduct))) 
            {
                
                responses.Add(await GetKeyResponse(GetUrl(product), product.ToString(), key));
            }

            var okProducts = string.Join("\n", responses.Where(response => response.value == OK).Select(response => "\t" + response.product).ToArray()) + "\n";
            var failProducts = string.Join("\n", responses.Where(response => response.value == FAIL).Select(response => "\t" + response.product + "\n\t\t" + response.details).ToArray()) + "\n";

            Console.WriteLine("\nKey get OK for products:\n" + okProducts);
            Console.WriteLine("Key get Fail for products:\n" + failProducts);

            Console.ReadLine();
        }

        private static string GetUrl(StatisticsProduct product)
        {
            if (product == japan)
                return URL_JAPAN;

            return URL;
        }

        private static async Task<KeyResponse> GetKeyResponse(string url, string product, string key)
        {
            var request = new KeyRequest(product, key);
            var toSend = JsonSerializer.Serialize<KeyRequest>(request);
            var response = await client.PostAsync(URL, new StringContent(toSend, Encoding.UTF8, "application/json"));
            var keyResponse = await JsonSerializer.DeserializeAsync<KeyResponse>(await response.Content.ReadAsStreamAsync());
            keyResponse.product = product;
            return keyResponse;
             
        }
    }

    class KeyRequest
    {
        public KeyRequest(string productName, string key)
        {
            username = key;
            password = "";
            product = productName;
            version = 0;
            hardware = new Hardware();
            master = false;
            isTeacher = false;
        }

        public string username { get; set; }
        public string password { get; set; }
        public string product { get; set; }
        public int version { get; set; }
        public Hardware hardware { get; set; }
        public bool master { get; set; }
        public bool isTeacher { get; set; }
    }

    public enum StatisticsProduct
    {
        school,
        cards_app,
        cards_app_school,
        logopedia,
        school_japan,
        robot_key,
        vna_labs,
        expedition_magnet,
        school_demo
    }

    public class Hardware
    {
        public Hardware()
        {
            id = "TestKeyApp";
            deviceModel = "deviceModel";
            deviceName = "deviceName";
            deviceType = "deviceType";
            operatingSystem = "operatingSystem";
            processorCount = "processorCount";
            processorFrequency = "processorFrequency";
            processorType = "processorFrequency";
            systemMemorySize = "systemMemorySize";
            graphicsDeviceID = "graphicsDeviceID";
            graphicsDeviceName = "graphicsDeviceName";
            graphicsDeviceType = "graphicsDeviceType";
            graphicsDeviceVendor = "graphicsDeviceVendor";
            graphicsDeviceVendorID = "graphicsDeviceVendorID";
            graphicsDeviceVersion = "graphicsDeviceVersion";
            graphicsMemorySize = "graphicsMemorySize";
            graphicsMultiThreaded = "graphicsMultiThreaded";
            graphicsShaderLevel = "graphicsShaderLevel";
            maxTextureSize = "maxTextureSize";
            npotSupport = "npotSupport";
        }

        public string id { get; set; }
        public string deviceModel { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }

        public string operatingSystem { get; set; }
        public string processorCount { get; set; }
        public string processorFrequency { get; set; }
        public string processorType { get; set; }
        public string systemMemorySize { get; set; }

        public string graphicsDeviceID { get; set; }
        public string graphicsDeviceName { get; set; }
        public string graphicsDeviceType { get; set; }
        public string graphicsDeviceVendor { get; set; }

        public string graphicsDeviceVendorID { get; set; }
        public string graphicsDeviceVersion { get; set; }
        public string graphicsMemorySize { get; set; }
        public string graphicsMultiThreaded { get; set; }

        public string graphicsShaderLevel { get; set; }
        public string maxTextureSize { get; set; }
        public string npotSupport { get; set; }
    }

    public class KeyResponse
    {
        public string value { get; set; }
        public string details { get; set; }
        public int delay { get; set; }
        public string product;
    }
}
