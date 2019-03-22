using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsScriptGui
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 標準出力の内容をテキストボックスに書きだすライター
        /// </summary>
        TextBoxStreamWriter outWriter;

        /// <summary>
        /// タブによるコード補完リスト
        /// </summary>
        Dictionary<string, string> tabInteriDic = new Dictionary<string, string>();

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            tabInteriDic = new Dictionary<string, string> {
                { "cw", "Console.WriteLine" },
            };

            this.outWriter = new TextBoxStreamWriter(txtLog);
            Console.SetOut(outWriter);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtScript.Text =
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MainClass
{
    public static object Main()
    {
        return ""Hello, world!"";
    }
}
";
            txtDll.AppendText("System.dll" + Environment.NewLine);
            txtDll.AppendText("System.Core.dll" + Environment.NewLine);
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {

            txtLog.Clear();

            string src = txtScript.Text;

            try
            {
                using (var provider = new CSharpCodeProvider())
                {

                    var cprm = new CompilerParameters();
                    cprm.GenerateInMemory = true;

                    //参照の追加
                    var dlls = txtDll.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    cprm.ReferencedAssemblies.AddRange(dlls);

                    var res = provider.CompileAssemblyFromSource(cprm, src);
                    if (res.Errors.Count != 0)
                    {
                        sysout("Build Failure: ");
                        foreach (var er in res.Errors)
                        {
                            sysout(er.ToString());
                        }
                        txtLog.ScrollToEnd();
                        return;
                    }

                    var comp = res.CompiledAssembly;
                    var t = comp.GetType("MainClass");

                    var rtn = t.InvokeMember("Main", BindingFlags.InvokeMethod, null, null, null);

                    sysout("results: " + rtn.ToString());
                }

            }
            catch (Exception ex)
            {
                sysout(ex.ToString());
            }
        }

        private void sysout(string msg)
        {
            txtLog.Dispatcher.BeginInvoke(new Action(() => {
                txtLog.AppendText(msg);
                txtLog.AppendText(Environment.NewLine);
                txtLog.ScrollToEnd();
            }));
        }


        /// <summary>
        /// スクリプト記述欄のプレビューキーダウンイベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScript_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab) return;

            e.Handled = true;

            int beforeIdx = txtScript.CaretIndex;

            //補完リスト確認
            string leftText = txtScript.Text.Remove(beforeIdx);
            foreach (var item in tabInteriDic)
            {
                if (!leftText.EndsWith(item.Key)) continue;

                //置換対象の直前の文字が空文字でなければ置換対象としない
                string textBeforeKey = leftText.Remove(leftText.Length - item.Key.Length, item.Key.Length);
                if (textBeforeKey.Length == 0
                    || textBeforeKey.EndsWith(" ")
                    || textBeforeKey.EndsWith(Environment.NewLine))
                {

                    txtScript.Text = txtScript.Text.Remove(beforeIdx - item.Key.Length, item.Key.Length).Insert(beforeIdx - item.Key.Length, item.Value);
                    txtScript.CaretIndex = beforeIdx - item.Key.Length + item.Value.Length;
                    return;
                }
            }

            txtScript.Text = txtScript.Text.Insert(beforeIdx, "    ");
            txtScript.CaretIndex = beforeIdx + 4;
        }


    }
}
