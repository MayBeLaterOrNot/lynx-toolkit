// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OperatorModel.cs" company="Lynx Toolkit">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace XmlDocT
{
    using System.Reflection;
    using System.Text;

    public class OperatorModel : MethodModel
    {
        public OperatorModel(TypeModel parent, MethodInfo mi)
            : base(parent, mi)
        {
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();

            var mb = this.MethodInfo;
            XmlUtilities.AppendAttributes(mb.GetCustomAttributes(false), sb);
            if (mb.IsPublic)
            {
                sb.Append("public ");
            }

            if (mb.IsPrivate)
            {
                sb.Append("private ");
            }

            if (mb.IsVirtual)
            {
                sb.Append("virtual ");
            }

            sb.Append("static ");
            bool explicitOperator = mb.Name == "op_Explicit";
            bool implicitOperator = mb.Name == "op_Implicit";

            if (!explicitOperator && !implicitOperator)
            {
                sb.Append(XmlUtilities.GetNiceTypeName(mb.ReturnType));
                sb.Append(" ");
                sb.Append("operator ");
                sb.Append(GetNiceOperatorName(mb.Name));
            }
            else
            {
                sb.Append(GetNiceOperatorName(mb.Name));
                sb.Append(" ");
                sb.Append(XmlUtilities.GetNiceTypeName(mb.ReturnType));
            }

            sb.Append("(");
            sb.Append(XmlUtilities.GetNiceMethodParameters(mb, true));
            sb.Append(")");
            return sb.ToString();
        }

        private static object GetNiceOperatorName(string name)
        {
            switch (name)
            {
                case "op_Implicit": return "implicit operator";
                case "op_Explicit": return "explicit operator";
                case "op_Addition": return "+";
                case "op_Subtraction": return "-";
                case "op_Multiply": return "*";
                case "op_Division": return "/";
                case "op_Modulus": return "%";
                case "op_ExclusiveOr": return "^";
                case "op_BitwiseAnd": return "&";
                case "op_BitwiseOr": return "^";
                case "op_LogicalAnd": return "&&";
                case "op_LogicalOr": return "||";
                case "op_Assign": return "=";
                case "op_LeftShift": return "<<";
                case "op_RightShift": return ">>";
                case "op_SignedRightShift": return ">>";
                case "op_UnsignedRightShift": return ">>";
                case "op_Equality": return "==";
                case "op_GreaterThan": return ">";
                case "op_LessThan": return "<";
                case "op_Inequality": return "!=";
                case "op_GreaterThanOrEqual": return ">=";
                case "op_LessThanOrEqual": return "<=";
                case "op_MultiplicationAssignment": return "*=";
                case "op_SubtractionAssignment": return "-=";
                case "op_ExclusiveOrAssignment": return "^=";
                case "op_LeftShiftAssignment": return "<<=";
                case "op_ModulusAssignment": return "%=";
                case "op_AdditionAssignment": return "+=";
                case "op_BitwiseAndAssignment": return "&=";
                case "op_BitwiseOrAssignment": return "|=";
                case "op_Comma": return ",";
                case "op_DivisionAssignment": return "/=";
                case "op_Decrement": return "--";
                case "op_Increment": return "++";
                case "op_UnaryNegation": return "!";
                case "op_UnaryPlus": return "+";
                case "op_OnesComplement": return "~";
                default:
                    return name.Substring(3);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "{0}({1})",
                XmlUtilities.GetNiceMethodName(this.MethodInfo),
                XmlUtilities.GetNiceMethodParameters(this.MethodInfo));
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Operator", XmlUtilities.GetNiceTypeName(this.DeclaringType), this);
        }
    }
}