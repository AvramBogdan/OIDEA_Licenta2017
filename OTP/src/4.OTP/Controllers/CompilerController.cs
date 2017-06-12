using Microsoft.AspNetCore.Mvc;
using OTP.ViewModels;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using System;
using LibGit2Sharp.Handlers;

namespace OTP.Controllers
{
    public class CompilerController : Controller
    {
        private static ErrorData errorData;
        private static string currentProject_Path = "";
        private static CompilerData data = new CompilerData { Inputs = new Dictionary<string, string>() };
        private static long maxSpace = 200000000;
        private bool proccessTerminated = false;

        [HttpPost]
        public IActionResult get_dictValue(string file)
        {
            string[] files = Directory.GetFiles(currentProject_Path);
            string content = "";
            
            foreach(string ffile in files)
            {
                if (ffile.Substring(ffile.LastIndexOf("\\") + 1) == file)
                {
                    file = Path.Combine(currentProject_Path, file);
                    content = System.IO.File.ReadAllText(file);
                }

            }
            
            return Json(content);
        }

        /*get the first file to be set on the tab menu*/
        private string get_FirstFile()
        {
            string[] files = Directory.GetFiles(currentProject_Path);

            foreach (string ffile in files)
            {
                /* NOT FINISHED : K - case */
                if (ffile.IndexOf(".c") > 0 || ffile.IndexOf(".py") > 0)
                    return ffile.Substring(ffile.LastIndexOf("\\") + 1);  
               
            }

            return "NewFile";
        }

        private long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        private void get_UserSpace()
        {
            long usedSpace = 0;
            string userPath = currentProject_Path.Substring(0,currentProject_Path.LastIndexOf("\\"));

            string[] dirs = Directory.GetDirectories(userPath);
            foreach (string dir in dirs)
            {

                usedSpace += DirSize(new DirectoryInfo(dir));

            }

            data.AvailableSpace = System.Math.Round(System.Convert.ToDouble((usedSpace * 100.00) / maxSpace),2);

            /* set to real value */
            if (data.AvailableSpace == 0)
                data.AvailableSpace = 0.1;

        }


        private void set_Inputs()
        {
            string[] files = Directory.GetFiles(currentProject_Path);
            string content;

            foreach(string file in files)
            {
                if (file.IndexOf(".c") >= 0 || file.IndexOf(".py") >= 0)
                {
                    content = System.IO.File.ReadAllText(Path.Combine(currentProject_Path, file));

                    if (data.Inputs.ContainsKey(file.Substring(file.LastIndexOf("\\") + 1)))
                        data.Inputs.Remove(file.Substring(file.LastIndexOf("\\") + 1));

                    data.Inputs.Add(file.Substring(file.LastIndexOf("\\") + 1),content);
                }
            }
        }

        private string get_FilesAndFolders(string path)
        {
            string data = "";

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (file.Substring(file.LastIndexOf("\\") + 1).Trim() != "Config.txt" && file.Substring(file.LastIndexOf("\\") + 1).Trim() != "a.exe")
                    data += file.Substring(file.LastIndexOf("\\") + 1) + "\n";
            }

            files = Directory.GetDirectories(path);
            foreach (string file in files)
            {
                data += file.Substring(file.LastIndexOf("\\") + 1) + "\n";
            }

            return data;
        }

        private string list_items(string option, string parameter )
        {
            string output = "";

            if (option == "")
            {
                output = get_FilesAndFolders(currentProject_Path);
                return output;
            }
            else if (option == "-d")
            {
                string currentFolder = Path.Combine(currentProject_Path, parameter.Trim());
                output = get_FilesAndFolders(currentFolder);
                return output;   

            }

            return null;
        }

        private string make_dir(string[] options)
        {
            string data = "";
            string newfolder = "";
            if (options.Length == 2)
            {
                newfolder = Path.Combine(currentProject_Path, options[1]);
                Directory.CreateDirectory(newfolder);
                data = "The folder " + newfolder.Substring(newfolder.LastIndexOf("\\") + 1) + " was created \n";

                return data;     
            }
            else
            {
                string root = Path.Combine(currentProject_Path, options[2]);
                newfolder = Path.Combine(currentProject_Path, options[1]);
                Directory.CreateDirectory(newfolder);
                data = "The folder " + newfolder.Substring(newfolder.LastIndexOf("\\") + 1) + " was created \n";

                return data;
            }
        }

