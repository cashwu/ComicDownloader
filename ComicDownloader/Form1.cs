using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace ComicDownloader
{
    public partial class Form1 : Form
    {
        private Dictionary<int, string> _comicDic = new Dictionary<int, string>()
        {
            { 1698, "食戟之靈" },
            { 1220, "美食的俘虜" },
            { 3945, "火之丸相扑" },
            { 1152, "海賊王" },
            { 3583, "一拳超人"}
        };

        /// <summary>
        /// {0} => comic name
        /// {1} => vol number
        /// {2} => page number
        /// </summary>
        private string _baseUri = @"http://web4.cartoonmad.com/c86es736r62/{0}/{1}/{2}.jpg";

        /// <summary>
        /// {0} => base path
        /// {1} => comic name
        /// {2} => vol number
        /// {3} => page number
        /// </summary>
        private string _baseFileName = @"{0}\{1}\{2}\{3}.jpg";

        private int _pageMaxNumber = 30;

        private ILog _log;

        public Form1(ILog log)
        {
            this._log = log;

            InitializeComponent();

            Init();
        }

        private void Init()
        {
            cbComic.DisplayMember = "Name";
            cbComic.ValueMember = "Id";

            foreach (var item in _comicDic)
            {
                cbComic.Items.Add(new Comic { Name = item.Value, Id = item.Key });
            }

            cbComic.SelectedIndex = 0;

            lblPath.Text = @"H:\comic";
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.ShowDialog();

            lblPath.Text = folderBrowser.SelectedPath;

            numericFrom.Value = 1;
            numericTo.Value = 1;
        }

        private void numericFrom_ValueChanged(object sender, EventArgs e)
        {
            numericTo.Minimum = numericFrom.Value;

            if (numericFrom.Value > numericTo.Value)
            {
                numericTo.Value = numericFrom.Value;
            }
        }

        private void numericTo_ValueChanged(object sender, EventArgs e)
        {
            numericFrom.Maximum = numericTo.Value;

            if (numericFrom.Value > numericTo.Value)
            {
                numericFrom.Value = numericTo.Value;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblPath.Text))
            {
                MessageBox.Show("Please select path !!");

                return;
            }

            var comicItem = cbComic.Items[cbComic.SelectedIndex] as Comic;

            this.Download(comicItem);
        }

        private void Download(Comic comic)
        {
            var downloadDic = this.GetDownloadUri(comic);

            var dlUri = string.Empty;
            try
            {
                using (var client = new WebClient())
                {
                    foreach (var dic in downloadDic)
                    {
                        dlUri = dic.Key;
                        var uri = new Uri(dic.Key);

                        if (CheckFile(dic.Key))
                        {
                            _log.Info($"DL Uri .. {dlUri} start");
                            client.DownloadFile(uri, dic.Value);

                            _log.Info($"DL Uri .. done");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error($"DL Uri..{dlUri}");
                _log.Error(e);
            }

            _log.Info("done..");
            MessageBox.Show("done..");
        }

        private Dictionary<string, string> GetDownloadUri(Comic comic)
        {
            var result = new Dictionary<string, string>();

            var minVol = numericFrom.Value;
            var maxVol = numericTo.Value;

            for (; minVol <= maxVol; minVol++)
            {
                for (int pageNumber = 1; pageNumber <= _pageMaxNumber; pageNumber++)
                {
                    var fixPageNumber = this.FixNumber(pageNumber);

                    var fixVolNumber = this.FixNumber(minVol);

                    var dlUri = string.Format(_baseUri, comic.Id, fixVolNumber, fixPageNumber);

                    var fileName = string.Format(_baseFileName, lblPath.Text, comic.Name, fixVolNumber, fixPageNumber);

                    this.CheckPath(fileName);

                    result.Add(dlUri, fileName);
                }
            }

            return result;
        }

        private void CheckPath(string fileName)
        {
            var path = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string FixNumber(int number)
        {
            return number.ToString().Trim().PadLeft(3, '0');
        }

        private string FixNumber(decimal number)
        {
            return number.ToString().Trim().PadLeft(3, '0');
        }

        private bool CheckFile(string url)
        {
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";

            var isFileExist = true;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                isFileExist = false;
                _log.Error($"CheckFile Error Url .. {url}", ex);
            }
            finally
            {
                if (response != null)
                {
                    isFileExist = response.StatusCode == HttpStatusCode.OK;
                    response.Close();
                }
            }

            return isFileExist;
        }

        private void cbComic_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericFrom.Value = 1;
            numericTo.Value = 1;
        }
    }

    class Comic
    {
        public string Name { get; set; }

        public int Id { get; set; }
    }
}
