using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CsScriptGui
{
    class TextBoxStreamWriter : TextWriter
    {
        readonly TextBox target;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="target">書き込み先のテキストボックスを指定します。</param>
        public TextBoxStreamWriter(TextBox target)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            base.Write(value);

            target.Dispatcher.BeginInvoke(new Action(() => {
                target.AppendText(value.ToString());
                target.ScrollToEnd();
            }));
        }
    }
}
