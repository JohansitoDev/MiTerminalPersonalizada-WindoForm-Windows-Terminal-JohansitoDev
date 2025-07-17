using System;
using System.Diagnostics;
using System.Drawing; 
using System.IO;
using System.Windows.Forms;

namespace MiTerminalPersonalizada
{
    public partial class Form1 : Form
    {
        private string currentDirectory; 

        public Form1()
        {
            InitializeComponent();
            InitializeTerminal();
        }

        private void InitializeTerminal()
        {
            
            this.Text = "Mi Terminal Personalizada";
            this.Size = new Size(800, 600); 

           
            RichTextBox outputBox = new RichTextBox();
            outputBox.Name = "OutputBox";
            outputBox.Dock = DockStyle.Fill; 
            outputBox.ReadOnly = true; 
            outputBox.BackColor = Color.Black; 
            outputBox.ForeColor = Color.LimeGreen; 
            outputBox.Font = new Font("Consolas", 10); 

           
            TextBox inputBox = new TextBox();
            inputBox.Name = "InputBox";
            inputBox.Dock = DockStyle.Bottom;
            inputBox.BackColor = Color.Black;
            inputBox.ForeColor = Color.White;
            inputBox.Font = new Font("Consolas", 10);
            inputBox.KeyUp += InputBox_KeyUp; 

          
            Panel mainPanel = new Panel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Controls.Add(outputBox); 
            mainPanel.Controls.Add(inputBox);  

            this.Controls.Add(mainPanel);

            
            currentDirectory = Environment.CurrentDirectory;

          
            AppendLine($"Hola, {Environment.UserName}! Bienvenido a tu terminal personalizada.");
            AppendLine($"Directorio actual: {currentDirectory}");
            AppendLine("Escribe 'help' para ver los comandos.");
            AppendLine(""); 
            Prompt();
        }

        private void InputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox inputBox = sender as TextBox;
                string command = inputBox.Text.Trim();
                inputBox.Clear(); 

