import os
import json
import google.generativeai as genai
genai.configure(api_key="NO NO NO I CANT LEAK IT")
model = genai.GenerativeModel("gemini-1.5-flash")
def get_valid_input(prompt, input_type, min_value=None):
    while True:
        try:
            user_input = input_type(input(prompt))
            if min_value and user_input < min_value:
                print(f"Please enter a value greater than or equal to {min_value}.")
            else:
                return user_input
        except ValueError:
            print("Invalid input. Please enter a valid number.")
def calculate_bmi(weight, height):
    return weight / (height ** 2)
def get_bmi_category(bmi):
    if bmi < 18.5:
        return "Underweight", "\033[34m"
    elif 18.5 <= bmi < 24.9:
        return "Normal weight", "\033[32m"
    elif 25 <= bmi < 29.9:
        return "Overweight", "\033[33m"
    else:
        return "Obese", "\033[31m"
def health_recommendations(bmi_category):
    if bmi_category == "Underweight":
        dietplanllm(1)
    elif bmi_category == "Normal weight":
        dietplanllm(2)
    elif bmi_category == "Overweight":
        dietplanllm(3)
    elif bmi_category == "Obese":
        dietplanllm(4)
def motivational_message(bmi_category):
    if bmi_category == "Underweight":
        llmmotivation(1)
    elif bmi_category == "Normal weight":
        llmmotivation(2)
    elif bmi_category == "Overweight":
        llmmotivation(3)
    elif bmi_category == "Obese":
        llmmotivation(4)
def calculate_calories(weight, height, age, gender, activity_level):
    if gender == "male":
        bmr = 10 * weight + 6.25 * height * 100 - 5 * age + 5
    else:
        bmr = 10 * weight + 6.25 * height * 100 - 5 * age - 161
    activity_multipliers = {
        "light": 1.375,
        "moderate": 1.55,
        "intense": 1.725,
        "athlete": 1.9
    }
    return bmr * activity_multipliers.get(activity_level, 1.55)
def save(name, weight, height, age, gender, activity_level, filename="history.json"):
    history_entry = {
        "name": name, 
        "weight": weight,
        "height": height,
        "age": age,
        "gender": gender,
        "activity_level": activity_level
    }
    try:
        if os.path.exists(filename):
            with open(filename, 'r', encoding='utf-8') as file:
                history = json.load(file)
        else:
            history = []

        history.append(history_entry)

        with open(filename, 'w', encoding='utf-8') as file:
            json.dump(history, file, ensure_ascii=False, indent=4)
        print("History saved successfully.")
    except Exception as e:
        print(f"Error saving history: {e}")
def view_past_bmi_data():
    try:
        with open("history.json", "r", encoding='utf-8') as file:
            history = json.load(file)
            if not history:
                print("No saved data found.")  
                return       
            print("\nPast BMI Data:")
            print("-------------------------------------------------------------")
            for entry in history:
                try:
                    print(f"Name: {entry['name']}")
                    print(f"Weight: {entry['weight']} kg")
                    print(f"Height: {entry['height']} m")
                    print(f"Age: {entry['age']} years")
                    print(f"Gender: {entry['gender']}")
                    print(f"Activity Level: {entry['activity_level']}")
                    print("-" * 60)
                except KeyError as e:
                    print(f"Missing key in data: {e}")
                    continue
    except FileNotFoundError:
        print("No saved data found.")
    except json.JSONDecodeError:
        print("Error reading JSON file. The file may be corrupted or empty.")
def bmi_calculator():
    os.system('cls' if os.name == 'nt' else 'clear')
    print("Welcome to the BMI Calculator!\n")
    print("-" * 80)
    name = get_valid_input("Enter your name: ", str)
    weight = get_valid_input("Enter your weight in kg: ", float, 1)
    height = get_valid_input("Enter your height in meters (e.g., 1.75): ", float, 0.5)
    age = get_valid_input("Enter your age: ", int, 1)
    gender = input("Enter your gender (male/female): ").lower()
    while gender not in ['male', 'female']:
        print("Invalid input for gender. Please enter 'male' or 'female'.")
        gender = input("Enter your gender (male/female): ").lower()
    activity_level = input("Enter your activity level (light, moderate, intense, athlete): ").lower()
    while activity_level not in ['light', 'moderate', 'intense', 'athlete']:
        print("Invalid activity level. Please choose from 'light', 'moderate', 'intense', 'athlete'.")
        activity_level = input("Enter your activity level (light, moderate, intense, athlete): ").lower()
    bmi = calculate_bmi(weight, height)
    bmi_category, color_code = get_bmi_category(bmi)
    print("-------------------------------------------------------------")
    print(f"\nYour BMI is {color_code}{bmi:.2f}\033[0m, which falls under the category: {bmi_category}.")
    asciidisp(bmi_category)
    print("-" * 80)
    save(name, weight, height, age, gender, activity_level)
    health_recommendations(bmi_category)
    print("-" * 80)
    motivational_message(bmi_category)
    print("-" * 80)
    daily_calories = calculate_calories(weight, height, age, gender, activity_level)
    print(f"Your estimated daily calorie requirement based on your activity level is: {daily_calories:.0f} kcal.")
    print("-" * 80)