        /* delete a file specified by user */
        private string delete_FileOrFolder(string[] commands)
        {
            string filePath = "";

            /* folder is in the root path */
            if (commands.Length == 2)
            {
                filePath = Path.Combine(currentProject_Path, commands[1]);
                System.IO.File.Delete(filePath);

                return "deleted";
            }
            else if (commands.Length == 3) // file/folder is in a specific path
            {
                filePath = Path.Combine(currentProject_Path, commands[2]);
                filePath = Path.Combine(filePath, commands[1]);

                return "deleted";
            }
            else
            {
                return "Wrong command";
            }
        }

        /* get the command from CMD compiler page and execute it */
        [HttpPost]
        public IActionResult execute_cmdCommand(string command)
        {
            string output ="";
            string folder ="";
            string[] commands;

            if (command == "list")
            {
                output = list_items("", "");
                return Json(output);
            }
                
            if (command.IndexOf("list -d") != -1)
            {
                folder = command.Substring(command.IndexOf("-d") + 2);
                output = list_items("-d", folder.Trim());

                return Json(list_items("-d",folder));
            }

            if (command.IndexOf("mkdir") != -1)
            {   
                if(data.AvailableSpace >= 100)
                {
                    errorData = new ErrorData();
                    errorData.MessageError = "Error: can't create new directory.Maximum space limit excedeed.";
                    return Json("Error");
                     
                }
                commands = command.Split(' ');
                output = make_dir(commands);

                return Json(output);
                 
            }

            if (command.Contains("delete"))
            {
                commands = command.Split(' ');
                output = delete_FileOrFolder(commands);

                return Json(output);
            }

            if (command == "help")
            {

                output = @"List of commands:
1. list { -d } - display the current project files and folders or a specified folder content with the '-d' option 
2. mkdir [ dirname ] { foldername } - make a new dir; if the foldername is not specified, the directory will be created on the root project folder 
3. open [file] {folder} - open a specific file from folder; if 'folder' is not specified, file will be searched on root folder" + '\n';

                return Json(output);
            }

            return Json("");
        }

        /* Save data into the specified file */
        [HttpPost]
        public IActionResult Save(string currentFile, string content)
        {
            currentFile = currentFile.Remove(currentFile.Length - 1);

            if (data.Inputs.ContainsKey(currentFile))
                data.Inputs.Remove(currentFile);

            data.Inputs.Add(currentFile, content);
            currentFile = Path.Combine(currentProject_Path, currentFile);

            System.IO.File.WriteAllText(currentFile, content);

            return Json("File Saved!");
        }


        [HttpPost]
        public IActionResult Save_As(string old_f, string new_f, string content)
        {

            FileStream fs_new;
            StreamWriter sw_new;

            /* if there are no project opened */
            if (currentProject_Path == "")
            {
                errorData = new ErrorData();
                errorData.MessageError = "No project open; can't procced with the action.";

                return Json("Error");
            }

            /* add a new key with the content */
            if (data.Inputs.ContainsKey(new_f))
                data.Inputs.Remove(new_f);

            data.Inputs.Add(new_f, content);

            /* if there's a new tab */
            if (old_f == null)
            {   


                new_f = Path.Combine(currentProject_Path, new_f);
                fs_new = new FileStream(new_f, FileMode.OpenOrCreate, FileAccess.Write);
                sw_new = new StreamWriter(fs_new);

                sw_new.Write(content);
                sw_new.Dispose();
                fs_new.Dispose();

                return Json("New Tab");
            }

            /* remove the old key */
            data.Inputs.Remove(old_f);

            string[] files = Directory.GetFiles(currentProject_Path);
            string file;

            /* search for the file available for change */
            foreach (string ffile in files)
            {
                file = ffile.Substring(ffile.LastIndexOf("\\") + 1);

                if (file == old_f)
                {
                    old_f = Path.Combine(currentProject_Path, old_f);
                    new_f = Path.Combine(currentProject_Path, new_f);

                    System.IO.File.Move(old_f, new_f);
                    System.IO.File.Delete(old_f);

                    System.IO.File.WriteAllText(new_f, content);
                }
            }
            
            return Json(content);
        }