                AppendLine($"> {command}"); 
                ExecuteCommand(command);
                Prompt(); 
            }
        }

        private void AppendLine(string text, Color? color = null)
        {
            RichTextBox outputBox = this.Controls.Find("OutputBox", true)[0] as RichTextBox;
            if (outputBox == null) return;

            outputBox.SelectionStart = outputBox.TextLength;
            outputBox.SelectionLength = 0;

            outputBox.SelectionColor = color ?? outputBox.ForeColor; 
            outputBox.AppendText(text + Environment.NewLine);
            outputBox.ScrollToCaret(); 
        }

        private void Prompt()
        {
            AppendLine($"{currentDirectory}> ", Color.LightGray); 
        }

        private void ExecuteCommand(string command)
        {
            string[] parts = command.Split(' ', 2);
            string cmd = parts[0].ToLower();
            string args = parts.Length > 1 ? parts[1] : "";

            switch (cmd)
            {
                case "help":
                    AppendLine("Comandos disponibles:");
                    AppendLine("  cd <ruta>     - Cambia el directorio actual.");
                    AppendLine("  ls            - Lista el contenido del directorio actual (archivos y carpetas).");
                    AppendLine("  mkdir <nombre> - Crea una nueva carpeta.");
                    AppendLine("  cls           - Limpia la pantalla.");
                    AppendLine("  bgcolor <nombre_color> - Cambia el color de fondo (ej: black, blue, red).");
                    AppendLine("  fgcolor <nombre_color> - Cambia el color del texto (ej: limegreen, white, yellow).");
                    AppendLine("  exec <comando_windows> [args] - Ejecuta un comando externo de Windows (ej: ping google.com).");
                    AppendLine("  exit          - Cierra la terminal.");
                    break;

                case "cd":
                    HandleCdCommand(args);
                    break;

                case "ls":
                    HandleLsCommand(args);
                    break;

                case "mkdir":
                    HandleMkdirCommand(args);
                    break;

                case "cls":
                    (this.Controls.Find("OutputBox", true)[0] as RichTextBox)?.Clear();
                    break;

                case "bgcolor":
                    SetBackgroundColor(args);
                    break;

                case "fgcolor":
                    SetForegroundColor(args);
                    break;

                case "exec":
                    ExecuteExternalCommand(args);
                    break;

                case "exit":
                    Application.Exit();
                    break;

                default:
                  
                    ExecuteExternalCommand(command);
                    break;
            }
        }

        private void HandleCdCommand(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    AppendLine(currentDirectory);
                    return;
                }

               
                string newPath = Path.GetFullPath(Path.Combine(currentDirectory, path));

                if (Directory.Exists(newPath))
                {
                    currentDirectory = newPath;
                    Environment.CurrentDirectory = newPath; 
                    AppendLine($"Directorio cambiado a: {currentDirectory}");
                }
                else
                {
                    AppendLine($"Error: El directorio '{path}' no existe.", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendLine($"Error al cambiar de directorio: {ex.Message}", Color.Red);
            }
        }

        private void HandleLsCommand(string args)
        {
            try
            {
                string targetPath = currentDirectory;
                if (!string.IsNullOrWhiteSpace(args))
                {
                    targetPath = Path.GetFullPath(Path.Combine(currentDirectory, args));
                    if (!Directory.Exists(targetPath))
                    {
                        AppendLine($"Error: El directorio '{args}' no existe.", Color.Red);
                        return;
                    }
                }

                AppendLine($"Contenido de '{targetPath}':");
                foreach (string dir in Directory.GetDirectories(targetPath))
                {
                    AppendLine($"  <DIR> {Path.GetFileName(dir)}", Color.Cyan); 
                foreach (string file in Directory.GetFiles(targetPath))
                {
                    AppendLine($"  {Path.GetFileName(file)}", Color.White); 
                }
            }
            catch (Exception ex)
            {
                AppendLine($"Error al listar: {ex.Message}", Color.Red);
            }
        }

        private void HandleMkdirCommand(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                AppendLine("Uso: mkdir <nombre_carpeta>", Color.Yellow);
                return;
            }

            try
            {
                string newFolderPath = Path.Combine(currentDirectory, folderName);
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    AppendLine($"Carpeta '{folderName}' creada con éxito.");
                }
                else
                {
                    AppendLine($"Error: La carpeta '{folderName}' ya existe.", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendLine($"Error al crear carpeta: {ex.Message}", Color.Red);
            }
        }

        private void SetBackgroundColor(string colorName)
        {
            try
            {
                Color newColor = Color.FromName(colorName);
                (this.Controls.Find("OutputBox", true)[0] as RichTextBox).BackColor = newColor;
                (this.Controls.Find("InputBox", true)[0] as TextBox).BackColor = newColor;
                AppendLine($"Color de fondo cambiado a: {colorName}");
            }
            catch
            {
                AppendLine($"Color '{colorName}' no reconocido. Prueba con colores estándar como Black, Blue, Green, Red, White.", Color.Red);
            }
        }

        private void SetForegroundColor(string colorName)
        {
            try
            {
                Color newColor = Color.FromName(colorName);
                (this.Controls.Find("OutputBox", true)[0] as RichTextBox).ForeColor = newColor;
                (this.Controls.Find("InputBox", true)[0] as TextBox).ForeColor = newColor;
                AppendLine($"Color del texto cambiado a: {colorName}");
            }
            catch
            {
                AppendLine($"Color '{colorName}' no reconocido. Prueba con colores estándar como LimeGreen, White, Yellow, Cyan.", Color.Red);
            }
        }

        private void ExecuteExternalCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true, 
                    WorkingDirectory = currentDirectory 
                };

                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        AppendLine(output);
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        AppendLine("ERROR: " + error, Color.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLine($"Error al ejecutar el comando externo: {ex.Message}", Color.Red);
            }
        }
    }
}
