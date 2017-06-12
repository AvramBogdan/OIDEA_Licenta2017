using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using OTP.ViewModels.User;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OTP.ViewModels;

namespace OTP.Controllers
{
    public class HomeController : Controller
    {
        private static List<ProjectList> projectList;
        private static ErrorData errorData;
        private static List<string> groupUsers;

        private string get_UserPath()
        {
            string path = "";
            string applicationPath = AppContext.BaseDirectory;
            int countslashes = 0;

            while (countslashes < 3)
            {
                applicationPath = applicationPath.Substring(0, applicationPath.LastIndexOf("\\")); 
                countslashes++ ; 
            }

            path = Path.Combine(applicationPath, "Users");
            path = Path.Combine(path, User.Identity.Name);
            return path;
        }

        private string get_ProjectType(string userpath, string project_name)
        {
            string configFile = "";
            string firstline = "";
            string projectPath = Path.Combine(userpath, project_name);
            configFile = Path.Combine(projectPath, "Config.txt");

            FileStream fs = System.IO.File.Open(configFile, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);

            /* we only need the first line; on the first line is the project type wrote */
            firstline = sr.ReadLine();

            /* get the project type */
            firstline = firstline.Substring(13, firstline.Length - 13);

            sr.Dispose();
            fs.Dispose();
            return firstline;
        }

        private void loadProjects()
        {
            projectList = new List<ProjectList>();
            
            string userPath = get_UserPath();
            string rootPath = userPath.Substring(0, userPath.LastIndexOf("\\"));
            string[] dirs = Directory.GetDirectories(userPath);
            
            ProjectList ps;
            foreach (string dir in dirs)
            {   
                ps = new ProjectList();
                ps.ProjectName = dir.Substring(dir.LastIndexOf("\\") + 1);
                ps.ProjectType = get_ProjectType(userPath, dir);

                projectList.Add(ps);

            }

            /* check the groups for projects */
            ps = new ProjectList();
            rootPath = Path.Combine(rootPath,"Groups");
            dirs = Directory.GetDirectories(rootPath);
            string p_path = "";
            FileStream fs;
            StreamReader sr;
            string line = "";
            foreach (string dir in dirs)
            {
                p_path = Path.Combine(rootPath, dir);
                p_path = Path.Combine(p_path, "Config.txt");

                fs = new FileStream(p_path, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(fs);

                while ((line = sr.ReadLine() ) != null)
                {
                    if (line.Contains("Users"))
                    {
                        if (line.Contains(User.Identity.Name))
                        {
                            sr.Dispose();
                            fs.Dispose();
                      
                            ps = new ProjectList();
                            ps.ProjectName = dir.Substring(dir.LastIndexOf("\\") + 1) + "(Group)";
                            ps.ProjectType = get_ProjectType(dir, dir);

                            projectList.Add(ps);
                            break;
                        }
                    }
                }
            }
        }

        private string get_Extension(string project_type)
        {
            string extension = ""; 

            switch (project_type)
            {
                case "Python":
                    extension = ".py";
                    break;

                case "C":
                    extension = ".c";
                    break;

                case "C++":
                    extension = ".cpp";
                    break;

                case "K":
                    extension = ".k";
                    break;

            }

            return extension;           
        }

        [Authorize]
        public IActionResult Home()
        {
            string userPath = get_UserPath();
           
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }

            loadProjects();
            return View();
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

        private long get_UserSpace()
        {
            long usedSpace = 0;
            string userPath = get_UserPath();

            string[] dirs = Directory.GetDirectories(userPath);
            foreach (string dir in dirs)
            {

                usedSpace += DirSize(new DirectoryInfo(dir));

            }

            return usedSpace;

        }

        [HttpPost]
        public IActionResult Add_NewProject(string project_name, string project_type)
        {

            string userPath = get_UserPath();
            string projectPath = Path.Combine(userPath, project_name);
            string configFile = "";
            string newfile = "";


            /*check if exists available space */
            long size = get_UserSpace();
            if (size >= 100000000)
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: maximum space excedeed, can't create new project";

                return RedirectToAction("Error");
            }
            
            /* create a folder for project */
            Directory.CreateDirectory(projectPath);

            /* create the config file */
            configFile = Path.Combine(projectPath, "Config.txt");
            newfile = Path.Combine(projectPath, "newfile" + get_Extension(project_type));

            /* add in the config file the project type */
            FileStream fs_config = new FileStream(configFile, FileMode.Create, FileAccess.Write);
            FileStream fs_new = new FileStream(newfile, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs_config);

            sw.WriteLine("ProjectType: " + project_type);

            sw.Dispose();
            fs_config.Dispose();
            fs_new.Dispose();
            loadProjects();

            return RedirectToAction("Compile", "Compiler", new { path = projectPath });
        }

