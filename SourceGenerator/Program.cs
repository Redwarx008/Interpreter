﻿using System.Text;

namespace SourceGenerator
{
    internal class Program
    {
        private static StringBuilder _codeSB = new StringBuilder();

        private static void DefineVisitor(string baseName, List<string> types, bool isGeneric)
        {
            if(isGeneric)
            {
                _codeSB.Append($@"
        internal interface Visitor<R>
        {{
");
            }
            else
            {
                _codeSB.Append($@"
        internal interface Visitor
        {{
");
            }
            for(int i = 0; i < types.Count; i++)
            {
                string typeName = types[i].Split(':')[0].Trim();
                if(isGeneric)
                {
                    _codeSB.Append($@"
            public R Visit{typeName}{baseName}({typeName} {baseName.ToLower()});
");
                }
                else
                {
                    _codeSB.Append($@"
            public void Visit{typeName}{baseName}({typeName} {baseName.ToLower()});
");
                }
            }

            _codeSB.Append($@"
        }}
");
        }
        private static void DefineType(string baseName, string className, string fieldList, bool isGeneric)
        {
            _codeSB.Append($@"
        internal class {className} : {baseName}
        {{
            // Constructor.
            public {className} ({fieldList})
            {{
        ");
            // Store parameters in fields.
            string[] fields = fieldList.Split(',');
            foreach (string field in fields)
            {
                string name = field.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                _codeSB.Append($@"
                _{name} = {name};
        ");
            }
            _codeSB.Append($@"
            }}
        ");
            // Visitor pattern.
            if(isGeneric)
            {
                _codeSB.Append($@"
            internal override R Accept<R>(Visitor<R> visitor)
            {{
                return visitor.Visit{className}{baseName}(this);
            }}");
            }
            else
            {
                _codeSB.Append($@"
            internal override void Accept(Visitor visitor)
            {{
                visitor.Visit{className}{baseName}(this);
            }}");
            }
            // Fields.
            foreach (string field in fields)
            {
                string name = field.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
                string type = field.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
                _codeSB.Append($@"

            internal {type} _{name};
        ");
            }

            _codeSB.Append($@"
        }}
        ");
        }
        private static void DefineAst(string baseName, List<string> dsp, bool isGeneric)
        {
            _codeSB.Append($@"// <auto-generated/>
using System;
//using System.Collections.Generic;
using Interpreter;
namespace LoxGenerated
{{  
    public abstract class {baseName}
    {{
");
            DefineVisitor(baseName, dsp, isGeneric);
            for (int i = 0; i < dsp.Count; i++)
            {
                string className = dsp[i].Split(':')[0].Trim();
                string fields = dsp[i].Split(':')[1];
                DefineType(baseName, className, fields, isGeneric);
            }

            if(isGeneric)
            {
                _codeSB.Append($@"
        internal abstract R Accept<R>(Visitor<R> visitor);
    }}
}}
");
            }
            else
            {
                _codeSB.Append($@"
        internal abstract void Accept(Visitor visitor);
    }}
}}
");
            }
        }
        static void Main(string[] args)
        {
            List<string> _exprDsp = new List<string>()
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token loxOperator, Expr right",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Unary    : Token loxOperator, Expr right",
                "Variable : Token name"
            };
            DefineAst("Expr", _exprDsp, true);
            using (var writeStream = File.CreateText("Expr.g.cs"))
            {
                writeStream.Write(_codeSB.ToString());
            };
            _codeSB.Clear();
            List<string> _stmtDsp = new List<string>()
            {
                "Block      : List<Stmt> statements",
                "Expression : Expr expression",
                "Print      : Expr expression",
                "Var        : Token name, Expr initializer"
            };
            DefineAst("Stmt", _stmtDsp, false);
            using (var writeStream = File.CreateText("Stmt.g.cs"))
            {
                writeStream.Write(_codeSB.ToString());
            };
        }
    }
}