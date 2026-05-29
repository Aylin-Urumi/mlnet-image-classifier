# Digit Classifier AI

A cross-platform desktop application built with C# and .NET that trains a local machine learning model to recognize handwritten digits
and classifies user-uploaded images in real-time.

## 🚀 Features

* **Local Machine Learning:** Trains an ML.NET pipeline instantly on startup using the L-BFGS Maximum Entropy algorithm.
* **Modern Cross-Platform UI:** Built with Avalonia UI following the MVVM (Model-View-ViewModel) architectural pattern.
* **Asynchronous Processing:** Utilizes background threads (`Task.Run`) to keep the user interface completely fluid and responsive during heavy AI math.
* **Confidence Metrics:** Displays the predicted digit alongside a real-time mathematical confidence percentage.
* **Reset Functionality:** Includes a clear feature to reset the application state instantly without restarting.

---

## 🛠️ Tech Stack

* **Language:** C#
* **Framework:** .NET 8.0+
* **Machine Learning:** ML.NET (`Microsoft.ML`)
* **UI Framework:** Avalonia UI
* **Architecture:** CommunityToolkit.Mvvm
* **Graphics:** SkiaSharp (configured for native macOS compatibility)

---

## 📁 Dataset Structure

The application trains itself automatically using a folder-based dataset. Ensure your training images are arranged as follows:

```text
Data/
├── 0/ (Images of zeros)
├── 1/ (Images of ones)
...
└── 9/ (Images of nines)

Note: Images are dynamically preprocessed and normalized to a standard 28x28 pixel resolution for lightning-fast training execution.

## ⚙️ How to Run
Prerequisites
.NET SDK installed on your machine.

A training dataset placed inside the Data/ directory as structured above.

Execution Commands
Open your terminal in the project root directory and run the following commands to clear old build artifacts and start the application:

dotnet clean
dotnet run

Note on Architecture: This project demonstrates proper separation of concerns by isolating the Machine Learning pipeline wrapper (MLService)
from the UI layout (MainWindow.axaml) via data-bound state management in the ViewModels.
