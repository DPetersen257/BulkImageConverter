using System.Drawing.Imaging;

namespace BulkImageConverterWinForms;

public partial class Form1 : Form
{
    public Form1() => InitializeComponent();

    private void ButtonBrowse_Click(object sender, EventArgs e)
    {
        folderBrowserDialog1.ShowDialog();
        textBox1.Text = folderBrowserDialog1.SelectedPath;
    }

    private void Button1_Click(object sender, EventArgs e)
    {
        textBoxLog.Text = "";
        if (!ValidateForm())
            return;

        string directory = textBox1.Text;
        int converted = 0;
        string fromExtension = comboBoxFromExtension.Text;
        switch (comboBoxToExtension.SelectedItem)
        {
            case "JPG":
                converted = ConvertFilesToJPG(directory, fromExtension, checkBoxRecurse.Checked, checkBoxDelete.Checked);
                break;
        }

        AppendLog($"Converted: {converted} files.", Color.Green);
    }

    private int ConvertFilesToJPG(string directory, string fromExtension, bool recurseSubdirectories, bool deleteOld)
    {
        int converted = 0;
        foreach (string file in Directory.EnumerateFiles(directory, $"*{fromExtension}", new EnumerationOptions() { RecurseSubdirectories = recurseSubdirectories }))
        {
            try
            {
                string jpgFile = ConvertToJpg(file);
                // Copy the file access/creation times from the original file to the new file.
                File.SetCreationTime(jpgFile, File.GetCreationTime(file));
                File.SetLastWriteTime(jpgFile, File.GetLastWriteTime(file));
                File.SetLastAccessTime(jpgFile, File.GetLastAccessTime(file));
                File.SetAttributes(jpgFile, File.GetAttributes(file));

                if (deleteOld)
                    File.Delete(file);
                converted++;
                AppendLog($"Converted: {file}", Color.Black);
            }
            catch (Exception ex)
            {
                AppendLog($"Failed converting: {file}", Color.Red);
                Console.Error.WriteLine(ex.Message);
            }
        }
        return converted;
    }

    //This method appends a string to textboxLog in the specified color
    private void AppendLog(string text, Color color)
    {
        Color previousColor = textBoxLog.ForeColor;

        textBoxLog.SelectionStart = textBoxLog.TextLength;
        textBoxLog.SelectionLength = 0;
        textBoxLog.ForeColor = color;
        if (textBoxLog.Text.Length > 0)
            textBoxLog.AppendText(Environment.NewLine);
        textBoxLog.AppendText(text);
        textBoxLog.ForeColor = previousColor;
    }

    private bool ValidateForm()
    {
        if (comboBoxFromExtension.SelectedIndex == -1)
        {
            MessageBox.Show("Select FROM extension");
            return false;
        }
        if (comboBoxToExtension.SelectedIndex == -1)
        {
            MessageBox.Show("Select TO extension");
            return false;
        }
        if (string.IsNullOrWhiteSpace(textBox1.Text) || !Directory.Exists(textBox1.Text))
        {
            MessageBox.Show("Directory not valid");
            return false;
        }

        return true;
    }

    // This method will prompt the user for a directory path
    public static string? GetDirectoryPathFromUser()
    {
        Console.WriteLine("Enter a directory path");
        string? directoryPath = Console.ReadLine();
        return (Directory.Exists(directoryPath)) ? directoryPath : null;
    }

    //This method converts a .bmp file into a .jpg file
    public static string ConvertToJpg(string filePath)
    {
        string jpgFilePath = Path.ChangeExtension(filePath, ".jpg");
        using var bitmap = new Bitmap(filePath);
        bitmap.Save(jpgFilePath, ImageFormat.Jpeg);
        return jpgFilePath;
    }
}
