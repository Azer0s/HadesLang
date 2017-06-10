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
            if (input != null)
            {
                _text.Text += input;
            }
        }

        public void WriteLine(string input)
        {
            if (input != null)
            {
                Write(input + "\n");
            }
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