using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Environment = Android.OS.Environment;

namespace HadesMobile
{
    public class FileDialog
    {
        private AutoResetEvent _autoResetEvent;
        private const int FileOpen = 0;
        private const int FileSave = 1;
        private const int FolderChoose = 2;
        private readonly int _selectType;
        private readonly string _mSdcardDirectory;
        private readonly Context _mContext;
        private TextView _mTitleView1;
        private TextView _mTitleView;
        public string DefaultFileName = "default.txt";
        private string _selectedFileName = "default.txt";
        private EditText _inputText;

        private string _mDir = "";
        private List<string> _mSubdirs = null;
        private ArrayAdapter<string> _mListAdapter = null;
        private readonly bool _mGoToUpper = false;
        private AlertDialog _dirsDialog;

        //////////////////////////////////////////////////////
        // Callback interface for selected directory
        //////////////////////////////////////////////////////


        public FileDialog(Context context, FileSelectionMode mode)
        {
            switch (mode)
            {
                case FileSelectionMode.FileOpen:
                    _selectType = FileOpen;
                    break;
                case FileSelectionMode.FileSave:
                    _selectType = FileSave;
                    break;
                case FileSelectionMode.FolderChoose:
                    _selectType = FolderChoose;
                    break;
                case FileSelectionMode.FileOpenRoot:
                    _selectType = FileOpen;
                    _mGoToUpper = true;
                    break;
                case FileSelectionMode.FileSaveRoot:
                    _selectType = FileSave;
                    _mGoToUpper = true;
                    break;
                case FileSelectionMode.FolderChooseRoot:
                    _selectType = FolderChoose;
                    _mGoToUpper = true;
                    break;
                default:
                    _selectType = FileOpen;
                    break;
            }


            _mContext = context;
            _mSdcardDirectory = Environment.ExternalStorageDirectory.AbsolutePath;

            try
            {
                _mSdcardDirectory = new File(_mSdcardDirectory).CanonicalPath;
            }
            catch (IOException ioe)
            {
            }
        }

        public enum FileSelectionMode
        {
            FileOpen,
            FileSave,
            FolderChoose,
            FileOpenRoot,
            FileSaveRoot,
            FolderChooseRoot

        }


        public async Task<string> GetFileOrDirectoryAsync(String dir)
        {
            File dirFile = new File(dir);
            while (!dirFile.Exists() || !dirFile.IsDirectory)
            {
                dir = dirFile.Parent;
                dirFile = new File(dir);
                Log.Debug("~~~~~", "dir=" + dir);
            }
            Log.Debug("~~~~~", "dir=" + dir);
            //m_sdcardDirectory
            try
            {
                dir = new File(dir).CanonicalPath;
            }
            catch (IOException ioe)
            {
                return _result;
            }

            _mDir = dir;
            _mSubdirs = GetDirectories(dir);

            AlertDialog.Builder dialogBuilder = CreateDirectoryChooserDialog(dir, _mSubdirs, (sender, args) =>
            {
                String mDirOld = _mDir;
                String sel = "" + ((AlertDialog)sender).ListView.Adapter.GetItem(args.Which);
                if (sel[sel.Length - 1] == '/') sel = sel.Substring(0, sel.Length - 1);

                // Navigate into the sub-directory
                if (sel.Equals(".."))
                {
                    _mDir = _mDir.Substring(0, _mDir.LastIndexOf("/"));
                    if ("".Equals(_mDir))
                    {
                        _mDir = "/";
                    }
                }
                else
                {
                    _mDir += "/" + sel;
                }
                _selectedFileName = DefaultFileName;

                if ((new File(_mDir).IsFile)) // If the selection is a regular file
                {
                    _mDir = mDirOld;
                    _selectedFileName = sel;
                }

                UpdateDirectory();
            });
            dialogBuilder.SetPositiveButton("OK", (sender, args) =>
            {
                // Current directory chosen
                // Call registered listener supplied with the chosen directory

                {
                    if (_selectType == FileOpen || _selectType == FileSave)
                    {
                        _selectedFileName = _inputText.Text + "";
                        _result = _mDir + "/" + _selectedFileName;
                        _autoResetEvent.Set();
                    }
                    else
                    {
                        _result = _mDir;
                        _autoResetEvent.Set();
                    }
                }

            });
            dialogBuilder.SetNegativeButton("Cancel", (sender, args) => { });
            _dirsDialog = dialogBuilder.Create();

            _dirsDialog.CancelEvent += (sender, args) => { _autoResetEvent.Set(); };
            _dirsDialog.DismissEvent += (sender, args) => { _autoResetEvent.Set(); };
            // Show directory chooser dialog
            _autoResetEvent = new AutoResetEvent(false);
            _dirsDialog.Show();

            await Task.Run(() => { _autoResetEvent.WaitOne(); });

            return _result;
        }

