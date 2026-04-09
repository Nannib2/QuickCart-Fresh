namespace DalTest;
using Dal;
using DalApi;
using DO;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

public static class Initialization
{
    private static IDal? s_dal;
    private static readonly Random s_rand = new();


   

    /// <summary>
    /// Generates a strong random password containing English letters (upper & lower) and digits.
    /// Generates a strong password that meets all requirements: min 8 chars, and at least one uppercase, 
    // one lowercase, one digit, and one special character.
    /// </summary>
    /// <returns>A randomly generated password as a string.</returns>
    // Assumed static Random instance from the user's context
    private static string generateStrongPassword()
    {
        // Define character sets for each required category.
        const string UPPERCASE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string LOWERCASE_CHARS = "abcdefghijklmnopqrstuvwxyz";
        const string DIGITS = "0123456789";
        // Define special characters that are NOT word characters or whitespace.
        const string SPECIAL_CHARS = "!@#$%^&*()-_+=[]{}|\\:;\"'<>,.?/";

        // Define the minimum required length for the password.
        const int MIN_LENGTH = 8;

        // Combine all possible characters for filling the rest of the password.
        const string ALL_CHARS = UPPERCASE_CHARS + LOWERCASE_CHARS + DIGITS + SPECIAL_CHARS;

        // List of delegates to ensure one character from each required type is included.
        List<Func<string>> requiredCharsGenerators = new List<Func<string>>
    {
        // Guarantee at least one uppercase letter.
        () => UPPERCASE_CHARS[s_rand.Next(UPPERCASE_CHARS.Length)].ToString(),
        // Guarantee at least one lowercase letter.
        () => LOWERCASE_CHARS[s_rand.Next(LOWERCASE_CHARS.Length)].ToString(),
        // Guarantee at least one digit.
        () => DIGITS[s_rand.Next(DIGITS.Length)].ToString(),
        // Guarantee at least one special character.
        () => SPECIAL_CHARS[s_rand.Next(SPECIAL_CHARS.Length)].ToString()
    };

        // Initialize password by including all required characters first.
        StringBuilder passwordBuilder = new StringBuilder();
        foreach (var generator in requiredCharsGenerators)
        {
            passwordBuilder.Append(generator());
        }

        // Determine how many random characters are needed to reach the minimum length.
        int remainingLength = MIN_LENGTH - passwordBuilder.Length;
        // Fill the remaining length with random characters from all pools.
        for (int i = 0; i < remainingLength; i++)
        {
            char randomChar = ALL_CHARS[s_rand.Next(ALL_CHARS.Length)];
            passwordBuilder.Append(randomChar);
        }

        // Shuffle the password characters to randomize the position of the required characters.
        string finalPassword = new string(passwordBuilder.ToString().OrderBy(c => s_rand.Next()).ToArray());

        return finalPassword;
    }

