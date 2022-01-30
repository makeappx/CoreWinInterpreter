using System;
using System.Linq;
using System.Windows.Forms;

namespace RedCode
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Clear();
        }
        void Clear()
        {
            startidx = 0;
            listBox1.Items.Clear();
            for (int i = 0; i < 8000; i++)
                listBox1.Items.Add(i.ToString().PadLeft(4, '0') + " DAT 0");
            label2.Text = "0";
        }
        string pointers = "@$<>";
        string[] instructions = new[] { "MOV", "CMP", "ADD", "SUB", "MUL", "DIV", "MOD", "JMP", "JMZ", "JMN" };
        void Validate(string s)
        {
            var tmp = s.Split(',');
            if (!pointers.Contains(tmp[0][0].ToString()) ||
                !pointers.Contains(tmp[1][0].ToString()) ||
                !int.TryParse(tmp[0].Substring(1), out _) ||
                !int.TryParse(tmp[1].Substring(1), out _))
                throw new Exception();
        }
        void Parse()
        {
            try
            {
                Clear();
                int start = 0;
                startidx = 0;
                for (int i = 0; i < richTextBox1.Lines.Length; i++)
                {
                    var arr = richTextBox1.Lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 0) continue;
                    switch (arr[0].ToUpper())
                    {
                        case "ORG":
                            startidx = int.Parse(arr[1]);
                            break;
                        case "DAT":
                            {
                                listBox1.Items[start] = start.ToString().PadLeft(4, '0') + " DAT";
                                if (int.TryParse(arr[1], out int val))
                                    listBox1.Items[start++] += " " + val;
                                break;
                            }
                        case "JMP":
                            {
                                listBox1.Items[start] = start++.ToString().PadLeft(4, '0') + " JMP " + arr[1];
                                if (!int.TryParse(arr[1].Substring(1), out int val) && !pointers.Contains(arr[1][0]))
                                    throw new Exception();
                                break;
                            }
                        default:
                            if(!instructions.Contains(arr[0].ToUpper()))
                                throw new Exception();
                            else
                            {
                                listBox1.Items[start] = start++.ToString().PadLeft(4, '0') +
                                    $" {instructions.First(x => x == arr[0].ToUpper())} " + arr[1];
                                Validate(arr[1]);
                                break;
                            }
                    }
                }
                label2.Text = startidx.ToString();
                listBox1.SelectedIndex = startidx;
            }
            catch { Clear(); }
        }
        int startidx = 0;

        int GetAddress(string cur, string addr)
        {
            int i = int.Parse(cur);
            string[] kek; int temp;
            switch(addr[0])
            {
                case '@':
                    kek = ((string)listBox1.Items[i + int.Parse(addr.Substring(1))]).Split(' ');
                    if (kek[2].Split(',').Length == 1)
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    else
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Split(',')[1].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    break;
                case '>':
                    kek = ((string)listBox1.Items[i + int.Parse(addr.Substring(1))]).Split(' ');
                    temp = int.Parse(kek[2].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    listBox1.Items[i + int.Parse(addr.Substring(1))] =
                       ((string)listBox1.Items[i + int.Parse(addr.Substring(1))])
                       .Remove(((string)listBox1.Items[i + int.Parse(addr.Substring(1))]).LastIndexOf(' ') + (pointers.Contains(kek[2][0]) ? 1 : 0))
                       + " " + (temp + 1).ToString();
                    if (kek[2].Split(',').Length == 1)
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    else
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Split(',')[1].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    break;
                case '<':
                    kek = ((string)listBox1.Items[i + int.Parse(addr.Substring(1))]).Split(' ');
                    temp = int.Parse(kek[2].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    listBox1.Items[i + int.Parse(addr.Substring(1))] =
                     ((string)listBox1.Items[i + int.Parse(addr.Substring(1))])
                     .Remove(((string)listBox1.Items[i + int.Parse(addr.Substring(1))]).LastIndexOf(' ') + (pointers.Contains(kek[2][0]) ? 1 : 0))
                     + " " + (temp - 1).ToString();
                    if (kek[2].Split(',').Length == 1)
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    else
                        i = int.Parse(kek[0]) + int.Parse(kek[2].Split(',')[1].Substring(pointers.Contains(kek[2][0]) ? 1 : 0));
                    break;
                default:
                    i += int.Parse(addr.Substring(1));
                    break;
            }
            return (i + 8000) % 8000;
        }

        void NextStep()
        {
            var s = ((string)listBox1.Items[startidx]).Split(' ');
            int x, y, temp, temp1, temp2, tempx;
            if (s[1] == "DAT")
                return;
            else
                switch(s[1])
                {
                    case "MOV":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        listBox1.Items[y] =
                            ((string)listBox1.Items[y]).Split(' ')[0] +
                            ((string)listBox1.Items[x]).Substring(4);
                        break;
                    case "ADD":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        listBox1.Items[y] = ((string)listBox1.Items[y]).Remove(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 1) + " ";
                        listBox1.Items[y] += (temp2 + temp1).ToString();
                        break;
                    case "MUL":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        listBox1.Items[y] = ((string)listBox1.Items[y]).Remove(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 1) + " ";
                        listBox1.Items[y] += (temp2 * temp1).ToString();
                        break;
                    case "SUB":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        listBox1.Items[y] = ((string)listBox1.Items[y]).Remove(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 1) + " ";
                        listBox1.Items[y] += (temp1 - temp2).ToString();
                        break;
                    case "DIV":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        listBox1.Items[y] = ((string)listBox1.Items[y]).Remove(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 1) + " ";
                        listBox1.Items[y] += (temp1 / temp2).ToString();
                        break;
                    case "MOD":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        listBox1.Items[y] = ((string)listBox1.Items[y]).Remove(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 1) + " ";
                        listBox1.Items[y] += (temp1 % temp2).ToString();
                        break;
                    case "JMP":
                        x = GetAddress(s[0], s[2]);
                        startidx = x - 1;
                        break;
                    case "CMP":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        tempx = ((string)listBox1.Items[x]).IndexOf(',');
                        temp2 = int.Parse(((string)listBox1.Items[x]).Substring(tempx == -1 ? ((string)listBox1.Items[x]).LastIndexOf(' ') : tempx + 2));
                        if (temp1 == temp2) startidx++;
                        break;
                    case "JMZ":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        if (temp1 == 0) startidx = x - 1;
                        break;
                    case "JMN":
                        x = GetAddress(s[0], s[2].Split(',')[0]);
                        y = GetAddress(s[0], s[2].Split(',')[1]);
                        temp = ((string)listBox1.Items[y]).IndexOf(',');
                        temp1 = int.Parse(((string)listBox1.Items[y]).Substring(temp == -1 ? ((string)listBox1.Items[y]).LastIndexOf(' ') : temp + 2));
                        if (temp1 != 0) startidx = x - 1;
                        break;
                }
            startidx++;
            label2.Text = startidx.ToString();
            listBox1.SelectedIndex = startidx;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Parse();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Автор: Бурлаченко Антон Сергеевич\n" +
                "4 Курс ИСТ\n" +
                "Поддерживаемые команды: MOV, CMP, ADD, SUB, MUL, DIV, MOD, JMP, JMZ, JMN\n" +
                "Поддерживаемые типы адресации: $, @, <, >");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = startidx;
        }
    }
}