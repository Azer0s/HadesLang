using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Views.InputMethods;
using CustomFunctions;
using Interpreter;
using Variables;
using Inter = Interpreter.Interpreter;

namespace HadesMobile
{
    [Activity(Label = "HadesMobile", Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private EditText _editText;
        private TextView _textView;
        private Inter _interpreter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _editText = FindViewById<EditText>(Resource.Id.editText1);
            _textView = FindViewById<TextView>(Resource.Id.textView1);

            _editText.InputType = InputTypes.ClassText;
            _editText.ImeOptions = ImeAction.Go;

            _editText.EditorAction += EditText_EditorAction;
            _editText.LongClick += async delegate
            {
                try
                {
                    var file = await new FileDialog(this,FileDialog.FileSelectionMode.FileOpen).GetFileOrDirectoryAsync(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
                    RunOnUiThread(() =>
                    {
                        _editText.Text += file;
                    });
                }
                catch (Exception)
                {
                }
            };
            _textView.MovementMethod = new ScrollingMovementMethod();

            _interpreter = new Inter(new AndroidOutput(_textView));
            Cache.Instance.Variables = new Dictionary<Tuple<string, string>, Types>();

            _interpreter.RegisterFunction(new Function("toast", () =>
            {
                Toast.MakeText(this, _interpreter.GetFunctionValues().FirstOrDefault(), ToastLength.Long).Show();
            }));
        }



        private void EditText_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Go)
            {
                var input = _editText.Text;
                _editText.Text = "";
                var imm = (InputMethodManager)GetSystemService(InputMethodService);
                imm.HideSoftInputFromWindow(_editText.WindowToken, 0);
                _textView.Text += input;
                Log.Info("X", input);

                if (input.Split(':')[0] == "scriptOutput")
                {
                    int toggle;
                    int.TryParse(input.Split(':')[1], out toggle);
                    switch (toggle)
                    {
                        case 0:
                            _interpreter.Evaluator.Output = new NoOutput();
                            _textView.Text += "\nScript-output disabled!";
                            break;
                        case 1:
                            _interpreter.Evaluator.Output = new AndroidOutput(_textView);
                            _textView.Text += "\nScript-output enabled!";
                            break;
                    }
                    _textView.Text += "\n>";
                    return;
                }

                string op;
                var returnVar = _interpreter.InterpretLine(input, "console", out op).Key;

                if (_interpreter.Clear)
                {
                    _interpreter.Clear = false;
                    _textView.Text = ">";
                }
                else
                {
                    _textView.Text += "\n" + returnVar + "\n>";
                }
            }
        }
    }
}