        private string _result = null;

        private bool CreateSubDir(String newDir)
        {
            File newDirFile = new File(newDir);
            if (!newDirFile.Exists()) return newDirFile.Mkdir();
            else return false;
        }

        private List<String> GetDirectories(String dir)
        {
            List<String> dirs = new List<String>();
            try
            {
                File dirFile = new File(dir);

                // if directory is not the base sd card directory add ".." for going up one directory
                if ((_mGoToUpper || !_mDir.Equals(_mSdcardDirectory)) && !"/".Equals(_mDir))
                {
                    dirs.Add("..");
                }
                Log.Debug("~~~~", "m_dir=" + _mDir);
                if (!dirFile.Exists() || !dirFile.IsDirectory)
                {
                    return dirs;
                }

                foreach (File file in dirFile.ListFiles())
                {
                    if (file.IsDirectory)
                    {
                        // Add "/" to directory names to identify them in the list
                        dirs.Add(file.Name + "/");
                    }
                    else if (_selectType == FileSave || _selectType == FileOpen)
                    {
                        // Add file names to the list if we are doing a file save or file open operation
                        dirs.Add(file.Name);
                    }
                }
            }
            catch (Exception e) { }

            dirs.Sort();
            return dirs;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////                                   START DIALOG DEFINITION                                    //////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        private AlertDialog.Builder CreateDirectoryChooserDialog(String title, List<String> listItems, EventHandler<DialogClickEventArgs> onClickListener)
        {
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(_mContext);
            ////////////////////////////////////////////////
            // Create title text showing file select type // 
            ////////////////////////////////////////////////
            _mTitleView1 = new TextView(_mContext);
            _mTitleView1.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            //m_titleView1.setTextAppearance(m_context, android.R.style.TextAppearance_Large);
            //m_titleView1.setTextColor( m_context.getResources().getColor(android.R.color.black) );

            if (_selectType == FileOpen) _mTitleView1.Text = "Open";
            if (_selectType == FileSave) _mTitleView1.Text = "Save As";
            if (_selectType == FolderChoose) _mTitleView1.Text = "Select folder";

            //need to make this a variable Save as, Open, Select Directory
            _mTitleView1.Gravity = GravityFlags.CenterVertical;
            //_mTitleView1.SetBackgroundColor(Color.DarkGray); // dark gray 	-12303292
            _mTitleView1.SetTextColor(Color.Black);
            _mTitleView1.SetPadding(10, 10, 10, 5);
            _mTitleView1.SetTextSize(ComplexUnitType.Dip, 18);
            _mTitleView1.SetTypeface(null, TypefaceStyle.Bold);
            // Create custom view for AlertDialog title
            LinearLayout titleLayout1 = new LinearLayout(_mContext);
            titleLayout1.Orientation = Orientation.Vertical;
            titleLayout1.AddView(_mTitleView1);

            if (_selectType == FileSave)
            {
                ///////////////////////////////
                // Create New Folder Button  //
                ///////////////////////////////
                Button newDirButton = new Button(_mContext);
                newDirButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                newDirButton.Text = "New Folder";
                newDirButton.Click += (sender, args) =>
                {
                    EditText input = new EditText(_mContext);
                    new AlertDialog.Builder(_mContext).SetTitle("New Folder Name").SetView(input).SetPositiveButton("OK", (o, eventArgs) =>
                    {
                        String newDirName = input.Text;
                        // Create new directory
                        if (CreateSubDir(_mDir + "/" + newDirName))
                        {
                            // Navigate into the new directory
                            _mDir += "/" + newDirName;
                            UpdateDirectory();
                        }
                        else
                        {
                            Toast.MakeText(_mContext, "Failed to create '" + newDirName + "' folder", ToastLength.Short).Show();
                        }
                    }).SetNegativeButton("Cancel", (o, eventArgs) => { }).Show();
                };
                titleLayout1.AddView(newDirButton);
            }
            /////////////////////////////////////////////////////
            // Create View with folder path and entry text box // 
            /////////////////////////////////////////////////////
            LinearLayout titleLayout = new LinearLayout(_mContext);
            titleLayout.Orientation = Orientation.Vertical;



            var currentSelection = new TextView(_mContext);
            currentSelection.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            currentSelection.SetTextColor(Color.Black);
            currentSelection.Gravity = GravityFlags.CenterVertical;
            currentSelection.Text = "Current selection:";
            currentSelection.SetPadding(10, 10, 10, 5);
            currentSelection.SetTextSize(ComplexUnitType.Dip, 12);
            currentSelection.SetTypeface(null, TypefaceStyle.Bold);

            titleLayout.AddView(currentSelection);

            _mTitleView = new TextView(_mContext);
            _mTitleView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            _mTitleView.SetTextColor(Color.Black);
            _mTitleView.Gravity = GravityFlags.CenterVertical;
            _mTitleView.Text = title;
            _mTitleView.SetPadding(10,10,10, 5);
            _mTitleView.SetTextSize(ComplexUnitType.Dip, 10);
            _mTitleView.SetTypeface(null, TypefaceStyle.Normal);

            titleLayout.AddView(_mTitleView);

            if (_selectType == FileOpen || _selectType == FileSave)
            {
                _inputText = new EditText(_mContext);
                _inputText.Text = DefaultFileName;
                titleLayout.AddView(_inputText);
            }
            //////////////////////////////////////////
            // Set Views and Finish Dialog builder  //
            //////////////////////////////////////////
            dialogBuilder.SetView(titleLayout);
            dialogBuilder.SetCustomTitle(titleLayout1);
            _mListAdapter = CreateListAdapter(listItems);
            dialogBuilder.SetSingleChoiceItems(_mListAdapter, -1, onClickListener);
            dialogBuilder.SetCancelable(true);
            return dialogBuilder;
        }


        private void UpdateDirectory()
        {
            _mSubdirs.Clear();
            _mSubdirs.AddRange(GetDirectories(_mDir));
            _mTitleView.Text = _mDir;
            _dirsDialog.ListView.Adapter = null;
            _dirsDialog.ListView.Adapter = CreateListAdapter(_mSubdirs);
            //#scorch
            if (_selectType == FileSave || _selectType == FileOpen)
            {
                _inputText.Text = _selectedFileName;
            }
        }

        private ArrayAdapter<String> CreateListAdapter(List<String> items)
        {
            var adapter = new SimpleArrayAdaper(_mContext, Android.Resource.Layout.SelectDialogItem, Android.Resource.Id.Text1, items);
            return adapter;
        }

    }