        [HttpPost]
        public IActionResult open_Project(string[] CheckBox)
        {
            /* if there User tries to open multiple projects or none, a display message error will be displayed */
            if (CheckBox.Length == 0 )
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: No project selected for opening";
                return RedirectToAction("Error", "Home");
            }
            else if ( CheckBox.Length > 1)
            {
                errorData = new ErrorData();
                errorData.MessageError = "Error: Multiple projects selected for opening";
                return RedirectToAction("Error", "Home");
            }

            string userPath = get_UserPath();
            string projectPath = "";

            if (CheckBox[0].Contains("Group"))
            {
                userPath = userPath.Substring(0, userPath.LastIndexOf("\\"));
                userPath = Path.Combine(userPath, "Groups");

                string project_name = CheckBox[0].Substring(0, CheckBox[0].IndexOf("(Group)"));
                projectPath = Path.Combine(userPath, project_name);
            }
            else
            { 
                projectPath = Path.Combine(userPath, CheckBox[0]);
            }


            return RedirectToAction("Compile", "Compiler", new { path = projectPath });
        }

        [HttpPost]
        public IActionResult upload_SourceFile(IFormFile file, string project_name)
        {
            var filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            string userpath = get_UserPath();
            string projectpath = Path.Combine(userpath, project_name);
            long size = file.Length;
            
            projectpath = Path.Combine(projectpath, filename);
            using (FileStream fs = System.IO.File.Create(projectpath))
            {
                file.CopyTo(fs);
                fs.Flush();
                fs.Dispose();
            }

            return RedirectToAction("Projects");
        }

        [HttpPost]
        public IActionResult delete_project(string[] CheckBox)
        {
            string userpath = get_UserPath();
            string projectpath = "";

            /* foreach directory selected, the directories are deleted */
            for (int i=0; i<CheckBox.Length; i++)
            {   
                projectpath = Path.Combine(userpath, CheckBox[i]);
                
                /* delete only directory exists */
                if (Directory.Exists(projectpath))
                {
                    Directory.Delete(projectpath, true);
                }
            }

            /* reload actual projects */
            loadProjects();
            return RedirectToAction("Projects");
            
        }
        
        [HttpPost]
        public void add_userToGroup(string user)
        {

            groupUsers.Add(user);

        }
      
        [HttpPost]
        public IActionResult create_ProjectGroup(string project_name, string project_type)
        {
            string path = get_UserPath();
            path = path.Substring(0, path.LastIndexOf("\\"));

            /* create directory 'Group' if doesn't exists */
            path = Path.Combine(path, "Groups");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, project_name);

            /* create project folder if doesn't exists */
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);

                string configFile = "";
                string newfile = "";
                string users = "";

                /* create the config file */
                configFile = Path.Combine(path, "Config.txt");
                newfile = Path.Combine(path, "newfile" + get_Extension(project_type));

                /* add in the config file the project type */
                FileStream fs_config = new FileStream(configFile, FileMode.Create, FileAccess.Write);
                FileStream fs_new = new FileStream(newfile, FileMode.Create, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs_config);

                users = groupUsers[0];
                for (int i = 1; i < groupUsers.Count; i++)
                    users += "," + groupUsers[i];

                sw.WriteLine("ProjectType: " + project_type);
                sw.WriteLine("Users: " + users);

                sw.Dispose();
                fs_config.Dispose();
                fs_new.Dispose();

                loadProjects();

            }

            return RedirectToAction("Compile", "Compiler", new { path = path });
        }

        [Authorize]
        public IActionResult Projects()
        {
            groupUsers = new List<string>();
            groupUsers.Add(User.Identity.Name);

            return View(projectList);
        }

        [Authorize]
        public IActionResult Error()
        {
            return View(errorData);
        }

    }
}
