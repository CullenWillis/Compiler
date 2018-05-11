using AllanMilne.Ardkit;

namespace Parser
{
    public class MySemantics : Semantics
    {
        public MySemantics(IParser parser) : base (parser)
        { }

        public void DeclareId(IToken id)
        {
            if (!id.Is(Token.IdentifierToken))
            {
                return;
            }

            Scope symbols = Scope.CurrentScope;

            if (symbols.IsDefined(id.TokenValue))
            {
                semanticError(new AlreadyDeclaredError(id, symbols.Get(id.TokenValue)));
            }
            else
            {
                symbols.Add(new VarSymbol(id, currentType));
            }
        }

        public int CheckId(IToken id)
        {
            if (!Scope.CurrentScope.IsDefined(id.TokenValue))
            {
                semanticError(new NotDeclaredError(id));
                return LanguageType.Undefined;
            }
            else
            {
                return CheckType(id);
            }  
        }

        public int CheckType(IToken token)
        {
            int thisType = LanguageType.Undefined;

            if (token.Is(Token.IdentifierToken))
            {
                thisType = Scope.CurrentScope.Get(token.TokenValue).Type;
                return thisType;
            }
            else if (token.Is(Token.IntegerToken))
            {
                thisType = LanguageType.Integer;
                return thisType;
            }
            else if (token.Is(Token.RealToken))
            {
                thisType = LanguageType.Real;
                return thisType;
            }

            if (currentType == LanguageType.Undefined)
            {
                currentType = thisType;
                return currentType;
            }
            if (currentType != thisType)
            {
                semanticError(new TypeConflictError(token, thisType, currentType));
            }

            return thisType;
        }

        /*
         * -- Found from C parser --
         * Checks for extra errors such as IF INT > REAL
         */

        public void CheckAssignment(IToken token, IToken variable, int right)
        {
            // if scope is not define, report error
            if (!Scope.CurrentScope.IsDefined(variable.TokenValue))
            {
                semanticError(new NotDeclaredError(variable));
            }

            // get currentType
            int left = CheckType(variable); 

            // If left is undefined, make left = right
            if (left == LanguageType.Undefined)
            {
                left = right;
            }

            // If right is undefined, make right = left
            if (right == LanguageType.Undefined)
            {
                right = left;
            }

            if(left != right)
            {
                semanticError(new TypeConflictError(token, left, right));
            }
            else
            {
                return;
            }

        }

        public int CheckExpression(IToken token, int left, int right)
        {
            // Error handling 
            if (left == LanguageType.Undefined && right == LanguageType.Undefined)
            {
                return LanguageType.Undefined;
            }

            // If left is undefined, make left = right
            if (left == LanguageType.Undefined)
            {
                left = right;
            }

            // If right is undefined, make right = left
            if (right == LanguageType.Undefined)
            {
                right = left;
            }

            // if left not equals right, output semanticError
            if (left != right)
            {
                semanticError(new TypeConflictError(token, left, right));
                return LanguageType.Undefined;
            }

            return left;
        }

        public void CheckBoolean(IToken token, int left, int right)
        {
            // Error handling 
            if(left == LanguageType.Undefined && right == LanguageType.Undefined) // check if both undefined
            {
                return;
            }

            // If left is undefined, make left = right
            if(left == LanguageType.Undefined)
            {
                left = right;
            }

            // If right is undefined, make right = left
            if (right == LanguageType.Undefined)
            {
                right = left;
            }

            // if left not equals right, output semanticError
            if(left != right)
            {
                semanticError(new TypeConflictError(token, left, right));
                return;
            }
        }
    }
}
