# JFiler

**JFiler** is a lightweight file management application designed to help you manage and organize your drives and storage settings with ease.

## Features
- **Manage Drives**:
  - View details of connected drives, including total and free space.
  - Add or remove drives dynamically via the admin interface.
- **User-Friendly Interface**:
  - Simple and intuitive UI for easy navigation.
- **Configuration Persistence**:
  - Changes made to drives are persisted in `appsettings.json`.

## Configuration
Before running the application, configure the following settings in the `appsettings.json` file:

- Ensure the StorageSettings:Drives section is initialized (even if empty).
  ## Installation

1. **Clone the repository**:
    ```bash
    git clone https://github.com/yourusername/jfiler.git
    ```
2. **Navigate to the project directory**:
    ```bash
    cd jfiler
    ```
3. **Build and run the application**:
    ```bash
    dotnet build
    dotnet run
    ```

## Usage

### Access the Admin Interface
Log in using the admin username and password specified in the `appsettings.json` file.

You can change the username nad password but a restart of the website is required for that to take effect

### Drive Management
- **View Drives**: Displays a list of all configured drives with their total and free space.
- **Add Drives**: Use the "Add Drive" form to add new drives by specifying the drive path.
- **Remove Drives**: Click the "Remove" button next to a drive to delete it from the configuration.

## Customizing `appsettings.json`
You can directly edit the `appsettings.json` file to configure your settings. For example:
```json
{
  "AdminUsername": "admin",
  "AdminPassword": "password",
  "StorageSettings": {
    "Drives": [
      { "DrivePath": "C:\\Nas" },
      { "DrivePath": "D:\\Nas" }
    ]
  }
}
```
## Troubleshooting
- If the application fails to start, ensure the `appsettings.json` file is correctly formatted and contains all required keys.
- For invalid drive paths or errors during drive updates, check the application logs for detailed messages.

## Contributions
Contributions are welcome! Feel free to fork the repository, make changes, and submit a pull request.

## License
This project is licensed under the [MIT License](LICENSE).

# Ui
I decided to make a very spartan ui since I was tired of javascript frameworks :D 

