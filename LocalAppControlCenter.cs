using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LocalAppControlCenter
{
    public class ServicePanel : GroupBox
    {
        public TextBox PathBox;
        public TextBox CommandBox;
        public NumericUpDown PortBox;
        public Label StatusLabel;
        public Label PidLabel;
        public Button StartButton;
        public Button OpenButton;
        public Button StopButton;
        public Button BrowseButton;
        public int LaunchedRootPid = 0;
        public bool StartRequested = false;
        public DateTime StartRequestedAt = DateTime.MinValue;

        public ServicePanel(string title, int y)
        {
            Text = title;
            ForeColor = Color.White;
            BackColor = Color.FromArgb(30, 34, 42);
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Location = new Point(20, y);
            Size = new Size(700, 245);

            Label pathLabel = MakeLabel("Project folder", 18, 35, 110);
            Controls.Add(pathLabel);

            PathBox = new TextBox();
            PathBox.Location = new Point(130, 31);
            PathBox.Size = new Size(450, 27);
            PathBox.Font = new Font("Segoe UI", 9F);
            Controls.Add(PathBox);

            BrowseButton = MakeButton("Browse...", 590, 30, 90, 30);
            Controls.Add(BrowseButton);

            Label cmdLabel = MakeLabel("Start command", 18, 77, 110);
            Controls.Add(cmdLabel);

            CommandBox = new TextBox();
            CommandBox.Location = new Point(130, 73);
            CommandBox.Size = new Size(550, 27);
            CommandBox.Font = new Font("Consolas", 9F);
            Controls.Add(CommandBox);

            Label portLabel = MakeLabel("Port", 18, 119, 110);
            Controls.Add(portLabel);

            PortBox = new NumericUpDown();
            PortBox.Location = new Point(130, 115);
            PortBox.Size = new Size(100, 27);
            PortBox.Minimum = 1;
            PortBox.Maximum = 65535;
            PortBox.Font = new Font("Segoe UI", 9F);
            Controls.Add(PortBox);

            StatusLabel = MakeLabel("STOPPED", 270, 116, 180);
            StatusLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            StatusLabel.ForeColor = Color.IndianRed;
            Controls.Add(StatusLabel);

            PidLabel = MakeLabel("PID: -", 430, 116, 220);
            PidLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            PidLabel.ForeColor = Color.Silver;
            Controls.Add(PidLabel);

            StartButton = MakeButton("START", 130, 165, 130, 42);
            OpenButton = MakeButton("OPEN", 275, 165, 130, 42);
            StopButton = MakeButton("STOP", 420, 165, 130, 42);

            Controls.Add(StartButton);
            Controls.Add(OpenButton);
            Controls.Add(StopButton);
        }

        private Label MakeLabel(string text, int x, int y, int width)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.Size = new Size(width, 25);
            l.ForeColor = Color.White;
            l.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            return l;
        }

        private Button MakeButton(string text, int x, int y, int width, int height)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(width, height);
            b.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(90, 100, 120);
            b.BackColor = Color.FromArgb(48, 54, 66);
            b.ForeColor = Color.White;
            b.Cursor = Cursors.Hand;
            return b;
        }
    }

    public class ControlCenterForm : Form
    {
        private ServicePanel frontend;
        private ServicePanel backend;
        private Timer timer;
        private string configFile;
        private Label lastRefresh;

        public ControlCenterForm()
        {
            Text = "Local Development Control Center";
            Size = new Size(760, 690);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(22, 25, 31);
            Font = new Font("Segoe UI", 9F);

            configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "controlcenter.ini");

            Label title = new Label();
            title.Text = "LOCAL DEVELOPMENT CONTROL CENTER";
            title.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.AutoSize = true;
            title.Location = new Point(115, 18);
            Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "Visual control for your frontend and backend ports";
            subtitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            subtitle.ForeColor = Color.Silver;
            subtitle.AutoSize = true;
            subtitle.Location = new Point(200, 53);
            Controls.Add(subtitle);

            frontend = new ServicePanel("Frontend", 85);
            backend = new ServicePanel("Backend API", 340);
            Controls.Add(frontend);
            Controls.Add(backend);

            Button save = MakeFooterButton("SAVE SETTINGS", 20, 600, 165);
            Button startAll = MakeFooterButton("START ALL", 200, 600, 150);
            Button stopAll = MakeFooterButton("STOP ALL", 365, 600, 150);
            Button refresh = MakeFooterButton("REFRESH", 530, 600, 120);
            Controls.Add(save);
            Controls.Add(startAll);
            Controls.Add(stopAll);
            Controls.Add(refresh);

            lastRefresh = new Label();
            lastRefresh.ForeColor = Color.Gray;
            lastRefresh.Font = new Font("Segoe UI", 8F);
            lastRefresh.AutoSize = true;
            lastRefresh.Location = new Point(655, 611);
            Controls.Add(lastRefresh);

            frontend.PortBox.Value = 5173;
            frontend.CommandBox.Text = "npm run dev";
            frontend.PathBox.Text = AppDomain.CurrentDomain.BaseDirectory;

            backend.PortBox.Value = 8000;
            backend.CommandBox.Text = "python -m uvicorn main:app --reload --port 8000";
            backend.PathBox.Text = AppDomain.CurrentDomain.BaseDirectory;

            frontend.BrowseButton.Click += delegate { BrowseForFolder(frontend.PathBox); };
            backend.BrowseButton.Click += delegate { BrowseForFolder(backend.PathBox); };

            frontend.StartButton.Click += delegate { StartService(frontend, "Frontend"); };
            backend.StartButton.Click += delegate { StartService(backend, "Backend API"); };

            frontend.StopButton.Click += delegate { StopService(frontend, "Frontend"); };
            backend.StopButton.Click += delegate { StopService(backend, "Backend API"); };

            frontend.OpenButton.Click += delegate { OpenService(frontend); };
            backend.OpenButton.Click += delegate { OpenService(backend); };

            save.Click += delegate { SaveConfig(true); };
            startAll.Click += delegate
            {
                StartService(frontend, "Frontend");
                StartService(backend, "Backend API");
            };
            stopAll.Click += delegate
            {
                StopService(frontend, "Frontend");
                StopService(backend, "Backend API");
            };
            refresh.Click += delegate { RefreshStatus(); };

            LoadConfig();

            timer = new Timer();
            timer.Interval = 500;
            timer.Tick += delegate { RefreshStatus(); };
            timer.Start();

            Shown += delegate { RefreshStatus(); };
            FormClosing += delegate { SaveConfig(false); };
        }

        private Button MakeFooterButton(string text, int x, int y, int width)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(width, 38);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderColor = Color.FromArgb(90, 100, 120);
            b.BackColor = Color.FromArgb(48, 54, 66);
            b.ForeColor = Color.White;
            b.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private void BrowseForFolder(TextBox box)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select the project folder";
                if (Directory.Exists(box.Text)) dlg.SelectedPath = box.Text;
                if (dlg.ShowDialog() == DialogResult.OK)
                    box.Text = dlg.SelectedPath;
            }
        }

        private void StartService(ServicePanel panel, string serviceName)
        {
            int port = Convert.ToInt32(panel.PortBox.Value);
            List<int> existing = GetPidsForPort(port);
            if (existing.Count > 0)
            {
                MessageBox.Show(serviceName + " is already listening on port " + port + ".",
                    "Already running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string folder = panel.PathBox.Text.Trim();
            string command = panel.CommandBox.Text.Trim();

            // npm commands must normally run from the folder containing package.json.
            // If the user selected a child folder such as \src, automatically walk upward
            // and use the nearest npm project root.
            string resolvedFolder = ResolveWorkingDirectory(folder, command);
            if (!String.Equals(resolvedFolder, folder, StringComparison.OrdinalIgnoreCase))
            {
                folder = resolvedFolder;
                panel.PathBox.Text = resolvedFolder;
            }

            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Project folder does not exist:\n" + folder,
                    "Invalid folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (command.Length == 0)
            {
                MessageBox.Show("Enter a start command for " + serviceName + ".",
                    "Missing command", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "cmd.exe";
                // /c keeps the console open while the dev server is running, then closes it
                // automatically when the process tree is stopped.
                psi.Arguments = "/c cd /d \"" + folder + "\" && " + command;
                psi.WorkingDirectory = folder;
                psi.UseShellExecute = true;
                Process launched = Process.Start(psi);
                if (launched != null)
                {
                    panel.LaunchedRootPid = launched.Id;
                    panel.StartRequested = true;
                    panel.StartRequestedAt = DateTime.Now;
                    ShowRunningVisual(panel, port, launched.Id, true);
                }
                SaveConfig(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start " + serviceName + ":\n\n" + ex.Message,
                    "Start error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ResolveWorkingDirectory(string folder, string command)
        {
            if (!Directory.Exists(folder)) return folder;

            string trimmed = command.TrimStart();
            bool isNpm = trimmed.StartsWith("npm ", StringComparison.OrdinalIgnoreCase) ||
                         trimmed.Equals("npm", StringComparison.OrdinalIgnoreCase) ||
                         trimmed.StartsWith("npx ", StringComparison.OrdinalIgnoreCase);

            if (!isNpm) return folder;
            if (File.Exists(Path.Combine(folder, "package.json"))) return folder;

            DirectoryInfo current = new DirectoryInfo(folder);
            for (int i = 0; i < 5 && current != null; i++)
            {
                if (File.Exists(Path.Combine(current.FullName, "package.json")))
                    return current.FullName;
                current = current.Parent;
            }

            return folder;
        }

        private void ShowRunningVisual(ServicePanel panel, int port, int pid, bool starting)
        {
            // Bright fluorescent green requested for the active state.
            panel.StatusLabel.Text = "RUNNING   Port " + port;
            panel.StatusLabel.ForeColor = Color.FromArgb(57, 255, 20);
            panel.PidLabel.Text = starting ? "PID: " + pid + "  • starting..." : "PID: " + pid;
            panel.PidLabel.ForeColor = Color.Gainsboro;
            panel.StartButton.Enabled = false;
            panel.StopButton.Enabled = true;
            panel.OpenButton.Enabled = !starting;
        }

        private void StopService(ServicePanel panel, string serviceName)
        {
            int port = Convert.ToInt32(panel.PortBox.Value);
            bool attempted = false;

            // Preferred path: kill the complete CMD/npm/python process tree that this app launched.
            // This is important for Vite/npm and uvicorn --reload, which often use child processes.
            if (panel.LaunchedRootPid > 0 && IsProcessAlive(panel.LaunchedRootPid))
            {
                attempted = true;
                KillProcessTree(panel.LaunchedRootPid);
                panel.LaunchedRootPid = 0;
                panel.StartRequested = false;
                WaitForPortToClose(port, 2500);
            }

            // Fallback: if the service was started before the Control Center was opened,
            // kill whatever is currently listening on the configured port.
            List<int> pids = GetPidsForPort(port);
            foreach (int pid in pids)
            {
                attempted = true;
                KillProcessTree(pid);
            }

            WaitForPortToClose(port, 2500);
            RefreshStatus();

            if (GetPidsForPort(port).Count > 0)
            {
                MessageBox.Show(
                    serviceName + " is still listening on port " + port + ".\n\n" +
                    "Try running the Control Center as Administrator if that process was started by another user or elevated terminal.",
                    "Could not fully stop service",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else if (!attempted)
            {
                RefreshStatus();
            }
        }

        private bool IsProcessAlive(int pid)
        {
            try
            {
                Process p = Process.GetProcessById(pid);
                return !p.HasExited;
            }
            catch
            {
                return false;
            }
        }

        private void KillProcessTree(int pid)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "taskkill.exe";
                psi.Arguments = "/PID " + pid + " /T /F";
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                using (Process killer = Process.Start(psi))
                {
                    if (killer != null)
                    {
                        killer.WaitForExit(5000);
                    }
                }
            }
            catch
            {
                // The caller verifies the port afterwards and reports failure if needed.
            }
        }

        private void WaitForPortToClose(int port, int timeoutMs)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (GetPidsForPort(port).Count == 0)
                    return;
                System.Threading.Thread.Sleep(150);
            }
        }

        private void OpenService(ServicePanel panel)
        {
            int port = Convert.ToInt32(panel.PortBox.Value);
            string url = "http://localhost:" + port;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(url);
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open " + url + ".\n\n" + ex.Message,
                    "Open error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<int> GetPidsForPort(int port)
        {
            List<int> result = new List<int>();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "netstat.exe";
                psi.Arguments = "-ano -p TCP";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(3000);
                    string[] lines = output.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string raw in lines)
                    {
                        string line = raw.Trim();
                        if (!line.StartsWith("TCP", StringComparison.OrdinalIgnoreCase)) continue;

                        string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 5) continue;

                        string localAddress = parts[1];
                        string state = parts[3];
                        int pid;

                        if (!state.Equals("LISTENING", StringComparison.OrdinalIgnoreCase)) continue;
                        if (!localAddress.EndsWith(":" + port, StringComparison.OrdinalIgnoreCase)) continue;
                        if (!Int32.TryParse(parts[4], out pid)) continue;
                        if (!result.Contains(pid)) result.Add(pid);
                    }
                }
            }
            catch
            {
                // Keep the GUI alive even if netstat is temporarily unavailable.
            }
            return result;
        }

        private void RefreshStatus()
        {
            UpdatePanelStatus(frontend);
            UpdatePanelStatus(backend);
            lastRefresh.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void UpdatePanelStatus(ServicePanel panel)
        {
            int port = Convert.ToInt32(panel.PortBox.Value);
            List<int> pids = GetPidsForPort(port);
            bool portListening = pids.Count > 0;

            if (portListening)
            {
                panel.StartRequested = false;
                panel.StatusLabel.Text = "RUNNING   Port " + port;
                panel.StatusLabel.ForeColor = Color.FromArgb(57, 255, 20);
                panel.PidLabel.Text = "PID: " + String.Join(", ", pids.ToArray());
                panel.PidLabel.ForeColor = Color.Gainsboro;
                panel.StartButton.Enabled = false;
                panel.StopButton.Enabled = true;
                panel.OpenButton.Enabled = true;
                return;
            }

            bool rootAlive = panel.LaunchedRootPid > 0 && IsProcessAlive(panel.LaunchedRootPid);
            if (panel.StartRequested && rootAlive)
            {
                // Do not flash back to STOPPED during npm/Vite/Node startup.
                // Keep the requested bright RUNNING state while the child server binds its port.
                ShowRunningVisual(panel, port, panel.LaunchedRootPid, true);
                return;
            }

            if (rootAlive)
            {
                // A launched server process still exists even if the configured port is not
                // listening yet. Keep it visibly active so STOP remains available.
                ShowRunningVisual(panel, port, panel.LaunchedRootPid, true);
                return;
            }

            panel.StartRequested = false;
            panel.LaunchedRootPid = 0;
            panel.StatusLabel.Text = "STOPPED   Port " + port;
            panel.StatusLabel.ForeColor = Color.IndianRed;
            panel.PidLabel.Text = "PID: -";
            panel.PidLabel.ForeColor = Color.Silver;
            panel.StartButton.Enabled = true;
            panel.StopButton.Enabled = false;
            panel.OpenButton.Enabled = false;
        }

        private void LoadConfig()
        {
            if (!File.Exists(configFile)) return;
            try
            {
                Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string raw in File.ReadAllLines(configFile))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1);
                    values[key] = value;
                }

                ApplyConfig(values, "frontend", frontend);
                ApplyConfig(values, "backend", backend);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Settings could not be loaded:\n\n" + ex.Message,
                    "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyConfig(Dictionary<string, string> values, string prefix, ServicePanel panel)
        {
            string value;
            if (values.TryGetValue(prefix + "_path", out value)) panel.PathBox.Text = value;
            if (values.TryGetValue(prefix + "_command", out value)) panel.CommandBox.Text = value;
            if (values.TryGetValue(prefix + "_port", out value))
            {
                decimal port;
                if (Decimal.TryParse(value, out port) && port >= 1 && port <= 65535)
                    panel.PortBox.Value = port;
            }
        }

        private void SaveConfig(bool showConfirmation)
        {
            try
            {
                string[] lines = new string[]
                {
                    "# Local Development Control Center settings",
                    "frontend_path=" + frontend.PathBox.Text,
                    "frontend_command=" + frontend.CommandBox.Text,
                    "frontend_port=" + frontend.PortBox.Value,
                    "backend_path=" + backend.PathBox.Text,
                    "backend_command=" + backend.CommandBox.Text,
                    "backend_port=" + backend.PortBox.Value
                };
                File.WriteAllLines(configFile, lines);
                if (showConfirmation)
                    MessageBox.Show("Settings saved.", "Control Center",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (showConfirmation)
                    MessageBox.Show("Settings could not be saved:\n\n" + ex.Message,
                        "Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ControlCenterForm());
        }
    }
}