    /// <summary>
    /// Generates a cryptographically secure hash of a given password using PBKDF2 with SHA256 and a high iteration count (100,000).
    /// A unique, random 16-byte salt is generated for each password. The salt and the resulting 32-byte hash are combined
    /// and returned as a Base64-encoded string for secure storage.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A Base64 string containing the combined salt and hash.</returns>
    internal static string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash
                                               // Combine salt + hash
            byte[] hashBytes = new byte[48]; // 16 bytes salt + 32 bytes hash
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);
            // Convert to base64 for storage
            return Convert.ToBase64String(hashBytes);
        }
    }

 
    /// <summary>
    ///  Generates a random integer ID within the range of 200,000,000 to 400,000,000.
    /// </summary>
    /// <returns>A randomly generated id as a int</returns>
    /// <exception cref="Exception"></exception>
    public static int GenerateValidId()
    {
        Random rand = new Random();
        int[] digits = new int[9];


        for (int i = 0; i < 8; i++)
        {
            digits[i] = rand.Next(0, 10);
        }


        int sum = 0;
        for (int i = 0; i < 8; i++)
        {
            int num = digits[i];

            int weight = (i % 2 == 0) ? 1 : 2;

            int step = num * weight;

            if (step > 9)
                step = (step % 10) + (step / 10);

            sum += step;
        }


        int checkDigit = (10 - (sum % 10)) % 10;

        digits[8] = checkDigit;


        string idString = string.Join("", digits);
        return int.Parse(idString);
    }

    /// <summary>
    /// Generates a random numeric string within the specified range.
    /// </summary>
    /// <param name="min">
    /// The inclusive lower bound of the random number range. Default is 0.
    /// </param>
    /// <param name="max">
    /// The exclusive upper bound of the random number range. Default is 10.
    /// </param>
    /// <returns>
    /// A string representation of a randomly generated integer between 
    /// <paramref name="min"/> (inclusive) and <paramref name="max"/> (exclusive).
    /// </returns>
    private static string? randomDigits(int min = 0, int max = 10)
    {
        return s_rand.Next(min, max).ToString();
    }
    /// <summary>
    ///    generates a random double number between 0.0 and 1.0 (exclusive).
    /// </summary>
    /// <returns> returns a random double number between 0.0 and 1.0 (exclusive).</returns>
    private static double? randomDouble()
    {
        double randNum = 0.0;
        while (randNum == 0.0)
        {
            randNum = s_rand.NextDouble();
        }
        return randNum;

    }
    /// <summary>
    /// Generates a valid random Israeli mobile phone number.
    /// The number always starts with '05' followed by 8 random digits (0–9).
    /// </summary>
    /// <returns>A string representing a valid random phone number.</returns>
    private static string? generateRandomPhoneNumber()
    {

        string phone = "05";

        // Generate 8 random digits and append them
        for (int i = 0; i < 8; i++)
        {
            if (i == 0)
                phone += s_rand.Next(0, 8).ToString(); // first digit after '05' should be between 0 and 7
            else
                phone += randomDigits(); // generates a digit between 0 and 9
        }

        return phone;
    }
    /// <summary>
    ///    generates a random email based on the provided name.
    /// </summary>
    /// <param name="name"> </param>
    /// <returns> random email as a string </returns>
    private static string? GenerateRandomEmail(string name)
    {

        string email = name.Split(' ')[0];
        int amountOfDigits = s_rand.Next(1, 4); // Randomly decide how many digits to add (1 to 3)
        // Generate  random digits and append them
        for (int i = 0; i < amountOfDigits; i++)
        {
            email += randomDigits(); // generates a digit between 0 and 9
        }
        email += "@gmail.com";

        return email;
    }

    //}
    /// <summary>
    ///     generates and adds 20 couriers with random attributes to the DAL Courier instance.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void createCouriers()
    {

        string[] courierNames =
    {
        "David Levi", "Maya Cohen", "Ron Ben Ami", "Yael Peretz", "Amit Shalev",
        "Noy Azulay", "Eden Biton", "Daniel Mor", "Tamar Shachar", "Lior Hadad",
        "Itai Ronen", "Sivan Ben David", "Noa Shalev", "Galit Regev", "Yossi Harel",
        "Omer Golan", "Tal Levi", "Neta Azulay", "Roni Michaeli", "Guy Refael"
    };

        int count = 0;
        while (count < 20)
        {
            try
            {
                int id = GenerateValidId();
                Courier? courier = s_dal!.Courier.Read(id);
                if (courier == null)// ID is unique, proceed with using it
                {
                    string name = courierNames[count];
                    string phoneNumber = generateRandomPhoneNumber() ?? throw new DalFailedToGenerate(" failed generate a PhoneNumber");
                    string email = GenerateRandomEmail(name) ?? throw new DalFailedToGenerate("failed generated an  email address");
                    string password = HashPassword(generateStrongPassword());
                    bool active = count % 3 == 0 ? false : true;
                    double? maxDistance;
                    if (s_dal.Config.MaxAirDeliveryDistanceKm != null)
                    {
                        maxDistance = (count % 4 == 0) ? null : randomDouble() * s_dal.Config.MaxAirDeliveryDistanceKm.Value;
                    }
                    else
                    {
                        maxDistance = (count % 4 == 0) ? null : s_rand.NextDouble() + s_rand.Next(0, 20);
                    }
                    DeliveryTypeMethods vehicle = (DeliveryTypeMethods)(count % Enum.GetValues(typeof(DeliveryTypeMethods)).Length);
                    DateTime start = new DateTime(s_dal!.Config.Clock.Year - 2, 1, 1); //stage 1
                    int range = (s_dal!.Config.Clock - start).Days; //stage 1
                    start.AddDays(s_rand.Next(range));
                    s_dal!.Courier.Create(new Courier()
                    {
                        Id = id,
                        EmploymentStartDateTime = start,
                        NameCourier = name,
                        PhoneNumber = phoneNumber,
                        EmailCourier = email,
                        PasswordCourier = password,
                        Active = active,
                        PersonalMaxAirDistance = maxDistance,
                        CourierDeliveryType = vehicle
                    });
                    count++;
                }

            }
            catch (DalFailedToGenerate ex)
            {
                throw new DalFailedToGenerate($"Failed to generate courier data: {ex.Message}");
            }

        }


    }
    /// <summary>
    ///     generates and adds 50 orders with random attributes to the DAL Order instance.
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void createOrders()
    {
        int i = 0;
        string[] address = {
"Ben Yehuda 5, Jerusalem, Israel",
"Jaffa 4, Jerusalem, Israel",
"King George 6, Jerusalem, Israel",
"Hillel 8, Jerusalem, Israel",
"Straus 12, Jerusalem, Israel",
"Hillel 3, Jerusalem, Israel",
"Shmuel HaNavi 4, Jerusalem, Israel",
"Agron 8, Jerusalem, Israel",
"Derech HaRakevet 9, Jerusalem, Israel",
"Emek Refaim 10, Jerusalem, Israel",
"Bar-Ilan 16, Jerusalem, Israel",
"Paran 9, Jerusalem, Israel",
"Balfour 5, Jerusalem, Israel",
"Ein Kerem 4, Jerusalem, Israel",
"Ha-Nevi’im 7, Jerusalem, Israel",
"Herzl 2, Jerusalem, Israel",
"Kaplan 4, Jerusalem, Israel",
"Keren HaYesod 9, Jerusalem, Israel",
"Malkhei Yisrael 8, Jerusalem, Israel",
"Mea Shearim 13, Jerusalem, Israel",
"Jaffa 30, Jerusalem, Israel",
"Ramban 39, Jerusalem, Israel",
"Ibn Ezra 7, Jerusalem, Israel",
"Saadia Gaon 14, Jerusalem, Israel",
"Ben-Maimon 5, Jerusalem, Israel",
"Alkalai  8, Jerusalem, Israel",
"HaArazim 11, Jerusalem, Israel",
"HaAyal 6, Jerusalem, Israel",
"HaGolan 9, Jerusalem, Israel",
"HaTzvi 5, Jerusalem, Israel",
"Jaffa 4, Jerusalem, Israel",
"koresh 3, Jerusalem, Israel",
"HaMesilah 2, Jerusalem, Israel",
"HaOr 8, Jerusalem, Israel",
"HaGalgal 4, Jerusalem, Israel",
"HaBanay 5, Jerusalem, Israel",
"HaDekel 6, Jerusalem, Israel",
"Golda Meir 5, Jerusalem, Israel",
"HaErez 8, Jerusalem, Israel",
"Gaza 9, Jerusalem, Israel",
"HaAliya 10, Jerusalem, Israel",
"HaTkufa 11, Jerusalem, Israel",
"king George 12, Jerusalem, Israel",
"Hachlutz 13, Jerusalem, Israel",
"HaHarash 6, Jerusalem, Israel",
"Gaza 6, Jerusalem, Israel",
"David Remez 7, Jerusalem, Israel",
"Rachel Imenu 8, Jerusalem, Israel",
"Pierre Koenig 10, Jerusalem, Israel",
"Yirmiyahu 7, Jerusalem, Israel"
        };
        string[] fullNames = {
    "Liam Johnson",
    "Noah Smith",
    "Oliver Brown",
    "Elijah Davis",
    "James Miller",
    "William Wilson",
    "Benjamin Moore",
    "Lucas Taylor",
    "Henry Anderson",
    "Alexander Thomas",
    "Mason Jackson",
    "Michael White",
    "Ethan Harris",
    "Daniel Martin",
    "Jacob Thompson",
    "Logan Garcia",
    "Jackson Martinez",
    "Levi Robinson",
    "Sebastian Clark",
    "Mateo Rodriguez",
    "Jack Lewis",
    "Owen Lee",
    "Theodore Walker",
    "Aiden Hall",
    "Samuel Allen",
    "Emma Young",
    "Olivia Hernandez",
    "Ava King",
    "Isabella Wright",
    "Sophia Lopez",
    "Charlotte Hill",
    "Amelia Scott",
    "Mia Green",
    "Harper Adams",
    "Evelyn Baker",
    "Abigail Nelson",
    "Ella Carter",
    "Scarlett Mitchell",
    "Grace Perez",
    "Chloe Roberts",
    "Luna Turner",
    "Layla Phillips",
    "Zoe Campbell",
    "Aria Parker",
    "Ellie Evans",
    "Nora Edwards",
    "Lily Collins",
    "Hazel Stewart",
    "Violet Sanchez",
    "Aurora Rivera",
    "Hannah Cooper"
};
        double[] latitudes = {
31.781800837896395, 31.781235478490185, 31.782955115216623, 31.780458620634448,
31.78510715178902, 35.216309649360994, 31.787266184408217, 31.775723251684028,
31.762766482638796, 31.766086478055488, 31.79517208869829, 35.22838561551069,
31.773707509465204, 31.769111373879248, 31.78398739301798, 31.789028024197105,
31.780988457477854, 31.77491283266829, 31.78901584098935, 31.788462625201287,
35.22184630518237, 31.774399666577086, 31.776281429698727, 31.77398121847861,
31.77396913471586, 31.77019900026061, 31.779896838631206, 31.753925440674028,
31.74439235113998, 31.7897178135403, 31.781262839116977, 31.779943948183362,
31.760042132826857, 31.790468644382667, 31.74877394879823, 31.775529548166375,
31.785737228965072, 35.218041423292526, 31.78291063449324, 31.780294239706752,
31.78411585286362, 31.760805152257035, 31.782885215647934, 31.781200078559998,
31.785965129636693, 31.774207776046058, 31.768265725581003, 31.76359111329198,
31.756900535981494, 31.792072299633066
};
        double[] longitudes = {
35.21861712052528, 35.22122852052534, 35.21748754936087, 35.216726705182396,
35.21888473216782, 31.780725899774545, 35.22672466285353, 35.21843156285413,
35.22190634751183, 35.22197736100453, 35.21808293216735, 31.801593320512783,
35.21695050518262, 35.16667086285437, 35.22585227634651, 35.20057649168911,
35.20072008983942, 35.2186494340184, 35.217689618674726, 35.21956599168916,
31.780901039937802, 35.21316710518271, 35.213754378196946, 35.21008450333267,
35.21569319353994, 35.21831429168998, 35.18585396285385, 35.18659254751231,
34.992474962855646, 35.20337881214553, 35.221249978196774, 35.22256186623777,
35.21604272052633, 35.20416366285336, 35.20784773401963, 35.189918405182645,
35.211018307032134, 31.79797840809855, 35.21264672052514, 35.2178969358682,
35.203637620525114, 35.20174677634759, 35.21708900802311, 35.19282801867513,
35.222095062853526, 35.21643273216838, 35.224437832168626, 35.217944434018946,
35.21444977634792, 35.210424989838856
};
        try
        {
            for (; i < 55; i++)
            {
                int id = 0;// going to be changed to the running number from config at creation
                DateTime orderDate = s_dal!.Config.Clock.AddDays(-s_rand.Next(0, 60)).AddHours(-s_rand.Next(0, 24)).AddMinutes(-s_rand.Next(0, 60));//need to cheak
                OrderRequirements orderType = (OrderRequirements)(i % Enum.GetValues(typeof(OrderRequirements)).Length);
                string shortOrderDescription = $"Order {i + 1} description";
                string orderAddress = address[i % 50];
                double latitude = latitudes[i % 50];
                double longitude = longitudes[i % 50];
                string customerFullName = fullNames[i % 50];
                string customerPhone = generateRandomPhoneNumber() ?? throw new DalFailedToGenerate(" failed generate a PhoneNumber");
                int amountItems = s_rand.Next(1, 50);
                bool freeShippingEligibility = (amountItems >= 20) ? true : false;
                s_dal!.Order.Create(new Order()
                {
                    Id = id,
                    OpenOrderDateTime = orderDate,
                    OrderType = orderType,
                    ShortOrderDescription = shortOrderDescription,
                    OrderAddress = orderAddress,
                    Latitude = latitude,
                    Longitude = longitude,
                    CustomerFullName = customerFullName,
                    CustomerPhone = customerPhone,
                    AmountItems = amountItems,
                    FreeShippingEligibility = freeShippingEligibility
                });
            }
        }
        catch (DalFailedToGenerate ex)
        {
            throw new DalFailedToGenerate(ex.Message);
        }

        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to creat new order: {ex.Message}");
        }
    }


    /// <summary>
    ///  generates and adds 50 deliveries with random attributes to the DAL Delivery instance.
    /// </summary>
    private static void createDelivery()
    {

        double[] distancesKm =
{
    0.0, 0.31, 0.61, 0.92, 1.22, 1.53, 1.84, 2.14, 2.45, 2.76,
    3.06, 3.37, 3.67, 3.98, 4.29, 4.59, 4.90, 5.20, 5.51, 5.82,
    6.12, 6.43, 6.73, 7.04, 7.35, 7.65, 7.96, 8.27, 8.57, 8.88,
    9.18, 9.49, 9.80, 10.10, 10.41, 10.71, 11.02, 11.33, 11.63, 11.94,
    12.24, 12.55, 12.86, 13.16, 13.47, 13.78, 14.08, 14.39, 14.69, 15.0
};

        try
        {
            List<Order> allOrders = s_dal!.Order.ReadAll().ToList();
            List<Courier> activeCouriers = s_dal!.Courier.ReadAll().Where(c => c.Active).ToList();
            if (!activeCouriers.Any()) return; //if there are not active couriers
           
            HashSet<int> busyCourierIds = new HashSet<int>();
            for (int i = 0; i < allOrders.Count && i < 50; i++)
            {
                int id = 0;
                int orderId = allOrders[i].Id;

                // decide randomly if the delivery is completed or not
                bool isCompleted = (i < 30);

             
                Courier? selectedCourier = null;

                if (isCompleted)
                {
                    // delivery is completed, so we can assign any active courier
                    selectedCourier = activeCouriers[s_rand.Next(activeCouriers.Count)];
                }
                else
                {
                    // search for an available courier who is not busy
                    selectedCourier = activeCouriers.FirstOrDefault(c => !busyCourierIds.Contains(c.Id));

                    if (selectedCourier != null)
                    {
                        // we mark the courier as busy
                        busyCourierIds.Add(selectedCourier.Id);
                    }
                }

                // if we don't find a suitable courier, skip this order
                if (selectedCourier == null) continue;

                int courierId = selectedCourier.Id;
                DeliveryTypeMethods deliveryType = selectedCourier.CourierDeliveryType;

                DateTime orderStartDateTime = allOrders[i].OpenOrderDateTime.AddDays(s_rand.Next(0, 4))
                                                                             .AddHours(s_rand.Next(0, 24));
                double? deliveryDistanceKm = distancesKm[i % distancesKm.Length];

                DeliveryCompletionType? completionType = isCompleted ? (DeliveryCompletionType)(i % 5) : null;
                DateTime? orderEndDateTime = isCompleted ? orderStartDateTime.AddHours(s_rand.Next(1, 4)) : null;

                s_dal!.Delivery.Create(new Delivery(
                    id,
                    orderId,
                    courierId,
                    deliveryType,
                    orderStartDateTime,
                    deliveryDistanceKm,
                    completionType,
                    orderEndDateTime
                ));
            }
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Failed to creat new delivery: {ex.Message}");
        }
    }






    /// <summary>
    /// gets DAL instances and resets the configuration and data lists, then initializes them with new random data.
    /// </summary>
    /// <param name="dalDelivery"></param>
    /// <param name="dalOrder"></param>
    /// <param name="dalCourier"></param>
    /// <param name="dalConfig"></param>
    /// <exception cref="NullReferenceException"></exception>

    //public static void Do(IDal? dal) //stage 2
    /// <summary>
    /// gets DAL instances and resets the configuration and data lists, then initializes them with new random data.
    /// </summary>
    /// <exception cref="DalFailedToGenerate">Thrown if there is an issue generating random data (ID, Phone, Email).</exception>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown if there is an issue with the underlying DAL storage (e.g., XML file operations).</exception>
    /// <exception cref="NullReferenceException">Thrown if the DAL object (s_dal) is unexpectedly null.</exception>
    /// <exception cref="FormatException">Thrown if a conversion of a generated value (e.g., ID) fails.</exception>
    /// <exception cref="Exception">Thrown for any other unexpected error.</exception>
    public static void Do() //stage 4
    {
        try
        {
            //s_dal = dal ?? throw new DalNullReferenceException("DAL object can not be null!");
            s_dal = DalApi.Factory.Get; //stage 4

            Console.WriteLine("Reset Configuration values and Delivery, Order, Courier Lists values");
            s_dal.ResetDB();

            Console.WriteLine("Initializing Delivery list, Order list and Courier list ");
            createCouriers();
            createOrders();
            createDelivery();
        }
        catch (DalFailedToGenerate ex)
        {
            throw new DalFailedToGenerate(($"Initialization Error (Data Generation): {ex.Message}"));
        }
        catch (DalXMLFileLoadCreateException ex)
        {
            throw new DalXMLFileLoadCreateException($"Initialization Error (DAL Operation): {ex.Message}");
        }
        catch (FormatException ex)
        {
            throw new FormatException($"Initialization Error (Format Conversion): {ex.Message}");
        }


    }

}