        private string calculate_RunSize()
        {
            long size = 0;

                while (proccessTerminated == false)
                {
                    size = DirSize(new DirectoryInfo(currentProject_Path));
                    data.AvailableSpace = size;

                    if (size >= maxSpace)
                    {
                        errorData = new ErrorData();
                        errorData.MessageError = "Error: Space limit excedeed. Proccess was terminated.";
                      
                        throw new System.Exception();
                    }
                }

            data.AvailableSpace = System.Math.Round(System.Convert.ToDouble((size * 100.00) / maxSpace), 2);
            return "OK";
        }


        private string Generate_COutput(string input, string file)
        {   
            /* save the input into current file */
            string currentFile = Path.Combine(currentProject_Path, file);

            /*replace the old code*/
            System.IO.File.Delete(currentFile);
            System.IO.File.WriteAllText(currentFile, input);

            /*add in the dictionary the new code */
            data.Inputs.Remove(file);
            data.Inputs.Add(file, input);

            if (currentProject_Path == "")
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: No project open, please open a project or create a new one.";
                return "_Error";
            }
            else
            {
                /* find source file name */

                string filename = file;
                string PATH = currentProject_Path;
                string output = "";
                string errorOutput = "";

               
                /* Run CMD for compiling the source file */
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = PATH;
                startInfo.FileName = "CMD.exe";
                startInfo.Arguments = "/c gcc " + filename;
                process.StartInfo = startInfo;
                process.Start();


                /* redirect errors to outputError string variable */
                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    errorOutput = string.Concat(errorOutput, line);
                }
                process.WaitForExit();

