using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Diagnostics;

namespace VisualExplorer
{
    class ClsTreeListView
    {
        public void CreateTreeView(TreeView treeView) // Tạo cây cha My Computer
        {
            const int RemovableDisk = 2;  //Readalbe
            const int LocalDisk = 3;      //Read/Write
            const int NetworkDisk = 4;    //Write once
            const int CDDisk = 5;
            TreeNode tnMyComputer;
            tnMyComputer = new TreeNode("My Computer", 0, 0); // Tạo node "My computer"
            treeView.Nodes.Clear();                           // Reset treeView
            treeView.Nodes.Add(tnMyComputer);                 // Add node "My computer"  vào treeView
            TreeNodeCollection nodeCollection = tnMyComputer.Nodes;
            ManagementObjectSearcher query = new ManagementObjectSearcher("Select * From Win32_LogicalDisk"); // Tìm các ổ đĩa hiện tại
            ManagementObjectCollection queryCollection = query.Get();  // Lấy danh sách các ổ đĩa hiện tại
            foreach (ManagementObject mo in queryCollection)
            {
                TreeNode diskTreeNode;
                int imageIndex, selectIndex;  // Gán icon image, select cho các ổ đĩa
                switch (int.Parse(mo["DriveType"].ToString()))
                {
                    case RemovableDisk:
                        imageIndex = 1;
                        selectIndex = 1;
                        break;
                    case LocalDisk:
                        imageIndex = 2;
                        selectIndex = 2;
                        break;
                    case CDDisk:
                        imageIndex = 3;
                        selectIndex = 3;
                        break;
                    case NetworkDisk:
                        imageIndex = 4;
                        selectIndex = 4;
                        break;
                    default:
                        imageIndex = 5; // Mặc định là thư mục lúc đóng
                        selectIndex = 6; // Mặc định là thư mục lúc mở
                        break;
                }
                diskTreeNode = new TreeNode(mo["Name"].ToString() + "\\", imageIndex, selectIndex); // Tạo các Node ổ đĩa của My Computer
                nodeCollection.Add(diskTreeNode); // Add các Node con vào My Computer 
            }
        }
        public bool ShowFolderTree(TreeView treeView, ListView listView, TreeNode currentNode) // Tạo các Node con của thư mục khi nhấp vào
        {
            if (currentNode.Text != "My Computer") // Kiểm tra phải cây cha hay không 
            {
                try
                {
                    if (Directory.Exists(GetFullPath(currentNode.FullPath)) == false) // Kiểm tra đường dẫn thư mục tồn tại 
                    {
                        MessageBox.Show("Ổ đĩa hoặc thư mục không tồn tại");
                    }
                    else
                    {
                        string[] strDirectories = Directory.GetDirectories(GetFullPath(currentNode.FullPath)); // Lấy tập hợp các thư mục con 
                        List<string> strNames = new List<string>(); // Tập hợp tên của thư mục con 
                        foreach (string stringDir in strDirectories) // Tạo các Node thư mục con 
                        {
                            string strName = GetName(stringDir);
                            strNames.Add(strName);
                            //TreeNode nodeDir;
                            //nodeDir = new TreeNode(strName, 5, 6);
                            if(!currentNode.Nodes.ContainsKey(strName))
                                currentNode.Nodes.Add(strName, strName, 5, 6); // Thêm các Node của thư mục con vào Node cha, gán khóa strName
                        }
                        foreach(TreeNode treeNode in currentNode.Nodes) // Kiểm tra các Node con có bị trùng không, có thì xóa Node con
                        {
                            if (strNames.IndexOf(treeNode.Name.ToString()) == -1)
                                currentNode.Nodes.Remove(treeNode);
                        }
                        ShowContent(listView, currentNode); // Hiện thỉ thư mục

                    }
                    return true;
                }
                catch (IOException)
                {
                    MessageBox.Show("Ổ đĩa hoặc thư mục không tồn tại");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Bạn không có quyền truy cập thư mục hoặc file này");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            else
            {
                try
                {
                    //DriveInfo[] allDrives = DriveInfo.GetDrives();
                    //foreach (DriveInfo d in allDrives)
                    //{
                    //    MessageBox.Show("Drive " + d.Name);
                    //    MessageBox.Show("  Drive type: " + d.DriveType.ToString());
                    //    if (d.IsReady == true)
                    //    {
                    //        MessageBox.Show("  Volume label: " + d.VolumeLabel);
                    //        MessageBox.Show("  File system: " + d.DriveFormat);
                    //        MessageBox.Show(
                    //            "  Available space to current user:" + d.AvailableFreeSpace.ToString() + "bytes"
                    //            );
                    //        MessageBox.Show(
                    //            "  Available space to current user:" + d.TotalFreeSpace.ToString() + "bytes"
                    //            );
                    //        MessageBox.Show(
                    //            "  Available space to current user:" + d.TotalSize.ToString() + "bytes"
                    //            );
                    //    }
                    //}
                    ShowContent(listView, currentNode);
                    return true;
                }
                catch (IOException)
                {
                    MessageBox.Show("Ổ đĩa hoặc thư mục không tồn tại");
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Bạn không có quyền truy cập thư mục hoặc file này");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            return false;
        }
        public string GetFullPath(string strPath)  // Xử lý đường dẫn từ D:\\Study thành D:\Study
        {
            return strPath.Replace("My Computer\\", "").Replace("\\\\", "\\"); 
        }
        public string GetName(string strPath) // Lấy tên của thư mục hoặc file
        {
            string[] strSplit = strPath.Split('\\');
            int maxIndex = strSplit.Length;
            return strSplit[maxIndex - 1];
        }
        public void ShowContent(ListView listView, TreeNode currentNode) // Hiện nội dung thư mục từ Node cây 
        {
            try
            {
                if (currentNode.Text != "My Computer")
                {
                    listView.Items.Clear(); // Reset item trong list view
                    ListViewItem item;
                    DirectoryInfo directory = GetPathDir(currentNode); // Lấy thư mục từ Node hiện tại
                    foreach (DirectoryInfo folder in directory.GetDirectories()) // Hiện các thư mục con
                    {
                        item = GetLVItems(folder);
                        listView.Items.Add(item); // Thêm item vào listView
                    }
                    foreach (FileInfo file in directory.GetFiles()) // Hiện các file con
                    {
                        item = GetLVItems(file);
                        listView.Items.Add(item);
                    }
                }
                else
                {
                    listView.Items.Clear();
                    DriveInfo[] allDrives = DriveInfo.GetDrives();
                    ListViewItem item;
                    foreach (DriveInfo drive in allDrives)
                    {
                        item = GetLVItems(drive);
                        listView.Items.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        public bool ShowContent(ListView listView, string strPath) // Hiện nội dung thư mục từ đường dẫn
        {
            try
            {
                if (!strPath.EndsWith("\\")) // Kiểm tra đường dẫn có kết thúc = \\
                    strPath += "\\";
                ListViewItem item;
                DirectoryInfo directory = new DirectoryInfo(strPath); // Lấy thư mục từ đường dẫn hiện tại 
                if (!directory.Exists) // Kiểm tra thư mục có tồn tại không
                {
                    MessageBox.Show("Thư mục không tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                listView.Items.Clear(); // Reset listView
                foreach (DirectoryInfo folder in directory.GetDirectories()) // Hiện các thư mục con 
                {
                    item = GetLVItems(folder);
                    listView.Items.Add(item); // Thêm item vào listView
                }
                foreach (FileInfo file in directory.GetFiles()) // Hiện các file con
                {
                    item = GetLVItems(file);
                    listView.Items.Add(item); 
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
        public bool FocusItem(ListView listView, string strPath, string focusName) // Hiện nội dung thư mục và Select 1 item trong thư mục
        {
            try
            {
                if (!strPath.EndsWith("\\"))
                    strPath += "\\";
                ListViewItem item;
                DirectoryInfo directory = new DirectoryInfo(strPath);
                if (!directory.Exists)
                {
                    MessageBox.Show("Thư mục không tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                listView.Items.Clear();
                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    item = GetLVItems(folder);
                    listView.Items.Add(item);
                    if (item.SubItems[0].Text == focusName)
                    {
                        item.Focused = true;
                        item.Selected = true;
                    }
                }
                foreach (FileInfo file in directory.GetFiles())
                {
                    item = GetLVItems(file);
                    listView.Items.Add(item);
                    if (item.SubItems[0].Text == focusName)
                    {
                        item.Focused = true;
                        item.Selected = true;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
        public ListViewItem GetLVItems(DirectoryInfo folder) // Tạo các listView item từ thư mục 
        {
            string[] item = new string[5];
            item[0] = folder.Name; // Gán tên
            item[1] = "Folder";    // Folder hay file 
            item[2] = folder.CreationTime.ToString(); // Ngày tạo 
            item[3] = folder.LastWriteTime.ToString(); // Ngày sửa
            item[4] = folder.FullName; // Đường dẫn
            ListViewItem LVItem = new ListViewItem(item); // Thêm các item vào listView
            LVItem.ImageIndex = 16; // Thêm icon folder vào
            return LVItem;
        }
        public ListViewItem GetLVItems(FileInfo file) // Tạo các listView item từ file
        {
            long size = 0;
            string[] s = { file.Name, file.Extension.ToUpper(), size + "KB", file.LastWriteTime.ToString(), file.FullName, file.Directory.FullName.ToString() };
            string[] item = new string[5];
            item[0] = file.Name; // Gán tên
            item[1] = (file.Length / 1024).ToString() + "KB"; // Dung lượng
            item[2] = file.CreationTime.ToString(); // Ngày tạo 
            item[3] = file.LastWriteTime.ToString(); // Ngày sửa
            item[4] = file.FullName; // Đường dẫn
            ListViewItem LVitem = new ListViewItem(item); // Thêm các item vào listView
            LVitem.ImageIndex = GetImageIndex(file); // Thêm icon tương ứng vào file
            return LVitem;
        }
        public ListViewItem GetLVItems(DriveInfo drive) // Tạo các listView item từ ổ đĩa
        {
            //long size = 0;
            //string[] s = { file.Name, file.Extension.ToUpper(), size + "KB", file.LastWriteTime.ToString(), file.FullName, file.Directory.FullName.ToString() };
            string[] item = new string[5];
            //item[0] = file.Name; // Gán tên
            //item[1] = (file.Length / 1024).ToString() + "KB"; // Dung lượng
            //item[2] = file.CreationTime.ToString(); // Ngày tạo 
            //item[3] = file.LastWriteTime.ToString(); // Ngày sửa
            //item[4] = file.FullName; // Đường dẫn
            item[0] = drive.Name;
            item[1] = drive.TotalFreeSpace.ToString();
            item[2] = drive.TotalSize.ToString();
            item[3] = drive.VolumeLabel.ToString();
            item[4] = drive.GetType().ToString();
            ListViewItem LVitem = new ListViewItem(item); // Thêm các item vào listView
            LVitem.ImageIndex = 1; // Thêm icon tương ứng vào file
            return LVitem;
        }
        public DirectoryInfo GetPathDir(TreeNode currentNode)  // Xử lý path từ Node C:\Study\LTTQ thành C:\\Study\\Study\\LTTQ\\
        {
            string[] strList = currentNode.FullPath.Split('\\');  // Chia path từ \\
            string strPath = strList.GetValue(1).ToString();//+"\\"; // Skip phần từ đầu tiên 
            for (int i = 2; i < strList.Length; ++i)
                strPath += strList.GetValue(i) + "\\";  // Thêm  \\ bắt đầu từ phần tử thứ 2
            return new DirectoryInfo(strPath);
        }
        public int GetImageIndex(FileInfo file) // Icon tương ứng của file
        {
            switch (file.Extension.ToUpper())
            {
                case ".TXT":
                case ".DIZ":
                case ".LOG":
                    return 0;
                case ".PDF":
                    return 1;
                case ".HTM":
                case ".HTML":
                case ".URL":
                    return 2;
                case ".DOC":
                case ".DOCX":
                    return 3;
                case ".EXE":
                    return 4;
                case ".JPG":
                case ".PNG":
                case ".BMP":
                case ".GIF":
                    return 5;
                case ".MP3":
                case ".WAV":
                case ".WMV":
                case ".ASf":
                case ".MPEG":
                case ".AVI":
                    return 6;
                case ".RAR":
                case ".ZIP":
                    return 7;
                case ".PPT":
                case ".PPTX":
                    return 8;
                case ".MDB":
                    return 10;
                case ".XLS":
                case ".XLSX":
                    return 9;
                case "SWF":
                case ".FLV":
                case ".FLA":
                    return 11;
                case ".MP4":
                case ".FMV":
                case ".3GP":
                case ".MKV":
                    return 14;
                case ".ISO":
                    return 12;
                case ".TORRENT":
                    return 17;
                default:
                    return 15;
            }
        }
        public bool ClickItem(ListView listView, ListViewItem LVITem) // Khi double click item trên listView 
        {
            try
            {
                string path = LVITem.SubItems[4].Text; // Lấy đường dẫn của item
                FileInfo fi = new FileInfo(path);
                if (fi.Exists) // Nếu item là file 
                {
                    Process.Start(path); // Chạy file 
                }
                else // Item là folder 
                {
                    ListViewItem item;
                    DirectoryInfo directory = new DirectoryInfo(path + "\\"); // Xử lý đường dẫn
                    if (!directory.Exists) // Kiểm tra folder tồn tại 
                    {
                        MessageBox.Show("Không tồn tại thư mục", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    listView.Items.Clear(); // Reset listView
                    foreach (DirectoryInfo folder in directory.GetDirectories()) // Hiện các thư mục con
                    {
                        item = GetLVItems(folder);
                        listView.Items.Add(item);
                    }
                    foreach (FileInfo file in directory.GetFiles()) // Hiện các file con
                    {
                        item = GetLVItems(file);
                        listView.Items.Add(item);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return false;
        }
        public string GetPathDir(string path) // Xử lý path từ path C:\Study\LTTQ thành C:\\Study\\Study\\LTTQ\\
        {
            string[] strList = path.Split('\\'); // Chia path từ \\
            string strPath = "";
            for (int i = 0; i < strList.Length - 1; ++i) 
            {
                strPath += strList.GetValue(i) + "\\"; // Thêm vào \\ giữa các phần tử
            }
            return strPath;
        }
        public void DeleteItem(ListView listView, ListViewItem LVitem) // Xóa item trong listView
        {
            try
            {
                string path = LVitem.SubItems[4].Text; // Lấy full đường dẫn của item
                if(LVitem.SubItems[1].Text=="Folder") // Nếu item là folder
                {
                    DirectoryInfo directory = new DirectoryInfo(path + "\\"); 
                    if(!directory.Exists) // Kiểm tra thư mục có tồn tại
                    {
                        MessageBox.Show("Khong tồn tại thư mục", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        // Hiện dialog confirm delete
                        DialogResult dialog = MessageBox.Show("Bạn có chắc chắn muốn xóa thư mục " + LVitem.Text.ToString() + " ?", " Cảnh báo ", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (dialog == DialogResult.Yes)
                        {
                            directory.Delete(true);
                        }
                        else return;
                        string pathFolder = GetPathDir(path);
                        ShowContent(listView, pathFolder); // Hiện thị list sau khi xóa
                    }
                }
                else // Item là file 
                {
                    FileInfo file = new FileInfo(path);
                    if(!file.Exists) // Kiểm tra file có tồn tại
                    {
                        MessageBox.Show("File này không tồn tại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        // Hiện dialog confirm delete
                        DialogResult dialog = MessageBox.Show("Bạn có chắc chán muốn xóa file " + LVitem.Text.ToString() + " ?", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        if (dialog == DialogResult.Yes)
                        {
                            file.Delete();
                        }
                        else return;
                        string pathFolder = GetPathDir(path);
                        ShowContent(listView, pathFolder); // Hiện thị list sau khi xóa
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
