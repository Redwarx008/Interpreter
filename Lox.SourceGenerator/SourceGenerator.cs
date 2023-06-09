﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lox.SourceGenerator
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        private StringBuilder _codeSB = new StringBuilder();

        private void DefineVisitor(string baseName, List<string> types, bool isGeneric)
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
            for (int i = 0; i < types.Count; i++)
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
        private void DefineType(string baseName, string className, string fieldList, bool isGeneric)
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
                List<string> splitField = field.Split(' ').ToList();
                splitField.RemoveAll(item =>
                {
                    return String.IsNullOrEmpty(item);
                });
                string type = splitField[0];
                string name = splitField[1];
                _codeSB.Append($@"
                this.{name} = {name};
        ");
            }

            _codeSB.Append($@"
            }}
        ");

            // Visitor pattern.
            _codeSB.Append($@"
            internal override void Accept(Visitor visitor)
            {{
                visitor.Visit{className}{baseName}(this);
            }}

            internal override R Accept<R>(Visitor<R> visitor)
            {{
                return visitor.Visit{className}{baseName}(this);
            }}");

            // Fields.
            foreach (string field in fields)
            {
                List<string> splitField = field.Split(' ').ToList();
                splitField.RemoveAll(item =>
                {
                    return String.IsNullOrEmpty(item);
                });

                string name = splitField[1];
                string type = splitField[0];
                _codeSB.Append($@"

            internal {type} {name};
        ");
            }

            _codeSB.Append($@"
        }}
        ");
        }
        private void DefineAst(string baseName, List<string> dsp, bool isGeneric)
        {
            _codeSB.Append($@"// <auto-generated/>
//#nullable enable
using System;
//using System.Collections.Generic;
using Interpreter;
namespace LoxGenerated
{{  
    public abstract class {baseName}
    {{
");
            DefineVisitor(baseName, dsp, true);
            DefineVisitor(baseName, dsp, false);
            for (int i = 0; i < dsp.Count; i++)
            {
                string className = dsp[i].Split(':')[0].Trim();
                string fields = dsp[i].Split(':')[1];
                DefineType(baseName, className, fields, isGeneric);
            }
            _codeSB.Append($@"

        internal abstract void Accept(Visitor visitor);

        internal abstract R Accept<R>(Visitor<R> visitor);
    }}
}}
");

        }
        public void Execute(GeneratorExecutionContext context)
        {
            _codeSB.Clear();
            List<string> _exprDsp = new List<string>()
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token loxOperator, Expr right",
                "Call     : Expr callee, Token paren, List<Expr> arguments",
                "Grouping : Expr expression",
                "Literal  : Object value",
                "Logical  : Expr left, Token loxOperator, Expr right",
                "Unary    : Token loxOperator, Expr right",
                "Variable : Token name"
            };
            DefineAst("Expr", _exprDsp, true);
            context.AddSource("Expr.g.cs", SourceText.From(_codeSB.ToString(), Encoding.UTF8));
            _codeSB.Clear();

            List<string> _stmtDsp = new List<string>()
            {
                "Block      : List<Stmt> statements",
                "Expression : Expr expression",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "Print      : Expr expression",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initializer",
                "While      : Expr condition, Stmt body"
            };
            DefineAst("Stmt", _stmtDsp, false);
            context.AddSource("Stmt.g.cs", SourceText.From(_codeSB.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
//#if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
//#endif 
//            Debug.WriteLine("Initalize code generator");
        }
    }
}
