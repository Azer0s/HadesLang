using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Interpreter;

namespace HadesMobile
{
    class AndroidOutput : IScriptOutput
    {
        private readonly TextView _text;
        public AndroidOutput(TextView text)
        {
            _text = text;
        }
        public void Write(string input)
        {
            throw new NotImplementedException();
        }

        public void WriteLine(string input)
        {
            _text.Text += input + "\n";
        }

        public void Clear()
        {
            _text.Text = ">";
        }

        public string ReadLine()
        {
            return "";
        }
    }
}