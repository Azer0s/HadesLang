using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Widget;
using Android.OS;
using Android.Text;
using Android.Util;
using Android.Views.InputMethods;
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

            _interpreter = new Inter();
            Cache.Instance.Variables = new Dictionary<Tuple<string, string>, Types>();
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

                string op;
                var returnVar = _interpreter.InterpretLine(input, "console", out op).Key;

                if (_interpreter.Clear)
                {
                    _interpreter.Clear = false;
                    _textView.Text = ">";
                }
                else
                {
                    _textView.Text += "\n" + returnVar + "\n";
                    _textView.Text += ">";
                }
            }
        }
    }
}