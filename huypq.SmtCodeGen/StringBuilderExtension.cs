using System.Text;

namespace huypq.SmtCodeGen
{
    public static class StringBuilderExtension
    {
        public static StringBuilder AppendLineEx(this StringBuilder sb)
        {
            sb.Append(Constant.LineEnding);
            return sb;
        }

        public static StringBuilder AppendLineEx(this StringBuilder sb, string text)
        {
            sb.Append(text);
            sb.Append(Constant.LineEnding);
            return sb;
        }

        public static StringBuilder AppendLineExWithFormat(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendFormat(format, args);
            sb.Append(Constant.LineEnding);
            return sb;
        }

        public static StringBuilder AppendLineExWithTab(this StringBuilder sb, string tab, string text)
        {
            sb.Append(tab);
            sb.Append(text);
            sb.Append(Constant.LineEnding);
            return sb;
        }

        public static StringBuilder AppendTabAndFormat(this StringBuilder sb, string tab, string format, params object[] args)
        {
            sb.Append(tab);
            sb.AppendFormat(format, args);
            return sb;
        }

        public static StringBuilder AppendTab(this StringBuilder sb, string tab, string text)
        {
            sb.Append(tab);
            sb.Append(text);
            return sb;
        }

        public static StringBuilder AppendLineExWithTabAndFormat(this StringBuilder sb, string tab, string format, params object[] args)
        {
            sb.Append(tab);
            sb.AppendFormat(format, args);
            sb.Append(Constant.LineEnding);
            return sb;
        }
    }
}
