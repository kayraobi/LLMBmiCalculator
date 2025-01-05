using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

class Program
{
    static string apiKey ="NO NO NO I CANT LEAK IT"; 

    static async Task Main(string[] args)
    {
            bool repeat = true;
      
        while (repeat)
        {
            await RunProgram();
            string repeatOption;
            while (true)
            {
                Console.WriteLine("Do you want to calculate BMI again? (yes/no): ");
                repeatOption = Console.ReadLine().ToLower();

                if (repeatOption == "yes")
                {
                    break;
                }
                else if (repeatOption == "no")
                {
                    repeat = false;
                    Console.WriteLine("Goodbye!");
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'yes' or 'no'.");
                }
            }
        }
    static async Task RunProgram()
    {
        Console.Clear();
        Console.WriteLine("Welcome to the BMI Calculator\n");
        string name=GetValidName();
        double weight = GetValidInput("Enter your weight in kg: ", 1);
        double height = GetValidInput("Enter your height in meters (e.g., 1.75): ", 0.5);
        int age = (int)GetValidInput("Enter your age: ", 1);
        string gender = GetValidGender();
        string activityLevel = GetValidActivityLevel();

        double bmi = CalculateBMI(weight, height);
        var (bmiCategory, _) = GetBMICategory(bmi);

        save( name, weight, height,age,gender,activityLevel);


        Console.WriteLine($"\nYour BMI is {bmi:F2}, which falls under the category: {bmiCategory}.");
        AsciiDisp(bmiCategory);
        await GenerateMotivationAsync(bmiCategory);
        Console.WriteLine("Would you want to see history? (yes/no)");
        string optionshow=Console.ReadLine();
        historyoption(optionshow);

        double dailyCalories = CalculateCalories(weight, height, age, gender, activityLevel);
        Console.WriteLine($"Your estimated daily calorie requirement based on your activity level is: {dailyCalories:F0} kcal.");

        Console.WriteLine("Thank you for using the BMI Calculator. Goodbye!");
    }
    static double GetValidInput(string prompt, double minValue)
   {
    Console.Write(prompt);
    if (double.TryParse(Console.ReadLine(), out double input) && input >= minValue)
    {
        return input;
    }
    Console.WriteLine($"Invalid input. Please enter a value greater than or equal to {minValue}.");
    return GetValidInput(prompt, minValue);
    }
    static string GetValidGender()
    {
        while (true)
        {
            Console.Write("Enter your gender (male/female): ");
            string gender = Console.ReadLine().ToLower();
            if (gender == "male" || gender == "female")
            {
                return gender;
            }
            Console.WriteLine("Invalid input. Please enter 'male' or 'female'.");
        }
    }
    static string GetValidName()
    {
    Console.Write("Enter your name: ");
    string name = Console.ReadLine();
    return name;
    }
    static string GetValidActivityLevel()
    {
        string[] validLevels = { "light", "moderate", "intense", "athlete" };
        while (true)
        {
            Console.Write("Enter your activity level (light, moderate, intense, athlete): ");
            string level = Console.ReadLine().ToLower();
            if (Array.Exists(validLevels, l => l == level))
            {
                return level;
            }
            Console.WriteLine("Invalid input. Please choose from 'light', 'moderate', 'intense', 'athlete'.");
        }
    }

    static double CalculateBMI(double weight, double height)
    {
        return weight / (height * height);
    }

    static (string, string) GetBMICategory(double bmi)
    {
        if (bmi < 18.5) return ("Underweight", "Blue");
        if (bmi < 24.9) return ("Normal weight", "Green");
        if (bmi < 29.9) return ("Overweight", "Yellow");
        return ("Obese", "Red");
    }

    static double CalculateCalories(double weight, double height, int age, string gender, string activityLevel)
    {
        double bmr;
        if (gender == "male")
        {
            bmr = 10 * weight + 6.25 * height * 100 - 5 * age + 5;
        }
        else
        {
            bmr = 10 * weight + 6.25 * height * 100 - 5 * age - 161;
        }

        Dictionary<string, double> activityMultipliers = new Dictionary<string, double>
        {
            { "light", 1.375 },
            { "moderate", 1.55 },
            { "intense", 1.725 },
            { "athlete", 1.9 }
        };

        return bmr * activityMultipliers[activityLevel];
    }

    static async Task GenerateMotivationAsync(string category)
    {
        string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

        var client = new HttpClient();

        string prompt = GetPromptForCategory(category);
        var requestData = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        try
        {
            string jsonString = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);

                if (jsonDoc.RootElement.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out JsonElement contentElement) &&
                        contentElement.TryGetProperty("parts", out JsonElement parts) && parts.GetArrayLength() > 0)
                    {
                        string textValue = parts[0].GetProperty("text").GetString();
                        Console.WriteLine("Motivational Message: " + textValue);
                    }
                    else
                    {
                        Console.WriteLine("Response did not contain 'content' or 'parts'.");
                    }
                }
                else
                {
                    Console.WriteLine("Response did not contain 'candidates'.");
                }
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                string errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Details: {errorBody}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
    static string GetPromptForCategory(string category)
    {
       return category switch
{
       "Underweight" => @"
        As a dietitian, give a general diet plan for underweight and give Motivation for Underweight.
        ---
        Only response like that dont write extra stuff
        - Breakfast (? calories): ?
        - Lunch (? calories): ?
        - Dinner (? calories): ?
        - Snacks (? calories): ?
        ",
        "Normal weight" => @"
        As a dietitian, give a general diet plan for Normalweight and give Motivation.
        ---
        Only response like that dont write extra stuff
        - Breakfast (? calories): ?
        - Lunch (? calories): ?
        - Dinner (? calories): ?
        - Snacks (? calories): ?
        ",
        "Overweight" => @"
        As a dietitian, give a general diet plan for Overweight and give Motivation.
        ---
        Only response like that dont write extra stuff
        - Breakfast (? calories): ?
        - Lunch (? calories): ?
        - Dinner (? calories): ?
        - Snacks (? calories): ?
        ",
        "Obese" => @"
        As a dietitian, give a general diet plan for Obese and give Motivation.
        ---
        Only response like that dont write extra stuff
        - Breakfast (? calories): ?
        - Lunch (? calories): ?
        - Dinner (? calories): ?
        - Snacks (? calories): ?
        ",
   };

    }
 static void AsciiDisp(string bmiCategory)
    {
        switch (bmiCategory)
        {
            case "Underweight":
                Console.WriteLine(@"
⠀⠀⠀⢀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⠀⠀⠀⠀
⠀⠀⠀⣽⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⠛⣿⠀⠀⠀⠀
⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀⠀⠀⠀
⠀⠀⠀⣿⠀⢰⣶⣶⣶⡆⠀⠀⠀⢰⣶⣶⣶⡆⠀⣿⠀⠀⠀⠀
⠀⠀⠀⣿⠀⢸⣿⣿⣿⡇⠀⠀⠀⢸⣿⣿⣿⡇⠀⣿⠀⠀⠀⠀
⠀⠀⠀⣿⠀⠈⠉⠉⠉⠁⣶⣶⣶⠈⠉⠉⠉⠁⠀⣿⠀⠀⠀⠀
⠀⠀⠀⣼⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⣿⠀⠀⠀⠀
⠀⠀⠀⢹⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⣿⠀⠀⠀⠀
⠀⠀⠀⢸⠇⠀⠀⣸⠿⣏⣉⣉⣉⣹⡿⣇⡀⠀⢸⣛⡀⠀⠀⠀
⠀⠀⠀⠈⢩⠉⠉⠉⠉⠉⠉⠉⠉⠉⠉⢩⠍⠉⠉⠁⡇⠀⠀⠀
⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡇⠀⠀⠀
⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡇⠀⠀⠀
⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡇⠀⠀⠀
⠀⠀⠀⠀⢸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡇⠀⠀⠀
⠀⠀⠀⠀⢸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡇⠀⠀⠀
⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⡁⠀⠀⠀
⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⣦⣀⢀⠀
⠀⠀⠀⢀⣼⠇⣤⣀⠀⠀⠀⠀⠀⠀⠀⢸⠀⠀⠀⢸⣠⡴⢫⠇
⣤⣖⠚⠉⠀⠀⠈⠉⢛⡲⢦⣤⣀⠀⠀⣸⠀⢀⡴⣾⠁⠀⢸⠀
⣯⠉⠛⠶⣤⣀⡠⠾⠛⠁⠀⠀⠉⠛⢓⣩⠾⠋⠀⣿⠀⠀⢸⠀
⣿⠀⠀⠀⠀⢯⠙⠳⢦⣤⣀⠀⢠⡴⠛⢱⣂⠀⠀⣿⠀⠀⢛⡄
⡿⠀⠀⠀⠀⢸⠀⠀⠀⠀⠉⣛⠉⠀⠀⢸⣿⣿⣷⣶⢀⣴⠟⠀
⣿⣶⣤⡀⠀⢸⠀⠀⠀⠀⠀⣿⠀⠀⠀⢿⡛⠿⣿⣿⠟⠁⠀⠀
⠹⣿⣿⣿⣷⣮⣄⡀⠀⠀⠀⣿⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠉⠻⢿⣿⣿⣿⣷⣦⣄⡻⠀⠀⢀⡼⠃⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠈⠙⠻⣿⣿⣿⣿⢀⡴⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠿⠊⠀
                ");
                break;
            case "Normal weight":
                Console.WriteLine(@"
                                         ⠀⢠⣾⣷⣄⠀⠀⠀⣀⣤⣤⣤⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣶⠏⠀⠀⣿⠀⢀⡾⠛⠋⠀⣾⣿⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡀⡏⠀⠀⠀⣿⢀⣾⠁⠀⣰⠆⢹⡿⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠃⣧⠀⠀⢠⡟⢸⡇⠀⣰⠟⠀⣼⠃⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣹⣆⢀⣸⣇⣸⠃⢠⡏⠀⣸⠋⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣤⣴⣶⣶⣶⠾⠟⠛⠉⠉⠉⠈⠉⠉⠛⠁⢾⠁⣴⠇⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣤⣤⣶⣶⠾⠟⠛⠛⣻⣿⣙⡁⠀⠀⢾⣶⣾⣷⣿⣶⣄⠀⠀⠀⠀⠰⢿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⢀⣀⣀⣀⣠⣴⣶⣶⠾⠟⠛⠉⠉⠉⠀⠀⠀⠀⠀⣿⣻⣟⣻⣿⡦⠀⠘⣿⣿⣛⡿⢶⡇⠀⠀⠀⠀⠀⠀⢻⣆⠀⠀⠀⠀⠀⠀⠀⠀
⣠⣶⣶⣶⣾⣿⣿⣿⣿⣿⣿⣿⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡟⠙⣿⣿⡗⠀⠀⠿⠉⣿⣿⣿⣶⠀⠀⠀⠀⠀⠀⠈⢿⠀⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⠳⣄⣿⡿⠁⠀⠀⠘⢦⣿⣿⠇⠟⠁⠀⠀⠀⠀⠀⠀⣸⡇⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠁⡇⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⣇⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡏⠀⠀⠀⠀⠀⠀⠀
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡇⠀⠀⠀⠀⠀⠀⠀
⢻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣇⡇⠀⠀⠀⠀⠀⠀⠀
⠀⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢤⣤⡀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠈⠙⢿⣿⣿⣿⣿⣿⣿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣤⡾⠟⠛⠆⠀⠀⠀⠀⠀⢀⢻⡇⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠈⠙⠿⣿⣭⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣴⣶⠾⠟⠋⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣾⠇⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠈⠉⠙⠛⠷⠶⢶⣶⣦⣤⣴⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣌⣿⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠙⠛⠛⠛⠃⠀⠀⠀⠀⠀⠀⠀⣤⣴⣾⣿⣿⣿⣓⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⣿⣷⣦⣄⣀⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣠⣤⣶⣾⣟⣯⣽⠟⠋⠀⠉⠳⣄⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⢇⠀⠉⠛⠷⣮⣍⣩⡍⢻⡟⠉⣉⢹⡏⠉⣿⣹⣷⣦⣿⠿⠟⠉⠀⠀⠀⠀⠀⠀⠙⣆⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⠏⢸⠇⠀⠀⠀⠀⠀⠉⠉⠛⠛⠛⠛⠛⠛⠛⠋⠉⠉⠀⠀⠀⠀⠀⢠⣠⡶⠀⠀⠀⠀⠘⣧⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⡿⠀⣸⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠟⠁⠀⠀⠀⠀⠀⠘⣆⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⠃⠀⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣾⣇⡀⠀⠀⠀⠀⠀⠀⢹⡆⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣾⠀⣾⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢿⣥⢠⣤⠼⠇⠀⠀⠘⣿⡄
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣽⡄⠈⢿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣿⠿⠾⠷⠄⠀⠀⠀⢀⣿⠁
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣧⠀⠸⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣤⣾⠋⠀⠀⠀⠀⠀⠀⢰⣾⡿⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣦⣠⣿⣿⣶⣶⣤⣤⣄⣀⣀⣀⣀⠀⠀⠀⠀⠀⠀⠀⣀⣀⣠⣴⣿⣇⠀⠀⠀⠀⠀⠀⠀⣸⡟⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⢻⣿⠀⠉⠛⢿⣿⣯⣿⡟⢿⠻⣿⢻⣿⢿⣿⣿⣿⣿⣿⠿⠟⠹⣟⢷⣄⠀⠀⠀⢀⣼⠟⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿⣄⠀⠀⠘⢷⣌⡻⠿⣿⣛⣿⣟⣛⣛⣋⣉⣉⣉⣀⡀⠀⠀⠈⠻⢿⣷⣶⣶⢛⣧⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣏⠀⠀⠀⠀⠹⢯⣟⣛⢿⣿⣽⣅⣀⡀⠀⣀⡀⠀⠀⠀⠠⢦⣀⠰⡦⠀⢸⠀⣏⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢿⡀⠀⠀⠀⠀⠀⠀⠈⠉⢻⣿⡟⠛⠉⠉⠁⠀⠀⠀⠀⠀⠀⠈⠛⠷⠀⣸⠀⣿⡀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⣧⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿⣇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀⣿⡇⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⢿⠀⠀⢦⡀⡀⠀⠀⠀⠀⢹⣿⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡄⡏⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡄⠀⠈⠳⣝⠦⢄⠀⠀⠀⣟⣷⠀⠀⠀⣷⣄⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⡇⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣄⣷⡀⠀⠀⠈⠙⠂⠀⠀⠀⢸⣿⡄⠀⠀⠘⢦⡙⢦⡀⠀⠀⠀⠀⢰⣷⣷⡇⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡿⢧⣤⣀⡀⠀⠀⠀⠀⠀⠀⢿⣷⣄⠀⠀⠀⠁⠋⠀⠀⠀⠀⠀⢸⣿⣿⣇⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⣷⡀⠈⠉⠛⠛⠛⠛⠛⠛⠛⠛⢿⡍⠛⠳⠶⣶⣤⣤⣤⣤⣤⣤⠼⠟⡟⢿⡇⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣷⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⣾⡇⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣠⣤⣴⣿⣷⣶⣶⣶⣶⣶⣶⣦⣀⣀⣀⣻⡀⠀⠀⠀⣀⣀⠀⡀⠀⠀⠀⢀⣼⣿⠇⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⠟⠉⠁⠀⠀⠈⠻⣿⡆⢹⣯⣽⣿⣿⠟⠋⠙⣿⣶⣿⣿⣿⣿⣾⣿⣿⣿⣟⠋⠉⣇⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡇⠀⠀⠀⡀⠀⠀⠀⠈⢻⣆⣿⠀⠀⠀⢁⣶⣿⠿⠟⠛⠷⣶⣽⣿⣿⣻⣏⠙⠃⣴⢻⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠸⣷⣀⠀⠀⠉⠀⠀⠀⠀⠀⢹⣿⠀⣀⣴⣿⠋⠀⠀⠀⠀⠀⠀⠉⠻⣿⣧⣿⢀⣰⣿⣿⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⢿⣶⣶⣤⣤⣤⣤⣤⣤⣾⣿⣟⣿⣿⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣿⣅⣾⢿⣵⠇⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠛⠛⠛⠛⠛⠛⠛⠉⠉⠉⠁⢹⣜⠷⠦⠤⠤⠤⠤⠤⠴⠶⠛⣉⣱⠿⠁⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⠿⠷⣦⣤⣤⣄⣠⣤⣤⡶⠟⠁⠀⠀⠀⠀⠀⠀  
                ");
                break;
            case "Overweight":
                Console.WriteLine(@"
        ⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡤⠖⠚⠋⠉⠉⠛⠒⠶⢤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⢀⣰⠃⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢹⣄⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠸⡉⠙⠻⣶⣶⣶⣶⠶⢶⣶⣶⣶⡾⠋⠁⡇⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⢱⠀⠀⠀⠉⠉⠁⢤⡤⠀⠉⠉⠀⠀⢺⡀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⢣⠀⠀⠀⠀⠈⠂⠤⠤⠔⠀⠀⠀⠀⢰⡁⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⡠⠞⠉⠉⠒⠀⠀⠀⠠⣀⣀⡀⠀⠀⠀⠔⠋⠉⠳⣄⠀⠀⠀⠀⠀
⠀⠀⢀⡤⠊⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠁⠀⠀⠀⠀⠀⠀⠀⠀⠈⢳⡄⠀⠀⠀
⠀⣠⠏⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⣆⠀⠀
⢰⡏⠀⢀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⡄⠘⣆⠀
⣼⠀⠀⡜⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠃⠀⠘⡄
⡿⠀⣠⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⡄⠀⢱
⡇⠀⣽⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⢸⠀⢸
⢷⠀⠊⢳⣄⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⣴⡇⠀⠀⢨
⠈⠓⠀⠈⢿⣿⣿⣷⣶⣶⣤⣤⣤⣤⣤⣤⣤⣤⣤⣶⣶⣿⣿⣿⣿⣿⠃⠀⠀⠀
⠀⠀⠀⠀⠈⢿⡿⢿⣿⣿⣿⣿⠿⣿⠿⠿⢿⠿⠿⠿⠿⠿⠿⠿⣿⠏⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠳⡀⠀⠀⠀⠀⠰⠁⠀⠀⠈⢆⠀⠀⠀⠀⠀⡰⠃⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠙⡄⠀⠀⢠⠁⠀⠀⠀⠀⠈⡄⠀⠀⢠⠎⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⡹⠀⠀⠘⢄⠀⠀⠀⠀⢀⠃⠀⠀⠸⢀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠠⣖⣂⡡⠤⠤⠤⣀⠜⠀⠀⠀⠀⠘⠤⠤⠒⠲⠦⠤⠕⠃⠀⠀⠀    
                ");
                break;
            case "Obese":
                Console.WriteLine(@"
⠀⠀⠀⠀⠀⢀⣀⣀⢀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣴⣶⡿⠷⠷⠿⠿⠷⢶⣿⣦⣔⡢⢀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣾⣮⡭⠽⣶⣄⣥⣄⣰⡾⣟⣏⣻⣿⠻⣷⣵⡢⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡔⡲⠬⢽⡀⠀⣶⠀⡼⠋⠀⠈⢻⡄⠀⣦⠀⢸⡟⢾⣿⣿⣷⣄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣤⡟⠋⠀⠀⠀⢌⠳⢤⣠⣤⣧⡀⠠⠀⢰⡷⣄⣁⣡⠟⡀⠈⠙⠏⢟⡿⢆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣼⠋⠀⢀⠀⠌⢀⠀⣿⡆⠀⠀⣈⣡⡀⠤⡬⠤⠀⠉⠛⣦⠁⡐⠀⡀⠀⠄⢻⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠆⠀⠌⠠⠀⠀⠂⠐⢸⡇⠀⠀⠀⠀⠀⠀⠀⠐⠒⢒⣾⠏⡀⠄⠂⡀⠁⠀⠈⣛⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⡏⠀⠈⠠⠁⠂⠁⣾⡟⠛⠋⠙⠋⠛⠒⠒⠶⠦⡤⠶⠋⠁⠀⠠⠀⠐⠠⠁⢈⠀⠘⣷⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣿⠀⠄⠡⠀⠄⠂⢀⣈⢷⣀⡀⠄⠂⠄⡀⠂⠀⠀⠄⠀⠀⡐⠈⡀⠡⠐⠀⡈⠀⠄⠀⢹⣆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⠇⠂⠠⠁⡌⠘⡀⣢⠾⠛⠙⠁⠀⠡⠈⠄⡁⠈⠀⠄⣧⠀⡀⠂⠄⡁⢀⠂⠀⠁⡐⠀⠨⣿⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣿⠀⠐⠠⢁⠘⡀⠰⣏⠓⠀⠂⠈⠀⠁⠌⡀⠄⠐⠈⠀⣸⡇⢀⠡⠐⠠⠀⠌⠀⡁⠠⠀⠀⢹⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⣤⡺⠇⠀⡀⠁⠂⠐⡀⠣⠙⢷⣬⣤⣡⣴⠾⢷⣄⣐⣀⣢⣵⠟⠁⠂⠀⠁⠠⠈⢀⠂⠀⠄⠠⢁⠈⠻⣦⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⡺⠏⠁⠀⢀⠐⡀⠠⠈⢀⠀⠁⢂⠠⠠⠄⢁⠀⡀⢀⠈⠉⢉⠉⢀⠀⠀⡐⠈⠀⠄⠠⠀⠐⠈⠠⠐⠂⠠⠀⠈⠙⣖⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⣴⠟⠁⢠⣼⠂⢁⠠⠀⡀⠄⠐⡀⢈⠠⠀⠀⠁⡀⠂⠠⠐⠀⠀⠁⡀⠀⠂⠀⠁⠀⠄⠁⡀⠂⢈⠀⠌⠠⠁⠌⡀⠐⣠⠀⠄⠓⢵⢄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡾⠋⠀⣀⣾⠟⠁⠠⠀⠠⠁⠄⠀⠂⢀⠠⠀⡀⠁⠄⠀⠠⠐⠀⠈⠐⡀⠀⠄⠂⠁⠌⠠⠀⠂⢀⠐⠀⠠⠀⠡⢈⠐⡀⠄⠹⣷⡂⠉⠀⠻⣦⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣴⠟⠁⢀⣼⡟⠉⠀⠌⡀⠈⠀⡐⠈⠀⡐⠀⠀⡀⠐⠠⠈⠀⠁⡀⢈⡀⠁⡀⠌⠀⠀⠉⠠⢁⠂⠁⠀⠠⠈⠀⠄⠁⡀⠆⠠⢀⠂⠈⠻⣧⡀⠄⠘⢿⢄⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⣴⡟⠁⠠⠐⢸⣏⣠⠎⠂⠄⠀⡐⠀⠠⠐⠀⡀⠀⠆⠰⡷⠶⠛⠛⠙⠋⠉⠉⠋⠙⠛⠛⠓⠾⢦⠄⠠⡁⠌⠀⠄⠁⡀⢂⠀⢌⠐⠠⠰⣦⣁⠸⣳⡔⢀⠀⠻⣢⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⢠⡾⢋⠀⠀⡐⣠⡾⠋⠁⡐⠈⠄⠂⠐⠠⠁⡀⢂⠀⡐⠠⢀⠀⠀⠄⠠⠀⠐⠈⢀⠈⠠⢀⠠⠀⠠⠀⠀⢂⠐⠀⡈⠀⠂⢀⠀⠌⠠⠈⠄⠠⠀⡙⠳⣌⣿⢬⠀⢀⠹⡧⡀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣐⡟⠁⠀⢠⣢⡾⠋⡀⠀⠂⠀⠐⠀⠈⠐⢀⠀⠄⠀⢂⣼⣷⣾⣶⣅⠈⢐⣈⢠⣐⣠⣀⣁⡀⠂⢄⣡⣾⣿⣶⡎⠀⠀⠄⠐⠀⠠⠈⠄⢡⠈⠄⠐⠀⡀⠉⢻⣆⢂⠀⠀⠘⣷⡄⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣴⡟⠀⠀⢌⣴⠏⠐⠀⠠⠐⠀⢈⠀⠄⠁⡈⠀⠄⠠⠈⢄⢻⣦⣟⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣷⣣⣞⣿⡏⠀⠐⠀⡈⠄⠡⢈⠐⠂⠄⠂⠀⠂⢀⠁⠂⠙⣧⡄⠁⠀⠈⢾⡀⠀⠀⠀⠀
⠀⠀⠀⠀⣼⠏⠀⢀⢸⡞⠣⠀⠀⢈⠀⠄⠈⢀⠀⠂⡀⠄⠂⢈⠠⢈⣴⣿⣾⣿⣿⣿⣿⡿⣟⣿⣻⣟⣿⡿⣿⣿⣿⣿⣿⣿⣿⣧⡀⢂⠀⠐⠈⡐⠠⠁⠌⢀⠀⠡⠈⠄⣈⠐⠀⠈⢷⡌⠀⠁⠈⣟⡀⠀⠀⠀
⠀⠀⠀⣸⡏⠀⠀⣲⢟⡐⠀⡀⠈⡀⠠⠀⢈⠀⠄⠂⠀⠄⡐⠀⣆⣿⣟⣷⣻⢾⣽⣳⣯⢿⡽⣞⣷⣻⣞⡿⣽⣟⣿⢿⣿⣿⣿⣿⣿⣦⡀⠂⢀⠀⠀⠌⠀⠠⠐⠠⢁⠂⠄⠂⢁⠀⠈⢻⡆⣁⠠⠘⣷⡀⠀⠀
⠀⠀⡰⡟⠀⢀⣸⢏⡜⠀⡀⠀⠀⠄⠀⠂⠄⠠⠀⠂⢁⠠⢀⣧⣿⣻⣞⡷⣯⢿⣾⣿⣽⢯⡿⣽⣞⡷⣯⣟⡿⣞⣿⣻⣿⣿⣿⣿⣿⣿⣿⣄⠠⠀⠂⠀⠌⠀⢀⠂⠡⠈⠐⠀⠂⠐⠀⡈⢻⡄⠄⠀⢹⣇⠀⠀
⠀⢠⣿⠁⠀⣠⡟⠈⢀⠐⠠⢁⠀⠂⠁⠈⠐⠀⠠⢁⠠⠐⣲⣿⡳⣟⡾⣽⣻⣿⣿⣿⣯⢿⣽⣳⢯⣟⡷⣯⢿⣿⣽⣿⢻⡿⣿⣿⣿⣿⣿⣿⣷⣄⠡⠀⠀⠐⠀⠌⠀⠁⠀⠂⠀⠂⠐⠀⡈⢿⡀⠌⠀⢿⡄⠀
⠀⢾⠇⠀⢀⡿⠡⢁⠂⠌⠐⠀⠠⠀⡀⠁⠠⠈⠄⢂⢴⣿⣟⡾⣽⣻⡝⣶⣹⢿⣿⣿⣽⣻⢾⣽⣻⢾⣽⣻⢿⣿⣟⣮⢳⡽⣻⣿⣿⣿⣿⣿⣿⣿⣆⠀⠈⠀⠈⠀⠀⡀⠁⠈⡀⠠⠁⡐⠠⠸⣷⠁⢀⠸⣷⠀
⢰⣿⠀⠀⣸⠇⡀⠄⡈⠐⡀⠌⠀⠄⠀⠄⡁⠠⠈⢦⣿⣿⢾⣝⡧⣟⡽⣞⡽⣻⢟⡿⣾⢽⡻⢶⣏⡿⢾⡽⣯⣿⣿⣞⣯⢿⣽⣿⣿⣿⣿⣿⣿⣿⣿⡄⠀⠂⢀⠈⠀⠀⠠⠐⠀⠀⠄⠀⡀⠄⢹⡆⠄⠀⣿⡀
⣸⡏⠀⢀⡿⠈⢀⠐⠠⠁⠄⠠⠈⠀⠈⡐⠠⢁⠘⣰⣿⣿⣛⣮⢗⣻⢼⣣⠟⣬⢫⠝⣎⢳⣙⠳⢮⡝⣯⣽⣳⢿⡾⣽⣾⢯⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⢀⠐⡀⢀⠂⡐⠀⡀⠈⢀⠠⠀⠀⠠⢘⣇⠀⡀⢻⡅
⣿⠆⠀⢸⡏⡅⠠⠀⠁⠄⠂⠐⠀⠁⢂⠀⠄⠂⠌⡐⣿⣟⡿⣼⣫⡗⣯⢞⡹⣄⠃⠎⡄⢃⠌⢣⢣⡝⢶⣣⢯⣻⣽⣳⢯⡿⣽⣿⣿⣿⣿⣿⣿⣿⣿⠈⠄⢂⠐⡀⠆⠠⢁⠄⠡⠀⠀⠄⠁⡀⠂⣿⢐⠀⢸⡇
⣿⠀⠀⢸⡇⠃⠄⠁⢂⠀⠂⠠⠁⠐⡀⠠⠀⠄⢂⠡⢺⣿⣟⡷⣧⠿⣼⣹⠲⣌⠣⠌⡐⣈⠘⡤⢓⡼⢣⡟⣞⣧⢷⣯⢿⣽⣳⣿⣿⣿⣿⣿⣿⣿⠏⠀⠌⢀⠂⡐⢈⠁⠂⠌⠠⠁⠈⠀⢂⠀⠅⣿⢘⠀⢸⡇
⣿⠄⠀⢸⣇⠡⠈⠐⠀⡐⠈⢀⠐⢀⠠⠐⠀⠠⠀⠌⠤⡙⣾⢿⣽⣛⡶⣭⢳⣌⢣⠜⡰⢀⠣⡜⡱⢎⡷⣹⡞⣽⢾⣽⣻⢾⡽⣿⣿⣻⣿⣿⣿⠛⠈⢀⠂⠄⢂⠐⠠⠈⠐⠀⠂⢀⠀⠁⠠⢈⠀⣿⢠⠂⢸⡇
⢿⡆⠀⠸⣯⠀⠂⠀⢁⠠⠐⠀⠀⠂⢀⠠⠈⠀⡁⠂⠄⡐⠉⠿⢾⣽⣳⢯⣗⢮⢧⣛⡴⣋⢶⣩⢗⡯⣞⢷⣻⣽⣻⢾⡽⣯⢿⣿⣽⣿⣿⠟⣣⠀⠂⢀⠀⠂⠀⠂⠁⢀⠠⠁⠂⠀⠂⠈⠐⠀⠠⣿⠘⣤⢸⡃         
                ");
                break;
        }
    }
        static void save(String name,double weight, double height,int age,string gender,string activityLevel)
        {
            string fileName = "history.json";

            List<Dictionary<string, string>> history = new List<Dictionary<string, string>>();

            if (File.Exists(fileName))
            {
                try
                {
                    string existingData = File.ReadAllText(fileName);
                    if (!string.IsNullOrEmpty(existingData))
                    {
                        history = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(existingData) ?? new List<Dictionary<string, string>>();
                    }
                }
                catch
                {
                    Console.WriteLine("File modification failed (json)");
                }
            }

        var entry = new Dictionary<string, string>
       {
       { "Name", name },
       { "Weight", weight.ToString() },
       { "Height", height.ToString() },
       { "Age", age.ToString() },
       { "Gender", gender },
       { "ActivityLevel", activityLevel }
};

            history.Add(entry);

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonString = JsonSerializer.Serialize(history, options);
                jsonString = jsonString.Replace("\\u002B", "+");

                File.WriteAllText(fileName, jsonString);
                Console.WriteLine("Result saved to history.");
            }
            catch
            {
                Console.WriteLine("Save process failed.");
            }
        }
}
static void ShowHistory()
{
    string fileName = "history.json";
    if (File.Exists(fileName))
    {
        string jsonContent = File.ReadAllText(fileName);
        var history = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonContent);

        Console.WriteLine("\n--- History ---\n");
        if (history != null && history.Count > 0)
        {
            int count = 1;
            foreach (var entry in history)
            {
                Console.WriteLine($"Record {count}:");
                Console.WriteLine($"  Name: {entry["Name"]}");
                Console.WriteLine($"  Weight: {entry["Weight"]}");
                Console.WriteLine($"  Height: {entry["Height"]}");
                Console.WriteLine($"  Age: {entry["Age"]}");
                Console.WriteLine($"  Gender: {entry["Gender"]}");
                Console.WriteLine($"  Activity Level: {entry["ActivityLevel"]}\n");
                
                count++;
            }
        }
        else
        {
         Console.WriteLine("No history found.");
        }
    }
    else
    {
        Console.WriteLine("\nNo history found.");
    }
}
static void historyoption(string option1)
{
    if (option1 == "yes" || option1 == "YES")
    {
        ShowHistory();
    }
    else if (option1 == "no" || option1 == "No")
    {
        Console.WriteLine("History:Off");
    }
    else
    {
        while (true)
        {
            Console.Write("You entered an invalid option. Please write (yes/no): ");
            option1 = Console.ReadLine(); 
            
            if (option1 == "yes" || option1 == "YES")
            {
                ShowHistory();
                break; 
            }
            else if (option1 == "no" || option1 == "No")
            {
                Console.Write("History:Off");
                break; 
            }
        }
    }
}
} 