                if (errorOutput.Length == 0 || errorOutput.Length != 0 && errorOutput.IndexOf("error") == -1)
                {

                    /* Run CMD for executing the source file */
                    process = new Process();
                    startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.WorkingDirectory = PATH;
                    startInfo.FileName = "CMD.exe";
                    startInfo.Arguments = "/c a.exe";
                    process.StartInfo = startInfo;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    proccessTerminated = true;
                    if (errorOutput != "")
                        output = errorOutput + "\n Result:  " + output;

                    return output;
                }
                else
                {
                    proccessTerminated = true;
                    return errorOutput;
                }

                }
        }

        private string Generate_CplusplusOutput(string input, string file)
        {
            /* save the input into current file */
            string currentFile = Path.Combine(currentProject_Path, file);

            /*replace the old code*/
            System.IO.File.Delete(currentFile);
            System.IO.File.WriteAllText(currentFile, input);

            /*add in the dictionary the new code */
            data.Inputs.Remove(file);
            data.Inputs.Add(file, input);

            if (currentProject_Path == "")
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: No project open, please open a project or create a new one.";
                return "_Error";
            }
            else
            {
                /* find source file name */

                string filename = file;
                string PATH = currentProject_Path;
                string output = "";
                string errorOutput = "";


                /* Run CMD for compiling the source file */
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = PATH;
                startInfo.FileName = "CMD.exe";
                startInfo.Arguments = "/c g++ " + filename;
                process.StartInfo = startInfo;
                process.Start();


                /* redirect errors to outputError string variable */
                while (!process.StandardError.EndOfStream)
                {
                    string line = process.StandardError.ReadLine();
                    errorOutput = string.Concat(errorOutput, line);
                }
                process.WaitForExit();

                if (errorOutput.Length == 0 || errorOutput.Length != 0 && errorOutput.IndexOf("error") == -1)
                {

                    /* Run CMD for executing the source file */
                    process = new Process();
                    startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.WorkingDirectory = PATH;
                    startInfo.FileName = "CMD.exe";
                    startInfo.Arguments = "/c a.exe";
                    process.StartInfo = startInfo;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    proccessTerminated = true;
                    if (errorOutput != "")
                        output = errorOutput + "\n Result:  " + output;

                    return output;
                }
                else
                {
                    proccessTerminated = true;
                    return errorOutput;
                }

            }

        }
        
        private string Generate_PythonOutput(string input, string file)
        {
            /* save the input into current file */
            string currentFile = Path.Combine(currentProject_Path, file);

            /*replace the old code*/
            System.IO.File.Delete(currentFile);
            System.IO.File.WriteAllText(currentFile, input);

            /*add in the dictionary the new code */
            data.Inputs.Remove(file);
            data.Inputs.Add(file, input);

            if (currentProject_Path == "")
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: No project open, please open a project or create a new one.";
                return "_Error";
            }
            else
            {
                /* find source file name */

                string filename = file;
                string PATH = currentProject_Path;
                string output = "";
                string erroutput = "";
 

                /* Run CMD for compiling the source file */
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WorkingDirectory = PATH;
                startInfo.FileName = "python.exe";
                startInfo.Arguments = filename;
                process.StartInfo = startInfo;
                process.Start();
                erroutput = process.StandardError.ReadToEnd();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                proccessTerminated = true;
                if (erroutput.Length != 0)
                    return erroutput;
                else
                    return output;
            }
        }     

        [HttpPost]
        public IActionResult ProcessInput(string input, string file)
        {

            /* transfer output data to client side */
            file = file.Remove(file.Length - 2);
            string Output = "";
            string type = file.Substring(file.LastIndexOf("."));
            proccessTerminated = false;

            /* no space available for execute */
            if (data.AvailableSpace >= maxSpace)
                return Json("Error");

            /* calculate the actual size of user folder while the source created is executed. */

            try
            {
                switch(type)
                {
                    case ".c":

                        Parallel.Invoke(
                            () => calculate_RunSize(),
                            () => Output = Generate_COutput(input, file)
                        );

                        break;


                    case ".cpp":

                        Parallel.Invoke(
                            () => calculate_RunSize(),
                            () => Output = Generate_CplusplusOutput(input, file)
                        );
                        
                        break;

                    case ".py":

                        Parallel.Invoke(
                            () => calculate_RunSize(),
                            () => Output = Generate_PythonOutput(input, file)
                        );

                        break;
                }
            }
            catch (System.Exception e)
            {
                return Json("Error");
            }
        
            return Json(Output);            
        }

        [HttpPost]
        public IActionResult export_ToGitHub(string username, string password, string gurl)
        {
            string folder = Repository.Init(currentProject_Path);

            try
            {
                using (var repo = new Repository(folder))
                {

                    /* origin remote does not exists */
                    if (repo.Network.Remotes["origin"] != null)
                        repo.Network.Remotes.Remove("origin");

                        repo.Network.Remotes.Add("origin", gurl);

                    Remote remote = repo.Network.Remotes["origin"];
                    repo.Network.Remotes.Update(remote, r => { r.Url = gurl; });

                    repo.Branches.Update(repo.Head,
                                         b => b.Remote = remote.Name,
                                         b => b.UpstreamBranch = repo.Head.CanonicalName);

                    Commands.Stage(repo, "*");

                    Signature author = new Signature(username, "@mail", DateTime.Now);
                    Signature committer = author;

                    Commit commit = repo.Commit("Automated commit from OIDEA.", author, committer);

                    LibGit2Sharp.PushOptions options = new LibGit2Sharp.PushOptions();
                    options.CredentialsProvider = new CredentialsHandler(
                        (url, usernameFromUrl, types) =>
                            new UsernamePasswordCredentials()
                            {
                                Username = username,
                                Password = password
                            });

                    repo.Network.Push(repo.Branches["master"], options);
                } 

            }catch(Exception e)
            {
                return Json(e.Message);
            }

                return Json("Success");
        }

        /* COMPILER VIEW */
        [Authorize]
        public IActionResult Compile(string path)
        {

            if (path != null)
            {
                currentProject_Path = path;
                data.currentProject = path.Substring(path.LastIndexOf("\\") + 1);
            }
            else
            {
                if (currentProject_Path == "")
                {
                    errorData = new ErrorData();
                    errorData.MessageError = "No project open";
                    return RedirectToAction("Error");
                }
            }


            /* calculate the used space */
            get_UserSpace();

            /*get the first tab for compiler page */
            data.FirstFile = get_FirstFile();

            /* set the key[file]:input[code] for every source file from project */
            set_Inputs();

            return View(data);

        }

        /* ERROR VIEW - displaying different client side errors */
        [Authorize]
        public IActionResult Error()
        {
            return View(errorData);
        }
    }
}