def run_program():
    while True:
        bmi_calculator()
        while True:
            again = input("Would you like to calculate your BMI again? (yes/no): ").lower()
            if again in ['yes', 'no']:
                break
            else:
                print("Invalid input! Please enter 'yes' or 'no'.")
        
        if again == 'no':
            print("Thank you for using the BMI Calculator. Goodbye!")
            break
    
    while True:
        view_data = input("Would you like to view your past BMI data? (yes/no): ").lower()
        if view_data in ['yes', 'no']:
            break
        else:
            print("Invalid input! Please enter 'yes' or 'no'.")
    
    if view_data == 'yes':
        view_past_bmi_data()

def llmmotivation(bmiresult):
    match bmiresult:
        case 1:
            response = model.generate_content("Give a short motivation for people who are underweight in one sentence")
        case 2:
            response = model.generate_content("Give a short motivation for people who are normal weight in one sentence")
        case 3:
            response = model.generate_content("Give a short motivation for people who are overweight in one sentence")
        case 4:
            response = model.generate_content("Give a short motivation for people who are obese in one sentence")
    print(response.text)
def dietplanllm(bmiresult):
    match bmiresult:
        case 1:
            response = model.generate_content(
            """As a dietitian, give me a diet plan for underweight.
                ---
                Only response like that dont write extra stuff
                - Breakfast (? calories): ?
                - Lunch (? calories): ?
                - Dinner (? calories): ?
                - Snacks (? calories): ?
                """
            )
        case 2:
            response = model.generate_content(
               """As a dietitian, give me a diet plan for normalweight.
                ---
                Only response like that dont write extra stuff
                - Breakfast (? calories): ?
                - Lunch (? calories): ?
                - Dinner (? calories): ?
                - Snacks (? calories): ?
                """
            )
        case 3:
            response = model.generate_content(
              """As a dietitian, give me a diet plan for overweight.
                ---
                Only response like that dont write extra stuff
                - Breakfast (? calories): ?
                - Lunch (? calories): ?
                - Dinner (? calories): ?
                - Snacks (? calories): ?
                """
            )
        case 4:
            response = model.generate_content(
              """As a dietitian, give me a diet plan for obese.
                ---
                Only response like that dont write extra stuff
                - Breakfast (? calories): ?
                - Lunch (? calories): ?
                - Dinner (? calories): ?
                - Snacks (? calories): ?
                """
            )
            
    print(response.text)

    
def showhistory():
    try:
        with open("history.json", "r", encoding='utf-8') as file:
            history = json.load(file)
            if not history:
                print("No saved data found.")  
                return       
            print("\nPast BMI Data:")
            print("-------------------------------------------------------------")
            for entry in history:
                try:
                    print(f"Name: {entry['name']}")
                    print(f"Weight: {entry['weight']} kg")
                    print(f"Height: {entry['height']} m")
                    print(f"Age: {entry['age']} years")
                    print(f"Gender: {entry['gender']}")
                    print(f"Activity Level: {entry['activity_level']}")
                    print("-" * 60)
                except KeyError as e:
                    print(f"Missing key in data: {e}")
                    continue
    except FileNotFoundError:
        print("No saved data found.")
    except json.JSONDecodeError:
        print("Error reading JSON file. The file may be corrupted or empty.")


def asciidisp(bmi_category):
    if bmi_category == "Underweight":
        print("""
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
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠿⠊⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
        """)
    elif bmi_category == "Normal weight":
        print("""
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣾⣷⣄⠀⠀⠀⣀⣤⣤⣤⡀⠀⠀⠀⠀⠀⠀⠀
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
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⠿⠷⣦⣤⣤⣄⣠⣤⣤⡶⠟⠁⠀⠀⠀⠀⠀⠀⠀
        """)
    elif bmi_category == "Overweight":
        print("""
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
⠀⠀⠀⠀⠠⣖⣂⡡⠤⠤⠤⣀⠜⠀⠀⠀⠀⠘⠤⠤⠒⠲⠦⠤⠕⠃⠀⠀⠀⠀
        """)
    elif bmi_category == "Obese":
        print("""
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⢀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
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
        """)
if __name__ == "__main__":
    run_program()