    internal class SimpleArrayAdaper : ArrayAdapter<String>
    {
        public SimpleArrayAdaper(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public SimpleArrayAdaper(Context context, int textViewResourceId) : base(context, textViewResourceId)
        {
        }

        public SimpleArrayAdaper(Context context, int resource, int textViewResourceId) : base(context, resource, textViewResourceId)
        {
        }

        public SimpleArrayAdaper(Context context, int textViewResourceId, string[] objects) : base(context, textViewResourceId, objects)
        {
        }

        public SimpleArrayAdaper(Context context, int resource, int textViewResourceId, string[] objects) : base(context, resource, textViewResourceId, objects)
        {
        }

        public SimpleArrayAdaper(Context context, int textViewResourceId, IList<string> objects) : base(context, textViewResourceId, objects)
        {
        }

        public SimpleArrayAdaper(Context context, int resource, int textViewResourceId, IList<string> objects) : base(context, resource, textViewResourceId, objects)
        {
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View v = base.GetView(position, convertView, parent);
            if (v is TextView)
            {
                // Enable list item (directory) text wrapping
                TextView tv = (TextView)v;
                tv.LayoutParameters.Height = ViewGroup.LayoutParams.WrapContent;
                tv.Ellipsize = null;
            }
            return v;
        }
    }
}