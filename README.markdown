# MBBSF Metrics Dashboard

## Overview
The MBBSF Metrics Dashboard is a web application developed by the UNF Digital Talons, a team of senior undergraduates in Information Science and Systems at the University of North Florida, as part of our senior capstone project. Designed for the Marie Barney Boston Scholarship Foundation (MBBSF), this dashboard provides actionable insights to support MBBSF’s mission of empowering high-potential students from underserved communities. The application aggregates and visualizes key performance indicators (KPIs) for MBBSF’s Google Certification, Mentoring, and Scholarship Application programs, enabling data-driven decision-making.

## Features
- **Real-Time KPI Tracking**: Displays metrics such as total participants, completion rates, and session counts for Google Certification, Mentoring, and Scholarship programs.
- **Platform-Specific Reports**: Interactive, sortable tables for Coursera (Specialization, Membership, Location, Usage), Cognito, and Google Forms data, with bulk deletion capabilities.
- **Data Export**: Supports CSV and PDF exports of KPIs and reports using JavaScript and jsPDF.
- **Responsive UI**: Built with Bootstrap and custom CSS for a user-friendly, mobile-friendly interface with hover effects, custom dropdowns, and comment toggling.
- **Activity Logs**: Tracks user actions (e.g., deletions, searches) with timestamps for auditing.
- **Search Functionality**: Allows searching across all platform data with a clear query option.

## Technology Stack
- **Backend**: ASP.NET Core MVC, C#, Entity Framework Core 9.0.4
- **Database**: Azure SQL Database (migrated from local SQL Server)
- **Frontend**: Bootstrap 5, Custom CSS, JavaScript
- **Export Libraries**: jsPDF for PDF generation
- **Deployment**: Azure App Service with Zip Deploy
- **Configuration**: Managed via `appsettings.json` for secure database connections
- **Version Control**: Git, hosted on GitHub

## Project Structure
```
MBBSF-Dashboard/
├── MBBS.Dashboard.web/                # ASP.NET Core MVC project
│   ├── Controllers/                   # MVC controllers (e.g., DashboardController)
│   ├── Models/                        # ViewModels (e.g., DashboardViewModel)
│   ├── Views/                         # Razor views (e.g., Dashboard.cshtml)
│   ├── wwwroot/                       # Static assets (CSS, JS)
│   ├── appsettings.json               # Configuration (e.g., Azure SQL connection)
├── .gitignore                         # Git ignore file
├── README.md                          # This file
```

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Account](https://azure.microsoft.com/) for SQL Database and App Service
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/) or Azure Data Studio
- [Git](https://git-scm.com/) for cloning the repository
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or VS Code

## Setup Instructions
1. **Clone the Repository**:
   ```bash
   git clone https://github.com/your-username/MBBSF-Dashboard.git
   cd MBBSF-Dashboard
   ```

2. **Configure the Database**:
   - Set up an Azure SQL Database or local SQL Server instance.
   - Update the connection string in `appsettings.json`:
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=tcp:mbbs11.database.windows.net,1433;Initial Catalog=mbbs;Persist Security Info=False;User ID=your-user;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
     }
     ```
   - Run Entity Framework migrations to create the database schema:
     ```bash
     dotnet ef migrations add InitialCreate
     dotnet ef database update
     ```

3. **Restore Dependencies**:
   ```bash
   dotnet restore
   ```

4. **Run the Application**:
   ```bash
   dotnet run
   ```
   - Access the dashboard at `https://localhost:5001/Dashboard`.

5. **Deploy to Azure**:
   - Create an Azure App Service instance.
   - Use Zip Deploy to publish:
     ```bash
     dotnet publish -c Release
     zip -r publish.zip bin/Release/net9.0/publish/
     az webapp deployment source config-zip --resource-group your-resource-group --name your-app-name --src publish.zip
     ```
   - Configure the Azure App Service with the connection string in the Application Settings.

## Usage
- **Access the Dashboard**: Navigate to `/Dashboard` to view KPIs, platform data, and activity logs.
- **Search Data**: Use the search bar to filter across all platforms (Coursera, Cognito, Google Forms).
- **Export Reports**: Select “Export” from the dropdown to download KPIs and reports as CSV or PDF.
- **Manage Records**: Check records in platform-specific tables and use the “Delete Selected” button (with confirmation) to remove data.
- **View Comments**: In the Google Forms table, click `[+]` to expand/collapse comments.

## Screenshots
*Coming soon: Dashboard overview, platform reports, and export dropdown.*

## Contributing
This project was developed as a capstone and is not actively maintained. However, suggestions or bug reports are welcome! Please:
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-feature`).
3. Commit changes (`git commit -m "Add your feature"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a Pull Request.

## Team
- UNF Digital Talons (Senior Capstone Team, University of North Florida)
- Contributors: [Your Name] (Front-End, Export Functionality, Azure Integration), [Other Team Members]

## License
This project is licensed under the [MIT License](LICENSE).

## Acknowledgments
- Marie Barney Boston Scholarship Foundation for their collaboration and mission-driven work.
- University of North Florida for academic support.
- Azure for providing cloud infrastructure.