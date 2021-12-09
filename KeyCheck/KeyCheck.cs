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
        private static readonly string[] _servers = { URL, URL_JAPAN };

        private static string _baseUrl;
        
        private static string LoginUrl => _baseUrl + "pub/admin/login";
        private static string UserDataUrl => _baseUrl + "pub/school/user/{0}";
        private static string  KeyUrl => _baseUrl + "pub/school";
        
        private const string URL = "https://nau-mag.com/";
        private const string URL_JAPAN = "https://savanto.me/";
        private const string OK = "ok";
        private const string FAIL = "fail";

        static void Main(string[] args)
        {
            SelectServer();
        }

        private static void SelectServer()
        {
            Console.WriteLine($"Input number to select server:");

            for (int index = 0; index < _servers.Length; index++)
            {
                Console.WriteLine($"[{index}] -\t [{_servers[index]}]");
            }

            bool isInt = int.TryParse(Console.ReadLine(), out int choisedIndex);

            if (isInt && choisedIndex < _servers.Length)
            {
                _baseUrl = _servers[choisedIndex];
                AuthorizeServer();
            }
            else
            {
                Console.WriteLine("Incorrect input. Please try again");
                SelectServer();
            }
        }

        private static void AuthorizeServer()
        {
            Console.WriteLine("Enter login:");
            var login = Console.ReadLine();

            Console.WriteLine("Enter password:");
            var password = Console.ReadLine();

            var token = TryGetAuthorizeToken(login, password);

            if (token != null)
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", token);
                GetKey();
            }
            else
            {
                Console.WriteLine("Not correct login or password. Try again");
                AuthorizeServer();
            }

        }

        private static void GetKey()
        {
            Console.WriteLine("Enter product key:");
            var key = Console.ReadLine();
            
            GetKeyData(key);
        }

        private static void GetKeyData(string key)
        {
            var user = GetUserData(key).Result;

            if (user != null && user.hardwares.Length > 0)
            {
                TrySelectDevice(user.hardwares, key);
            }
            else
            {
                Console.WriteLine($"User not use any devices on [ {_baseUrl} ]. Try another server");
                SelectServer();
            }
        }

        private static void TrySelectDevice(UserHardware[] hardwares, string key)
        {
            Console.WriteLine("Input number of avalible device and press 'Enter'\n");
            Console.WriteLine($"[0] -\tChange server\n");

            for (int index = 1; index < hardwares.Length + 1; index++)
            {
                var hardware = hardwares[index - 1];
                Console.WriteLine($"[{index}] -\tProduct:\t[ {hardware.product} ]\n" +
                                  $"\tID:\t[ {hardware.id} ]\n" +
                                  $"\tName:\t[ {hardware.deviceName} ]\n" +
                                  $"\tType:\t[ {hardware.deviceType} ]\n" +
                                  $"\tOS:\t[ {hardware.operatingSystem} ]\n" +
                                  $"\tModel:\t[ {hardware.deviceModel} ]\n" +
                                  $"\tProc:\t[ {hardware.processorType} ]\n" +
                                  $"\tVideo:\t[ {hardware.graphicsDeviceName} ]\n");
            }

            bool isInt = int.TryParse(Console.ReadLine(), out int selectedIndex);

            if (isInt && selectedIndex < hardwares.Length + 1 && selectedIndex >= 0) 
            {
                if (selectedIndex == 0)
                {
                    SelectServer();
                    return;
                }

                CheckKey(hardwares[selectedIndex - 1], key);
            }
            else
            {
                Console.WriteLine("Incorrect input. Please try again");
                TrySelectDevice(hardwares, key);
            }

        }

        private static string TryGetAuthorizeToken(string login, string password)
        {
            return GetLoginResponse(login, password).Result.token;
        }


        private static async Task<LoginResponse> GetLoginResponse(string login, string password)
        {
            var request = new LoginRequest(login, password);
            var toSend = JsonSerializer.Serialize(request);
            var response = await client.PostAsync(LoginUrl, new StringContent(toSend, Encoding.UTF8, "application/json"));
            var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(await response.Content.ReadAsStreamAsync());
            return loginResponse;

        }

        private static async Task<User> GetUserData(string username)
        {
            var response = await client.GetAsync(string.Format(UserDataUrl, username));
            var userData = await JsonSerializer.DeserializeAsync<UserData>(await response.Content.ReadAsStreamAsync());
            return userData.user;
        }

        private static async Task<KeyResponse> GetKeyResponse(string url, string product, string key, UserHardware userHardware)
        {
            var request = new KeyRequest(product, key, userHardware);
            var toSend = JsonSerializer.Serialize(request);
            var response = await client.PostAsync(url, new StringContent(toSend, Encoding.UTF8, "application/json"));
            var keyResponse = await JsonSerializer.DeserializeAsync<KeyResponse>(await response.Content.ReadAsStreamAsync());
            keyResponse.product = product;
            return keyResponse;
             
        }

        private static void CheckKey(UserHardware hardware, string key)
        {
            Console.WriteLine($"Select device: [ {hardware.product} ] [ {hardware.id} ]");

            List<KeyResponse> responses = new List<KeyResponse>();

            foreach (StatisticsProduct product in Enum.GetValues(typeof(StatisticsProduct)))
            {
                responses.Add(GetKeyResponse(KeyUrl, product.ToString(), key, hardware).Result);
            }

            var okProducts = string.Join("\n", responses.Where(response => response.value == OK).Select(response => "\t" + response.product).ToArray()) + "\n";
            var failProducts = string.Join("\n", responses.Where(response => response.value == FAIL).Select(response => "\t" + response.product + "\n\t\t" + response.details).ToArray()) + "\n";

            Console.WriteLine("\nKey get OK for products:\n" + okProducts);
            Console.WriteLine("Key get Fail for products:\n" + failProducts);
            Console.WriteLine("[0] - Check another key\n[1] - Change server\nAny other input to exit");

            if(int.TryParse(Console.ReadLine(), out int number))
            {
                switch (number)
                {
                    case 0:
                        GetKey();
                        break;
                    case 1:
                        SelectServer();
                        break;
                    default:
                        break;
                }
            }

        }
    }

    class LoginRequest
    {
        public LoginRequest(string login, string pass)
        {
            username = login;
            password = pass;
            browser = new Browser();
        }

        public string username { get; set; }
        public string password { get; set; }
        public Browser browser { get; set; }
    }

    class Browser
    {
        public Browser()
        {
            browser = "KeyChekApp";
            os = "Windows";
            userAgent = "KeyChekApp version 1";
            language = "Russian";
            ip = "0.0.0.0";
        }

        public string browser { get; set; }
        public string language { get; set; }
        public string os { get; set; }
        public string userAgent { get; set; }
        public string ip { get; set; }
    }

    class KeyRequest
    {
        public KeyRequest(string productName, string key, UserHardware userHardware)
        {
            username = key;
            password = "";
            product = productName;
            version = 0;
            hardware = new Hardware(userHardware);
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
        public Hardware(UserHardware hardware)
        {
            id = hardware.id;
            deviceModel = hardware.deviceModel;
            deviceName = hardware.deviceName;
            deviceType = hardware.deviceType;
            operatingSystem = hardware.operatingSystem;
            processorCount = hardware.processorType;
            processorFrequency = "";
            processorType = hardware.processorType;
            systemMemorySize = hardware.systemMemorySize;
            graphicsDeviceID = hardware.graphicsDeviceID;
            graphicsDeviceName = hardware.graphicsDeviceName;
            graphicsDeviceType = "";
            graphicsDeviceVendor = "";
            graphicsDeviceVendorID = "";
            graphicsDeviceVersion = "";
            graphicsMemorySize = hardware.graphicsMemorySize;
            graphicsMultiThreaded = "";
            graphicsShaderLevel = "";
            maxTextureSize = "";
            npotSupport = "";
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

    public class LoginResponse
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
        public string[] authorities { get; set; }
    }

    public class UserData
    {
        public User user { get; set; }
        public School school { get; set; }
    }

    public class User
    {
        public string username { get; set; }
        public bool enabled { get; set; }
        public string registred { get; set; }
        public bool verified { get; set; }
        public bool isBounced { get; set; }
        public UserHardware[] hardwares { get; set; }
        public Purchase[] purchases { get; set; }
        public bool bounced { get; set; }
    }

    public class School
    {
        public string username { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string address { get; set; }
        public string association { get; set; }
        public string description { get; set; }
        public bool master { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string region { get; set; }
        public string contact { get; set; }
        public string position { get; set; }
        public int devices { get; set; }
        public int additionalComputers { get; set; }
        public string[] licenses { get; set; }
        public string[] trials { get; set; }
        public Dictionary<string, object> expire { get; set; }
        public bool bounced { get; set; }
        public Teacher[] teachers { get; set; }
    }

    public class Teacher
    {
        public string name { get; set; }
        public string teacher { get; set; }
    }


    public class UserHardware
    {
        public string save { get; set; }
        public string settings { get; set; }
        public string id { get; set; }
        public string deviceModel { get; set; }
        public string deviceName { get; set; }
        public string deviceType { get; set; }
        public string operatingSystem { get; set; }
        public string processorType { get; set; }
        public string systemMemorySize { get; set; }
        public string graphicsDeviceID { get; set; }
        public string graphicsDeviceName { get; set; }
        public string graphicsDeviceVersion { get; set; }
        public string graphicsMemorySize { get; set; }
        public int emailBounceAttempts { get; set; }
        public string product { get; set; }
        public bool authorized { get; set; }
        public string duplicatedId { get; set; }
        public string token { get; set; }
        public string refreshToken { get; set; }
        public string previousToken { get; set; }
        public int deviceId { get; set; }
    }

    public class Purchase
    {
        public int id { get; set; }
        public string username { get; set; }
        public string dlc { get; set; }
        public string date { get; set; }
        public float price { get; set; }
        public string save { get; set; }
        public string seller { get; set; }
        public bool canceled { get; set; }
        public string canceler { get; set; }
        public string canceldate { get; set; }
    }
}
