    const readline = require('readline'); 
    const fs = require('fs'); 
    const { GoogleGenerativeAI } = require("@google/generative-ai"); 
    const DATA_FILE = 'history.json';

    const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout
    });
    function getValidInput(prompt, parseFn, minValue) {
        return new Promise((resolve) => {
            const ask = () => {
                rl.question(prompt, (answer) => {
                    try {
                        const userInput = parseFn(answer); 
                        if (minValue !== undefined && userInput < minValue) {
                            console.log(`Please enter a value greater than or equal to ${minValue}.`);
                            ask(); 
                        } else {
                            resolve(userInput); 
                        }
                    } catch {
                        console.log("Invalid input. Please enter a valid number.");
                        ask(); 
                    }
                });
            };
            ask();
        });
    }
    function calculateBMI(weight, height) {
        return weight / (height ** 2);
    }
    function getBMICategory(bmi) {
        if (bmi < 18.5) {
            return ["Underweight", "\x1b[34m"]; 
        } else if (bmi >= 18.5 && bmi < 24.9) {
            return ["Normal weight", "\x1b[32m"];
        } else if (bmi >= 25 && bmi < 29.9) {
            return ["Overweight", "\x1b[33m"]; 
        } else {
            return ["Obese", "\x1b[31m"]; 
        }
    }
    async function sendGeminiPrompt(prompt) {
        try {
            const genAI = new GoogleGenerativeAI("NO NO NO I CANT LEAK IT");
            const model = genAI.getGenerativeModel({ model: "gemini-1.5-flash" });
            const result = await model.generateContent(prompt);
            return result.response.text();
        } catch (error) {
            console.error("Error fetching AI response:", error.message);
            return "AI response unavailable. Please try again later.";
        }
    }
    function calculateCalories(weight, height, age, gender, activityLevel) {
        const bmr = gender === "male"
            ? 10 * weight + 6.25 * height * 100 - 5 * age + 5
            : 10 * weight + 6.25 * height * 100 - 5 * age - 161;
        const activityMultipliers = {
            "light": 1.375,
            "moderate": 1.55,
            "intense": 1.725,
            "athlete": 1.9
        };
        return bmr * (activityMultipliers[activityLevel] || 1.55);
    }
    async function bmiCalculator() {
        console.clear(); 
        console.log("Welcome to the BMI Calculator!\n");
        console.log("-".repeat(80));
        const name = await new Promise((resolve) => rl.question("Enter your name: ", resolve));
        const weight = await getValidInput("Enter your weight in kg: ", parseFloat, 1);
        const height = await getValidInput("Enter your height in meters (e.g., 1.75): ", parseFloat, 0.5);
        const age = await getValidInput("Enter your age: ", parseInt, 1);
        let gender = "";
        while (!["male", "female"].includes(gender)) {
            gender = await new Promise((resolve) => rl.question("Enter your gender (male/female): ", resolve));
        }
        let activityLevel = "";
        while (!["light", "moderate", "intense", "athlete"].includes(activityLevel)) {
            activityLevel = await new Promise((resolve) => rl.question("Enter your activity level (light, moderate, intense, athlete): ", resolve));
        }
        const bmi = calculateBMI(weight, height);
        const [bmiCategory, colorCode] = getBMICategory(bmi);
        saveData({ name, weight, height, age, gender, activity_level: activityLevel });
        console.log(`\nYour BMI is ${colorCode}${bmi.toFixed(2)}\x1b[0m, which falls under the category: ${bmiCategory}.`);
        asciiDisp(bmiCategory);
        console.log("-".repeat(80));
        const motivationPrompt = `Give a short motivation for people who are ${bmiCategory}  in one sentence 
        and as a dietitian, give a general diet plan for ${bmiCategory}
                    ---
                    Only response like that dont write extra stuff
                    - Breakfast (? calories): ?
                    - Lunch (? calories): ?
                    - Dinner (? calories): ?
                    - Snacks (? calories): ?
        `;
        const motivation = await sendGeminiPrompt(motivationPrompt);
        console.log("Motivational Message: " + motivation);
        console.log("-".repeat(80));

        const dailyCalories = calculateCalories(weight, height, age, gender, activityLevel);
        console.log(`Your estimated daily calorie requirement based on your activity level is: ${dailyCalories.toFixed(0)} kcal.`);
        await viewHistoryoption();
        console.log("-".repeat(80));
    }
    async function runProgram() {
        while (true) {
            await bmiCalculator();
            let isValidInput = false;

            while (!isValidInput) { 
                const again = await new Promise((resolve) => 
                    rl.question("Would you like to calculate your BMI again? (yes/no): ", resolve)
                );

                if (again.toLowerCase() === "yes") {
                    isValidInput = true; 
                } else if (again.toLowerCase() === "no") {
                    console.log("Thank you for using the BMI Calculator. Goodbye!");
                    rl.close();
                    return; 
                } else {
                    console.log("Invalid option! Please type 'yes' or 'no'."); 
                }
            }
        }
    }
    function saveData(data) {
    
        try {
            const jsonData = fs.existsSync(DATA_FILE) ? JSON.parse(fs.readFileSync(DATA_FILE)) : [];
            jsonData.push(data);
            fs.writeFileSync(DATA_FILE, JSON.stringify(jsonData, null, 4));
            console.log("Data saved successfully!");
        } catch (error) {
            console.error("Error saving data: ", error.message);
        }
    }
    function viewHistory() {
        if (fs.existsSync(DATA_FILE)) {
            const jsonData = JSON.parse(fs.readFileSync(DATA_FILE));
            console.log("History:");
            console.log("-".repeat(40));
            jsonData.forEach((entry, index) => {
                console.log(`Record ${index + 1}:`);
                console.log(`  Name: ${entry.name}`);
                console.log(`  Weight: ${entry.weight} kg`);
                console.log(`  Height: ${entry.height} m`);
                console.log(`  Age: ${entry.age}`);
                console.log(`  Gender: ${entry.gender}`);
                console.log(`  Activity Level: ${entry.activity_level}`);
                console.log("-".repeat(40)); 
            });
        } else {
            console.log("No history found.");
        }
    }
    async function viewHistoryoption() {
        let isValidInput = false; 

        while (!isValidInput) {
            const answer = await new Promise((resolve) =>
                rl.question("Would you like to view the history? (yes/no): ", resolve)
            );

            if (answer.toLowerCase() === "yes") {
                viewHistory(); 
                isValidInput = true;
            } else if (answer.toLowerCase() === "no") {
                console.log("History: off"); 
                isValidInput = true; 
            } else {
                console.log("Invalid option! Please type 'yes' or 'no'."); 
            }
        }
    }
    function asciiDisp(bmiCategory) {
        switch (bmiCategory) {
            case "Underweight":
                console.log(`


   ⢀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⠀⠀⠀⠀
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
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠉⠛⠿⠊⠀⠀⠀⠀⠀
                `);
                break;
            case "Normal weight":
                console.log(`                 

                ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⣾⣷⣄⠀⠀⠀⣀⣤⣤⣤⡀⠀⠀⠀⠀⠀⠀⠀
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
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠛⠿⠷⣦⣤⣤⣄⣠⣤⣤⡶⠟⠁
                `);
                break;
            case "Overweight":
                console.log(`           
    
                
                 ⢀⡤⠖⠚⠋⠉⠉⠛⠒⠶⢤⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀
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
⠀⠀⠀⠀⠠⣖⣂⡡⠤⠤⠤⣀⠜⠀⠀⠀⠀⠘⠤⠤⠒⠲⠦⠤⠕⠃               
                    `);
                break;
            case "Obese":
                console.log(`                
                    ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⢀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
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

                `);
                break;
            default:
                console.log("Invalid BMI Category");
        }
    }
    function getValidInput(prompt, parseFn, minValue) {
        return new Promise((resolve) => {
            const ask = () => {
                rl.question(prompt, (answer) => {
                    const userInput = parseFn(answer); 

                    
                    if (isNaN(userInput) || (minValue !== undefined && userInput < minValue)) {
                        console.log(`Invalid input. Please enter a valid number greater than or equal to ${minValue}.`);
                        ask(); 
                    } else {
                        resolve(userInput); 
                    }
                });
            };
            ask();
        });
    }























    runProgram();







