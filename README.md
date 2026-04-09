🛒 QuickCartFresh: Advanced Delivery Management System

![WhatsApp Image 2026-04-09 at 19 45 22](https://github.com/user-attachments/assets/efe03d2c-99cb-42a7-bf35-1702417c3846)


QuickCartFresh is a comprehensive software solution designed to streamline the logistics of a delivery company. Whether it's a restaurant or a bookstore, this system provides a robust infrastructure for managing orders, couriers, and real-time delivery tracking with a focus on precision and performance.
📸 Visual Showcase
Project Branding
Your Groceries, Faster. Fresh & Fast.

🏗️ System Architecture
The system is built on a Modular n-Tier Layered Architecture, ensuring high maintainability, scalability, and separation of concerns.

![WhatsApp Image 2026-04-09 at 19 45 06](https://github.com/user-attachments/assets/9c9c44ae-dc6f-48e2-a45b-5b9726c83864)

Core ComponentsDAL (Data Access Layer): Handles persistent data storage using both XML files for persistence and In-Memory lists for initial stages.BL (Business Logic Layer): The "brain" of the system. It processes complex delivery logic, calculates delivery times, and manages the custom system clock.PL (Presentation Layer): A sophisticated WPF graphical interface with data binding to ensure a responsive and modern user experience.

🌟 Key Features1. Dual-User PortalAdmin Dashboard: Full control over courier management, order tracking, and system configuration. Managers can monitor delivery histories and adjust the system clock.Courier Interface: Personalized portal for couriers to select available orders, update delivery statuses (Delivered, Refused, etc.), and view their own performance statistics.

2. Advanced Logistics EngineGeocoding & Pathfinding: Integrates external API logic for calculating distances based on transportation types (Vehicle, Motorcycle, Bicycle, or Walking).Real-time Simulator: Utilizes Multi-threading to simulate courier movement and order lifecycle, providing a dynamic view of system activity.3. Smart Data ManagementLINQ-Based Processing: Heavy use of LINQ for efficient data filtering, sorting, and grouping.TDD (Test-Driven Development): Built with a commitment to quality through rigorous unit testing of core logic.

🛠️ Software Engineering Principles
This system isn't just "code"—it's an engineered solution following industry standards:
Principle,Implementation in QuickCartFresh
Singleton Pattern,Ensures single instances of service providers and data managers.
Simple Factory,"Decouples object creation from implementation, allowing for flexible layer switching."
Async/Await,Used for non-blocking UI during heavy calculations like geocoding and distance pathfinding.+1
Data Binding,Implemented in WPF to synchronize the UI with underlying data models seamlessly.
Interface Segregation,Using DalApi and BIApi to define clear contracts between layers.+1

💻 Tech StackLanguage: Chttps://www.google.com/search?q=%23 (.NET) UI Framework: WPF (Windows Presentation Foundation) Data Handling: LINQ, XML Architecture: n-Tier Layered Model Concurrency: Multi-threading & Task Parallel Library

🚀 Getting Started
Clone the repository. 

Open in Visual Studio: Load the .sln file.

Restore Packages: Ensure all .NET dependencies are installed.

Add Configuration File: Create a file named secrets.json in the project's root directory. This file must follow a specific JSON structure to allow the application to authenticate with external services:

Structure: The file should contain a single JSON object.

Required Keys: Ensure the following keys are present:

OpenCageApiKey: Your API key for geolocation services.

SmtpUsername: The email address used for sending automated messages.

SmtpPassword: The App Password (not your regular login password) for the SMTP server.

AdminEmail: The destination address for administrative notifications.

Format Example:

JSON
{
  "OpenCageApiKey": "YOUR_KEY_HERE",
  "SmtpUsername": "YOUR_EMAIL_HERE",
  "SmtpPassword": "YOUR_APP_PASSWORD_HERE",
  "AdminEmail": "ADMIN_EMAIL_HERE"
}

Run: Launch the WPF application to start the simulator.
