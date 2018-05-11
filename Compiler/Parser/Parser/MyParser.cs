using AllanMilne.Ardkit;
using System.Collections.Generic;

namespace Parser
{
    class MyParser : RdParser
    {
        private MySemantics semantics;

        public MyParser() : base (new MyScanner())
        {
            semantics = new MySemantics(this);
        }

        protected override void recStarter()
        {
            Scope.OpenScope();

            recProgram();

            Scope.CloseScope();
        }

        /*
         * <Program> ::= PROGRAM Identifier
         * WITH <VarDecls> 
         * IN (<Statement>)+
         * END
        */
        private void recProgram()
        {
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);

            mustBe("WITH");
            recVariableDeclares();

            mustBe("IN");
            recStatementList();

            mustBe("END");
            mustBe(Token.EndOfFile);
        }

        // <VarDecls> ::= (<IdentList> AS <Type>)* 
        private void recVariableDeclares()
        {
            while (have(Token.IdentifierToken))
            {
                IToken token = scanner.CurrentToken;
                List<IToken> identifierList = recIdentifierList();

                mustBe("AS");

                recType(token);

                foreach (IToken id in identifierList)
                {
                    semantics.DeclareId(id);
                }
            }
        }

        // <IdentList> ::= Identifier ( , Identifier)* ;
        private List<IToken> recIdentifierList()
        {
            List<IToken> identList = new List<IToken>();
            identList.Add(scanner.NextToken());

            while (have(","))
            {
                mustBe(",");
                identList.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }

            return identList;
        }

        // <Type> ::= REAL | INTEGER ;
        private void recType(IToken token)
        {
            if (have("INTEGER"))
            {
                mustBe("INTEGER");
                semantics.CurrentType = LanguageType.Integer;
                semantics.DeclareId(token);
            }
            else if (have("REAL"))
            {
                mustBe("REAL");
                semantics.CurrentType = LanguageType.Real;
                semantics.DeclareId(token);
            }
            else
            {
                syntaxError("<Type>");
            }
        }

        // <Statement> Loop for more statements 
        private void recStatementList()
        {
            while (have(Token.IdentifierToken) || have("UNTIL") || have("IF") || have("INPUT") || have("OUTPUT"))
            {
                recStatement();
            }
        }

        // <Statement> ::= <Assignment> | <Loop> | <Conditional> | <I-o> ;
        private void recStatement()
        {
            if (have(Token.IdentifierToken))
            {
                recAssignment();
            }
            else if (have("UNTIL"))
            {
                recLoop();
            }
            else if (have("IF"))
            {
                recConditional();
            }
            else if (have("INPUT") || have("OUTPUT"))
            {
                recInputOutput();
            }
            else if (have("("))
            {
                recFactor();
            }
            else
            {
                syntaxError("<Statement>");
            }
        }

        // <Assignment> ::= Identifier = <Expression> ;
        private void recAssignment()
        {
            IToken token = scanner.CurrentToken;

            mustBe(Token.IdentifierToken);
            semantics.CheckId(token);

            IToken operator_ = scanner.CurrentToken;
            bool check = have("=");
            mustBe("=");

            int right = recExpression();

            if (check)
            {
                semantics.CheckAssignment(operator_, token, right);
            }
        }

        // UNTIL <Loop> ::= UNTIL <BooleanExpr> REPEAT (<Statement>)* ENDLOOP ;
        private void recLoop()
        {
            mustBe("UNTIL");
            recBooleanExpression();

            mustBe("REPEAT");
            recStatement();

            mustBe("ENDLOOP");
        }

        // IF <Conditional> ::= IF <BooleanExpr> THEN (<Statement>)* ( ELSE(<Statement>)* )? ENDIF ;
        private void recConditional()
        {
            mustBe("IF");
            recBooleanExpression();

            mustBe("THEN");
            recStatementList();
            
            if(have("ELSE"))
            {
                mustBe("ELSE");
                recStatementList();
            }

            mustBe("ENDIF");  
        }

        // <BooleanExpr> ::= <Expression> ("<" | "=" | ">") <Expression> ;
        private void recBooleanExpression()
        {
            int left = recExpression();
            IToken operator_ = scanner.CurrentToken;

            if (have("<"))
            {
                mustBe("<");
            }
            else if (have("="))
            {
                mustBe("=");
            }
            else if (have(">"))
            {
                mustBe(">");
            }
            else
            {
                syntaxError("<BooleanExpr>");
            }

            int right = recExpression();
            semantics.CheckBoolean(operator_, left, right);
        }

        //<I-o> ::= INPUT <IdentList> | OUTPUT<Expression>( , <Expression>)* ;
        private void recInputOutput()
        {       
            if (have("INPUT")) // INPUT <IdentList>
            {
                mustBe("INPUT");
                recIdentifierList();
            }
            else if (have("OUTPUT")) // OUTPUT <Expression>( , <Expression>)*
            {
                mustBe("OUTPUT");
                recExpression();

                while (have(","))
                {
                    mustBe(",");
                    recExpression();
                }
            }
            else
            {
                syntaxError("<I-o>");
            }
        }

        // <Expression> ::= <Term> ( (+|-) <Term>)* ;
        private int recExpression()
        {
            int type = recTerm();

            // ( (+|-) <Term>)*
            while (have("+") || have("-"))
            {
                IToken operator_ = scanner.CurrentToken;

                if (have("+"))
                {
                    mustBe("+");
                }
                else
                {
                    mustBe("-");
                }

                int right = recTerm();
                type = semantics.CheckExpression(operator_, type, right);
            }
            return type;
        }

        // <Term> ::= <Factor> ( (*|/) <Factor>)* ;
        private int recTerm()
        {
            int type = recFactor();

            while (have("*") || have("/"))
            {
                IToken operator_ = scanner.CurrentToken;

                if (have("*"))
                {
                    mustBe("*");
                }
                else
                {
                    mustBe("/");
                }

                int right = recFactor();
                type = semantics.CheckExpression(operator_, type, right);
            }

            return type;
        }

        // <Factor> ::= (+|-)? ( <Value> | "(" <Expression> ")" ) ;
        private int recFactor()
        {
            if (have("+"))
            {
                mustBe("+");
            }
            else if (have("-"))
            {
                mustBe("-");
            }

            if (have("("))
            {
                mustBe("(");

                int type = recExpression();

                mustBe(")");

                return type;
            }
            else
            {
                return recValue();
            }
        }

        // <Value> ::= Identifier | IntegerValue | RealValue ;
        private int recValue()
        {
            int type = LanguageType.Undefined;
            IToken token = scanner.CurrentToken;

            if (have(Token.IdentifierToken))
            {
                mustBe(Token.IdentifierToken);
                type = semantics.CheckId(token);
            }
            else if (have(Token.IntegerToken))
            {
                mustBe(Token.IntegerToken);
            }
            else if (have(Token.RealToken))
            {
                mustBe(Token.RealToken);
            }
            else
            {
                syntaxError("<Value>");
            }

            return type;
        }
    }
}
