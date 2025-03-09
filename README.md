# Code Vulnerability Analyzer

This project automates the process of analyzing source code files for potential security vulnerabilities using Generative AI. It uploads a set of code files, sends them to an AI model for analysis, and generates a detailed report identifying vulnerabilities in the code. The project uses the [GenerativeAI](https://www.youtube.com/watch?v=3-XqHtBemlk) platform and integrates with Google's AI models to provide security insights.

## How It Works

1. The program accepts three arguments:
   - `apiKey`: Your API key for accessing Generative AI services.
   - `fileExtension`: The file extension of the code files you want to analyze (e.g., `cs`, `js`).
   - `folderPath`: The path to the folder containing the code files.

2. It uploads the code files to the Google AI platform.

3. The AI analyzes the code and identifies potential security vulnerabilities, providing detailed reports such as:
   - File Name
   - Vulnerable Line
   - Vulnerability Description
   - Suggested Fix

4. The report is displayed in the console.

## Usage

```bash
dotnet run <apiKey> <fileExtension> <folderPath>
